using System;
using System.Runtime.InteropServices;

namespace OpenBve {
	internal static class PluginManager {
		
		// enumerations
		internal enum PluginLoadState { Successful, CouldNotLoadDll, InvalidPluginVersion }
		
		// members
		internal static string PluginName = null;
		internal static bool PluginValid = false;
		internal static bool PluginError = false;
		
		// constants
		private const int ATS_VERSION = 131072;
		internal const int ATS_KEY_S = 0;
		internal const int ATS_KEY_A1 = 1;
		internal const int ATS_KEY_A2 = 2;
		internal const int ATS_KEY_B1 = 3;
		internal const int ATS_KEY_B2 = 4;
		internal const int ATS_KEY_C1 = 5;
		internal const int ATS_KEY_C2 = 6;
		internal const int ATS_KEY_D = 7;
		internal const int ATS_KEY_E = 8;
		internal const int ATS_KEY_F = 9;
		internal const int ATS_KEY_G = 10;
		internal const int ATS_KEY_H = 11;
		internal const int ATS_KEY_I = 12;
		internal const int ATS_KEY_J = 13;
		internal const int ATS_KEY_K = 14;
		internal const int ATS_KEY_L = 15;
		
		// structures and constants
		internal static int[] PluginPanel = new int[256];
		private static int[] PluginSound = new int[256];
		private static int[] PluginSoundCache = new int[256];
		private static bool[] AtsKeyPressed = new bool[16];
		private static GCHandle PanelHandle;
		private static GCHandle SoundHandle;
		[StructLayout(LayoutKind.Sequential, Size = 20)]
		private struct ATS_VEHICLESPEC {
			internal int BrakeNotches;
			internal int PowerNotches;
			internal int AtsNotch;
			internal int B67Notch;
			internal int Cars;
		}
		[StructLayout(LayoutKind.Sequential, Size = 40)]
		private struct ATS_VEHICLESTATE {
			internal double Location;
			internal float Speed;
			internal int Time;
			internal float BcPressure;
			internal float MrPressure;
			internal float ErPressure;
			internal float BpPressure;
			internal float SapPressure;
			internal float Current;
		}
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct ATS_BEACONDATA {
			internal int Type;
			internal int Signal;
			internal float Distance;
			internal int Optional;
		}
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct ATS_HANDLES {
			internal int Brake;
			internal int Power;
			internal int Reverser;
			internal int ConstantSpeed;
		}
		private const int ATS_INIT_OFF = -1;
		private const int ATS_INIT_ON_SVC = 0;
		private const int ATS_INIT_ON_EMG = 1;
		private const int ATS_SOUND_STOP = -10000;
		private const int ATS_SOUND_PLAYLOOPING = 0;
		private const int ATS_SOUND_PLAY = 1;
		private const int ATS_SOUND_CONTINUE = 2;
		private const int ATS_HORN_PRIMARY = 0;
		private const int ATS_HORN_SECONDARY = 1;
		private const int ATS_HORN_MUSIC = 2;
		private const int ATS_CONSTANTSPEED_CONTINUE = 0;
		private const int ATS_CONSTANTSPEED_ENABLE = 1;
		private const int ATS_CONSTANTSPEED_DISABLE = 2;
		
