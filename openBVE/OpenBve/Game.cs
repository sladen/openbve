using System;

namespace OpenBve {
    internal static class Game {

        // random numbers
        internal static Random Generator = new Random();

        // game mode
        internal enum GameMode {
            Arcade = 0,
            Normal = 1,
            Expert = 2
        }
        internal static GameMode CurrentMode = GameMode.Normal;

        // date and time
        internal static double SecondsSinceMidnight = 0.0;
        internal static double StartupTime = 0.0;
        internal static bool MinimalisticSimulation = false;

        // fog
        internal struct Fog {
            internal float Start;
            internal float End;
            internal World.ColorRGB Color;
            internal double TrackPosition;
            internal Fog(float Start, float End, World.ColorRGB Color, double TrackPosition) {
                this.Start = Start;
                this.End = End;
                this.Color = Color;
                this.TrackPosition = TrackPosition;
            }
        }
        internal static Fog PreviousFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.0);
        internal static Fog CurrentFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.5);
        internal static Fog NextFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 1.0);

        // route constants
        internal static string RouteComment = "";
        internal static string RouteImage = "";
        internal static double RouteAccelerationDueToGravity = 9.80665;
        internal static double RouteRailGauge = 1.435;
        internal static double RouteInitialAirPressure = 101325.0;
        internal static double RouteInitialAirTemperature = 293.15;
        internal static double RouteInitialElevation = 0.0;
        internal static double RouteSeaLevelAirPressure = 101325.0;
        internal static double RouteSeaLevelAirTemperature = 293.15;
        internal const double CoefficientOfGroundFriction = 0.2;
        internal const double CriticalCollisionSpeedDifference = 8.0;
        internal const double BrakePipeLeakRate = 500000.0;
        internal const double MolarMass = 0.0289644;
        internal const double UniversalGasConstant = 8.31447;
        internal const double TemperatureLapseRate = -0.0065;
        internal const double CoefficientOfStiffness = 144117.325646911;

        // athmospheric functions
        internal static void CalculateSeaLevelConstants() {
            RouteSeaLevelAirTemperature = RouteInitialAirTemperature - TemperatureLapseRate * RouteInitialElevation;
            double Exponent = RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
            double Base = 1.0 + TemperatureLapseRate * RouteInitialElevation / RouteSeaLevelAirTemperature;
            if (Base >= 0.0) {
                RouteSeaLevelAirPressure = RouteInitialAirPressure * Math.Pow(Base, Exponent);
                if (RouteSeaLevelAirPressure < 0.001) RouteSeaLevelAirPressure = 0.001;
            } else {
                RouteSeaLevelAirPressure = 0.001;
            }
        }
        internal static double GetAirTemperature(double Elevation) {
            double x = RouteSeaLevelAirTemperature + TemperatureLapseRate * Elevation;
            if (x >= 1.0) {
                return x;
            } else return 1.0;
        }
        internal static double GetAirDensity(double AirPressure, double AirTemperature) {
            double x = AirPressure * MolarMass / (UniversalGasConstant * AirTemperature);
            if (x >= 0.001) {
                return x;
            } else return 0.001;
        }
        internal static double GetAirPressure(double Elevation, double AirTemperature) {
            double Exponent = -RouteAccelerationDueToGravity * MolarMass / (UniversalGasConstant * TemperatureLapseRate);
            double Base = 1.0 + TemperatureLapseRate * Elevation / RouteSeaLevelAirTemperature;
            if (Base >= 0.0) {
                double x = RouteSeaLevelAirPressure * Math.Pow(Base, Exponent);
                if (x >= 0.001) {
                    return x;
                } return 0.001;
            } else return 0.001;
        }
        internal static double GetSpeedOfSound(double AirPressure, double AirTemperature) {
            double AirDensity = GetAirDensity(AirPressure, AirTemperature);
            return Math.Sqrt(CoefficientOfStiffness / AirDensity);
        }

        // game constants
        internal static int PretrainsUsed = 0;
        internal static double PretrainInterval = 120.0;

        // startup
        internal enum TrainStartMode {
            ServiceBrakesAts = -1,
            EmergencyBrakesAts = 0,
            EmergencyBrakesNoAts = 1
        }
        internal static TrainStartMode TrainStart = TrainStartMode.EmergencyBrakesNoAts;
        internal static string TrainName = "";

        // information
        internal enum OutputMode {
            Default = 0,
            Fps = 1,
            Debug = 2,
            None = 3
        }
        internal const int OutputModeCount = 4;
        internal static OutputMode InfoOutputMode = OutputMode.Default;
        internal static double InfoFrameRate = 1.0;
        internal static int InfoTexturesRegistered = 0;
        internal static int InfoTexturesLoaded = 0;
        internal static int InfoSoundSourcesRegistered = 0;
        internal static int InfoSoundSourcesPlaying = 0;
        internal static string InfoDebugString = "";

        // ================================

        // interface type
        internal enum InterfaceType { Normal, Pause, Menu }
        internal static InterfaceType CurrentInterface = InterfaceType.Normal;
        internal enum MenuTag { None, Caption, Back, JumpToStation, ExitToMainMenu, Quit }
        internal abstract class MenuEntry {
            internal string Text;
            internal double Highlight;
            internal double Alpha;
        }
        internal class MenuCaption : MenuEntry {
            internal MenuCaption(string Text) {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
            }
        }
        internal class MenuCommand : MenuEntry {
            internal MenuTag Tag;
            internal int Data;
            internal MenuCommand(string Text, MenuTag Tag, int Data) {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
                this.Tag = Tag;
                this.Data = Data;
            }
        }
        internal class MenuSubmenu : MenuEntry {
            internal MenuEntry[] Entries;
            internal MenuSubmenu(string Text, MenuEntry[] Entries) {
                this.Text = Text;
                this.Highlight = 0.0;
                this.Alpha = 0.0;
                this.Entries = Entries;
            }
        }
        internal static MenuEntry[] CurrentMenu = new MenuEntry[] { };
        internal static int[] CurrentMenuSelection = new int[] { -1 };
        internal static double[] CurrentMenuOffsets = new double[] { double.NegativeInfinity };
        internal static void CreateMenu(bool QuitOnly) {
            if (QuitOnly) {
                // quit menu only
                CurrentMenu = new MenuEntry[3];
                CurrentMenu[0] = new MenuCaption(Interface.GetInterfaceString("menu_quit_question"));
                CurrentMenu[1] = new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0);
                CurrentMenu[2] = new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0);
                CurrentMenuSelection = new int[] { 1 };
                CurrentMenuOffsets = new double[] { double.NegativeInfinity };
                CurrentMenu[1].Highlight = 1.0;
            } else {
                // full menu
                int n = 0;
                for (int i = 0; i < Stations.Length; i++) {
                    if (Stations[i].StopAtStation & Stations[i].Stops.Length > 0) {
                        n++;
                    }
                }
                MenuEntry[] a = new MenuCommand[n];
                n = 0;
                for (int i = 0; i < Stations.Length; i++) {
                    if (Stations[i].StopAtStation & Stations[i].Stops.Length > 0) {
                        a[n] = new MenuCommand(Stations[i].Name, MenuTag.JumpToStation, i);
                        n++;
                    }
                }
                CurrentMenu = new MenuEntry[4];
                CurrentMenu[0] = new MenuCommand(Interface.GetInterfaceString("menu_resume"), MenuTag.Back, 0);
                CurrentMenu[1] = new MenuSubmenu(Interface.GetInterfaceString("menu_jump"), a);
                CurrentMenu[2] = new MenuSubmenu(Interface.GetInterfaceString("menu_exit"), new MenuEntry[] {
                    new MenuCaption(Interface.GetInterfaceString("menu_exit_question")),
                    new MenuCommand(Interface.GetInterfaceString("menu_exit_no"), MenuTag.Back, 0),
                    new MenuCommand(Interface.GetInterfaceString("menu_exit_yes"), MenuTag.ExitToMainMenu, 0)
                });
                CurrentMenu[3] = new MenuSubmenu(Interface.GetInterfaceString("menu_quit"), new MenuEntry[] {
                    new MenuCaption(Interface.GetInterfaceString("menu_quit_question")),
                    new MenuCommand(Interface.GetInterfaceString("menu_quit_no"), MenuTag.Back, 0),
                    new MenuCommand(Interface.GetInterfaceString("menu_quit_yes"), MenuTag.Quit, 0)
                });
                CurrentMenuSelection = new int[] { 0 };
                CurrentMenuOffsets = new double[] { double.NegativeInfinity };
                CurrentMenu[0].Highlight = 1.0;
            }
        }

        // ================================

        internal static void Reset(bool ResetLogs) {
            // track manager
            TrackManager.CurrentTrack = new TrackManager.Track();
            // train manager
            TrainManager.Trains = new TrainManager.Train[] { };
            // game
            Interface.ClearMessages();
            CurrentInterface = InterfaceType.Normal;
            RouteComment = "";
            RouteImage = "";
            RouteAccelerationDueToGravity = 9.80665;
            RouteRailGauge = 1.435;
            RouteInitialAirPressure = 101325.0;
            RouteInitialAirTemperature = 293.15;
            RouteInitialElevation = 0.0;
            RouteSeaLevelAirPressure = 101325.0;
            RouteSeaLevelAirTemperature = 293.15;
            Stations = new Station[] { };
            Sections = new Section[] { };
            Signals = new Signal[] { };
            BufferTrackPositions = new double[] { };
            Messages = new Message[] { };
            MarkerTextures = new int[] { };
            PointsOfInterest = new PointOfInterest[] { };
            BogusPretrainInstructions = new BogusPretrainInstruction[] { };
            TrainName = "";
            TrainStart = TrainStartMode.EmergencyBrakesNoAts;
            PreviousFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.0);
            CurrentFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 0.5);
            NextFog = new Fog(0.0f, 0.0f, new World.ColorRGB(128, 128, 128), 1.0);
            if (ResetLogs) {
                LogRouteName = "";
                LogTrainName = "";
                LogDateTime = DateTime.Now;
                CurrentScore = new Score();
                ScoreMessages = new ScoreMessage[] { };
                ScoreLogs = new ScoreLog[64];
                ScoreLogCount = 0;
                BlackBoxEntries = new BlackBoxEntry[256];
                BlackBoxEntryCount = 0;
                BlackBoxNextUpdate = 0.0;
            }
            // renderer
            Renderer.Reset();
        }

        // ================================

        // score
        internal struct Score {
            internal int Value;
            internal int Maximum;
            internal double OpenedDoorsCounter;
            internal double OverspeedCounter;
            internal double TopplingCounter;
            internal bool RedSignal;
            internal bool Derailed;
            internal int ArrivalStation;
            internal int DepartureStation;
            internal double PassengerTimer;
        }
        internal static Score CurrentScore = new Score();
        private const double ScoreFactorOpenedDoors = -10.0;
        private const double ScoreFactorOverspeed = -1.0;
        private const double ScoreFactorToppling = -10.0;
        private const double ScoreFactorStationLate = -0.333333333333333;
        private const double ScoreFactorStationStop = -50.0;
        private const double ScoreFactorStationDeparture = -1.5;
        private const int ScoreValueDerailment = -1000;
        private const int ScoreValueRedSignal = -100;
        private const int ScoreValueStationPerfectTime = 15;
        private const int ScoreValueStationPerfectStop = 15;
        private const int ScoreValuePassengerDiscomfort = -20;
        internal const int ScoreValueStationArrival = 100;
        internal static void UpdateScore(double TimeElapsed) {
            int i = TrainManager.PlayerTrain;
            // doors
            {
                bool leftopen = false;
                bool rightopen = false;
                for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
                    int k; for (k = 0; k < TrainManager.Trains[i].Cars[j].Specs.Doors.Length; k++) {
                        if (TrainManager.Trains[i].Cars[j].Specs.Doors[k].State != 0.0) {
                            if (TrainManager.Trains[i].Cars[j].Specs.Doors[k].Direction == -1) {
                                leftopen = true;
                            } else if (TrainManager.Trains[i].Cars[j].Specs.Doors[k].Direction == 1) {
                                rightopen = true;
                            }
                        }
                    }
                }
                bool bad;
                if (leftopen | rightopen) {
                    bad = true;
                    int j = TrainManager.Trains[i].Station;
                    if (j >= 0) {
                        int p = Game.GetStopIndex(j, TrainManager.Trains[i].Cars.Length);
                        if (p >= 0) {
                            if (Math.Abs(TrainManager.Trains[i].Specs.CurrentAverageSpeed) < 0.1) {
                                if (leftopen == Stations[j].OpenLeftDoors & rightopen == Stations[j].OpenRightDoors) {
                                    bad = false;
                                }
                            }
                        }
                    }
                } else {
                    bad = false;
                }
                if (bad) {
                    CurrentScore.OpenedDoorsCounter += (Math.Abs(TrainManager.Trains[i].Specs.CurrentAverageSpeed) + 0.25) * TimeElapsed;
                } else if (CurrentScore.OpenedDoorsCounter != 0.0) {
                    int x = (int)Math.Ceiling(ScoreFactorOpenedDoors * CurrentScore.OpenedDoorsCounter);
                    CurrentScore.Value += x;
                    if (x != 0) {
                        AddScore(x, ScoreTextToken.DoorsOpened, 5.0);
                    }
                    CurrentScore.OpenedDoorsCounter = 0.0;
                }
            }
            // overspeed
            if (TrainManager.Trains[i].Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                double nr = TrainManager.Trains[i].CurrentRouteLimit;
                double ns = TrainManager.Trains[i].CurrentSectionLimit;
                double n = nr < ns ? nr : ns;
                double a = Math.Abs(TrainManager.Trains[i].Specs.CurrentAverageSpeed) - 0.277777777777778;
                if (a > n) {
                    CurrentScore.OverspeedCounter += (a - n) * TimeElapsed;
                } else if (CurrentScore.OverspeedCounter != 0.0) {
                    int x = (int)Math.Ceiling(ScoreFactorOverspeed * CurrentScore.OverspeedCounter);
                    CurrentScore.Value += x;
                    if (x != 0) {
                        AddScore(x, ScoreTextToken.Overspeed, 5.0);
                    }
                    CurrentScore.OverspeedCounter = 0.0;
                }
            }
            // toppling
            {
                bool q = false;
                for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
                    if (TrainManager.Trains[i].Cars[j].Topples) {
                        q = true;
                        break;
                    }
                }
                if (q) {
                    CurrentScore.TopplingCounter += TimeElapsed;
                } else if (CurrentScore.TopplingCounter != 0.0) {
                    int x = (int)Math.Ceiling(ScoreFactorToppling * CurrentScore.TopplingCounter);
                    CurrentScore.Value += x;
                    if (x != 0) {
                        AddScore(x, ScoreTextToken.Toppling, 5.0);
                    }
                    CurrentScore.TopplingCounter = 0.0;
                }
            }
            // derailment
            if (!CurrentScore.Derailed) {
                bool q = false;
                for (int j = 0; j < TrainManager.Trains[i].Cars.Length; j++) {
                    if (TrainManager.Trains[i].Cars[j].Derailed) {
                        q = true;
                        break;
                    }
                }
                if (q) {
                    int x = ScoreValueDerailment;
                    if (CurrentScore.Value > 0) x -= CurrentScore.Value;
                    CurrentScore.Value += x;
                    if (x != 0) {
                        AddScore(x, ScoreTextToken.Derailed, 5.0);
                    }
                    CurrentScore.Derailed = true;
                }
            }
            // red signal
            {
                if (TrainManager.Trains[i].CurrentSectionLimit == 0.0 & TrainManager.Trains[i].Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                    if (!CurrentScore.RedSignal) {
                        int x = ScoreValueRedSignal;
                        CurrentScore.Value += x;
                        if (x != 0) {
                            AddScore(x, ScoreTextToken.PassedRedSignal, 5.0);
                        }
                        CurrentScore.RedSignal = true;
                    }
                } else {
                    CurrentScore.RedSignal = false;
                }
            }
            // arrival
            {
                int j = TrainManager.Trains[i].Station;
                if (j >= 0 & j < Stations.Length) {
                    if (j >= CurrentScore.ArrivalStation & TrainManager.Trains[i].StationState == TrainManager.TrainStopState.Boarding) {
                        /// arrival
                        int xa = ScoreValueStationArrival;
                        CurrentScore.Value += xa;
                        if (xa != 0) {
                            AddScore(xa, ScoreTextToken.ArrivedAtStation, 10.0);
                        }
                        /// early/late
                        int xb;
                        if (Stations[j].ArrivalTime >= 0) {
                            double d = SecondsSinceMidnight - Stations[j].ArrivalTime;
                            if (d >= -5.0 & d <= 0.0) {
                                xb = ScoreValueStationPerfectTime;
                                CurrentScore.Value += xb;
                                AddScore(xb, ScoreTextToken.PerfectTimeBonus, 10.0);
                            } else if (d > 0.0) {
                                xb = (int)Math.Ceiling(ScoreFactorStationLate * (d - 1.0));
                                CurrentScore.Value += xb;
                                if (xb != 0) {
                                    AddScore(xb, ScoreTextToken.Late, 10.0);
                                }
                            } else {
                                xb = 0;
                            }
                        } else {
                            xb = 0;
                        }
                        /// position
                        int xc;
                        int p = Game.GetStopIndex(j, TrainManager.Trains[i].Cars.Length);
                        if (p >= 0) {
                            double d = TrainManager.Trains[i].StationStopDifference;
                            double r;
                            if (d >= 0) {
                                double t = Stations[j].Stops[p].BackwardTolerance;
                                r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
                            } else {
                                double t = Stations[j].Stops[p].ForwardTolerance;
                                r = (Math.Sqrt(d * d + 1.0) - 1.0) / (Math.Sqrt(t * t + 1.0) - 1.0);
                            }
                            if (r < 0.01) {
                                xc = ScoreValueStationPerfectStop;
                                CurrentScore.Value += xc;
                                AddScore(xc, ScoreTextToken.PerfectStopBonus, 10.0);
                            } else {
                                if (r > 1.0) r = 1.0;
                                r = (r - 0.01) * 1.01010101010101;
                                xc = (int)Math.Ceiling(ScoreFactorStationStop * r);
                                CurrentScore.Value += xc;
                                if (xc != 0) {
                                    AddScore(xc, ScoreTextToken.Stop, 10.0);
                                }
                            }
                        } else {
                            xc = 0;
                        }
                        /// sum
                        if (CurrentMode == GameMode.Arcade) {
                            int xs = xa + xb + xc;
                            AddScore("", 10.0);
                            AddScore(xs, ScoreTextToken.Total, 10.0, false);
                        }
                        /// evaluation
                        if (CurrentMode == GameMode.Arcade) {
                            if (Stations[j].IsTerminalStation) {
                                double y = (double)CurrentScore.Value / (double)CurrentScore.Maximum;
                                if (y < 0.0) y = 0.0;
                                if (y > 1.0) y = 1.0;
                                int k = (int)Math.Floor(y * (double)Interface.RatingsCount);
                                if (k >= Interface.RatingsCount) k = Interface.RatingsCount - 1;
                                System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
                                AddScore(Interface.GetInterfaceString("score_rating"), 20.0);
                                AddScore(Interface.GetInterfaceString("rating_" + k.ToString(Culture)) + " (" + (100.0 * y).ToString("0.00", Culture) + "%)", 20.0);
                            }
                        }
                        /// finalize
                        CurrentScore.DepartureStation = j;
                        CurrentScore.ArrivalStation = j + 1;
                    }
                }
            }
            // departure
            {
                int j = TrainManager.Trains[i].Station;
                if (j >= 0 & j < Stations.Length & j == CurrentScore.DepartureStation) {
                    bool q;
                    if (Stations[j].OpenLeftDoors | Stations[j].OpenRightDoors) {
                        q = TrainManager.Trains[i].StationState == TrainManager.TrainStopState.Completed;
                    } else {
                        q = TrainManager.Trains[i].StationState != TrainManager.TrainStopState.Pending & (TrainManager.Trains[i].Specs.CurrentAverageSpeed < -1.5 | TrainManager.Trains[i].Specs.CurrentAverageSpeed > 1.5);
                    }
                    if (q) {
                        double r = TrainManager.Trains[i].StationDepartureTime - SecondsSinceMidnight;
                        if (r > 0.0) {
                            int x = (int)Math.Ceiling(ScoreFactorStationDeparture * r);
                            CurrentScore.Value += x;
                            if (x != 0) {
                                AddScore(x, ScoreTextToken.PrematureDeparture, 5.0);
                            }
                        }
                        CurrentScore.DepartureStation = -1;
                    }
                }
            }
            // passengers
            if (TrainManager.Trains[i].Passengers.FallenOver & CurrentScore.PassengerTimer == 0.0) {
                int x = ScoreValuePassengerDiscomfort;
                CurrentScore.Value += x;
                AddScore(x, ScoreTextToken.PassengerDiscomfort, 5.0);
                CurrentScore.PassengerTimer = 5.0;
            } else {
                CurrentScore.PassengerTimer -= TimeElapsed;
                if (CurrentScore.PassengerTimer < 0.0) CurrentScore.PassengerTimer = 0.0;
            }
        }

        // score messages and logs
        internal enum ScoreTextToken : short {
            Invalid = 0,
            Overspeed = 1,
            PassedRedSignal = 2,
            Toppling = 3,
            Derailed = 4,
            PassengerDiscomfort = 5,
            DoorsOpened = 6,
            ArrivedAtStation = 7,
            PerfectTimeBonus = 8,
            Late = 9,
            PerfectStopBonus = 10,
            Stop = 11,
            PrematureDeparture = 12,
            Total = 13
        }
        internal struct ScoreMessage {
            internal int Value;
            internal string Text;
            internal double Timeout;
            internal double Alpha;
            internal double RendererPosition;
        }
        internal struct ScoreLog {
            internal int Value;
            internal ScoreTextToken TextToken;
            internal double Position;
            internal double Time;
        }
        internal static ScoreLog[] ScoreLogs = new ScoreLog[64];
        internal static int ScoreLogCount = 0;
        internal static ScoreMessage[] ScoreMessages = new ScoreMessage[] { };
        internal static string LogRouteName = "";
        internal static string LogTrainName = "";
        internal static DateTime LogDateTime = DateTime.Now;
        private static void AddScore(int Value, ScoreTextToken TextToken, double Duration) {
            AddScore(Value, TextToken, Duration, true);
        }
        private static void AddScore(int Value, ScoreTextToken TextToken, double Duration, bool Count) {
            if (CurrentMode == GameMode.Arcade) {
                int n = ScoreMessages.Length;
                Array.Resize<ScoreMessage>(ref ScoreMessages, n + 1);
                ScoreMessages[n].Value = Value;
                ScoreMessages[n].Text = Interface.GetScoreText(TextToken);
                ScoreMessages[n].Timeout = SecondsSinceMidnight + Duration;
                ScoreMessages[n].Alpha = 0.0;
                ScoreMessages[n].RendererPosition = (double)n;
            }
            if (Value != 0 & Count) {
                if (ScoreLogCount == ScoreLogs.Length) {
                    Array.Resize<ScoreLog>(ref ScoreLogs, ScoreLogs.Length << 1);
                }
                ScoreLogs[ScoreLogCount].Value = Value;
                ScoreLogs[ScoreLogCount].TextToken = TextToken;
                ScoreLogs[ScoreLogCount].Position = TrainManager.Trains[TrainManager.PlayerTrain].Cars[0].FrontAxle.Follower.TrackPosition;
                ScoreLogs[ScoreLogCount].Time = SecondsSinceMidnight;
                ScoreLogCount++;
            }
        }
        private static void AddScore(string Text, double Duration) {
            if (CurrentMode == GameMode.Arcade) {
                int n = ScoreMessages.Length;
                Array.Resize<ScoreMessage>(ref ScoreMessages, n + 1);
                ScoreMessages[n].Value = 0;
                ScoreMessages[n].Text = Text;
                ScoreMessages[n].Timeout = SecondsSinceMidnight + Duration;
                ScoreMessages[n].Alpha = 0.0;
                ScoreMessages[n].RendererPosition = (double)n;
            }
        }
        internal static void UpdateScoreMessages(double TimeElapsed) {
            if (CurrentMode == GameMode.Arcade) {
                for (int i = 0; i < ScoreMessages.Length; i++) {
                    if (SecondsSinceMidnight >= ScoreMessages[i].Timeout) {
                        for (int j = i; j < ScoreMessages.Length - 1; j++) {
                            ScoreMessages[j] = ScoreMessages[j + 1];
                        } Array.Resize<ScoreMessage>(ref ScoreMessages, ScoreMessages.Length - 1);
                        i--;
                    } else {
                        if (SecondsSinceMidnight >= ScoreMessages[i].Timeout - 1.0) {
                            ScoreMessages[i].Alpha -= TimeElapsed;
                            if (ScoreMessages[i].Alpha < 0.0) ScoreMessages[i].Alpha = 0.0;
                        } else {
                            ScoreMessages[i].Alpha += TimeElapsed;
                            if (ScoreMessages[i].Alpha > 1.0) ScoreMessages[i].Alpha = 1.0;
                        }
                        double a = (double)i;
                        if (ScoreMessages[i].RendererPosition < a) {
                            double d = a - ScoreMessages[i].RendererPosition;
                            ScoreMessages[i].RendererPosition += (0.25 + d * d) * TimeElapsed;
                            if (ScoreMessages[i].RendererPosition > a) ScoreMessages[i].RendererPosition = a;
                        } else if (ScoreMessages[i].RendererPosition > a) {
                            double d = ScoreMessages[i].RendererPosition - a;
                            ScoreMessages[i].RendererPosition -= (0.25 + d * d) * TimeElapsed;
                            if (ScoreMessages[i].RendererPosition < a) ScoreMessages[i].RendererPosition = a;
                        }
                    }
                }
            }
        }

        // ================================

        // black box
        internal enum BlackBoxEventToken : short {
            None = 0
        }
        internal enum BlackBoxPower : short {
            PowerNull = 0
        }
        internal enum BlackBoxBrake : short {
            BrakeNull = 0,
            Emergency = -1,
            HoldBrake = -2,
            Release = -3,
            Lap = -4,
            Service = -5
        }
        internal struct BlackBoxEntry {
            internal double Time;
            internal double Position;
            internal float Speed;
            internal float Acceleration;
            internal short ReverserDriver;
            internal short ReverserSecurity;
            internal BlackBoxPower PowerDriver;
            internal BlackBoxPower PowerSecurity;
            internal BlackBoxBrake BrakeDriver;
            internal BlackBoxBrake BrakeSecurity;
            internal BlackBoxEventToken EventToken;
        }
        internal static BlackBoxEntry[] BlackBoxEntries = new BlackBoxEntry[256];
        internal static int BlackBoxEntryCount = 0;
        private static double BlackBoxNextUpdate = 0.0;
        internal static void UpdateBlackBox() {
            if (SecondsSinceMidnight >= BlackBoxNextUpdate) {
                AddBlackBoxEntry(BlackBoxEventToken.None);
                BlackBoxNextUpdate = SecondsSinceMidnight + 1.0;
            }
        }
        internal static void AddBlackBoxEntry(BlackBoxEventToken EventToken) {
            if (Interface.CurrentOptions.BlackBox) {
                if (BlackBoxEntryCount >= BlackBoxEntries.Length) {
                    Array.Resize<BlackBoxEntry>(ref BlackBoxEntries, BlackBoxEntries.Length << 1);
                }
                int d = TrainManager.Trains[TrainManager.PlayerTrain].DriverCar;
                BlackBoxEntries[BlackBoxEntryCount].Time = SecondsSinceMidnight;
                BlackBoxEntries[BlackBoxEntryCount].Position = TrainManager.Trains[TrainManager.PlayerTrain].Cars[0].FrontAxle.Follower.TrackPosition;
                BlackBoxEntries[BlackBoxEntryCount].Speed = (float)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageSpeed;
                BlackBoxEntries[BlackBoxEntryCount].Acceleration = (float)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageAcceleration;
                BlackBoxEntries[BlackBoxEntryCount].ReverserDriver = (short)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentReverser.Driver;
                BlackBoxEntries[BlackBoxEntryCount].ReverserSecurity = (short)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentReverser.Actual;
                BlackBoxEntries[BlackBoxEntryCount].PowerDriver = (BlackBoxPower)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentPowerNotch.Driver;
                BlackBoxEntries[BlackBoxEntryCount].PowerSecurity = (BlackBoxPower)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentPowerNotch.Security;
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentHoldBrake.Driver) {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.HoldBrake;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                    switch (TrainManager.Trains[TrainManager.PlayerTrain].Specs.AirBrake.Handle.Driver) {
                        case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Release; break;
                        case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Lap; break;
                        case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Service; break;
                        default: BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = BlackBoxBrake.Emergency; break;
                    }
                } else {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeDriver = (BlackBoxBrake)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentBrakeNotch.Driver;
                }
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Security) {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.Emergency;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentHoldBrake.Actual) {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.HoldBrake;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                    switch (TrainManager.Trains[TrainManager.PlayerTrain].Specs.AirBrake.Handle.Security) {
                        case TrainManager.AirBrakeHandleState.Release: BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.Release; break;
                        case TrainManager.AirBrakeHandleState.Lap: BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.Lap; break;
                        case TrainManager.AirBrakeHandleState.Service: BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.Service; break;
                        default: BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = BlackBoxBrake.Emergency; break;
                    }
                } else {
                    BlackBoxEntries[BlackBoxEntryCount].BrakeSecurity = (BlackBoxBrake)TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentBrakeNotch.Security;
                }
                BlackBoxEntries[BlackBoxEntryCount].EventToken = EventToken;
                BlackBoxEntryCount++;
            }
        }

        // ================================

        // stations
        internal struct StationStop {
            internal double TrackPosition;
            internal double ForwardTolerance;
            internal double BackwardTolerance;
            internal int Cars;
        }
        internal enum SecuritySystem {
            Any = -1,
            Ats = 0,
            Atc = 1
        }
        internal struct Station {
            internal string Name;
            internal double ArrivalTime;
            internal int ArrivalSoundIndex;
            internal double DepartureTime;
            internal int DepartureSoundIndex;
            internal double StopTime;
            internal World.Vector3D SoundOrigin;
            internal bool StopAtStation;
            internal bool IsTerminalStation;
            internal bool ForceStopSignal;
            internal bool OpenLeftDoors;
            internal bool OpenRightDoors;
            internal SecuritySystem SecuritySystem;
            internal StationStop[] Stops;
            internal double PassengerRatio;
            internal int TimetableDaytimeTexture;
            internal int TimetableNighttimeTexture;
        }
        internal static Station[] Stations = new Station[] { };
        internal static int GetStopIndex(int StationIndex, int Cars) {
            int j = -1;
            for (int i = Stations[StationIndex].Stops.Length - 1; i >= 0; i--) {
                if (Cars <= Stations[StationIndex].Stops[i].Cars | Stations[StationIndex].Stops[i].Cars == 0) {
                    j = i;
                }
            }
            if (j == -1) {
                return Stations[StationIndex].Stops.Length - 1;
            } else return j;
        }

        // ================================

        // sections
        internal enum SectionType { ValueBased, IndexBased }
        internal struct SectionAspect {
            internal int Number;
            internal double Speed;
            internal SectionAspect(int Number, double Speed) {
                this.Number = Number;
                this.Speed = Speed;
            }
        }
        internal struct Section {
            internal int PreviousSection;
            internal int NextSection;
            internal int[] TrainIndices;
            internal bool TrainReachedStopPoint;
            internal int StationIndex;
            internal bool Invisible;
            internal int[] SignalIndices;
            internal double TrackPosition;
            internal SectionType Type;
            internal SectionAspect[] Aspects;
            internal int CurrentAspect;
            internal int FreeSections;
            internal void Enter(int TrainIndex) {
                int n = this.TrainIndices.Length;
                for (int i = 0; i < n; i++) {
                    if (this.TrainIndices[i] == TrainIndex) return;
                }
                Array.Resize<int>(ref this.TrainIndices, n + 1);
                this.TrainIndices[n] = TrainIndex;
            }
            internal void Leave(int TrainIndex) {
                int n = this.TrainIndices.Length;
                for (int i = 0; i < n; i++) {
                    if (this.TrainIndices[i] == TrainIndex) {
                        for (int j = i; j < n - 1; j++) {
                            this.TrainIndices[j] = this.TrainIndices[j + 1];
                        }
                        Array.Resize<int>(ref this.TrainIndices, n - 1);
                        return;
                    }
                }
            }
            internal bool Exists(int TrainIndex) {
                for (int i = 0; i < this.TrainIndices.Length; i++) {
                    if (this.TrainIndices[i] == TrainIndex) return true;
                } return false;
            }
        }
        internal static Section[] Sections = new Section[] { };
        internal static void UpdateAllSections() {
            if (Sections.Length != 0) {
                UpdateSection(Sections.Length - 1);
            }
        }
        internal static void UpdateSection(int SectionIndex) {
            // preparations
            int zeroaspect;
            bool settored = false;
            if (Sections[SectionIndex].Type == SectionType.ValueBased) {
                // value-based
                zeroaspect = int.MaxValue;
                for (int i = 0; i < Sections[SectionIndex].Aspects.Length; i++) {
                    if (Sections[SectionIndex].Aspects[i].Number < zeroaspect) {
                        zeroaspect = Sections[SectionIndex].Aspects[i].Number;
                    }
                } if (zeroaspect == int.MaxValue) {
                    zeroaspect = -1;
                }
            } else {
                // index-based
                zeroaspect = 0;
            }
            // hold station departure signal at red
            int d = Sections[SectionIndex].StationIndex;
            if (d >= 0) {
                // look for train in previous blocks
                int l = Sections[SectionIndex].PreviousSection;
                int train = -1;
                while (true) {
                    if (l >= 0) {
                        if (Sections[l].TrainIndices.Length != 0) {
                            train = Sections[l].TrainIndices[0];
                            break;
                        } else {
                            l = Sections[l].PreviousSection;
                        }
                    } else break;
                }
                if (train == -1) {
                    double b = -double.MaxValue;
                    for (int i = 0; i < TrainManager.Trains.Length; i++) {
                        if (!TrainManager.Trains[i].Disposed) {
                            if (TrainManager.Trains[i].PretrainAheadTimetable > b) {
                                b = TrainManager.Trains[i].PretrainAheadTimetable;
                                train = i;
                            }
                        }
                    }
                }
                // set to red where applicable
                if (train >= 0) {
                    if (!Sections[SectionIndex].TrainReachedStopPoint) {
                        if (TrainManager.Trains[train].Station == d) {
                            int c = GetStopIndex(d, TrainManager.Trains[train].Cars.Length);
                            if (c >= 0) {
                                double p0 = TrainManager.Trains[train].Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.Trains[train].Cars[0].FrontAxlePosition + 0.5 * TrainManager.Trains[train].Cars[0].Length;
                                double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;
                                if (p0 >= p1) {
                                    Sections[SectionIndex].TrainReachedStopPoint = true;
                                }
                            } else {
                                Sections[SectionIndex].TrainReachedStopPoint = true;
                            }
                        }
                    }
                    double t = -15.0;
                    if (Stations[d].DepartureTime >= 0.0) {
                        t = Stations[d].DepartureTime - 15.0;
                    } else if (Stations[d].ArrivalTime >= 0.0) {
                        t = Stations[d].ArrivalTime;
                    }
                    if (train == TrainManager.PlayerTrain & Stations[d].IsTerminalStation) {
                        settored = true;
                    } else if (t >= 0.0 & SecondsSinceMidnight < t - TrainManager.Trains[train].PretrainAheadTimetable) {
                        settored = true;
                    } else if (!Sections[SectionIndex].TrainReachedStopPoint) {
                        settored = true;
                    }
                } else if (Stations[d].IsTerminalStation) {
                    settored = true;
                }
            }
            // train in block
            if (Sections[SectionIndex].TrainIndices.Length != 0) {
                settored = true;
            }
            // free sections
            int newaspect = -1;
            if (settored) {
                Sections[SectionIndex].FreeSections = 0;
                newaspect = zeroaspect;
            } else {
                int n = Sections[SectionIndex].NextSection;
                if (n >= 0) {
                    if (Sections[n].FreeSections == -1) {
                        Sections[SectionIndex].FreeSections = -1;
                    } else {
                        Sections[SectionIndex].FreeSections = Sections[n].FreeSections + 1;
                    }
                } else {
                    Sections[SectionIndex].FreeSections = -1;
                }
            }
            // change aspect
            if (newaspect == -1) {
                if (Sections[SectionIndex].Type == SectionType.ValueBased) {
                    // value-based
                    int n = Sections[SectionIndex].NextSection;
                    int a = Sections[SectionIndex].Aspects[Sections[SectionIndex].Aspects.Length - 1].Number;
                    if (n >= 0 && Sections[n].CurrentAspect >= 0) {
                        a = Sections[n].Aspects[Sections[n].CurrentAspect].Number;
                    }
                    for (int i = Sections[SectionIndex].Aspects.Length - 1; i >= 0; i--) {
                        if (Sections[SectionIndex].Aspects[i].Number > a) {
                            newaspect = i;
                        }
                    } if (newaspect == -1) {
                        newaspect = Sections[SectionIndex].Aspects.Length - 1;
                    }
                } else {
                    // index-based
                    if (Sections[SectionIndex].FreeSections >= 0 & Sections[SectionIndex].FreeSections < Sections[SectionIndex].Aspects.Length) {
                        newaspect = Sections[SectionIndex].FreeSections;
                    } else {
                        newaspect = Sections[SectionIndex].Aspects.Length - 1;
                    }
                }
            }
            if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
                // call setsignal for plugins when section changes aspect
                int p = Sections[SectionIndex].PreviousSection;
                if (p >= 0 && Sections[p].TrainIndices.Length == 1 && Sections[p].TrainIndices[0] == TrainManager.PlayerTrain && TrainManager.Trains[TrainManager.PlayerTrain].CurrentSectionIndex == p) {
                    int f;
                    if (Sections[SectionIndex].FreeSections >= 0) {
                        f = Sections[SectionIndex].FreeSections + 1;
                    } else {
                        f = -1;
                    }
                    int c; if (f >= 0 & f < Sections[p].Aspects.Length) {
                        c = f;
                    } else {
                        c = Sections[p].Aspects.Length - 1;
                    }
                    int b = Sections[p].Aspects[c].Number;
                    PluginManager.UpdateSignal(b);
                }
            }
            Sections[SectionIndex].CurrentAspect = newaspect;
            // update previous section
            if (Sections[SectionIndex].PreviousSection >= 0) {
                UpdateSection(Sections[SectionIndex].PreviousSection);
            }
            // update associated signals
            for (int i = 0; i < Sections[SectionIndex].SignalIndices.Length; i++) {
                UpdateSignal(Sections[SectionIndex].SignalIndices[i]);
            }
        }

        // signals
        internal struct SignalAspect {
            internal int Number;
            internal ObjectManager.StaticObject Object;
        }
        internal struct Signal {
            internal string Name;
            internal int Section;
            internal SignalAspect[] Aspects;
            internal int CurrentAspect;
            internal int ObjectIndex;
        }
        internal static Signal[] Signals = new Signal[] { };
        internal static void UpdateSignalVisibility() {
            double dist = World.BackgroundImageDistance + World.ExtraViewingDistance;
            dist *= dist;
            for (int i = 0; i < Signals.Length; i++) {
                int o = Signals[i].ObjectIndex;
                if (o >= 0) {
                    if (ObjectManager.Objects[o].Meshes.Length != 0) {
                        if (ObjectManager.Objects[o].Meshes[0].Vertices.Length != 0) {
                            double dx = ObjectManager.Objects[o].Meshes[0].Vertices[0].Coordinates.X - World.AbsoluteCameraPosition.X;
                            double dy = ObjectManager.Objects[o].Meshes[0].Vertices[0].Coordinates.Y - World.AbsoluteCameraPosition.Y;
                            double dz = ObjectManager.Objects[o].Meshes[0].Vertices[0].Coordinates.Z - World.AbsoluteCameraPosition.Z;
                            double t = dx * dx + dy * dy + dz * dz;
                            if (t <= dist) {
                                Renderer.ShowObject(o, false);
                            } else {
                                Renderer.HideObject(o);
                            }
                        }
                    }
                }
            }
        }
        private static void UpdateSignal(int SignalIndex) {
            int s = Signals[SignalIndex].Section;
            int a = Sections[s].CurrentAspect;
            int c = -1;
            if (a >= 0) {
                int n = Sections[s].Aspects[a].Number;
                for (int i = Signals[SignalIndex].Aspects.Length - 1; i >= 0; i--) {
                    if (Signals[SignalIndex].Aspects[i].Number >= n) {
                        c = i;
                    }
                } if (c == -1) {
                    c = Signals[SignalIndex].Aspects.Length - 1;
                }
            }
            if (c != Signals[SignalIndex].CurrentAspect) {
                Signals[SignalIndex].CurrentAspect = c;
                ChangeSignalAspect(SignalIndex, c);
            }
        }
        private static void ChangeSignalAspect(int SignalIndex, int AspectIndex) {
            if (SignalIndex == 0) { }
            int o = Signals[SignalIndex].ObjectIndex;
            if (o >= 0) {
                Renderer.HideObject(o);
                if (AspectIndex >= 0) {
                    ObjectManager.Objects[o].Meshes = new World.Mesh[Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes.Length];
                    for (int i = 0; i < Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes.Length; i++) {
                        ObjectManager.Objects[o].Meshes[i].Vertices = new World.Vertex[Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes[i].Vertices.Length];
                        for (int j = 0; j < Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes[i].Vertices.Length; j++) {
                            ObjectManager.Objects[o].Meshes[i].Vertices[j] = Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes[i].Vertices[j];
                        }
                        ObjectManager.Objects[o].Meshes[i].Faces = Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes[i].Faces;
                        ObjectManager.Objects[o].Meshes[i].Materials = Signals[SignalIndex].Aspects[AspectIndex].Object.Meshes[i].Materials;
                        World.CreateNormals(ref ObjectManager.Objects[o].Meshes[i], true);
                    }
                    Renderer.ShowObject(o, false);
                }
            }
        }

        // buffers
        internal static double[] BufferTrackPositions = new double[] { };

        // ================================

        // ai
        internal abstract class GeneralAI {
            internal abstract void Initialize(TrainManager.Train Train);
            internal abstract void Trigger(TrainManager.Train Train);
        }

        // better human driver
