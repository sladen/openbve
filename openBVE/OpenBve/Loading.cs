using System;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace OpenBve {
	internal static class Loading {

		// members
		internal static double RouteProgress;
		internal static double TrainProgress;
		internal static bool Cancel;
		internal static bool Complete;
		private static Thread Loader = null;
		private static string CurrentRouteFile;
		private static Encoding CurrentRouteEncoding;
		private static string CurrentTrainFolder;
		private static Encoding CurrentTrainEncoding;
		internal static double TrainProgressCurrentSum;
		internal static double TrainProgressCurrentWeight;

		// load
		internal static bool Load(string RouteFile, Encoding RouteEncoding, string TrainFolder, Encoding TrainEncoding) {
			// members
			RouteProgress = 0.0;
			TrainProgress = 0.0;
			TrainProgressCurrentSum = 0.0;
			TrainProgressCurrentWeight = 1.0;
			Cancel = false;
			Complete = false;
			CurrentRouteFile = RouteFile;
			CurrentRouteEncoding = RouteEncoding;
			CurrentTrainFolder = TrainFolder;
			CurrentTrainEncoding = TrainEncoding;
			// thread
			Loader = new Thread(new ThreadStart(LoadThreaded));
			Loader.IsBackground = true;
			Loader.Start();
			// dialog
			formLoading Dialog = new formLoading();
			System.Windows.Forms.DialogResult Result = Dialog.ShowDialog();
			Dialog.Dispose();
			// finalize
			return Result == DialogResult.OK;
		}

		// get railway folder
		private static string GetRailwayFolder(string RouteFile) {
			string Folder = System.IO.Path.GetDirectoryName(RouteFile);
			while (true) {
				string Subfolder = Interface.GetCombinedFolderName(Folder, "Railway");
				if (System.IO.Directory.Exists(Subfolder)) {
					/* Checking for the "Train" folder is unnecessary, as it is not used later */
					return Subfolder;
				}
				System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
				if (Info == null) break;
				Folder = Info.FullName;
			}
			Folder = System.IO.Path.GetDirectoryName(RouteFile);
			while (true) {
				string Subfolder = Interface.GetCombinedFolderName(Folder, "Railway");
				if (System.IO.Directory.Exists(Subfolder)) {
					return Subfolder;
				}
				System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
				if (Info == null) return null;
				Folder = Info.FullName;
			}
		}

		// load threaded
		private static void LoadThreaded() {
			#if DEBUG
			LoadEverythingThreaded();
			#else
			try {
				LoadEverythingThreaded();
			} catch (Exception ex) {
				Interface.AddMessage(Interface.MessageType.Critical, false, "The route and train loader encountered the following critical error: " + ex.Message);
			}
			#endif
			Complete = true;
		}
		private static void LoadEverythingThreaded() {
			string RailwayFolder = GetRailwayFolder(CurrentRouteFile);
			if (RailwayFolder == null) {
				Interface.AddMessage(Interface.MessageType.Critical, false, "The Railway folder could not be found in any of the route file's parent directories. Please check your folder structure.");
				return;
			}
			string ObjectFolder = Interface.GetCombinedFolderName(RailwayFolder, "Object");
			string SoundFolder = Interface.GetCombinedFolderName(RailwayFolder, "Sound");
			// reset
			Game.Reset(true);
			Game.MinimalisticSimulation = true;
			// screen
			World.CameraTrackFollower = new TrackManager.TrackFollower();
			World.CameraTrackFollower.Train = null;
			World.CameraTrackFollower.CarIndex = -1;
			World.CameraMode = World.CameraViewMode.Interior;
			// load route
			CsvRwRouteParser.ParseRoute(CurrentRouteFile, CurrentRouteEncoding, CurrentTrainFolder, ObjectFolder, SoundFolder, false);
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Game.CalculateSeaLevelConstants();
			if (Game.BogusPretrainInstructions.Length != 0) {
				double t = Game.BogusPretrainInstructions[0].Time;
				double p = Game.BogusPretrainInstructions[0].TrackPosition;
				for (int i = 1; i < Game.BogusPretrainInstructions.Length; i++) {
					if (Game.BogusPretrainInstructions[i].Time > t) {
						t = Game.BogusPretrainInstructions[i].Time;
					} else {
						t += 1.0;
						Game.BogusPretrainInstructions[i].Time = t;
					}
					if (Game.BogusPretrainInstructions[i].TrackPosition > p) {
						p = Game.BogusPretrainInstructions[i].TrackPosition;
					} else {
						p += 1.0;
						Game.BogusPretrainInstructions[i].TrackPosition = p;
					}
				}
			}
			RouteProgress = 1.0;
			// camera
			ObjectManager.InitializeVisibility();
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.0, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -0.1, true, false);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, 0.1, true, false);
			World.CameraTrackFollower.TriggerType = TrackManager.EventTriggerType.Camera;
			// initialize trains
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			TrainManager.Trains = new TrainManager.Train[Game.PrecedingTrainTimeDeltas.Length + 1 + (Game.BogusPretrainInstructions.Length != 0 ? 1 : 0)];
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				TrainManager.Trains[k] = new TrainManager.Train();
				TrainManager.Trains[k].TrainIndex = k;
				if (k == TrainManager.Trains.Length - 1 & Game.BogusPretrainInstructions.Length != 0) {
					TrainManager.Trains[k].State = TrainManager.TrainState.Bogus;
				} else {
					TrainManager.Trains[k].State = TrainManager.TrainState.Pending;
				}
			}
			TrainManager.PlayerTrain = TrainManager.Trains[Game.PrecedingTrainTimeDeltas.Length];
			// load trains
			double TrainProgressMaximum = 0.7 + 0.3 * (double)TrainManager.Trains.Length;
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				if (TrainManager.Trains[k].State == TrainManager.TrainState.Bogus) {
					// bogus train
					string Folder = Interface.GetDataFolder("Compatibility", "PreTrain");
					TrainDatParser.ParseTrainData(Folder, System.Text.Encoding.UTF8, TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					SoundCfgParser.LoadNoSound(TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.3 / TrainProgressMaximum;
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
				} else {
					// real train
					TrainProgressCurrentWeight = 0.1 / TrainProgressMaximum;
					TrainDatParser.ParseTrainData(CurrentTrainFolder, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					TrainProgressCurrentWeight = 0.2 / TrainProgressMaximum;
					SoundCfgParser.ParseSoundConfig(CurrentTrainFolder, CurrentTrainEncoding, TrainManager.Trains[k]);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					// door open/close speed
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						if (TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.SoundBufferIndex >= 0 & TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.SoundBufferIndex);
								double b = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenL.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorOpenR.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = a > 0.0 ? 1.0 / a : 0.8;
							} else {
								TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency = 0.8;
							}
						}
						if (TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency <= 0.0) {
							if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.SoundBufferIndex >= 0 & TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.SoundBufferIndex);
								double b = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a + b > 0.0 ? 2.0 / (a + b) : 0.2;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseL.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.2;
							} else if (TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.SoundBufferIndex >= 0) {
								double a = SoundManager.GetSoundLength(TrainManager.Trains[k].Cars[i].Sounds.DoorCloseR.SoundBufferIndex);
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = a > 0.0 ? 1.0 / a : 0.2;
							} else {
								TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency = 0.2;
							}
						}
						const double f = 0.1;
						TrainManager.Trains[k].Cars[i].Specs.DoorOpenFrequency *= Math.Exp(f * (2.0 * Game.Generator.NextDouble() - 1.0));
						TrainManager.Trains[k].Cars[i].Specs.DoorCloseFrequency *= Math.Exp(f * (2.0 * Game.Generator.NextDouble() - 1.0));
					}
				}
				for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
					TrainManager.Trains[k].Cars[i].FrontAxle.Follower.Train = TrainManager.Trains[k];
					TrainManager.Trains[k].Cars[i].RearAxle.Follower.Train = TrainManager.Trains[k];
					TrainManager.Trains[k].Cars[i].BeaconReceiver.Train = TrainManager.Trains[k];
				}
				// add panel section
				if (k == TrainManager.PlayerTrain.TrainIndex) {
					TrainManager.Trains[k].Cars[0].Sections = new TrainManager.Section[1];
					TrainManager.Trains[k].Cars[0].Sections[0].Elements = new ObjectManager.AnimatedObject[] { };
					TrainManager.Trains[k].Cars[0].Sections[0].Overlay = true;
					TrainProgressCurrentWeight = 0.7 / TrainProgressMaximum;
					TrainManager.ParsePanelConfig(CurrentTrainFolder, CurrentTrainEncoding, TrainManager.Trains[k], TrainManager.Trains[k].Cars[0].Sections[0].Overlay);
					TrainProgressCurrentSum += TrainProgressCurrentWeight;
					System.Threading.Thread.Sleep(1); if (Cancel) return;
				}
				// add exterior section
				if (TrainManager.Trains[k].State != TrainManager.TrainState.Bogus) {
					ObjectManager.UnifiedObject[] CarObjects;
					ExtensionsCfgParser.ParseExtensionsConfig(CurrentTrainFolder, CurrentTrainEncoding, out CarObjects, TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						if (CarObjects[i] == null) {
							// load default exterior object
							string file = Interface.GetCombinedFileName(Interface.GetDataFolder("Compatibility"), "exterior.csv");
							ObjectManager.StaticObject so = ObjectManager.LoadStaticObject(file, System.Text.Encoding.UTF8, ObjectManager.ObjectLoadMode.Normal, false, false, false);
							double sx = TrainManager.Trains[k].Cars[i].Width;
							double sy = TrainManager.Trains[k].Cars[i].Height;
							double sz = TrainManager.Trains[k].Cars[i].Length;
							CsvB3dObjectParser.ApplyScale(so, sx, sy, sz);
							CarObjects[i] = so;
						}
						if (CarObjects[i] != null) {
							// add object
							int j = TrainManager.Trains[k].Cars[i].Sections.Length;
							Array.Resize<TrainManager.Section>(ref TrainManager.Trains[k].Cars[i].Sections, j + 1);
							if (CarObjects[i] is ObjectManager.StaticObject) {
								ObjectManager.StaticObject s = (ObjectManager.StaticObject)CarObjects[i];
								TrainManager.Trains[k].Cars[i].Sections[j].Elements = new ObjectManager.AnimatedObject[1];
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0] = new ObjectManager.AnimatedObject();
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0].States = new ObjectManager.AnimatedObjectState[1];
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0].States[0].Position = new World.Vector3D(0.0, 0.0, 0.0);
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0].States[0].Object = s;
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0].CurrentState = 0;
								TrainManager.Trains[k].Cars[i].Sections[j].Elements[0].ObjectIndex = ObjectManager.CreateDynamicObject();
							} else if (CarObjects[i] is ObjectManager.AnimatedObjectCollection) {
								ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)CarObjects[i];
								TrainManager.Trains[k].Cars[i].Sections[j].Elements = new ObjectManager.AnimatedObject[a.Objects.Length];
								for (int h = 0; h < a.Objects.Length; h++) {
									TrainManager.Trains[k].Cars[i].Sections[j].Elements[h] = a.Objects[h];
									TrainManager.Trains[k].Cars[i].Sections[j].Elements[h].ObjectIndex = ObjectManager.CreateDynamicObject();
								}
							}
						}
					}
				}
				// place cars
				{
					double z = 0.0;
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						TrainManager.Trains[k].Cars[i].FrontAxle.Follower.TrackPosition = z - 0.5 * TrainManager.Trains[k].Cars[i].Length + TrainManager.Trains[k].Cars[i].FrontAxlePosition;
						TrainManager.Trains[k].Cars[i].RearAxle.Follower.TrackPosition = z - 0.5 * TrainManager.Trains[k].Cars[i].Length + TrainManager.Trains[k].Cars[i].RearAxlePosition;
						TrainManager.Trains[k].Cars[i].BeaconReceiver.TrackPosition = z - 0.5 * TrainManager.Trains[k].Cars[i].Length + TrainManager.Trains[k].Cars[i].BeaconReceiverPosition;
						z -= TrainManager.Trains[k].Cars[i].Length;
						if (i < TrainManager.Trains[k].Cars.Length - 1) {
							z -= 0.5 * (TrainManager.Trains[k].Couplers[i].MinimumDistanceBetweenCars + TrainManager.Trains[k].Couplers[i].MaximumDistanceBetweenCars);
						}
					}
				}
				// configure ai / timetable
				if (TrainManager.Trains[k] == TrainManager.PlayerTrain) {
					TrainManager.Trains[k].TimetableDelta = 0.0;
				} else if (TrainManager.Trains[k].State != TrainManager.TrainState.Bogus) {
					TrainManager.Trains[k].AI = new Game.SimplisticHumanDriverAI(TrainManager.Trains[k]);
					TrainManager.Trains[k].TimetableDelta = Game.PrecedingTrainTimeDeltas[k];
					TrainManager.Trains[k].Specs.DoorOpenMode = TrainManager.DoorMode.Manual;
					TrainManager.Trains[k].Specs.DoorCloseMode = TrainManager.DoorMode.Manual;
				}
			}
			TrainProgress = 1.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			ObjectManager.FinishCreatingObjects();
			// starting time and track position
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Game.SecondsSinceMidnight = 0.0;
			Game.StartupTime = 0.0;
			int PlayerFirstStationIndex = -1;
			double PlayerFirstStationPosition = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerStop & Game.Stations[i].Stops.Length != 0) {
					PlayerFirstStationIndex = i;
					int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0) {
						PlayerFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
					} else {
						PlayerFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
					}
					if (Game.Stations[i].ArrivalTime < 0.0) {
						if (Game.Stations[i].DepartureTime < 0.0) {
							Game.SecondsSinceMidnight = 0.0;
							Game.StartupTime = 0.0;
						} else {
							Game.SecondsSinceMidnight = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
							Game.StartupTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
						}
					} else {
						Game.SecondsSinceMidnight = Game.Stations[i].ArrivalTime;
						Game.StartupTime = Game.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			int OtherFirstStationIndex = -1;
			double OtherFirstStationPosition = 0.0;
			double OtherFirstStationTime = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].StopMode == Game.StationStopMode.AllStop | Game.Stations[i].StopMode == Game.StationStopMode.PlayerPass & Game.Stations[i].Stops.Length != 0) {
					OtherFirstStationIndex = i;
					int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0) {
						OtherFirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
					} else {
						OtherFirstStationPosition = Game.Stations[i].DefaultTrackPosition;
					}
					if (Game.Stations[i].ArrivalTime < 0.0) {
						if (Game.Stations[i].DepartureTime < 0.0) {
							OtherFirstStationTime = 0.0;
						} else {
							OtherFirstStationTime = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
						}
					} else {
						OtherFirstStationTime = Game.Stations[i].ArrivalTime;
					}
					break;
				}
			}
			if (Game.PrecedingTrainTimeDeltas.Length != 0) {
				OtherFirstStationTime -= Game.PrecedingTrainTimeDeltas[Game.PrecedingTrainTimeDeltas.Length - 1];
				if (OtherFirstStationTime < Game.SecondsSinceMidnight) {
					Game.SecondsSinceMidnight = OtherFirstStationTime;
				}
			}
			// score
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Game.CurrentScore.ArrivalStation = PlayerFirstStationIndex + 1;
			Game.CurrentScore.DepartureStation = PlayerFirstStationIndex;
			Game.CurrentScore.Maximum = 0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (i != PlayerFirstStationIndex & Game.PlayerStopsAtStation(i)) {
					Game.CurrentScore.Maximum += Game.ScoreValueStationArrival;
				}
			}
			if (Game.CurrentScore.Maximum <= 0) {
				Game.CurrentScore.Maximum = Game.ScoreValueStationArrival;
			}
			// initialize trains
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				TrainManager.InitializeTrain(TrainManager.Trains[i]);
				int s = i == TrainManager.PlayerTrain.TrainIndex ? PlayerFirstStationIndex : OtherFirstStationIndex;
				if (s >= 0) {
					if (Game.Stations[s].OpenLeftDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedLeftDoorsOpened = true;
						}
					}
					if (Game.Stations[s].OpenRightDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedRightDoorsOpened = true;
						}
					}
				}
				if (Game.Sections.Length != 0) {
					Game.Sections[0].Enter(TrainManager.Trains[i]);
				}
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					double length = TrainManager.Trains[i].Cars[0].Length;
					TrainManager.MoveCar(TrainManager.Trains[i], j, -length, 0.01);
					TrainManager.MoveCar(TrainManager.Trains[i], j, length, 0.01);
				}
			}
			// signals
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// load plugin
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			if (TrainManager.PlayerTrain != null) {
				PluginManager.LoadAtsConfig(CurrentTrainFolder, CurrentTrainEncoding, TrainManager.PlayerTrain);
			}
			// move train in position
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				double p;
				if (i == TrainManager.PlayerTrain.TrainIndex) {
					p = PlayerFirstStationPosition;
				} else if (TrainManager.Trains[i].State == TrainManager.TrainState.Bogus) {
					p = Game.BogusPretrainInstructions[0].TrackPosition;
					TrainManager.Trains[i].AI = new Game.BogusPretrainAI(TrainManager.Trains[i]);
				} else {
					p = OtherFirstStationPosition;
				}
				if (TrainManager.Trains[i].Cars[0].FrontAxle.Follower.TrackPosition < 0.1) {
					double d = 0.1 - TrainManager.Trains[i].Cars[0].FrontAxle.Follower.TrackPosition;
					for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
						TrainManager.MoveCar(TrainManager.Trains[i], j, p + d, 0.01);
					}
				} else {
					for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
						TrainManager.MoveCar(TrainManager.Trains[i], j, p, 0.01);
					}
				}
			}
			// time table
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			if (Timetable.TimetableDescription.Length == 0) {
				Timetable.TimetableDescription = Game.LogTrainName;
			}
			// initialize camera
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			TrainManager.UpdateCamera(TrainManager.PlayerTrain);
			TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, -1.0, true, false);
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
			World.CameraSavedInterior = new World.CameraAlignment();
			World.CameraSavedExterior = new World.CameraAlignment(new World.Vector3D(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, PlayerFirstStationPosition, 1.0);
			World.CameraSavedTrack = new World.CameraAlignment(new World.Vector3D(-3.0, 2.5, 0.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - 10.0, 1.0);
		}

	}
}