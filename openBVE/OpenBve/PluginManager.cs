using System;
using System.Reflection;
using OpenBveApi.Runtime;

namespace OpenBve {
	internal static partial class PluginManager {
		
		/// <summary>Represents an abstract plugin.</summary>
		internal abstract class Plugin {
			// members
			/// <summary>The file title of the plugin, including the file extension.</summary>
			internal string PluginTitle;
			/// <summary>Whether the plugin returned valid information in the last Elapse call.</summary>
			internal bool PluginValid;
			/// <summary>The debug message the plugin returned in the last Elapse call.</summary>
			internal string PluginMessage;
			/// <summary>The train the plugin is attached to.</summary>
			internal TrainManager.Train Train;
			/// <summary>The array of panel variables.</summary>
			internal int[] Panel;
			/// <summary>Whether the plugin supports the AI.</summary>
			internal bool SupportsAI;
			/// <summary>The last in-game time reported to the plugin.</summary>
			internal double LastTime;
			/// <summary>The last reverser reported to the plugin.</summary>
			internal int LastReverser;
			/// <summary>The last power notch reported to the plugin.</summary>
			internal int LastPowerNotch;
			/// <summary>The last brake notch reported to the plugin.</summary>
			internal int LastBrakeNotch;
			/// <summary>The last aspects per relative section reported to the plugin. Section 0 is the current section, section 1 the upcoming section, and so on.</summary>
			internal int[] LastAspects;
			/// <summary>The absolute section the train was last.</summary>
			internal int LastSection;
			/// <summary>The last exception the plugin raised.</summary>
			internal Exception LastException;
			// functions
			/// <summary>Called to load and initialize the plugin.</summary>
			/// <param name="specs">The train specifications.</param>
			/// <param name="mode">The initialization mode of the train.</param>
			/// <returns>Whether loading the plugin was successful.</returns>
			internal abstract bool Load(VehicleSpecs specs, InitializationModes mode);
			/// <summary>Called to unload the plugin.</summary>
			internal abstract void Unload();
			/// <summary>Called before the train jumps to a different location.</summary>
			/// <param name="mode">The initialization mode of the train.</param>
			internal abstract void BeginJump(InitializationModes mode);
			/// <summary>Called when the train has finished jumping to a different location.</summary>
			internal abstract void EndJump();
			/// <summary>Called every frame to update the plugin.</summary>
			internal void UpdatePlugin() {
				/*
				 * Prepare the vehicle state.
				 * */
				double location = this.Train.Cars[0].FrontAxle.Follower.TrackPosition - this.Train.Cars[0].FrontAxlePosition + 0.5 * this.Train.Cars[0].Length;
				double speed = this.Train.Cars[this.Train.DriverCar].Specs.CurrentPerceivedSpeed;
				double bcPressure = this.Train.Cars[this.Train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure;
				double mrPressure = this.Train.Cars[this.Train.DriverCar].Specs.AirBrake.MainReservoirCurrentPressure;
				double erPressure = this.Train.Cars[this.Train.DriverCar].Specs.AirBrake.EqualizingReservoirCurrentPressure;
				double bpPressure = this.Train.Cars[this.Train.DriverCar].Specs.AirBrake.BrakePipeCurrentPressure;
				double sapPressure = this.Train.Cars[this.Train.DriverCar].Specs.AirBrake.StraightAirPipeCurrentPressure;
				VehicleState vehicle = new VehicleState(location, new Speed(speed), bcPressure, mrPressure, erPressure, bpPressure, sapPressure);
				/*
				 * Prepare the preceding vehicle state.
				 * */
				double bestLocation = double.MaxValue;
				double bestSpeed = 0.0;
				for (int i = 0; i < TrainManager.Trains.Length; i++) {
					if (TrainManager.Trains[i] != this.Train) {
						int c = TrainManager.Trains[i].Cars.Length - 1;
						double z = TrainManager.Trains[i].Cars[c].RearAxle.Follower.TrackPosition - TrainManager.Trains[i].Cars[c].RearAxlePosition - 0.5 * TrainManager.Trains[i].Cars[c].Length;
						if (z >= location & z < bestLocation) {
							bestLocation = z;
							bestSpeed = TrainManager.Trains[i].Specs.CurrentAverageSpeed;
						}
					}
				}
				PrecedingVehicleState precedingVehicle;
				if (bestLocation != double.MaxValue) {
					precedingVehicle = new PrecedingVehicleState(bestLocation, bestLocation - location, new Speed(bestSpeed));
				} else {
					precedingVehicle = null;
				}
				/*
				 * Get the driver handles.
				 * */
				Handles handles = GetHandles();
				/*
				 * Update the plugin.
				 * */
				double totalTime = Game.SecondsSinceMidnight;
				double elapsedTime = Game.SecondsSinceMidnight - LastTime;
				ElapseData data = new ElapseData(vehicle, precedingVehicle, handles, new Time(totalTime), new Time(elapsedTime));
				LastTime = Game.SecondsSinceMidnight;
				Elapse(data);
				this.PluginMessage = data.DebugMessage;
				/*
				 * Set the virtual handles.
				 * */
				this.PluginValid = true;
				SetHandles(data.Handles, true);
			}
			/// <summary>Gets the driver handles.</summary>
			/// <returns>The driver handles.</returns>
			private Handles GetHandles() {
				int reverser = this.Train.Specs.CurrentReverser.Driver;
				int powerNotch = this.Train.Specs.CurrentPowerNotch.Driver;
				int brakeNotch;
				if (this.Train.Cars[this.Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? 3 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? this.Train.Specs.MaximumBrakeNotch + 2 : this.Train.Specs.CurrentBrakeNotch.Driver > 0 ? this.Train.Specs.CurrentBrakeNotch.Driver + 1 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? this.Train.Specs.MaximumBrakeNotch + 1 : this.Train.Specs.CurrentBrakeNotch.Driver;
					}
				}
				bool constSpeed = this.Train.Specs.CurrentConstSpeed;
				return new Handles(reverser, powerNotch, brakeNotch, constSpeed);
			}
			/// <summary>Sets the driver handles or the virtual handles.</summary>
			/// <param name="handles">The handles.</param>
			/// <param name="virtualHandles">Whether to set the virtual handles.</param>
			private void SetHandles(Handles handles, bool virtualHandles) {
				/*
				 * Process the handles.
				 */
				if (this.Train.Specs.SingleHandle & handles.BrakeNotch != 0) {
					handles.PowerNotch = 0;
				}
				/*
				 * Process the reverser.
				 */
				if (handles.Reverser >= -1 & handles.Reverser <= 1) {
					if (virtualHandles) {
						this.Train.Specs.CurrentReverser.Actual = handles.Reverser;
					} else {
						TrainManager.ApplyReverser(this.Train, handles.Reverser, false);
					}
				} else {
					if (virtualHandles) {
						this.Train.Specs.CurrentReverser.Actual = this.Train.Specs.CurrentReverser.Driver;
					}
					this.PluginValid = false;
				}
				/*
				 * Process the power.
				 * */
				if (handles.PowerNotch >= 0 & handles.PowerNotch <= this.Train.Specs.MaximumPowerNotch) {
					if (virtualHandles) {
						this.Train.Specs.CurrentPowerNotch.Safety = handles.PowerNotch;
					} else {
						TrainManager.ApplyNotch(this.Train, handles.PowerNotch, false, 0, true);
					}
				} else {
					if (virtualHandles) {
						this.Train.Specs.CurrentPowerNotch.Safety = this.Train.Specs.CurrentPowerNotch.Driver;
					}
					this.PluginValid = false;
				}
				if (handles.BrakeNotch != 0) {
					if (virtualHandles) {
						this.Train.Specs.CurrentPowerNotch.Safety = 0;
					}
				}
				/*
				 * Process the brakes.
				 * */
				if (virtualHandles) {
					this.Train.Specs.CurrentEmergencyBrake.Safety = false;
					this.Train.Specs.CurrentHoldBrake.Actual = false;
				}
				if (this.Train.Cars[this.Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (handles.BrakeNotch == 0) {
						if (virtualHandles) {
							this.Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Release;
						} else {
							TrainManager.UnapplyEmergencyBrake(this.Train);
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 1) {
						if (virtualHandles) {
							this.Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Lap;
						} else {
							TrainManager.UnapplyEmergencyBrake(this.Train);
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Lap);
						}
					} else if (handles.BrakeNotch == 2) {
						if (virtualHandles) {
							this.Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
						} else {
							TrainManager.UnapplyEmergencyBrake(this.Train);
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Release);
						}
					} else if (handles.BrakeNotch == 3) {
						if (virtualHandles) {
							this.Train.Specs.AirBrake.Handle.Safety = TrainManager.AirBrakeHandleState.Service;
							this.Train.Specs.CurrentEmergencyBrake.Safety = true;
						} else {
							TrainManager.ApplyAirBrakeHandle(this.Train, TrainManager.AirBrakeHandleState.Service);
							TrainManager.ApplyEmergencyBrake(this.Train);
						}
					} else {
						this.PluginValid = false;
					}
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						if (handles.BrakeNotch == this.Train.Specs.MaximumBrakeNotch + 2) {
							if (virtualHandles) {
								this.Train.Specs.CurrentEmergencyBrake.Safety = true;
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.MaximumBrakeNotch;
							} else {
								TrainManager.ApplyHoldBrake(this.Train, false);
								TrainManager.ApplyNotch(this.Train, 0, true, this.Train.Specs.MaximumBrakeNotch, false);
								TrainManager.ApplyEmergencyBrake(this.Train);
							}
						} else if (handles.BrakeNotch >= 2 & handles.BrakeNotch <= this.Train.Specs.MaximumBrakeNotch + 1) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch - 1;
							} else {
								TrainManager.UnapplyEmergencyBrake(this.Train);
								TrainManager.ApplyHoldBrake(this.Train, false);
								TrainManager.ApplyNotch(this.Train, 0, true, handles.BrakeNotch - 1, false);
							}
						} else if (handles.BrakeNotch == 1) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = 0;
								this.Train.Specs.CurrentHoldBrake.Actual = true;
							} else {
								TrainManager.UnapplyEmergencyBrake(this.Train);
								TrainManager.ApplyNotch(this.Train, 0, true, 0, false);
								TrainManager.ApplyHoldBrake(this.Train, true);
							}
						} else if (handles.BrakeNotch == 0) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = 0;
							} else {
								TrainManager.UnapplyEmergencyBrake(this.Train);
								TrainManager.ApplyNotch(this.Train, 0, true, 0, false);
								TrainManager.ApplyHoldBrake(this.Train, false);
							}
						} else {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.CurrentBrakeNotch.Driver;
							}
							this.PluginValid = false;
						}
					} else {
						if (handles.BrakeNotch == this.Train.Specs.MaximumBrakeNotch + 1) {
							if (virtualHandles) {
								this.Train.Specs.CurrentEmergencyBrake.Safety = true;
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.MaximumBrakeNotch;
							} else {
								TrainManager.ApplyHoldBrake(this.Train, false);
								TrainManager.ApplyEmergencyBrake(this.Train);
							}
						} else if (handles.BrakeNotch >= 0 & handles.BrakeNotch <= this.Train.Specs.MaximumBrakeNotch | this.Train.Specs.CurrentBrakeNotch.DelayedChanges.Length == 0) {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = handles.BrakeNotch;
							} else {
								TrainManager.UnapplyEmergencyBrake(this.Train);
								TrainManager.ApplyNotch(this.Train, 0, true, handles.BrakeNotch, false);
							}
						} else {
							if (virtualHandles) {
								this.Train.Specs.CurrentBrakeNotch.Safety = this.Train.Specs.CurrentBrakeNotch.Driver;
							}
							this.PluginValid = false;
						}
					}
				}
				/*
				 * Process the const speed system.
				 * */
				this.Train.Specs.CurrentConstSpeed = handles.ConstSpeed & this.Train.Specs.HasConstSpeed;
			}
			/// <summary>Called every frame to update the plugin.</summary>
			/// <param name="data">The data passed to the plugin on Elapse.</param>
			/// <remarks>This function should not be called directly. Call UpdatePlugin instead.</remarks>
			internal abstract void Elapse(ElapseData data);
			/// <summary>Called to update the reverser. This invokes a call to SetReverser only if a change actually occured.</summary>
			internal void UpdateReverser() {
				int reverser = this.Train.Specs.CurrentReverser.Driver;
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
			internal void UpdatePower() {
				int powerNotch = this.Train.Specs.CurrentPowerNotch.Driver;
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
			internal void UpdateBrake() {
				int brakeNotch;
				if (this.Train.Cars[this.Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? 4 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 3 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 2 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? 3 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? 2 : this.Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? 1 : 0;
					}
				} else {
					if (this.Train.Specs.HasHoldBrake) {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? this.Train.Specs.MaximumBrakeNotch + 2 : this.Train.Specs.CurrentBrakeNotch.Driver > 0 ? this.Train.Specs.CurrentBrakeNotch.Driver + 1 : this.Train.Specs.CurrentHoldBrake.Driver ? 1 : 0;
					} else {
						brakeNotch = this.Train.Specs.CurrentEmergencyBrake.Driver ? this.Train.Specs.MaximumBrakeNotch + 1 : this.Train.Specs.CurrentBrakeNotch.Driver;
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
			/// <summary>Called to update the aspects of the section. This invokes a call to SetSignal only if a change in aspect occured or when changing section boundaries.</summary>
			/// <param name="data">The sections to submit to the plugin.</param>
			internal void UpdateSignals(SignalData[] data) {
				if (data.Length != 0) {
					bool update;
					if (this.Train.CurrentSectionIndex != this.LastSection) {
						update = true;
					} else if (data.Length != this.LastAspects.Length) {
						update = true;
					} else {
						update = false;
						for (int i = 0; i < data.Length; i++) {
							if (data[i].Aspect != this.LastAspects[i]) {
								update = true;
								break;
							}
						}
					}
					if (update) {
						SetSignal(data);
						this.LastAspects = new int[data.Length];
						for (int i = 0; i < data.Length; i++) {
							//Game.AddMessage("SECTION " + i.ToString() + ", ASPECT " + data[i].Aspect.ToString() + ", DISTANCE " + data[i].Distance.ToString("0"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Magenta, Game.SecondsSinceMidnight + 2.5);
							this.LastAspects[i] = data[i].Aspect;
						}
						//Game.AddMessage("TRAIN SECTION INDEX " + Train.CurrentSectionIndex.ToString(), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Magenta, Game.SecondsSinceMidnight + 2.5);
					}
				}
			}
			/// <summary>Is called when the aspect in the current or any of the upcoming sections changes.</summary>
			/// <param name="data">Signal information per section. In the array, index 0 is the current section, index 1 the upcoming section, and so on.</param>
			/// <remarks>This function should not be called directly. Call UpdateSignal instead.</remarks>
			internal abstract void SetSignal(SignalData[] signal);
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="type">The beacon type.</param>
			/// <param name="sectionIndex">The section the beacon is attached to, or -1 if none, or TrackManager.TransponderSpecialSection.NextRedSection.</param>
			/// <param name="optional">Optional data attached to the beacon.</param>
			internal void UpdateBeacon(int type, int sectionIndex, int optional) {
				int aspect = 255;
				double distance = double.MaxValue;
				if (sectionIndex == (int)TrackManager.TransponderSpecialSection.NextRedSection) {
					sectionIndex = this.Train.CurrentSectionIndex;
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
					if (Game.Sections[sectionIndex].Exists(this.Train)) {
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
					double position = this.Train.Cars[0].FrontAxle.Follower.TrackPosition - this.Train.Cars[0].FrontAxlePosition + 0.5 * this.Train.Cars[0].Length;
					distance = Game.Sections[sectionIndex].TrackPosition - position;
				}
				SetBeacon(new BeaconData(type, optional, new SignalData(aspect, distance)));
			}
			/// <summary>Called when the train passes a beacon.</summary>
			/// <param name="data">The beacon data.</param>
			/// <remarks>This function should not be called directly. Call UpdateBeacon instead.</remarks>
			internal abstract void SetBeacon(BeaconData beacon);
			/// <summary>Updates the AI.</summary>
			/// <returns>The AI response.</returns>
			internal AIResponse UpdateAI() {
				if (this.SupportsAI) {
					AIData data = new AIData(GetHandles());
					this.PerformAI(data);
					if (data.Response != AIResponse.None) {
						SetHandles(data.Handles, false);
					}
					return data.Response;
				} else {
					return AIResponse.None;
				}
			}
			/// <summary>Called when the AI should be performed.</summary>
			/// <param name="data">The AI data.</param>
			/// <remarks>This function should not be called directly. Call UpdateAI instead.</remarks>
			internal abstract void PerformAI(AIData data);
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
			bool hasHoldBrake;
			if (brakeType == BrakeTypes.AutomaticAirBrake) {
				brakeNotches = 2;
				powerNotches = train.Specs.MaximumPowerNotch;
				hasHoldBrake = false;
			} else {
				brakeNotches = train.Specs.MaximumBrakeNotch + (train.Specs.HasHoldBrake ? 1 : 0);
				powerNotches = train.Specs.MaximumPowerNotch;
				hasHoldBrake = train.Specs.HasHoldBrake;
			}
			int cars = train.Cars.Length;
			VehicleSpecs specs = new VehicleSpecs(powerNotches, brakeType, brakeNotches, hasHoldBrake, cars);
			InitializationModes mode = (InitializationModes)Game.TrainStart;
			/*
			 * Check if the plugin is a .NET plugin.
			 * */
			Assembly assembly;
			try {
				assembly = Assembly.LoadFile(pluginFile);
			} catch {
				assembly = null;
			}
			if (assembly != null) {
				Type[] types;
				try {
					types = assembly.GetTypes();
				} catch (ReflectionTypeLoadException ex) {
					foreach (Exception e in ex.LoaderExceptions) {
						Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " raised an exception on loading: " + e.Message);
					}
					return false;
				}
				foreach (Type type in types) {
					if (type.IsPublic && (type.Attributes & TypeAttributes.Abstract) == 0) {
						object instance = assembly.CreateInstance(type.FullName);
						IRuntime api = instance as IRuntime;
						if (api != null) {
							CurrentPlugin = new NetPlugin(pluginFile, trainFolder, api, train);
							if (CurrentPlugin.Load(specs, mode)) {
								train.Specs.Safety.Mode = TrainManager.SafetySystem.Plugin;
								return true;
							} else {
								CurrentPlugin = null;
								return false;
							}
						}
					}
				}
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " does not export a internal type that inherits from OpenBveApi.Runtime.IPlugin and therefore cannot be used with openBVE.");
				return false;
			}
			/*
			 * Check if the plugin is a Win32 plugin.
			 * */
			if (!CheckWin32Header(pluginFile)) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The train plugin " + pluginTitle + " is of an unsupported binary format and therefore cannot be used with openBVE.");
				return false;
			}
			if (Program.CurrentPlatform != Program.Platform.Windows | IntPtr.Size != 4) {
				Interface.AddMessage(Interface.MessageType.Warning, false, "The train plugin " + pluginTitle + " can only be used on 32-bit Microsoft Windows.");
				return false;
			}
			CurrentPlugin = new LegacyPlugin(pluginFile, train);
			if (CurrentPlugin.Load(specs, mode)) {
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