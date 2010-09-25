using System;
using OpenBveApi;

namespace OpenBve {
	internal class NetPlugin : PluginManager.Plugin {
		
		// --- members ---
		private string PluginFile;
		private string TrainFolder;
		private IPlugin Api;
		
		// --- constructors ---
		internal NetPlugin(string pluginFile, string trainFolder, IPlugin api) {
			base.PluginTitle = System.IO.Path.GetFileName(pluginFile);
			base.PluginValid = true;
			base.PluginMessage = null;
			base.Panel = new int[] { };
			base.Sound = new int[] { };
			base.LastTime = 0.0;
			base.LastSound = new int[] { };
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
				base.LastSound = new int[base.Sound.Length];
				for (int i = 0; i < base.Sound.Length; i++) {
					base.LastSound[i] = base.Sound[i];
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
				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + base.PluginTitle + " failed to load for the following reason: " + properties.Reason);
				return false;
			} else {
				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + base.PluginTitle + " failed to load for an unspecified reason.");
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
			this.Api.Elapse(state, handles, out message);
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
		internal override void SetSignal(int aspect) {
			try {
				this.Api.SetSignal(new SignalData(aspect));
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
		internal override void SetBeacon(int type, int aspect, double distance, int data) {
			try {
				this.Api.SetBeacon(new BeaconData(type, aspect, distance, data));
			} catch (Exception ex) {
				base.LastException = ex;
				throw;
			}
		}
	}
	
}