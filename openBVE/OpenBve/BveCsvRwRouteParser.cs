using System;
using System.Collections;

namespace OpenBve {
    internal class BveCsvRwRouteParser {

        // mode
        internal static bool IsRouteViewer = false;

        // structures
        private struct Rail {
            internal bool RailStart;
            internal bool RailStartRefreshed;
            internal double RailStartX;
            internal double RailStartY;
            internal bool RailEnd;
            internal double RailEndX;
            internal double RailEndY;
        }
        private struct WallDike {
            internal bool Exists;
            internal int Type;
            internal int Direction;
        }
        private struct FreeObj {
            internal double TrackPosition;
            internal int Type;
            internal double X;
            internal double Y;
            internal double Yaw;
            internal double Pitch;
            internal double Roll;
            internal int Section;
        }
        private struct Pole {
            internal bool Exists;
            internal int Mode;
            internal double Location;
            internal double Interval;
            internal int Type;
        }
        private struct Form {
            internal int PrimaryRail;
            internal int SecondaryRail;
            internal int FormType;
            internal int RoofType;
            internal const int SecondaryRailStub = 0;
            internal const int SecondaryRailL = -1;
            internal const int SecondaryRailR = -2;
        }
        private struct Crack {
            internal int PrimaryRail;
            internal int SecondaryRail;
            internal int Type;
        }
        private struct Signal {
            internal double TrackPosition;
            internal int Section;
            internal string Name;
            internal int SignalCompatibilityObjectIndex;
            internal int SignalObjectIndex;
            internal double X;
            internal double Y;
            internal double Yaw;
            internal double Pitch;
            internal double Roll;
            internal bool ShowObject;
            internal bool ShowPost;
            internal int GameSignalIndex;
        }
        private struct Section {
            internal double TrackPosition;
            internal int[] Aspects;
            internal int DepartureStationIndex;
            internal bool Invisible;
            internal Game.SectionType Type;
        }
        private struct Limit {
            internal double TrackPosition;
            internal double Speed;
            internal int Direction;
            internal int Cource;
        }
        private struct Stop {
            internal double TrackPosition;
            internal int Station;
            internal int Direction;
            internal double ForwardTolerance;
            internal double BackwardTolerance;
            internal int Cars;
        }
        private struct Brightness {
            internal double TrackPosition;
            internal float Value;
        }
        private struct Marker {
            internal double StartingPosition;
            internal double EndingPosition;
            internal int Texture;
        }
        private enum SoundType { World, TrainStatic, TrainDynamic }
        private struct Sound {
            internal double TrackPosition;
            internal int SoundIndex;
            internal SoundType Type;
            internal double X;
            internal double Y;
            internal double Radius;
            internal double Speed;
        }
        private struct Transponder {
            internal double TrackPosition;
            internal TrackManager.TransponderType Type;
            internal bool ShowDefaultObject;
            internal bool SwitchSubsystem;
            internal int BeaconStructureIndex;
            internal int OptionalInteger;
            internal double OptionalFloat;
            internal int Section;
        }
        private struct PointOfInterest {
            internal double TrackPosition;
            internal int RailIndex;
            internal double X;
            internal double Y;
            internal double Yaw;
            internal double Pitch;
            internal double Roll;
            internal string Text;
        }
        private class Block {
            internal int Background;
            internal Brightness[] Brightness;
            internal Game.Fog Fog;
            internal bool FogDefined;
            internal int[] Cycle;
            internal double Height;
            internal Rail[] Rail;
            internal int[] RailType;
            internal WallDike[] RailWall;
            internal WallDike[] RailDike;
            internal Pole[] RailPole;
            internal FreeObj[][] RailFreeObj;
            internal FreeObj[] GroundFreeObj;
            internal Form[] Form;
            internal Crack[] Crack;
            internal Signal[] Signal;
            internal Section[] Section;
            internal Limit[] Limit;
            internal Stop[] Stop;
            internal Sound[] Sound;
            internal Transponder[] Transponder;
            internal PointOfInterest[] PointsOfInterest;
            internal TrackManager.TrackElement CurrentTrackState;
            internal double Pitch;
            internal double Turn;
            internal int Station;
            internal bool StationPassAlarm;
            internal double Accuracy;
            internal double AdhesionMultiplier;
        }
        private struct StructureData {
            internal ObjectManager.UnifiedObject[] Rail;
            internal ObjectManager.UnifiedObject[][] Poles;
            internal ObjectManager.UnifiedObject[] Ground;
            internal ObjectManager.UnifiedObject[] WallL;
            internal ObjectManager.UnifiedObject[] WallR;
            internal ObjectManager.UnifiedObject[] DikeL;
            internal ObjectManager.UnifiedObject[] DikeR;
            internal ObjectManager.UnifiedObject[] FormL;
            internal ObjectManager.UnifiedObject[] FormR;
            internal ObjectManager.StaticObject[] FormCL;
            internal ObjectManager.StaticObject[] FormCR;
            internal ObjectManager.UnifiedObject[] RoofL;
            internal ObjectManager.UnifiedObject[] RoofR;
            internal ObjectManager.StaticObject[] RoofCL;
            internal ObjectManager.StaticObject[] RoofCR;
            internal ObjectManager.StaticObject[] CrackL;
            internal ObjectManager.StaticObject[] CrackR;
            internal ObjectManager.UnifiedObject[] FreeObj;
            internal ObjectManager.UnifiedObject[] Beacon;
            internal int[][] Cycle;
            internal int[] Run;
            internal int[] Flange;
        }
        private abstract class SignalData { }
        private class Bve4SignalData : SignalData {
            internal ObjectManager.StaticObject BaseObject;
            internal ObjectManager.StaticObject GlowObject;
            internal int[] DaylightTextures;
            internal int[] GlowTextures;
        }
        private class CompatibilitySignalData : SignalData {
            internal int[] Numbers;
            internal ObjectManager.StaticObject[] Objects;
            internal CompatibilitySignalData(int[] Numbers, ObjectManager.StaticObject[] Objects) {
                this.Numbers = Numbers;
                this.Objects = Objects;
            }
        }
        private class AnimatedObjectSignalData : SignalData {
            internal ObjectManager.AnimatedObjectCollection Objects;
        }
        private struct RouteData {
            internal double TrackPosition;
            internal double BlockInterval;
            internal double UnitOfSpeed;
            internal bool AccurateObjectDisposal;
            internal bool FogTransitionMode;
            internal StructureData Structure;
            internal SignalData[] SignalData;
            internal CompatibilitySignalData[] CompatibilitySignalData;
            internal int[] TimetableDaytime;
            internal int[] TimetableNighttime;
            internal World.Background[] Backgrounds;
            internal double[] SignalSpeeds;
            internal Block[] Blocks;
            internal Marker[] Markers;
            internal int FirstUsedBlock;
        }

