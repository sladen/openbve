using System;
using OpenBveApi.Runtime;

namespace OpenBve {
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : PluginManager.Plugin {
		
		// sound handle
		internal class SoundHandleEx : SoundHandle {
			internal int SoundSourceIndex;
			internal SoundHandleEx(double volume, double pitch, int soundSourceIndex) {
				base.MyVolume = volume;
				base.MyPitch = pitch;
				base.MyValid = true;
				this.SoundSourceIndex = soundSourceIndex;
			}
		}
		
		// --- members ---
		private string PluginFolder;
		private string TrainFolder;
		private IRuntime Api;
		private SoundHandleEx[] Sounds;
		private int SoundCount;
		
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
			this.Sounds = new SoundHandleEx[16];
			this.SoundCount = 0;
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
				/* 
				 * Process the sounds.
				 * */
				for (int i = 0; i < this.SoundCount; i++) {
					if (this.Sounds[i].Stopped || !SoundManager.IsPlaying(this.Sounds[i].SoundSourceIndex)) {
						SoundManager.StopSound(ref this.Sounds[i].SoundSourceIndex);
						this.Sounds[i].Stop();
						this.Sounds[i] = this.Sounds[this.SoundCount - 1];
						this.SoundCount--;
						i--;
					} else {
						double pitch = Math.Max(0.01, this.Sounds[i].Pitch);
						double volume = Math.Max(0.0, this.Sounds[i].Volume);
						SoundManager.ModulateSound(this.Sounds[i].SoundSourceIndex, pitch, volume);
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
				this.Api.SetSignal(signal);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBeacon(BeaconData beacon) {
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
			if (index >= 0 && index < this.Train.Cars[this.Train.DriverCar].Sounds.Plugin.Length && this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].SoundBufferIndex >= 0) {
				int soundSourceIndex = -1;
				SoundManager.PlaySound(ref soundSourceIndex, this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].SoundBufferIndex, base.Train, base.Train.DriverCar, this.Train.Cars[this.Train.DriverCar].Sounds.Plugin[index].Position, SoundManager.Importance.DontCare, looped, pitch, volume);
				if (this.SoundCount == this.Sounds.Length) {
					Array.Resize<SoundHandleEx>(ref this.Sounds, this.Sounds.Length << 1);
				}
				this.Sounds[this.SoundCount] = new SoundHandleEx(volume, pitch, soundSourceIndex);
				this.SoundCount++;
				return this.Sounds[this.SoundCount - 1];
			} else {
				return null;
			}
		}
	}
	
}