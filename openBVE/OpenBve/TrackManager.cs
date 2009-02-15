using System;

namespace OpenBve {
    internal static class TrackManager {

        // events
        internal enum EventTriggerType {
            None = 0,
            Camera = 1,
            FrontCarFrontAxle = 2,
            RearCarRearAxle = 3,
            OtherCarFrontAxle = 4,
            OtherCarRearAxle = 5
        }
        internal abstract class GeneralEvent {
            internal double TrackPositionDelta;
            internal bool DontTriggerAnymore;
            internal abstract void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex);
        }
        internal static void TryTriggerEvent(GeneralEvent Event, int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
            if (!Event.DontTriggerAnymore) {
                Event.Trigger(Direction, TriggerType, TrainIndex, CarIndex);
            }
        }
        // background change
        internal class BackgroundChangeEvent : GeneralEvent {
            internal World.Background PreviousBackground;
            internal World.Background NextBackground;
            internal BackgroundChangeEvent(double TrackPositionDelta, World.Background PreviousBackground,World.Background NextBackground) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousBackground = PreviousBackground;
                this.NextBackground = NextBackground;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        World.TargetBackground = this.PreviousBackground;
                        World.TargetBackgroundCountdown = World.TargetBackgroundDefaultCountdown;
                    } else if (Direction > 0) {
                        World.TargetBackground = this.NextBackground;
                        World.TargetBackgroundCountdown = World.TargetBackgroundDefaultCountdown;
                    }
                }
            }
        }
        // fog change
        internal class FogChangeEvent : GeneralEvent {
            internal Game.Fog PreviousFog;
            internal Game.Fog CurrentFog;
            internal Game.Fog NextFog;
            internal FogChangeEvent(double TrackPositionDelta, Game.Fog PreviousFog, Game.Fog CurrentFog, Game.Fog NextFog) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousFog = PreviousFog;
                this.CurrentFog = CurrentFog;
                this.NextFog = NextFog;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.Camera) {
                    if (Direction < 0) {
                        Game.PreviousFog = this.PreviousFog;
                        Game.NextFog = this.CurrentFog;
                    } else if (Direction > 0) {
                        Game.PreviousFog = this.CurrentFog;
                        Game.NextFog = this.NextFog;
                    }
                }
            }
        }
        // brightness change
        internal class BrightnessChangeEvent : GeneralEvent {
            internal float CurrentBrightness;
            internal float PreviousBrightness;
            internal double PreviousDistance;
            internal float NextBrightness;
            internal double NextDistance;
            internal BrightnessChangeEvent(double TrackPositionDelta, float CurrentBrightness, float PreviousBrightness, double PreviousDistance, float NextBrightness, double NextDistance) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.CurrentBrightness = CurrentBrightness;
                this.PreviousBrightness = PreviousBrightness;
                this.PreviousDistance = PreviousDistance;
                this.NextBrightness = NextBrightness;
                this.NextDistance = NextDistance;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.NextBrightness = TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.PreviousBrightness;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.NextTrackPosition = TrainManager.Trains[TrainIndex].Cars[CarIndex].FrontAxle.Follower.TrackPosition;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.PreviousBrightness = this.PreviousBrightness;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.PreviousTrackPosition = TrainManager.Trains[TrainIndex].Cars[CarIndex].FrontAxle.Follower.TrackPosition - this.PreviousDistance;
                    } else if (Direction > 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.PreviousBrightness = TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.NextBrightness;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.PreviousTrackPosition = TrainManager.Trains[TrainIndex].Cars[CarIndex].FrontAxle.Follower.TrackPosition;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.NextBrightness = this.NextBrightness;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Brightness.NextTrackPosition = TrainManager.Trains[TrainIndex].Cars[CarIndex].FrontAxle.Follower.TrackPosition + this.NextDistance;
                    }
                }
            }
        }
        // marker start
        internal class MarkerStartEvent : GeneralEvent {
            internal int TextureIndex;
            internal MarkerStartEvent(double TrackPositionDelta, int TextureIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.TextureIndex = TextureIndex;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TrainIndex == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (Direction < 0) {
                        Game.RemoveMarker(this.TextureIndex);
                    } else if (Direction > 0) {
                        Game.AddMarker(this.TextureIndex);
                    }
                }
            }
        }
        // marker end
        internal class MarkerEndEvent : GeneralEvent {
            internal int TextureIndex;
            internal MarkerEndEvent(double TrackPositionDelta, int TextureIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.TextureIndex = TextureIndex;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TrainIndex == TrainManager.PlayerTrain & TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (Direction < 0) {
                        Game.AddMarker(this.TextureIndex);
                    } else if (Direction > 0) {
                        Game.RemoveMarker(this.TextureIndex);
                    }
                }
            }
        }
        // station pass alarm
        internal class StationPassAlarmEvent : GeneralEvent {
            internal StationPassAlarmEvent(double TrackPositionDelta) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (Direction > 0) {
                        int d = TrainManager.Trains[TrainIndex].DriverCar;
                        int snd = TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.SoundBufferIndex;
                        if (snd >= 0) {
                            World.Vector3D pos = TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.Position;
                            if (TrainManager.Trains[TrainIndex].Specs.PassAlarm == TrainManager.PassAlarmType.Single) {
                                SoundManager.PlaySound(snd, TrainIndex, d, pos, SoundManager.Importance.DontCare, false);
                            } else if (TrainManager.Trains[TrainIndex].Specs.PassAlarm == TrainManager.PassAlarmType.Loop) {
                                SoundManager.PlaySound(ref TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.SoundSourceIndex, snd, TrainIndex, d, pos, SoundManager.Importance.DontCare, true);
                            }
                        }
                        this.DontTriggerAnymore = true;
                    }
                }
            }
        }
        // station start
        internal class StationStartEvent : GeneralEvent {
            internal int StationIndex;
            internal StationStartEvent(double TrackPositionDelta, int StationIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.StationIndex = StationIndex;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].Station = -1;
                        TrainManager.Trains[TrainIndex].StationFrontCar = false;
                        if (Game.Stations[this.StationIndex].SecuritySystem != Game.SecuritySystem.Atc) {
                            TrainManager.Trains[TrainIndex].Specs.Security.Atc.Transmitting = false;
                        }
                    } else if (Direction > 0) {
                        TrainManager.Trains[TrainIndex].Station = StationIndex;
                        TrainManager.Trains[TrainIndex].StationFrontCar = true;
                        TrainManager.Trains[TrainIndex].StationState = TrainManager.TrainStopState.Pending;
                        if (Game.Stations[this.StationIndex].SecuritySystem == Game.SecuritySystem.Atc) {
                            TrainManager.Trains[TrainIndex].Specs.Security.Atc.Transmitting = true;
                        }
                    }
                } else if (TriggerType == EventTriggerType.RearCarRearAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].StationRearCar = false;
                    } else {
                        TrainManager.Trains[TrainIndex].StationRearCar = true;
                    }
                }
            }
        }
        // station end
        internal class StationEndEvent : GeneralEvent {
            internal int StationIndex;
            internal StationEndEvent(double TrackPositionDelta, int StationIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.StationIndex = StationIndex;
            }
            override internal void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].StationFrontCar = true;
                        if (Game.Stations[this.StationIndex].SecuritySystem == Game.SecuritySystem.Atc) {
                            TrainManager.Trains[TrainIndex].Specs.Security.Atc.Transmitting = true;
                        }
                    } else if (Direction > 0) {
                        TrainManager.Trains[TrainIndex].StationFrontCar = false;
                        if (Game.Stations[this.StationIndex].SecuritySystem != Game.SecuritySystem.Atc) {
                            TrainManager.Trains[TrainIndex].Specs.Security.Atc.Transmitting = false;
                        }
                        if (TrainIndex == TrainManager.PlayerTrain) {
                            Timetable.UpdateCustomTimetable(Game.Stations[this.StationIndex].TimetableDaytimeTexture, Game.Stations[this.StationIndex].TimetableNighttimeTexture);
                        }
                    }
                } else if (TriggerType == EventTriggerType.RearCarRearAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].Station = StationIndex;
                        TrainManager.Trains[TrainIndex].StationRearCar = true;
                    } else if (Direction > 0) {
                        if (TrainManager.Trains[TrainIndex].Station == StationIndex) {
                            if (TrainIndex == TrainManager.PlayerTrain) {
                                if (Game.Stations[StationIndex].StopAtStation & TrainManager.Trains[TrainManager.PlayerTrain].StationState == TrainManager.TrainStopState.Pending) {
                                    string s = Interface.GetInterfaceString("message_station_passed");
                                    s = s.Replace("[name]", Game.Stations[StationIndex].Name);
                                    Game.AddMessage(s, Game.MessageDependency.None, Game.MessageType.Game, Game.MessageColor.Orange, Game.SecondsSinceMidnight + 10.0);
                                } else if (Game.Stations[StationIndex].StopAtStation & TrainManager.Trains[TrainManager.PlayerTrain].StationState == TrainManager.TrainStopState.Boarding) {
                                    string s = Interface.GetInterfaceString("message_station_passed_boarding");
                                    s = s.Replace("[name]", Game.Stations[StationIndex].Name);
                                    Game.AddMessage(s, Game.MessageDependency.None, Game.MessageType.Game, Game.MessageColor.Red, Game.SecondsSinceMidnight + 10.0);
                                }
                            }
                            TrainManager.Trains[TrainIndex].Station = -1;
                            TrainManager.Trains[TrainIndex].StationRearCar = false;
                            TrainManager.Trains[TrainIndex].StationState = TrainManager.TrainStopState.Pending;
                            int d = TrainManager.Trains[TrainIndex].DriverCar;
                            if (TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.SoundBufferIndex >= 0 && SoundManager.IsPlaying(TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.SoundSourceIndex)) {
                                SoundManager.StopSound(ref TrainManager.Trains[TrainIndex].Cars[d].Sounds.Halt.SoundSourceIndex);
                            }
                        }
                    }
                }
            }
        }
        // section change
        internal class SectionChangeEvent : GeneralEvent {
            internal int PreviousSectionIndex;
            internal int NextSectionIndex;
            internal SectionChangeEvent(double TrackPositionDelta, int PreviousSectionIndex, int NextSectionIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousSectionIndex = PreviousSectionIndex;
                this.NextSectionIndex = NextSectionIndex;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TrainIndex >= 0) {
                    if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                        if (Direction < 0) {
                            if (this.NextSectionIndex >= 0) {
                                Game.Sections[this.NextSectionIndex].TrainReachedStopPoint = false;
                            }
                            UpdateFrontBackward(TrainIndex);
                        } else if (Direction > 0) {
                            UpdateFrontForward(TrainIndex, true, true);
                        }
                    } else if (TriggerType == EventTriggerType.RearCarRearAxle) {
                        if (Direction < 0) {
                            UpdateRearBackward(TrainIndex, true, true);
                        } else if (Direction > 0) {
                            if (this.PreviousSectionIndex >= 0) {
                                Game.Sections[this.PreviousSectionIndex].TrainReachedStopPoint = false;
                            }
                            UpdateRearForward(TrainIndex);
                        }
                    }
                }
            }
            private void UpdateFrontBackward(int TrainIndex) {
                // update sections
                if (this.PreviousSectionIndex >= 0) {
                    Game.Sections[this.PreviousSectionIndex].Enter(TrainIndex);
                    Game.UpdateSection(this.PreviousSectionIndex);
                }
                if (this.NextSectionIndex >= 0) {
                    Game.Sections[this.NextSectionIndex].Leave(TrainIndex);
                    Game.UpdateSection(this.NextSectionIndex);
                }
            }
            private void UpdateFrontForward(int TrainIndex, bool UpdateTrain, bool UpdateSection) {
                if (UpdateTrain) {
                    // update train
                    if (this.NextSectionIndex >= 0) {
                        if (!Game.Sections[this.NextSectionIndex].Invisible) {
                            if (Game.Sections[this.NextSectionIndex].CurrentAspect >= 0) {
                                TrainManager.Trains[TrainIndex].CurrentSectionLimit = Game.Sections[this.NextSectionIndex].Aspects[Game.Sections[this.NextSectionIndex].CurrentAspect].Speed;
                                if (TrainManager.Trains[TrainIndex].CurrentSectionLimit == 0.0) { }
                            } else {
                                TrainManager.Trains[TrainIndex].CurrentSectionLimit = double.PositiveInfinity;
                            }
                            TrainManager.Trains[TrainIndex].CurrentSectionIndex = this.NextSectionIndex;
                            if (TrainManager.Trains[TrainIndex].Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
                                int a = Game.Sections[this.NextSectionIndex].CurrentAspect;
                                if (a >= 0) {
                                    PluginManager.UpdateSignal(Game.Sections[this.NextSectionIndex].Aspects[a].Number);
                                }
                            }
                        }
                    } else {
                        TrainManager.Trains[TrainIndex].CurrentSectionLimit = double.PositiveInfinity;
                        TrainManager.Trains[TrainIndex].CurrentSectionIndex = -1;
                    }
                    // messages
                    if (this.NextSectionIndex < 0 || !Game.Sections[this.NextSectionIndex].Invisible) {
                        if (TrainIndex == TrainManager.PlayerTrain & TrainManager.Trains[TrainIndex].Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                            if (TrainManager.Trains[TrainIndex].CurrentSectionLimit == 0.0) {
                                Game.AddMessage(Interface.GetInterfaceString("message_signal_stop"), Game.MessageDependency.SectionLimit, Game.MessageType.Game, Game.MessageColor.Red, double.PositiveInfinity);
                            } else if (TrainManager.Trains[TrainIndex].Specs.CurrentAverageSpeed > TrainManager.Trains[TrainIndex].CurrentSectionLimit) {
                                Game.AddMessage(Interface.GetInterfaceString("message_signal_overspeed"), Game.MessageDependency.SectionLimit, Game.MessageType.Game, Game.MessageColor.Orange, double.PositiveInfinity);
                            }
                        }
                    }
                }
                if (UpdateSection) {
                    // update sections
                    if (this.NextSectionIndex >= 0) {
                        Game.Sections[this.NextSectionIndex].Enter(TrainIndex);
                        Game.UpdateSection(this.NextSectionIndex);
                    }
                }
            }
            private void UpdateRearBackward(int TrainIndex, bool UpdateTrain, bool UpdateSection) {
                if (UpdateSection) {
                    // update sections
                    if (this.PreviousSectionIndex >= 0) {
                        Game.Sections[this.PreviousSectionIndex].Enter(TrainIndex);
                        Game.UpdateSection(this.PreviousSectionIndex);
                    }
                }
                if (UpdateTrain) {
                    // update train
                    if (this.PreviousSectionIndex >= 0) {
                        if (!Game.Sections[this.PreviousSectionIndex].Invisible) {
                            if (Game.Sections[this.PreviousSectionIndex].CurrentAspect >= 0) {
                                TrainManager.Trains[TrainIndex].CurrentSectionLimit = Game.Sections[this.PreviousSectionIndex].Aspects[Game.Sections[this.PreviousSectionIndex].CurrentAspect].Speed;
                            } else {
                                TrainManager.Trains[TrainIndex].CurrentSectionLimit = double.PositiveInfinity;
                            }
                            TrainManager.Trains[TrainIndex].CurrentSectionIndex = this.PreviousSectionIndex;
                        }
                        if (TrainManager.Trains[TrainIndex].Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
                            int a = Game.Sections[this.PreviousSectionIndex].CurrentAspect;
                            if (a >= 0) {
                                PluginManager.UpdateSignal(Game.Sections[this.PreviousSectionIndex].Aspects[a].Number);
                            } else {
                                PluginManager.UpdateSignal(255);
                            }
                        }
                    } else {
                        TrainManager.Trains[TrainIndex].CurrentSectionLimit = double.PositiveInfinity;
                        TrainManager.Trains[TrainIndex].CurrentSectionIndex = -1;
                    }
                }
            }
            private void UpdateRearForward(int TrainIndex) {
                // update sections
                if (this.PreviousSectionIndex >= 0) {
                    Game.Sections[this.PreviousSectionIndex].Leave(TrainIndex);
                    Game.UpdateSection(this.PreviousSectionIndex);
                }
                if (this.NextSectionIndex >= 0) {
                    Game.Sections[this.NextSectionIndex].Enter(TrainIndex);
                    Game.UpdateSection(this.NextSectionIndex);
                }
            }
        }
        // transponder
        internal enum TransponderType {
            None = -1,
            S = 0,
            Sn = 1,
            AccidentalDeparture = 2,
            AtsPPatternOrigin = 3,
            AtsPImmediateStop = 4,
            AtsPTemporarySpeedRestriction = -2,
            AtsPPermanentSpeedRestriction = -3
        }
        internal enum TransponderSpecialSection : int {
            NextRedSection = -2,
        }
        internal class TransponderEvent : GeneralEvent {
            internal TransponderType Type;
            internal bool SwitchSubsystem;
            internal int OptionalInteger;
            internal double OptionalFloat;
            internal int SectionIndex;
            internal TransponderEvent(double TrackPositionDelta, TransponderType Type, bool SwitchSubsystem, int OptionalInteger, double OptionalFloat, int SectionIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.Type = Type;
                this.SwitchSubsystem = SwitchSubsystem;
                this.OptionalInteger = OptionalInteger;
                this.OptionalFloat = OptionalFloat;
                this.SectionIndex = SectionIndex;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                    if (TrainManager.Trains[TrainIndex].Specs.Security.Mode != TrainManager.SecuritySystem.Bve4Plugin) {
                        if (this.Type == TransponderType.AccidentalDeparture) {
                            if (TrainManager.Trains[TrainIndex].Station == -1) return;
                            if (TrainManager.Trains[TrainIndex].StationStopDifference >= 0.0) return;
                        }
                        //} else if (this.Type == TransponderType.AtsPPatternOrigin | this.Type == TransponderType.AtsPImmediateStop) {
                        //    int s = this.SectionIndex;
                        //    if (s >= 0) {
                        //        s = Game.Sections[s].PreviousSection;
                        //        while (true) {
                        //            if (s == -1) break;
                        //            if (Game.Sections[s].Exists(TrainIndex)) break;
                        //            int a = Game.Sections[s].CurrentAspect;
                        //            if (a >= 0) {
                        //                if (Game.Sections[s].Aspects[a].Number == 0) return;
                        //            }
                        //            s = Game.Sections[s].PreviousSection;
                        //        }
                        //    }
                        //}
                    }
                    int s = this.SectionIndex;
                    if (s >= 0) {
                        while (true) {
                            if (Game.Sections[s].Exists(TrainIndex)) {
                                s = this.SectionIndex;
                                break;
                            }
                            int a = Game.Sections[s].CurrentAspect;
                            if (a >= 0) {
                                if (Game.Sections[s].Aspects[a].Number == 0) {
                                    break;
                                }
                            }
                            s = Game.Sections[s].PreviousSection;
                            if (s < 0) {
                                s = this.SectionIndex;
                                break;
                            }
                        }
                    }
                    TrainManager.TrainPendingTransponder Data;
                    Data.Type = this.Type;
                    Data.SwitchSubsystem = this.SwitchSubsystem;
                    Data.SectionIndex = s;
                    Data.OptionalFloat = this.OptionalFloat;
                    Data.OptionalInteger = this.OptionalInteger;
                    TrainManager.Trains[TrainIndex].Specs.Security.AddPendingTransponder(Data);
                }
            }
        }
        // limit change
        internal class LimitChangeEvent : GeneralEvent {
            internal double PreviousSpeedLimit;
            internal double NextSpeedLimit;
            internal LimitChangeEvent(double TrackPositionDelta, double PreviousSpeedLimit, double NextSpeedLimit) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousSpeedLimit = PreviousSpeedLimit;
                this.NextSpeedLimit = NextSpeedLimit;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (Direction < 0) {
                    if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                        int n = TrainManager.Trains[TrainIndex].RouteLimits.Length;
                        if (n > 0) {
                            Array.Resize<double>(ref TrainManager.Trains[TrainIndex].RouteLimits, n - 1);
                            TrainManager.Trains[TrainIndex].CurrentRouteLimit = double.PositiveInfinity;
                            for (int i = 0; i < n - 1; i++) {
                                if (TrainManager.Trains[TrainIndex].RouteLimits[i] < TrainManager.Trains[TrainIndex].CurrentRouteLimit) {
                                    TrainManager.Trains[TrainIndex].CurrentRouteLimit = TrainManager.Trains[TrainIndex].RouteLimits[i];
                                }
                            }
                        }
                    } else if (TriggerType == EventTriggerType.RearCarRearAxle) {
                        int n = TrainManager.Trains[TrainIndex].RouteLimits.Length;
                        Array.Resize<double>(ref TrainManager.Trains[TrainIndex].RouteLimits, n + 1);
                        for (int i = n; i > 0; i--) {
                            TrainManager.Trains[TrainIndex].RouteLimits[i] = TrainManager.Trains[TrainIndex].RouteLimits[i - 1];
                        }
                        TrainManager.Trains[TrainIndex].RouteLimits[0] = this.PreviousSpeedLimit;
                    }
                } else if (Direction > 0) {
                    if (TriggerType == EventTriggerType.FrontCarFrontAxle) {
                        int n = TrainManager.Trains[TrainIndex].RouteLimits.Length;
                        Array.Resize<double>(ref TrainManager.Trains[TrainIndex].RouteLimits, n + 1);
                        TrainManager.Trains[TrainIndex].RouteLimits[n] = this.NextSpeedLimit;
                        if (this.NextSpeedLimit < TrainManager.Trains[TrainIndex].CurrentRouteLimit) {
                            TrainManager.Trains[TrainIndex].CurrentRouteLimit = this.NextSpeedLimit;
                        }
                        if (TrainIndex == TrainManager.PlayerTrain & TrainManager.Trains[TrainIndex].Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                            if (TrainManager.Trains[TrainIndex].Specs.CurrentAverageSpeed > this.NextSpeedLimit) {
                                Game.AddMessage(Interface.GetInterfaceString("message_route_overspeed"), Game.MessageDependency.RouteLimit, Game.MessageType.Game, Game.MessageColor.Orange, double.PositiveInfinity);
                            }
                        }
                    } else if (TriggerType == EventTriggerType.RearCarRearAxle) {
                        int n = TrainManager.Trains[TrainIndex].RouteLimits.Length;
                        if (n > 0) {
                            TrainManager.Trains[TrainIndex].CurrentRouteLimit = double.PositiveInfinity;
                            for (int i = 0; i < n - 1; i++) {
                                TrainManager.Trains[TrainIndex].RouteLimits[i] = TrainManager.Trains[TrainIndex].RouteLimits[i + 1];
                                if (TrainManager.Trains[TrainIndex].RouteLimits[i] < TrainManager.Trains[TrainIndex].CurrentRouteLimit) {
                                    TrainManager.Trains[TrainIndex].CurrentRouteLimit = TrainManager.Trains[TrainIndex].RouteLimits[i];
                                }
                            } Array.Resize<double>(ref TrainManager.Trains[TrainIndex].RouteLimits, n - 1);
                        }
                    }
                }
            }
        }
        // sound
        internal static bool SuppressSoundEvents = false;
        internal class SoundEvent : GeneralEvent {
            internal int SoundIndex;
            internal int TrainIndex;
            internal int CarIndex;
            internal bool Once;
            internal bool Dynamic;
            internal World.Vector3D Position;
            internal double Speed;
            internal SoundEvent(double TrackPositionDelta, int SoundIndex, int TrainIndex, int CarIndex, bool Once, bool Dynamic, World.Vector3D Position, double Speed) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.SoundIndex = SoundIndex;
                this.TrainIndex = TrainIndex;
                this.CarIndex = CarIndex;
                this.Once = Once;
                this.Dynamic = Dynamic;
                this.Position = Position;
                this.Speed = Speed;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (SuppressSoundEvents) return;
                if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle | TriggerType == EventTriggerType.OtherCarRearAxle | TriggerType == EventTriggerType.RearCarRearAxle) {
                    int t = this.TrainIndex;
                    if (t == int.MinValue & TrainIndex == TrainManager.PlayerTrain) t = TrainIndex;
                    if (t == TrainIndex | t == -1) {
                        World.Vector3D p = this.Position;
                        double pitch = 1.0;
                        double gain = 1.0;
                        int i = this.SoundIndex;
                        switch (i) {
                            case SoundIndexTrainPoint:
                                if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
                                    i = TrainManager.Trains[t].Cars[CarIndex].Sounds.PointFrontAxle.SoundBufferIndex;
                                    p = TrainManager.Trains[t].Cars[CarIndex].Sounds.PointFrontAxle.Position;
                                } else {
                                    i = TrainManager.Trains[t].Cars[CarIndex].Sounds.PointRearAxle.SoundBufferIndex;
                                    p = TrainManager.Trains[t].Cars[CarIndex].Sounds.PointRearAxle.Position;
                                } break;
                        }
                        if (i >= 0) {
                            SoundManager.Importance q;
                            if (this.Dynamic) {
                                t = TrainIndex;
                                double spd = Math.Abs(TrainManager.Trains[t].Specs.CurrentAverageSpeed);
                                pitch = spd / this.Speed;
                                gain = pitch < 0.5 ? 2.0 * pitch : 1.0;
                                if (pitch < 0.2 | gain < 0.2) i = -1;
                                q = SoundManager.Importance.DontCare;
                            } else {
                                q = SoundManager.Importance.AlwaysPlay;
                            }
                            if (i >= 0) {
                                SoundManager.PlaySound(i, t, CarIndex, p, q, false, pitch, gain);
                            }
                        }
                        this.DontTriggerAnymore = this.Once;
                    }
                }
            }
            internal const int SoundIndexTrainPoint = -2;
        }
        // rail sounds change
        internal class RailSoundsChangeEvent : GeneralEvent {
            internal int PreviousRunIndex;
            internal int PreviousFlangeIndex;
            internal int NextRunIndex;
            internal int NextFlangeIndex;
            internal RailSoundsChangeEvent(double TrackPositionDelta, int PreviousRunIndex, int PreviousFlangeIndex, int NextRunIndex, int NextFlangeIndex) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
                this.PreviousRunIndex = PreviousRunIndex;
                this.PreviousFlangeIndex = PreviousFlangeIndex;
                this.NextRunIndex = NextRunIndex;
                this.NextFlangeIndex = NextFlangeIndex;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.FrontCarFrontAxle | TriggerType == EventTriggerType.OtherCarFrontAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.FrontAxleRunIndex = this.PreviousRunIndex;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.PreviousFlangeIndex;
                    } else if (Direction > 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.FrontAxleRunIndex = this.NextRunIndex;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.FrontAxleFlangeIndex = this.NextFlangeIndex;
                    }
                } else if (TriggerType == EventTriggerType.RearCarRearAxle | TriggerType == EventTriggerType.OtherCarRearAxle) {
                    if (Direction < 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.RearAxleRunIndex = this.PreviousRunIndex;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.PreviousFlangeIndex;
                    } else if (Direction > 0) {
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.RearAxleRunIndex = this.NextRunIndex;
                        TrainManager.Trains[TrainIndex].Cars[CarIndex].Sounds.RearAxleFlangeIndex = this.NextFlangeIndex;
                    }
                }
            }
        }
        // track end
        internal class TrackEndEvent : GeneralEvent {
            internal TrackEndEvent(double TrackPositionDelta) {
                this.TrackPositionDelta = TrackPositionDelta;
                this.DontTriggerAnymore = false;
            }
            internal override void Trigger(int Direction, EventTriggerType TriggerType, int TrainIndex, int CarIndex) {
                if (TriggerType == EventTriggerType.RearCarRearAxle & TrainIndex != TrainManager.PlayerTrain) {
                    TrainManager.DisposeTrain(TrainManager.Trains[TrainIndex]);
                }
            }
        }

        // ================================

        // track element
        internal struct TrackElement {
            internal double StartingTrackPosition;
            internal double PlanarCurveRadius;
            internal double PlanarCurveCant;
            internal double Inaccuracy;
            internal double AdhesionMultiplier;
            internal World.Vector3D WorldPosition;
            internal World.Vector3D WorldDirection;
            internal World.Vector3D WorldUp;
            internal World.Vector3D WorldSide;
            internal GeneralEvent[] Events;
            internal TrackElement(double StartingTrackPosition) {
                this.StartingTrackPosition = StartingTrackPosition;
                this.PlanarCurveRadius = 0.0;
                this.PlanarCurveCant = 0.0;
                this.Inaccuracy = 0.0;
                this.AdhesionMultiplier = 1.0;
                this.WorldPosition = new World.Vector3D(0.0, 0.0, 0.0);
                this.WorldDirection = new World.Vector3D(0.0, 0.0, 1.0);
                this.WorldUp = new World.Vector3D(0.0, 1.0, 0.0);
                this.WorldSide = new World.Vector3D(1.0, 0.0, 0.0);
                this.Events = new GeneralEvent[] { };
            }
        }

        // track
        internal struct Track {
            internal TrackElement[] Elements;
        }
        internal static Track CurrentTrack;

        // track follower
        internal struct TrackFollower {
            internal int LastTrackElement;
            internal double TrackPosition;
            internal World.Vector3D WorldPosition;
            internal World.Vector3D WorldDirection;
            internal World.Vector3D WorldUp;
            internal World.Vector3D WorldSide;
            internal double CurveRadius;
            internal double CurveCant;
            internal double AdhesionMultiplier;
            internal EventTriggerType TriggerType;
            internal int CarIndex;
            internal int TrainIndex;
        }
        internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) {
            if (CurrentTrack.Elements.Length == 0) return;
            int i = Follower.LastTrackElement;
            while (i >= 0 && NewTrackPosition < CurrentTrack.Elements[i].StartingTrackPosition) {
                double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
                double tb = -0.01;
                CheckEvents(ref Follower, i, -1, ta, tb);
                i--;
            }
            if (i >= 0) {
                while (i < CurrentTrack.Elements.Length - 1) {
                    if (NewTrackPosition < CurrentTrack.Elements[i + 1].StartingTrackPosition) break;
                    double ta = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
                    double tb = CurrentTrack.Elements[i + 1].StartingTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition + 0.01;
                    CheckEvents(ref Follower, i, 1, ta, tb);
                    i++;
                }
            } else i = 0;
            double da = Follower.TrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
            double db = NewTrackPosition - CurrentTrack.Elements[i].StartingTrackPosition;
            // track
            if (UpdateWorldCoordinates) {
                if (db != 0.0) {
                    if (CurrentTrack.Elements[i].PlanarCurveRadius != 0.0) {
                        // side curve
                        double r = CurrentTrack.Elements[i].PlanarCurveRadius;
                        double p = CurrentTrack.Elements[i].WorldDirection.Y / Math.Sqrt(CurrentTrack.Elements[i].WorldDirection.X * CurrentTrack.Elements[i].WorldDirection.X + CurrentTrack.Elements[i].WorldDirection.Z * CurrentTrack.Elements[i].WorldDirection.Z);
                        double s = db / Math.Sqrt(1.0 + p * p);
                        double h = s * p;
                        double b = s / Math.Abs(r);
                        double f = 2.0 * r * r * (1.0 - Math.Cos(b));
                        double c = (double)Math.Sign(db) * Math.Sqrt(f >= 0.0 ? f : 0.0);
                        double a = 0.5 * (double)Math.Sign(r) * b;
                        World.Vector3D D = new World.Vector3D(CurrentTrack.Elements[i].WorldDirection.X, 0.0, CurrentTrack.Elements[i].WorldDirection.Z);
                        World.Normalize(ref D.X, ref D.Y, ref D.Z);
                        double cosa = Math.Cos(a);
                        double sina = Math.Sin(a);
                        World.Rotate(ref D.X, ref D.Y, ref D.Z, 0.0, 1.0, 0.0, cosa, sina);
                        Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + c * D.X;
                        Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + h;
                        Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + c * D.Z;
                        World.Rotate(ref D.X, ref D.Y, ref D.Z, 0.0, 1.0, 0.0, cosa, sina);
                        Follower.WorldDirection.X = D.X;
                        Follower.WorldDirection.Y = p;
                        Follower.WorldDirection.Z = D.Z;
                        World.Normalize(ref Follower.WorldDirection.X, ref Follower.WorldDirection.Y, ref Follower.WorldDirection.Z);
                        double cos2a = Math.Cos(2.0 * a);
                        double sin2a = Math.Sin(2.0 * a);
                        Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
                        World.Rotate(ref Follower.WorldSide.X, ref Follower.WorldSide.Y, ref Follower.WorldSide.Z, 0.0, 1.0, 0.0, cos2a, sin2a);
                        World.Cross(Follower.WorldDirection.X, Follower.WorldDirection.Y, Follower.WorldDirection.Z, Follower.WorldSide.X, Follower.WorldSide.Y, Follower.WorldSide.Z, out Follower.WorldUp.X, out Follower.WorldUp.Y, out Follower.WorldUp.Z);
                        Follower.CurveRadius = CurrentTrack.Elements[i].PlanarCurveRadius;
                        Follower.CurveCant = CurrentTrack.Elements[i].PlanarCurveCant;
                    } else {
                        // straight
                        Follower.WorldPosition.X = CurrentTrack.Elements[i].WorldPosition.X + db * CurrentTrack.Elements[i].WorldDirection.X;
                        Follower.WorldPosition.Y = CurrentTrack.Elements[i].WorldPosition.Y + db * CurrentTrack.Elements[i].WorldDirection.Y;
                        Follower.WorldPosition.Z = CurrentTrack.Elements[i].WorldPosition.Z + db * CurrentTrack.Elements[i].WorldDirection.Z;
                        Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
                        Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
                        Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
                        Follower.CurveRadius = 0.0;
                        Follower.CurveCant = CurrentTrack.Elements[i].PlanarCurveCant;
                    }
                } else {
                    Follower.WorldPosition = CurrentTrack.Elements[i].WorldPosition;
                    Follower.WorldDirection = CurrentTrack.Elements[i].WorldDirection;
                    Follower.WorldUp = CurrentTrack.Elements[i].WorldUp;
                    Follower.WorldSide = CurrentTrack.Elements[i].WorldSide;
                    Follower.CurveRadius = CurrentTrack.Elements[i].PlanarCurveRadius;
                    Follower.CurveCant = CurrentTrack.Elements[i].PlanarCurveCant;
                }
            }
            Follower.AdhesionMultiplier = CurrentTrack.Elements[i].AdhesionMultiplier;
            if (AddTrackInaccurary) {
                double f = NewTrackPosition;
                f = 0.3121 * Math.Sin(0.9843 * f) + 1.1217 * Math.Sin(0.1874 * f) + 1.6421 * Math.Sin(0.1126 * f) + 1.8421 * Math.Sin(0.2546 * f);
                f *= 0.15 * CurrentTrack.Elements[i].Inaccuracy;
                double g = NewTrackPosition;
                g = 0.3495 * Math.Sin(0.8272 * g) + 1.6321 * Math.Sin(0.2356 * g);
                g *= 0.15 * CurrentTrack.Elements[i].Inaccuracy;
                Follower.WorldPosition.X += f * Follower.WorldSide.X + g * Follower.WorldUp.X;
                Follower.WorldPosition.Y += f * Follower.WorldSide.Y + g * Follower.WorldUp.Y;
                Follower.WorldPosition.Z += f * Follower.WorldSide.Z + g * Follower.WorldUp.Z;
            }
            // events
            CheckEvents(ref Follower, i, Math.Sign(db - da), da, db);
            // finish
            Follower.TrackPosition = NewTrackPosition;
            Follower.LastTrackElement = i;
        }

        // check events
        private static void CheckEvents(ref TrackFollower Follower, int ElementIndex, int Direction, double OldDelta, double NewDelta) {
            if (Direction < 0) {
                for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++) {
                    if (OldDelta > CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta <= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
                        TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], -1, Follower.TriggerType, Follower.TrainIndex, Follower.CarIndex);
                    }
                }
            } else if (Direction > 0) {
                for (int j = 0; j < CurrentTrack.Elements[ElementIndex].Events.Length; j++) {
                    if (OldDelta < CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta & NewDelta >= CurrentTrack.Elements[ElementIndex].Events[j].TrackPositionDelta) {
                        TryTriggerEvent(CurrentTrack.Elements[ElementIndex].Events[j], 1, Follower.TriggerType, Follower.TrainIndex, Follower.CarIndex);
                    }
                }
            }
        }

    }
}