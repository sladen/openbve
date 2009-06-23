using System;
using Tao.OpenGl;
using Tao.Sdl;
using System.Windows.Forms;

namespace OpenBve {
	internal static class MainLoop {

		//// declarations
		internal static bool LimitFramerate = false;
		private static bool Quit = false;
		private static int TimeFactor = 1;

		//// --------------------------------

		//// start loop
		internal static void StartLoop() {
			// timer
			Timers.Initialize();
			// train loop sounds
			for (int i = 0; i < TrainManager.Trains.Length; i++) {
				for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
					if (TrainManager.Trains[i].Cars[j].Specs.IsMotorCar) {
						if (TrainManager.Trains[i].Cars[j].Sounds.Loop.SoundBufferIndex >= 0) {
							SoundManager.PlaySound(TrainManager.Trains[i].Cars[j].Sounds.Loop.SoundBufferIndex, TrainManager.Trains[i], j, TrainManager.Trains[i].Cars[j].Sounds.Loop.Position, SoundManager.Importance.AlwaysPlay, true);
						}
					}
				}
			}
			// framerate display
			double TotalTimeElapsedForInfo = 0.0;
			double TotalTimeElapsedForSectionUpdate = 0.0;
			int TotalFramesElapsed = 0;
			// fast-forward until start time
			{
				Game.MinimalisticSimulation = true;
				const double w = 0.25;
				double u = Game.StartupTime - Game.SecondsSinceMidnight;
				if (u > 0) {
					while (true) {
						double v = u < w ? u : w; u -= v;
						Game.SecondsSinceMidnight += v;
						TrainManager.UpdateTrains(v);
						if (u <= 0.0) break;
						TotalTimeElapsedForSectionUpdate += v;
						if (TotalTimeElapsedForSectionUpdate >= 1.0) {
							if (Game.Sections.Length > 0) {
								Game.UpdateSection(Game.Sections.Length - 1);
							}
							TotalTimeElapsedForSectionUpdate = 0.0;
						}
					}
				}
				Game.MinimalisticSimulation = false;
			}
			// update sections
			int s = TrainManager.PlayerTrain.Cars[0].CurrentSection;
			if (s >= 0) {
				Game.Sections[s].Enter(TrainManager.PlayerTrain);
			}
			if (Game.Sections.Length > 0) {
				Game.UpdateSection(Game.Sections.Length - 1);
			}
			// loop
			Asynchronous.Initialize();
			World.InitializeCameraRestriction();
			while (true) {
				// timer
				double TimeElapsed;
				if (Game.SecondsSinceMidnight >= Game.StartupTime) {
					TimeElapsed = Timers.GetElapsedTime() * (double)TimeFactor;
				} else {
					TimeElapsed = Game.StartupTime - Game.SecondsSinceMidnight;
				}
				TotalTimeElapsedForInfo += TimeElapsed;
				TotalTimeElapsedForSectionUpdate += TimeElapsed;
				TotalFramesElapsed++;
				if (TotalTimeElapsedForSectionUpdate >= 1.0) {
					if (Game.Sections.Length != 0) {
						Game.UpdateSection(Game.Sections.Length - 1);
					}
					TotalTimeElapsedForSectionUpdate = 0.0;
				}
				if (TotalTimeElapsedForInfo >= 0.2) {
					Game.InfoFrameRate = (double)TotalFramesElapsed / TotalTimeElapsedForInfo;
					TotalTimeElapsedForInfo = 0.0;
					TotalFramesElapsed = 0;
				}
				// events
				ProcessEvents();
				World.CameraAlignmentDirection = new World.CameraAlignment();
				ProcessControls(TimeElapsed);
				if (World.CameraMode == World.CameraViewMode.Interior) {
					World.UpdateAbsoluteCamera(TimeElapsed);
				}
				if (Quit) break;
				// update in pieces
				{
					const double w = 0.1;
					double u = TimeElapsed;
					while (true) {
						double v = u < w ? u : w; u -= v;
						Game.SecondsSinceMidnight += v;
						double a = World.CameraTrackFollower.TrackPosition;
						TrainManager.UpdateTrains(v);
						double b = World.CameraTrackFollower.TrackPosition;
						if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.Exterior) {
							World.CameraTrackFollower.TrackPosition = a;
							TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, b, false, false);
						}
						if (u <= 0.0) break;
					}
				}
				// update in one piece
				ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, false);
				if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.Exterior) {
					ObjectManager.UpdateVisibility(World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z);
					int d = TrainManager.PlayerTrain.DriverCar;
					World.CameraSpeed = TrainManager.PlayerTrain.Cars[d].Specs.CurrentSpeed;
				} else {
					World.CameraSpeed = 0.0;
				}
				if (World.CameraMode != World.CameraViewMode.Interior) {
					World.UpdateAbsoluteCamera(TimeElapsed);
				}
				Game.UpdateScore(TimeElapsed);
				Game.UpdateMessages();
				Game.UpdateScoreMessages(TimeElapsed);
				Renderer.RenderScene(TimeElapsed);
				Sdl.SDL_GL_SwapBuffers();
				TextureManager.Update(TimeElapsed);
				SoundManager.Update(TimeElapsed);
				Game.UpdateBlackBox();
				// pause/menu
				while (Game.CurrentInterface != Game.InterfaceType.Normal) {
					ProcessEvents();
					ProcessControls(0.0);
					if (Quit) break;
					if (Game.CurrentInterface == Game.InterfaceType.Pause) {
						System.Threading.Thread.Sleep(10);
					}
					Renderer.RenderScene(TimeElapsed);
					Sdl.SDL_GL_SwapBuffers();
					TimeElapsed = Timers.GetElapsedTime();
				}
				// limit framerate
				if (LimitFramerate) {
					System.Threading.Thread.Sleep(10);
				}
			}
			try {
				Interface.SaveLogs();
			} catch { }
			try {
				Interface.SaveOptions();
			} catch { }
		}

		//// --------------------------------

		//// process events
		private static Interface.KeyboardModifier CurrentKeyboardModifier = Interface.KeyboardModifier.None;
		private static void ProcessEvents() {
			Sdl.SDL_Event Event;
			while (Sdl.SDL_PollEvent(out Event) != 0) {
				switch (Event.type) {
						// quit
					case Sdl.SDL_QUIT:
						Quit = true;
						return;
						// resize
					case Sdl.SDL_VIDEORESIZE:
						Renderer.ScreenWidth = Event.resize.w;
						Renderer.ScreenHeight = Event.resize.h;
						UpdateViewport();
						InitializeMotionBlur();
						break;
						// key down
					case Sdl.SDL_KEYDOWN:
						if (Event.key.keysym.sym == Sdl.SDLK_LSHIFT | Event.key.keysym.sym == Sdl.SDLK_RSHIFT) CurrentKeyboardModifier |= Interface.KeyboardModifier.Shift;
						if (Event.key.keysym.sym == Sdl.SDLK_LCTRL | Event.key.keysym.sym == Sdl.SDLK_RCTRL) CurrentKeyboardModifier |= Interface.KeyboardModifier.Ctrl;
						if (Event.key.keysym.sym == Sdl.SDLK_LALT | Event.key.keysym.sym == Sdl.SDLK_RALT) CurrentKeyboardModifier |= Interface.KeyboardModifier.Alt;
						for (int i = 0; i < Interface.CurrentControls.Length; i++) {
							if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
								if (Interface.CurrentControls[i].Element == Event.key.keysym.sym & Interface.CurrentControls[i].Modifier == CurrentKeyboardModifier) {
									Interface.CurrentControls[i].AnalogState = 1.0;
									if (Interface.CurrentControls[i].DigitalState != Interface.DigitalControlState.PressedAcknowledged) {
										Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
									}
								}
							}
						} break;
						// key up
					case Sdl.SDL_KEYUP:
						if (Event.key.keysym.sym == Sdl.SDLK_LSHIFT | Event.key.keysym.sym == Sdl.SDLK_RSHIFT) CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Shift;
						if (Event.key.keysym.sym == Sdl.SDLK_LCTRL | Event.key.keysym.sym == Sdl.SDLK_RCTRL) CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Ctrl;
						if (Event.key.keysym.sym == Sdl.SDLK_LALT | Event.key.keysym.sym == Sdl.SDLK_RALT) CurrentKeyboardModifier &= ~Interface.KeyboardModifier.Alt;
						for (int i = 0; i < Interface.CurrentControls.Length; i++) {
							if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
								if (Interface.CurrentControls[i].Element == Event.key.keysym.sym) {
									Interface.CurrentControls[i].AnalogState = 0.0;
									if (Interface.CurrentControls[i].DigitalState != Interface.DigitalControlState.ReleasedAcknowledged) {
										Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
									}
								}
							}
						} break;
						// joystick button down
					case Sdl.SDL_JOYBUTTONDOWN:
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Button) {
										if (Interface.CurrentControls[i].Device == (int)Event.jbutton.which & Interface.CurrentControls[i].Element == (int)Event.jbutton.button) {
											Interface.CurrentControls[i].AnalogState = 1.0;
											Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
										}
									}
								}
							}
						} break;
						// joystick button up
					case Sdl.SDL_JOYBUTTONUP:
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Button) {
										if (Interface.CurrentControls[i].Device == (int)Event.jbutton.which & Interface.CurrentControls[i].Element == (int)Event.jbutton.button) {
											Interface.CurrentControls[i].AnalogState = 0.0;
											Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
										}
									}
								}
							}
						} break;
						// joystick hat
					case Sdl.SDL_JOYHATMOTION:
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Hat) {
										if (Interface.CurrentControls[i].Device == (int)Event.jhat.which) {
											if (Interface.CurrentControls[i].Element == (int)Event.jhat.hat) {
												if (Interface.CurrentControls[i].Direction == (int)Event.jhat.val) {
													Interface.CurrentControls[i].AnalogState = 1.0;
													Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
												} else {
													Interface.CurrentControls[i].AnalogState = 0.0;
													Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
												}
											}
										}
									}
								}
							}
						} break;
						// joystick axis
					case Sdl.SDL_JOYAXISMOTION:
						if (Interface.CurrentOptions.UseJoysticks) {
							for (int i = 0; i < Interface.CurrentControls.Length; i++) {
								if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
									if (Interface.CurrentControls[i].Component == Interface.JoystickComponent.Axis) {
										if (Interface.CurrentControls[i].Device == (int)Event.jaxis.which & Interface.CurrentControls[i].Element == (int)Event.jaxis.axis) {
											double a = (double)Event.jaxis.val / 32768.0;
											if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf) {
												if (Math.Sign(a) == Math.Sign(Interface.CurrentControls[i].Direction)) {
													a = Math.Abs(a);
													if (a < Interface.CurrentOptions.JoystickAxisThreshold) {
														Interface.CurrentControls[i].AnalogState = 0.0;
													} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
														Interface.CurrentControls[i].AnalogState = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														Interface.CurrentControls[i].AnalogState = 1.0;
													}
												}
											} else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull) {
												a *= (double)Interface.CurrentControls[i].Direction;
												if (a > -Interface.CurrentOptions.JoystickAxisThreshold & a < Interface.CurrentOptions.JoystickAxisThreshold) {
													Interface.CurrentControls[i].AnalogState = 0.0;
												} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
													if (a < 0.0) {
														Interface.CurrentControls[i].AnalogState = (a + Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else if (a > 0.0) {
														Interface.CurrentControls[i].AnalogState = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														Interface.CurrentControls[i].AnalogState = 0.0;
													}
												} else {
													Interface.CurrentControls[i].AnalogState = (double)Math.Sign(a);
												}
											} else {
												if (Math.Sign(a) == Math.Sign(Interface.CurrentControls[i].Direction)) {
													a = Math.Abs(a);
													if (a < Interface.CurrentOptions.JoystickAxisThreshold) {
														a = 0.0;
													} else if (Interface.CurrentOptions.JoystickAxisThreshold != 1.0) {
														a = (a - Interface.CurrentOptions.JoystickAxisThreshold) / (1.0 - Interface.CurrentOptions.JoystickAxisThreshold);
													} else {
														a = 1.0;
													}
													if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released | Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.ReleasedAcknowledged) {
														if (a > 0.67) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
													} else {
														if (a < 0.33) Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
													}
												}
											}
										}
									}
								}
							}
						} break;
				}
			}
		}

		//// process controls
		private static void ProcessControls(double TimeElapsed) {
			switch (Game.CurrentInterface) {
				case Game.InterfaceType.Pause:
					// pause
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MiscPause:
										Game.CurrentInterface = Game.InterfaceType.Normal;
										break;
									case Interface.Command.MenuActivate:
										Game.CreateMenu(false);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscQuit:
										Game.CreateMenu(true);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscFullscreen:
										ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										SoundManager.Mute = !SoundManager.Mute;
										SoundManager.Update(0.0);
										break;
								}
							}
						}
					} break;
				case Game.InterfaceType.Menu:
					// menu
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MenuUp:
										{
											// up
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											int k = 0; while (k < j) {
												Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
												if (b == null) break;
												a = b.Entries; k++;
											}
											if (Game.CurrentMenuSelection[j] > 0 && !(a[Game.CurrentMenuSelection[j] - 1] is Game.MenuCaption)) {
												Game.CurrentMenuSelection[j]--;
											}
										} break;
									case Interface.Command.MenuDown:
										{
											// down
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											int k = 0; while (k < j) {
												Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
												if (b == null) break;
												a = b.Entries; k++;
											}
											if (Game.CurrentMenuSelection[j] < a.Length - 1) {
												Game.CurrentMenuSelection[j]++;
											}
										} break;
									case Interface.Command.MenuEnter:
										{
											// enter
											Game.MenuEntry[] a = Game.CurrentMenu;
											int j = Game.CurrentMenuSelection.Length - 1;
											{
												int k = 0; while (k < j) {
													Game.MenuSubmenu b = a[Game.CurrentMenuSelection[k]] as Game.MenuSubmenu;
													if (b == null) break;
													a = b.Entries; k++;

												}
											}
											if (a[Game.CurrentMenuSelection[j]] is Game.MenuCommand) {
												// command
												Game.MenuCommand b = (Game.MenuCommand)a[Game.CurrentMenuSelection[j]];
												switch (b.Tag) {
													case Game.MenuTag.Back:
														// back
														if (Game.CurrentMenuSelection.Length <= 1) {
															Game.CurrentInterface = Game.InterfaceType.Normal;
														} else {
															Array.Resize<int>(ref Game.CurrentMenuSelection, Game.CurrentMenuSelection.Length - 1);
															Array.Resize<double>(ref Game.CurrentMenuOffsets, Game.CurrentMenuOffsets.Length - 1);
														} break;
													case Game.MenuTag.JumpToStation:
														{
															// jump to station
															int k = b.Data;
															int t = Game.GetStopIndex(k, TrainManager.PlayerTrain.Cars.Length);
															if (t >= 0) {
																PluginManager.InitializePlugin(TrainManager.PlayerTrain);
																for (int h = 0; h < TrainManager.PlayerTrain.Cars.Length; h++) {
																	TrainManager.PlayerTrain.Cars[h].Specs.CurrentSpeed = 0.0;
																}
																double p = Game.Stations[k].Stops[t].TrackPosition;
																double d = p - TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition;
																TrackManager.SuppressSoundEvents = true;
																while (Math.Abs(d) > 1.0) {
																	for (int h = 0; h < TrainManager.PlayerTrain.Cars.Length; h++) {
																		TrainManager.MoveCar(TrainManager.PlayerTrain, h, (double)Math.Sign(d), 0.0);
																	} d -= (double)Math.Sign(d);
																}
																TrackManager.SuppressSoundEvents = false;
																if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
																	TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, false, 0, true);
																} else {
																	TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, false, TrainManager.PlayerTrain.Specs.MaximumBrakeNotch, false);
																	TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Service);
																}
																if (Game.Sections.Length > 0) {
																	Game.UpdateSection(Game.Sections.Length - 1);
																}
																if (Game.CurrentScore.ArrivalStation <= k) {
																	Game.CurrentScore.ArrivalStation = k + 1;
																}
																if (Game.Stations[k].ArrivalTime >= 0.0) {
																	Game.SecondsSinceMidnight = Game.Stations[k].ArrivalTime;
																} else if (Game.Stations[k].DepartureTime >= 0.0) {
																	Game.SecondsSinceMidnight = Game.Stations[k].DepartureTime - Game.Stations[k].StopTime;
																}
																TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, Game.Stations[k].OpenLeftDoors, Game.Stations[k].OpenRightDoors);
																TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, !Game.Stations[k].OpenLeftDoors, !Game.Stations[k].OpenRightDoors);
																Game.CurrentScore.DepartureStation = k;
																Game.CurrentInterface = Game.InterfaceType.Normal;
																Game.Messages = new Game.Message[] { };
																ObjectManager.UpdateAnimatedWorldObjects(TimeElapsed, true);
															}
														} break;
													case Game.MenuTag.ExitToMainMenu:
														Program.RestartProcessArguments = Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade ? "/review" : "";
														Quit = true;
														break;
													case Game.MenuTag.Quit:
														// quit
														Quit = true;
														break;
												}
											} else if (a[Game.CurrentMenuSelection[j]] is Game.MenuSubmenu) {
												// menu
												Game.MenuSubmenu b = (Game.MenuSubmenu)a[Game.CurrentMenuSelection[j]];
												int n = Game.CurrentMenuSelection.Length;
												Array.Resize<int>(ref Game.CurrentMenuSelection, n + 1);
												Array.Resize<double>(ref Game.CurrentMenuOffsets, n + 1);
												int k; for (k = 0; k < b.Entries.Length; k++) {
													if (!(b.Entries[k] is Game.MenuCaption)) break;
												}
												Game.CurrentMenuSelection[n] = k < b.Entries.Length ? k : 0;
												Game.CurrentMenuOffsets[n] = double.NegativeInfinity;
												a = b.Entries;
												for (int h = 0; h < a.Length; h++) {
													a[h].Highlight = h == 0 ? 1.0 : 0.0;
													a[h].Alpha = 0.0;
												}
											}
										} break;
									case Interface.Command.MenuBack:
										// back
										if (Game.CurrentMenuSelection.Length <= 1) {
											Game.CurrentInterface = Game.InterfaceType.Normal;
										} else {
											Array.Resize<int>(ref Game.CurrentMenuSelection, Game.CurrentMenuSelection.Length - 1);
											Array.Resize<double>(ref Game.CurrentMenuOffsets, Game.CurrentMenuOffsets.Length - 1);
										} break;
									case Interface.Command.MiscFullscreen:
										// fullscreen
										ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										// mute
										SoundManager.Mute = !SoundManager.Mute;
										SoundManager.Update(0.0);
										break;

								}
							}
						}
					} break;
				case Game.InterfaceType.Normal:
					// normal
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogHalf | Interface.CurrentControls[i].InheritedType == Interface.CommandType.AnalogFull) {
							// analog control
							if (Interface.CurrentControls[i].AnalogState != 0.0) {
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.PowerHalfAxis:
									case Interface.Command.PowerFullAxis:
										// power half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											double a = Interface.CurrentControls[i].AnalogState;
											if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
												a = 0.5 * (a + 1.0);
											}
											a *= (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch;
											int p = (int)Math.Round(a);
											TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, 0, true);
										} break;
									case Interface.Command.BrakeHalfAxis:
									case Interface.Command.BrakeFullAxis:
										// brake half/full-axis
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												double a = Interface.CurrentControls[i].AnalogState;
												if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
													a = 0.5 * (a + 1.0);
												}
												int b = (int)Math.Round(3.0 * a);
												switch (b) {
													case 0:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
														break;
													case 1:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
														break;
													case 2:
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Service);
														break;
													case 3:
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
														break;
												}
											} else {
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
														a = 0.5 * (a + 1.0);
													}
													a *= (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2;
													int b = (int)Math.Round(a);
													bool q = b == 1;
													if (b > 0) b--;
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b, false);
													} else {
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
												} else {
													double a = Interface.CurrentControls[i].AnalogState;
													if (Interface.CurrentControls[i].Command == Interface.Command.BrakeFullAxis) {
														a = 0.5 * (a + 1.0);
													}
													a *= (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1;
													int b = (int)Math.Round(a);
													if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
														TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
														TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, b, false);
													} else {
														TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
													}
												}
											}
										} break;
									case Interface.Command.SingleFullAxis:
										// single full axis
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											if (TrainManager.PlayerTrain.Specs.HasHoldBrake) {
												double a = Interface.CurrentControls[i].AnalogState;
												int p = (int)Math.Round(a * (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b = (int)Math.Round(-a * (double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 2);
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												bool q = b == 1;
												if (b > 0) b--;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												} else {
													TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
												}
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, q);
											} else {
												double a = Interface.CurrentControls[i].AnalogState;
												int p = (int)Math.Round(a * (double)TrainManager.PlayerTrain.Specs.MaximumPowerNotch);
												int b = (int)Math.Round(-a * ((double)TrainManager.PlayerTrain.Specs.MaximumBrakeNotch + 1));
												if (p < 0) p = 0;
												if (b < 0) b = 0;
												if (b <= TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver = false;
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, p, false, b, false);
												} else {
													TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
												}
											}
										} break;
									case Interface.Command.ReverserFullAxis:
										// reverser full axis
										{
											double a = Interface.CurrentControls[i].AnalogState;
											int r = (int)Math.Round(a);
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, r, false);
										} break;
									case Interface.Command.CameraMoveForward:
										// camera move forward
										if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.Exterior) {
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = s * Interface.CurrentControls[i].AnalogState;
										} else {
											World.CameraAlignmentDirection.TrackPosition = World.CameraExteriorTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveBackward:
										// camera move backward
										if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.Exterior) {
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Z = -s * Interface.CurrentControls[i].AnalogState;
										} else {
											World.CameraAlignmentDirection.TrackPosition = -World.CameraExteriorTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveLeft:
										// camera move left
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.X = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveRight:
										// camera move right
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.X = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveUp:
										// camera move up
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Y = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraMoveDown:
										// camera move down
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopSpeed : World.CameraExteriorTopSpeed;
											World.CameraAlignmentDirection.Position.Y = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateLeft:
										// camera rotate left
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Yaw = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateRight:
										// camera rotate right
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Yaw = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateUp:
										// camera rotate up
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Pitch = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateDown:
										// camera rotate down
										{
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Pitch = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateCCW:
										// camera rotate ccw
										if (World.CameraMode != World.CameraViewMode.Interior | !World.CameraRestriction) {
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = -s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraRotateCW:
										// camera rotate cw
										if (World.CameraMode != World.CameraViewMode.Interior | !World.CameraRestriction) {
											double s = World.CameraMode == World.CameraViewMode.Interior ? World.CameraInteriorTopAngularSpeed : World.CameraExteriorTopAngularSpeed;
											World.CameraAlignmentDirection.Roll = s * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraZoomIn:
										// camera zoom in
										if (TimeElapsed > 0.0) {
											World.CameraAlignmentDirection.Zoom = -World.CameraZoomTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.CameraZoomOut:
										// camera zoom out
										if (TimeElapsed > 0.0) {
											World.CameraAlignmentDirection.Zoom = World.CameraZoomTopSpeed * Interface.CurrentControls[i].AnalogState;
										} break;
									case Interface.Command.TimetableUp:
										// timetable up
										if (TimeElapsed > 0.0) {
											Renderer.OptionTimetablePosition += 64.0 * Interface.CurrentControls[i].AnalogState * TimeElapsed;
											if (Renderer.OptionTimetablePosition > 0.0) Renderer.OptionTimetablePosition = 0.0;
										} break;
									case Interface.Command.TimetableDown:
										// timetable down
										if (TimeElapsed > 0.0) {
											Renderer.OptionTimetablePosition -= 64.0 * Interface.CurrentControls[i].AnalogState * TimeElapsed;
										} break;
								}
							}
						} else if (Interface.CurrentControls[i].InheritedType == Interface.CommandType.Digital) {
							// digital control
							if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Pressed) {
								// pressed
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.MiscQuit:
										// quit
										Game.CreateMenu(true);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.CameraInterior:
										// camera: interior
										Game.AddMessage(Interface.GetInterfaceString("notification_interior"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										SaveCameraSettings();
										World.CameraMode = World.CameraViewMode.Interior;
										RestoreCameraSettings();
										if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[0].Sections.Length >= 1) {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, 0);
										} else {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, -1);
										}
										for (int j = 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
										}
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport();
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if (!World.PerformCameraRestrictionTest()) {
											World.InitializeCameraRestriction();
										}
										break;
									case Interface.Command.CameraExterior:
										// camera: exterior
										Game.AddMessage(Interface.GetInterfaceString("notification_exterior"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										SaveCameraSettings();
										World.CameraMode = World.CameraViewMode.Exterior;
										RestoreCameraSettings();
										if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[0].Sections.Length >= 2) {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, 1);
										} else {
											TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, -1);
										}
										for (int j = 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
											if (TrainManager.PlayerTrain.Cars[j].Sections.Length >= 1) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
											}
										}
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport();
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										break;
									case Interface.Command.CameraTrack:
									case Interface.Command.CameraFlyBy:
										// camera: track / fly-by
										{
											SaveCameraSettings();
											if (Interface.CurrentControls[i].Command == Interface.Command.CameraTrack) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											} else {
												if (World.CameraMode == World.CameraViewMode.FlyBy) {
													World.CameraMode = World.CameraViewMode.FlyByZooming;
													Game.AddMessage(Interface.GetInterfaceString("notification_flybyzooming"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
												} else {
													World.CameraMode = World.CameraViewMode.FlyBy;
													Game.AddMessage(Interface.GetInterfaceString("notification_flybynormal"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
												}
											}
											RestoreCameraSettings();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[0].Sections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, -1);
											}
											for (int j = 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (TrainManager.PlayerTrain.Cars[j].Sections.Length >= 1) {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
												} else {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
												}
											}
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											UpdateViewport();
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraPreviousPOI:
										// camera: previous poi
										if (Game.ApplyPointOfView(-1, true)) {
											if (World.CameraMode != World.CameraViewMode.Track & World.CameraMode != World.CameraViewMode.FlyBy & World.CameraMode != World.CameraViewMode.FlyByZooming) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position = new World.Vector3D(World.CameraCurrentAlignment.Position.X, World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[0].Sections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, -1);
											}
											for (int j = 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (TrainManager.PlayerTrain.Cars[j].Sections.Length >= 1) {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
												} else {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
												}
											}
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport();
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraNextPOI:
										// camera: next poi
										if (Game.ApplyPointOfView(1, true)) {
											if (World.CameraMode != World.CameraViewMode.Track & World.CameraMode != World.CameraViewMode.FlyBy & World.CameraMode != World.CameraViewMode.FlyByZooming) {
												World.CameraMode = World.CameraViewMode.Track;
												Game.AddMessage(Interface.GetInterfaceString("notification_track"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
											}
											double z = World.CameraCurrentAlignment.Position.Z;
											World.CameraCurrentAlignment.Position = new World.Vector3D(World.CameraCurrentAlignment.Position.X, World.CameraCurrentAlignment.Position.Y, 0.0);
											World.CameraCurrentAlignment.Zoom = 0.0;
											World.CameraAlignmentDirection = new World.CameraAlignment();
											World.CameraAlignmentSpeed = new World.CameraAlignment();
											if (TrainManager.PlayerTrain.Cars.Length >= 1 && TrainManager.PlayerTrain.Cars[0].Sections.Length >= 2) {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, 1);
											} else {
												TrainManager.ChangeCarSection(TrainManager.PlayerTrain, 0, -1);
											}
											for (int j = 1; j < TrainManager.PlayerTrain.Cars.Length; j++) {
												if (TrainManager.PlayerTrain.Cars[j].Sections.Length >= 1) {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, 0);
												} else {
													TrainManager.ChangeCarSection(TrainManager.PlayerTrain, j, -1);
												}
											}
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraTrackFollower.TrackPosition + z, true, false);
											World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
											World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
											UpdateViewport();
											World.UpdateAbsoluteCamera(TimeElapsed);
											World.UpdateViewingDistances();
										} break;
									case Interface.Command.CameraReset:
										// camera: reset
										if (World.CameraMode == World.CameraViewMode.Interior) {
											World.CameraCurrentAlignment.Position = new World.Vector3D(0.0, 0.0, 0.0);
										}
										World.CameraCurrentAlignment.Yaw = 0.0;
										World.CameraCurrentAlignment.Pitch = 0.0;
										World.CameraCurrentAlignment.Roll = 0.0;
										if (World.CameraMode == World.CameraViewMode.Track) {
											TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition, true, false);
										} else if (World.CameraMode == World.CameraViewMode.FlyBy | World.CameraMode == World.CameraViewMode.FlyByZooming) {
											if (TrainManager.PlayerTrain.Specs.CurrentAverageSpeed >= 0.0) {
												double d = 30.0 + TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition + d, true, false);
											} else {
												double d = 30.0 - TrainManager.PlayerTrain.Specs.CurrentAverageSpeed;
												TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower.TrackPosition - d, true, false);
											}
										}
										World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
										World.CameraCurrentAlignment.Zoom = 0.0;
										World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
										World.CameraAlignmentDirection = new World.CameraAlignment();
										World.CameraAlignmentSpeed = new World.CameraAlignment();
										UpdateViewport();
										World.UpdateAbsoluteCamera(TimeElapsed);
										World.UpdateViewingDistances();
										if (World.CameraMode == World.CameraViewMode.Interior & World.CameraRestriction) {
											if (!World.PerformCameraRestrictionTest()) {
												World.InitializeCameraRestriction();
											}
										}
										break;
									case Interface.Command.CameraRestriction:
										// camera: restriction
										World.CameraRestriction = !World.CameraRestriction;
										World.InitializeCameraRestriction();
										Game.AddMessage(Interface.GetInterfaceString(World.CameraRestriction ? "notification_camerarestriction_on" : "notification_camerarestriction_off"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										break;
									case Interface.Command.SinglePower:
										// single power
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
											if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
												TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
											} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
											} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
												TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
											} else if (b > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
											} else {
												int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
												if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
												}
											}
										} break;
									case Interface.Command.SingleNeutral:
										// single neutral
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (b > 0) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										} break;
									case Interface.Command.SingleBrake:
										// single brake
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										} break;
									case Interface.Command.SingleEmergency:
										// single emergency
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										} break;
									case Interface.Command.PowerIncrease:
										// power increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p < TrainManager.PlayerTrain.Specs.MaximumPowerNotch) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, 1, true, 0, true);
											}
										} break;
									case Interface.Command.PowerDecrease:
										// power decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int p = TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver;
											if (p > 0) {
												TrainManager.ApplyNotch(TrainManager.PlayerTrain, -1, true, 0, true);
											}
										} break;
									case Interface.Command.BrakeIncrease:
										// brake increase
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Service);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
												}
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.HasHoldBrake & b == 0 & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (b < TrainManager.PlayerTrain.Specs.MaximumBrakeNotch) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 1, true);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												}
											}
										} break;
									case Interface.Command.BrakeDecrease:
										// brake decrease
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (TrainManager.PlayerTrain.Specs.HasHoldBrake & TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap & !TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Release);
												} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service) {
													TrainManager.ApplyAirBrakeHandle(TrainManager.PlayerTrain, TrainManager.AirBrakeHandleState.Lap);
												}
											} else {
												int b = TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver;
												if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
													TrainManager.UnapplyEmergencyBrake(TrainManager.PlayerTrain);
												} else if (b == 1 & TrainManager.PlayerTrain.Specs.HasHoldBrake) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, 0, false);
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, true);
												} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
													TrainManager.ApplyHoldBrake(TrainManager.PlayerTrain, false);
												} else if (b > 0) {
													TrainManager.ApplyNotch(TrainManager.PlayerTrain, 0, true, -1, true);
												}
											}
										} break;
									case Interface.Command.BrakeEmergency:
										// brake emergency
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											TrainManager.ApplyEmergencyBrake(TrainManager.PlayerTrain);
										} break;
									case Interface.Command.DeviceConstSpeed:
										// const speed
										if (TrainManager.PlayerTrain.Specs.HasConstSpeed) {
											TrainManager.PlayerTrain.Specs.CurrentConstSpeed = !TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
										} break;
									case Interface.Command.ReverserForward:
										// reverser forward
										if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 1) {
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, 1, true);
										} break;
									case Interface.Command.ReverserBackward:
										// reverser backward
										if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > -1) {
											TrainManager.ApplyReverser(TrainManager.PlayerTrain, -1, true);
										} break;
									case Interface.Command.HornPrimary:
										// horn: primary
										{
											const int j = 0;
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns.Length > j) {
												int snd = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundBufferIndex;
												if (snd >= 0) {
													World.Vector3D pos = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Position;
													int src = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex;
													if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Loop) {
														if (SoundManager.IsPlaying(src)) {
															SoundManager.StopSound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex);
														} else {
															SoundManager.PlaySound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex, snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, true);
														}
													} else {
														SoundManager.PlaySound(snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, false);
													}
													PluginManager.UpdateHorn(0);
												}
											}
										} break;
									case Interface.Command.HornSecondary:
										// horn: secondary
										{
											const int j = 1;
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns.Length > j) {
												int snd = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundBufferIndex;
												if (snd >= 0) {
													World.Vector3D pos = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Position;
													int src = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex;
													if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Loop) {
														if (SoundManager.IsPlaying(src)) {
															SoundManager.StopSound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex);
														} else {
															SoundManager.PlaySound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex, snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, true);
														}
													} else {
														SoundManager.PlaySound(snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, false);
													}
													PluginManager.UpdateHorn(1);
												}
											}
										} break;
									case Interface.Command.HornMusic:
										// horn: music
										{
											const int j = 2;
											int d = TrainManager.PlayerTrain.DriverCar;
											if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns.Length > j) {
												int snd = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundBufferIndex;
												if (snd >= 0) {
													World.Vector3D pos = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.Position;
													int src = TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex;
													if (TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Loop) {
														if (SoundManager.IsPlaying(src)) {
															SoundManager.StopSound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex);
														} else {
															SoundManager.PlaySound(ref TrainManager.PlayerTrain.Cars[d].Sounds.Horns[j].Sound.SoundSourceIndex, snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, true);
														}
													} else {
														SoundManager.PlaySound(snd, TrainManager.PlayerTrain, TrainManager.PlayerTrain.DriverCar, pos, SoundManager.Importance.DontCare, false);
													}
													PluginManager.UpdateHorn(2);
												}
											}
										} break;
									case Interface.Command.DoorsLeft:
										// doors: left
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false) & TrainManager.TrainDoorState.Opened) == 0) {
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, true, false);
											}
										} else {
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != TrainManager.DoorMode.Automatic) {
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, true, false);
											}
										} break;
									case Interface.Command.DoorsRight:
										// doors: right
										if ((TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true) & TrainManager.TrainDoorState.Opened) == 0) {
											if (TrainManager.PlayerTrain.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
												TrainManager.OpenTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										} else {
											if (TrainManager.PlayerTrain.Specs.DoorCloseMode != TrainManager.DoorMode.Automatic) {
												TrainManager.CloseTrainDoors(TrainManager.PlayerTrain, false, true);
											}
										} break;
									case Interface.Command.SecurityPower:
										// security: power
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.None) {
											if (TrainManager.PlayerTrain.Specs.Security.Ats.AtsAvailable) {
												TrainManager.PlayerTrain.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSN;
											} else if (TrainManager.PlayerTrain.Specs.Security.Atc.Available) {
												TrainManager.PlayerTrain.Specs.Security.ModeChange = TrainManager.SecuritySystem.Atc;
											}
										} else {
											TrainManager.PlayerTrain.Specs.Security.ModeChange = TrainManager.SecuritySystem.None;
										} break;
									case Interface.Command.SecurityS:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_S, true);
										TrainManager.AcknowledgeSecuritySystem(TrainManager.PlayerTrain, TrainManager.AcknowledgementType.Alarm);
										break;
									case Interface.Command.SecurityA1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_A1, true);
										TrainManager.AcknowledgeSecuritySystem(TrainManager.PlayerTrain, TrainManager.AcknowledgementType.Chime);
										break;
									case Interface.Command.SecurityA2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_A2, true);
										TrainManager.AcknowledgeSecuritySystem(TrainManager.PlayerTrain, TrainManager.AcknowledgementType.Eb);
										break;
									case Interface.Command.SecurityB1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_B1, true);
										TrainManager.AcknowledgeSecuritySystem(TrainManager.PlayerTrain, TrainManager.AcknowledgementType.Reset);
										break;
									case Interface.Command.SecurityB2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_B2, true);
										TrainManager.AcknowledgeSecuritySystem(TrainManager.PlayerTrain, TrainManager.AcknowledgementType.Override);
										break;
									case Interface.Command.SecurityC1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_C1, true);
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc & TrainManager.PlayerTrain.Specs.Security.Ats.AtsAvailable) {
											TrainManager.PlayerTrain.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSN;
										} break;
									case Interface.Command.SecurityC2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_C2, true);
										if (TrainManager.PlayerTrain.Specs.Security.Atc.Available & (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN | TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP)) {
											TrainManager.PlayerTrain.Specs.Security.ModeChange = TrainManager.SecuritySystem.Atc;
										} break;
									case Interface.Command.SecurityD:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_D, true);
										break;
									case Interface.Command.SecurityE:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_E, true);
										break;
									case Interface.Command.SecurityF:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_F, true);
										break;
									case Interface.Command.SecurityG:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_G, true);
										break;
									case Interface.Command.SecurityH:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_H, true);
										break;
									case Interface.Command.SecurityI:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_I, true);
										break;
									case Interface.Command.SecurityJ:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_J, true);
										break;
									case Interface.Command.SecurityK:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_K, true);
										break;
									case Interface.Command.SecurityL:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_L, true);
										break;
									case Interface.Command.TimetableToggle:
										// option: timetable
										if (Timetable.CustomTrainIndex >= 0) {
											if (Timetable.CustomVisible) {
												Timetable.CustomVisible = false;
												Renderer.OptionTimetable = true;
											} else if (Renderer.OptionTimetable) {
												Renderer.OptionTimetable = false;
											} else {
												Timetable.CustomVisible = true;
											}
										} else {
											Renderer.OptionTimetable = !Renderer.OptionTimetable;
										} break;
									case Interface.Command.DebugWireframe:
										// option: wireframe
										Renderer.OptionWireframe = !Renderer.OptionWireframe;
										if (Renderer.OptionWireframe) {
											Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
										} else {
											Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
										} break;
									case Interface.Command.DebugNormals:
										// option: normals
										Renderer.OptionNormals = !Renderer.OptionNormals;
										break;
									case Interface.Command.MiscAI:
										// option: AI
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											if (TrainManager.PlayerTrain.AI == null) {
												TrainManager.PlayerTrain.AI = new Game.SimplisticHumanDriverAI(TrainManager.PlayerTrain);
												if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
													Game.AddMessage(Interface.GetInterfaceString("notification_aiunable"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 10.0);
												}
											} else {
												TrainManager.PlayerTrain.AI = null;
											}
										} break;
									case Interface.Command.MiscInterfaceMode:
										// option: debug
										switch (Renderer.CurrentOutputMode) {
											case Renderer.OutputMode.Default:
												Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode == Interface.GameMode.Expert ? Renderer.OutputMode.None : Renderer.OutputMode.Debug;
												break;
											case Renderer.OutputMode.Debug:
												Renderer.CurrentOutputMode = Renderer.OutputMode.None;
												break;
											default:
												Renderer.CurrentOutputMode = Renderer.OutputMode.Default;
												break;
										} break;
									case Interface.Command.MiscBackfaceCulling:
										// option: backface culling
										Renderer.OptionBackfaceCulling = !Renderer.OptionBackfaceCulling;
										Game.AddMessage(Interface.GetInterfaceString(Renderer.OptionBackfaceCulling ? "notification_backfaceculling_on" : "notification_backfaceculling_off"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										break;
									case Interface.Command.MiscCPUMode:
										// option: limit frame rate
										LimitFramerate = !LimitFramerate;
										Game.AddMessage(Interface.GetInterfaceString(LimitFramerate ? "notification_cpu_low" : "notification_cpu_normal"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 2.0);
										break;
									case Interface.Command.DebugBrakeSystems:
										// option: brake systems
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											Renderer.OptionBrakeSystems = !Renderer.OptionBrakeSystems;
										} break;
									case Interface.Command.MenuActivate:
										// menu
										Game.CreateMenu(false);
										Game.CurrentInterface = Game.InterfaceType.Menu;
										break;
									case Interface.Command.MiscPause:
										// pause
										Game.CurrentInterface = Game.InterfaceType.Pause;
										break;
									case Interface.Command.MiscClock:
										// clock
										Renderer.OptionClock = !Renderer.OptionClock;
										break;
									case Interface.Command.MiscTimeFactor:
										// time factor
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											TimeFactor = TimeFactor == 1 ? 5 : 1;
											Game.AddMessage(TimeFactor.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x", Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0 * (double)TimeFactor);
										}
										break;
									case Interface.Command.MiscSpeed:
										// speed
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Expert) {
											Game.AddMessage(Interface.GetInterfaceString("notification_notavailableexpert"), Game.MessageDependency.None, Interface.GameMode.Expert, Game.MessageColor.Blue, Game.SecondsSinceMidnight + 5.0);
										} else {
											Renderer.OptionSpeed++;
											if ((int)Renderer.OptionSpeed >= 3) Renderer.OptionSpeed = 0;
										} break;
									case Interface.Command.MiscFps:
										// fps
										Renderer.OptionFrameRates = !Renderer.OptionFrameRates;
										break;
									case Interface.Command.MiscFullscreen:
										// toggle fullscreen
										ToggleFullscreen();
										break;
									case Interface.Command.MiscMute:
										// mute
										SoundManager.Mute = !SoundManager.Mute;
										break;
								}
							} else if (Interface.CurrentControls[i].DigitalState == Interface.DigitalControlState.Released) {
								// released
								Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.ReleasedAcknowledged;
								switch (Interface.CurrentControls[i].Command) {
									case Interface.Command.SecurityS:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_S, false);
										break;
									case Interface.Command.SecurityA1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_A1, false);
										break;
									case Interface.Command.SecurityA2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_A2, false);
										break;
									case Interface.Command.SecurityB1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_B1, false);
										break;
									case Interface.Command.SecurityB2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_B2, false);
										break;
									case Interface.Command.SecurityC1:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_C1, false);
										break;
									case Interface.Command.SecurityC2:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_C2, false);
										break;
									case Interface.Command.SecurityD:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_D, false);
										break;
									case Interface.Command.SecurityE:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_E, false);
										break;
									case Interface.Command.SecurityF:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_F, false);
										break;
									case Interface.Command.SecurityG:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_G, false);
										break;
									case Interface.Command.SecurityH:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_H, false);
										break;
									case Interface.Command.SecurityI:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_I, false);
										break;
									case Interface.Command.SecurityJ:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_J, false);
										break;
									case Interface.Command.SecurityK:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_K, false);
										break;
									case Interface.Command.SecurityL:
										if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) PluginManager.UpdateKey(PluginManager.ATS_KEY_L, false);
										break;
								}
							}
						}
					} break;
			}
		}
		
		//// --------------------------------

		//// save camera setting
		private static void SaveCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
					World.CameraSavedInterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraSavedExterior = World.CameraCurrentAlignment;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraSavedTrack = World.CameraCurrentAlignment;
					break;
			}
		}
		
		//// restore camera setting
		private static void RestoreCameraSettings() {
			switch (World.CameraMode) {
				case World.CameraViewMode.Interior:
					World.CameraCurrentAlignment = World.CameraSavedInterior;
					break;
				case World.CameraViewMode.Exterior:
					World.CameraCurrentAlignment = World.CameraSavedExterior;
					break;
				case World.CameraViewMode.Track:
				case World.CameraViewMode.FlyBy:
				case World.CameraViewMode.FlyByZooming:
					World.CameraCurrentAlignment = World.CameraSavedTrack;
					TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, World.CameraSavedTrack.TrackPosition, true, false);
					World.CameraCurrentAlignment.TrackPosition = World.CameraTrackFollower.TrackPosition;
					break;
			}
			World.CameraCurrentAlignment.Zoom = 0.0;
			World.VerticalViewingAngle = World.OriginalVerticalViewingAngle;
		}

		// --------------------------------

		// toggle fullscreen
		internal static void ToggleFullscreen() {
			Interface.CurrentOptions.FullscreenMode = !Interface.CurrentOptions.FullscreenMode;
			Gl.glDisable(Gl.GL_FOG); Renderer.FogEnabled = false;
			Gl.glDisable(Gl.GL_LIGHTING); Renderer.LightingEnabled = false;
			TextureManager.UnuseAllTextures();
			Fonts.Initialize();
			if (Interface.CurrentOptions.FullscreenMode) {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.FullscreenWidth, Interface.CurrentOptions.FullscreenHeight, Interface.CurrentOptions.FullscreenBits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | Sdl.SDL_FULLSCREEN);
				Renderer.ScreenWidth = Interface.CurrentOptions.FullscreenWidth;
				Renderer.ScreenHeight = Interface.CurrentOptions.FullscreenHeight;
			} else {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.WindowWidth, Interface.CurrentOptions.WindowHeight, 32, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF);
				Renderer.ScreenWidth = Interface.CurrentOptions.WindowWidth;
				Renderer.ScreenHeight = Interface.CurrentOptions.WindowHeight;
			}
			Renderer.InitializeLighting();
			UpdateViewport();
			InitializeMotionBlur();
			Timetable.CreateTimetable();
			Timetable.UpdateCustomTimetable(-1, -1);
			World.InitializeCameraRestriction();
		}

		// update viewport
		internal static void UpdateViewport() {
			Gl.glViewport(0, 0, Renderer.ScreenWidth, Renderer.ScreenHeight);
			World.AspectRatio = (double)Renderer.ScreenWidth / (double)Renderer.ScreenHeight;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();
			const double invdeg = 57.295779513082320877;
			Glu.gluPerspective(World.VerticalViewingAngle * invdeg, -World.AspectRatio, 0.2, World.BackgroundImageDistance + 3.0 * World.ExtraViewingDistance);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glLoadIdentity();
		}

		//// initialize motion blur
		internal static void InitializeMotionBlur() {
			if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None) {
				if (Renderer.PixelBufferOpenGlTextureIndex != 0) {
					Gl.glDeleteTextures(1, new int[] { Renderer.PixelBufferOpenGlTextureIndex });
					Renderer.PixelBufferOpenGlTextureIndex = 0;
				}
				int w = Interface.RoundToPowerOfTwo(Renderer.ScreenWidth);
				int h = Interface.RoundToPowerOfTwo(Renderer.ScreenHeight);
				Renderer.PixelBuffer = new byte[4 * w * h];
				int[] a = new int[1];
				Gl.glGenTextures(1, a);
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, a[0]);
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR);
				Gl.glTexParameterf(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR);
				Gl.glTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, w, h, 0, Gl.GL_RGB, Gl.GL_UNSIGNED_BYTE, Renderer.PixelBuffer);
				Renderer.PixelBufferOpenGlTextureIndex = a[0];
				Gl.glCopyTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, 0, 0, w, h, 0);
			}
		}

	}
}