		// proxy functions
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int LoadDLL([MarshalAs(UnmanagedType.LPWStr)]string UnicodeFileName, [MarshalAs(UnmanagedType.LPStr)]string AnsiFileName);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int UnloadDLL();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Load();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Dispose();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int GetPluginVersion();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetVehicleSpec(ref int spec);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Initialize(int brake);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Elapse(ref int handles, ref double state, ref int panel, ref int sound);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetPower(int notch);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetBrake(int notch);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetReverser(int pos);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void KeyDown(int atsKeyCode);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void KeyUp(int atsKeyCode);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void HornBlow(int hornType);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void DoorOpen();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void DoorClose();
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetSignal(int signal);
		[DllImport("AtsPluginProxy.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void SetBeaconData(ref int beacon);
		
		// cached data
		private static bool PluginLoaded = false;
		private static int LastSignalAspect = -1;
		private static int LastReverserPosition = -2;
		private static int LastPowerNotch = -1;
		private static int LastBrakeNotch = -1;
		
		// load plugin
		private static PluginLoadState LoadPlugin(string FileName, TrainManager.Train Train) {
			// unload previous plugin
			UnloadPlugin();
			// pin arrays
			if (PanelHandle.IsAllocated) PanelHandle.Free();
			if (SoundHandle.IsAllocated) SoundHandle.Free();
			PanelHandle = GCHandle.Alloc(PluginPanel, GCHandleType.Pinned);
			SoundHandle = GCHandle.Alloc(PluginSound, GCHandleType.Pinned);
			// initialize arrays
			for (int i = 0; i < PluginPanel.Length; i++) {
				PluginPanel[i] = 0;
			}
			for (int i = 0; i < PluginSound.Length; i++) {
				PluginSound[i] = ATS_SOUND_PLAYLOOPING;
				PluginSoundCache[i] = ATS_SOUND_PLAYLOOPING;
			}
			// initialize proxy
			PluginName = System.IO.Path.GetFileName(FileName);
			int a = LoadDLL(FileName, FileName);
			if (a == 0) {
				UnloadPlugin();
				return PluginLoadState.CouldNotLoadDll;
			}
			// load and check version
			{
				PluginError = true;
				Load();
				PluginError = false;
			}
			PluginLoaded = true;
			int ver;
			{
				PluginError = true;
				ver = GetPluginVersion();
				PluginError = false;
			}
			if (ver != ATS_VERSION) {
				UnloadPlugin();
				return PluginLoadState.InvalidPluginVersion;
			}
			// set vehicle spec
			ATS_VEHICLESPEC Spec = new ATS_VEHICLESPEC();
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
				Spec.BrakeNotches = 2;
				Spec.PowerNotches = Train.Specs.MaximumPowerNotch;
				Spec.AtsNotch = 1;
				Spec.B67Notch = 1;
			} else {
				Spec.BrakeNotches = Train.Specs.MaximumBrakeNotch + (Train.Specs.HasHoldBrake ? 1 : 0);
				Spec.PowerNotches = Train.Specs.MaximumPowerNotch;
				Spec.AtsNotch = Train.Specs.HasHoldBrake ? 2 : 1;
				Spec.B67Notch = (int)Math.Round(0.7 * Spec.BrakeNotches);
			}
			if (Spec.B67Notch < 1) Spec.B67Notch = 1;
			Spec.Cars = Train.Cars.Length;
			{
				PluginError = true;
				SetVehicleSpec(ref Spec.BrakeNotches);
				PluginError = false;
			}
			// initialize
			InitializePlugin(Train);
			// handles
			UpdatePower(Train);
			UpdateBrake(Train);
			UpdateReverser(Train);
			// finalize
			Train.Specs.Security.Mode = TrainManager.SecuritySystem.Bve4Plugin;
			return PluginLoadState.Successful;
		}
		
		// initialize plugin
		internal static void InitializePlugin(TrainManager.Train Train) {
			if (!PluginLoaded) return;
			switch (Game.TrainStart) {
				case Game.TrainStartMode.ServiceBrakesAts:
					PluginError = true;
					Initialize(ATS_INIT_ON_SVC);
					PluginError = false;
					break;
				case Game.TrainStartMode.EmergencyBrakesAts:
					PluginError = true;
					Initialize(ATS_INIT_ON_EMG);
					PluginError = false;
					break;
				case Game.TrainStartMode.EmergencyBrakesNoAts:
					PluginError = true;
					Initialize(ATS_INIT_OFF);
					PluginError = false;
					break;
				default:
					PluginError = true;
					Initialize(ATS_INIT_ON_SVC);
					PluginError = false;
					break;
			}
		}
		
		// unload plugin
		internal static void UnloadPlugin() {
			if (!PluginLoaded) return;
			{
				PluginError = true;
				Dispose();
				PluginError = false;
			}
			UnloadDLL();
			if (PanelHandle.IsAllocated) PanelHandle.Free();
			if (SoundHandle.IsAllocated) SoundHandle.Free();
			PluginLoaded = false;
			PluginName = null;
		}
		
		// update plugin
		internal static void UpdatePlugin(TrainManager.Train Train) {
			if (!PluginLoaded) return;
			// prepare vehicle state
			ATS_VEHICLESTATE State = new ATS_VEHICLESTATE();
			State.Location = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
			State.Speed = (float)(3.6 * Train.Cars[0].Specs.CurrentPerceivedSpeed);
			double t = 1000.0 * Game.SecondsSinceMidnight;
			State.Time = (int)Math.Floor(t - 2073600000.0 * Math.Floor(t / 2073600000.0));
			State.BcPressure = (float)Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure;
			State.MrPressure = (float)Train.Cars[Train.DriverCar].Specs.AirBrake.MainReservoirCurrentPressure;
			State.ErPressure = (float)Train.Cars[Train.DriverCar].Specs.AirBrake.EqualizingReservoirCurrentPressure;
			State.BpPressure = (float)Train.Cars[Train.DriverCar].Specs.AirBrake.BrakePipeCurrentPressure;
			State.SapPressure = (float)Train.Cars[Train.DriverCar].Specs.AirBrake.StraightAirPipeCurrentPressure;
			State.Current = 0.0f;
			// elapse
			ATS_HANDLES Handles = new ATS_HANDLES();
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
				Handles.Brake = Train.Specs.CurrentEmergencyBrake.Driver ? 3 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
			} else {
				if (Train.Specs.HasHoldBrake) {
					Handles.Brake = Train.Specs.CurrentEmergencyBrake.Driver ? Train.Specs.MaximumBrakeNotch + 2 : Train.Specs.CurrentBrakeNotch.Driver > 0 ? Train.Specs.CurrentBrakeNotch.Driver + 1 : Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
				} else {
					Handles.Brake = Train.Specs.CurrentEmergencyBrake.Driver ? Train.Specs.MaximumBrakeNotch + 1 : Train.Specs.CurrentBrakeNotch.Driver;
				}
			}
			Handles.Power = Train.Specs.CurrentPowerNotch.Driver;
			Handles.Reverser = Train.Specs.CurrentReverser.Driver;
			Handles.ConstantSpeed = ATS_CONSTANTSPEED_CONTINUE;
			{
				PluginError = true;
				Elapse(ref Handles.Brake, ref State.Location, ref PluginPanel[0], ref PluginSound[0]);
				PluginError = false;
			}
			if (Train.Specs.SingleHandle & Handles.Brake != 0) Handles.Power = 0;
			PluginValid = true;
			// process reverser
			if (Handles.Reverser >= -1 & Handles.Reverser <= 1) {
				Train.Specs.CurrentReverser.Actual = Handles.Reverser;
			} else {
				Train.Specs.CurrentReverser.Actual = Train.Specs.CurrentReverser.Driver;
				PluginValid = false;
			}
			// process power
			if (Handles.Power >= 0 & Handles.Power <= Train.Specs.MaximumPowerNotch) {
				Train.Specs.CurrentPowerNotch.Security = Handles.Power;
			} else {
				Train.Specs.CurrentPowerNotch.Security = Train.Specs.CurrentPowerNotch.Driver;
				PluginValid = false;
			}
			if (Handles.Brake != 0) Train.Specs.CurrentPowerNotch.Security = 0;
			// process brake
			Train.Specs.CurrentEmergencyBrake.Security = false;
			Train.Specs.CurrentHoldBrake.Actual = false;
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
				if (Handles.Brake == 0) {
					Train.Specs.AirBrake.Handle.Security = TrainManager.AirBrakeHandleState.Release;
				} else if (Handles.Brake == 1) {
					Train.Specs.AirBrake.Handle.Security = TrainManager.AirBrakeHandleState.Lap;
				} else if (Handles.Brake == 2) {
					Train.Specs.AirBrake.Handle.Security = TrainManager.AirBrakeHandleState.Service;
				} else if (Handles.Brake == 3) {
					Train.Specs.AirBrake.Handle.Security = TrainManager.AirBrakeHandleState.Service;
					Train.Specs.CurrentEmergencyBrake.Security = true;
				} else {
					PluginValid = false;
				}
			} else {
				// notched brake
				if (Train.Specs.HasHoldBrake) {
					// with hold brake
					if (Handles.Brake == Train.Specs.MaximumBrakeNotch + 2) {
						Train.Specs.CurrentEmergencyBrake.Security = true;
						Train.Specs.CurrentBrakeNotch.Security = Train.Specs.MaximumBrakeNotch;
					} else if (Handles.Brake >= 2 & Handles.Brake <= Train.Specs.MaximumBrakeNotch + 1) {
						Train.Specs.CurrentBrakeNotch.Security = Handles.Brake - 1;
					} else if (Handles.Brake == 1) {
						Train.Specs.CurrentBrakeNotch.Security = 0;
						Train.Specs.CurrentHoldBrake.Actual = true;
					} else if (Handles.Brake == 0) {
						Train.Specs.CurrentBrakeNotch.Security = 0;
					} else {
						Train.Specs.CurrentBrakeNotch.Security = Train.Specs.CurrentBrakeNotch.Driver;
						PluginValid = false;
					}
				} else {
					// without hold brake
					if (Handles.Brake == Train.Specs.MaximumBrakeNotch + 1) {
						Train.Specs.CurrentEmergencyBrake.Security = true;
						Train.Specs.CurrentBrakeNotch.Security = Train.Specs.MaximumBrakeNotch;
					} else if (Handles.Brake >= 0 & Handles.Brake <= Train.Specs.MaximumBrakeNotch | Train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0) {
						Train.Specs.CurrentBrakeNotch.Security = Handles.Brake;
					} else {
						Train.Specs.CurrentBrakeNotch.Security = Train.Specs.CurrentBrakeNotch.Driver;
						PluginValid = false;
					}
				}
			}
			// process const speed
			if (Handles.ConstantSpeed == ATS_CONSTANTSPEED_ENABLE) {
				Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
			} else if (Handles.ConstantSpeed == ATS_CONSTANTSPEED_DISABLE) {
				Train.Specs.CurrentConstSpeed = false;
			} else if (Handles.ConstantSpeed != ATS_CONSTANTSPEED_CONTINUE) {
				PluginValid = false;
			}
			// process sound
			for (int i = 0; i < PluginSound.Length; i++) {
				if (PluginSound[i] != PluginSoundCache[i]) {
					if (PluginSound[i] == ATS_SOUND_STOP) {
						if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length) {
							SoundManager.StopSound(ref Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex);
						}
					} else if (PluginSound[i] > ATS_SOUND_STOP & PluginSound[i] <= ATS_SOUND_PLAYLOOPING) {
						if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length) {
							int snd = Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundBufferIndex;
							if (snd >= 0) {
								double gain = (double)(PluginSound[i] - ATS_SOUND_STOP) / (double)(ATS_SOUND_PLAYLOOPING - ATS_SOUND_STOP);
								if (SoundManager.IsPlaying(Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex)) {
									SoundManager.ModulateSound(Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, 1.0, gain);
								} else {
									SoundManager.PlaySound(ref Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, snd, Train, Train.DriverCar, Train.Cars[Train.DriverCar].Sounds.Plugin[i].Position, SoundManager.Importance.AlwaysPlay, true, 1.0, gain);
								}
							}
						}
					} else if (PluginSound[i] == ATS_SOUND_PLAY) {
						if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length) {
							int snd = Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundBufferIndex;
							if (snd >= 0) {
								SoundManager.PlaySound(ref Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, snd, Train, Train.DriverCar, Train.Cars[Train.DriverCar].Sounds.Plugin[i].Position, SoundManager.Importance.AlwaysPlay, false);
							}
						}
						PluginSound[i] = ATS_SOUND_CONTINUE;
					} else if (PluginSound[i] != ATS_SOUND_CONTINUE) {
						PluginValid = false;
					}
					PluginSoundCache[i] = PluginSound[i];
				} else {
					if (i < Train.Cars[Train.DriverCar].Sounds.Plugin.Length) {
						if (Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex >= 0) {
							if (SoundManager.HasFinishedPlaying(Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex)) {
								SoundManager.StopSound(ref Train.Cars[Train.DriverCar].Sounds.Plugin[i].SoundSourceIndex);
							}
						}
					}
					if ((PluginSound[i] < ATS_SOUND_STOP | PluginSound[i] > ATS_SOUND_PLAYLOOPING) && PluginSound[i] != ATS_SOUND_PLAY & PluginSound[i] != ATS_SOUND_CONTINUE) {
						PluginValid = false;
					}
				}
			}
		}
		
		// update signal
		internal static void UpdateSignal(int Aspect) {
			if (!PluginLoaded) return;
			if (Aspect != LastSignalAspect) {
				{
					PluginError = true;
					SetSignal(Aspect);
					PluginError = false;
				}
				LastSignalAspect = Aspect;
			}
		}
		
		// update beacon
		internal static void UpdateBeacon(TrainManager.Train Train, TrainManager.TrainPendingTransponder Data) {
			if (!PluginLoaded) return;
			ATS_BEACONDATA data = new ATS_BEACONDATA();
			data.Type = (int)Data.Type;
			data.Optional = Data.OptionalInteger;
			data.Signal = 255;
			data.Distance = float.MaxValue;
			int s = Data.SectionIndex;
			if (s == (int)TrackManager.TransponderSpecialSection.NextRedSection) {
				// next red section
				s = Train.CurrentSectionIndex;
				if (s >= 0) {
					s = Game.Sections[s].NextSection;
					while (s >= 0) {
						int a = Game.Sections[s].CurrentAspect;
						if (a >= 0) {
							if (Game.Sections[s].Aspects[a].Number == 0) {
								break;
							}
						} s = Game.Sections[s].NextSection;
					}
				}
			}
			// explicit section
			if (s >= 0) {
				if (Game.Sections[s].Exists(TrainManager.PlayerTrain)) {
					// referencing the section of the player's train
					int n = Game.Sections[s].NextSection;
					if (n >= 0) {
						int c = -1;
						int a = Game.Sections[n].CurrentAspect;
						for (int i = Game.Sections[s].Aspects.Length - 1; i >= 0; i--) {
							if (Game.Sections[s].Aspects[i].Number > Game.Sections[n].Aspects[a].Number) {
								c = i;
							}
						} if (c == -1) {
							c = Game.Sections[s].Aspects.Length - 1;
						}
						data.Signal = Game.Sections[s].Aspects[c].Number;
					} else {
						data.Signal = Game.Sections[s].Aspects[Game.Sections[s].Aspects.Length - 1].Number;
					}
				} else {
					// normal behavior
					int a = Game.Sections[s].CurrentAspect;
					if (a >= 0) {
						data.Signal = Game.Sections[s].Aspects[a].Number;
					}
				}
				double p = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
				data.Distance = (float)(Game.Sections[s].TrackPosition - p);
			}
			{
				PluginError = true;
				SetBeaconData(ref data.Type);
				PluginError = false;
			}
		}
		
		// update power
		internal static void UpdatePower(TrainManager.Train Train) {
			if (!PluginLoaded) return;
			int p = Train.Specs.CurrentPowerNotch.Driver;
			if (p != LastPowerNotch) {
				{
					PluginError = true;
					SetPower(Train.Specs.CurrentPowerNotch.Driver);
					PluginError = false;
				}
				LastPowerNotch = p;
			}
		}
		
		// update brake
		internal static void UpdateBrake(TrainManager.Train Train) {
			if (!PluginLoaded) return;
			int b;
			if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
				if (Train.Specs.HasHoldBrake) {
					b = Train.Specs.CurrentEmergencyBrake.Driver ? 4 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 3 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 2 : Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
				} else {
					b = Train.Specs.CurrentEmergencyBrake.Driver ? 3 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				}
			} else {
				if (Train.Specs.HasHoldBrake) {
					b = Train.Specs.CurrentEmergencyBrake.Driver ? Train.Specs.MaximumBrakeNotch + 2 : Train.Specs.CurrentBrakeNotch.Driver > 0 ? Train.Specs.CurrentBrakeNotch.Driver + 1 : Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
				} else {
					b = Train.Specs.CurrentEmergencyBrake.Driver ? Train.Specs.MaximumBrakeNotch + 1 : Train.Specs.CurrentBrakeNotch.Driver;
				}
			}
			if (b != LastBrakeNotch) {
				{
					PluginError = true;
					SetBrake(b);
					PluginError = false;
				}
				LastBrakeNotch = b;
			}
		}
		
		// update reverser
		internal static void UpdateReverser(TrainManager.Train Train) {
			if (!PluginLoaded) return;
			int r = Train.Specs.CurrentReverser.Driver;
			if (r != LastReverserPosition) {
				{
					PluginError = true;
					SetReverser(r);
					PluginError = false;
				}
				LastReverserPosition = r;
			}
		}
		
		// update doors
		internal static void UpdateDoors(bool Closed) {
			if (!PluginLoaded) return;
			if (Closed) {
				PluginError = true;
				DoorClose();
				PluginError = false;
			} else {
				PluginError = true;
				DoorOpen();
				PluginError = false;
			}
		}
		
		// update key
		internal static void UpdateKey(int AtsKeyCode, bool Down) {
			if (!PluginLoaded) return;
			if (Down) {
				if (!AtsKeyPressed[AtsKeyCode]) {
					AtsKeyPressed[AtsKeyCode] = true;
					{
						PluginError = true;
						KeyDown(AtsKeyCode);
						PluginError = false;
					}
				}
			} else {
				if (AtsKeyPressed[AtsKeyCode]) {
					AtsKeyPressed[AtsKeyCode] = false;
					{
						PluginError = true;
						KeyUp(AtsKeyCode);
						PluginError = false;
					}
				}
			}
		}
		
		// update horn
		internal static void UpdateHorn(int Horn) {
			if (!PluginLoaded) return;
			{ 
				PluginError = true;
				HornBlow(Horn);
				PluginError = false;
			}
		}
		
		// load ats config
		internal static bool LoadAtsConfig(string TrainPath, System.Text.Encoding Encoding, TrainManager.Train Train) {
			string File = Interface.GetCombinedFileName(TrainPath, "ats.cfg");
			if (System.IO.File.Exists(File)) {
				string DllTitle = System.IO.File.ReadAllText(File, Encoding).Trim();
				string DllFile = Interface.GetCombinedFileName(TrainPath, DllTitle);
				if (System.IO.File.Exists(DllFile)) {
					if (Program.CurrentPlatform == Program.Platform.Windows) {
						if (IntPtr.Size == 4) {
							PluginLoadState State = LoadPlugin(DllFile, Train);
							switch (State) {
								case PluginLoadState.CouldNotLoadDll:
									Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + DllTitle + " could not be loaded in " + File);
									return false;
								case PluginLoadState.InvalidPluginVersion:
									Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + DllTitle + " is of an unsupported version in " + File);
									return false;
								case PluginLoadState.Successful:
									return true;
								default:
									return false;
							}
						} else {
							Interface.AddMessage(Interface.MessageType.Error, false, "Train plugins are not supported in 64-bit environments. The built-in safety systems will be used for this train, which might not be compatible with the route.");
							return false;
						}
					} else {
						Interface.AddMessage(Interface.MessageType.Information, false, "Train plugins are not supported on operating systems other than Windows. The built-in safety systems will be used for this train, which might not be compatible with the route.");
						return false;
					}
				} else {
					Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + DllTitle + " could not be found in " + File);
					return false;
				}
			} else {
				return false;
			}
		}
		
	}
}