#if false
        internal class BetterHumanDriverAI : GeneralAI {
            internal BetterHumanDriverAI(TrainManager.Train Train) {
                this.Initialize(ref Train);
            }
            private double TimeLastChecked;
            private double TimeLastAdjusted;
            private double CurrentCheckInterval;
            private double CurrentAdjustInterval;
            private struct Settings {
                internal int PowerNotch;
                internal int BrakeNotch;
                internal double AirBrakePressure;
                internal bool ConstSpeed;
                internal bool HoldBrake;
                internal Settings(int PowerNotch, bool ConstSpeed) {
                    this.PowerNotch = PowerNotch;
                    this.BrakeNotch = 0;
                    this.AirBrakePressure = 0.0;
                    this.ConstSpeed = ConstSpeed;
                    this.HoldBrake = false;
                }
                internal Settings(int BrakeNotch, double AirBrakePressure) {
                    this.PowerNotch = 0;
                    this.BrakeNotch = BrakeNotch;
                    this.AirBrakePressure = AirBrakePressure;
                    this.ConstSpeed = false;
                    this.HoldBrake = false;
                }
                internal Settings(bool HoldBrake) {
                    this.PowerNotch = 0;
                    this.BrakeNotch = 0;
                    this.AirBrakePressure = 0.0;
                    this.ConstSpeed = false;
                    this.HoldBrake = HoldBrake;
                }
            }
            private struct Observations {
                internal int PowerNotch;
                internal int BrakeNotch;
                internal double[] PowerNotchObservedAcceleration;
                internal double[] BrakeNotchObservedAcceleration;
                internal double[] PowerNotchNextForcedObservation;
                internal double[] BrakeNotchNextForcedObservation;
                internal double NextObservationTime;
            }
            private Settings TargetSettings;
            private Observations CurrentObservations;
            // initialize
            internal override void Initialize(TrainManager.Train Train) {
                this.TimeLastChecked = 0.0;
                this.TimeLastAdjusted = 0.0;
                this.CurrentCheckInterval = 1.0;
                this.CurrentAdjustInterval = 1.0;
                this.TargetSettings = new Settings();
                // specs the driver is assuming on startup
                this.CurrentObservations.PowerNotchObservedAcceleration = new double[Train.Specs.MaximumPowerNotch];
                this.CurrentObservations.PowerNotchNextForcedObservation = new double[Train.Specs.MaximumPowerNotch];
                for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++) {
                    this.CurrentObservations.PowerNotchObservedAcceleration[i] = TrainManager.GetAcceleration(Train, Train.DriverCar, i, 12.5);
                }
                this.CurrentObservations.BrakeNotchObservedAcceleration = new double[Train.Specs.MaximumBrakeNotch];
                this.CurrentObservations.BrakeNotchNextForcedObservation = new double[Train.Specs.MaximumBrakeNotch];
                for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++) {
                    this.CurrentObservations.BrakeNotchObservedAcceleration[i] = -Train.Cars[Train.DriverCar].Specs.BrakeDecelerationAtServiceMaximumPressure * (double)(i + 1) / (double)Train.Specs.MaximumBrakeNotch;
                }
            }
            // trigger
            internal override void Trigger(TrainManager.Train Train) {
                bool AutomaticAirBrake = Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake;
                bool Check = SecondsSinceMidnight - this.TimeLastChecked >= this.CurrentCheckInterval;
                bool Adjust = SecondsSinceMidnight - this.TimeLastAdjusted >= this.CurrentAdjustInterval;
                if (Check) {
                    TimeLastChecked = SecondsSinceMidnight;
                    // check
                    bool DoorsOpen = false;
                    for (int i = 0; i < Train.Cars.Length; i++) {
                        for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
                            if (Train.Cars[i].Specs.Doors[j].State != 0.0) {
                                DoorsOpen = true; break;
                            } if (DoorsOpen) break;
                        }
                    }
                    if (DoorsOpen) {
                        // doors open
                        this.TargetSettings = new Settings(Train.Specs.MaximumBrakeNotch, 0.8 * Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure);
                        CurrentCheckInterval = 0.5 + 2.5 * Generator.NextDouble();
                    } else if (Train.StationState == TrainManager.TrainStopState.Boarding) {
                        // boarding
                        this.TargetSettings = new Settings(Train.Specs.MaximumBrakeNotch, 0.8 * Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure);
                        CurrentCheckInterval = 0.5 + 2.5 * Generator.NextDouble();
                    } else {
                        double Position = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
                        double CurrentSpeed = Train.Cars[Train.DriverCar].Specs.CurrentSpeed;
                        double AbsoluteSpeed = Math.Abs(CurrentSpeed);
                        // station
                        if (Train.Station >= 0) {
                            if (Train.StationState == TrainManager.TrainStopState.Pending) {
                                if (AbsoluteSpeed < 0.15) {
                                    int s = GetStopIndex(Train.Station, Train.Cars.Length);
                                    if (s >= 0) {
                                        double p = Game.Stations[Train.Station].Stops[s].TrackPosition;
                                        double dist = p - Position;
                                        if (dist <= Game.Stations[Train.Station].Stops[s].BackwardTolerance) {
                                            TrainManager.OpenTrainDoors(Train, Game.Stations[Train.Station].OpenLeftDoors, Game.Stations[Train.Station].OpenRightDoors);
                                            this.TargetSettings = new Settings(Train.Specs.MaximumBrakeNotch, 0.8 * Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure);
                                            Check = false;
                                        }
                                    }
                                }
                            }
                        }
                        if (Check) {
                            // security
                            if (Train.Specs.Security.Mode != TrainManager.SecuritySystem.Bve4Plugin) {
                                if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.None) {
                                    Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSn;
                                    return;
                                } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSn | Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                    if (Train.Specs.Security.Atc.Available & Train.Specs.Security.Atc.Transmitting & Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.277777777777778) {
                                        Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.Atc;
                                        return;
                                    }
                                } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    if (!Train.Specs.Security.Atc.Transmitting) {
                                        Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSn;
                                        return;
                                    }
                                }
                            }
                            // normal operation
                            CurrentCheckInterval = 1.0 + 3.0 * Generator.NextDouble();
                            double AllowedSpeed; if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                AllowedSpeed = Train.Specs.Security.Atc.SpeedRestriction;
                            } else {
                                AllowedSpeed = Train.CurrentRouteLimit < Train.CurrentSectionLimit ? Train.CurrentRouteLimit : Train.CurrentSectionLimit;
                            }
                            double RequiredAcceleration;
                            {
                                double d = Math.Atan(AllowedSpeed - CurrentSpeed);
                                RequiredAcceleration = 0.3 * d * d * d;
                            }
                            double ExpectedMaximumDeceleration;
                            if (AutomaticAirBrake) {
                                ExpectedMaximumDeceleration = 0.9 * Train.Cars[Train.DriverCar].Specs.BrakeDecelerationAtServiceMaximumPressure;
                                if (ExpectedMaximumDeceleration < 0.01) ExpectedMaximumDeceleration = 0.01;
                            } else if (Train.Specs.MaximumBrakeNotch == 0) {
                                ExpectedMaximumDeceleration = 0.1;
                            } else {
                                ExpectedMaximumDeceleration = 0.0;
                                for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++) {
                                    double dec = -this.CurrentObservations.BrakeNotchObservedAcceleration[i];
                                    if (dec > ExpectedMaximumDeceleration) {
                                        ExpectedMaximumDeceleration = dec;
                                    }
                                }
                                if (ExpectedMaximumDeceleration < 0.1) ExpectedMaximumDeceleration = 0.1;
                            }
                            // look ahead
                            double LookAhead = 50.0 + 2.0 * (CurrentSpeed * CurrentSpeed) / (2.0 * ExpectedMaximumDeceleration);
                            bool Curve = true;
                            {
                                bool Event = false;
                                int lte = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
                                for (int i = lte; i < TrackManager.CurrentTrack.Elements.Length; i++) {
                                    double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
                                    if (Position + LookAhead <= stp) break;
                                    for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
                                        if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent) {
                                            // speed limit
                                            TrackManager.LimitChangeEvent e = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                            if (e.NextSpeedLimit <= CurrentSpeed) {
                                                double dist = stp + e.TrackPositionDelta - Position;
                                                if (dist > 1.0) {
                                                    double dec = (CurrentSpeed * CurrentSpeed - e.NextSpeedLimit * e.NextSpeedLimit) / (2.0 * dist);
                                                    double acc = -dec;
                                                    if (acc < RequiredAcceleration) RequiredAcceleration = acc;
                                                    Event = true;
                                                }
                                            }
                                        } else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.SectionChangeEvent) {
                                            // section
                                            if (Train.Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                                                TrackManager.SectionChangeEvent e = (TrackManager.SectionChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                                if (!Game.Sections[e.NextSectionIndex].Invisible & Game.Sections[e.NextSectionIndex].CurrentAspect >= 0) {
                                                    double dist = stp + e.TrackPositionDelta - Position;
                                                    if (dist > 0.0) {
                                                        double lim = Game.Sections[e.NextSectionIndex].Aspects[Game.Sections[e.NextSectionIndex].CurrentAspect].Speed;
                                                        if (lim <= CurrentSpeed) {
                                                            double dec;
                                                            if (lim == 0.0) {
                                                                double StopDistance = 10.0 + 20.0 * (1.0 - 1.0 / (1.0 + 0.01 * CurrentSpeed * CurrentSpeed));
                                                                if (dist > StopDistance) {
                                                                    dec = CurrentSpeed * CurrentSpeed / (2.0 * (dist - StopDistance));
                                                                } else {
                                                                    dec = ExpectedMaximumDeceleration;
                                                                }
                                                            } else {
                                                                if (dist >= 1.0) {
                                                                    dec = (CurrentSpeed * CurrentSpeed - lim * lim) / (2.0 * dist);
                                                                } else dec = 0.0;
                                                            }
                                                            double acc = -dec;
                                                            if (acc < RequiredAcceleration) RequiredAcceleration = acc;
                                                            Event = true;
                                                        }
                                                    }
                                                }
                                            }
                                        } else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent) {
                                            // station
                                            TrackManager.StationEndEvent e = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                            if (Train.Station != e.StationIndex | Train.StationState == TrainManager.TrainStopState.Pending) {
                                                if (Game.Stations[e.StationIndex].StopAtStation) {
                                                    int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
                                                    if (s >= 0) {
                                                        double p = Game.Stations[e.StationIndex].Stops[s].TrackPosition;
                                                        double dist = p - Position;
                                                        double acc;
                                                        if (dist > 0.0) {
                                                            double StopDistance = (CurrentSpeed > 2.0 ? 5.0 : 0.0) + 25.0 * (1.0 - 1.0 / (1.0 + 0.01 * CurrentSpeed * CurrentSpeed));
                                                            dist -= StopDistance;
                                                            if (dist > 0.0) {
                                                                double dec = CurrentSpeed * CurrentSpeed / (2.0 * (dist));
                                                                acc = -dec;
                                                            } else {
                                                                acc = -2.0 * ExpectedMaximumDeceleration;
                                                            }
                                                        } else {
                                                            acc = -2.0 * ExpectedMaximumDeceleration;
                                                        }
                                                        if (acc < RequiredAcceleration) RequiredAcceleration = acc;
                                                        Event = true;
                                                        Curve = false;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (Event) {
                                    CurrentCheckInterval = 0.5 + 1.0 * Generator.NextDouble();
                                }
                            }

                            // debug
                            //Game.InfoDebugString = RequiredAcceleration.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture) + " P[";
                            //for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++) {
                            //    if (i > 0) Game.InfoDebugString += ",";
                            //    Game.InfoDebugString += this.CurrentObservations.PowerNotchObservedAcceleration[i].ToString("0.000");
                            //} Game.InfoDebugString += "] B[";
                            //for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++) {
                            //    if (i > 0) Game.InfoDebugString += ",";
                            //    Game.InfoDebugString += this.CurrentObservations.BrakeNotchObservedAcceleration[i].ToString("0.000");
                            //} Game.InfoDebugString += "]";

                            // security systems
                            if (RequiredAcceleration >= 0.0) {
                                if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.Atc & (CurrentSpeed >= Train.Specs.Security.Atc.SpeedRestriction | Train.Specs.Security.State == TrainManager.SecurityState.Service)) {
                                    RequiredAcceleration = -0.0001;
                                } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP & (Train.Specs.Security.State == TrainManager.SecurityState.Pattern | Train.Specs.Security.State == TrainManager.SecurityState.Service)) {
                                    RequiredAcceleration = -0.0001;
                                }
                            }
                            // apply acceleration/deceleration
                            if (RequiredAcceleration < 0.0) {
                                // curve
                                if (Curve) {
                                    if (AbsoluteSpeed < 1.0) {
                                        double x = 1.0 - AbsoluteSpeed;
                                        double x2 = x * x;
                                        RequiredAcceleration *= 2.25 * x2 - 1.5 * x * x2 + 0.25;
                                    } else if (AbsoluteSpeed < 3.0) {
                                        double x = 0.5 * AbsoluteSpeed - 0.5;
                                        double x2 = x * x;
                                        RequiredAcceleration *= 2.25 * x2 - 1.5 * x * x2 + 0.25;
                                    }
                                    if (AbsoluteSpeed < 5.0) {
                                        CurrentCheckInterval = 0.25 + 0.25 * Generator.NextDouble();
                                    }
                                }
                                // decelerate
                                if (CurrentSpeed > Train.Cars[Train.DriverCar].Specs.BrakeControlSpeed) {
                                    RequiredAcceleration *= 1.0 + 0.02 * CurrentSpeed;
                                } else {
                                    RequiredAcceleration *= 1.0 + 0.01 * CurrentSpeed;
                                }
                                // find power notch to sufficiently brake (e.g. on incline)
                                int BestPowerNotch = 0;
                                double BestDifference = double.MinValue;
                                for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++) {
                                    double d = this.CurrentObservations.PowerNotchObservedAcceleration[i] - RequiredAcceleration;
                                    if (d <= 0.0 & d > BestDifference) {
                                        BestPowerNotch = i + 1;
                                        BestDifference = d;
                                    }
                                }
                                if (BestPowerNotch != 0) {
                                    if (Train.Specs.HasConstSpeed) {
                                        // use constant speed system
                                        this.TargetSettings = new Settings(Train.Specs.MaximumPowerNotch, true);
                                    } else {
                                        // use power notch
                                        this.TargetSettings = new Settings(BestPowerNotch, false);
                                    }
                                } else if (AutomaticAirBrake) {
                                    // air brake pressure
                                    double dec = -RequiredAcceleration;
                                    this.TargetSettings = new Settings(0, Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure * dec / Train.Cars[Train.DriverCar].Specs.BrakeDecelerationAtServiceMaximumPressure);
                                } else {
                                    // find brake notch to sufficiently brake
                                    int BestBrakeNotch = 0;
                                    BestDifference = double.MaxValue;
                                    for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++) {
                                        double d = this.CurrentObservations.BrakeNotchObservedAcceleration[i] - RequiredAcceleration;
                                        if (d >= 0.0 & d < BestDifference) {
                                            BestBrakeNotch = i + 1;
                                            BestDifference = d;
                                        }
                                    }
                                    if (BestBrakeNotch != 0) {
                                        // use brake notch
                                        if (this.CurrentObservations.BrakeNotchObservedAcceleration[Train.Specs.MaximumBrakeNotch - 1] > this.CurrentObservations.BrakeNotchObservedAcceleration[BestBrakeNotch - 1]) {
                                            BestBrakeNotch = Train.Specs.MaximumBrakeNotch;
                                        }
                                        this.TargetSettings = new Settings(BestBrakeNotch, 0.0);
                                    } else {
                                        // go to neutral
                                        this.TargetSettings = new Settings(0, 0.0);
                                    }
                                }
                            } else {
                                // find brake notch to sufficiently accelerate (e.g. on decline)
                                int BestBrakeNotch = 0;
                                if (!AutomaticAirBrake) {
                                    double BestDifference = double.MaxValue;
                                    if (!AutomaticAirBrake) {
                                        for (int i = 0; i < Train.Specs.MaximumBrakeNotch; i++) {
                                            double d = this.CurrentObservations.BrakeNotchObservedAcceleration[i] - RequiredAcceleration;
                                            if (d >= 0.0 & d < BestDifference) {
                                                BestBrakeNotch = i + 1;
                                                BestDifference = d;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (BestBrakeNotch != 0) {
                                    if (Train.Specs.HasHoldBrake) {
                                        // use hold brake
                                        this.TargetSettings = new Settings(true);
                                    } else {
                                        // use brake notch
                                        this.TargetSettings = new Settings(BestBrakeNotch, 0.0);
                                    }
                                } else {
                                    // find power notch to accelerate
                                    int BestPowerNotch = 0;
                                    double BestDifference = double.MaxValue;
                                    for (int i = 0; i < Train.Specs.MaximumPowerNotch; i++) {
                                        double d = Math.Abs(this.CurrentObservations.PowerNotchObservedAcceleration[i] - RequiredAcceleration);
                                        if (d < BestDifference) {
                                            BestPowerNotch = i + 1;
                                            BestDifference = d;
                                        }
                                    }
                                    if (BestPowerNotch != 0) {
                                        // use power notch
                                        if (this.CurrentObservations.PowerNotchObservedAcceleration[Train.Specs.MaximumPowerNotch - 1] < this.CurrentObservations.PowerNotchObservedAcceleration[BestPowerNotch - 1]) {
                                            BestPowerNotch = Train.Specs.MaximumPowerNotch;
                                        }
                                        this.TargetSettings = new Settings(BestPowerNotch, false);
                                    } else {
                                        // go to neutral
                                        this.TargetSettings = new Settings(0, false);
                                    }
                                }
                            }
                        }
                    }
                }
                if (Adjust) {
                    TimeLastAdjusted = SecondsSinceMidnight;
                    // forced observations
                    //if (Math.Abs(Train.Cars[Train.DriverCar].Specs.CurrentSpeed) > 1.38888888888889) {
                    //    if (Train.Specs.CurrentPowerNotch.Driver != 0 & this.TargetSettings.BrakeNotch == 0 & this.TargetSettings.AirBrakePressure == 0.0) {
                    //        if (SecondsSinceMidnight > this.CurrentObservations.PowerNotchNextForcedObservation[Train.Specs.CurrentPowerNotch.Driver - 1]) {
                    //            this.TargetSettings = new Settings(Train.Specs.CurrentPowerNotch.Driver, false);
                    //        }
                    //    } else if (!AutomaticAirBrake & Train.Specs.CurrentBrakeNotch.Driver != 0 & this.TargetSettings.PowerNotch == 0) {
                    //        if (SecondsSinceMidnight > this.CurrentObservations.BrakeNotchNextForcedObservation[Train.Specs.CurrentBrakeNotch.Driver - 1]) {
                    //            this.TargetSettings = new Settings(Train.Specs.CurrentBrakeNotch.Driver, 0.0);
                    //        }
                    //    }
                    //}
                    // adjust
                    this.CurrentAdjustInterval = 0.0;
                    if (this.TargetSettings.PowerNotch == 0 & Train.Specs.CurrentPowerNotch.Driver != 0) {
                        // power notch to zero
                        this.CurrentAdjustInterval = 0.15 + 0.2 * Generator.NextDouble() + 0.2 / (double)Train.Specs.CurrentPowerNotch.Driver;
                        TrainManager.ApplyNotch(Train, -1, true, 0, true);
                    } else if (!AutomaticAirBrake & this.TargetSettings.BrakeNotch == 0 & Train.Specs.CurrentBrakeNotch.Driver != 0) {
                        // brake notch to zero
                        this.CurrentAdjustInterval = 0.15 + 0.2 * Generator.NextDouble() + 0.2 / (double)Train.Specs.CurrentBrakeNotch.Driver;
                        TrainManager.ApplyNotch(Train, 0, true, -1, true);
                    } else if (AutomaticAirBrake & this.TargetSettings.AirBrakePressure == 0.0 & Train.Specs.AirBrake.Handle.Driver != TrainManager.AirBrakeHandleState.Release) {
                        // release air brake
                        TrainManager.ApplyAirBrakeHandle(Train, -1);
                    } else if (this.TargetSettings.ConstSpeed != Train.Specs.CurrentConstSpeed) {
                        // change constant speed system
                        this.CurrentAdjustInterval = 0.5 + 1.0 * Generator.NextDouble();
                        Train.Specs.CurrentConstSpeed = this.TargetSettings.ConstSpeed;
                    } else if (this.TargetSettings.PowerNotch != Train.Specs.CurrentPowerNotch.Driver) {
                        // change power notch
                        int d = this.TargetSettings.PowerNotch - Train.Specs.CurrentPowerNotch.Driver;
                        this.CurrentAdjustInterval = 0.15 + 0.2 * Generator.NextDouble() + 0.2 / (double)Math.Abs(d);
                        TrainManager.ApplyNotch(Train, Math.Sign(d), true, 0, true);
                    } else if (this.TargetSettings.HoldBrake != Train.Specs.CurrentHoldBrake.Driver) {
                        // change hold brake
                        TrainManager.ApplyHoldBrake(Train, this.TargetSettings.HoldBrake);
                    } else if (!AutomaticAirBrake & this.TargetSettings.BrakeNotch != Train.Specs.CurrentBrakeNotch.Driver) {
                        // change brake notch
                        int d = this.TargetSettings.BrakeNotch - Train.Specs.CurrentBrakeNotch.Driver;
                        this.CurrentAdjustInterval = 0.15 + 0.2 * Generator.NextDouble() + 0.2 / (double)Math.Abs(d);
                        TrainManager.ApplyNotch(Train, 0, true, Math.Sign(d), true);
                    } else if (AutomaticAirBrake & this.TargetSettings.AirBrakePressure != 0.0) {
                        // change air brake handle
                        double d = this.TargetSettings.AirBrakePressure - Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderCurrentPressure;
                        double r = d / Train.Cars[Train.DriverCar].Specs.AirBrake.BrakeCylinderServiceMaximumPressure;
                        if (r < -0.025) {
                            this.CurrentAdjustInterval = 0.075 + 0.15 * Generator.NextDouble();
                            TrainManager.ApplyAirBrakeHandle(Train, -1);
                        } else if (r > 0.025) {
                            this.CurrentAdjustInterval = 0.075 + 0.15 * Generator.NextDouble();
                            TrainManager.ApplyAirBrakeHandle(Train, 1);
                        } else {
                            this.CurrentAdjustInterval = 0.1 + 0.3 * Generator.NextDouble();
                            TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
                        }
                    }
                    if (this.CurrentAdjustInterval == 0.0) {
                        this.CurrentAdjustInterval = 0.2 + 0.4 * Generator.NextDouble();
                    }
                    // observations
                    bool Postpone = true;
                    if (Math.Abs(Train.Cars[Train.DriverCar].Specs.CurrentSpeed) > 1.38888888888889) {
                        if (this.CurrentObservations.PowerNotch == Train.Specs.CurrentPowerNotch.Driver & this.CurrentObservations.BrakeNotch == Train.Specs.CurrentBrakeNotch.Driver) {
                            if ((this.CurrentObservations.PowerNotch != 0 ^ this.CurrentObservations.BrakeNotch != 0) & !Train.Specs.CurrentHoldBrake.Driver & !Train.Specs.CurrentConstSpeed) {
                                if (SecondsSinceMidnight >= this.CurrentObservations.NextObservationTime) {
                                    if (this.CurrentObservations.PowerNotch != 0) {
                                        this.CurrentObservations.PowerNotchObservedAcceleration[this.CurrentObservations.PowerNotch - 1] = Train.Specs.CurrentAverageAcceleration;
                                        this.CurrentObservations.PowerNotchNextForcedObservation[this.CurrentObservations.PowerNotch - 1] = SecondsSinceMidnight + 60.0;
                                    } else if (this.CurrentObservations.BrakeNotch != 0) {
                                        this.CurrentObservations.BrakeNotchObservedAcceleration[this.CurrentObservations.BrakeNotch - 1] = Train.Specs.CurrentAverageAcceleration;
                                        this.CurrentObservations.BrakeNotchNextForcedObservation[this.CurrentObservations.BrakeNotch - 1] = SecondsSinceMidnight + 60.0;
                                    }
                                } else {
                                    Postpone = false;
                                }
                            }
                        }
                    }
                    if (Postpone) {
                        const double time = 1.0;
                        if (this.CurrentObservations.PowerNotch != 0) {
                            this.CurrentObservations.NextObservationTime = SecondsSinceMidnight + Train.Specs.DelayPowerStart + Train.Specs.DelayPowerStop + time;
                        } else if (this.CurrentObservations.BrakeNotch != 0) {
                            this.CurrentObservations.NextObservationTime = SecondsSinceMidnight + Train.Specs.DelayBrakeStart + Train.Specs.DelayBrakeEnd + time;
                        } else {
                            this.CurrentObservations.NextObservationTime = SecondsSinceMidnight + time;
                        }
                    }
                    this.CurrentObservations.PowerNotch = Train.Specs.CurrentPowerNotch.Driver;
                    this.CurrentObservations.BrakeNotch = Train.Specs.CurrentBrakeNotch.Driver;
                }
            }
        }
