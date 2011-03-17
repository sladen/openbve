using System;

namespace OpenBve {
	internal static class SoundCfgParser {

		// parse sound config
		internal static void ParseSoundConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			string FileName = Interface.GetCombinedFileName(TrainPath, "sound.cfg");
			if (System.IO.File.Exists(FileName)) {
				LoadBve4Sound(FileName, TrainPath, Encoding, Train);
			} else {
				LoadBve2Sound(TrainPath, Train);
			}
		}
		
		internal static void LoadDefaultPluginSounds(TrainManager.Train train, string trainFolder) {
			World.Vector3D position = new World.Vector3D(train.Cars[0].DriverX, train.Cars[0].DriverY, train.Cars[0].DriverZ + 1.0);
			const double radius = 2.0;
			train.Cars[0].Sounds.Plugin = new TrainManager.CarSound[] {
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "ats.wav"), position, radius),
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "atscnt.wav"), position, radius),
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "ding.wav"), position, radius),
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "toats.wav"), position, radius),
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "toatc.wav"), position, radius),
				TryLoadSound(Interface.GetCombinedFileName(trainFolder, "eb.wav"), position, radius)
			};
		}

		// load no sound
		internal static void LoadNoSound(TrainManager.Train Train) {
			// initialize
			for (int i = 0; i < Train.Cars.Length; i++) {
				Train.Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
				Train.Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
				Train.Cars[i].Sounds.Adjust = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Air = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.AirHigh = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.AirZero = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Brake = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BrakeHandleApply = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BrakeHandleMin = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BrakeHandleMax = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BrakeHandleRelease = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.CpEnd = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.CpLoop = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.CpStart = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.DoorCloseL = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.DoorCloseR = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.DoorOpenL = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.DoorOpenR = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.EmrBrake = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Flange = new TrainManager.CarSound[] { };
				Train.Cars[i].Sounds.FlangeVolume = new double[] { };
				Train.Cars[i].Sounds.Halt = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Horns = new TrainManager.Horn[] { TrainManager.Horn.Empty, TrainManager.Horn.Empty, TrainManager.Horn.Empty };
				Train.Cars[i].Sounds.Loop = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.MasterControllerUp = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.MasterControllerDown = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.MasterControllerMin = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.MasterControllerMax = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.PilotLampOn = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.PointFrontAxle = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.PointRearAxle = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.ReverserOn = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.ReverserOff = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Rub = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Run = new TrainManager.CarSound[] { };
				Train.Cars[i].Sounds.RunVolume = new double[] { };
				Train.Cars[i].Sounds.SpringL = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.SpringR = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.Plugin = new TrainManager.CarSound[] { };
			}
		}

		// load bve 2 sound
		private static void LoadBve2Sound(string TrainPath, TrainManager.Train Train) {
			// set sound positions and radii
			World.Vector3D front = new World.Vector3D(0.0, 0.0, 0.5 * Train.Cars[0].Length);
			World.Vector3D center = new World.Vector3D(0.0, 0.0, 0.0);
			World.Vector3D left = new World.Vector3D(-1.3, 0.0, 0.0);
			World.Vector3D right = new World.Vector3D(1.3, 0.0, 0.0);
			World.Vector3D cab = new World.Vector3D(-Train.Cars[0].DriverX, Train.Cars[0].DriverY, Train.Cars[0].DriverZ - 0.5);
			World.Vector3D panel = new World.Vector3D(Train.Cars[0].DriverX, Train.Cars[0].DriverY, Train.Cars[0].DriverZ + 1.0);
			double large = 30.0;
			double medium = 10.0;
			double small = 5.0;
			double tiny = 2.0;
			LoadNoSound(Train);
			// load sounds for driver's car
			Train.Cars[Train.DriverCar].Sounds.Adjust = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Adjust.wav"), panel, tiny);
			Train.Cars[Train.DriverCar].Sounds.Brake = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Brake.wav"), center, small);
			Train.Cars[Train.DriverCar].Sounds.Halt = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Halt.wav"), cab, tiny);
			Train.Cars[Train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Klaxon0.wav"), front, large);
			Train.Cars[Train.DriverCar].Sounds.Horns[0].Loop = false;
			if (Train.Cars[Train.DriverCar].Sounds.Horns[0].Sound.SoundBufferIndex == -1) {
				Train.Cars[Train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Klaxon.wav"), front, large);
			}
			Train.Cars[Train.DriverCar].Sounds.Horns[1].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Klaxon1.wav"), front, large);
			Train.Cars[Train.DriverCar].Sounds.Horns[1].Loop = false;
			Train.Cars[Train.DriverCar].Sounds.Horns[2].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Klaxon2.wav"), front, medium);
			Train.Cars[Train.DriverCar].Sounds.Horns[2].Loop = true;
			Train.Cars[Train.DriverCar].Sounds.PilotLampOn = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Leave.wav"), cab, tiny);
			Train.Cars[Train.DriverCar].Sounds.PilotLampOff = TrainManager.CarSound.Empty;
			// load sounds for all cars
			for (int i = 0; i < Train.Cars.Length; i++) {
				World.Vector3D frontaxle = new World.Vector3D(0.0, 0.0, Train.Cars[i].FrontAxlePosition);
				World.Vector3D rearaxle = new World.Vector3D(0.0, 0.0, Train.Cars[i].RearAxlePosition);
				Train.Cars[i].Sounds.Air = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Air.wav"), center, small);
				Train.Cars[i].Sounds.AirHigh = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "AirHigh.wav"), center, small);
				Train.Cars[i].Sounds.AirZero = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "AirZero.wav"), center, small);
				if (Train.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
					Train.Cars[i].Sounds.CpEnd = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "CpEnd.wav"), center, medium);
					Train.Cars[i].Sounds.CpLoop = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "CpLoop.wav"), center, medium);
					Train.Cars[i].Sounds.CpStart = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "CpStart.wav"), center, medium);
				}
				Train.Cars[i].Sounds.BreakerResume = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BreakerResumeOrInterrupt = TrainManager.CarSound.Empty;
				Train.Cars[i].Sounds.BreakerResumed = false;
				Train.Cars[i].Sounds.DoorCloseL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorClsL.wav"), left, small);
				Train.Cars[i].Sounds.DoorCloseR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorClsR.wav"), right, small);
				if (Train.Cars[i].Sounds.DoorCloseL.SoundBufferIndex == -1) {
					Train.Cars[i].Sounds.DoorCloseL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorCls.wav"), left, small);
				}
				if (Train.Cars[i].Sounds.DoorCloseR.SoundBufferIndex == -1) {
					Train.Cars[i].Sounds.DoorCloseR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorCls.wav"), right, small);
				}
				Train.Cars[i].Sounds.DoorOpenL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorOpnL.wav"), left, small);
				Train.Cars[i].Sounds.DoorOpenR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorOpnR.wav"), right, small);
				if (Train.Cars[i].Sounds.DoorOpenL.SoundBufferIndex == -1) {
					Train.Cars[i].Sounds.DoorOpenL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorOpn.wav"), left, small);
				}
				if (Train.Cars[i].Sounds.DoorOpenR.SoundBufferIndex == -1) {
					Train.Cars[i].Sounds.DoorOpenR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "DoorOpn.wav"), right, small);
				}
				Train.Cars[i].Sounds.EmrBrake = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "EmrBrake.wav"), center, medium);
				Train.Cars[i].Sounds.Flange = TryLoadSoundArray(TrainPath, "Flange", ".wav", center, medium);
				Train.Cars[i].Sounds.FlangeVolume = new double[Train.Cars[i].Sounds.Flange.Length];
				Train.Cars[i].Sounds.Loop = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Loop.wav"), center, medium);
				Train.Cars[i].Sounds.PointFrontAxle = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Point.wav"), frontaxle, small);
				Train.Cars[i].Sounds.PointRearAxle = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Point.wav"), rearaxle, small);
				Train.Cars[i].Sounds.Rub = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Rub.wav"), center, medium);
				Train.Cars[i].Sounds.Run = TryLoadSoundArray(TrainPath, "Run", ".wav", center, medium);
				Train.Cars[i].Sounds.RunVolume = new double[Train.Cars[i].Sounds.Run.Length];
				Train.Cars[i].Sounds.SpringL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "SpringL.wav"), left, small);
				Train.Cars[i].Sounds.SpringR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "SpringR.wav"), right, small);
				// motor sound
				if (Train.Cars[i].Specs.IsMotorCar) {
					System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
					Train.Cars[i].Sounds.Motor.Position = center;
					for (int j = 0; j < Train.Cars[i].Sounds.Motor.Tables.Length; j++) {
						Train.Cars[i].Sounds.Motor.Tables[j].SoundBufferIndex = -1;
						Train.Cars[i].Sounds.Motor.Tables[j].SoundSourceIndex = -1;
						for (int k = 0; k < Train.Cars[i].Sounds.Motor.Tables[j].Entries.Length; k++) {
							int idx = Train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundBufferIndex;
							if (idx <= -2) {
								idx = -2 - idx;
								if (idx >= 0) {
									TrainManager.CarSound snd = TryLoadSound(Interface.GetCombinedFileName(TrainPath, "Motor" + idx.ToString(Culture) + ".wav"), center, medium);
									Train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundBufferIndex = snd.SoundBufferIndex;
								} else {
									Train.Cars[i].Sounds.Motor.Tables[j].Entries[k].SoundBufferIndex = -1;
								}
							}
						}
					}
				}
			}
		}

		// load bve 4 sound
		private static void LoadBve4Sound(string FileName, string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			// set sound positions and radii
			World.Vector3D center = new World.Vector3D(0.0, 0.0, 0.0);
			World.Vector3D left = new World.Vector3D(-1.3, 0.0, 0.0);
			World.Vector3D right = new World.Vector3D(1.3, 0.0, 0.0);
			World.Vector3D front = new World.Vector3D(0.0, 0.0, 0.5 * Train.Cars[0].Length);
			World.Vector3D cab = new World.Vector3D(-Train.Cars[0].DriverX, Train.Cars[0].DriverY, Train.Cars[0].DriverZ - 0.5);
			World.Vector3D panel = new World.Vector3D(Train.Cars[0].DriverX, Train.Cars[0].DriverY, Train.Cars[0].DriverZ + 1.0);
			double large = 30.0;
			double medium = 10.0;
			double small = 5.0;
			double tiny = 2.0;
			LoadNoSound(Train);
			// parse configuration file
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
			}
			if (Lines.Length < 1 || string.Compare(Lines[0], "version 1.0", StringComparison.OrdinalIgnoreCase) != 0) {
				Interface.AddMessage(Interface.MessageType.Error, false, "Invalid file format encountered in " + FileName + ". The first line is expected to be \"Version 1.0\".");
			}
			string[] MotorFiles = new string[] { };
			double invfac = Lines.Length == 0 ? Loading.TrainProgressCurrentWeight : Loading.TrainProgressCurrentWeight / (double)Lines.Length;
			for (int i = 0; i < Lines.Length; i++) {
				Loading.TrainProgress = Loading.TrainProgressCurrentSum + invfac * (double)i;
				if ((i & 7) == 0) {
					System.Threading.Thread.Sleep(1);
					if (Loading.Cancel) return;
				}
				switch (Lines[i].ToLowerInvariant()) {
					case "[run]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										for (int c = 0; c < Train.Cars.Length; c++) {
											int n = Train.Cars[c].Sounds.Run.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref Train.Cars[c].Sounds.Run, k + 1);
												for (int h = n; h < k; h++) {
													Train.Cars[c].Sounds.Run[h] = TrainManager.CarSound.Empty;
													Train.Cars[c].Sounds.Run[h].SoundSourceIndex = -1;
												}
											}
											Train.Cars[c].Sounds.Run[k] = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[flange]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										for (int c = 0; c < Train.Cars.Length; c++) {
											int n = Train.Cars[c].Sounds.Flange.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref Train.Cars[c].Sounds.Flange, k + 1);
												for (int h = n; h < k; h++) {
													Train.Cars[c].Sounds.Flange[h] = TrainManager.CarSound.Empty;
													Train.Cars[c].Sounds.Flange[h].SoundSourceIndex = -1;
												}
											}
											Train.Cars[c].Sounds.Flange[k] = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[motor]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								int k;
								if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									if (k >= 0) {
										if (k >= MotorFiles.Length) {
											Array.Resize<string>(ref MotorFiles, k + 1);
										}
										MotorFiles[k] = Interface.GetCombinedFileName(TrainPath, b);
										if (!System.IO.File.Exists(MotorFiles[k])) {
											Interface.AddMessage(Interface.MessageType.Error, true, "File " + MotorFiles[k] + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											MotorFiles[k] = null;
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							} i++;
						} i--; break;
					case "[switch]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (a == "0") {
									for (int c = 0; c < Train.Cars.Length; c++) {
										World.Vector3D frontaxle = new World.Vector3D(0.0, 0.0, Train.Cars[c].FrontAxlePosition);
										World.Vector3D rearaxle = new World.Vector3D(0.0, 0.0, Train.Cars[c].RearAxlePosition);
										Train.Cars[c].Sounds.PointFrontAxle = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), frontaxle, small);
										Train.Cars[c].Sounds.PointRearAxle = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), rearaxle, small);
									}
								} else {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported index " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} i++;
						} i--; break;
					case "[brake]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "bc release high":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.AirHigh = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, small);
											} break;
										case "bc release":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.Air = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, small);
											} break;
										case "bc release full":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.AirZero = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, small);
											} break;
										case "emergency":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.EmrBrake = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
											} break;
										case "bp decomp":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.Brake = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[compressor]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									for (int c = 0; c < Train.Cars.Length; c++) {
										if (Train.Cars[c].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
											switch (a.ToLowerInvariant()) {
												case "attack":
													Train.Cars[c].Sounds.CpStart = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
													break;
												case "loop":
													Train.Cars[c].Sounds.CpLoop = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
													break;
												case "release":
													Train.Cars[c].Sounds.CpEnd = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
													break;
												default:
													Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
											}
										}
									}
								}
							} i++;
						} i--; break;
					case "[suspension]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "left":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.SpringL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), left, small);
											} break;
										case "right":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.SpringR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), right, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[horn]":
						i++;
						while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "primary":
											Train.Cars[Train.DriverCar].Sounds.Horns[0].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), front, large);
											Train.Cars[Train.DriverCar].Sounds.Horns[0].Loop = false;
											break;
										case "secondary":
											Train.Cars[Train.DriverCar].Sounds.Horns[1].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), front, large);
											Train.Cars[Train.DriverCar].Sounds.Horns[1].Loop = false;
											break;
										case "music":
											Train.Cars[Train.DriverCar].Sounds.Horns[2].Sound = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), front, medium);
											Train.Cars[Train.DriverCar].Sounds.Horns[2].Loop = true;
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[door]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "open left":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.DoorOpenL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), left, small);
											} break;
										case "open right":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.DoorOpenR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), left, small);
											} break;
										case "close left":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.DoorCloseL = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), left, small);
											} break;
										case "close right":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.DoorCloseR = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), left, small);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[ats]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									int k;
									if (!int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out k)) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (k >= 0) {
											int n = Train.Cars[Train.DriverCar].Sounds.Plugin.Length;
											if (k >= n) {
												Array.Resize<TrainManager.CarSound>(ref Train.Cars[Train.DriverCar].Sounds.Plugin, k + 1);
												for (int h = n; h < k; h++) {
													Train.Cars[Train.DriverCar].Sounds.Plugin[h] = TrainManager.CarSound.Empty;
													Train.Cars[Train.DriverCar].Sounds.Plugin[h].SoundSourceIndex = -1;
												}
											}
											Train.Cars[Train.DriverCar].Sounds.Plugin[k] = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
										} else {
											Interface.AddMessage(Interface.MessageType.Warning, false, "Index must be greater or equal to zero at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
								}
							} i++;
						} i--; break;
					case "[buzzer]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "correct":
											Train.Cars[Train.DriverCar].Sounds.Adjust = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[pilot lamp]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											Train.Cars[Train.DriverCar].Sounds.PilotLampOn = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "off":
											Train.Cars[Train.DriverCar].Sounds.PilotLampOff = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[brake handle]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "apply":
											Train.Cars[Train.DriverCar].Sounds.BrakeHandleApply = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "release":
											Train.Cars[Train.DriverCar].Sounds.BrakeHandleRelease = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "min":
											Train.Cars[Train.DriverCar].Sounds.BrakeHandleMin = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "max":
											Train.Cars[Train.DriverCar].Sounds.BrakeHandleMax = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[master controller]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "up":
											Train.Cars[Train.DriverCar].Sounds.MasterControllerUp = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "down":
											Train.Cars[Train.DriverCar].Sounds.MasterControllerDown = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "min":
											Train.Cars[Train.DriverCar].Sounds.MasterControllerMin = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "max":
											Train.Cars[Train.DriverCar].Sounds.MasterControllerMax = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[reverser]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											Train.Cars[Train.DriverCar].Sounds.ReverserOn = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										case "off":
											Train.Cars[Train.DriverCar].Sounds.ReverserOff = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, tiny);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[breaker]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "on":
											Train.Cars[Train.DriverCar].Sounds.BreakerResume = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, small);
											break;
										case "off":
											Train.Cars[Train.DriverCar].Sounds.BreakerResumeOrInterrupt = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), panel, small);
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
					case "[others]":
						i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal)) {
							int j = Lines[i].IndexOf("=");
							if (j >= 0) {
								string a = Lines[i].Substring(0, j).TrimEnd();
								string b = Lines[i].Substring(j + 1).TrimStart();
								if (Interface.ContainsInvalidPathChars(b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "FileName contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									switch (a.ToLowerInvariant()) {
										case "noise":
											for (int c = 0; c < Train.Cars.Length; c++) {
												if (Train.Cars[c].Specs.IsMotorCar | c == Train.DriverCar) {
													Train.Cars[c].Sounds.Loop = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
												}
											} break;
										case "shoe":
											for (int c = 0; c < Train.Cars.Length; c++) {
												Train.Cars[c].Sounds.Rub = TryLoadSound(Interface.GetCombinedFileName(TrainPath, b), center, medium);
											} break;
										default:
											Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
							} i++;
						} i--; break;
				}
			}
			for (int i = 0; i < Train.Cars.Length; i++) {
				Train.Cars[i].Sounds.RunVolume = new double[Train.Cars[i].Sounds.Run.Length];
				Train.Cars[i].Sounds.FlangeVolume = new double[Train.Cars[i].Sounds.Flange.Length];
			}
			// motor sound
			for (int c = 0; c < Train.Cars.Length; c++) {
				if (Train.Cars[c].Specs.IsMotorCar) {
					Train.Cars[c].Sounds.Motor.Position = center;
					for (int i = 0; i < Train.Cars[c].Sounds.Motor.Tables.Length; i++) {
						Train.Cars[c].Sounds.Motor.Tables[i].SoundBufferIndex = -1;
						Train.Cars[c].Sounds.Motor.Tables[i].SoundSourceIndex = -1;
						for (int j = 0; j < Train.Cars[c].Sounds.Motor.Tables[i].Entries.Length; j++) {
							int idx = Train.Cars[c].Sounds.Motor.Tables[i].Entries[j].SoundBufferIndex;
							if (idx <= -2) {
								idx = -2 - idx;
								if (idx >= 0 && idx < MotorFiles.Length && MotorFiles[idx] != null) {
									TrainManager.CarSound snd = TryLoadSound(MotorFiles[idx], center, medium);
									Train.Cars[c].Sounds.Motor.Tables[i].Entries[j].SoundBufferIndex = snd.SoundBufferIndex;
								} else {
									Train.Cars[c].Sounds.Motor.Tables[i].Entries[j].SoundBufferIndex = -1;
								}
							}
						}
					}
				}
			}
		}

		// try load sound
		private static TrainManager.CarSound TryLoadSound(string FileName, World.Vector3D Position, double Radius) {
			TrainManager.CarSound s;
			s = TrainManager.CarSound.Empty;
			s.Position = Position;
			s.SoundSourceIndex = -1;
			if (FileName != null) {
				if (System.IO.File.Exists(FileName)) {
					s.SoundBufferIndex = SoundManager.LoadSound(FileName, Radius);
				}
			}
			return s;
		}
		private static TrainManager.CarSound[] TryLoadSoundArray(string Folder, string FileStart, string FileEnd, World.Vector3D Position, double Radius) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			TrainManager.CarSound[] Sounds = new TrainManager.CarSound[] { };
			string[] Files = System.IO.Directory.GetFiles(Folder);
			for (int i = 0; i < Files.Length; i++) {
				string a = System.IO.Path.GetFileName(Files[i]);
				if (a.Length > FileStart.Length + FileEnd.Length) {
					if (a.StartsWith(FileStart, StringComparison.OrdinalIgnoreCase) & a.EndsWith(FileEnd, StringComparison.OrdinalIgnoreCase)) {
						string b = a.Substring(FileStart.Length, a.Length - FileEnd.Length - FileStart.Length);
						int n; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, Culture, out n)) {
							if (n >= 0) {
								int m = Sounds.Length;
								if (n >= m) {
									Array.Resize<TrainManager.CarSound>(ref Sounds, n + 1);
									for (int j = m; j < n; j++) {
										Sounds[j] = TrainManager.CarSound.Empty;
										Sounds[j].SoundSourceIndex = -1;
									}
								}
								Sounds[n] = TryLoadSound(Files[i], Position, Radius);
							}
						}
					}
				}
			}
			return Sounds;
		}

	}
}