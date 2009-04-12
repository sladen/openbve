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
					string a = Interface.GetCombinedFolderName(Folder, "Train");
					if (System.IO.Directory.Exists(a)) {
						return Subfolder;
					}
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
				Interface.AddMessage(Interface.MessageType.Critical, false, "The Railway and Train folders could not be found in any of the route file's parent directories. Please check your folder structure.");
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
			// default starting time
			Game.SecondsSinceMidnight = -(double)Game.PretrainsUsed * Game.PretrainInterval;
			Game.StartupTime = 0.0;
			// initialize trains
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			TrainManager.Trains = new TrainManager.Train[1 + Game.PretrainsUsed];
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				TrainManager.Trains[k] = new TrainManager.Train();
				TrainManager.Trains[k].TrainIndex = k;
			}
			TrainManager.PlayerTrain = TrainManager.Trains[Game.PretrainsUsed];
			// load trains
			double TrainProgressMaximum = 0.7 + 0.3 * (double)TrainManager.Trains.Length;
			for (int k = 0; k < TrainManager.Trains.Length; k++) {
				bool bogus = k != TrainManager.PlayerTrain.TrainIndex & Game.BogusPretrainInstructions.Length > 0;
				TrainManager.Trains[k].IsBogusTrain = bogus;
				if (bogus) {
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
				if (!bogus) {
					ObjectManager.UnifiedObject[] CarObjects;
					ExtensionsCfgParser.ParseExtensionsConfig(CurrentTrainFolder, CurrentTrainEncoding, out CarObjects, TrainManager.Trains[k]);
					System.Threading.Thread.Sleep(1); if (Cancel) return;
					for (int i = 0; i < TrainManager.Trains[k].Cars.Length; i++) {
						if (CarObjects[i] == null & Interface.CurrentOptions.ShowDefaultExteriorObjects) {
							// create default solid-color object
							ObjectManager.StaticObject o = new ObjectManager.StaticObject();
							o.Mesh.Vertices = new World.Vertex[8];
							double sx = 0.5 * TrainManager.Trains[k].Cars[i].Width;
							double sy = TrainManager.Trains[k].Cars[i].Height;
							double sz = 0.5 * TrainManager.Trains[k].Cars[i].Length;
							o.Mesh.Vertices[0].Coordinates = new World.Vector3D(sx, sy, -sz);
							o.Mesh.Vertices[1].Coordinates = new World.Vector3D(sx, 0.0, -sz);
							o.Mesh.Vertices[2].Coordinates = new World.Vector3D(-sx, 0.0, -sz);
							o.Mesh.Vertices[3].Coordinates = new World.Vector3D(-sx, sy, -sz);
							o.Mesh.Vertices[4].Coordinates = new World.Vector3D(sx, sy, sz);
							o.Mesh.Vertices[5].Coordinates = new World.Vector3D(sx, 0.0, sz);
							o.Mesh.Vertices[6].Coordinates = new World.Vector3D(-sx, 0.0, sz);
							o.Mesh.Vertices[7].Coordinates = new World.Vector3D(-sx, sy, sz);
							o.Mesh.Materials = new World.MeshMaterial[1];
							o.Mesh.Materials[0] = new World.MeshMaterial();
							o.Mesh.Materials[0].Color = new World.ColorRGBA(192, 192, 192, 255);
							o.Mesh.Materials[0].DaytimeTextureIndex = -1;
							o.Mesh.Materials[0].NighttimeTextureIndex = -1;
							o.Mesh.Faces = new World.MeshFace[6];
							o.Mesh.Faces[0] = new World.MeshFace(new int[] { 0, 1, 2, 3 });
							o.Mesh.Faces[1] = new World.MeshFace(new int[] { 0, 4, 5, 1 });
							o.Mesh.Faces[2] = new World.MeshFace(new int[] { 0, 3, 7, 4 });
							o.Mesh.Faces[3] = new World.MeshFace(new int[] { 6, 5, 4, 7 });
							o.Mesh.Faces[4] = new World.MeshFace(new int[] { 6, 7, 3, 2 });
							o.Mesh.Faces[5] = new World.MeshFace(new int[] { 6, 2, 1, 5 });
							CarObjects[i] = o;
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
						z -= TrainManager.Trains[k].Cars[i].Length;
						if (i < TrainManager.Trains[k].Cars.Length - 1) {
							z -= 0.5 * (TrainManager.Trains[k].Couplers[i].MinimumDistanceBetweenCars + TrainManager.Trains[k].Couplers[i].MaximumDistanceBetweenCars);
						}
					}
				}
				// configure ai / timetable
				if (k == TrainManager.PlayerTrain.TrainIndex) {
					TrainManager.Trains[k].PretrainAheadTimetable = 0.0;
				} else if (TrainManager.Trains[k].IsBogusTrain) {
					TrainManager.Trains[k].AI = new Game.BogusPretrainAI(TrainManager.Trains[k]);
				} else {
					TrainManager.Trains[k].AI = new Game.SimplisticHumanDriverAI(TrainManager.Trains[k]);
					TrainManager.Trains[k].PretrainAheadTimetable = (double)(Game.PretrainsUsed - k) * Game.PretrainInterval;
					TrainManager.Trains[k].Specs.DoorOpenMode = TrainManager.DoorMode.Manual;
					TrainManager.Trains[k].Specs.DoorCloseMode = TrainManager.DoorMode.Manual;
				}
			}
			TrainProgress = 1.0;
			// finished created objects
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			ObjectManager.FinishCreatingObjects();
			// starting track position
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			int FirstStationIndex = -1;
			double FirstStationPosition = 0.0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (Game.Stations[i].Stops.Length != 0) {
					FirstStationIndex = i;
					int s = Game.GetStopIndex(i, TrainManager.PlayerTrain.Cars.Length);
					if (s >= 0) {
						FirstStationPosition = Game.Stations[i].Stops[s].TrackPosition;
						if (Game.Stations[i].ArrivalTime < 0.0) {
							if (Game.Stations[i].DepartureTime < 0.0) {
								Game.SecondsSinceMidnight = 0.0;
							} else {
								Game.SecondsSinceMidnight = Game.Stations[i].DepartureTime - Game.Stations[i].StopTime;
							}
						} else {
							Game.SecondsSinceMidnight = Game.Stations[i].ArrivalTime;
						}
						Game.SecondsSinceMidnight -= (double)Game.PretrainsUsed * Game.PretrainInterval;
						Game.StartupTime = Game.SecondsSinceMidnight + (double)Game.PretrainsUsed * Game.PretrainInterval;
						break;
					}
				}
			}
			double TrackPosition = FirstStationPosition;
			// score
			System.Threading.Thread.Sleep(1); if (Cancel) return;
			Game.CurrentScore.ArrivalStation = FirstStationIndex + 1;
			Game.CurrentScore.DepartureStation = FirstStationIndex;
			Game.CurrentScore.Maximum = 0;
			for (int i = 0; i < Game.Stations.Length; i++) {
				if (i != FirstStationIndex & Game.Stations[i].StopAtStation) {
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
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					if (TrainManager.Trains[i].Cars[j].Sections.Length > 0) {
						TrainManager.ChangeCarSection(TrainManager.Trains[i], j, j == 0 | i != TrainManager.PlayerTrain.TrainIndex ? 0 : -1);
					}
				}
				if (FirstStationIndex >= 0) {
					if (Game.Stations[FirstStationIndex].OpenLeftDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedLeftDoorsOpened = true;
						}
					}
					if (Game.Stations[FirstStationIndex].OpenRightDoors) {
						for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
							TrainManager.Trains[i].Cars[j].Specs.AnticipatedRightDoorsOpened = true;
						}
					}
				}
				double p;
				if (TrainManager.Trains[i].IsBogusTrain & Game.BogusPretrainInstructions.Length != 0) {
					p = Game.BogusPretrainInstructions[0].TrackPosition;
				} else {
					p = i == TrainManager.PlayerTrain.TrainIndex ? TrackPosition : FirstStationPosition;
				}
				double d = p - TrainManager.Trains[i].Cars[0].FrontAxle.Follower.TrackPosition + 0.01;
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					TrainManager.MoveCar(TrainManager.Trains[i], j, -0.1, 0.01);
					TrainManager.MoveCar(TrainManager.Trains[i], j, 0.1, 0.01);
					if (TrainManager.Trains[i] != TrainManager.PlayerTrain) {
						TrainManager.MoveCar(TrainManager.Trains[i], j, d, 0.01);
					}
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
			// move player's train in position
			if (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition < 0.1) {
				double d = 0.1 - TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition;
				for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
					TrainManager.MoveCar(TrainManager.PlayerTrain, j, TrackPosition + d, 0.01);
				}
			} else {
				for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++) {
					TrainManager.MoveCar(TrainManager.PlayerTrain, j, TrackPosition, 0.01);
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
			ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
			//World.CameraSavedTrackPosition = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition;
			World.CameraSavedInterior = new World.CameraAlignment();
			World.CameraSavedExterior = new World.CameraAlignment(new World.Vector3D(-2.5, 1.5, -15.0), 0.3, -0.2, 0.0, 0.0, 1.0);
			World.CameraSavedTrack = new World.CameraAlignment(new World.Vector3D(-3.0, 2.5, -10.0), 0.3, 0.0, 0.0, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, 1.0);
		}

	}
}