#endif

        // simplistic human driver
        internal class SimplisticHumanDriverAI : GeneralAI {
            private double TimeLastProcessed;
            private double CurrentInterval;
            private bool BrakeMode;
            internal override void Initialize(TrainManager.Train Train) {
                this.TimeLastProcessed = 0.0;
                this.CurrentInterval = 1.0;
                this.BrakeMode = false;
            }
            internal override void Trigger(TrainManager.Train Train) {
                if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval) {
                    TimeLastProcessed = SecondsSinceMidnight;
                    // door states
                    bool doorsopen = false;
                    for (int i = 0; i < Train.Cars.Length; i++) {
                        for (int j = 0; j < Train.Cars[i].Specs.Doors.Length; j++) {
                            if (Train.Cars[i].Specs.Doors[j].State != 0.0) {
                                doorsopen = true; break;
                            } if (doorsopen) break;
                        }
                    }
                    // handle the security system
                    if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.None) {
                        Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSN;
                    } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                        // ats-s
                        if (Train.Specs.Security.State == TrainManager.SecurityState.Ringing) {
                            TrainManager.AcknowledgeSecuritySystem(Train, TrainManager.AcknowledgementType.Alarm);
                        } else if (Train.Specs.Security.State == TrainManager.SecurityState.Emergency & Math.Abs(Train.Specs.CurrentAverageSpeed) < 1.0) {
                            Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.None;
                            this.CurrentInterval = 1.0;
                            return;
                        }
                        if (Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.277777777777778) {
                            TrainManager.AcknowledgeSecuritySystem(Train, TrainManager.AcknowledgementType.Chime);
                        }
                    } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                        // ats-p
                        if (Train.Specs.Security.State == TrainManager.SecurityState.Service) {
                            if (Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.138888888888889) {
                                Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.None;
                                this.CurrentInterval = 1.0;
                                return;
                            }
                        }
                    }
                    if (Train.Specs.Security.Eb.BellState != TrainManager.SecurityState.Normal) {
                        // eb
                        TrainManager.AcknowledgeSecuritySystem(Train, TrainManager.AcknowledgementType.Eb);
                    }
                    // do the ai
                    Train.Specs.CurrentConstSpeed = false;
                    TrainManager.ApplyHoldBrake(Train, false);
                    if (Train.CurrentSectionLimit == 0.0 & Train.Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                        // passing red signal
                        TrainManager.ApplyEmergencyBrake(Train);
                        TrainManager.ApplyNotch(Train, -1, true, 1, true);
                        CurrentInterval = 0.5;
                    } else if (doorsopen) {
                        // door opened
                        if (Train.Station >= 0 && Stations[Train.Station].IsTerminalStation & Train.TrainIndex == TrainManager.PlayerTrain) {
                            // player's terminal station
                            TrainManager.ApplyReverser(Train, 0, false);
                            TrainManager.ApplyNotch(Train, -1, true, 1, true);
                            TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
                            TrainManager.ApplyEmergencyBrake(Train);
                            CurrentInterval = 10.0;
                        } else {
                            TrainManager.ApplyNotch(Train, -1, true, 1, true);
                            TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
                            TrainManager.UnapplyEmergencyBrake(Train);
                            if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Completed) {
                                // ready for departure - close doors
                                if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
                                    TrainManager.CloseTrainDoors(Train, true, true);
                                }
                            } else if (Train.Station >= 0 & Train.StationState == TrainManager.TrainStopState.Boarding) {
                                // still boarding
                                if (Stations[Train.Station].SecuritySystem == SecuritySystem.Ats & Train.Specs.Security.Mode != TrainManager.SecuritySystem.AtsSN & Train.Specs.Security.Mode != TrainManager.SecuritySystem.AtsP) {
                                    Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.AtsSN;
                                } else if (Stations[Train.Station].SecuritySystem == SecuritySystem.Atc & Train.Specs.Security.Mode != TrainManager.SecuritySystem.Atc & Train.Specs.Security.Atc.Available) {
                                    Train.Specs.Security.ModeChange = TrainManager.SecuritySystem.Atc;
                                }
                            } else {
                                // not at station - close doors
                                if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
                                    TrainManager.CloseTrainDoors(Train, true, true);
                                }
                            }
                            CurrentInterval = 1.0;
                        }
                    } else if (Train.Station >= 0 && (Stations[Train.Station].StopAtStation & (Stations[Train.Station].OpenLeftDoors | Stations[Train.Station].OpenRightDoors) & Math.Abs(Train.Specs.CurrentAverageSpeed) < 0.25 & Train.StationState == TrainManager.TrainStopState.Pending)) {
                        // arrived at station - open doors
                        if (Train.Specs.DoorOpenMode != TrainManager.DoorMode.Automatic) {
                            TrainManager.OpenTrainDoors(Train, Stations[Train.Station].OpenLeftDoors, Stations[Train.Station].OpenRightDoors);
                        }
                        CurrentInterval = 1.0;
                    } else {
                        // drive
                        TrainManager.UnapplyEmergencyBrake(Train);
                        TrainManager.ApplyReverser(Train, 1, false);
                        double spd = Train.Specs.CurrentAverageSpeed;
                        double acc = Train.Specs.CurrentAverageAcceleration;
                        double lim = double.PositiveInfinity;
                        if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                            if (Train.Specs.Security.Atc.SpeedRestriction < lim) lim = Train.Specs.Security.Atc.SpeedRestriction;
                        } else {
                            if (Train.CurrentRouteLimit < lim) lim = Train.CurrentRouteLimit;
                            if (Train.CurrentSectionLimit < lim) lim = Train.CurrentSectionLimit;
                        }
                        double powerstart = Math.Max(0.95 * lim, lim - 1.3);
                        double powerend = lim;
                        double brakestart = BrakeMode ? lim : Math.Max(Math.Min(1.1 * lim, lim + 1.3), lim + 0.3);
                        double dec = 0.0;
                        double dectol;
                        double BrakeDeceleration = Train.Cars[Train.DriverCar].Specs.BrakeDecelerationAtServiceMaximumPressure;
                        if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | Train.Specs.MaximumBrakeNotch <= 0) {
                            dectol = 0.1 * BrakeDeceleration;
                        } else {
                            dectol = BrakeDeceleration / (double)Train.Specs.MaximumBrakeNotch;
                        }
                        if (spd > 0.0 & spd > brakestart) {
                            double r = 0.125 + (spd - lim) / spd;
                            if (r > 0.0) {
                                if (r > 1.0) {
                                    dec = BrakeDeceleration;
                                } else {
                                    dec = r * BrakeDeceleration;
                                }
                            }
                        }
                        double lookahead = 25.0 + 1.5 * (spd * spd) / (2.0 * BrakeDeceleration);
                        double tp = Train.Cars[0].FrontAxle.Follower.TrackPosition - Train.Cars[0].FrontAxlePosition + 0.5 * Train.Cars[0].Length;
                        { // events
                            int te = Train.Cars[0].FrontAxle.Follower.LastTrackElement;
                            for (int i = te; i < TrackManager.CurrentTrack.Elements.Length; i++) {
                                double stp = TrackManager.CurrentTrack.Elements[i].StartingTrackPosition;
                                if (tp + lookahead <= stp) break;
                                for (int j = 0; j < TrackManager.CurrentTrack.Elements[i].Events.Length; j++) {
                                    if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.LimitChangeEvent) {
                                        // speed limit
                                        TrackManager.LimitChangeEvent e = (TrackManager.LimitChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                        if (e.NextSpeedLimit < spd) {
                                            double dist = stp + e.TrackPositionDelta - tp;
                                            double edec = (spd * spd - e.NextSpeedLimit * e.NextSpeedLimit) / (2.0 * dist);
                                            if (edec > dec) dec = edec;
                                        }
                                    } else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.SectionChangeEvent) {
                                        // section
                                        if (Train.Specs.Security.Mode != TrainManager.SecuritySystem.Atc) {
                                            TrackManager.SectionChangeEvent e = (TrackManager.SectionChangeEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                            if (stp + e.TrackPositionDelta > tp) {
                                                if (!Game.Sections[e.NextSectionIndex].Invisible & Game.Sections[e.NextSectionIndex].CurrentAspect >= 0) {
                                                    double elim = Game.Sections[e.NextSectionIndex].Aspects[Game.Sections[e.NextSectionIndex].CurrentAspect].Speed;
                                                    if (elim < spd | spd <= 0.0) {
                                                        double dist = stp + e.TrackPositionDelta - tp;
                                                        double edec;
                                                        if (elim == 0.0) {
                                                            const double redstopdist = 10.0;
                                                            if (dist > redstopdist) {
                                                                edec = (spd * spd - elim * elim) / (2.0 * (dist - redstopdist));
                                                            } else {
                                                                edec = BrakeDeceleration;
                                                            }
                                                        } else {
                                                            if (dist >= 1.0) {
                                                                edec = (spd * spd - elim * elim) / (2.0 * dist);
                                                            } else edec = 0.0;
                                                        }
                                                        if (edec > dec) dec = edec;
                                                    }
                                                }
                                            }
                                        }
                                    } else if (TrackManager.CurrentTrack.Elements[i].Events[j] is TrackManager.StationEndEvent) {
                                        // station stop
                                        TrackManager.StationEndEvent e = (TrackManager.StationEndEvent)TrackManager.CurrentTrack.Elements[i].Events[j];
                                        if (Stations[e.StationIndex].StopAtStation & (Stations[e.StationIndex].OpenLeftDoors | Stations[e.StationIndex].OpenRightDoors) & Train.StationState == TrainManager.TrainStopState.Pending) {
                                            int s = GetStopIndex(e.StationIndex, Train.Cars.Length);
                                            if (s >= 0) {
                                                double dist = Stations[e.StationIndex].Stops[s].TrackPosition - tp;
                                                double edec;
                                                if (dist < Stations[e.StationIndex].Stops[s].BackwardTolerance & -dist < Stations[e.StationIndex].Stops[s].ForwardTolerance) {
                                                    edec = BrakeDeceleration;
                                                } else {
                                                    edec = (spd * spd) / (2.0 * dist);
                                                }
                                                if (edec > dec) dec = edec;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // current station
                        if (Train.Station >= 0) {
                            if (Stations[Train.Station].StopAtStation & (Stations[Train.Station].OpenLeftDoors | Stations[Train.Station].OpenRightDoors) & Train.StationState == TrainManager.TrainStopState.Pending) {
                                int s = GetStopIndex(Train.Station, Train.Cars.Length);
                                if (s >= 0) {
                                    double dist = Stations[Train.Station].Stops[s].TrackPosition - tp;
                                    double edec;
                                    if (dist < Stations[Train.Station].Stops[s].BackwardTolerance) {
                                        edec = BrakeDeceleration;
                                    } else {
                                        edec = (spd * spd) / (2.0 * dist);
                                    }
                                    if (edec > dec) dec = edec;
                                }
                            }
                        }
                        // handle the security system
                        if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                            // ats-s
                            if (Train.Specs.Security.State == TrainManager.SecurityState.Ringing) {
                                dec = BrakeDeceleration;
                            }
                        } else if (Train.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                            // ats-p
                            if (Train.Specs.Security.State == TrainManager.SecurityState.Pattern & !Train.Specs.Security.Ats.AtsPOverride | Train.Specs.Security.State == TrainManager.SecurityState.Service) {
                                powerstart = spd - 1.0;
                                powerend = spd - 1.0;
                            }
                        }
                        // power / brake
                        if (dec > dectol) {
                            // brake
                            BrakeMode = true;
                            double decdiff = -acc - dec;
                            if (decdiff < -dectol) {
                                // brake start
                                TrainManager.ApplyNotch(Train, -1, true, 1, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Service);
                                CurrentInterval *= 0.3;
                                if (CurrentInterval < 0.25) CurrentInterval = 0.25;
                            } else if (decdiff > dectol) {
                                // brake stop
                                TrainManager.ApplyNotch(Train, -1, true, -1, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
                                CurrentInterval *= 0.3;
                                if (CurrentInterval < 0.25) CurrentInterval = 0.25;
                            } else {
                                // keep brake
                                TrainManager.ApplyNotch(Train, -1, true, 0, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Lap);
                                CurrentInterval *= 1.3;
                                if (CurrentInterval > 2.5) CurrentInterval = 2.5;
                            }
                            if (Train.Specs.HasHoldBrake) {
                                if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                                    if (Train.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
                                        TrainManager.ApplyHoldBrake(Train, true);
                                    }
                                } else if (Train.Specs.CurrentBrakeNotch.Driver == 0) {
                                    TrainManager.ApplyHoldBrake(Train, true);
                                }
                            }
                            if (Train.Cars[Train.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                                CurrentInterval = 0.1;
                            }
                        } else {
                            // power
                            BrakeMode = false;
                            if (spd < powerstart) {
                                // power start
                                TrainManager.ApplyNotch(Train, 1, true, -1, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
                                CurrentInterval *= 0.3;
                                if (CurrentInterval < 0.25) CurrentInterval = 0.25;
                            } else if (spd > powerend) {
                                // power end
                                TrainManager.ApplyNotch(Train, -1, true, -1, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
                                CurrentInterval *= 0.3;
                                if (CurrentInterval < 0.25) CurrentInterval = 0.25;
                            } else {
                                // keep power
                                TrainManager.ApplyNotch(Train, 0, true, -1, true);
                                TrainManager.ApplyAirBrakeHandle(Train, TrainManager.AirBrakeHandleState.Release);
                                if (Train.Specs.CurrentPowerNotch.Driver > 0) {
                                    Train.Specs.CurrentConstSpeed = Train.Specs.HasConstSpeed;
                                }
                                if (Train.Specs.CurrentPowerNotch.Driver == 0 & Train.Specs.CurrentBrakeNotch.Driver == 0) {
                                    TrainManager.ApplyHoldBrake(Train, Train.Specs.HasHoldBrake);
                                }
                                CurrentInterval *= 1.3;
                                if (CurrentInterval > 2.5) CurrentInterval = 2.5;
                            }
                        }
                    }
                }
            }
        }

        // bogus pretrain
        internal struct BogusPretrainInstruction {
            internal double TrackPosition;
            internal double Time;
        }
        internal static BogusPretrainInstruction[] BogusPretrainInstructions = new BogusPretrainInstruction[] { };
        internal class BogusPretrainAI : GeneralAI {
            private double TimeLastProcessed;
            private double CurrentInterval;
            internal override void Initialize(TrainManager.Train Train) {
                this.TimeLastProcessed = 0.0;
                this.CurrentInterval = 1.0;
            }
            internal override void Trigger(TrainManager.Train Train) {
                if (SecondsSinceMidnight - TimeLastProcessed >= CurrentInterval) {
                    TimeLastProcessed = SecondsSinceMidnight;
                    CurrentInterval = 5.0;
                    double ap = double.MaxValue, at = double.MaxValue;
                    double bp = double.MinValue, bt = double.MinValue;
                    for (int i = 0; i < BogusPretrainInstructions.Length; i++) {
                        if (BogusPretrainInstructions[i].Time < SecondsSinceMidnight | at == double.MaxValue) {
                            at = BogusPretrainInstructions[i].Time;
                            ap = BogusPretrainInstructions[i].TrackPosition;
                        }
                    }
                    for (int i = BogusPretrainInstructions.Length - 1; i >= 0; i--) {
                        if (BogusPretrainInstructions[i].Time > at | bt == double.MinValue) {
                            bt = BogusPretrainInstructions[i].Time;
                            bp = BogusPretrainInstructions[i].TrackPosition;
                        }
                    }
                    if (at != double.MaxValue & bt != double.MinValue & SecondsSinceMidnight <= BogusPretrainInstructions[BogusPretrainInstructions.Length - 1].Time) {
                        double r = bt - at;
                        if (r > 0.0) {
                            r = (SecondsSinceMidnight - at) / r;
                            if (r < 0.0) r = 0.0;
                            if (r > 1.0) r = 1.0;
                        } else {
                            r = 0.0;
                        }
                        double p = ap + r * (bp - ap);
                        double d = p - Train.Cars[0].FrontAxle.Follower.TrackPosition;
                        for (int j = 0; j < Train.Cars.Length; j++) {
                            TrainManager.MoveCar(Train, j, d, 0.1);
                        }
                    } else {
                        TrainManager.DisposeTrain(Train);
                    }
                }
            }
        }

        // ================================

        // messages
        internal enum MessageType { Game, Interface }
        internal enum MessageColor { White, Red, Orange, Green, Blue, Magenta }
        internal enum MessageDependency {
            None = 0,
            RouteLimit = 1,
            SectionLimit = 2,
            Station = 3
        }
        internal struct Message {
            internal string InternalText;
            internal string DisplayText;
            internal MessageDependency Depencency;
            internal World.ColorRGB Color;
            internal double Timeout;
            internal double RendererPosition;
        }
        internal static Message[] Messages = new Message[] { };
        internal const double MessageFadeOutTime = 1.0;
        internal static void AddMessage(string Text, MessageDependency Depencency, MessageType Type, MessageColor Color, double Timeout) {
            if (Type != MessageType.Game | CurrentMode != GameMode.Expert) {
                if (Depencency == MessageDependency.RouteLimit | Depencency == MessageDependency.SectionLimit) {
                    for (int i = 0; i < Messages.Length; i++) {
                        if (Messages[i].Depencency == Depencency) return;
                    }
                }
                int n = Messages.Length;
                Array.Resize<Message>(ref Messages, n + 1);
                Messages[n].InternalText = Text;
                Messages[n].DisplayText = "";
                Messages[n].Depencency = Depencency;
                switch (Color) {
                    case MessageColor.White: Messages[n].Color = new World.ColorRGB(255, 255, 255); break;
                    case MessageColor.Red: Messages[n].Color = new World.ColorRGB(255, 0, 0); break;
                    case MessageColor.Orange: Messages[n].Color = new World.ColorRGB(255, 192, 0); break;
                    case MessageColor.Green: Messages[n].Color = new World.ColorRGB(0, 255, 0); break;
                    case MessageColor.Blue: Messages[n].Color = new World.ColorRGB(0, 192, 255); break;
                    case MessageColor.Magenta: Messages[n].Color = new World.ColorRGB(255, 0, 255); break;
                    default: Messages[n].Color = new World.ColorRGB(255, 255, 255); break;
                }
                Messages[n].Timeout = Timeout;
                Messages[n].RendererPosition = -1.0;
            }
        }
        internal static void UpdateMessages() {
            for (int i = 0; i < Messages.Length; i++) {
                bool rem = SecondsSinceMidnight >= Messages[i].Timeout;
                switch (Messages[i].Depencency) {
                    case MessageDependency.RouteLimit: {
                            double spd = Math.Abs(TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageSpeed);
                            double lim = TrainManager.Trains[TrainManager.PlayerTrain].CurrentRouteLimit;
                            spd = Math.Round(spd * 3.6);
                            lim = Math.Round(lim * 3.6);
                            rem = spd <= lim;
                            string s = Messages[i].InternalText, t;
                            t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            s = s.Replace("[speed]", t);
                            t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            s = s.Replace("[limit]", t);
                            Messages[i].DisplayText = s;
                        } break;
                    case MessageDependency.SectionLimit: {
                            double spd = Math.Abs(TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageSpeed);
                            double lim = TrainManager.Trains[TrainManager.PlayerTrain].CurrentSectionLimit;
                            spd = Math.Round(spd * 3.6);
                            lim = Math.Round(lim * 3.6);
                            rem = spd <= lim;
                            string s = Messages[i].InternalText, t;
                            t = spd.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            s = s.Replace("[speed]", t);
                            t = lim.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            s = s.Replace("[limit]", t);
                            Messages[i].DisplayText = s;
                        } break;
                    case MessageDependency.Station: {
                            int j = TrainManager.Trains[TrainManager.PlayerTrain].Station;
                            if (j >= 0 & TrainManager.Trains[TrainManager.PlayerTrain].StationState != TrainManager.TrainStopState.Completed) {
                                double d = TrainManager.Trains[TrainManager.PlayerTrain].StationDepartureTime - SecondsSinceMidnight + 1.0;
                                if (d < 0.0) d = 0.0;
                                string s = Messages[i].InternalText;
                                TimeSpan a = TimeSpan.FromSeconds(d);
                                System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
                                string t = a.Hours.ToString("00", Culture) + ":" + a.Minutes.ToString("00", Culture) + ":" + a.Seconds.ToString("00", Culture);
                                s = s.Replace("[time]", t);
                                s = s.Replace("[name]", Stations[j].Name);
                                Messages[i].DisplayText = s;
                                if (d > 0.0) rem = false;
                            } else {
                                rem = true;
                            }
                        } break;
                    default:
                        Messages[i].DisplayText = Messages[i].InternalText;
                        break;
                }
                if (rem) {
                    if (Messages[i].Timeout == double.PositiveInfinity) {
                        Messages[i].Timeout = SecondsSinceMidnight;
                    }
                    if (SecondsSinceMidnight >= Messages[i].Timeout + MessageFadeOutTime) {
                        for (int j = i; j < Messages.Length - 1; j++) {
                            Messages[j] = Messages[j + 1];
                        } i--;
                        Array.Resize<Message>(ref Messages, Messages.Length - 1);
                    }
                }
            }
        }

        // ================================

        // marker
        internal static int[] MarkerTextures = new int[] { };
        internal static void AddMarker(int TextureIndex) {
            int n = MarkerTextures.Length;
            Array.Resize<int>(ref MarkerTextures, n + 1);
            MarkerTextures[n] = TextureIndex;
        }
        internal static void RemoveMarker(int TextureIndex) {
            int n = MarkerTextures.Length;
            for (int i = 0; i < n; i++) {
                if (MarkerTextures[i] == TextureIndex) {
                    for (int j = i; j < n - 1; j++) {
                        MarkerTextures[j] = MarkerTextures[j + 1];
                    } n--;
                    Array.Resize<int>(ref MarkerTextures, n);
                    break;
                }
            }
        }

        // ================================

        // points of interest
        internal struct PointOfInterest {
            internal double TrackPosition;
            internal World.Vector3D TrackOffset;
            internal double TrackYaw;
            internal double TrackPitch;
            internal double TrackRoll;
            internal string Text;
            //internal World.Vector3D Position;
            //internal World.Vector3D Direction;
            //internal World.Vector3D Up;
            //internal World.Vector3D Side;
        }
        internal static PointOfInterest[] PointsOfInterest = new PointOfInterest[] { };
        internal static bool ApplyPointOfView(int Value, bool Relative) {
            double t = 0.0;
            int j = -1;
            if (Relative) {
                // relative
                if (Value < 0) {
                    // previous poi
                    t = double.NegativeInfinity;
                    for (int i = 0; i < PointsOfInterest.Length; i++) {
                        if (PointsOfInterest[i].TrackPosition < World.CameraTrackFollower.TrackPosition) {
                            if (PointsOfInterest[i].TrackPosition > t) {
                                t = PointsOfInterest[i].TrackPosition;
                                j = i;
                            }
                        }
                    }
                } else if (Value > 0) {
                    // next poi
                    t = double.PositiveInfinity;
                    for (int i = 0; i < PointsOfInterest.Length; i++) {
                        if (PointsOfInterest[i].TrackPosition > World.CameraTrackFollower.TrackPosition) {
                            if (PointsOfInterest[i].TrackPosition < t) {
                                t = PointsOfInterest[i].TrackPosition;
                                j = i;
                            }
                        }
                    }
                }
            } else {
                // absolute
                j = Value >= 0 & Value < PointsOfInterest.Length ? Value : -1;
            }
            // process poi
            if (j >= 0) {
                TrackManager.UpdateTrackFollower(ref World.CameraTrackFollower, t, true, false);
                World.CameraCurrentAlignment.TrackOffset = PointsOfInterest[j].TrackOffset;
                World.CameraCurrentAlignment.Yaw = PointsOfInterest[j].TrackYaw;
                World.CameraCurrentAlignment.Pitch = PointsOfInterest[j].TrackPitch;
                World.CameraCurrentAlignment.Roll = PointsOfInterest[j].TrackRoll;
                World.UpdateAbsoluteCamera(0.0);
                if (PointsOfInterest[j].Text != null) {
                    double n = 3.0 + 0.5 * Math.Sqrt((double)PointsOfInterest[j].Text.Length);
                    Game.AddMessage(PointsOfInterest[j].Text, Game.MessageDependency.None, Game.MessageType.Interface, Game.MessageColor.Blue, Game.SecondsSinceMidnight + n);
                }
                return true;
            } else {
                return false;
            }
        }


    }
}