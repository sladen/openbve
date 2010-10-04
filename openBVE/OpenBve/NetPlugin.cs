using System;
using OpenBveApi.Runtime;

namespace OpenBve {
	/// <summary>Represents a .NET assembly plugin.</summary>
	internal class NetPlugin : PluginManager.Plugin {
		
		// --- members ---
		private string PluginFile;
		private string TrainFolder;
		private IRuntime Api;
		
		// --- constructors ---
		internal NetPlugin(string pluginFile, string trainFolder, IRuntime api) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Panel = null;
			base.Sound = null;
			base.LastTime = 0.0;
			base.LastSound = null;
			base.LastReverser = -2;
			base.LastPowerNotch = -1;
			base.LastBrakeNotch = -1;
			base.LastAspect = -1;
			base.LastException = null;
			this.PluginFile = pluginFile;
			this.TrainFolder = trainFolder;
			this.Api = api;
		}
		
		// --- functions ---
		internal override bool Load(TrainManager.Train train, VehicleSpecs specs, InitializationModes mode) {
			LoadProperties properties = new LoadProperties(this.PluginFile, this.TrainFolder, null);
			bool success;
			try {
				success = this.Api.Load(properties);
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
			if (success) {
				base.Panel = properties.Panel;
				base.Sound = properties.Sound;
				if (base.Sound != null) {
					base.LastSound = new int[base.Sound.Length];
					for (int i = 0; i < base.Sound.Length; i++) {
						base.LastSound[i] = base.Sound[i];
					}
				} else {
					base.LastSound = null;
				}
				try {
					Api.SetVehicleSpecs(specs);
					Api.Initialize(mode);
				} catch (Exception ex) {
					base.LastException = ex;
					throw;
				}
				UpdatePower(train);
				UpdateBrake(train);
				UpdateReverser(train);
				return true;
			} else if (properties.Reason != null) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + base.PluginTitle + " failed to load for the following reason: " + properties.Reason);
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
		internal override void EndJump() {
		}
		internal override void Elapse(VehicleState state, Handles handles, out string message) {
			try {
				this.Api.Elapse(state, handles, this.Panel, this.Sound, out message);
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
		internal override void SetSignal(SignalData signal) {
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
	}
	
}