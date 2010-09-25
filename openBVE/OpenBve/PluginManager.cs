using System;
using System.Reflection;
using OpenBveApi;

namespace OpenBve {
	internal static partial class PluginManager {
		
		internal abstract class Plugin {
			// members
			internal string PluginTitle;
			internal bool PluginValid;
			internal string PluginMessage;
			internal int[] Panel;
			internal int[] Sound;
			internal double LastTime;
			internal int[] LastSound;
			internal int LastReverser;
			internal int LastPowerNotch;
			internal int LastBrakeNotch;
			internal int LastAspect;
			internal Exception LastException;
			// functions
			/// <summary>Called to load and initialize the plugin.</summary>
			/// <param name="train">The train.</param>
			/// <param name="specs">The train specifications.</param>
			/// <param name="mode">The initialization mode of the train.</param>
			/// <returns>Whether loading the plugin was successful.</returns>
			internal abstract bool Load(TrainManager.Train train, VehicleSpecs specs, InitializationModes mode);
			/// <summary>Called to unload the plugin.</summary>
			internal abstract void Unload();
			/// <summary>Called before the train jumps to a different location.</summary>
			/// <param name="mode">The initialization mode of the train.</param>
			internal abstract void BeginJump(InitializationModes mode);
			/// <summary>Called when the train has finished jumping to a different location.</summary>
			internal abstract void EndJump();
			/// <summary>Called every frame to update the plugin.</summary>
			/// <param name="train">The train.</param>
			internal void UpdatePlugin(TrainManager.Train train) {
				/*
				 * Prepare the vehicle state.
				 * */
				double location = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxlePosition + 0.5 * train.Cars[0].Length;
				double speed = train.Cars[train.DriverCar].Specs.CurrentPerceivedSpeed;
				double time = Game.SecondsSinceMidnight;
				double elapsedTime = Game.SecondsSinceMidnight - LastTime;
				double bcPressure = train.Cars[train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure;
				double mrPressure = train.Cars[train.DriverCar].Specs.AirBrake.MainReservoirCurrentPressure;
				double erPressure = train.Cars[train.DriverCar].Specs.AirBrake.EqualizingReservoirCurrentPressure;
				double bpPressure = train.Cars[train.DriverCar].Specs.AirBrake.BrakePipeCurrentPressure;
				double sapPressure = train.Cars[train.DriverCar].Specs.AirBrake.StraightAirPipeCurrentPressure;
				VehicleState state = new VehicleState(location, speed, time, elapsedTime, bcPressure, mrPressure, erPressure, bpPressure, sapPressure);
				LastTime = Game.SecondsSinceMidnight;
				/*
				 * Prepare the handles.
				 * */
				int reverser = train.Specs.CurrentReverser.Driver;
				int powerNotch = train.Specs.CurrentPowerNotch.Driver;
				int brakeNotch;
				if (train.Cars[train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? 3 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				} else {
					if (train.Specs.HasHoldBrake) {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? train.Specs.MaximumBrakeNotch + 2 : train.Specs.CurrentBrakeNotch.Driver > 0 ? train.Specs.CurrentBrakeNotch.Driver + 1 : train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? train.Specs.MaximumBrakeNotch + 1 : train.Specs.CurrentBrakeNotch.Driver;
					}
				}
				ConstSpeedInstructions constSpeed = ConstSpeedInstructions.Continue;
				Handles handles = new Handles(reverser, powerNotch, brakeNotch, constSpeed);
				/*
				 * Update the plugin.
				 * */
				Elapse(state, handles, out this.PluginMessage);
				this.PluginValid = true;
				/*
				 * Process the handles.
				 */
				if (train.Specs.SingleHandle & handles.BrakeNotch != 0) {
					handles.PowerNotch = 0;
				}
				/*
				 * Process the reverser.
				 */
				if (handles.Reverser >= -1 & handles.Reverser <= 1) {
					train.Specs.CurrentReverser.Actual = handles.Reverser;
				} else {
					train.Specs.CurrentReverser.Actual = train.Specs.CurrentReverser.Driver;
					this.PluginValid = false;
				}
				/*
				 * Process the power.
				 * */
				if (handles.PowerNotch >= 0 & handles.PowerNotch <= train.Specs.MaximumPowerNotch) {
					train.Specs.CurrentPowerNotch.Safety = handles.PowerNotch;
				} else {
					train.Specs.CurrentPowerNotch.Safety = train.Specs.CurrentPowerNotch.Driver;
					this.PluginValid = false;
				}
				if (handles.BrakeNotch != 0) {
					train.Specs.CurrentPowerNotch.Safety = 0;
				}
				/*
				 * Process the brakes.
				 * */
				train.Specs.CurrentEmergencyBrake.Safety = false;
				train.Specs.CurrentHoldBrake.Actual = false;
				if (train.Cars[train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (handles.BrakeNotch == 0) {
						train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Release;
					} else if (handles.BrakeNotch == 1) {
						train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Lap;
					} else if (handles.BrakeNotch == 2) {
						train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
					} else if (handles.BrakeNotch == 3) {
						train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
						train.Specs.CurrentEmergencyBrake.Safety = true;
					} else {
						this.PluginValid = false;
					}
				} else {
					if (train.Specs.HasHoldBrake) {
						if (handles.BrakeNotch == train.Specs.MaximumBrakeNotch + 2) {
							train.Specs.CurrentEmergencyBrake.Safety = true;
							train.Specs.CurrentBrakeNotch.Safety = train.Specs.MaximumBrakeNotch;
						} else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= train.Specs.MaximumBrakeNotch + 1) {
							train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch - 1;
						} else if (handles.BrakeNotch == 1) {
							train.Specs.CurrentBrakeNotch.Safety = 0;
							train.Specs.CurrentHoldBrake.Actual = true;
						} else if (handles.BrakeNotch == 0) {
							train.Specs.CurrentBrakeNotch.Safety = 0;
						} else {
							train.Specs.CurrentBrakeNotch.Safety = train.Specs.CurrentBrakeNotch.Driver;
							this.PluginValid = false;
						}
					} else {
						if (handles.BrakeNotch == train.Specs.MaximumBrakeNotch + 1) {
							train.Specs.CurrentEmergencyBrake.Safety = true;
							train.Specs.CurrentBrakeNotch.Safety = train.Specs.MaximumBrakeNotch;
						} else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= train.Specs.MaximumBrakeNotch | train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0) {
							train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch;
						} else {
							train.Specs.CurrentBrakeNotch.Safety = train.Specs.CurrentBrakeNotch.Driver;
							this.PluginValid = false;
						}
					}
				}
				/*
				 * Process the const speed system.
				 * */
				if (handles.ConstSpeed == ConstSpeedInstructions.Enable) {
					train.Specs.CurrentConstSpeed = train.Specs.HasConstSpeed;
				} else if (handles.ConstSpeed == ConstSpeedInstructions.Disable) {
					train.Specs.CurrentConstSpeed = false;
				} else if (handles.ConstSpeed != ConstSpeedInstructions.Continue) {
					this.PluginValid = false;
				}
				/*
				 * Process sound instructions.
				 * */
				for (int i = 0; i < this.Sound.Length; i++) {
					if (this.Sound[i] != this.LastSound[i]) {
						if (this.Sound[i] == SoundInstructions.Stop) {
							if (i < train.Cars[train.DriverCar].Sounds.Plugin.Length) {
								SoundManager.StopSound(ref train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex);
							}
						} else if (this.Sound[i] > SoundInstructions.Stop & this.Sound[i] <= SoundInstructions.PlayLooping) {
							if (i < train.Cars[train.DriverCar].Sounds.Plugin.Length) {
								int snd = train.Cars[train.DriverCar].Sounds.Plugin[i].SoundBufferIndex;
								if (snd >= 0) {
									double gain = (double)(this.Sound[i] - SoundInstructions.Stop) / (double)(SoundInstructions.PlayLooping - SoundInstructions.Stop);
									if (SoundManager.IsPlaying(train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex)) {
										SoundManager.ModulateSound(train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, 1.0, gain);
									} else {
										SoundManager.PlaySound(ref train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, snd, train, train.DriverCar, train.Cars[train.DriverCar].Sounds.Plugin[i].Position, SoundManager.Importance.AlwaysPlay, true, 1.0, gain);
									}
								}
							}
						} else if (this.Sound[i] == SoundInstructions.PlayOnce) {
							if (i < train.Cars[train.DriverCar].Sounds.Plugin.Length) {
								int snd = train.Cars[train.DriverCar].Sounds.Plugin[i].SoundBufferIndex;
								if (snd >= 0) {
									SoundManager.PlaySound(ref train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex, snd, train, train.DriverCar, train.Cars[train.DriverCar].Sounds.Plugin[i].Position, SoundManager.Importance.AlwaysPlay, false);
								}
							}
							this.Sound[i] = SoundInstructions.Continue;
						} else if (this.Sound[i] != SoundInstructions.Continue) {
							PluginValid = false;
						}
						this.LastSound[i] = this.Sound[i];
					} else {
						if (i < train.Cars[train.DriverCar].Sounds.Plugin.Length) {
							if (train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex >= 0) {
								if (SoundManager.HasFinishedPlaying(train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex)) {
									SoundManager.StopSound(ref train.Cars[train.DriverCar].Sounds.Plugin[i].SoundSourceIndex);
								}
							}
						}
						if ((this.Sound[i] < SoundInstructions.Stop | this.Sound[i] > SoundInstructions.PlayLooping) && this.Sound[i] != SoundInstructions.PlayOnce & this.Sound[i] != SoundInstructions.Continue) {
							PluginValid = false;
						}
					}
				}
			}
			/// <summary>Called every frame to update the plugin.</summary>
			/// <param name="state">The current state of the train.</param>
			/// <param name="handles">The handles of the cab.</param>
			/// <param name="message">The message the plugin passes to the host application for debugging purposes, or a null reference.</param>
			/// <remarks>This function should not be called directly. Call UpdatePlugin instead.</remarks>
			internal abstract void Elapse(VehicleState state, Handles handles, out string message);
			/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occured.</summary>
			/// <param name="train">The train.</param>
			internal void UpdateReverser(TrainManager.Train train) {
				int reverser = train.Specs.CurrentReverser.Driver;
				if (reverser != this.LastReverser) {
					this.LastReverser = reverser;
					SetReverser(reverser);
				}
			}
			/// <summary>Called to indicate a change of the reverser.</summary>
			/// <param name="reverser">The reverser.</param>
			/// <remarks>This function should not be called directly. Call UpdateReverser instead.</remarks>
			internal abstract void SetReverser(int reverser);
			/// <summary>Called to update the power notch. This invokes a call to SetPower only if a change actually occured.</summary>
			/// <param name="train">The train.</param>
			internal void UpdatePower(TrainManager.Train train) {
				int powerNotch = train.Specs.CurrentPowerNotch.Driver;
				if (powerNotch != this.LastPowerNotch) {
					this.LastPowerNotch = powerNotch;
					SetPower(powerNotch);
				}
			}
			/// <summary>Called to indicate a change of the power notch.</summary>
			/// <param name="powerNotch">The power notch.</param>
			/// <remarks>This function should not be called directly. Call UpdatePower instead.</remarks>
			internal abstract void SetPower(int powerNotch);
			/// <summary>Called to update the brake notch. This invokes a call to SetBrake only if a change actually occured.</summary>
			/// <param name="train">The train.</param>
			internal void UpdateBrake(TrainManager.Train train) {
				int brakeNotch;
				if (train.Cars[train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (train.Specs.HasHoldBrake) {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? 4 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 3 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 2 : train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? 3 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
					}
				} else {
					if (train.Specs.HasHoldBrake) {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? train.Specs.MaximumBrakeNotch + 2 : train.Specs.CurrentBrakeNotch.Driver > 0 ? train.Specs.CurrentBrakeNotch.Driver + 1 : train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = train.Specs.CurrentEmergencyBrake.Driver ? train.Specs.MaximumBrakeNotch + 1 : train.Specs.CurrentBrakeNotch.Driver;
					}
				}
				if (brakeNotch != this.LastBrakeNotch) {
					this.LastBrakeNotch = brakeNotch;
					SetBrake(brakeNotch);
				}
			}
			/// <summary>Called to indicate a change of the brake notch.</summary>
			/// <param name="brakeNotch">The brake notch.</param>
			/// <remarks>This function should not be called directly. Call UpdateBrake instead.</remarks>
			internal abstract void SetBrake(int brakeNotch);
			/// <summary>Called when a virtual key is pressed.</summary>
			internal abstract void KeyDown(VirtualKeys key);
			/// <summary>Called when a virtual key is released.</summary>
			internal abstract void KeyUp(VirtualKeys key);
			/// <summary>Called when a horn is played or stopped.</summary>
			internal abstract void HornBlow(HornTypes type);
			/// <summary>Called when the state of the doors changes.</summary>
			internal abstract void DoorChange(DoorStates oldState, DoorStates newState);
			/// <summary>Called to update the aspect of the current section. This invokes a call to SetSignal only if a change actually occured.</summary>
			/// <param name="aspect"></param>
			internal void UpdateSignal(int aspect) {
				if (aspect != this.LastAspect) {
					this.LastAspect = aspect;
					SetSignal(aspect);
				}
			}
			/// <summary>Called when the aspect in the current section changes.</summary>
			/// <param name="aspect">The aspect.</param>
			/// <remarks>This function should not be called directly. Call UpdateSignal instead.</remarks>
			internal abstract void SetSignal(int aspect);
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="train">The train.</param>
			/// <param name="type">The beacon type.</param>
			/// <param name="sectionIndex">The section the beacon is attached to, or -1 if none, or TrackManager.TransponderSpecialSection.NextRedSection.</param>
			/// <param name="data">The additional data attached to the beacon.</param>
			internal void UpdateBeacon(TrainManager.Train train, int type, int sectionIndex, int data) {
				int aspect = 255;
				double distance = double.MaxValue;
				if (sectionIndex == (int)TrackManager.TransponderSpecialSection.NextRedSection) {
					sectionIndex = train.CurrentSectionIndex;
					if (sectionIndex >= 0) {
						sectionIndex = Game.Sections[sectionIndex].NextSection;
						while (sectionIndex >= 0) {
							int a = Game.Sections[sectionIndex].CurrentAspect;
							if (a >= 0) {
								if (Game.Sections[sectionIndex].Aspects[a].Number == 0) {
									break;
								}
							}
							sectionIndex = Game.Sections[sectionIndex].NextSection;
						}
					}
				}
				if (sectionIndex >= 0) {
					if (Game.Sections[sectionIndex].Exists(TrainManager.PlayerTrain)) {
						int n = Game.Sections[sectionIndex].NextSection;
						if (n >= 0) {
							int c = -1;
							int a = Game.Sections[n].CurrentAspect;
							for (int i = Game.Sections[sectionIndex].Aspects.Length - 1; i >= 0; i--) {
								if (Game.Sections[sectionIndex].Aspects[i].Number > Game.Sections[n].Aspects[a].Number) {
									c = i;
								}
							}
							if (c == -1) {
								c = Game.Sections[sectionIndex].Aspects.Length - 1;
							}
							aspect = Game.Sections[sectionIndex].Aspects[c].Number;
						} else {
							aspect = Game.Sections[sectionIndex].Aspects[Game.Sections[sectionIndex].Aspects.Length - 1].Number;
						}
					} else {
						int a = Game.Sections[sectionIndex].CurrentAspect;
						if (a >= 0) {
							aspect = Game.Sections[sectionIndex].Aspects[a].Number;
						}
					}
					double position = train.Cars[0].FrontAxle.Follower.TrackPosition - train.Cars[0].FrontAxlePosition + 0.5 * train.Cars[0].Length;
					distance = Game.Sections[sectionIndex].TrackPosition - position;
				}
				SetBeacon(type, aspect, distance, data);
			}
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="type">The beacon type.</param>
			/// <param name="aspect">The aspect shown in the attached section.</param>
			/// <param name="distance">The distance to the attached section.</param>
			/// <param name="data">The additional data attached to the beacon.</param>
			/// <remarks>This function should not be called directly. Call UpdateBeacon instead.</remarks>
			internal abstract void SetBeacon(int type, int aspect, double distance, int data);
		}
		
		/// <summary>The currently loaded plugin, or a null reference.</summary>
		internal static Plugin CurrentPlugin = null;
		
		/// <summary>Loads the train plugin from the ats.cfg for the specified train.</summary>
		/// <param name="trainFolder">The absolute path to the train folder.</param>
		/// <param name="encoding">The encoding to be used.</param>
		/// <param name="train">The train to attach the plugin to.</param>
		/// <returns>Whether a plugin was loaded successfully.</returns>
		internal static bool LoadPlugin(string trainFolder, System.Text.Encoding encoding, TrainManager.Train train) {
			/*
			 * Unload plugin if already loaded.
			 * */
			if (CurrentPlugin != null) {
				UnloadPlugin();
			}
			/*
			 * Check if the ats.cfg file exists, and if
			 * it points to an existing plugin file.
			 * */
			string config = Interface.GetCombinedFileName(trainFolder, "ats.cfg");
			if (!System.IO.File.Exists(config)) {
				return false;
			}
			string[] lines = System.IO.File.ReadAllLines(config, encoding);
			if (lines.Length == 0) {
				return false;
			}
			string pluginFile = Interface.GetCombinedFileName(trainFolder, lines[0]);
			string pluginTitle = System.IO.Path.GetFileName(pluginFile);
			if (!System.IO.File.Exists(pluginFile)) {
				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " could not be found in " + config);
				return false;
			}
			/*
			 * Prepare initialization data for the plugin.
			 * */
			BrakeTypes brakeType = (BrakeTypes)train.Cars[train.DriverCar].Specs.BrakeType;
			int brakeNotches;
			int powerNotches;
			int atsNotch;
			int b67Notch;
			if (brakeType == BrakeTypes.AutomaticAirBrake) {
				brakeNotches = 2;
				powerNotches = train.Specs.MaximumPowerNotch;
				atsNotch = 1;
				b67Notch = 1;
			} else {
				brakeNotches = train.Specs.MaximumBrakeNotch + (train.Specs.HasHoldBrake ? 1 : 0);
				powerNotches = train.Specs.MaximumPowerNotch;
				atsNotch = train.Specs.HasHoldBrake ? 2 : 1;
				b67Notch = Math.Max(1, (int)Math.Round(0.7 * brakeNotches));
			}
			int cars = train.Cars.Length;
			VehicleSpecs specs = new VehicleSpecs(powerNotches, brakeType, brakeNotches, atsNotch, b67Notch, cars);
			InitializationModes mode = (InitializationModes)Game.TrainStart;
			/*
			 * Check if the plugin is a .NET plugin.
			 * */
//			Assembly assembly;
//			try {
//				assembly = Assembly.LoadFile(pluginFile);
//			} catch {
//				assembly = null;
//			}
//			if (assembly != null) {
//				Type[] types;
//				try {
//					types = assembly.GetTypes();
//				} catch (ReflectionTypeLoadException ex) {
//					foreach (Exception e in ex.LoaderExceptions) {
//						Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " raised an exception on loading: " + e.Message);
//					}
//					return false;
//				}
//				foreach (Type type in types) {
//					if (type.IsPublic && (type.Attributes & TypeAttributes.Abstract) == 0) {
//						object instance = assembly.CreateInstance(type.FullName);
//						OpenBveApi.IPlugin api = instance as OpenBveApi.IPlugin;
//						if (api != null) {
//							CurrentPlugin = new NetPlugin(pluginFile, trainFolder, api);
//							if (CurrentPlugin.Load(train, specs, mode)) {
//								train.Specs.Safety.Mode = TrainManager.SafetySystem.Plugin;
//								return true;
//							} else {
//								CurrentPlugin = null;
//								return false;
//							}
//						}
//					}
//				}
//				Interface.AddMessage(Interface.MessageType.Error, true, "The train plugin " + pluginTitle + " does not export a public type that inherits from OpenBveApi.IPlugin and cannot be used with openBVE.");
//				return false;
//			}
			/*
			 * Check if the plugin is a Win32 plugin.
			 * */
			if (!CheckWin32Header(pluginFile)) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " is of an unsupported binary format and cannot be used with openBVE.");
				return false;
			}
			if (Program.CurrentPlatform != Program.Platform.Windows | IntPtr.Size != 4) {
				Interface.AddMessage(Interface.MessageType.Warning, false, "The train plugin " + pluginTitle + " can only be used on 32-bit Microsoft Windows.");
				return false;
			}
			CurrentPlugin = new LegacyPlugin(pluginFile);
			if (CurrentPlugin.Load(train, specs, mode)) {
				train.Specs.Safety.Mode = TrainManager.SafetySystem.Plugin;
				return true;
			} else {
				CurrentPlugin = null;
				return false;
			}
		}
		
		/// <summary>Checks whether a specified file is a valid Win32 plugin.</summary>
		/// <param name="file">The file to check.</param>
		/// <returns>Whether the file is a valid Win32 plugin.</returns>
		private static bool CheckWin32Header(string file) {
			try {
				using (System.IO.FileStream stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
					using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream)) {
						if (reader.ReadUInt16() != 0x5A4D) {
							/* Not MZ signature */
							return false;
						}
						stream.Position = 0x3C;
						stream.Position = reader.ReadInt32();
						if (reader.ReadUInt32() != 0x00004550) {
							/* Not PE signature */
							return false;
						}
						if (reader.ReadUInt16() != 0x014C) {
							/* Not IMAGE_FILE_MACHINE_I386 */
							return false;
						}
					}
				}
				return true;
			} catch {
				return false;
			}
		}
		
		/// <summary>Unloads the currently loaded plugin, if any.</summary>
		internal static void UnloadPlugin() {
			if (CurrentPlugin != null) {
				CurrentPlugin.Unload();
				CurrentPlugin = null;
			}
		}
		
	}
}