        // parse route
        internal static void ParseRoute(string FileName, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, string CompatibilityPath, bool PreviewOnly) {
            // initialize data
            bool IsRW = string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
            string CompatibilityFolder = Interface.GetCombinedFolderName(System.Windows.Forms.Application.StartupPath, "Compatibility");
            RouteData Data = new RouteData();
            Data.BlockInterval = 25.0;
            Data.AccurateObjectDisposal = false;
            Data.FirstUsedBlock = -1;
            Data.Blocks = new Block[1];
            Data.Blocks[0] = new Block();
            Data.Blocks[0].Rail = new Rail[1];
            Data.Blocks[0].Rail[0].RailStart = true;
            Data.Blocks[0].RailType = new int[] { 0 };
            Data.Blocks[0].Limit = new Limit[] { };
            Data.Blocks[0].Stop = new Stop[] { };
            Data.Blocks[0].Station = -1;
            Data.Blocks[0].StationPassAlarm = false;
            Data.Blocks[0].Accuracy = 2.0;
            Data.Blocks[0].AdhesionMultiplier = 1.0;
            Data.Blocks[0].CurrentTrackState = new TrackManager.TrackElement(0.0);
            if (!PreviewOnly) {
                Data.Blocks[0].Background = 0;
                Data.Blocks[0].Brightness = new Brightness[] { };
                Data.Blocks[0].Fog.Start = (float)(World.BackgroundImageDistance + World.ExtraViewingDistance);
                Data.Blocks[0].Fog.End = (float)(World.BackgroundImageDistance + 2.0 * World.ExtraViewingDistance);
                Data.Blocks[0].Fog.Color = new World.ColorRGB(255, 255, 255);
                Data.Blocks[0].Cycle = new int[] { -1 };
                Data.Blocks[0].Height = IsRW ? 0.3 : 0.0;
                Data.Blocks[0].RailFreeObj = new FreeObj[][] { };
                Data.Blocks[0].GroundFreeObj = new FreeObj[] { };
                Data.Blocks[0].RailWall = new WallDike[] { };
                Data.Blocks[0].RailDike = new WallDike[] { };
                Data.Blocks[0].RailPole = new Pole[] { };
                Data.Blocks[0].Form = new Form[] { };
                Data.Blocks[0].Crack = new Crack[] { };
                Data.Blocks[0].Signal = new Signal[] { };
                Data.Blocks[0].Section = new Section[] { };
                Data.Blocks[0].Sound = new Sound[] { };
                Data.Blocks[0].Transponder = new Transponder[] { };
                Data.Blocks[0].PointsOfInterest = new PointOfInterest[] { };
                Data.Markers = new Marker[] { };
                string PoleFolder = Interface.GetCombinedFolderName(CompatibilityFolder, "poles");
                Data.Structure.Poles = new ObjectManager.UnifiedObject[][] {
                    new ObjectManager.UnifiedObject[] {
                        ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (PoleFolder, "pole_1.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false)
                    }, new ObjectManager.UnifiedObject[] {
                        ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (PoleFolder, "pole_2.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false)
                    }, new ObjectManager.UnifiedObject[] {
                        ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (PoleFolder, "pole_3.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false)
                    }, new ObjectManager.UnifiedObject[] {
                        ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (PoleFolder, "pole_4.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false)
                    }
                };
                Data.Structure.Rail = new ObjectManager.UnifiedObject[] { };
                Data.Structure.Ground = new ObjectManager.UnifiedObject[] { };
                Data.Structure.WallL = new ObjectManager.UnifiedObject[] { };
                Data.Structure.WallR = new ObjectManager.UnifiedObject[] { };
                Data.Structure.DikeL = new ObjectManager.UnifiedObject[] { };
                Data.Structure.DikeR = new ObjectManager.UnifiedObject[] { };
                Data.Structure.FormL = new ObjectManager.UnifiedObject[] { };
                Data.Structure.FormR = new ObjectManager.UnifiedObject[] { };
                Data.Structure.FormCL = new ObjectManager.StaticObject[] { };
                Data.Structure.FormCR = new ObjectManager.StaticObject[] { };
                Data.Structure.RoofL = new ObjectManager.UnifiedObject[] { };
                Data.Structure.RoofR = new ObjectManager.UnifiedObject[] { };
                Data.Structure.RoofCL = new ObjectManager.StaticObject[] { };
                Data.Structure.RoofCR = new ObjectManager.StaticObject[] { };
                Data.Structure.CrackL = new ObjectManager.StaticObject[] { };
                Data.Structure.CrackR = new ObjectManager.StaticObject[] { };
                Data.Structure.FreeObj = new ObjectManager.UnifiedObject[] { };
                Data.Structure.Beacon = new ObjectManager.UnifiedObject[] { };
                Data.Structure.Cycle = new int[][] { };
                Data.Structure.Run = new int[] { };
                Data.Structure.Flange = new int[] { };
                Data.Backgrounds = new World.Background[] { };
                Data.TimetableDaytime = new int[] { -1, -1, -1, -1 };
                Data.TimetableNighttime = new int[] { -1, -1, -1, -1 };
                // signals
                string SignalFolder = Interface.GetCombinedFolderName(CompatibilityFolder, "signals");
                Data.SignalData = new SignalData[7];
                Data.SignalData[3] = new CompatibilitySignalData(new int[] { 0, 2, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_3_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_3_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_3_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.SignalData[4] = new CompatibilitySignalData(new int[] { 0, 1, 2, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_4_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_4a_1.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_4a_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName (SignalFolder, "signal_4a_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.SignalData[5] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5a_1.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.SignalData[6] = new CompatibilitySignalData(new int[] { 0, 3, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                // compatibility signals
                Data.CompatibilitySignalData = new CompatibilitySignalData[9];
                Data.CompatibilitySignalData[0] = new CompatibilitySignalData(new int[] { 0, 2 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_2_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_2a_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[1] = new CompatibilitySignalData(new int[] { 0, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_2_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_2b_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[2] = new CompatibilitySignalData(new int[] { 0, 2, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_3_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_3_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_3_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[3] = new CompatibilitySignalData(new int[] { 0, 1, 2, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4a_1.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4a_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4a_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[4] = new CompatibilitySignalData(new int[] { 0, 2, 3, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4b_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4b_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_4b_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[5] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5a_1.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[6] = new CompatibilitySignalData(new int[] { 0, 2, 3, 4, 5 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_5b_5.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[7] = new CompatibilitySignalData(new int[] { 0, 1, 2, 3, 4, 5 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_1.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_2.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "signal_6_5.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                Data.CompatibilitySignalData[8] = new CompatibilitySignalData(new int[] { 0, 3, 4 }, new ObjectManager.StaticObject[] { 
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_0.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_3.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false),
                    ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalFolder, "repeatingsignal_4.csv"), Encoding, ObjectManager.ObjectLoadMode.PreloadTextures, false, false)
                });
                // game data
                Game.Sections = new Game.Section[1];
                Game.Sections[0].Aspects = new Game.SectionAspect[] { new Game.SectionAspect(0, 0.0), new Game.SectionAspect(4, double.PositiveInfinity) };
                Game.Sections[0].CurrentAspect = 0;
                Game.Sections[0].NextSection = -1;
                Game.Sections[0].PreviousSection = -1;
                Game.Sections[0].SignalIndices = new int[] { };
                Game.Sections[0].StationIndex = -1;
                Game.Sections[0].TrackPosition = 0;
                Game.Sections[0].TrainIndices = new int[] { };
                // continue
                Data.SignalSpeeds = new double[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };
            }
            ParseRouteForData(FileName, Encoding, TrainPath, ObjectPath, SoundPath, ref Data, PreviewOnly);
            if (Loading.Cancel) return;
            ApplyRouteData(FileName, Encoding, CompatibilityPath, ref Data, PreviewOnly);
        }

        // ================================

        // parse route for data
        private class Expression {
            internal string Text;
            internal int Line;
            internal int Column;
        }
        private static void ParseRouteForData(string FileName, System.Text.Encoding Encoding, string TrainPath, string ObjectPath, string SoundPath, ref RouteData Data, bool PreviewOnly) {
            // parse
            string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
            Expression[] Expressions;
            PreprocessSplitIntoExpressions(FileName, Lines, out Expressions);
            PreprocessChrRndSub(FileName, ref Expressions);
            double[] UnitOfLength = new double[] { 1.0 };
            Data.UnitOfSpeed = 0.277777777777778;
            PreprocessOptions(FileName, Encoding, Expressions, ref Data, ref UnitOfLength);
            PreprocessSortByTrackPosition(FileName, UnitOfLength, ref Expressions);
            ParseRouteForData(FileName, Encoding, Expressions, TrainPath, ObjectPath, SoundPath, UnitOfLength, ref Data, PreviewOnly);
        }

        // preprocess split into expressions
        private static void PreprocessSplitIntoExpressions(string FileName, string[] Lines, out Expression[] Expressions) {
            bool IsRW = string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
            Expressions = new Expression[4096]; int e = 0;
            bool RWRouteDescription = true;
            for (int i = 0; i < Lines.Length; i++) {
                if (IsRW) {
                    // ignore rw route description
                    if (RWRouteDescription) {
                        if (Lines[i].StartsWith("[", StringComparison.OrdinalIgnoreCase) & Lines[i].IndexOf("]", StringComparison.OrdinalIgnoreCase) > 0) {
                            RWRouteDescription = false;
                            Game.RouteComment = Game.RouteComment.Trim();
                        } else {
                            if (Game.RouteComment.Length != 0) Game.RouteComment += "\n";
                            Game.RouteComment += Lines[i];
                            continue;
                        }
                    }
                    // strip away RW comments
                    int j = Lines[i].IndexOf(';');
                    if (j >= 0) {
                        Lines[i] = Lines[i].Substring(0, j).TrimEnd();
                    }
                }
                {
                    // count expressions
                    int n = 0; int Level = 0;
                    for (int j = 0; j < Lines[i].Length; j++) {
                        switch (Lines[i][j]) {
                            case '(':
                                Level++;
                                break;
                            case ')':
                                Level--;
                                break;
                            case ',':
                                if (Level == 0) n++;
                                break;
                            case '@':
                                if (IsRW & Level == 0) n++;
                                break;
                        }
                    }
                    // create expressions
                    int m = e + n + 1;
                    while (m >= Expressions.Length) {
                        Array.Resize<Expression>(ref Expressions, Expressions.Length << 1);
                    }
                    Level = 0;
                    int a = 0, c = 0;
                    for (int j = 0; j < Lines[i].Length; j++) {
                        switch (Lines[i][j]) {
                            case '(':
                                Level++;
                                break;
                            case ')':
                                Level--;
                                break;
                            case ',':
                            case '@':
                                if (Level == 0 & (IsRW | Lines[i][j] != '@')) {
                                    string t = Lines[i].Substring(a, j - a).Trim();
                                    if (t.Length > 0 && !t.StartsWith(";")) {
                                        Expressions[e] = new Expression();
                                        Expressions[e].Text = t;
                                        Expressions[e].Line = i + 1;
                                        Expressions[e].Column = c + 1;
                                        e++;
                                    }
                                    a = j + 1;
                                    c++;
                                }
                                break;
                        }
                    }
                    if (Lines[i].Length - a > 0) {
                        string t = Lines[i].Substring(a).Trim();
                        if (t.Length > 0 && !t.StartsWith(";")) {
                            Expressions[e] = new Expression();
                            Expressions[e].Text = t;
                            Expressions[e].Line = i + 1;
                            Expressions[e].Column = c + 1;
                            e++;
                        }
                    }
                }
            }
            Array.Resize<Expression>(ref Expressions, e);
        }

        // preprocess chrrndsub
        private static void PreprocessChrRndSub(string FileName, ref Expression[] Expressions) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            System.Text.Encoding Encoding = new System.Text.ASCIIEncoding();
            string[] Subs = new string[16];
            for (int i = 0; i < Expressions.Length; i++) {
                string Epilog = " at line " + Expressions[i].Line.ToString(Culture) + ", column " + Expressions[i].Column.ToString(Culture) + " in file " + FileName;
                bool err = false;
                for (int j = Expressions[i].Text.Length - 1; j >= 0; j--) {
                    if (Expressions[i].Text[j] == '$') {
                        int k; for (k = j + 1; k < Expressions[i].Text.Length; k++) {
                            if (Expressions[i].Text[k] == '(') break;
                        } if (k <= Expressions[i].Text.Length) {
                            string t = Expressions[i].Text.Substring(j, k - j).TrimEnd();
                            int l = 1, h;
                            for (h = k + 1; h < Expressions[i].Text.Length; h++) {
                                switch (Expressions[i].Text[h]) {
                                    case '(': l++; break;
                                    case ')':
                                        l--; if (l < 0) {
                                            err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Invalid paranthesis structure in " + t + Epilog);
                                        } break;
                                } if (l <= 0) break;
                            } if (err) break;
                            if (l != 0) {
                                Interface.AddMessage(Interface.MessageType.Error, false, "Invalid paranthesis structure in " + t + Epilog);
                                err = true; break;
                            }
                            string s = Expressions[i].Text.Substring(k + 1, h - k - 1).Trim();
                            switch (t.ToLowerInvariant()) {
                                case "$chr": {
                                        int x; if (Interface.TryParseIntVb6(s, out x)) {
                                            if (x > 0 & x < 128) {
                                                Expressions[i].Text = Expressions[i].Text.Substring(0, j) + new string(Encoding.GetChars(new byte[] { (byte)x })) + Expressions[i].Text.Substring(h + 1);
                                            } else {
                                                err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index does not correspond to a valid ASCII character in " + t + Epilog);
                                            }
                                        } else {
                                            err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in " + t + Epilog);
                                        }
                                    } break;
                                case "$rnd": {
                                        int m = s.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                                        if (m >= 0) {
                                            string s1 = s.Substring(0, m).TrimEnd();
                                            string s2 = s.Substring(m + 1).TrimStart();
                                            int x; if (Interface.TryParseIntVb6(s1, out x)) {
                                                int y; if (Interface.TryParseIntVb6(s2, out y)) {
                                                    int z = x + (int)Math.Floor(Game.Generator.NextDouble() * (double)(y - x + 1));
                                                    Expressions[i].Text = Expressions[i].Text.Substring(0, j) + z.ToString(Culture) + Expressions[i].Text.Substring(h + 1);
                                                } else {
                                                    err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is invalid in " + t + Epilog);
                                                }
                                            } else {
                                                err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is invalid in " + t + Epilog);
                                            }
                                        } else {
                                            err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Two arguments are expected in " + t + Epilog);
                                        }
                                    } break;
                                case "$sub": {
                                        int m = s.IndexOf(";", StringComparison.OrdinalIgnoreCase);
                                        if (m >= 0) {
                                            string s1 = s.Substring(0, m).TrimEnd();
                                            string s2 = s.Substring(m + 1).TrimStart();
                                            int x; if (Interface.TryParseIntVb6(s1, out x)) {
                                                if (x >= 0) {
                                                    while (x >= Subs.Length) Array.Resize<string>(ref Subs, Subs.Length << 1);
                                                    Subs[x] = s2;
                                                    Expressions[i].Text = Expressions[i].Text.Substring(0, j) + s2 + Expressions[i].Text.Substring(h + 1);
                                                } else {
                                                    err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative in " + t + Epilog);
                                                }
                                            } else {
                                                err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in " + t + Epilog);
                                            }
                                        } else {
                                            l = 0; bool f = false;
                                            for (m = h + 1; m < Expressions[i].Text.Length; m++) {
                                                switch (Expressions[i].Text[m]) {
                                                    case '(': l++; break;
                                                    case ')': l--; break;
                                                    case '=': if (l == 0) {
                                                            f = true;
                                                        } break;
                                                    default:
                                                        if (!char.IsWhiteSpace(Expressions[i].Text[m])) l = -1;
                                                        break;
                                                } if (f | l < 0) break;
                                            } if (f) {
                                                l = 0;
                                                int n; for (n = m + 1; n < Expressions[i].Text.Length; n++) {
                                                    switch (Expressions[i].Text[n]) {
                                                        case '(': l++; break;
                                                        case ')': l--; break;
                                                    } if (l < 0) break;
                                                }
                                                int x; if (Interface.TryParseIntVb6(s, out x)) {
                                                    if (x >= 0) {
                                                        while (x >= Subs.Length) Array.Resize<string>(ref Subs, Subs.Length << 1);
                                                        Subs[x] = Expressions[i].Text.Substring(m + 1, n - m - 1).Trim();
                                                        Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Expressions[i].Text.Substring(n);
                                                    } else {
                                                        err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative in " + t + Epilog);
                                                    }
                                                } else {
                                                    err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in " + t + Epilog);
                                                }
                                            } else {
                                                int x; if (Interface.TryParseIntVb6(s, out x)) {
                                                    if (x >= 0 & x < Subs.Length && Subs[x] != null) {
                                                        Expressions[i].Text = Expressions[i].Text.Substring(0, j) + Subs[x] + Expressions[i].Text.Substring(h + 1);
                                                    } else {
                                                        err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is out of range in " + t + Epilog);
                                                    }
                                                } else {
                                                    err = true; Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in " + t + Epilog);
                                                }
                                            }
                                        }
                                    } break;
                            }
                        }
                    } if (err) continue;
                }
            }
        }

        // preprocess options
        private static void PreprocessOptions(string FileName, System.Text.Encoding Encoding, Expression[] Expressions, ref RouteData Data, ref double[] UnitOfLength) {
            bool IsRW = string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            string Section = ""; bool SectionAlwaysPrefix = false;
            // process expressions
            for (int j = 0; j < Expressions.Length; j++) {
                if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
                    Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
                    if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
                        Section = "Structure";
                    } else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
                        Section = "Track";
                    }
                    SectionAlwaysPrefix = true;
                } else {
                    // find equals
                    int Equals = Expressions[j].Text.IndexOf('=');
                    if (Equals >= 0) {
                        // handle RW cycle syntax
                        string t = Expressions[j].Text.Substring(0, Equals);
                        if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix) {
                            double b; if (Interface.TryParseDoubleVb6(t, out b)) {
                                t = ".Ground(" + t + ")";
                            }
                        }
                        // convert RW style into CSV style
                        Expressions[j].Text = t + " " + Expressions[j].Text.Substring(Equals + 1);
                    }
                    // separate command and arguments
                    string Command, ArgumentSequence;
                    SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, FileName, j, true);
                    // process command
                    double Number;
                    bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
                    if (!NumberCheck || !Interface.TryParseDoubleVb6(Command, UnitOfLength, out Number)) {
                        // split arguments
                        string[] Arguments;
                        {
                            int n = 0;
                            for (int k = 0; k < ArgumentSequence.Length; k++) {
                                switch (ArgumentSequence[k]) {
                                    case ';':
                                    case ',':
                                        n++;
                                        break;
                                }
                            }
                            Arguments = new string[n + 1];
                            int a = 0, h = 0;
                            for (int k = 0; k < ArgumentSequence.Length; k++) {
                                switch (ArgumentSequence[k]) {
                                    case ';':
                                    case ',':
                                        Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
                                        a = k + 1; h++;
                                        break;
                                }
                            }
                            if (ArgumentSequence.Length - a > 0) {
                                Arguments[h] = ArgumentSequence.Substring(a).Trim();
                                h++;
                            }
                            Array.Resize<string>(ref Arguments, h);
                        }
                        // preprocess command
                        if (Command.ToLowerInvariant() == "with") {
                            if (Arguments.Length >= 1) {
                                Section = Arguments[0];
                                SectionAlwaysPrefix = false;
                            } else {
                                Section = "";
                                SectionAlwaysPrefix = false;
                            }
                            Command = null;
                        } else {
                            if (Command.StartsWith(".")) {
                                Command = Section + Command;
                            } else if (SectionAlwaysPrefix) {
                                Command = Section + "." + Command;
                            }
                        }
                        // handle indices
                        int CommandIndex1 = 0, CommandIndex2 = 0;
                        if (Command != null && Command.EndsWith(")")) {
                            for (int k = Command.Length - 2; k >= 0; k--) {
                                if (Command[k] == '(') {
                                    string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
                                    Command = Command.Substring(0, k).TrimEnd();
                                    int h = Indices.IndexOf(";");
                                    if (h >= 0) {
                                        string a = Indices.Substring(0, h).TrimEnd();
                                        string b = Indices.Substring(h + 1).TrimStart();
                                        if (a.Length > 0 && !Interface.TryParseIntVb6(a, out CommandIndex1)) {
                                            Command = null; break;
                                        } else if (b.Length > 0 && !Interface.TryParseIntVb6(b, out CommandIndex2)) {
                                            Command = null; break;
                                        }
                                    } else {
                                        if (Indices.Length > 0 && !Interface.TryParseIntVb6(Indices, out CommandIndex1)) {
                                            Command = null; break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        // process command
                        if (Command != null) {
                            switch (Command.ToLowerInvariant()) {
                                // options
                                case "options.unitoflength": {
                                        if (Arguments.Length == 0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "At least 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            UnitOfLength = new double[Arguments.Length];
                                            for (int i = 0; i < Arguments.Length; i++) {
                                                UnitOfLength[i] = i == 0 ? 1.0 : 0.0;
                                                if (Arguments[i].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[i], out UnitOfLength[i])) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "FactorInMeters" + i.ToString(Culture) + " is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    UnitOfLength[i] = i == 0 ? 1.0 : 0.0;
                                                }
                                            }
                                        }
                                    } break;
                                case "options.unitofspeed": {
                                        if (Arguments.Length < 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            if (Arguments.Length > 1) {
                                                Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            }
                                            if (Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out Data.UnitOfSpeed)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "FactorInKmph is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                Data.UnitOfSpeed = 0.277777777777778;
                                            } else if (Data.UnitOfSpeed <= 0.0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "FactorInKmph is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                Data.UnitOfSpeed = 0.277777777777778;
                                            }
                                        }
                                    } break;
                                case "options.objectvisibility": {
                                        if (Arguments.Length == 0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            if (Arguments.Length > 1) {
                                                Interface.AddMessage(Interface.MessageType.Warning, false, "Exactly 1 argument is expected in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            }
                                            int mode = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length != 0 && !Interface.TryParseIntVb6(Arguments[0], out mode)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                mode = 0;
                                            } else if (mode != 0 & mode != 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "The specified Mode is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                mode = 0;
                                            }
                                            Data.AccurateObjectDisposal = mode == 1;
                                        }
                                    } break;
                            }
                        }
                    }
                }
            }
        }

        // preprocess sort by track position
        private struct PositionedExpression {
            internal double TrackPosition;
            internal Expression Expression;
        }
        private static void PreprocessSortByTrackPosition(string FileName, double[] UnitFactors, ref Expression[] Expressions) {
            bool IsRW = string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            PositionedExpression[] p = new PositionedExpression[Expressions.Length]; int n = 0;
            double a = -1.0;
            bool NumberCheck = !IsRW;
            for (int i = 0; i < Expressions.Length; i++) {
                if (IsRW) {
                    // only check for track positions for the railway section in RW routes
                    if (Expressions[i].Text.StartsWith("[", StringComparison.OrdinalIgnoreCase) & Expressions[i].Text.EndsWith("]", StringComparison.OrdinalIgnoreCase)) {
                        string s = Expressions[i].Text.Substring(1, Expressions[i].Text.Length - 2).Trim();
                        if (string.Compare(s, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
                            NumberCheck = true;
                        } else {
                            NumberCheck = false;
                        }
                    }
                }
                double x; if (NumberCheck && Interface.TryParseDoubleVb6(Expressions[i].Text, UnitFactors, out x)) {
                    a = x;
                } else {
                    p[n].TrackPosition = a;
                    p[n].Expression = Expressions[i];
                    int j = n; n++;
                    while (j > 0) {
                        if (p[j].TrackPosition < p[j - 1].TrackPosition) {
                            PositionedExpression t = p[j];
                            p[j] = p[j - 1];
                            p[j - 1] = t;
                            j--;
                        } else break;
                    }
                }
            }
            a = -1.0;
            Expression[] e = new Expression[Expressions.Length]; int m = 0;
            for (int i = 0; i < n; i++) {
                if (p[i].TrackPosition != a) {
                    a = p[i].TrackPosition;
                    e[m] = new Expression();
                    e[m].Text = a.ToString(Culture);
                    e[m].Line = -1;
                    e[m].Column = -1;
                    m++;
                }
                e[m] = p[i].Expression;
                m++;
            }
            Array.Resize<Expression>(ref e, m);
            Expressions = e;
        }

        // separate commands and arguments
        private static void SeparateCommandsAndArguments(Expression Expression, out string Command, out string ArgumentSequence, System.Globalization.CultureInfo Culture, string FileName, int LineNumber, bool RaiseErrors) {
            bool openingerror = false, closingerror = false;
            int i; for (i = 0; i < Expression.Text.Length; i++) {
                if (Expression.Text[i] == '(') {
                    bool found = false;
                    i++; while (i < Expression.Text.Length) {
                        if (Expression.Text[i] == '(') {
                            if (RaiseErrors & !openingerror) {
                                Interface.AddMessage(Interface.MessageType.Error, false, "Invalid opening parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                                openingerror = true;
                            }
                        } else if (Expression.Text[i] == ')') {
                            found = true; break;
                        } i++;
                    }
                    if (!found) {
                        if (RaiseErrors & !closingerror) {
                            Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                            closingerror = true;
                        }
                        Expression.Text += ")";
                    }
                } else if (Expression.Text[i] == ')') {
                    if (RaiseErrors & !closingerror) {
                        Interface.AddMessage(Interface.MessageType.Error, false, "Invalid closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                        closingerror = true;
                    }
                } else if (char.IsWhiteSpace(Expression.Text[i])) {
                    if (i >= Expression.Text.Length - 1 || !char.IsWhiteSpace(Expression.Text[i + 1])) {
                        break;
                    }
                }
            }
            if (i < Expression.Text.Length) {
                // space was found outside of parantheses
                string a = Expression.Text.Substring(0, i);
                if (a.IndexOf('(') >= 0 & a.IndexOf(')') >= 0) {
                    // indices found not separated from the command by spaces
                    Command = Expression.Text.Substring(0, i).TrimEnd();
                    ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
                    if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
                        // arguments are enclosed by parantheses
                        ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
                    } else if (ArgumentSequence.StartsWith("(")) {
                        // only opening parantheses found
                        if (RaiseErrors & !closingerror) {
                            Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                        }
                        ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
                    }
                } else {
                    // no indices found before the space
                    if (i < Expression.Text.Length - 1 && Expression.Text[i + 1] == '(') {
                        // opening parantheses follows the space
                        int j = Expression.Text.IndexOf(')', i + 1);
                        if (j > i + 1) {
                            // closing parantheses found
                            if (j == Expression.Text.Length - 1) {
                                // only closing parantheses found at the end of the expression
                                Command = Expression.Text.Substring(0, i).TrimEnd();
                                ArgumentSequence = Expression.Text.Substring(i + 2, j - i - 2).Trim();
                            } else {
                                // detect border between indices and arguments
                                bool found = false;
                                Command = null; ArgumentSequence = null;
                                for (int k = j + 1; k < Expression.Text.Length; k++) {
                                    if (char.IsWhiteSpace(Expression.Text[k])) {
                                        Command = Expression.Text.Substring(0, k).TrimEnd();
                                        ArgumentSequence = Expression.Text.Substring(k + 1).TrimStart();
                                        found = true; break;
                                    } else if (Expression.Text[k] == '(') {
                                        Command = Expression.Text.Substring(0, k).TrimEnd();
                                        ArgumentSequence = Expression.Text.Substring(k).TrimStart();
                                        found = true; break;
                                    }
                                }
                                if (!found) {
                                    if (RaiseErrors & !openingerror & !closingerror) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "Invalid syntax encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                                        openingerror = true;
                                        closingerror = true;
                                    }
                                    Command = Expression.Text;
                                    ArgumentSequence = "";
                                }
                                if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
                                    // arguments are enclosed by parantheses
                                    ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
                                } else if (ArgumentSequence.StartsWith("(")) {
                                    // only opening parantheses found
                                    if (RaiseErrors & !closingerror) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                                    }
                                    ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
                                }
                            }
                        } else {
                            // no closing parantheses found
                            if (RaiseErrors & !closingerror) {
                                Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                            }
                            Command = Expression.Text.Substring(0, i).TrimEnd();
                            ArgumentSequence = Expression.Text.Substring(i + 2).TrimStart();
                        }
                    } else {
                        // no index possible
                        Command = Expression.Text.Substring(0, i).TrimEnd();
                        ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
                        if (ArgumentSequence.StartsWith("(") & ArgumentSequence.EndsWith(")")) {
                            // arguments are enclosed by parantheses
                            ArgumentSequence = ArgumentSequence.Substring(1, ArgumentSequence.Length - 2).Trim();
                        } else if (ArgumentSequence.StartsWith("(")) {
                            // only opening parantheses found
                            if (RaiseErrors & !closingerror) {
                                Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                            }
                            ArgumentSequence = ArgumentSequence.Substring(1).TrimStart();
                        }
                    }
                }
            } else {
                // no single space found
                if (Expression.Text.EndsWith(")")) {
                    i = Expression.Text.LastIndexOf('(');
                    if (i >= 0) {
                        Command = Expression.Text.Substring(0, i).TrimEnd();
                        ArgumentSequence = Expression.Text.Substring(i + 1, Expression.Text.Length - i - 2).Trim();
                    } else {
                        Command = Expression.Text;
                        ArgumentSequence = "";
                    }
                } else {
                    i = Expression.Text.IndexOf('(');
                    if (i >= 0) {
                        if (RaiseErrors & !closingerror) {
                            Interface.AddMessage(Interface.MessageType.Error, false, "Missing closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                        }
                        Command = Expression.Text.Substring(0, i).TrimEnd();
                        ArgumentSequence = Expression.Text.Substring(i + 1).TrimStart();
                    } else {
                        if (RaiseErrors) {
                            i = Expression.Text.IndexOf(')');
                            if (i >= 0 & !closingerror) {
                                Interface.AddMessage(Interface.MessageType.Error, false, "Invalid closing parantheses encountered at line " + Expression.Line.ToString(Culture) + ", column " + Expression.Column.ToString(Culture) + " in file " + FileName);
                            }
                        }
                        Command = Expression.Text;
                        ArgumentSequence = "";
                    }
                }
            }
        }

        // parse route for data
        private static void ParseRouteForData(string FileName, System.Text.Encoding Encoding, Expression[] Expressions, string TrainPath, string ObjectPath, string SoundPath, double[] UnitOfLength, ref RouteData Data, bool PreviewOnly) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            bool IsRW = string.Equals(System.IO.Path.GetExtension(FileName), ".rw", StringComparison.OrdinalIgnoreCase);
            string Section = ""; bool SectionAlwaysPrefix = false;
            int BlockIndex = 0;
            int BlocksUsed = Data.Blocks.Length;
            Game.Stations = new Game.Station[] { };
            int CurrentStation = -1;
            int CurrentStop = -1;
            bool DepartureSignalUsed = false;
            int CurrentSection = 0;
            bool ValueBasedSections = false;
            // process expressions
            double invfac = Expressions.Length == 0 ? 0.5 : 0.5 / (double)Expressions.Length;
            for (int j = 0; j < Expressions.Length; j++) {
                Loading.RouteProgress = (double)j * invfac;
                if ((j & 255) == 0) {
                    System.Threading.Thread.Sleep(1);
                    if (Loading.Cancel) return;
                }
                if (Expressions[j].Text.StartsWith("[") & Expressions[j].Text.EndsWith("]")) {
                    Section = Expressions[j].Text.Substring(1, Expressions[j].Text.Length - 2).Trim();
                    if (string.Compare(Section, "object", StringComparison.OrdinalIgnoreCase) == 0) {
                        Section = "Structure";
                    } else if (string.Compare(Section, "railway", StringComparison.OrdinalIgnoreCase) == 0) {
                        Section = "Track";
                    }
                    SectionAlwaysPrefix = true;
                } else {
                    // find equals
                    int Equals = Expressions[j].Text.IndexOf('=');
                    if (Equals >= 0) {
                        // handle RW cycle syntax
                        string t = Expressions[j].Text.Substring(0, Equals);
                        if (Section.ToLowerInvariant() == "cycle" & SectionAlwaysPrefix) {
                            double b; if (Interface.TryParseDoubleVb6(t, out b)) {
                                t = ".Ground(" + t + ")";
                            }
                        }
                        // convert RW style into CSV style
                        Expressions[j].Text = t + " " + Expressions[j].Text.Substring(Equals + 1);
                    }
                    // separate command and arguments
                    string Command, ArgumentSequence;
                    SeparateCommandsAndArguments(Expressions[j], out Command, out ArgumentSequence, Culture, FileName, j, false);
                    // process command
                    double Number;
                    bool NumberCheck = !IsRW || string.Compare(Section, "track", StringComparison.OrdinalIgnoreCase) == 0;
                    if (NumberCheck && Interface.TryParseDoubleVb6(Command, UnitOfLength, out Number)) {
                        // track position
                        Data.TrackPosition = Number;
                        BlockIndex = (int)Math.Floor(Number / Data.BlockInterval + 0.001);
                        if (Data.FirstUsedBlock == -1) Data.FirstUsedBlock = BlockIndex;
                        CreateMissingBlocks(ref Data, ref BlocksUsed, BlockIndex, PreviewOnly);
                    } else {
                        // split arguments
                        string[] Arguments;
                        {
                            int n = 0;
                            for (int k = 0; k < ArgumentSequence.Length; k++) {
                                switch (ArgumentSequence[k]) {
                                    case ';':
                                    case ',':
                                        n++;
                                        break;
                                }
                            }
                            Arguments = new string[n + 1];
                            int a = 0, h = 0;
                            for (int k = 0; k < ArgumentSequence.Length; k++) {
                                switch (ArgumentSequence[k]) {
                                    case ';':
                                    case ',':
                                        Arguments[h] = ArgumentSequence.Substring(a, k - a).Trim();
                                        a = k + 1; h++;
                                        break;
                                }
                            }
                            if (ArgumentSequence.Length - a > 0) {
                                Arguments[h] = ArgumentSequence.Substring(a).Trim();
                                h++;
                            }
                            Array.Resize<string>(ref Arguments, h);
                        }
                        // preprocess command
                        if (Command.ToLowerInvariant() == "with") {
                            if (Arguments.Length >= 1) {
                                Section = Arguments[0];
                                SectionAlwaysPrefix = false;
                            } else {
                                Section = "";
                                SectionAlwaysPrefix = false;
                            }
                            Command = null;
                        } else {
                            if (Command.StartsWith(".")) {
                                Command = Section + Command;
                            } else if (SectionAlwaysPrefix) {
                                Command = Section + "." + Command;
                            }
                            if (Command.StartsWith("structure", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 5).TrimEnd();
                            } else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 5).TrimEnd();
                            } else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".x", StringComparison.OrdinalIgnoreCase)) {
                                Command = "texture.background.x" + Command.Substring(18, Command.Length - 20).TrimEnd();
                            } else if (Command.StartsWith("texture.background", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".aspect", StringComparison.OrdinalIgnoreCase)) {
                                Command = "texture.background.aspect" + Command.Substring(18, Command.Length - 25).TrimEnd();
                            } else if (Command.StartsWith("cycle", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".params", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 7).TrimEnd();
                            } else if (Command.StartsWith("signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".load", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 5).TrimEnd();
                            } else if (Command.StartsWith("train.run", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 4).TrimEnd();
                            } else if (Command.StartsWith("train.flange", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 4).TrimEnd();
                            } else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".day.load", StringComparison.OrdinalIgnoreCase)) {
                                Command = "train.timetable.day" + Command.Substring(15, Command.Length - 24).Trim();
                            } else if (Command.StartsWith("train.timetable", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".night.load", StringComparison.OrdinalIgnoreCase)) {
                                Command = "train.timetable.night" + Command.Substring(15, Command.Length - 26).Trim();
                            } else if (Command.StartsWith("route.signal", StringComparison.OrdinalIgnoreCase) & Command.EndsWith(".set", StringComparison.OrdinalIgnoreCase)) {
                                Command = Command.Substring(0, Command.Length - 4).TrimEnd();
                            }
                        }
                        // handle indices
                        int CommandIndex1 = 0, CommandIndex2 = 0;
                        if (Command != null && Command.EndsWith(")")) {
                            for (int k = Command.Length - 2; k >= 0; k--) {
                                if (Command[k] == '(') {
                                    string Indices = Command.Substring(k + 1, Command.Length - k - 2).TrimStart();
                                    Command = Command.Substring(0, k).TrimEnd();
                                    int h = Indices.IndexOf(";");
                                    if (h >= 0) {
                                        string a = Indices.Substring(0, h).TrimEnd();
                                        string b = Indices.Substring(h + 1).TrimStart();
                                        if (a.Length > 0 && !Interface.TryParseIntVb6(a, out CommandIndex1)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Invalid first index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName + ".");
                                            Command = null; break;
                                        } else if (b.Length > 0 && !Interface.TryParseIntVb6(b, out CommandIndex2)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Invalid second index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName + ".");
                                            Command = null; break;
                                        }
                                    } else {
                                        if (Indices.Length > 0 && !Interface.TryParseIntVb6(Indices, out CommandIndex1)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Invalid index appeared at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName + ".");
                                            Command = null; break;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        // process command
                        if (Command != null && Command.Length != 0) {
                            switch (Command.ToLowerInvariant()) {
                                // options
                                case "options.blocklength": {
                                        double length = 25.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out length)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Length is invalid in Options.BlockLength at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            length = 25.0;
                                        }
                                        Data.BlockInterval = length;
                                    } break;
                                case "options.unitoflength":
                                case "options.unitofspeed":
                                case "options.objectvisibility":
                                    break;
                                case "options.sectionbehavior":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        int a;
                                        if (!Interface.TryParseIntVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a != 0 & a != 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            ValueBasedSections = a == 1;
                                        }
                                    } break;
                                case "options.fogbehavior":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        int a;
                                        if (!Interface.TryParseIntVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a != 0 & a != 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is expected to be either 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Data.FogTransitionMode = a == 1;
                                        }
                                    } break;
                                // route
                                case "route.comment":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        Game.RouteComment = Arguments[0];
                                    } break;
                                case "route.image":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        string f = Interface.GetCombinedFileName(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
                                        if (!System.IO.File.Exists(f)) {
                                            Interface.AddMessage(Interface.MessageType.Error, true, "FileName " + f + " not found in "+Command+" at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteImage = f;
                                        }
                                    } break;
                                case "route.timetable":
                                    if (!PreviewOnly) {
                                        if (Arguments.Length < 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "" + Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Timetable.TimetableDescription = Arguments[0];
                                        }
                                    } break;
                                case "route.change":
                                    if (!PreviewOnly) {
                                        int change = 0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out change)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            change = 0;
                                        } else if (change < -1 | change > 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Mode is expected to be -1, 0 or 1 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            change = 0;
                                        }
                                        Game.TrainStart = (Game.TrainStartMode)change;
                                    } break;
                                case "route.gauge":
                                case "train.gauge":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double a;
                                        if (!Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInMillimeters is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a <= 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInMillimeters is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteRailGauge = 0.001 * a;
                                        }
                                    } break;
                                case "route.signal":
                                    if (!PreviewOnly) {
                                        if (Arguments.Length < 1) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            double a; if (!Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (CommandIndex1 < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "AspectIndex is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (a < 0.0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Speed is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.SignalSpeeds.Length) {
                                                        int n = Data.SignalSpeeds.Length;
                                                        Array.Resize<double>(ref Data.SignalSpeeds, CommandIndex1 + 1);
                                                        for (int i = n; i < CommandIndex1; i++) {
                                                            Data.SignalSpeeds[i] = double.PositiveInfinity;
                                                        }
                                                    }
                                                    Data.SignalSpeeds[CommandIndex1] = a * Data.UnitOfSpeed;
                                                }
                                            }
                                        }
                                    } break;
                                case "route.runinterval":
                                case "train.interval": {
                                        if (!PreviewOnly) {
                                            double val = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out val)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "ValueInSeconds is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                val = 0.0;
                                            }
                                            if (val < 0.0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "ValueInSeconds is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                val = 0.0;
                                            }
                                            if (val > 0.0) {
                                                Game.PretrainInterval = val;
                                                Game.PretrainsUsed = 1;
                                            } else {
                                                Game.PretrainInterval = 0.0;
                                                Game.PretrainsUsed = 0;
                                            }
                                        }
                                    } break;
                                case "route.accelerationduetogravity":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double a;
                                        if (!Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a <= 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteAccelerationDueToGravity = a;
                                        }
                                    } break;
                                case "route.elevation":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double a;
                                        if (!Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Height is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteInitialElevation = a;
                                        }
                                    } break;
                                case "route.temperature":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double a;
                                        if (!Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInCelsius is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a <= -273.15) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInCelsius is expected to be greater than to -273.15 in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteInitialAirTemperature = a + 273.15;
                                        }
                                    } break;
                                case "route.pressure":
                                    if (Arguments.Length < 1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double a;
                                        if (!Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInKPa is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else if (a <= 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "ValueInKPa is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        } else {
                                            Game.RouteInitialAirPressure = 1000.0 * a;
                                        }
                                    } break;
                                case "route.ambientlight": {
                                        byte r = 255, g = 255, b = 255;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseByteVb6(Arguments[0], out r)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "R is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseByteVb6(Arguments[1], out g)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "G is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseByteVb6(Arguments[2], out b)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "B is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        Renderer.OptionAmbientColor = new World.ColorRGB(r, g, b);
                                    } break;
                                case "route.directionallight": {
                                        byte r = 255, g = 255, b = 255;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseByteVb6(Arguments[0], out r)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "R is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseByteVb6(Arguments[1], out g)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "G is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseByteVb6(Arguments[2], out b)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "B is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        Renderer.OptionDiffuseColor = new World.ColorRGB(r, g, b);
                                    }
                                    break;
                                case "route.lightdirection": {
                                        double theta = 60.0, phi = -26.565051177078;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out theta)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Theta is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out phi)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Phi is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                        }
                                        theta *= 0.0174532925199433;
                                        phi *= 0.0174532925199433;
                                        double dx = Math.Cos(theta) * Math.Sin(phi);
                                        double dy = -Math.Sin(theta);
                                        double dz = Math.Cos(theta) * Math.Cos(phi);
                                        Renderer.OptionLightPosition = new World.Vector3Df((float)-dx, (float)-dy, (float)-dz);
                                    } break;
                                case "route.developerid":
                                    break;
                                // train
                                case "train.folder":
                                case "train.file": {
                                        if (PreviewOnly) {
                                            if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Train.Folder is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                Game.TrainName = Interface.GetCorrectedPathSeparation(Arguments[0]);
                                            }
                                        }
                                    } break;
                                case "train.run":
                                case "train.rail": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is out of range in Train.Run at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                int val = 0;
                                                if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out val)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in Train.Run at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    val = 0;
                                                }
                                                if (val < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be non-negative in Train.Run at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    val = 0;
                                                }
                                                if (CommandIndex1 >= Data.Structure.Run.Length) {
                                                    Array.Resize<int>(ref Data.Structure.Run, CommandIndex1 + 1);
                                                }
                                                Data.Structure.Run[CommandIndex1] = val;
                                            }
                                        }
                                    } break;
                                case "train.flange": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is out of range in Train.Flange at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                int val = 0;
                                                if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out val)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in Train.Flange at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    val = 0;
                                                }
                                                if (val < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Value expected to be non-negative in Train.Flange at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    val = 0;
                                                }
                                                if (CommandIndex1 >= Data.Structure.Flange.Length) {
                                                    Array.Resize<int>(ref Data.Structure.Flange, CommandIndex1 + 1);
                                                }
                                                Data.Structure.Flange[CommandIndex1] = val;
                                            }
                                        }
                                    } break;
                                case "train.timetable.day": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "timetableIndex is expected to be non-negative in Train.Timetable at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Train.Timetable is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (!IsRouteViewer) {
                                                if (CommandIndex1 >= Data.TimetableDaytime.Length) {
                                                    int n = Data.TimetableDaytime.Length;
                                                    Array.Resize<int>(ref Data.TimetableDaytime, n << 1);
                                                    for (int i = n; i < Data.TimetableDaytime.Length; i++) {
                                                        Data.TimetableDaytime[i] = -1;
                                                    }
                                                }
                                                // Hack: try something in the Route's hierarchy first; not sure
                                                // what the clean way to do this would be...  Also this is the only
                                                // (mis-)use? of Trainpath whilst parsing a Route
                                                string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                if(!System.IO.File.Exists(f)) {
                                                   f = Interface.GetCombinedFileName(TrainPath, Arguments[0]);
                                                }
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Texture file " + f + " not found in Train.Timetable at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.TimetableDaytime[CommandIndex1] = TextureManager.RegisterTexture(f, TextureManager.TextureWrapMode.ClampToEdge, true);
                                                    TextureManager.UseTexture(Data.TimetableDaytime[CommandIndex1], TextureManager.UseMode.Normal);
                                                }
                                            }
                                        }
                                    } break;
                                case "train.timetable.night": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "timetableIndex is expected to be non-negativ in Train.Timetable at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Train.Timetable is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (!IsRouteViewer) {
                                                if (CommandIndex1 >= Data.TimetableNighttime.Length) {
                                                    int n = Data.TimetableNighttime.Length;
                                                    Array.Resize<int>(ref Data.TimetableNighttime, n << 1);
                                                    for (int i = n; i < Data.TimetableNighttime.Length; i++) {
                                                        Data.TimetableNighttime[i] = -1;
                                                    }
                                                }
                                                // Hack: try something in the Route's hierarchy first; not sure
                                                // what the clean way to do this would be...  Also this is the only
                                                // (mis-)use? of Trainpath whilst parsing a Route
                                                string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                if(!System.IO.File.Exists(f)) {
                                                   f = Interface.GetCombinedFileName(TrainPath, Arguments[0]);
                                                }
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Texture file " + f + " not found in Train.Timetable at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.TimetableNighttime[CommandIndex1] = TextureManager.RegisterTexture(f, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                    TextureManager.UseTexture(Data.TimetableNighttime[CommandIndex1], TextureManager.UseMode.Normal);
                                                }
                                            }
                                        }
                                    } break;
                                // structure
                                case "structure.rail": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.Rail at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.Rail is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.Rail.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.Rail, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.Rail at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.Rail[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.beacon": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "structureIndex is expected to be non-negativ in Structure.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.Beacon is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.Beacon.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.Beacon, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.Beacon[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.pole": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Track is expected to be non-negativ in Structure.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (CommandIndex2 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.Pole is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.Poles.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject[]>(ref Data.Structure.Poles, CommandIndex1 + 1);
                                                    }
                                                    if (Data.Structure.Poles[CommandIndex1] == null) {
                                                        Data.Structure.Poles[CommandIndex1] = new ObjectManager.UnifiedObject[CommandIndex2 + 1];
                                                    } else if (CommandIndex2 >= Data.Structure.Poles[CommandIndex1].Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.Poles[CommandIndex1], CommandIndex2 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.Poles[CommandIndex1][CommandIndex2] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.ground": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.Ground is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.Ground.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.Ground, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.Ground[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.walll": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.WallL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.WallL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.WallL.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.WallL, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.WallL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.WallL[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.wallr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.WallR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.WallR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.WallR.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.WallR, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.WallR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.WallR[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.dikel": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.DikeL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.DikeL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.DikeL.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.DikeL, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.DikeL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.DikeL[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.diker": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.DikeR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.DikeR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.DikeR.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.DikeR, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.DikeR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.DikeR[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.forml": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.FormL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.FormL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.FormL.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.FormL, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.FormL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.FormL[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.formr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.FormR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.FormR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.FormR.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.FormR, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.FormR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.FormR[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.formcl": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.FormCL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.FormCL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.FormCL.Length) {
                                                        Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.FormCL, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.FormCL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.FormCL[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.formcr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.FormCR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.FormCR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.FormCR.Length) {
                                                        Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.FormCR, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.FormCR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.FormCR[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.roofl": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.RoofL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.RoofL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 == 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index was omitted or is 0 in Structure.RoofL argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        CommandIndex1 = 1;
                                                    }
                                                    if (CommandIndex1 < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negativ in Structure.RoofL argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        if (CommandIndex1 >= Data.Structure.RoofL.Length) {
                                                            Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.RoofL, CommandIndex1 + 1);
                                                        }
                                                        string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                        if (!System.IO.File.Exists(f)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.RoofL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Data.Structure.RoofL[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.roofr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.RoofR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.RoofR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 == 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index was omitted or is 0 in Structure.RoofR argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        CommandIndex1 = 1;
                                                    }
                                                    if (CommandIndex1 < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negativ in Structure.RoofR argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        if (CommandIndex1 >= Data.Structure.RoofR.Length) {
                                                            Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.RoofR, CommandIndex1 + 1);
                                                        }
                                                        string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                        if (!System.IO.File.Exists(f)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.RoofR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Data.Structure.RoofR[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.roofcl": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.RoofCL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.RoofCL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 == 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index was omitted or is 0 in Structure.RoofCL argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        CommandIndex1 = 1;
                                                    }
                                                    if (CommandIndex1 < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negativ in Structure.RoofCL argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        if (CommandIndex1 >= Data.Structure.RoofCL.Length) {
                                                            Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.RoofCL, CommandIndex1 + 1);
                                                        }
                                                        string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                        if (!System.IO.File.Exists(f)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.RoofCL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Data.Structure.RoofCL[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.roofcr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.RoofCR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.RoofCR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 == 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index was omitted or is 0 in Structure.RoofCR argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        CommandIndex1 = 1;
                                                    }
                                                    if (CommandIndex1 < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negativ in Structure.RoofCR argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        if (CommandIndex1 >= Data.Structure.RoofCR.Length) {
                                                            Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.RoofCR, CommandIndex1 + 1);
                                                        }
                                                        string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                        if (!System.IO.File.Exists(f)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.RoofCR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Data.Structure.RoofCR[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.crackl": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.CrackL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.CrackL is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.CrackL.Length) {
                                                        Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.CrackL, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.CrackL at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.CrackL[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.crackr": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.CrackR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.CrackR is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.CrackR.Length) {
                                                        Array.Resize<ObjectManager.StaticObject>(ref Data.Structure.CrackR, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.CrackR at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.CrackR[CommandIndex1] = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, true, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "structure.freeobj": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Structure.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Arguments.Length < 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Structure.FreeObj is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (CommandIndex1 >= Data.Structure.FreeObj.Length) {
                                                        Array.Resize<ObjectManager.UnifiedObject>(ref Data.Structure.FreeObj, CommandIndex1 + 1);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Structure.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Data.Structure.FreeObj[CommandIndex1] = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                // signal
                                case "signal": {
                                        if (!PreviewOnly) {
                                            if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (CommandIndex1 >= Data.SignalData.Length) {
                                                    Array.Resize<SignalData>(ref Data.SignalData, CommandIndex1 + 1);
                                                }
                                                if (Arguments[0].EndsWith(".animated", StringComparison.OrdinalIgnoreCase)) {
                                                    if (Arguments.Length > 1) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, Command + " is expected to have exactly 1 argument when using animated objects at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                    if (!System.IO.File.Exists(f)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Signal at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        ObjectManager.UnifiedObject Object = ObjectManager.LoadObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                        if (Object is ObjectManager.AnimatedObjectCollection) {
                                                            AnimatedObjectSignalData Signal = new AnimatedObjectSignalData();
                                                            Signal.Objects = (ObjectManager.AnimatedObjectCollection)Object;
                                                            Data.SignalData[CommandIndex1] = Signal;
                                                        } else {
                                                            Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " is not a valid animated object in Signal at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        }
                                                    }
                                                } else {
                                                    if (Arguments.Length > 2) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, Command + " is expected to have between 1 and 2 arguments at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    /// <info>System.IO.Path.Combine is used here as only a base file name is provided and thus does not exist anyway</info>
                                                    string f = System.IO.Path.Combine(ObjectPath, Interface.GetCorrectedPathSeparation(Arguments[0]));
                                                    Bve4SignalData Signal = new Bve4SignalData();
                                                    Signal.BaseObject = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                    Signal.GlowObject = null;
                                                    string Folder = Interface.GetCorrectedFolderName(System.IO.Path.GetDirectoryName(f));
                                                    if (!System.IO.Directory.Exists(Folder)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, true, "Folder " + Folder + " not found in Structure.Signal at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Signal.DaylightTextures = LoadAllTextures(f, new World.ColorRGB(0, 0, 0), 1, TextureManager.TextureLoadMode.Normal);
                                                        Signal.GlowTextures = new int[] { };
                                                        if (Arguments.Length >= 2) {
                                                            /// <info>System.IO.Path.Combine is used here as only a base file name is provided and thus does not exist anyway</info>
                                                            f = System.IO.Path.Combine(ObjectPath, Interface.GetCorrectedPathSeparation(Arguments[1]));
                                                            Signal.GlowObject = ObjectManager.LoadStaticObject(f, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                                                            Signal.GlowTextures = LoadAllTextures(f, new World.ColorRGB(0, 0, 0), 1, TextureManager.TextureLoadMode.Bve4SignalGlow);
                                                        }
                                                        Data.SignalData[CommandIndex1] = Signal;
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                // texture
                                case "texture.background":
                                case "structure.back": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxTxType is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Texture.Background is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (CommandIndex1 >= Data.Backgrounds.Length) {
                                                    int a = Data.Backgrounds.Length;
                                                    Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
                                                    for (int k = a; k <= CommandIndex1; k++) {
                                                        Data.Backgrounds[k] = new World.Background(-1, 6, false);
                                                    }
                                                }
                                                string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in Texture.Background at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.Backgrounds[CommandIndex1].Texture = TextureManager.RegisterTexture(f, new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.Repeat, false);
                                                }
                                            }
                                        }
                                    } break;
                                case "texture.background.x": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxTxType is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Texture.Background is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (CommandIndex1 >= Data.Backgrounds.Length) {
                                                    int a = Data.Backgrounds.Length;
                                                    Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
                                                    for (int k = a; k <= CommandIndex1; k++) {
                                                        Data.Backgrounds[k] = new World.Background(-1, 6, false);
                                                    }
                                                }
                                                int x;
                                                if (!Interface.TryParseIntVb6(Arguments[0], out x)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxTxType is invalid in Texture.Background at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (x == 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "repeatCount is expected to be non-zero in Texture.Background at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.Backgrounds[CommandIndex1].Repetition = x;
                                                }
                                            }
                                        }
                                    } break;
                                case "texture.background.aspect": {
                                        if (!PreviewOnly) {
                                            if (CommandIndex1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxTxType is expected to be non-negative at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Texture.Background.Aspect is expected to have one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (CommandIndex1 >= Data.Backgrounds.Length) {
                                                    int a = Data.Backgrounds.Length;
                                                    Array.Resize<World.Background>(ref Data.Backgrounds, CommandIndex1 + 1);
                                                    for (int k = a; k <= CommandIndex1; k++) {
                                                        Data.Backgrounds[k] = new World.Background(-1, 6, false);
                                                    }
                                                }
                                                int aspect;
                                                if (!Interface.TryParseIntVb6(Arguments[0], out aspect)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxTxType is invalid in Texture.Background.Aspect at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (aspect != 0 & aspect != 1) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be either 0 or 1 in Texture.Background.Aspect at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.Backgrounds[CommandIndex1].KeepAspectRatio = aspect == 1;
                                                }
                                            }
                                        }
                                    } break;
                                // cycle
                                case "cycle.ground":
                                    if (!PreviewOnly) {
                                        if (CommandIndex1 >= Data.Structure.Cycle.Length) {
                                            Array.Resize<int[]>(ref Data.Structure.Cycle, CommandIndex1 + 1);
                                        }
                                        Data.Structure.Cycle[CommandIndex1] = new int[Arguments.Length];
                                        for (int k = 0; k < Arguments.Length; k++) {
                                            int ix = 0;
                                            if (Arguments[k].Length > 0 && !Interface.TryParseIntVb6(Arguments[k], out ix)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "I" + (k + 1).ToString(Culture) + " is invalid in Cycle.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                ix = 0;
                                            }
                                            if (ix < 0 | ix >= Data.Structure.Ground.Length) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "I" + (k + 1).ToString(Culture) + " is out of range in Cycle.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                ix = 0;
                                            }
                                            Data.Structure.Cycle[CommandIndex1][k] = ix;
                                        }
                                    } break;
                                // track
                                case "track.railstart":
                                case "track.rail": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be positive in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (string.Compare(Command, "track.railstart", StringComparison.OrdinalIgnoreCase) == 0) {
                                                    if (idx < Data.Blocks[BlockIndex].Rail.Length && Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index is required to reference a non-existing rail in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                }
                                                if (Data.Blocks[BlockIndex].Rail.Length <= idx) {
                                                    Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rail, idx + 1);
                                                }
                                                if (Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed) {
                                                    Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
                                                }
                                                {
                                                    Data.Blocks[BlockIndex].Rail[idx].RailStart = true;
                                                    Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = true;
                                                    if (Arguments.Length >= 2) {
                                                        if (Arguments[1].Length > 0) {
                                                            double x;
                                                            if (!Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
                                                                Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                                x = 0.0;
                                                            }
                                                            Data.Blocks[BlockIndex].Rail[idx].RailStartX = x;
                                                        }
                                                        if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd) {
                                                            Data.Blocks[BlockIndex].Rail[idx].RailEndX = Data.Blocks[BlockIndex].Rail[idx].RailStartX;
                                                        }
                                                    }
                                                    if (Arguments.Length >= 3) {
                                                        if (Arguments[2].Length > 0) {
                                                            double y;
                                                            if (!Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
                                                                Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                                y = 0.0;
                                                            }
                                                            Data.Blocks[BlockIndex].Rail[idx].RailStartY = y;
                                                        }
                                                        if (!Data.Blocks[BlockIndex].Rail[idx].RailEnd) {
                                                            Data.Blocks[BlockIndex].Rail[idx].RailEndY = Data.Blocks[BlockIndex].Rail[idx].RailStartY;
                                                        }
                                                    }
                                                    if (Data.Blocks[BlockIndex].RailType.Length <= idx) {
                                                        Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
                                                    }
                                                    if (Arguments.Length >= 4) {
                                                        int sttype;
                                                        if (!Interface.TryParseIntVb6(Arguments[3], out sttype)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in " + Command + "at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                            sttype = 0;
                                                        }
                                                        if (sttype < 0) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else if (sttype >= Data.Structure.Rail.Length || Data.Structure.Rail[sttype] == null) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Data.Blocks[BlockIndex].RailType[idx] = sttype;
                                                        }
                                                    } else if (BlockIndex > 0) {
                                                        if (idx < Data.Blocks[BlockIndex - 1].RailType.Length) {
                                                            Data.Blocks[BlockIndex].RailType[idx] = Data.Blocks[BlockIndex - 1].RailType[idx];
                                                        } else {
                                                            Data.Blocks[BlockIndex].RailType[idx] = 0;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "track.railend": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.RailEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0 || idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index references a non-existing rail in Track.RailEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (Data.Blocks[BlockIndex].RailType.Length <= idx) {
                                                    Array.Resize<Rail>(ref Data.Blocks[BlockIndex].Rail, idx + 1);
                                                }
                                                Data.Blocks[BlockIndex].Rail[idx].RailStart = false;
                                                Data.Blocks[BlockIndex].Rail[idx].RailStartRefreshed = false;
                                                Data.Blocks[BlockIndex].Rail[idx].RailEnd = true;
                                                if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
                                                    double x;
                                                    if (!Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.RailEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        x = 0.0;
                                                    }
                                                    Data.Blocks[BlockIndex].Rail[idx].RailEndX = x;
                                                }
                                                if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
                                                    double y;
                                                    if (!Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.RailEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        y = 0.0;
                                                    }
                                                    Data.Blocks[BlockIndex].Rail[idx].RailEndY = y;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.railtype": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            int sttype = 0;
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out sttype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                sttype = 0;
                                            }
                                            if (idx < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negativ in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                if (sttype < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negativ in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (sttype >= Data.Structure.Rail.Length || Data.Structure.Rail[sttype] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.RailType at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (Data.Blocks[BlockIndex].RailType.Length <= idx) {
                                                        Array.Resize<int>(ref Data.Blocks[BlockIndex].RailType, idx + 1);
                                                    }
                                                    Data.Blocks[BlockIndex].RailType[idx] = sttype;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.accuracy": {
                                        double r = 2.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out r)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Rank is invalid in Track.Accuracy at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            r = 2.0;
                                        }
                                        Data.Blocks[BlockIndex].Accuracy = r;
                                    } break;
                                case "track.pitch": {
                                        double p = 0.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out p)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Rate is invalid in Track.Pitch at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            p = 0.0;
                                        }
                                        Data.Blocks[BlockIndex].Pitch = 0.001 * p;
                                    } break;
                                case "track.curve": {
                                        double r = 0.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out r)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Radius is invalid in Track.Curve at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            r = 0.0;
                                        }
                                        double c = 0.0;
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 1 && !Interface.TryParseDoubleVb6(Arguments[1], out c)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Cant is invalid in Track.Curve at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            c = 0.0;
                                        }
                                        c *= 0.001;
                                        if (r >= -0.0001 & r <= 0.0001) {
                                            Data.Blocks[BlockIndex].CurrentTrackState.PlanarCurveRadius = 0.0;
                                            Data.Blocks[BlockIndex].CurrentTrackState.PlanarCurveCant = 0.0;
                                        } else {
                                            Data.Blocks[BlockIndex].CurrentTrackState.PlanarCurveRadius = r;
                                            Data.Blocks[BlockIndex].CurrentTrackState.PlanarCurveCant = Math.Abs(c) * Math.Sign(r);
                                        }
                                    } break;
                                case "track.turn": {
                                        double s = 0.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out s)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Slope is invalid in Track.Turn at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            s = 0.0;
                                        }
                                        Data.Blocks[BlockIndex].CurrentTrackState.PlanarCurveCant = 0.0;
                                        Data.Blocks[BlockIndex].Turn = s;
                                    } break;
                                case "track.adhesion": {
                                        double a = 100.0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out a)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Rate is invalid in Track.Adhesion at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            a = 100.0;
                                        }
                                        if (a < 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Rate is expected to be non-negative in Track.Adhesion at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            a = 100.0;
                                        }
                                        Data.Blocks[BlockIndex].AdhesionMultiplier = 0.01 * a;
                                    } break;
                                case "track.brightness": {
                                        if (!PreviewOnly) {
                                            float value = 255.0f;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseFloatVb6(Arguments[0], out value)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in Track.Brightness at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                value = 255.0f;
                                            }
                                            value /= 255.0f;
                                            if (value < 0.0f) value = 0.0f;
                                            if (value > 1.0f) value = 1.0f;
                                            int n = Data.Blocks[BlockIndex].Brightness.Length;
                                            Array.Resize<Brightness>(ref Data.Blocks[BlockIndex].Brightness, n + 1);
                                            Data.Blocks[BlockIndex].Brightness[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Brightness[n].Value = value;
                                        }
                                    } break;
                                case "track.fog": {
                                        if (!PreviewOnly) {
                                            double start = 0.0, end = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out start)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Start is invalid in Track.Fog at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                start = 0.0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out end)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "End is invalid in Track.Fog at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                end = 0.0;
                                            }
                                            byte r = 128, g = 128, b = 128;
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseByteVb6(Arguments[2], out r)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Red is invalid in Track.Fog at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                r = 128;
                                            }
                                            if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseByteVb6(Arguments[3], out g)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Green is invalid in Track.Fog at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                g = 128;
                                            }
                                            if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseByteVb6(Arguments[4], out b)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Blue is invalid in Track.Fog at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                b = 128;
                                            }
                                            if (end <= 0.0 | start >= end) {
                                                start = World.BackgroundImageDistance + World.ExtraViewingDistance;
                                                end = World.BackgroundImageDistance + 2.0 * World.ExtraViewingDistance;
                                            }
                                            Data.Blocks[BlockIndex].Fog.Start = (float)start;
                                            Data.Blocks[BlockIndex].Fog.End = (float)end;
                                            Data.Blocks[BlockIndex].Fog.Color = new World.ColorRGB(r, g, b);
                                            Data.Blocks[BlockIndex].FogDefined = true;
                                        }
                                    } break;
                                case "track.signal":
                                case "track.sig": {
                                        if (!PreviewOnly) {
                                            int num = -2;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out num)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Number is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                num = -2;
                                            }
                                            if (num != -2 & num != 2 & num != 3 & num != -4 & num != 4 & num != -5 & num != 5 & num != 6) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "The number of aspects is not supported in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                num = num == -3 | num == -6 ? -num : -4;
                                            }
                                            double x = 0.0, y = 0.0;
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                x = 0.0;
                                            }
                                            if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                y = 0.0;
                                            }
                                            double yaw = 0.0, pitch = 0.0, roll = 0.0;
                                            if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out yaw)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                yaw = 0.0;
                                            }
                                            if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out pitch)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Pitch is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                pitch = 0.0;
                                            }
                                            if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[6], out roll)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                roll = 0.0;
                                            }
                                            int[] aspects; int comp;
                                            switch (num) {
                                                case 2: aspects = new int[] { 0, 2 }; comp = 0; break;
                                                case -2: aspects = new int[] { 0, 4 }; comp = 1; break;
                                                case 3: aspects = new int[] { 0, 2, 4 }; comp = 2; break;
                                                case 4: aspects = new int[] { 0, 1, 2, 4 }; comp = 3; break;
                                                case -4: aspects = new int[] { 0, 2, 3, 4 }; comp = 4; break;
                                                case 5: aspects = new int[] { 0, 1, 2, 3, 4 }; comp = 5; break;
                                                case -5: aspects = new int[] { 0, 2, 3, 4, 5 }; comp = 6; break;
                                                case 6: aspects = new int[] { 0, 1, 2, 3, 4, 5 }; comp = 7; break;
                                                default: aspects = new int[] { 0, 2 }; comp = 0; break;
                                            }
                                            int n = Data.Blocks[BlockIndex].Section.Length;
                                            Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
                                            Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
                                            Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
                                            Data.Blocks[BlockIndex].Section[n].Invisible = x == 0.0;
                                            Data.Blocks[BlockIndex].Section[n].Type = Game.SectionType.ValueBased;
                                            if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
                                                if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
                                                    Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
                                                    DepartureSignalUsed = true;
                                                }
                                            }
                                            CurrentSection++;
                                            n = Data.Blocks[BlockIndex].Signal.Length;
                                            Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
                                            Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection;
                                            Data.Blocks[BlockIndex].Signal[n].Name = Arguments.Length >= 2 ? Arguments[1] : "";
                                            Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = comp;
                                            Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = -1;
                                            Data.Blocks[BlockIndex].Signal[n].X = x;
                                            Data.Blocks[BlockIndex].Signal[n].Y = y <= 0.0 ? 4.8 : y;
                                            Data.Blocks[BlockIndex].Signal[n].Yaw = 0.0174532925199433 * yaw;
                                            Data.Blocks[BlockIndex].Signal[n].Pitch = 0.0174532925199433 * pitch;
                                            Data.Blocks[BlockIndex].Signal[n].Roll = 0.0174532925199433 * roll;
                                            Data.Blocks[BlockIndex].Signal[n].ShowObject = x != 0.0;
                                            Data.Blocks[BlockIndex].Signal[n].ShowPost = x != 0.0 & y < 0.0;
                                            Data.Blocks[BlockIndex].Signal[n].GameSignalIndex = -1;
                                        }
                                    } break;
                                case "track.section": {
                                        if (!PreviewOnly) {
                                            if (Arguments.Length == 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "At least one argument is required in Track.Section at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                int[] aspects = new int[Arguments.Length];
                                                for (int i = 0; i < Arguments.Length; i++) {
                                                    if (!Interface.TryParseIntVb6(Arguments[i], out aspects[i])) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "s" + i.ToString(Culture) + " is invalid in Track.Section at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        aspects[i] = -1;
                                                    } else if (aspects[i] < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "s" + i.ToString(Culture) + " is expected to be non-negative in Track.Section at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        aspects[i] = -1;
                                                    }
                                                }
                                                Array.Sort<int>(aspects);
                                                int n = Data.Blocks[BlockIndex].Section.Length;
                                                Array.Resize<Section>(ref Data.Blocks[BlockIndex].Section, n + 1);
                                                Data.Blocks[BlockIndex].Section[n].TrackPosition = Data.TrackPosition;
                                                Data.Blocks[BlockIndex].Section[n].Aspects = aspects;
                                                Data.Blocks[BlockIndex].Section[n].Type = ValueBasedSections ? Game.SectionType.ValueBased : Game.SectionType.IndexBased;
                                                Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = -1;
                                                if (CurrentStation >= 0 && Game.Stations[CurrentStation].ForceStopSignal) {
                                                    if (CurrentStation >= 0 & CurrentStop >= 0 & !DepartureSignalUsed) {
                                                        Data.Blocks[BlockIndex].Section[n].DepartureStationIndex = CurrentStation;
                                                        DepartureSignalUsed = true;
                                                    }
                                                }
                                                CurrentSection++;
                                            }
                                        }
                                    } break;
                                case "track.sigf": {
                                        if (!PreviewOnly) {
                                            int objidx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out objidx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "objectIndex is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                objidx = 0;
                                            }
                                            if (objidx >= 0 & objidx < Data.SignalData.Length && Data.SignalData[objidx] != null) {
                                                int section = 0;
                                                if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out section)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "section is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    section = 0;
                                                }
                                                double x = 0.0, y = 0.0;
                                                if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    x = 0.0;
                                                }
                                                if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    y = 0.0;
                                                }
                                                double yaw = 0.0, pitch = 0.0, roll = 0.0;
                                                if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out yaw)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    yaw = 0.0;
                                                }
                                                if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out pitch)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Pitch is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    pitch = 0.0;
                                                }
                                                if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[6], out roll)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    roll = 0.0;
                                                }
                                                int n = Data.Blocks[BlockIndex].Signal.Length;
                                                Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
                                                Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
                                                Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection + section;
                                                Data.Blocks[BlockIndex].Signal[n].Name = Arguments[1];
                                                Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = -1;
                                                Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = objidx;
                                                Data.Blocks[BlockIndex].Signal[n].X = x;
                                                Data.Blocks[BlockIndex].Signal[n].Y = y < 0.0 ? 4.8 : y;
                                                Data.Blocks[BlockIndex].Signal[n].Yaw = 0.0174532925199433 * yaw;
                                                Data.Blocks[BlockIndex].Signal[n].Pitch = 0.0174532925199433 * pitch;
                                                Data.Blocks[BlockIndex].Signal[n].Roll = 0.0174532925199433 * roll;
                                                Data.Blocks[BlockIndex].Signal[n].ShowObject = true;
                                                Data.Blocks[BlockIndex].Signal[n].ShowPost = y < 0.0;
                                                Data.Blocks[BlockIndex].Signal[n].GameSignalIndex = -1;
                                            } else {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "objectIndex references a signal object not loaded in Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            }
                                        }
                                    } break;
                                case "track.beacon": {
                                        if (!PreviewOnly) {
                                            int type = 0, structure = 0, section = 0, optional = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out type)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "typeBeacon is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                type = 0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out structure)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "structureIndex is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                structure = 0;
                                            }
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out section)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "section is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                section = 0;
                                            }
                                            if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out optional)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "n is invalid in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                optional = 0;
                                            }
                                            if (structure < -1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "structureIndex is expected to be non-negative or -1 in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                structure = -1;
                                            } else if (structure >= 0 && (structure >= Data.Structure.Beacon.Length || Data.Structure.Beacon[structure] == null)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "structureIndex references an object not loaded in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                structure = -1;
                                            }
                                            if (section == -1) {
                                                section = (int)TrackManager.TransponderSpecialSection.NextRedSection;
                                            } else if (section < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "section is expected to be non-negative or -1 in Track.Beacon at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                section = CurrentSection + 1;
                                            } else {
                                                section += CurrentSection;
                                            }
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = (TrackManager.TransponderType)type;
                                            Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = optional;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = structure;
                                            Data.Blocks[BlockIndex].Transponder[n].Section = section;
                                            Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = false;
                                        }
                                    } break;
                                case "track.relay": {
                                        if (!PreviewOnly) {
                                            double x = 0.0, y = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out x)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                x = 0.0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out y)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.Relay at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                y = 0.0;
                                            }
                                            int n = Data.Blocks[BlockIndex].Signal.Length;
                                            Array.Resize<Signal>(ref Data.Blocks[BlockIndex].Signal, n + 1);
                                            Data.Blocks[BlockIndex].Signal[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Signal[n].Section = CurrentSection + 1;
                                            Data.Blocks[BlockIndex].Signal[n].Name = "";
                                            Data.Blocks[BlockIndex].Signal[n].SignalCompatibilityObjectIndex = 8;
                                            Data.Blocks[BlockIndex].Signal[n].SignalObjectIndex = -1;
                                            Data.Blocks[BlockIndex].Signal[n].X = x;
                                            Data.Blocks[BlockIndex].Signal[n].Y = y == -1.0 ? 4.8 : y;
                                            Data.Blocks[BlockIndex].Signal[n].ShowObject = x != 0.0;
                                            Data.Blocks[BlockIndex].Signal[n].ShowPost = x != 0.0 & y < 0.0;
                                        }
                                    } break;
                                case "track.transponder":
                                case "track.tr": {
                                        if (!PreviewOnly) {
                                            int type = 0, oversig = 0, work = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out type)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                type = 0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out oversig)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "OverSig is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                oversig = 0;
                                            }
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out work)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Work is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                work = 0;
                                            }
                                            if (oversig < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "OverSig is expected to be non-negative in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                oversig = 0;
                                            }
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = (TrackManager.TransponderType)type;
                                            Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
                                            Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = work == 0;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
                                            if (type == 2) {
                                                Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = CurrentStop >= 0 ? CurrentStop : 0;
                                            } else {
                                                Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = work;
                                            }
                                            Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + oversig + 1;
                                        }
                                    } break;
                                case "track.atssn": {
                                        if (!PreviewOnly) {
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.S;
                                            Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
                                            Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = true;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + 1;
                                        }
                                    } break;
                                case "track.atsp": {
                                        if (!PreviewOnly) {
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.AtsPPatternOrigin;
                                            Data.Blocks[BlockIndex].Transponder[n].ShowDefaultObject = true;
                                            Data.Blocks[BlockIndex].Transponder[n].SwitchSubsystem = true;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].OptionalInteger = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].Section = CurrentSection + 1;
                                        }
                                    } break;
                                case "track.pattern": {
                                        if (!PreviewOnly) {
                                            int type = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out type)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Type is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                type = 0;
                                            }
                                            double speed = 0.0;
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out speed)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                speed = 0.0;
                                            }
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = type == 1 ? TrackManager.TransponderType.AtsPPermanentSpeedRestriction : TrackManager.TransponderType.AtsPTemporarySpeedRestriction;
                                            Data.Blocks[BlockIndex].Transponder[n].Section = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].OptionalFloat = speed * Data.UnitOfSpeed;
                                        }
                                    } break;
                                case "track.plimit": {
                                        if (!PreviewOnly) {
                                            double speed = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out speed)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Speed is invalid in " + Command + " at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                speed = 0.0;
                                            }
                                            int n = Data.Blocks[BlockIndex].Transponder.Length;
                                            Array.Resize<Transponder>(ref Data.Blocks[BlockIndex].Transponder, n + 1);
                                            Data.Blocks[BlockIndex].Transponder[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].Transponder[n].Type = TrackManager.TransponderType.AtsPPermanentSpeedRestriction;
                                            Data.Blocks[BlockIndex].Transponder[n].Section = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].BeaconStructureIndex = -1;
                                            Data.Blocks[BlockIndex].Transponder[n].OptionalFloat = speed * Data.UnitOfSpeed;
                                        }
                                    } break;
                                case "track.limit": {
                                        double limit = 0.0;
                                        int direction = 0, cource = 0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out limit)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Speed is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            limit = 0.0;
                                        }
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out direction)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Direction is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            direction = 0;
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out cource)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Cource is invalid in Track.Limit at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            cource = 0;
                                        }
                                        int n = Data.Blocks[BlockIndex].Limit.Length;
                                        Array.Resize<Limit>(ref Data.Blocks[BlockIndex].Limit, n + 1);
                                        Data.Blocks[BlockIndex].Limit[n].TrackPosition = Data.TrackPosition;
                                        Data.Blocks[BlockIndex].Limit[n].Speed = limit <= 0.0 ? double.PositiveInfinity : Data.UnitOfSpeed * limit;
                                        Data.Blocks[BlockIndex].Limit[n].Direction = direction;
                                        Data.Blocks[BlockIndex].Limit[n].Cource = cource;
                                    } break;
                                case "track.stop":
                                    if (CurrentStation == -1) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "A stop without a station is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        int dir = 0;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out dir)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Direction is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            dir = 0;
                                        }
                                        double backw = 5.0, forw = 5.0;
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out backw)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "marginBk is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            backw = 5.0;
                                        } else if (backw <= 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "marginBk is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            backw = 5.0;
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out forw)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "marginFw is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            forw = 5.0;
                                        } else if (forw <= 0.0) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "marginFw is expected to be positive in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            forw = 5.0;
                                        }
                                        int cars = 0;
                                        if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out cars)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Cars is invalid in Track.Stop at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            cars = 0;
                                        }
                                        int n = Data.Blocks[BlockIndex].Stop.Length;
                                        Array.Resize<Stop>(ref Data.Blocks[BlockIndex].Stop, n + 1);
                                        Data.Blocks[BlockIndex].Stop[n].TrackPosition = Data.TrackPosition;
                                        Data.Blocks[BlockIndex].Stop[n].Station = CurrentStation;
                                        Data.Blocks[BlockIndex].Stop[n].Direction = dir;
                                        Data.Blocks[BlockIndex].Stop[n].ForwardTolerance = forw;
                                        Data.Blocks[BlockIndex].Stop[n].BackwardTolerance = backw;
                                        Data.Blocks[BlockIndex].Stop[n].Cars = cars;
                                        CurrentStop = cars;
                                    } break;
                                case "track.sta": {
                                        CurrentStation++;
                                        Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
                                        Game.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture);
                                        Game.Stations[CurrentStation].StopAtStation = true;
                                        Game.Stations[CurrentStation].IsTerminalStation = false;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
                                            Game.Stations[CurrentStation].Name = Arguments[0];
                                        }
                                        double arr = -1.0, dep = -1.0;
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
                                            if (string.Compare(Arguments[1], "P", StringComparison.OrdinalIgnoreCase) == 0) {
                                                Game.Stations[CurrentStation].StopAtStation = false;
                                            } else if (!Interface.TryParseTime(Arguments[1], out arr)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time1 is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                arr = -1.0;
                                            }
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
                                            if (string.Compare(Arguments[2], "T", StringComparison.OrdinalIgnoreCase) == 0) {
                                                Game.Stations[CurrentStation].IsTerminalStation = true;
                                            } else if (!Interface.TryParseTime(Arguments[2], out dep)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time2 is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                dep = -1.0;
                                            }
                                        }
                                        int passalarm = 0;
                                        if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out passalarm)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "PassAlarm is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            passalarm = 0;
                                        }
                                        int door = 0;
                                        if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseIntVb6(Arguments[4], out door)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Door is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            door = 0;
                                        }
                                        int stop = 0;
                                        if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseIntVb6(Arguments[5], out stop)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Stop is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            stop = 0;
                                        }
                                        int device = 0;
                                        if (Arguments.Length >= 7 && Arguments[6].Length > 0) {
                                            if (string.Compare(Arguments[6], "ats", StringComparison.OrdinalIgnoreCase) == 0) {
                                                device = 0;
                                            } else if (string.Compare(Arguments[6], "atc", StringComparison.OrdinalIgnoreCase) == 0) {
                                                device = 1;
                                            } else if (!Interface.TryParseIntVb6(Arguments[6], out device)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Device is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                device = 0;
                                            } else if (device != 0 & device != 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "The specified Device is not supported in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                device = 0;
                                            }
                                        }
                                        int arrsnd = -1, depsnd = -1;
                                        if (!PreviewOnly) {
                                            if (Arguments.Length >= 8 && Arguments[7].Length > 0) {
                                                string f = Interface.GetCombinedFileName(SoundPath, Arguments[7]);
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Sound1 file " + f + " not found in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    arrsnd = SoundManager.LoadSound(f, 30.0);
                                                }
                                            }
                                        }
                                        double halt = 15.0;
                                        if (Arguments.Length >= 9 && Arguments[8].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[8], out halt)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Halt is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            halt = 15.0;
                                        } else if (halt < 5.0) {
                                            halt = 5.0;
                                        }
                                        double jam = 100.0;
                                        if (!PreviewOnly) {
                                            if (Arguments.Length >= 10 && Arguments[9].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[9], out jam)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Jam is invalid in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                jam = 100.0;
                                            } else if (jam < 0.0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Jam is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                jam = 100.0;
                                            }
                                        }
                                        if (!PreviewOnly) {
                                            if (Arguments.Length >= 11 && Arguments[10].Length > 0) {
                                                string f = Interface.GetCombinedFileName(SoundPath, Arguments[10]);
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Sound2 file " + f + " not found in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    depsnd = SoundManager.LoadSound(f, 30.0);
                                                }
                                            }
                                        }
                                        int ttidx = -1, tdt = -1, tnt = -1;
                                        if (!PreviewOnly) {
                                            if (Arguments.Length >= 12 && Arguments[11].Length > 0) {
                                                if (!Interface.TryParseIntVb6(Arguments[11], out ttidx)) {
                                                    ttidx = -1;
                                                } else {
                                                    if (ttidx < 0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "timetableIndex is expected to be non-negative in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        ttidx = -1;
                                                    } else if (ttidx >= Data.TimetableDaytime.Length & ttidx >= Data.TimetableNighttime.Length) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "timetableIndex references textures not loaded in Track.Sta at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        ttidx = -1;
                                                    }
                                                    tdt = ttidx >= 0 & ttidx < Data.TimetableDaytime.Length ? Data.TimetableDaytime[ttidx] : -1;
                                                    tnt = ttidx >= 0 & ttidx < Data.TimetableNighttime.Length ? Data.TimetableNighttime[ttidx] : -1;
                                                    ttidx = 0;
                                                }
                                            } else {
                                                ttidx = -1;
                                            }
                                            if (ttidx == -1) {
                                                if (CurrentStation > 0) {
                                                    tdt = Game.Stations[CurrentStation - 1].TimetableDaytimeTexture;
                                                    tnt = Game.Stations[CurrentStation - 1].TimetableNighttimeTexture;
                                                } else if (Data.TimetableDaytime.Length > 0 & Data.TimetableNighttime.Length > 0) {
                                                    tdt = Data.TimetableDaytime[0];
                                                    tnt = Data.TimetableNighttime[0];
                                                } else {
                                                    tdt = -1;
                                                    tnt = -1;
                                                }
                                            }
                                        }
                                        Game.Stations[CurrentStation].ArrivalTime = arr;
                                        Game.Stations[CurrentStation].ArrivalSoundIndex = arrsnd;
                                        Game.Stations[CurrentStation].DepartureTime = dep;
                                        Game.Stations[CurrentStation].DepartureSoundIndex = depsnd;
                                        Game.Stations[CurrentStation].StopTime = halt;
                                        Game.Stations[CurrentStation].ForceStopSignal = stop == 1;
                                        Game.Stations[CurrentStation].OpenLeftDoors = door < 0.0;
                                        Game.Stations[CurrentStation].OpenRightDoors = door > 0.0;
                                        Game.Stations[CurrentStation].SecuritySystem = device == 1 ? Game.SecuritySystem.Atc : Game.SecuritySystem.Ats;
                                        Game.Stations[CurrentStation].Stops = new Game.StationStop[] { };
                                        Game.Stations[CurrentStation].PassengerRatio = 0.01 * jam;
                                        Game.Stations[CurrentStation].TimetableDaytimeTexture = tdt;
                                        Game.Stations[CurrentStation].TimetableNighttimeTexture = tnt;
                                        Data.Blocks[BlockIndex].Station = CurrentStation;
                                        Data.Blocks[BlockIndex].StationPassAlarm = passalarm == 1;
                                        CurrentStop = -1;
                                        DepartureSignalUsed = false;
                                    } break;
                                case "track.station": {
                                        CurrentStation++;
                                        Array.Resize<Game.Station>(ref Game.Stations, CurrentStation + 1);
                                        Game.Stations[CurrentStation].Name = "Station " + (CurrentStation + 1).ToString(Culture);
                                        Game.Stations[CurrentStation].StopAtStation = true;
                                        Game.Stations[CurrentStation].IsTerminalStation = false;
                                        if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
                                            Game.Stations[CurrentStation].Name = Arguments[0];
                                        }
                                        double arr = -1.0, dep = -1.0;
                                        if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
                                            if (string.Compare(Arguments[1], "L", StringComparison.OrdinalIgnoreCase) == 0) {
                                                Game.Stations[CurrentStation].StopAtStation = false;
                                            } else if (!Interface.TryParseTime(Arguments[1], out arr)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time1 is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                arr = -1.0;
                                            }
                                        }
                                        if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
                                            if (string.Compare(Arguments[2], "=", StringComparison.OrdinalIgnoreCase) == 0) {
                                                Game.Stations[CurrentStation].IsTerminalStation = true;
                                            } else if (!Interface.TryParseTime(Arguments[2], out dep)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time2 is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                dep = -1.0;
                                            }
                                        }
                                        int stop = 0;
                                        if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out stop)) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "Stop is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            stop = 0;
                                        }
                                        int device = 0;
                                        if (Arguments.Length >= 5 && Arguments[4].Length > 0) {
                                            if (string.Compare(Arguments[4], "ats", StringComparison.OrdinalIgnoreCase) == 0) {
                                                device = 0;
                                            } else if (string.Compare(Arguments[4], "atc", StringComparison.OrdinalIgnoreCase) == 0) {
                                                device = 1;
                                            } else if (!Interface.TryParseIntVb6(Arguments[4], out device)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Device is invalid in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                device = 0;
                                            } else if (device != 0 & device != 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "The specified Device is not supported in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                device = 0;
                                            }
                                        }
                                        int depsnd = -1;
                                        if (!PreviewOnly) {
                                            if (Arguments.Length >= 6 && Arguments[5].Length > 0) {
                                                string f = Interface.GetCombinedFileName(SoundPath, Arguments[5]);
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Melody file " + f + " not found in Track.Station at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    depsnd = SoundManager.LoadSound(f, 30.0);
                                                }
                                            }
                                        }
                                        Game.Stations[CurrentStation].ArrivalTime = arr;
                                        Game.Stations[CurrentStation].ArrivalSoundIndex = -1;
                                        Game.Stations[CurrentStation].DepartureTime = dep;
                                        Game.Stations[CurrentStation].DepartureSoundIndex = depsnd;
                                        Game.Stations[CurrentStation].StopTime = 15.0;
                                        Game.Stations[CurrentStation].ForceStopSignal = stop == 1;
                                        Game.Stations[CurrentStation].OpenLeftDoors = true;
                                        Game.Stations[CurrentStation].OpenRightDoors = true;
                                        Game.Stations[CurrentStation].SecuritySystem = device == 1 ? Game.SecuritySystem.Atc : Game.SecuritySystem.Ats;
                                        Game.Stations[CurrentStation].Stops = new Game.StationStop[] { };
                                        Game.Stations[CurrentStation].PassengerRatio = 1.0;
                                        Game.Stations[CurrentStation].TimetableDaytimeTexture = -1;
                                        Game.Stations[CurrentStation].TimetableNighttimeTexture = -1;
                                        Data.Blocks[BlockIndex].Station = CurrentStation;
                                        Data.Blocks[BlockIndex].StationPassAlarm = false;
                                        CurrentStop = -1;
                                        DepartureSignalUsed = false;
                                    } break;
                                case "track.buffer": {
                                        if (!PreviewOnly) {
                                            int n = Game.BufferTrackPositions.Length;
                                            Array.Resize<double>(ref Game.BufferTrackPositions, n + 1);
                                            Game.BufferTrackPositions[n] = Data.TrackPosition;
                                        }
                                    } break;
                                case "track.form": {
                                        if (!PreviewOnly) {
                                            int idx1 = 0, idx2 = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx1)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx1 = 0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
                                                if (string.Compare(Arguments[1], "L", StringComparison.OrdinalIgnoreCase) == 0) {
                                                    idx2 = Form.SecondaryRailL;
                                                } else if (string.Compare(Arguments[1], "R", StringComparison.OrdinalIgnoreCase) == 0) {
                                                    idx2 = Form.SecondaryRailR;
                                                } else if (!Interface.TryParseIntVb6(Arguments[1], out idx2)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    idx2 = 0;
                                                }
                                            }
                                            if (IsRW) {
                                                if (idx2 == -9) idx2 = Form.SecondaryRailL;
                                                if (idx2 == 9) idx2 = Form.SecondaryRailR;
                                            }
                                            if (idx1 < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is expected to be non-negative in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (idx2 < 0 & idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is expected to be greater or equal to -2 in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx1 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx1].RailStart) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index1 could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                if (idx2 != Form.SecondaryRailStub & idx2 != Form.SecondaryRailL & idx2 != Form.SecondaryRailR && (idx2 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx2].RailStart)) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index2 could be out of range in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                int roof = 0, pf = 0;
                                                if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out roof)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    roof = 0;
                                                }
                                                if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out pf)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType is invalid in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    pf = 0;
                                                }
                                                if (roof != 0 & (roof < 0 || (roof >= Data.Structure.RoofL.Length || Data.Structure.RoofL[roof] == null) || (roof >= Data.Structure.RoofR.Length || Data.Structure.RoofR[roof] == null))) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (pf < 0 | (pf >= Data.Structure.FormL.Length || Data.Structure.FormL[pf] == null) & (pf >= Data.Structure.FormR.Length || Data.Structure.FormR[pf] == null)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references an object not loaded in Track.Form at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        int n = Data.Blocks[BlockIndex].Form.Length;
                                                        Array.Resize<Form>(ref Data.Blocks[BlockIndex].Form, n + 1);
                                                        Data.Blocks[BlockIndex].Form[n].PrimaryRail = idx1;
                                                        Data.Blocks[BlockIndex].Form[n].SecondaryRail = idx2;
                                                        Data.Blocks[BlockIndex].Form[n].FormType = pf;
                                                        Data.Blocks[BlockIndex].Form[n].RoofType = roof;
                                                    }

                                                }
                                            }
                                        }
                                    } break;
                                case "track.pole": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                if (idx >= Data.Blocks[BlockIndex].RailPole.Length) {
                                                    Array.Resize<Pole>(ref Data.Blocks[BlockIndex].RailPole, idx + 1);
                                                    Data.Blocks[BlockIndex].RailPole[idx].Mode = 0;
                                                    Data.Blocks[BlockIndex].RailPole[idx].Location = 0;
                                                    Data.Blocks[BlockIndex].RailPole[idx].Interval = 2.0 * Data.BlockInterval;
                                                    Data.Blocks[BlockIndex].RailPole[idx].Type = 0;
                                                }
                                                int typ = Data.Blocks[BlockIndex].RailPole[idx].Mode;
                                                int sttype = Data.Blocks[BlockIndex].RailPole[idx].Type;
                                                if (Arguments.Length >= 2 && Arguments[1].Length > 0) {
                                                    if (!Interface.TryParseIntVb6(Arguments[1], out typ)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Type is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        typ = 0;
                                                    }
                                                }
                                                if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
                                                    double loc;
                                                    if (!Interface.TryParseDoubleVb6(Arguments[2], out loc)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Locate is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        loc = 0;
                                                    } Data.Blocks[BlockIndex].RailPole[idx].Location = loc;
                                                }
                                                if (Arguments.Length >= 4 && Arguments[3].Length > 0) {
                                                    double dist;
                                                    if (!Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out dist)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Distance is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        dist = Data.BlockInterval;
                                                    }
                                                    Data.Blocks[BlockIndex].RailPole[idx].Interval = dist;
                                                }
                                                if (Arguments.Length >= 5 && Arguments[4].Length > 0) {
                                                    if (!Interface.TryParseIntVb6(Arguments[4], out sttype)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        sttype = 0;
                                                    }
                                                }
                                                if (typ < 0 || typ >= Data.Structure.Poles.Length || Data.Structure.Poles[typ] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Type references an object not loaded in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (sttype < 0 || sttype >= Data.Structure.Poles[typ].Length || Data.Structure.Poles[typ][sttype] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.Pole at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.Blocks[BlockIndex].RailPole[idx].Mode = typ;
                                                    Data.Blocks[BlockIndex].RailPole[idx].Type = sttype;
                                                    Data.Blocks[BlockIndex].RailPole[idx].Exists = true;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.poleend": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.PoleEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0 || idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index does not reference an existing rail in Track.PoleEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (idx < Data.Blocks[BlockIndex].RailPole.Length) {
                                                Data.Blocks[BlockIndex].RailPole[idx].Exists = false;
                                            }
                                        }
                                    } break;
                                case "track.wall": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            int dir = 0;
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out dir)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Direction is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                dir = 0;
                                            }
                                            int sttype = 0;
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out sttype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                sttype = 0;
                                            }
                                            if (dir <= 0 && (sttype >= Data.Structure.WallL.Length || Data.Structure.WallL[sttype] == null) ||
                                                dir >= 0 && (sttype >= Data.Structure.WallR.Length || Data.Structure.WallR[sttype] == null)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.Wall at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    if (idx >= Data.Blocks[BlockIndex].RailWall.Length) {
                                                        Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailWall, idx + 1);
                                                    }
                                                    Data.Blocks[BlockIndex].RailWall[idx].Exists = true;
                                                    Data.Blocks[BlockIndex].RailWall[idx].Type = sttype;
                                                    Data.Blocks[BlockIndex].RailWall[idx].Direction = dir;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.wallend": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailWall.Length) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index does not reference an existing wall in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.WallEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                Data.Blocks[BlockIndex].RailWall[idx].Exists = false;
                                            }
                                        }
                                    } break;
                                case "track.dike": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            int dir = 0;
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out dir)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Direction is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                dir = 0;
                                            }
                                            int sttype = 0;
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out sttype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                sttype = 0;
                                            }
                                            if (dir <= 0 && (sttype >= Data.Structure.DikeL.Length || Data.Structure.DikeL[sttype] == null) ||
                                                dir >= 0 && (sttype >= Data.Structure.DikeR.Length || Data.Structure.DikeR[sttype] == null)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.Dike at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    if (idx >= Data.Blocks[BlockIndex].RailDike.Length) {
                                                        Array.Resize<WallDike>(ref Data.Blocks[BlockIndex].RailDike, idx + 1);
                                                    }
                                                    Data.Blocks[BlockIndex].RailDike[idx].Exists = true;
                                                    Data.Blocks[BlockIndex].RailDike[idx].Type = sttype;
                                                    Data.Blocks[BlockIndex].RailDike[idx].Direction = dir;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.dikeend": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0 | idx >= Data.Blocks[BlockIndex].RailDike.Length) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index does not reference an existing dike in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx >= Data.Blocks[BlockIndex].Rail.Length || (!Data.Blocks[BlockIndex].Rail[idx].RailStart & !Data.Blocks[BlockIndex].Rail[idx].RailEnd)) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.DikeEnd at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                Data.Blocks[BlockIndex].RailDike[idx].Exists = false;
                                            }
                                        }
                                    } break;
                                case "track.marker": {
                                        if (!PreviewOnly) {
                                            if (Arguments.Length < 1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Track.Marker is expected to have at least one argument at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                string f = Interface.GetCombinedFileName(ObjectPath, Arguments[0]);
                                                if (!System.IO.File.Exists(f)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, true, "Marker texture " + f + " not found in Track.Marker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    double dist = Data.BlockInterval;
                                                    if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out dist)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Distance is invalid in Track.Marker at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        dist = Data.BlockInterval;
                                                    }
                                                    double start, end;
                                                    if (dist < 0.0) {
                                                        start = Data.TrackPosition;
                                                        end = Data.TrackPosition - dist;
                                                    } else {
                                                        start = Data.TrackPosition - dist;
                                                        end = Data.TrackPosition;
                                                    }
                                                    if (start < 0.0) start = 0.0;
                                                    if (end < 0.0) end = 0.0;
                                                    if (end <= start) end = start + 0.01;
                                                    int n = Data.Markers.Length;
                                                    Array.Resize<Marker>(ref Data.Markers, n + 1);
                                                    Data.Markers[n].StartingPosition = start;
                                                    Data.Markers[n].EndingPosition = end;
                                                    Data.Markers[n].Texture = TextureManager.RegisterTexture(f, new World.ColorRGB(64, 64, 64), 1, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                            }
                                        }
                                    } break;
                                case "track.height": {
                                        if (!PreviewOnly) {
                                            double h = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], UnitOfLength, out h)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.Height at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                h = 0.0;
                                            }
                                            Data.Blocks[BlockIndex].Height = IsRW ? h + 0.3 : h;
                                        }
                                    } break;
                                case "track.ground": {
                                        if (!PreviewOnly) {
                                            int cytype = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out cytype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxCyType is invalid in Track.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                cytype = 0;
                                            }
                                            if (cytype < Data.Structure.Cycle.Length && Data.Structure.Cycle[cytype] != null) {
                                                Data.Blocks[BlockIndex].Cycle = Data.Structure.Cycle[cytype];
                                            } else {
                                                if (cytype >= Data.Structure.Ground.Length || Data.Structure.Ground[cytype] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxCyType references an object not loaded in Track.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    Data.Blocks[BlockIndex].Cycle = new int[] { cytype };
                                                }
                                            }
                                        }
                                    } break;
                                case "track.crack": {
                                        if (!PreviewOnly) {
                                            int idx1 = 0, idx2 = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx1)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx1 = 0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out idx2)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx2 = 0;
                                            }
                                            int sttype = 0;
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out sttype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                sttype = 0;
                                            }
                                            if (sttype < 0 || (sttype >= Data.Structure.CrackL.Length || Data.Structure.CrackL[sttype] == null) || (sttype >= Data.Structure.CrackR.Length || Data.Structure.CrackR[sttype] == null)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx1 < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (idx2 < 0) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is expected to be non-negative in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else if (idx1 == idx2) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index1 is expected to be unequal to Index2 in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    if (idx1 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx1].RailStart) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, "Index1 could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    if (idx2 >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx2].RailStart) {
                                                        Interface.AddMessage(Interface.MessageType.Warning, false, "Index2 could be out of range in Track.Crack at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    int n = Data.Blocks[BlockIndex].Crack.Length;
                                                    Array.Resize<Crack>(ref Data.Blocks[BlockIndex].Crack, n + 1);
                                                    Data.Blocks[BlockIndex].Crack[n].PrimaryRail = idx1;
                                                    Data.Blocks[BlockIndex].Crack[n].SecondaryRail = idx2;
                                                    Data.Blocks[BlockIndex].Crack[n].Type = sttype;
                                                }
                                            }
                                        }
                                    } break;
                                case "track.freeobj": {
                                        if (!PreviewOnly) {
                                            int idx = 0, sttype = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out sttype)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                sttype = 0;
                                            }
                                            if (idx < -1) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Index is expected to be non-negative or -1 in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (sttype < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType is expected to be non-negative in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart)) {
                                                    Interface.AddMessage(Interface.MessageType.Warning, false, "Index could be out of range in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                }
                                                if (sttype >= Data.Structure.FreeObj.Length || Data.Structure.FreeObj[sttype] == null || Data.Structure.FreeObj[sttype] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references an object not loaded in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                } else {
                                                    double x = 0.0, y = 0.0;
                                                    double yaw = 0.0, pitch = 0.0, roll = 0.0;
                                                    int section = 0;
                                                    if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out x)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        x = 0.0;
                                                    }
                                                    if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], UnitOfLength, out y)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        y = 0.0;
                                                    }
                                                    if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out yaw)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        yaw = 0.0;
                                                    }
                                                    if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out pitch)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Pitch is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        pitch = 0.0;
                                                    }
                                                    if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[6], out roll)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        roll = 0.0;
                                                    }
                                                    if (Arguments.Length >= 8 && Arguments[7].Length > 0 && !Interface.TryParseIntVb6(Arguments[7], out section)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Section is invalid in Track.FreeObj at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                        section = 0;
                                                    } else if (Arguments.Length >= 8 && Arguments[7].Length > 0) {
                                                        Interface.AddMessage(Interface.MessageType.Information, false, "Section in Track.FreeObj maybe be removed in future versions. Please convert to Track.SigF at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    }
                                                    if (idx == -1) {
                                                        int n;
                                                        n = Data.Blocks[BlockIndex].GroundFreeObj.Length;
                                                        Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].GroundFreeObj, n + 1);
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].TrackPosition = Data.TrackPosition;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Type = sttype;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].X = x;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Y = y;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Yaw = yaw * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Pitch = pitch * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Roll = roll * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].GroundFreeObj[n].Section = CurrentSection + section;
                                                    } else {
                                                        if (idx >= Data.Blocks[BlockIndex].RailFreeObj.Length) {
                                                            Array.Resize<FreeObj[]>(ref Data.Blocks[BlockIndex].RailFreeObj, idx + 1);
                                                        }
                                                        int n;
                                                        if (Data.Blocks[BlockIndex].RailFreeObj[idx] == null) {
                                                            Data.Blocks[BlockIndex].RailFreeObj[idx] = new FreeObj[1];
                                                            n = 0;
                                                        } else {
                                                            n = Data.Blocks[BlockIndex].RailFreeObj[idx].Length;
                                                            Array.Resize<FreeObj>(ref Data.Blocks[BlockIndex].RailFreeObj[idx], n + 1);
                                                        }
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].TrackPosition = Data.TrackPosition;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Type = sttype;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].X = x;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Y = y;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Yaw = yaw * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Pitch = pitch * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Roll = roll * 0.0174532925199433;
                                                        Data.Blocks[BlockIndex].RailFreeObj[idx][n].Section = CurrentSection + section;
                                                    }
                                                }
                                            }
                                        }
                                    } break;
                                case "track.back": {
                                        if (!PreviewOnly) {
                                            int typ = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out typ)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "TextureType is invalid in Track.Ground at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                typ = 0;
                                            }
                                            if (typ < 0 | typ >= Data.Backgrounds.Length) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "TextureType references a texture not loaded in Track.Back at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else if (Data.Backgrounds[typ].Texture < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "TextureType has not been loaded via Texture.Background in Track.Back at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                Data.Blocks[BlockIndex].Background = typ;
                                            }
                                        }
                                    } break;
                                case "track.announce": {
                                        if (!PreviewOnly) {
                                            string f = Interface.GetCombinedFileName(SoundPath, Arguments[0]);
                                            if (!System.IO.File.Exists(f)) {
                                                Interface.AddMessage(Interface.MessageType.Error, true, "Sound file " + f + " not found in Track.Announce at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                double speed = 0.0;
                                                if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out speed)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Speed is invalid in Track.Announce at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    speed = 0.0;
                                                }
                                                int n = Data.Blocks[BlockIndex].Sound.Length;
                                                Array.Resize<Sound>(ref Data.Blocks[BlockIndex].Sound, n + 1);
                                                Data.Blocks[BlockIndex].Sound[n].TrackPosition = Data.TrackPosition;
                                                Data.Blocks[BlockIndex].Sound[n].SoundIndex = SoundManager.LoadSound(f, 10.0);
                                                Data.Blocks[BlockIndex].Sound[n].Type = speed == 0.0 ? SoundType.TrainStatic : SoundType.TrainDynamic;
                                                Data.Blocks[BlockIndex].Sound[n].Speed = speed * 0.277777777777778;
                                            }
                                        }
                                    } break;
                                case "track.doppler": {
                                        if (!PreviewOnly) {
                                            string f = Interface.GetCombinedFileName(SoundPath, Arguments[0]);
                                            if (!System.IO.File.Exists(f)) {
                                                Interface.AddMessage(Interface.MessageType.Error, true, "Sound file " + f + " not found in Track.Doppler at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            } else {
                                                double x = 0.0, y = 0.0;
                                                if (Arguments.Length >= 2 && Arguments[1].Length > 0 & !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.Doppler at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    x = 0.0;
                                                }
                                                if (Arguments.Length >= 3 && Arguments[2].Length > 0 & !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.Doppler at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                    y = 0.0;
                                                }
                                                const double radius = 15.0;
                                                int n = Data.Blocks[BlockIndex].Sound.Length;
                                                Array.Resize<Sound>(ref Data.Blocks[BlockIndex].Sound, n + 1);
                                                Data.Blocks[BlockIndex].Sound[n].TrackPosition = Data.TrackPosition;
                                                Data.Blocks[BlockIndex].Sound[n].SoundIndex = SoundManager.LoadSound(f, radius);
                                                Data.Blocks[BlockIndex].Sound[n].Type = SoundType.World;
                                                Data.Blocks[BlockIndex].Sound[n].X = x;
                                                Data.Blocks[BlockIndex].Sound[n].Y = y;
                                                Data.Blocks[BlockIndex].Sound[n].Radius = radius;
                                            }
                                        }
                                    } break;
                                case "track.pretrain": {
                                        if (!PreviewOnly) {
                                            double time = 0.0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 & !Interface.TryParseTime(Arguments[0], out time)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time is invalid in Track.PreTrain at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                time = 0.0;
                                            }
                                            int n = Game.BogusPretrainInstructions.Length;
                                            if (n != 0 && Game.BogusPretrainInstructions[n - 1].Time >= time) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Time is expected to be in ascending order between Track.Pretrain commands in Track.PreTrain at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            }
                                            Array.Resize<Game.BogusPretrainInstruction>(ref Game.BogusPretrainInstructions, n + 1);
                                            Game.BogusPretrainInstructions[n].TrackPosition = Data.TrackPosition;
                                            Game.BogusPretrainInstructions[n].Time = time;
                                            Game.PretrainsUsed = 1;
                                        }
                                    } break;
                                case "track.pointofinterest": {
                                        if (!PreviewOnly) {
                                            int idx = 0;
                                            if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out idx)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx < 0) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex is expected to be non-negative in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                idx = 0;
                                            }
                                            if (idx >= 0 && (idx >= Data.Blocks[BlockIndex].Rail.Length || !Data.Blocks[BlockIndex].Rail[idx].RailStart)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "RailIndex references a non-existing rail in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                            }
                                            double x = 0.0, y = 0.0;
                                            if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], UnitOfLength, out x)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                x = 0.0;
                                            }
                                            if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], UnitOfLength, out y)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                y = 0.0;
                                            }
                                            double yaw = 0.0, pitch = 0.0, roll = 0.0;
                                            if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], out yaw)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Yaw is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                yaw = 0.0;
                                            }
                                            if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[4], out pitch)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Pitch is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                pitch = 0.0;
                                            }
                                            if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[5], out roll)) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "Roll is invalid in Track.PointOfInterest at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                                roll = 0.0;
                                            }
                                            string text = null;
                                            if (Arguments.Length >= 7 && Arguments[6].Length != 0) {
                                                text = Arguments[6];
                                            }
                                            int n = Data.Blocks[BlockIndex].PointsOfInterest.Length;
                                            Array.Resize<PointOfInterest>(ref Data.Blocks[BlockIndex].PointsOfInterest, n + 1);
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].TrackPosition = Data.TrackPosition;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].RailIndex = idx;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].X = x;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].Y = y;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].Yaw = 0.0174532925199433 * yaw;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].Pitch = 0.0174532925199433 * pitch;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].Roll = 0.0174532925199433 * roll;
                                            Data.Blocks[BlockIndex].PointsOfInterest[n].Text = text;
                                        }
                                    } break;
                                default:
                                    Interface.AddMessage(Interface.MessageType.Warning, false, "The command " + Command + " is not supported at line " + Expressions[j].Line.ToString(Culture) + ", column " + Expressions[j].Column.ToString(Culture) + " in file " + FileName);
                                    break;
                            }
                        }
                    }
                }
            }
            if (!PreviewOnly) {
                // timetable
                Timetable.CustomTextureIndices = new int[Data.TimetableDaytime.Length + Data.TimetableNighttime.Length];
                int n = 0;
                for (int i = 0; i < Data.TimetableDaytime.Length; i++) {
                    if (Data.TimetableDaytime[i] >= 0) {
                        Timetable.CustomTextureIndices[n] = Data.TimetableDaytime[i];
                        n++;
                    }
                }
                for (int i = 0; i < Data.TimetableNighttime.Length; i++) {
                    if (Data.TimetableNighttime[i] >= 0) {
                        Timetable.CustomTextureIndices[n] = Data.TimetableNighttime[i];
                        n++;
                    }
                }
                Array.Resize<int>(ref Timetable.CustomTextureIndices, n);
            }
            // blocks
            Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
        }

        // ================================

        // create missing blocks
        private static void CreateMissingBlocks(ref RouteData Data, ref int BlocksUsed, int ToIndex, bool PreviewOnly) {
            if (ToIndex >= BlocksUsed) {
                while (Data.Blocks.Length <= ToIndex) {
                    Array.Resize<Block>(ref Data.Blocks, Data.Blocks.Length << 1);
                }
                for (int i = BlocksUsed; i <= ToIndex; i++) {
                    Data.Blocks[i] = new Block();
                    if (!PreviewOnly) {
                        Data.Blocks[i].Background = -1;
                        Data.Blocks[i].Brightness = new Brightness[] { };
                        Data.Blocks[i].Fog = Data.Blocks[i - 1].Fog;
                        Data.Blocks[i].FogDefined = false;
                        Data.Blocks[i].Cycle = Data.Blocks[i - 1].Cycle;
                        Data.Blocks[i].Height = double.NaN;
                    }
                    Data.Blocks[i].RailType = new int[Data.Blocks[i - 1].RailType.Length];
                    for (int j = 0; j < Data.Blocks[i].RailType.Length; j++) {
                        Data.Blocks[i].RailType[j] = Data.Blocks[i - 1].RailType[j];
                    }
                    Data.Blocks[i].Rail = new Rail[Data.Blocks[i - 1].Rail.Length];
                    for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
                        Data.Blocks[i].Rail[j].RailStart = Data.Blocks[i - 1].Rail[j].RailStart;
                        Data.Blocks[i].Rail[j].RailStartX = Data.Blocks[i - 1].Rail[j].RailStartX;
                        Data.Blocks[i].Rail[j].RailStartY = Data.Blocks[i - 1].Rail[j].RailStartY;
                        Data.Blocks[i].Rail[j].RailStartRefreshed = false;
                        Data.Blocks[i].Rail[j].RailEnd = false;
                        Data.Blocks[i].Rail[j].RailEndX = Data.Blocks[i - 1].Rail[j].RailStartX;
                        Data.Blocks[i].Rail[j].RailEndY = Data.Blocks[i - 1].Rail[j].RailStartY;
                    }
                    if (!PreviewOnly) {
                        Data.Blocks[i].RailWall = new WallDike[Data.Blocks[i - 1].RailWall.Length];
                        for (int j = 0; j < Data.Blocks[i].RailWall.Length; j++) {
                            Data.Blocks[i].RailWall[j] = Data.Blocks[i - 1].RailWall[j];
                        }
                        Data.Blocks[i].RailDike = new WallDike[Data.Blocks[i - 1].RailDike.Length];
                        for (int j = 0; j < Data.Blocks[i].RailDike.Length; j++) {
                            Data.Blocks[i].RailDike[j] = Data.Blocks[i - 1].RailDike[j];
                        }
                        Data.Blocks[i].RailPole = new Pole[Data.Blocks[i - 1].RailPole.Length];
                        for (int j = 0; j < Data.Blocks[i].RailPole.Length; j++) {
                            Data.Blocks[i].RailPole[j] = Data.Blocks[i - 1].RailPole[j];
                        }
                        Data.Blocks[i].Form = new Form[] { };
                        Data.Blocks[i].Crack = new Crack[] { };
                        Data.Blocks[i].Signal = new Signal[] { };
                        Data.Blocks[i].Section = new Section[] { };
                        Data.Blocks[i].Sound = new Sound[] { };
                        Data.Blocks[i].Transponder = new Transponder[] { };
                        Data.Blocks[i].RailFreeObj = new FreeObj[][] { };
                        Data.Blocks[i].GroundFreeObj = new FreeObj[] { };
                        Data.Blocks[i].PointsOfInterest = new PointOfInterest[] { };
                    }
                    Data.Blocks[i].Pitch = Data.Blocks[i - 1].Pitch;
                    Data.Blocks[i].Limit = new Limit[] { };
                    Data.Blocks[i].Stop = new Stop[] { };
                    Data.Blocks[i].Station = -1;
                    Data.Blocks[i].StationPassAlarm = false;
                    Data.Blocks[i].CurrentTrackState = Data.Blocks[i - 1].CurrentTrackState;
                    Data.Blocks[i].Turn = 0.0;
                    Data.Blocks[i].Accuracy = Data.Blocks[i - 1].Accuracy;
                    Data.Blocks[i].AdhesionMultiplier = Data.Blocks[i - 1].AdhesionMultiplier;
                }
                BlocksUsed = ToIndex + 1;
            }
        }

        // get mirrored object
        private static ObjectManager.UnifiedObject GetMirroredObject(ObjectManager.UnifiedObject Prototype) {
            if (Prototype is ObjectManager.StaticObject) {
                ObjectManager.StaticObject s = (ObjectManager.StaticObject)Prototype;
                return GetMirroredStaticObject(s);
            } else if (Prototype is ObjectManager.AnimatedObjectCollection) {
                ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)Prototype;
                ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
                Result.Objects = new ObjectManager.AnimatedObject[a.Objects.Length];
                for (int i = 0; i < a.Objects.Length; i++) {
                    Result.Objects[i] = a.Objects[i].Clone();
                    for (int j = 0; j < a.Objects[i].States.Length; j++) {
                        Result.Objects[i].States[j].Object = GetMirroredStaticObject(a.Objects[i].States[j].Object);
                    }
                    Result.Objects[i].TranslateXDirection.X *= -1.0;
                    Result.Objects[i].TranslateYDirection.X *= -1.0;
                    Result.Objects[i].TranslateZDirection.X *= -1.0;
                    Result.Objects[i].RotateXDirection.X *= -1.0;
                    Result.Objects[i].RotateYDirection.X *= -1.0;
                    Result.Objects[i].RotateZDirection.X *= -1.0;
                }
                return Result;
            } else {
                return null;
            }
        }
        private static ObjectManager.StaticObject GetMirroredStaticObject(ObjectManager.StaticObject Prototype) {
            ObjectManager.StaticObject Result = ObjectManager.CloneObject(Prototype);
            for (int i = 0; i < Result.Meshes.Length; i++) {
                for (int j = 0; j < Result.Meshes[i].Vertices.Length; j++) {
                    Result.Meshes[i].Vertices[j].Coordinates.X = -Result.Meshes[i].Vertices[j].Coordinates.X;
                }
                for (int j = 0; j < Result.Meshes[i].Faces.Length; j++) {
                    for (int k = 0; k < Result.Meshes[i].Faces[j].Vertices.Length; k++) {
                        Result.Meshes[i].Faces[j].Vertices[k].Normal.X = -Result.Meshes[i].Faces[j].Vertices[k].Normal.X;
                    }
                    for (int k = 0; k < Result.Meshes[i].Faces[j].Vertices.Length >> 1; k++) {
                        int h = Result.Meshes[i].Faces[j].Vertices.Length - k - 1;
                        World.MeshFaceVertex t = Result.Meshes[i].Faces[j].Vertices[k];
                        Result.Meshes[i].Faces[j].Vertices[k] = Result.Meshes[i].Faces[j].Vertices[h];
                        Result.Meshes[i].Faces[j].Vertices[h] = t;
                    }
                }
            }
            return Result;
        }

        // get transformed object
        private static ObjectManager.StaticObject GetTransformedStaticObject(ObjectManager.StaticObject Prototype, double NearDistance, double FarDistance) {
            ObjectManager.StaticObject Result = ObjectManager.CloneObject(Prototype);
            int n = 0;
            double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
            for (int i = 0; i < Result.Meshes.Length; i++) {
                for (int j = 0; j < Result.Meshes[i].Vertices.Length; j++) {
                    if (n == 2) {
                        x2 = Result.Meshes[i].Vertices[j].Coordinates.X;
                    } else if (n == 3) {
                        x3 = Result.Meshes[i].Vertices[j].Coordinates.X;
                    } else if (n == 6) {
                        x6 = Result.Meshes[i].Vertices[j].Coordinates.X;
                    } else if (n == 7) {
                        x7 = Result.Meshes[i].Vertices[j].Coordinates.X;
                    } n++;
                    if (n == 8) break;
                } if (n == 8) break;
            }
            if (n >= 4) {
                int m = 0;
                for (int i = 0; i < Result.Meshes.Length; i++) {
                    for (int j = 0; j < Result.Meshes[i].Vertices.Length; j++) {
                        if (m == 0) {
                            Result.Meshes[i].Vertices[j].Coordinates.X = NearDistance - x3;
                        } else if (m == 1) {
                            Result.Meshes[i].Vertices[j].Coordinates.X = FarDistance - x2;
                            if (n < 8) {
                                m = 8;
                                break;
                            }
                        } else if (m == 4) {
                            Result.Meshes[i].Vertices[j].Coordinates.X = NearDistance - x7;
                        } else if (m == 5) {
                            Result.Meshes[i].Vertices[j].Coordinates.X = NearDistance - x6;
                            m = 8;
                            break;
                        } m++;
                        if (m == 8) break;
                    } if (m == 8) break;
                }
            }
            return Result;
        }

        // load all textures
        private static int[] LoadAllTextures(string BaseFile, World.ColorRGB TransparentColor, byte TransparentColorUsed, TextureManager.TextureLoadMode LoadMode) {
            string Folder = Interface.GetCorrectedFolderName(System.IO.Path.GetDirectoryName(BaseFile));
            if (!System.IO.Directory.Exists(Folder)) return new int[] { };
            string Name = System.IO.Path.GetFileNameWithoutExtension(BaseFile);
            int[] Textures = new int[] { };
            string[] Files = System.IO.Directory.GetFiles(Folder);
            for (int i = 0; i < Files.Length; i++) {
                string a = System.IO.Path.GetFileNameWithoutExtension(Files[i]);
                if (a.StartsWith(Name, StringComparison.OrdinalIgnoreCase)) {
                    if (a.Length > Name.Length) {
                        string b = a.Substring(Name.Length).TrimStart();
                        int j; if (int.TryParse(b, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out j)) {
                            if (j >= 0) {
                                string c = System.IO.Path.GetExtension(Files[i]);
                                switch (c.ToLowerInvariant()) {
                                    case ".bmp":
                                    case ".gif":
                                    case ".jpg":
                                    case ".jpeg":
                                    case ".png":
                                    case ".tif":
                                    case ".tiff":
                                        if (j >= Textures.Length) {
                                            int n = Textures.Length;
                                            Array.Resize<int>(ref Textures, j + 1);
                                            for (int k = n; k < j; k++) {
                                                Textures[k] = -1;
                                            }
                                        }
                                        Textures[j] = TextureManager.RegisterTexture(Files[i], TransparentColor, TransparentColorUsed, LoadMode, TextureManager.TextureWrapMode.ClampToEdge, true, 0, 0, 0, 0);
                                        TextureManager.UseTexture(Textures[j], TextureManager.UseMode.Normal);
                                        break;
                                }
                            }
                        }
                    }
                }
            } return Textures;
        }

        // ================================

        // get brightness
        private static double GetBrightness(ref RouteData Data, double TrackPosition) {
            double tmin = double.PositiveInfinity;
            double tmax = double.NegativeInfinity;
            double bmin = 1.0, bmax = 1.0;
            for (int i = 0; i < Data.Blocks.Length; i++) {
                for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++) {
                    if (Data.Blocks[i].Brightness[j].TrackPosition <= TrackPosition) {
                        tmin = Data.Blocks[i].Brightness[j].TrackPosition;
                        bmin = (double)Data.Blocks[i].Brightness[j].Value;
                    }
                }
            }
            for (int i = Data.Blocks.Length - 1; i >= 0; i--) {
                for (int j = Data.Blocks[i].Brightness.Length - 1; j >= 0; j--) {
                    if (Data.Blocks[i].Brightness[j].TrackPosition >= TrackPosition) {
                        tmax = Data.Blocks[i].Brightness[j].TrackPosition;
                        bmax = (double)Data.Blocks[i].Brightness[j].Value;
                    }
                }
            }
            if (tmin == double.PositiveInfinity & tmax == double.NegativeInfinity) {
                return 1.0;
            } else if (tmin == double.PositiveInfinity) {
                return (bmax - 1.0) * TrackPosition / tmax + 1.0;
            } else if (tmax == double.NegativeInfinity) {
                return bmin;
            } else if (tmin == tmax) {
                return 0.5 * (bmin + bmax);
            } else {
                double n = (TrackPosition - tmin) / (tmax - tmin);
                return (1.0 - n) * bmin + n * bmax;
            }
        }

        // apply route data
        private static void ApplyRouteData(string FileName, System.Text.Encoding Encoding, string CompatibilityPath, ref RouteData Data, bool PreviewOnly) {
            string SignalPath, LimitPath, LimitGraphicsPath, TransponderPath;
            ObjectManager.StaticObject SignalPost, LimitPostStraight, LimitPostLeft, LimitPostRight, LimitPostInfinite;
            ObjectManager.StaticObject LimitOneDigit, LimitTwoDigits, LimitThreeDigits, StopPost;
            ObjectManager.StaticObject TransponderS, TransponderSN, TransponderFalseStart, TransponderPOrigin, TransponderPStop;
            if (!PreviewOnly) {
                // load signal compatibility objects
                SignalPath = Interface.GetCombinedFolderName(CompatibilityPath, "Signals");
                SignalPost = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(SignalPath, "signal_post.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                // load other compatibility objects
                LimitPath = Interface.GetCombinedFolderName(CompatibilityPath, "Limits");
                LimitGraphicsPath = Interface.GetCombinedFolderName(LimitPath, "Graphics");
                LimitPostStraight = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_straight.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitPostLeft = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_left.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitPostRight = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_right.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitPostInfinite = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_infinite.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitOneDigit = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_1_digit.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitTwoDigits = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_2_digits.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                LimitThreeDigits = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(LimitPath, "limit_3_digits.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                StopPost = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(CompatibilityPath, "stop.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                TransponderPath = Interface.GetCombinedFolderName(CompatibilityPath, "transponder");
                TransponderS = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(TransponderPath, "s.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                TransponderSN = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(TransponderPath, "sn.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                TransponderFalseStart = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(TransponderPath, "falsestart.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                TransponderPOrigin = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(TransponderPath, "porigin.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
                TransponderPStop = ObjectManager.LoadStaticObject(Interface.GetCombinedFileName(TransponderPath, "pstop.csv"), Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
            } else {
                SignalPath = null;
                LimitPath = null;
                LimitGraphicsPath = null;
                TransponderPath = null;
                SignalPost = null;
                LimitPostStraight = null;
                LimitPostLeft = null;
                LimitPostRight = null;
                LimitPostInfinite = null;
                LimitOneDigit = null;
                LimitTwoDigits = null;
                LimitThreeDigits = null;
                StopPost = null;
                TransponderS = null;
                TransponderSN = null;
                TransponderFalseStart = null;
                TransponderPOrigin = null;
                TransponderPStop = null;
            }
            // initialize
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            int LastBlock = (int)Math.Floor((Data.TrackPosition + 600.0) / Data.BlockInterval + 0.001) + 1;
            int BlocksUsed = Data.Blocks.Length;
            CreateMissingBlocks(ref Data, ref BlocksUsed, LastBlock, PreviewOnly);
            Array.Resize<Block>(ref Data.Blocks, BlocksUsed);
            // interpolate height
            if (!PreviewOnly) {
                int z = 0;
                for (int i = 0; i < Data.Blocks.Length; i++) {
                    if (!double.IsNaN(Data.Blocks[i].Height)) {
                        for (int j = i - 1; j >= 0; j--) {
                            if (!double.IsNaN(Data.Blocks[j].Height)) {
                                double a = Data.Blocks[j].Height;
                                double b = Data.Blocks[i].Height;
                                double d = (b - a) / (double)(i - j);
                                for (int k = j + 1; k < i; k++) {
                                    a += d;
                                    Data.Blocks[k].Height = a;
                                }
                                break;
                            }
                        }
                        z = i;
                    }
                }
                for (int i = z + 1; i < Data.Blocks.Length; i++) {
                    Data.Blocks[i].Height = Data.Blocks[z].Height;
                }
            }
            // background
            if (!PreviewOnly) {
                if (Data.Blocks[0].Background >= 0 & Data.Blocks[0].Background < Data.Backgrounds.Length) {
                    World.CurrentBackground = Data.Backgrounds[Data.Blocks[0].Background];
                } else {
                    World.CurrentBackground = new World.Background(-1, 6, false);
                }
                World.TargetBackground = World.CurrentBackground;
            }
            // create objects and track
            World.Vector3D Position = new World.Vector3D(0.0, 0.0, 0.0);
            World.Vector2D Direction = new World.Vector2D(0.0, 1.0);
            TrackManager.CurrentTrack = new TrackManager.Track();
            TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[] { };
            double CurrentSpeedLimit = double.PositiveInfinity;
            int CurrentRunIndex = 0;
            int CurrentFlangeIndex = 0;
            if (Data.FirstUsedBlock < 0) Data.FirstUsedBlock = 0;
            TrackManager.CurrentTrack.Elements = new TrackManager.TrackElement[256];
            int CurrentTrackLength = 0;
            int PreviousFogElement = -1;
            int PreviousFogEvent = -1;
            Game.Fog PreviousFog = new Game.Fog((float)(World.BackgroundImageDistance + World.ExtraViewingDistance), (float)(World.BackgroundImageDistance + 2.0 * World.ExtraViewingDistance), new World.ColorRGB(128, 128, 128), -Data.BlockInterval);
            Game.Fog CurrentFog = new Game.Fog((float)(World.BackgroundImageDistance + World.ExtraViewingDistance), (float)(World.BackgroundImageDistance + 2.0 * World.ExtraViewingDistance), new World.ColorRGB(128, 128, 128), 0.0);
            int CurrentBrightnessElement = -1;
            int CurrentBrightnessEvent = -1;
            float CurrentBrightnessValue = 1.0f;
            double CurrentBrightnessTrackPosition = (double)Data.FirstUsedBlock * Data.BlockInterval;
            // process blocks
            double updprgfac = Data.Blocks.Length - Data.FirstUsedBlock == 0 ? 0.5 : 0.5 / (double)(Data.Blocks.Length - Data.FirstUsedBlock);
            for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++) {
                Loading.RouteProgress = 0.5 + (double)(i - Data.FirstUsedBlock) * updprgfac;
                if ((i & 15) == 0) {
                    System.Threading.Thread.Sleep(1);
                    if (Loading.Cancel) return;
                }
                double StartingDistance = (double)i * Data.BlockInterval;
                double EndingDistance = StartingDistance + Data.BlockInterval;
                // normalize
                World.Normalize(ref Direction.X, ref Direction.Y);
                // track
                if (!PreviewOnly) {
                    if (Data.Blocks[i].Cycle.Length == 1 && Data.Blocks[i].Cycle[0] == -1) {
                        if (Data.Structure.Cycle.Length == 0 || Data.Structure.Cycle[0] == null) {
                            Data.Blocks[i].Cycle = new int[] { 0 };
                        } else {
                            Data.Blocks[i].Cycle = Data.Structure.Cycle[0];
                        }
                    }
                }
                TrackManager.TrackElement WorldTrackElement = Data.Blocks[i].CurrentTrackState;
                int n = CurrentTrackLength;
                if (n >= TrackManager.CurrentTrack.Elements.Length) {
                    Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, TrackManager.CurrentTrack.Elements.Length << 1);
                }
                CurrentTrackLength++;
                TrackManager.CurrentTrack.Elements[n] = WorldTrackElement;
                TrackManager.CurrentTrack.Elements[n].WorldPosition = Position;
                TrackManager.CurrentTrack.Elements[n].WorldDirection = new World.Vector3D(Direction, Data.Blocks[i].Pitch);
                TrackManager.CurrentTrack.Elements[n].WorldSide = new World.Vector3D(Direction.Y, 0.0, -Direction.X);
                World.Cross(TrackManager.CurrentTrack.Elements[n].WorldDirection.X, TrackManager.CurrentTrack.Elements[n].WorldDirection.Y, TrackManager.CurrentTrack.Elements[n].WorldDirection.Z, TrackManager.CurrentTrack.Elements[n].WorldSide.X, TrackManager.CurrentTrack.Elements[n].WorldSide.Y, TrackManager.CurrentTrack.Elements[n].WorldSide.Z, out TrackManager.CurrentTrack.Elements[n].WorldUp.X, out TrackManager.CurrentTrack.Elements[n].WorldUp.Y, out TrackManager.CurrentTrack.Elements[n].WorldUp.Z);
                TrackManager.CurrentTrack.Elements[n].StartingTrackPosition = StartingDistance;
                TrackManager.CurrentTrack.Elements[n].Events = new TrackManager.GeneralEvent[] { };
                TrackManager.CurrentTrack.Elements[n].Inaccuracy = 0.5 * (Math.Tanh(0.2 * Data.Blocks[i].Accuracy - 2.3) + 1);
                TrackManager.CurrentTrack.Elements[n].AdhesionMultiplier = Data.Blocks[i].AdhesionMultiplier;
                // background
                if (!PreviewOnly) {
                    if (Data.Blocks[i].Background >= 0) {
                        int typ;
                        if (i == Data.FirstUsedBlock) {
                            typ = Data.Blocks[i].Background;
                        } else {
                            typ = Data.Backgrounds.Length > 0 ? 0 : -1;
                            for (int j = i - 1; j >= Data.FirstUsedBlock; j--) {
                                if (Data.Blocks[j].Background >= 0) {
                                    typ = Data.Blocks[j].Background;
                                    break;
                                }
                            }
                        }
                        if (typ >= 0 & typ < Data.Backgrounds.Length) {
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BackgroundChangeEvent(0.0, Data.Backgrounds[typ], Data.Backgrounds[Data.Blocks[i].Background]);
                        }
                    }
                }
                // brightness
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++) {
                        int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                        Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                        double d = Data.Blocks[i].Brightness[j].TrackPosition - StartingDistance;
                        TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.BrightnessChangeEvent(d, Data.Blocks[i].Brightness[j].Value, CurrentBrightnessValue, Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition, Data.Blocks[i].Brightness[j].Value, 0.0);
                        if (CurrentBrightnessElement >= 0 & CurrentBrightnessEvent >= 0) {
                            TrackManager.BrightnessChangeEvent bce = (TrackManager.BrightnessChangeEvent)TrackManager.CurrentTrack.Elements[CurrentBrightnessElement].Events[CurrentBrightnessEvent];
                            bce.NextBrightness = Data.Blocks[i].Brightness[j].Value;
                            bce.NextDistance = Data.Blocks[i].Brightness[j].TrackPosition - CurrentBrightnessTrackPosition;
                        }
                        CurrentBrightnessElement = n;
                        CurrentBrightnessEvent = m;
                        CurrentBrightnessValue = Data.Blocks[i].Brightness[j].Value;
                        CurrentBrightnessTrackPosition = Data.Blocks[i].Brightness[j].TrackPosition;
                    }
                }
                // fog
                if (!PreviewOnly) {
                    if (Data.FogTransitionMode) {
                        if (Data.Blocks[i].FogDefined) {
                            Data.Blocks[i].Fog.TrackPosition = StartingDistance;
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, Data.Blocks[i].Fog, Data.Blocks[i].Fog);
                            if (PreviousFogElement >= 0 & PreviousFogEvent >= 0) {
                                TrackManager.FogChangeEvent e = (TrackManager.FogChangeEvent)TrackManager.CurrentTrack.Elements[PreviousFogElement].Events[PreviousFogEvent];
                                e.NextFog = Data.Blocks[i].Fog;
                            } else {
                                Game.PreviousFog = PreviousFog;
                                Game.CurrentFog = PreviousFog;
                                Game.NextFog = Data.Blocks[i].Fog;
                            }
                            PreviousFog = Data.Blocks[i].Fog;
                            PreviousFogElement = n;
                            PreviousFogEvent = m;
                        }
                    } else {
                        Data.Blocks[i].Fog.TrackPosition = StartingDistance + Data.BlockInterval;
                        int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                        Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                        TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.FogChangeEvent(0.0, PreviousFog, CurrentFog, Data.Blocks[i].Fog);
                        PreviousFog = CurrentFog;
                        CurrentFog = Data.Blocks[i].Fog;
                    }
                }
                // rail sounds
                if (!PreviewOnly) {
                    int j = Data.Blocks[i].RailType[0];
                    int r = j < Data.Structure.Run.Length ? Data.Structure.Run[j] : 0;
                    int f = j < Data.Structure.Flange.Length ? Data.Structure.Flange[j] : 0;
                    int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                    Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                    TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.RailSoundsChangeEvent(0.0, CurrentRunIndex, CurrentFlangeIndex, r, f);
                    CurrentRunIndex = r;
                    CurrentFlangeIndex = f;
                }
                // point sound
                if (!PreviewOnly) {
                    if (i < Data.Blocks.Length - 1) {
                        bool q = false;
                        for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
                            if (Data.Blocks[i].Rail[j].RailStart & Data.Blocks[i + 1].Rail.Length > j) {
                                bool qx = Math.Sign(Data.Blocks[i].Rail[j].RailStartX) != Math.Sign(Data.Blocks[i + 1].Rail[j].RailEndX);
                                bool qy = Data.Blocks[i].Rail[j].RailStartY * Data.Blocks[i + 1].Rail[j].RailEndY <= 0.0;
                                if (qx & qy) {
                                    q = true;
                                    break;
                                }
                            }
                        }
                        if (q) {
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(0.0, TrackManager.SoundEvent.SoundIndexTrainPoint, int.MinValue, int.MinValue, false, true, new World.Vector3D(0.0, 0.0, 0.0), 12.5);
                        }
                    }
                }
                // station
                if (Data.Blocks[i].Station >= 0) {
                    // station
                    int s = Data.Blocks[i].Station;
                    int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                    Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                    TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.StationStartEvent(0.0, s);
                    double dx, dy = 3.0;
                    if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors) {
                        dx = -5.0;
                    } else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors) {
                        dx = 5.0;
                    } else {
                        dx = 0.0;
                    }
                    Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
                    Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
                    Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
                    // passalarm
                    if (!PreviewOnly) {
                        if (Data.Blocks[i].StationPassAlarm) {
                            int b = i - 6;
                            if (b >= 0) {
                                int j = b - Data.FirstUsedBlock;
                                if (j >= 0) {
                                    m = TrackManager.CurrentTrack.Elements[j].Events.Length;
                                    Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[j].Events, m + 1);
                                    TrackManager.CurrentTrack.Elements[j].Events[m] = new TrackManager.StationPassAlarmEvent(0.0);
                                }
                            }
                        }
                    }
                }
                // stop
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Blocks[i].Stop.Length; j++) {
                        int s = Data.Blocks[i].Stop[j].Station;
                        int t = Game.Stations[s].Stops.Length;
                        Array.Resize<Game.StationStop>(ref Game.Stations[s].Stops, t + 1);
                        Game.Stations[s].Stops[t].TrackPosition = Data.Blocks[i].Stop[j].TrackPosition;
                        Game.Stations[s].Stops[t].ForwardTolerance = Data.Blocks[i].Stop[j].ForwardTolerance;
                        Game.Stations[s].Stops[t].BackwardTolerance = Data.Blocks[i].Stop[j].BackwardTolerance;
                        Game.Stations[s].Stops[t].Cars = Data.Blocks[i].Stop[j].Cars;
                        double dx, dy = 2.0;
                        if (Data.Blocks[i].Stop[j].Direction != 0 & Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors) {
                            Game.Stations[s].OpenLeftDoors = Data.Blocks[i].Stop[j].Direction > 0;
                            Game.Stations[s].OpenRightDoors = Data.Blocks[i].Stop[j].Direction < 0;
                        }
                        if (Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors) {
                            dx = -5.0;
                        } else if (!Game.Stations[s].OpenLeftDoors & Game.Stations[s].OpenRightDoors) {
                            dx = 5.0;
                        } else {
                            dx = 0.0;
                        }
                        Game.Stations[s].SoundOrigin.X = Position.X + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.X + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.X;
                        Game.Stations[s].SoundOrigin.Y = Position.Y + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Y + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Y;
                        Game.Stations[s].SoundOrigin.Z = Position.Z + dx * TrackManager.CurrentTrack.Elements[n].WorldSide.Z + dy * TrackManager.CurrentTrack.Elements[n].WorldUp.Z;
                    }
                }
                // limit
                for (int j = 0; j < Data.Blocks[i].Limit.Length; j++) {
                    int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                    Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                    double d = Data.Blocks[i].Limit[j].TrackPosition - StartingDistance;
                    TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.LimitChangeEvent(d, CurrentSpeedLimit, Data.Blocks[i].Limit[j].Speed);
                    CurrentSpeedLimit = Data.Blocks[i].Limit[j].Speed;
                }
                // marker
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Markers.Length; j++) {
                        if (Data.Markers[j].StartingPosition >= StartingDistance & Data.Markers[j].StartingPosition < EndingDistance) {
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            double d = Data.Markers[j].StartingPosition - StartingDistance;
                            int t = Data.Markers[j].Texture;
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerStartEvent(d, t);
                        }
                        if (Data.Markers[j].EndingPosition >= StartingDistance & Data.Markers[j].EndingPosition < EndingDistance) {
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            double d = Data.Markers[j].EndingPosition - StartingDistance;
                            int t = Data.Markers[j].Texture;
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.MarkerEndEvent(d, t);
                        }
                    }
                }
                // sound
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Blocks[i].Sound.Length; j++) {
                        if (Data.Blocks[i].Sound[j].Type == SoundType.TrainStatic | Data.Blocks[i].Sound[j].Type == SoundType.TrainDynamic) {
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            double d = Data.Blocks[i].Sound[j].TrackPosition - StartingDistance;
                            switch (Data.Blocks[i].Sound[j].Type) {
                                case SoundType.TrainStatic:
                                    TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundIndex, int.MinValue, int.MinValue, true, false, new World.Vector3D(0.0, 0.0, 0.0), 0.0);
                                    break;
                                case SoundType.TrainDynamic:
                                    TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.SoundEvent(d, Data.Blocks[i].Sound[j].SoundIndex, int.MinValue, int.MinValue, false, true, new World.Vector3D(0.0, 0.0, 0.0), Data.Blocks[i].Sound[j].Speed);
                                    break;
                            }
                        }
                    }
                }
                // turn
                if (Data.Blocks[i].Turn != 0.0) {
                    double ag = -Math.Atan(Data.Blocks[i].Turn);
                    double cosag = Math.Cos(ag);
                    double sinag = Math.Sin(ag);
                    World.Rotate(ref Direction, cosag, sinag);
                    World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldDirection, cosag, sinag);
                    World.RotatePlane(ref TrackManager.CurrentTrack.Elements[n].WorldSide, cosag, sinag);
                }
                // curves
                double a = 0.0;
                double c = Data.BlockInterval;
                double h = 0.0;
                if (WorldTrackElement.PlanarCurveRadius != 0.0 & Data.Blocks[i].Pitch != 0.0) {
                    double d = Data.BlockInterval;
                    double p = Data.Blocks[i].Pitch;
                    double r = WorldTrackElement.PlanarCurveRadius;
                    double s = d / Math.Sqrt(1.0 + p * p);
                    h = s * p;
                    double b = s / Math.Abs(r);
                    c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
                    a = 0.5 * (double)Math.Sign(r) * b;
                    World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
                } else if (WorldTrackElement.PlanarCurveRadius != 0.0) {
                    double d = Data.BlockInterval;
                    double r = WorldTrackElement.PlanarCurveRadius;
                    double b = d / Math.Abs(r);
                    c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
                    a = 0.5 * (double)Math.Sign(r) * b;
                    World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
                } else if (Data.Blocks[i].Pitch != 0.0) {
                    double p = Data.Blocks[i].Pitch;
                    double d = Data.BlockInterval;
                    c = d / Math.Sqrt(1.0 + p * p);
                    h = c * p;
                }
                double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
                double TrackPitch = Math.Atan(Data.Blocks[i].Pitch);
                World.Transformation GroundTransformation = new World.Transformation(TrackYaw, 0.0, 0.0);
                World.Transformation TrackTransformation = new World.Transformation(TrackYaw, TrackPitch, 0.0);
                World.Transformation NullTransformation = new World.Transformation(0.0, 0.0, 0.0);
                // ground
                if (!PreviewOnly) {
                    int cb = (int)Math.Floor((double)i + 0.001);
                    int ci = (cb % Data.Blocks[i].Cycle.Length + Data.Blocks[i].Cycle.Length) % Data.Blocks[i].Cycle.Length;
                    int gi = Data.Blocks[i].Cycle[ci];
                    if (gi >= 0 & gi < Data.Structure.Ground.Length) {
                        if (Data.Structure.Ground[gi] != null) {
                            ObjectManager.CreateObject(Data.Structure.Ground[Data.Blocks[i].Cycle[ci]], World.Vector3D.Add(Position, new World.Vector3D(0.0, -Data.Blocks[i].Height, 0.0)), GroundTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                        }
                    }
                }
                // ground-aligned free objects
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Blocks[i].GroundFreeObj.Length; j++) {
                        int sttype = Data.Blocks[i].GroundFreeObj[j].Type;
                        double d = Data.Blocks[i].GroundFreeObj[j].TrackPosition - StartingDistance;
                        double dx = Data.Blocks[i].GroundFreeObj[j].X;
                        double dy = Data.Blocks[i].GroundFreeObj[j].Y;
                        World.Vector3D wpos = World.Vector3D.Add(Position, new World.Vector3D(Direction.X * d + Direction.Y * dx, dy - Data.Blocks[i].Height, Direction.Y * d - Direction.X * dx));
                        double tpos = Data.Blocks[i].GroundFreeObj[j].TrackPosition;
                        ObjectManager.CreateObject(Data.Structure.FreeObj[sttype], wpos, GroundTransformation, new World.Transformation(Data.Blocks[i].GroundFreeObj[j].Yaw, Data.Blocks[i].GroundFreeObj[j].Pitch, Data.Blocks[i].GroundFreeObj[j].Roll), Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos);
                    }
                }
                // rail-aligned objects
                if (!PreviewOnly) {
                    for (int j = 0; j < Data.Blocks[i].Rail.Length; j++) {
                        if (j > 0 && !Data.Blocks[i].Rail[j].RailStart) continue;
                        // rail
                        World.Vector3D pos;
                        World.Transformation RailTransformation;
                        double planar, updown;
                        if (j == 0) {
                            // rail 0
                            pos = Position;
                            planar = 0.0;
                            updown = 0.0;
                        } else {
                            // rails 1-infinity
                            double x = Data.Blocks[i].Rail[j].RailStartX;
                            double y = Data.Blocks[i].Rail[j].RailStartY;
                            World.Vector3D offset = new World.Vector3D(Direction.Y * x, y, -Direction.X * x);
                            double dh;
                            if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j) {
                                double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
                                double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
                                planar = Math.Atan(dx / c);
                                dh = dy / c;
                            } else {
                                planar = 0.0;
                                dh = 0.0;
                            }
                            pos = World.Vector3D.Add(Position, offset);
                            updown = Math.Atan(dh);
                        }
                        RailTransformation = new World.Transformation(TrackTransformation, planar, updown, 0.0);
                        if (Data.Blocks[i].RailType[j] < Data.Structure.Rail.Length) {
                            if (Data.Structure.Rail[Data.Blocks[i].RailType[j]] != null) {
                                ObjectManager.CreateObject(Data.Structure.Rail[Data.Blocks[i].RailType[j]], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                            }
                        }
                        // points of interest
                        for (int k = 0; k < Data.Blocks[i].PointsOfInterest.Length; k++) {
                            if (Data.Blocks[i].PointsOfInterest[k].RailIndex == j) {
                                double d = Data.Blocks[i].PointsOfInterest[k].TrackPosition - StartingDistance;
                                double x = Data.Blocks[i].PointsOfInterest[k].X;
                                double y = Data.Blocks[i].PointsOfInterest[k].Y;
                                int m = Game.PointsOfInterest.Length;
                                Array.Resize<Game.PointOfInterest>(ref Game.PointsOfInterest, m + 1);
                                Game.PointsOfInterest[m].TrackPosition = Data.Blocks[i].PointsOfInterest[k].TrackPosition;
                                if (i < Data.Blocks.Length - 1 && Data.Blocks[i + 1].Rail.Length > j) {
                                    double dx = Data.Blocks[i + 1].Rail[j].RailEndX - Data.Blocks[i].Rail[j].RailStartX;
                                    double dy = Data.Blocks[i + 1].Rail[j].RailEndY - Data.Blocks[i].Rail[j].RailStartY;
                                    dx = Data.Blocks[i].Rail[j].RailStartX + d / Data.BlockInterval * dx;
                                    dy = Data.Blocks[i].Rail[j].RailStartY + d / Data.BlockInterval * dy;
                                    Game.PointsOfInterest[m].TrackOffset = new World.Vector3D(x + dx, y + dy, 0.0);
                                } else {
                                    double dx = Data.Blocks[i].Rail[j].RailStartX;
                                    double dy = Data.Blocks[i].Rail[j].RailStartY;
                                    Game.PointsOfInterest[m].TrackOffset = new World.Vector3D(x + dx, y + dy, 0.0);
                                }
                                Game.PointsOfInterest[m].TrackYaw = Data.Blocks[i].PointsOfInterest[k].Yaw + planar;
                                Game.PointsOfInterest[m].TrackPitch = Data.Blocks[i].PointsOfInterest[k].Pitch + updown;
                                Game.PointsOfInterest[m].TrackRoll = Data.Blocks[i].PointsOfInterest[k].Roll;
                                Game.PointsOfInterest[m].Text = Data.Blocks[i].PointsOfInterest[k].Text;
                            }
                        }
                        // poles
                        if (Data.Blocks[i].RailPole.Length > j && Data.Blocks[i].RailPole[j].Exists) {
                            double dz = StartingDistance / Data.Blocks[i].RailPole[j].Interval;
                            dz -= Math.Floor(dz + 0.5);
                            if (dz >= -0.01 & dz <= 0.01) {
                                if (Data.Blocks[i].RailPole[j].Mode == 0) {
                                    if (Data.Blocks[i].RailPole[j].Location <= 0.0) {
                                        ObjectManager.CreateObject(Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    } else {
                                        ObjectManager.UnifiedObject Pole = GetMirroredObject(Data.Structure.Poles[0][Data.Blocks[i].RailPole[j].Type]);
                                        ObjectManager.CreateObject(Pole, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                } else {
                                    int m = Data.Blocks[i].RailPole[j].Mode;
                                    double dx = -Data.Blocks[i].RailPole[j].Location * 3.8;
                                    double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
                                    double wx = Math.Cos(wa);
                                    double wy = Math.Tan(updown);
                                    double wz = Math.Sin(wa);
                                    World.Normalize(ref wx, ref wy, ref wz);
                                    double sx = Direction.Y;
                                    double sy = 0.0;
                                    double sz = -Direction.X;
                                    World.Vector3D wpos = World.Vector3D.Add(pos, new World.Vector3D(sx * dx + wx * dz, sy * dx + wy * dz, sz * dx + wz * dz));
                                    int type = Data.Blocks[i].RailPole[j].Type;
                                    ObjectManager.CreateObject(Data.Structure.Poles[m][type], wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                }
                            }
                        }
                        // walls
                        if (Data.Blocks[i].RailWall.Length > j && Data.Blocks[i].RailWall[j].Exists) {
                            if (Data.Blocks[i].RailWall[j].Direction <= 0) {
                                ObjectManager.CreateObject(Data.Structure.WallL[Data.Blocks[i].RailWall[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                            }
                            if (Data.Blocks[i].RailWall[j].Direction >= 0) {
                                ObjectManager.CreateObject(Data.Structure.WallR[Data.Blocks[i].RailWall[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                            }
                        }
                        // dikes
                        if (Data.Blocks[i].RailDike.Length > j && Data.Blocks[i].RailDike[j].Exists) {
                            if (Data.Blocks[i].RailDike[j].Direction <= 0) {
                                ObjectManager.CreateObject(Data.Structure.DikeL[Data.Blocks[i].RailDike[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                            }
                            if (Data.Blocks[i].RailDike[j].Direction >= 0) {
                                ObjectManager.CreateObject(Data.Structure.DikeR[Data.Blocks[i].RailDike[j].Type], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                            }
                        }
                        // sounds
                        if (j == 0) {
                            for (int k = 0; k < Data.Blocks[i].Sound.Length; k++) {
                                if (Data.Blocks[i].Sound[k].Type == SoundType.World) {
                                    if (Data.Blocks[i].Sound[k].SoundIndex >= 0) {
                                        double d = Data.Blocks[i].Sound[k].TrackPosition - StartingDistance;
                                        double dx = Data.Blocks[i].Sound[k].X;
                                        double dy = Data.Blocks[i].Sound[k].Y;
                                        double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
                                        double wx = Math.Cos(wa);
                                        double wy = Math.Tan(updown);
                                        double wz = Math.Sin(wa);
                                        World.Normalize(ref wx, ref wy, ref wz);
                                        double sx = Direction.Y;
                                        double sy = 0.0;
                                        double sz = -Direction.X;
                                        double ux, uy, uz;
                                        World.Cross(wx, wy, wz, sx, sy, sz, out ux, out uy, out uz);
                                        World.Vector3D wpos = World.Vector3D.Add(pos, new World.Vector3D(sx * dx + ux * dy + wx * d, sy * dx + uy * dy + wy * d, sz * dx + uz * dy + wz * d));
                                        SoundManager.PlaySound(Data.Blocks[i].Sound[k].SoundIndex, -1, -1, wpos, SoundManager.Importance.AlwaysPlay, true, 1.0, 1.0);
                                    }
                                }
                            }
                        }
                        // forms
                        for (int k = 0; k < Data.Blocks[i].Form.Length; k++) {
                            // primary rail
                            if (Data.Blocks[i].Form[k].PrimaryRail == j) {
                                if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailStub) {
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormL.Length || Data.Structure.FormL[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        if (Data.Blocks[i].Form[k].RoofType > 0) {
                                            if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofL.Length || Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType] == null) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                            } else {
                                                ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                            }
                                        }
                                    }
                                } else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailL) {
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormL.Length || Data.Structure.FormL[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormCL.Length || Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateStaticObject(Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].RoofType > 0) {
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofL.Length || Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofCL.Length || Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateStaticObject(Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    }
                                } else if (Data.Blocks[i].Form[k].SecondaryRail == Form.SecondaryRailR) {
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormR.Length || Data.Structure.FormR[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormCR.Length || Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateStaticObject(Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].RoofType > 0) {
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofR.Length || Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofCR.Length || Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateStaticObject(Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    }
                                } else if (Data.Blocks[i].Form[k].SecondaryRail > 0) {
                                    int p = Data.Blocks[i].Form[k].PrimaryRail;
                                    double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
                                    double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
                                    int s = Data.Blocks[i].Form[k].SecondaryRail;
                                    if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is out of range in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
                                    } else {
                                        double sx0 = Data.Blocks[i].Rail[s].RailStartX;
                                        double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
                                        double d0 = sx0 - px0;
                                        double d1 = sx1 - px1;
                                        if (d0 < 0.0) {
                                            if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormL.Length || Data.Structure.FormL[Data.Blocks[i].Form[k].FormType] == null) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                            } else {
                                                ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                            }
                                            if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormCL.Length || Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType] == null) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                            } else {
                                                ObjectManager.StaticObject FormC = GetTransformedStaticObject(Data.Structure.FormCL[Data.Blocks[i].Form[k].FormType], d0, d1);
                                                ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                            }
                                            if (Data.Blocks[i].Form[k].RoofType > 0) {
                                                if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofL.Length || Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                                } else {
                                                    ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                                }
                                                if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofCL.Length || Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofCL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                                } else {
                                                    ObjectManager.StaticObject RoofC = GetTransformedStaticObject(Data.Structure.RoofCL[Data.Blocks[i].Form[k].RoofType], d0, d1);
                                                    ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                                }
                                            }
                                        } else if (d0 > 0.0) {
                                            if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormR.Length || Data.Structure.FormR[Data.Blocks[i].Form[k].FormType] == null) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                            } else {
                                                ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                            }
                                            if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormCR.Length || Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType] == null) {
                                                Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                            } else {
                                                ObjectManager.StaticObject FormC = GetTransformedStaticObject(Data.Structure.FormCR[Data.Blocks[i].Form[k].FormType], d0, d1);
                                                ObjectManager.CreateStaticObject(FormC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                            }
                                            if (Data.Blocks[i].Form[k].RoofType > 0) {
                                                if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofR.Length || Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                                } else {
                                                    ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                                }
                                                if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofCR.Length || Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType] == null) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofCR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                                } else {
                                                    ObjectManager.StaticObject RoofC = GetTransformedStaticObject(Data.Structure.RoofCR[Data.Blocks[i].Form[k].RoofType], d0, d1);
                                                    ObjectManager.CreateStaticObject(RoofC, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            // secondary rail
                            if (Data.Blocks[i].Form[k].SecondaryRail == j) {
                                int p = Data.Blocks[i].Form[k].PrimaryRail;
                                double px = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
                                int s = Data.Blocks[i].Form[k].SecondaryRail;
                                double sx = Data.Blocks[i].Rail[s].RailStartX;
                                double d = px - sx;
                                if (d < 0.0) {
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormL.Length || Data.Structure.FormL[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateObject(Data.Structure.FormL[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].RoofType > 0) {
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofL.Length || Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofL not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateObject(Data.Structure.RoofL[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    }
                                } else {
                                    if (Data.Blocks[i].Form[k].FormType >= Data.Structure.FormR.Length || Data.Structure.FormR[Data.Blocks[i].Form[k].FormType] == null) {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "PfIdxStType references a FormR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                    } else {
                                        ObjectManager.CreateObject(Data.Structure.FormR[Data.Blocks[i].Form[k].FormType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                    }
                                    if (Data.Blocks[i].Form[k].RoofType > 0) {
                                        if (Data.Blocks[i].Form[k].RoofType >= Data.Structure.RoofR.Length || Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "RoofIdxStType references a RoofR not loaded in Track.Form at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.CreateObject(Data.Structure.RoofR[Data.Blocks[i].Form[k].RoofType], pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    }
                                }
                            }
                        }
                        // cracks
                        for (int k = 0; k < Data.Blocks[i].Crack.Length; k++) {
                            if (Data.Blocks[i].Crack[k].PrimaryRail == j) {
                                int p = Data.Blocks[i].Crack[k].PrimaryRail;
                                double px0 = p > 0 ? Data.Blocks[i].Rail[p].RailStartX : 0.0;
                                double px1 = p > 0 ? Data.Blocks[i + 1].Rail[p].RailEndX : 0.0;
                                int s = Data.Blocks[i].Crack[k].SecondaryRail;
                                if (s < 0 || s >= Data.Blocks[i].Rail.Length || !Data.Blocks[i].Rail[s].RailStart) {
                                    Interface.AddMessage(Interface.MessageType.Error, false, "Index2 is out of range in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName);
                                } else {
                                    double sx0 = Data.Blocks[i].Rail[s].RailStartX;
                                    double sx1 = Data.Blocks[i + 1].Rail[s].RailEndX;
                                    double d0 = sx0 - px0;
                                    double d1 = sx1 - px1;
                                    if (d0 < 0.0) {
                                        if (Data.Blocks[i].Crack[k].Type >= Data.Structure.CrackL.Length || Data.Structure.CrackL[Data.Blocks[i].Crack[k].Type] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references a CrackL not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.StaticObject Crack = GetTransformedStaticObject(Data.Structure.CrackL[Data.Blocks[i].Crack[k].Type], d0, d1);
                                            ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    } else if (d0 > 0.0) {
                                        if (Data.Blocks[i].Crack[k].Type >= Data.Structure.CrackR.Length || Data.Structure.CrackR[Data.Blocks[i].Crack[k].Type] == null) {
                                            Interface.AddMessage(Interface.MessageType.Error, false, "IdxStType references a CrackR not loaded in Track.Crack at track position " + StartingDistance.ToString(Culture) + " in file " + FileName + ".");
                                        } else {
                                            ObjectManager.StaticObject Crack = GetTransformedStaticObject(Data.Structure.CrackR[Data.Blocks[i].Crack[k].Type], d0, d1);
                                            ObjectManager.CreateStaticObject(Crack, pos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, StartingDistance);
                                        }
                                    }
                                }
                            }
                        }
                        // free objects
                        if (Data.Blocks[i].RailFreeObj.Length > j && Data.Blocks[i].RailFreeObj[j] != null) {
                            for (int k = 0; k < Data.Blocks[i].RailFreeObj[j].Length; k++) {
                                int sttype = Data.Blocks[i].RailFreeObj[j][k].Type;
                                double dx = Data.Blocks[i].RailFreeObj[j][k].X;
                                double dy = Data.Blocks[i].RailFreeObj[j][k].Y;
                                double dz = Data.Blocks[i].RailFreeObj[j][k].TrackPosition - StartingDistance;
                                World.Vector3D wpos = pos;
                                wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
                                wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
                                wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
                                double tpos = Data.Blocks[i].RailFreeObj[j][k].TrackPosition;
                                ObjectManager.CreateObject(Data.Structure.FreeObj[sttype], wpos, RailTransformation, new World.Transformation(Data.Blocks[i].RailFreeObj[j][k].Yaw, Data.Blocks[i].RailFreeObj[j][k].Pitch, Data.Blocks[i].RailFreeObj[j][k].Roll), Data.Blocks[i].RailFreeObj[j][k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, 1.0, false);
                            }
                        }
                        // transponder objects
                        if (j == 0) {
                            for (int k = 0; k < Data.Blocks[i].Transponder.Length; k++) {
                                ObjectManager.UnifiedObject obj = null;
                                if (Data.Blocks[i].Transponder[k].ShowDefaultObject) {
                                    switch (Data.Blocks[i].Transponder[k].Type) {
                                        case TrackManager.TransponderType.S: obj = TransponderS; break;
                                        case TrackManager.TransponderType.Sn: obj = TransponderSN; break;
                                        case TrackManager.TransponderType.AccidentalDeparture: obj = TransponderFalseStart; break;
                                        case TrackManager.TransponderType.AtsPPatternOrigin: obj = TransponderPOrigin; break;
                                        case TrackManager.TransponderType.AtsPImmediateStop: obj = TransponderPStop; break;
                                    }
                                } else {
                                    int b = Data.Blocks[i].Transponder[k].BeaconStructureIndex;
                                    if (b >= 0 & b < Data.Structure.Beacon.Length) {
                                        obj = Data.Structure.Beacon[b];
                                    }
                                }
                                if (obj != null) {
                                    double dz = Data.Blocks[i].Transponder[k].TrackPosition - StartingDistance;
                                    World.Vector3D wpos = pos;
                                    wpos.X += dz * RailTransformation.Z.X;
                                    wpos.Y += dz * RailTransformation.Z.Y;
                                    wpos.Z += dz * RailTransformation.Z.Z;
                                    double tpos = Data.Blocks[i].Transponder[k].TrackPosition;
                                    if (Data.Blocks[i].Transponder[k].ShowDefaultObject) {
                                        double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
                                        ObjectManager.CreateObject(obj, wpos, RailTransformation, NullTransformation, -1, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                    } else {
                                        ObjectManager.CreateObject(obj, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos);
                                    }
                                }
                            }
                        }
                        // sections/signals/transponders
                        if (j == 0) {
                            // signals
                            for (int k = 0; k < Data.Blocks[i].Signal.Length; k++) {
                                SignalData sd;
                                if (Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex >= 0) {
                                    sd = Data.CompatibilitySignalData[Data.Blocks[i].Signal[k].SignalCompatibilityObjectIndex];
                                } else {
                                    sd = Data.SignalData[Data.Blocks[i].Signal[k].SignalObjectIndex];
                                }
                                int m;
                                if (sd is AnimatedObjectSignalData) {
                                    // animated signal
                                    m = -1;
                                } else {
                                    // non-animated signal
                                    m = Game.Signals.Length;
                                    Array.Resize<Game.Signal>(ref Game.Signals, m + 1);
                                    Game.Signals[m].Name = Data.Blocks[i].Signal[k].Name;
                                    Game.Signals[m].Section = -1;
                                    if (sd is CompatibilitySignalData) {
                                        // compatibility signal
                                        CompatibilitySignalData csd = (CompatibilitySignalData)sd;
                                        Game.Signals[m].Aspects = new Game.SignalAspect[csd.Numbers.Length];
                                        for (int l = 0; l < csd.Numbers.Length; l++) {
                                            Game.Signals[m].Aspects[l].Number = csd.Numbers[l];
                                            Game.Signals[m].Aspects[l].Object = ObjectManager.CloneObject(csd.Objects[l]);
                                        }
                                        Game.Signals[m].CurrentAspect = -1;
                                    } else if (sd is Bve4SignalData) {
                                        // bve 4 signal
                                        Bve4SignalData bvesd = (Bve4SignalData)sd;
                                        int aspects = 0;
                                        for (int l = 0; l < bvesd.DaylightTextures.Length; l++) {
                                            if (bvesd.DaylightTextures[l] >= 0) aspects++;
                                        }
                                        Game.Signals[m].Aspects = new Game.SignalAspect[aspects];
                                        aspects = 0;
                                        for (int l = 0; l < bvesd.DaylightTextures.Length; l++) {
                                            if (bvesd.DaylightTextures[l] >= 0) {
                                                Game.Signals[m].Aspects[aspects].Number = l;
                                                if (l < bvesd.GlowTextures.Length && bvesd.GlowTextures[l] >= 0) {
                                                    ObjectManager.StaticObject oday = ObjectManager.CloneObject(bvesd.BaseObject, bvesd.DaylightTextures[l], -1);
                                                    ObjectManager.StaticObject oglow = ObjectManager.CloneObject(bvesd.GlowObject, bvesd.GlowTextures[l], -1);
                                                    ObjectManager.StaticObject osig = new ObjectManager.StaticObject();
                                                    osig.Meshes = new World.Mesh[oday.Meshes.Length + oglow.Meshes.Length];
                                                    for (int o = 0; o < oday.Meshes.Length; o++) {
                                                        osig.Meshes[o] = oday.Meshes[o];
                                                    }
                                                    for (int o = 0; o < oglow.Meshes.Length; o++) {
                                                        osig.Meshes[oday.Meshes.Length + o] = oglow.Meshes[o];
                                                        for (int p = 0; p < osig.Meshes[oday.Meshes.Length + o].Materials.Length; p++) {
                                                            osig.Meshes[oday.Meshes.Length + o].Materials[p].BlendMode = World.MeshMaterialBlendMode.Additive;
                                                            osig.Meshes[oday.Meshes.Length + o].Materials[p].GlowAttenuationData = World.GetGlowAttenuationData(200.0, World.GlowAttenuationMode.DivisionExponent4);
                                                        }
                                                    }
                                                    Game.Signals[m].Aspects[aspects].Object = osig;
                                                } else {
                                                    Game.Signals[m].Aspects[aspects].Object = ObjectManager.CloneObject(bvesd.BaseObject, bvesd.DaylightTextures[l], -1);
                                                }
                                                aspects++;
                                            }
                                        }
                                        Game.Signals[m].CurrentAspect = -1;
                                    }
                                    Data.Blocks[i].Signal[k].GameSignalIndex = m;
                                }
                                // objects
                                double dz = Data.Blocks[i].Signal[k].TrackPosition - StartingDistance;
                                if (Data.Blocks[i].Signal[k].ShowPost) {
                                    // post
                                    double dx = Data.Blocks[i].Signal[k].X;
                                    World.Vector3D wpos = pos;
                                    wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
                                    wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
                                    wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
                                    double tpos = Data.Blocks[i].Signal[k].TrackPosition;
                                    double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
                                    ObjectManager.CreateStaticObject(SignalPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                }
                                if (Data.Blocks[i].Signal[k].ShowObject) {
                                    // signal object
                                    double dx = Data.Blocks[i].Signal[k].X;
                                    double dy = Data.Blocks[i].Signal[k].Y;
                                    World.Vector3D wpos = pos;
                                    wpos.X += dx * RailTransformation.X.X + dy * RailTransformation.Y.X + dz * RailTransformation.Z.X;
                                    wpos.Y += dx * RailTransformation.X.Y + dy * RailTransformation.Y.Y + dz * RailTransformation.Z.Y;
                                    wpos.Z += dx * RailTransformation.X.Z + dy * RailTransformation.Y.Z + dz * RailTransformation.Z.Z;
                                    double tpos = Data.Blocks[i].Signal[k].TrackPosition;
                                    if (sd is AnimatedObjectSignalData) {
                                        AnimatedObjectSignalData aosd = (AnimatedObjectSignalData)sd;
                                        ObjectManager.CreateObject(aosd.Objects, wpos, RailTransformation, new World.Transformation(Data.Blocks[i].Signal[k].Yaw, Data.Blocks[i].Signal[k].Pitch, Data.Blocks[i].Signal[k].Roll), Data.Blocks[i].Signal[k].Section, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, 1.0, false);
                                    } else {
                                        double brightness = sd is CompatibilitySignalData ? 0.25 + 0.75 * GetBrightness(ref Data, tpos) : 1.0;
                                        int o; if (Game.Signals[m].Aspects.Length != 0) {
                                            o = ObjectManager.CreateStaticObject(Game.Signals[m].Aspects[0].Object, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, brightness, false);
                                        } else {
                                            o = -1;
                                        }
                                        Game.Signals[m].ObjectIndex = o;
                                        for (int l = 0; l < Game.Signals[m].Aspects.Length; l++) {
                                            ObjectManager.ApplyStaticObjectData(ref Game.Signals[m].Aspects[l].Object, ObjectManager.CloneObject(Game.Signals[m].Aspects[l].Object), wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, brightness, false);
                                        }
                                    }
                                } else if (!(sd is AnimatedObjectSignalData)) {
                                    Game.Signals[m].ObjectIndex = -1;
                                }
                            }
                            // sections
                            for (int k = 0; k < Data.Blocks[i].Section.Length; k++) {
                                int m = Game.Sections.Length;
                                Array.Resize<Game.Section>(ref Game.Sections, m + 1);
                                Game.Sections[m].SignalIndices = new int[] { };
                                // update associated signals
                                for (int g = 0; g <= i; g++) {
                                    for (int l = 0; l < Data.Blocks[g].Signal.Length; l++) {
                                        if (Data.Blocks[g].Signal[l].Section == m) {
                                            int s = Data.Blocks[g].Signal[l].GameSignalIndex;
                                            if (s >= 0) {
                                                Game.Signals[s].Section = m;
                                                int o = Game.Sections[m].SignalIndices.Length;
                                                Array.Resize<int>(ref Game.Sections[m].SignalIndices, o + 1);
                                                Game.Sections[m].SignalIndices[o] = s;
                                            }
                                            Data.Blocks[g].Signal[l].Section = -1;
                                        }
                                    }
                                }
                                // create associated transponders
                                for (int g = 0; g <= i; g++) {
                                    for (int l = 0; l < Data.Blocks[g].Transponder.Length; l++) {
                                        if (Data.Blocks[g].Transponder[l].Type != TrackManager.TransponderType.None & Data.Blocks[g].Transponder[l].Section == m) {
                                            int o = TrackManager.CurrentTrack.Elements[n - i + g].Events.Length;
                                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n - i + g].Events, o + 1);
                                            double dt = Data.Blocks[g].Transponder[l].TrackPosition - StartingDistance + (double)(i - g) * Data.BlockInterval;
                                            TrackManager.CurrentTrack.Elements[n - i + g].Events[o] = new TrackManager.TransponderEvent(dt, Data.Blocks[g].Transponder[l].Type, Data.Blocks[g].Transponder[l].SwitchSubsystem, Data.Blocks[g].Transponder[l].OptionalInteger, Data.Blocks[g].Transponder[l].OptionalFloat, m);
                                            Data.Blocks[g].Transponder[l].Type = TrackManager.TransponderType.None;
                                        }
                                    }
                                }
                                // create section
                                Game.Sections[m].TrackPosition = Data.Blocks[i].Section[k].TrackPosition;
                                Game.Sections[m].Aspects = new Game.SectionAspect[Data.Blocks[i].Section[k].Aspects.Length];
                                for (int l = 0; l < Data.Blocks[i].Section[k].Aspects.Length; l++) {
                                    Game.Sections[m].Aspects[l].Number = Data.Blocks[i].Section[k].Aspects[l];
                                    if (Data.Blocks[i].Section[k].Aspects[l] >= 0 & Data.Blocks[i].Section[k].Aspects[l] < Data.SignalSpeeds.Length) {
                                        Game.Sections[m].Aspects[l].Speed = Data.SignalSpeeds[Data.Blocks[i].Section[k].Aspects[l]];
                                    } else {
                                        Game.Sections[m].Aspects[l].Speed = double.PositiveInfinity;
                                    }
                                }
                                Game.Sections[m].Type = Data.Blocks[i].Section[k].Type;
                                Game.Sections[m].CurrentAspect = -1;
                                if (m > 0) {
                                    Game.Sections[m].PreviousSection = m - 1;
                                    Game.Sections[m - 1].NextSection = m;
                                } else {
                                    Game.Sections[m].PreviousSection = -1;
                                }
                                Game.Sections[m].NextSection = -1;
                                Game.Sections[m].StationIndex = Data.Blocks[i].Section[k].DepartureStationIndex;
                                Game.Sections[m].Invisible = Data.Blocks[i].Section[k].Invisible;
                                Game.Sections[m].TrainIndices = new int[] { };
                                // create section change event
                                double d = Data.Blocks[i].Section[k].TrackPosition - StartingDistance;
                                int p = TrackManager.CurrentTrack.Elements[n].Events.Length;
                                Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, p + 1);
                                TrackManager.CurrentTrack.Elements[n].Events[p] = new TrackManager.SectionChangeEvent(d, m - 1, m);
                            }
                            // signals introduced after corresponding sections
                            for (int k = 0; k < Data.Blocks[i].Signal.Length; k++) {
                                int t = Data.Blocks[i].Signal[k].Section;
                                if (t >= 0 & t < Game.Sections.Length) {
                                    int s = Data.Blocks[i].Signal[k].GameSignalIndex;
                                    if (s >= 0) {
                                        Game.Signals[s].Section = t;
                                        int m = Game.Sections[t].SignalIndices.Length;
                                        Array.Resize<int>(ref Game.Sections[t].SignalIndices, m + 1);
                                        Game.Sections[t].SignalIndices[m] = s;
                                    }
                                    Data.Blocks[i].Signal[k].Section = -1;
                                }
                            }
                            // transponders introduced after corresponding sections
                            for (int l = 0; l < Data.Blocks[i].Transponder.Length; l++) {
                                if (Data.Blocks[i].Transponder[l].Type != TrackManager.TransponderType.None) {
                                    int t = Data.Blocks[i].Transponder[l].Section;
                                    if (t >= 0 & t < Game.Sections.Length) {
                                        int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                                        Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                                        double dt = Data.Blocks[i].Transponder[l].TrackPosition - StartingDistance;
                                        TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(dt, Data.Blocks[i].Transponder[l].Type, Data.Blocks[i].Transponder[l].SwitchSubsystem, Data.Blocks[i].Transponder[l].OptionalInteger, Data.Blocks[i].Transponder[l].OptionalFloat, t);
                                        Data.Blocks[i].Transponder[l].Type = TrackManager.TransponderType.None;
                                    }
                                }
                            }
                        }
                        // limit
                        if (j == 0) {
                            for (int k = 0; k < Data.Blocks[i].Limit.Length; k++) {
                                if (Data.Blocks[i].Limit[k].Direction != 0) {
                                    double dx = 2.2 * (double)Data.Blocks[i].Limit[k].Direction;
                                    double dz = Data.Blocks[i].Limit[k].TrackPosition - StartingDistance;
                                    World.Vector3D wpos = pos;
                                    wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
                                    wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
                                    wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
                                    double tpos = Data.Blocks[i].Limit[k].TrackPosition;
                                    double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
                                    if (Data.Blocks[i].Limit[k].Speed <= 0.0 | Data.Blocks[i].Limit[k].Speed >= 1000.0) {
                                        ObjectManager.CreateStaticObject(LimitPostInfinite, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                    } else {
                                        if (Data.Blocks[i].Limit[k].Cource < 0) {
                                            ObjectManager.CreateStaticObject(LimitPostLeft, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                        } else if (Data.Blocks[i].Limit[k].Cource > 0) {
                                            ObjectManager.CreateStaticObject(LimitPostRight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                        } else {
                                            ObjectManager.CreateStaticObject(LimitPostStraight, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                        }
                                        double lim = Data.Blocks[i].Limit[k].Speed / Data.UnitOfSpeed;
                                        if (lim < 10.0) {
                                            int d0 = (int)Math.Round(lim);
                                            int o = ObjectManager.CreateStaticObject(LimitOneDigit, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, true);
                                            if (ObjectManager.Objects[o].Meshes.Length >= 1) {
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 1) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[0].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d0 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                            }
                                        } else if (lim < 100.0) {
                                            int d1 = (int)Math.Round(lim);
                                            int d0 = d1 % 10;
                                            d1 /= 10;
                                            int o = ObjectManager.CreateStaticObject(LimitTwoDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, true);
                                            if (ObjectManager.Objects[o].Meshes.Length >= 1) {
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 1) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[0].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d1 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 2) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[1].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d0 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                            }
                                        } else {
                                            int d2 = (int)Math.Round(lim);
                                            int d0 = d2 % 10;
                                            int d1 = (d2 / 10) % 10;
                                            d2 /= 100;
                                            int o = ObjectManager.CreateStaticObject(LimitThreeDigits, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, true);
                                            if (ObjectManager.Objects[o].Meshes.Length >= 1) {
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 1) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[0].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d2 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 2) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[1].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d1 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                                if (ObjectManager.Objects[o].Meshes[0].Materials.Length >= 3) {
                                                    ObjectManager.Objects[o].Meshes[0].Materials[2].DaytimeTextureIndex = TextureManager.RegisterTexture(Interface.GetCombinedFileName(LimitGraphicsPath, "limit_" + d0 + ".png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // stop
                        if (j == 0) {
                            for (int k = 0; k < Data.Blocks[i].Stop.Length; k++) {
                                if (Data.Blocks[i].Stop[k].Direction != 0) {
                                    double dx = 1.8 * (double)Data.Blocks[i].Stop[k].Direction;
                                    double dz = Data.Blocks[i].Stop[k].TrackPosition - StartingDistance;
                                    World.Vector3D wpos = pos;
                                    wpos.X += dx * RailTransformation.X.X + dz * RailTransformation.Z.X;
                                    wpos.Y += dx * RailTransformation.X.Y + dz * RailTransformation.Z.Y;
                                    wpos.Z += dx * RailTransformation.X.Z + dz * RailTransformation.Z.Z;
                                    double tpos = Data.Blocks[i].Stop[k].TrackPosition;
                                    double b = 0.25 + 0.75 * GetBrightness(ref Data, tpos);
                                    ObjectManager.CreateStaticObject(StopPost, wpos, RailTransformation, NullTransformation, Data.AccurateObjectDisposal, StartingDistance, EndingDistance, tpos, b, false);
                                }
                            }
                        }
                    }
                }
                // finalize block
                Position.X += Direction.X * c;
                Position.Y += h;
                Position.Z += Direction.Y * c;
                if (a != 0.0) {
                    World.Rotate(ref Direction, Math.Cos(-a), Math.Sin(-a));
                }
            }
            // orphaned transponders
            if (!PreviewOnly) {
                for (int i = Data.FirstUsedBlock; i < Data.Blocks.Length; i++) {
                    for (int j = 0; j < Data.Blocks[i].Transponder.Length; j++) {
                        if (Data.Blocks[i].Transponder[j].Type != TrackManager.TransponderType.None) {
                            int n = i - Data.FirstUsedBlock;
                            int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                            Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                            double d = Data.Blocks[i].Transponder[j].TrackPosition - TrackManager.CurrentTrack.Elements[n].StartingTrackPosition;
                            int s = Data.Blocks[i].Transponder[j].Section;
                            if (s >= 0) s = -1;
                            TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TransponderEvent(d, Data.Blocks[i].Transponder[j].Type, Data.Blocks[i].Transponder[j].SwitchSubsystem, Data.Blocks[i].Transponder[j].OptionalInteger, Data.Blocks[i].Transponder[j].OptionalFloat, s);
                            Data.Blocks[i].Transponder[j].Type = TrackManager.TransponderType.None;
                        }
                    }
                }
            }
            // insert station end events
            for (int i = 0; i < Game.Stations.Length; i++) {
                int j = Game.Stations[i].Stops.Length - 1;
                if (j >= 0) {
                    double p = Game.Stations[i].Stops[j].TrackPosition + Game.Stations[i].Stops[j].ForwardTolerance + Data.BlockInterval;
                    int k = (int)Math.Floor(p / (double)Data.BlockInterval) - Data.FirstUsedBlock;
                    if (k >= 0 & k < Data.Blocks.Length) {
                        double d = p - (double)(k + Data.FirstUsedBlock) * (double)Data.BlockInterval;
                        int m = TrackManager.CurrentTrack.Elements[k].Events.Length;
                        Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[k].Events, m + 1);
                        TrackManager.CurrentTrack.Elements[k].Events[m] = new TrackManager.StationEndEvent(d, i);
                    }
                }
            }
            // finalize
            Array.Resize<TrackManager.TrackElement>(ref TrackManager.CurrentTrack.Elements, CurrentTrackLength);
            if (Game.Stations.Length > 0) {
                Game.Stations[Game.Stations.Length - 1].IsTerminalStation = true;
            }
            if (TrackManager.CurrentTrack.Elements.Length != 0) {
                int n = TrackManager.CurrentTrack.Elements.Length - 1;
                int m = TrackManager.CurrentTrack.Elements[n].Events.Length;
                Array.Resize<TrackManager.GeneralEvent>(ref TrackManager.CurrentTrack.Elements[n].Events, m + 1);
                TrackManager.CurrentTrack.Elements[n].Events[m] = new TrackManager.TrackEndEvent(Data.BlockInterval);
            }
        }

    }
}