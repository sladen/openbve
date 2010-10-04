using System;
using System.Runtime.InteropServices;
using OpenBveApi.Runtime;

namespace OpenBve {
	/// <summary>Represents a legacy Win32 plugin.</summary>
	internal class LegacyPlugin : PluginManager.Plugin {
		
		// --- win32 proxy calls ---
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "LoadDLL", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int Win32LoadDLL([MarshalAs(UnmanagedType.LPWStr)]string UnicodeFileName, [MarshalAs(UnmanagedType.LPStr)]string AnsiFileName);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "UnloadDLL", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int Win32UnloadDLL();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Load", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32Load();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Dispose", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32Dispose();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "GetPluginVersion", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static int Win32GetPluginVersion();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetVehicleSpec", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetVehicleSpec(ref int spec);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Initialize", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32Initialize(int brake);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "Elapse", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32Elapse(ref int handles, ref double state, ref int panel, ref int sound);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetPower", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetPower(int notch);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetBrake", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetBrake(int notch);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetReverser", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetReverser(int pos);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "KeyDown", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32KeyDown(int atsKeyCode);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "KeyUp", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32KeyUp(int atsKeyCode);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "HornBlow", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32HornBlow(int hornType);

		[DllImport("AtsPluginProxy.dll", EntryPoint = "DoorOpen", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32DoorOpen();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "DoorClose", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32DoorClose();
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetSignal", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetSignal(int signal);
		
		[DllImport("AtsPluginProxy.dll", EntryPoint = "SetBeaconData", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		private extern static void Win32SetBeaconData(ref int beacon);
		
		[StructLayout(LayoutKind.Sequential, Size = 20)]
		private struct Win32VehicleSpec {
			internal int BrakeNotches;
			internal int PowerNotches;
			internal int AtsNotch;
			internal int B67Notch;
			internal int Cars;
		}
		
		[StructLayout(LayoutKind.Sequential, Size = 40)]
		private struct Win32VehicleState {
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
		private struct Win32Handles {
			internal int Brake;
			internal int Power;
			internal int Reverser;
			internal int ConstantSpeed;
		}
		
		[StructLayout(LayoutKind.Sequential, Size = 16)]
		private struct Win32BeaconData {
			internal int Type;
			internal int Signal;
			internal float Distance;
			internal int Optional;
		}
		
		// --- members ---
		private string PluginFile;
		private GCHandle PanelHandle;
		private GCHandle SoundHandle;
		
		// --- constructors ---
		internal LegacyPlugin(string pluginFile) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Panel = new int[256];
			base.Sound = new int[256];
			base.LastTime = 0.0;
			base.LastSound = new int[256];
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspect = -1;
			base.LastException = null;
			this.PluginFile = pluginFile;
			this.PanelHandle = new GCHandle();
			this.SoundHandle = new GCHandle();
		}
		
		// --- functions ---
		internal override bool Load(TrainManager.Train train, VehicleSpecs specs, InitializationModes mode) {
			int result;
			try {
				result = Win32LoadDLL(this.PluginFile, this.PluginFile);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			if (result == 0) {
				return false;
			}
			try {
				Win32Load();
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			int version;
			try {
				version = Win32GetPluginVersion();
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			if (version != 131072) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " is of an unsupported version.");
				try {
					Win32Load();
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
				Win32UnloadDLL();
				return false;
			}
			try {
				Win32VehicleSpec win32Spec;
				win32Spec.BrakeNotches = specs.BrakeNotches;
				win32Spec.PowerNotches = specs.PowerNotches;
				win32Spec.AtsNotch = specs.AtsNotch;
				win32Spec.B67Notch = specs.B67Notch;
				win32Spec.Cars = specs.Cars;
				Win32SetVehicleSpec(ref win32Spec.BrakeNotches);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			try {
				Win32Initialize((int)mode);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			UpdatePower(train);
			UpdateBrake(train);
			UpdateReverser(train);
			if (PanelHandle.IsAllocated) {
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated) {
				SoundHandle.Free();
			}
			PanelHandle = GCHandle.Alloc(Panel, GCHandleType.Pinned);
			SoundHandle = GCHandle.Alloc(Sound, GCHandleType.Pinned);
			return true;
		}
		internal override void Unload() {
			if (PanelHandle.IsAllocated) {
				PanelHandle.Free();
			}
			if (SoundHandle.IsAllocated) {
				SoundHandle.Free();
			}
			try {
				Win32UnloadDLL();
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void BeginJump(InitializationModes mode) {
			try {
				Win32Initialize((int)mode);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void EndJump() { }
		internal override void Elapse(VehicleState state, Handles handles, out string message) {
			try {
				double time = state.TotalTime.Milliseconds;
				Win32VehicleState win32State;
				win32State.Location = state.Location;
				win32State.Speed = (float)state.Speed.KilometersPerHour;
				win32State.Time = (int)Math.Floor(time - 2073600000.0 * Math.Floor(time / 2073600000.0));
				win32State.BcPressure = (float)state.BcPressure;
				win32State.MrPressure = (float)state.MrPressure;
				win32State.ErPressure = (float)state.ErPressure;
				win32State.BpPressure = (float)state.BpPressure;
				win32State.SapPressure = (float)state.SapPressure;
				win32State.Current = 0.0f;
				Win32Handles win32Handles;
				win32Handles.Brake = handles.BrakeNotch;
				win32Handles.Power = handles.PowerNotch;
				win32Handles.Reverser = handles.Reverser;
				win32Handles.ConstantSpeed = (int)handles.ConstSpeed;
				Win32Elapse(ref win32Handles.Brake, ref win32State.Location, ref base.Panel[0], ref base.Sound[0]);
				handles.Reverser = win32Handles.Reverser;
				handles.PowerNotch = win32Handles.Power;
				handles.BrakeNotch = win32Handles.Brake;
				handles.ConstSpeed = (ConstSpeedInstructions)win32Handles.ConstantSpeed;
				message = null;
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetReverser(int reverser) {
			try {
				Win32SetReverser(reverser);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetPower(int powerNotch) {
			try {
				Win32SetPower(powerNotch);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBrake(int brakeNotch) {
			try {
				Win32SetBrake(brakeNotch);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void KeyDown(VirtualKeys key) {
			try {
				Win32KeyDown((int)key);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void KeyUp(VirtualKeys key) {
			try {
				Win32KeyUp((int)key);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void HornBlow(HornTypes type) {
			try {
				Win32HornBlow((int)type);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void DoorChange(DoorStates oldState, DoorStates newState) {
			if (oldState == DoorStates.None & newState != DoorStates.None) {
				try {
					Win32DoorOpen();
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
			} else if (oldState != DoorStates.None & newState == DoorStates.None) {
				try {
					Win32DoorClose();
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
			}
		}
		internal override void SetSignal(SignalData signal) {
			try {
				Win32SetSignal(signal.Aspect);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBeacon(BeaconData beacon) {
			try {
				Win32BeaconData win32Beacon;
				win32Beacon.Type = beacon.Type;
				win32Beacon.Signal = beacon.Signal.Aspect;
				win32Beacon.Distance = (float)beacon.Signal.Distance;
				win32Beacon.Optional = beacon.Optional;
				Win32SetBeaconData(ref win32Beacon.Type);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		
	}
}