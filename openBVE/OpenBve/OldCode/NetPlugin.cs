using System;
using OpenBveApi.Runtime;

namespace OpenBve {
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : PluginManager.Plugin {
		
		// sound handle
		internal class SoundHandleEx : SoundHandle {
			internal Sounds.SoundSource Source;
			internal SoundHandleEx(double volume, double pitch, Sounds.SoundSource source) {
				base.MyVolume = volume;
				base.MyPitch = pitch;
				base.MyValid = true;
				this.Source = source;
			}
		}
		
		// --- members ---
		private string PluginFolder;
		private string TrainFolder;
		private IRuntime Api;
		private SoundHandleEx[] SoundHandles;
		private int SoundHandlesCount;
		
		// --- constructors ---
		internal NetPlugin(string pluginFile, string trainFolder, IRuntime api, TrainManager.Train train) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Train = train;
			base.Panel = null;
			base.SupportsAI = false;
			base.LastTime = 0.0;
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspects = new int[] { };
			base.LastSection = -1;
			base.LastException = null;
			this.PluginFolder = System.IO.Path.GetDirectoryName(pluginFile);
			this.TrainFolder = trainFolder;
			this.Api = api;
			this.SoundHandles = new SoundHandleEx[16];
			this.SoundHandlesCount = 0;
		}
		
		// --- functions ---
		internal override bool Load(VehicleSpecs specs, InitializationModes mode) {
			LoadProperties properties = new LoadProperties(this.PluginFolder, this.TrainFolder, this.PlaySound);
			bool success;
			try {
				success = this.Api.Load(properties);
				base.SupportsAI = properties.AISupport == AISupport.Basic;
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			if (success) {
				base.Panel = properties.Panel ?? new int[] { };
				try {
					Api.SetVehicleSpecs(specs);
					Api.Initialize(mode);
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
				UpdatePower();
				UpdateBrake();
				UpdateReverser();
				return true;
			} else if (properties.FailureReason != null) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for the following reason: " + properties.FailureReason);
				return false;
			} else {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for an unspecified reason.");
				return false;
			}
		}
		internal override void Unload() {
			try {
				this.Api.Unload();
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void BeginJump(InitializationModes mode) {
			try {
				this.Api.Initialize(mode);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void EndJump() { }
		internal override void Elapse(ElapseData data) {
			try {
				this.Api.Elapse(data);
				for (int i = 0; i < this.SoundHandlesCount; i++) {
					if (this.SoundHandles[i].Stopped) {
						this.SoundHandles[i].Stop();
						this.SoundHandles[i] = this.SoundHandles[this.SoundHandlesCount - 1];
						this.SoundHandlesCount--;
						i--;
					} else {
						this.SoundHandles[i].Source.Pitch = Math.Max(0.01, this.SoundHandles[i].Pitch);
						this.SoundHandles[i].Source.Volume = Math.Max(0.0, this.SoundHandles[i].Volume);
					}
				}
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetReverser(int reverser) {
			try {
				this.Api.SetReverser(reverser);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetPower(int powerNotch) {
			try {
				this.Api.SetPower(powerNotch);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBrake(int brakeNotch) {
			try {
				this.Api.SetBrake(brakeNotch);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void KeyDown(VirtualKeys key) {
			try {
				this.Api.KeyDown(key);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void KeyUp(VirtualKeys key) {
			try {
				this.Api.KeyUp(key);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void HornBlow(HornTypes type) {
			try {
				this.Api.HornBlow(type);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void DoorChange(DoorStates oldState, DoorStates newState) {
			try {
				this.Api.DoorChange(oldState, newState);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetSignal(SignalData[] signal) {
			try {
//				if (this.Train == TrainManager.PlayerTrain) {
//					for (int i = 0; i < signal.Length; i++) {
//						Game.AddDebugMessage(i.ToString() + " - " + signal[i].Aspect.ToString(), 3.0);
//					}
//				}
				this.Api.SetSignal(signal);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBeacon(BeaconData beacon) {
//			if (this.Train == TrainManager.PlayerTrain) {
//				Game.AddDebugMessage("beacon signal aspect " + beacon.Signal.Aspect.ToString(), 3.0);
//				Game.AddDebugMessage("Beacon, type=" + beacon.Type.ToString() + ", data=" + beacon.Optional.ToString(), 3.0);
//			}
			try {
				this.Api.SetBeacon(beacon);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void PerformAI(AIData data) {
			try {
				this.Api.PerformAI(data);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal SoundHandleEx PlaySound(int index, double volume, double pitch, bool looped) {
			if (index >= 0 && index < this.Train.Cars[this.Train.DriverCar].Sounds.Plugin.Length && this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer != null) {
				Sounds.SoundBuffer buffer = this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Buffer;
				OpenBveApi.Math.Vector3 position = this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Position.GetAPIStructure();
				const double power = 0.001; // TODO
				Sounds.SoundSource source = Sounds.PlaySound(buffer, power, pitch, volume, position, this.Train, this.Train.DriverCar, looped);
				if (this.SoundHandlesCount == this.SoundHandles.Length) {
					Array.Resize<SoundHandleEx>(ref this.SoundHandles, this.SoundHandles.Length << 1);
				}
				this.SoundHandles[this.SoundHandlesCount] = new SoundHandleEx(volume, pitch, source);
				this.SoundHandlesCount++;
				return this.SoundHandles[this.SoundHandlesCount - 1];
			} else {
				return null;
			}
		}
	}
	
}