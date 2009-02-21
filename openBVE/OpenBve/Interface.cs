using System;
using Tao.Sdl;

namespace OpenBve {
    internal static class Interface {

        // messages
        internal enum MessageType {
            Information = 1,
            Warning = 2,
            Error = 3,
            Critical = 4
        }
        internal struct Message {
            internal MessageType Type;
            internal bool FileNotFound;
            internal string Text;
        }
        internal static Message[] Messages = new Message[] { };
        internal static int MessageCount = 0;
        internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
            /// <info>Warning messages are suppressed in openBVE</info>
            if (Type != MessageType.Warning) {
                if (MessageCount == 0) {
                    Messages = new Message[16];
                } else if (MessageCount >= Messages.Length) {
                    Array.Resize<Message>(ref Messages, Messages.Length << 1);
                }
                Messages[MessageCount].Type = Type;
                Messages[MessageCount].FileNotFound = FileNotFound;
                Messages[MessageCount].Text = Text;
                MessageCount++;
            }
        }
        internal static void ClearMessages() {
            Messages = new Message[] { };
            MessageCount = 0;
        }

        // ================================

        // options
        internal struct EncodingValue {
            internal int Codepage;
            internal string Value;
        }
        internal enum MotionBlurMode {
            None = 0,
            Low = 1,
            Medium = 2,
            High = 3
        }
        internal enum SoundRange {
            Low = 0,
            Medium = 1,
            High = 2
        }
        internal struct Options {
            internal string LanguageCode;
            internal bool FullscreenMode;
            internal int WindowWidth;
            internal int WindowHeight;
            internal int FullscreenWidth;
            internal int FullscreenHeight;
            internal int FullscreenBits;
            internal TextureManager.InterpolationMode Interpolation;
            internal Renderer.TransparencyMode TransparencyMode;
            internal int AnisotropicFilteringLevel;
            internal int AnisotropicFilteringMaximum;
            internal bool AlternativeLighting;
            internal int ViewingDistance;
            internal MotionBlurMode MotionBlur;
            internal bool Toppling;
            internal bool Collisions;
            internal bool Derailments;
            internal bool ShowDefaultExteriorObjects;
            internal bool BlackBox;
            internal bool UseJoysticks;
            internal double JoystickAxisThreshold;
            internal SoundRange SoundRange;
            internal int SoundNumber;
            internal string RouteFolder;
            internal string TrainFolder;
            internal string[] RecentlyUsedRoutes;
            internal string[] RecentlyUsedTrains;
            internal int RecentlyUsedLimit;
            internal EncodingValue[] RouteEncodings;
            internal EncodingValue[] TrainEncodings;
        }
        internal static Options CurrentOptions;
        internal static bool LoadOptions() {
            CurrentOptions.LanguageCode = "none";
            CurrentOptions.FullscreenMode = false;
            CurrentOptions.WindowWidth = 960;
            CurrentOptions.WindowHeight = 600;
            CurrentOptions.FullscreenWidth = 1024;
            CurrentOptions.FullscreenHeight = 768;
            CurrentOptions.FullscreenBits = 32;
            CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped;
            CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp;
            CurrentOptions.AnisotropicFilteringLevel = 0;
            CurrentOptions.AnisotropicFilteringMaximum = 0;
            CurrentOptions.AlternativeLighting = false;
            CurrentOptions.ViewingDistance = 600;
            CurrentOptions.MotionBlur = MotionBlurMode.None;
            CurrentOptions.Toppling = true;
            CurrentOptions.Collisions = true;
            CurrentOptions.Derailments = true;
            CurrentOptions.ShowDefaultExteriorObjects = false;
            CurrentOptions.BlackBox = false;
            CurrentOptions.UseJoysticks = true;
            CurrentOptions.JoystickAxisThreshold = 0.0;
            CurrentOptions.SoundRange = SoundRange.Low;
            CurrentOptions.SoundNumber = 16;
            CurrentOptions.RouteFolder = "";
            CurrentOptions.TrainFolder = "";
            CurrentOptions.RecentlyUsedRoutes = new string[] { };
            CurrentOptions.RecentlyUsedTrains = new string[] { };
            CurrentOptions.RecentlyUsedLimit = 10;
            CurrentOptions.RouteEncodings = new EncodingValue[] { };
            CurrentOptions.TrainEncodings = new EncodingValue[] { };
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
            string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
            string File = Interface.GetCombinedFileName(Folder, "settings.cfg");
            if (System.IO.File.Exists(File)) {
                string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
                string Section = "";
                for (int i = 0; i < Lines.Length; i++) {
                    Lines[i] = Lines[i].Trim();
                    if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
                        if (Lines[i].StartsWith("[", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.OrdinalIgnoreCase)) {
                            Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
                        } else {
                            int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
                            string Key, Value;
                            if (j >= 0) {
                                Key = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
                                Value = Lines[i].Substring(j + 1).TrimStart();
                            } else {
                                Key = "";
                                Value = Lines[i];
                            }
                            switch (Section) {
                                case "language":
                                    switch (Key) {
                                        case "code": Interface.CurrentOptions.LanguageCode = Value; break;
                                    } break;
                                case "display":
                                    switch (Key) {
                                        case "mode": Interface.CurrentOptions.FullscreenMode = string.Compare(Value, "fullscreen", StringComparison.OrdinalIgnoreCase) == 0; break;
                                        case "windowwidth": {
                                                int a = 960; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.WindowWidth = a;
                                            } break;
                                        case "windowheight": {
                                                int a = 600; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.WindowHeight = a;
                                            } break;
                                        case "fullscreenwidth": {
                                                int a = 1024; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out  a);
                                                Interface.CurrentOptions.FullscreenWidth = a;
                                            } break;
                                        case "fullscreenheight": {
                                                int a = 768; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out  a);
                                                Interface.CurrentOptions.FullscreenHeight = a;
                                            } break;
                                        case "fullscreenbits": {
                                                int a = 32; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out  a);
                                                Interface.CurrentOptions.FullscreenBits = a;
                                            } break;
                                    } break;
                                case "quality":
                                    switch (Key) {
                                        case "interpolation":
                                            switch (Value.ToLowerInvariant()) {
                                                case "nearestneighbor": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighbor; break;
                                                case "bilinear": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.Bilinear; break;
                                                case "nearestneighbormipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.NearestNeighborMipmapped; ; break;
                                                case "bilinearmipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped; break;
                                                case "trilinearmipmapped": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.TrilinearMipmapped; break;
                                                case "anisotropicfiltering": Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.AnisotropicFiltering; break;
                                                default: Interface.CurrentOptions.Interpolation = TextureManager.InterpolationMode.BilinearMipmapped; break;
                                            } break;
                                        case "anisotropicfilteringlevel": {
                                                int a = 0; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.AnisotropicFilteringLevel = a;
                                            } break;
                                        case "anisotropicfilteringmaximum": {
                                                int a = 0; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.AnisotropicFilteringMaximum = a;
                                            } break;
                                        case "transparencymode":
                                            switch (Value.ToLowerInvariant()) {
                                                case "sharp": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp; break;
                                                case "smooth": Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Smooth; break;
                                                default: Interface.CurrentOptions.TransparencyMode = Renderer.TransparencyMode.Sharp; break;
                                            } break;
                                        case "alternativelighting": {
                                                Interface.CurrentOptions.AlternativeLighting = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "viewingdistance": {
                                                int a = 0; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.ViewingDistance = a;
                                            } break;
                                        case "motionblur":
                                            switch (Value.ToLowerInvariant()) {
                                                case "low": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Low; break;
                                                case "medium": Interface.CurrentOptions.MotionBlur = MotionBlurMode.Medium; break;
                                                case "high": Interface.CurrentOptions.MotionBlur = MotionBlurMode.High; break;
                                                default: Interface.CurrentOptions.MotionBlur = MotionBlurMode.None; break;
                                            } break;
                                    } break;
                                case "simulation":
                                    switch (Key) {
                                        case "toppling": {
                                                Interface.CurrentOptions.Toppling = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "collisions": {
                                                Interface.CurrentOptions.Collisions = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "derailments": {
                                                Interface.CurrentOptions.Derailments = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "blackbox": {
                                                Interface.CurrentOptions.BlackBox = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "defaultexterior": {
                                                Interface.CurrentOptions.ShowDefaultExteriorObjects = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                    } break;
                                case "controls":
                                    switch (Key) {
                                        case "usejoysticks": {
                                                Interface.CurrentOptions.UseJoysticks = string.Compare(Value, "false", StringComparison.OrdinalIgnoreCase) != 0;
                                            } break;
                                        case "joystickaxisthreshold": {
                                                double a = 0.0; double.TryParse(Value, System.Globalization.NumberStyles.Float, Culture, out a);
                                                Interface.CurrentOptions.JoystickAxisThreshold = a;
                                            } break;
                                    } break;
                                case "sound":
                                    switch (Key) {
                                        case "range": {
                                                switch (Value.ToLowerInvariant()) {
                                                    case "low": Interface.CurrentOptions.SoundRange = SoundRange.Low; break;
                                                    case "medium": Interface.CurrentOptions.SoundRange = SoundRange.Medium; break;
                                                    case "high": Interface.CurrentOptions.SoundRange = SoundRange.High; break;
                                                    default: Interface.CurrentOptions.SoundRange = SoundRange.Low; break;
                                                }
                                            } break;
                                        case "number": {
                                                int a = 0; int.TryParse(Value, System.Globalization.NumberStyles.Integer, Culture, out a);
                                                Interface.CurrentOptions.SoundNumber = a < 16 ? 16 : a;
                                            } break;
                                    } break;
                                case "folders":
                                    switch (Key) {
                                        case "route": {
                                                Interface.CurrentOptions.RouteFolder = Value;
                                            } break;
                                        case "train": {
                                                Interface.CurrentOptions.TrainFolder = Value;
                                            } break;
                                    } break;
                                case "recentlyusedroutes": {
                                        int n = Interface.CurrentOptions.RecentlyUsedRoutes.Length;
                                        Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedRoutes, n + 1);
                                        Interface.CurrentOptions.RecentlyUsedRoutes[n] = Value;
                                    } break;
                                case "recentlyusedtrains": {
                                        int n = Interface.CurrentOptions.RecentlyUsedTrains.Length;
                                        Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedTrains, n + 1);
                                        Interface.CurrentOptions.RecentlyUsedTrains[n] = Value;
                                    } break;
                                case "routeencodings": {
                                        int a = System.Text.Encoding.UTF8.CodePage;
                                        int.TryParse(Key, System.Globalization.NumberStyles.Integer, Culture, out a);
                                        int n = Interface.CurrentOptions.RouteEncodings.Length;
                                        Array.Resize<EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, n + 1);
                                        Interface.CurrentOptions.RouteEncodings[n].Codepage = a;
                                        Interface.CurrentOptions.RouteEncodings[n].Value = Value;
                                    } break;
                                case "trainencodings": {
                                        int a = System.Text.Encoding.UTF8.CodePage;
                                        int.TryParse(Key, System.Globalization.NumberStyles.Integer, Culture, out a);
                                        int n = Interface.CurrentOptions.TrainEncodings.Length;
                                        Array.Resize<EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, n + 1);
                                        Interface.CurrentOptions.TrainEncodings[n].Codepage = a;
                                        Interface.CurrentOptions.TrainEncodings[n].Value = Value;
                                    } break;
                            }
                        }
                    }
                }
                return true;
            } else {
                return false;
            }
        }
        internal static void SaveOptions() {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            System.Text.StringBuilder Builder = new System.Text.StringBuilder();
            Builder.AppendLine("; Settings");
            Builder.AppendLine("; ========");
            Builder.AppendLine("; This file was automatically generated.");
            Builder.AppendLine("; Please do not modify directly.");
            Builder.AppendLine();
            Builder.AppendLine("[language]");
            Builder.AppendLine("code = " + CurrentOptions.LanguageCode);
            Builder.AppendLine();
            Builder.AppendLine("[display]");
            Builder.AppendLine("mode = " + (CurrentOptions.FullscreenMode ? "fullscreen" : "window"));
            Builder.AppendLine("windowwidth = " + CurrentOptions.WindowWidth.ToString(Culture));
            Builder.AppendLine("windowheight = " + CurrentOptions.WindowHeight.ToString(Culture));
            Builder.AppendLine("fullscreenwidth = " + CurrentOptions.FullscreenWidth.ToString(Culture));
            Builder.AppendLine("fullscreenheight = " + CurrentOptions.FullscreenHeight.ToString(Culture));
            Builder.AppendLine("fullscreenbits = " + CurrentOptions.FullscreenBits.ToString(Culture));
            Builder.AppendLine();
            Builder.AppendLine("[quality]");
            {
                string t; switch (CurrentOptions.Interpolation) {
                    case TextureManager.InterpolationMode.NearestNeighbor: t = "nearestneighbor"; break;
                    case TextureManager.InterpolationMode.Bilinear: t = "bilinear"; break;
                    case TextureManager.InterpolationMode.NearestNeighborMipmapped: t = "nearestneighbormipmapped"; break;
                    case TextureManager.InterpolationMode.BilinearMipmapped: t = "bilinearmipmapped"; break;
                    case TextureManager.InterpolationMode.TrilinearMipmapped: t = "trilinearmipmapped"; break;
                    case TextureManager.InterpolationMode.AnisotropicFiltering: t = "anisotropicfiltering"; break;
                    default: t = "bilinearmipmapped"; break;
                }
                Builder.AppendLine("interpolation = " + t);
            }
            Builder.AppendLine("anisotropicfilteringlevel = " + CurrentOptions.AnisotropicFilteringLevel.ToString(Culture));
            Builder.AppendLine("anisotropicfilteringmaximum = " + CurrentOptions.AnisotropicFilteringMaximum.ToString(Culture));
            {
                string t; switch (CurrentOptions.TransparencyMode) {
                    case Renderer.TransparencyMode.Sharp: t = "sharp"; break;
                    case Renderer.TransparencyMode.Smooth: t = "smooth"; break;
                    default: t = "sharp"; break;
                }
                Builder.AppendLine("transparencymode = " + t);
            }
            Builder.AppendLine("alternativelighting = " + (CurrentOptions.AlternativeLighting ? "true" : "false"));
            Builder.AppendLine("viewingdistance = " + CurrentOptions.ViewingDistance.ToString(Culture));
            {
                string t; switch (CurrentOptions.MotionBlur) {
                    case MotionBlurMode.Low: t = "low"; break;
                    case MotionBlurMode.Medium: t = "medium"; break;
                    case MotionBlurMode.High: t = "high"; break;
                    default: t = "none"; break;
                }
                Builder.AppendLine("motionblur = " + t);
            }
            Builder.AppendLine();
            Builder.AppendLine("[simulation]");
            Builder.AppendLine("toppling = " + (CurrentOptions.Toppling ? "true" : "false"));
            Builder.AppendLine("collisions = " + (CurrentOptions.Collisions ? "true" : "false"));
            Builder.AppendLine("derailments = " + (CurrentOptions.Derailments ? "true" : "false"));
            Builder.AppendLine("blackbox = " + (CurrentOptions.BlackBox ? "true" : "false"));
            Builder.AppendLine("defaultexterior = " + (CurrentOptions.ShowDefaultExteriorObjects ? "true" : "false"));
            Builder.AppendLine();
            Builder.AppendLine("[controls]");
            Builder.AppendLine("usejoysticks = " + (CurrentOptions.UseJoysticks ? "true" : "false"));
            Builder.AppendLine("joystickaxisthreshold = " + CurrentOptions.JoystickAxisThreshold.ToString(Culture));
            Builder.AppendLine();
            Builder.AppendLine("[sound]");
            Builder.Append("range = ");
            switch (CurrentOptions.SoundRange) {
                case SoundRange.Low: Builder.AppendLine("low"); break;
                case SoundRange.Medium: Builder.AppendLine("medium"); break;
                case SoundRange.High: Builder.AppendLine("high"); break;
                default: Builder.AppendLine("low"); break;
            }
            Builder.AppendLine("number = " + CurrentOptions.SoundNumber.ToString(Culture));
            Builder.AppendLine();
            Builder.AppendLine("[folders]");
            Builder.AppendLine("route = " + CurrentOptions.RouteFolder);
            Builder.AppendLine("train = " + CurrentOptions.TrainFolder);
            Builder.AppendLine();
            Builder.AppendLine("[recentlyusedroutes]");
            for (int i = 0; i < CurrentOptions.RecentlyUsedRoutes.Length; i++) {
                Builder.AppendLine(CurrentOptions.RecentlyUsedRoutes[i]);
            }
            Builder.AppendLine();
            Builder.AppendLine("[recentlyusedtrains]");
            for (int i = 0; i < CurrentOptions.RecentlyUsedTrains.Length; i++) {
                Builder.AppendLine(CurrentOptions.RecentlyUsedTrains[i]);
            }
            Builder.AppendLine();
            Builder.AppendLine("[routeencodings]");
            for (int i = 0; i < CurrentOptions.RouteEncodings.Length; i++) {
                Builder.AppendLine(CurrentOptions.RouteEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.RouteEncodings[i].Value);
            }
            Builder.AppendLine();
            Builder.AppendLine("[trainencodings]");
            for (int i = 0; i < CurrentOptions.TrainEncodings.Length; i++) {
                Builder.AppendLine(CurrentOptions.TrainEncodings[i].Codepage.ToString(Culture) + " = " + CurrentOptions.TrainEncodings[i].Value);
            }
            try {
                string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
                string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
                if (!System.IO.Directory.Exists(Folder))
                    System.IO.Directory.CreateDirectory(Folder);
                string File = Interface.GetCombinedFileName(Folder, "settings.cfg");
                System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
            } catch (Exception exp) { Console.Error.WriteLine(exp.Message); }
        }

        // ================================

        // load logs
        internal static void LoadLogs() {
            string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
            string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
            string File = Interface.GetCombinedFileName(Folder, "logs.bin");
            try {
                using (System.IO.FileStream Stream = new System.IO.FileStream(File, System.IO.FileMode.Open, System.IO.FileAccess.Read)) {
                    using (System.IO.BinaryReader Reader = new System.IO.BinaryReader(Stream, System.Text.Encoding.UTF8)) {
                        byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
                        const short Version = 1;
                        byte[] Data = Reader.ReadBytes(Identifier.Length);
                        for (int i = 0; i < Identifier.Length; i++) {
                            if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
                        }
                        short Number = Reader.ReadInt16();
                        if (Version != Number) throw new System.IO.InvalidDataException();
                        Game.LogRouteName = Reader.ReadString();
                        Game.LogTrainName = Reader.ReadString();
                        Game.LogDateTime = DateTime.FromBinary(Reader.ReadInt64());
                        Game.CurrentMode = (Game.GameMode)Reader.ReadInt16();
                        Game.BlackBoxEntryCount = Reader.ReadInt32();
                        Game.BlackBoxEntries = new Game.BlackBoxEntry[Game.BlackBoxEntryCount];
                        for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
                            Game.BlackBoxEntries[i].Time = Reader.ReadDouble();
                            Game.BlackBoxEntries[i].Position = Reader.ReadDouble();
                            Game.BlackBoxEntries[i].Speed = Reader.ReadSingle();
                            Game.BlackBoxEntries[i].Acceleration = Reader.ReadSingle();
                            Game.BlackBoxEntries[i].ReverserDriver = Reader.ReadInt16();
                            Game.BlackBoxEntries[i].ReverserSecurity = Reader.ReadInt16();
                            Game.BlackBoxEntries[i].PowerDriver = (Game.BlackBoxPower)Reader.ReadInt16();
                            Game.BlackBoxEntries[i].PowerSecurity = (Game.BlackBoxPower)Reader.ReadInt16();
                            Game.BlackBoxEntries[i].BrakeDriver = (Game.BlackBoxBrake)Reader.ReadInt16();
                            Game.BlackBoxEntries[i].BrakeSecurity = (Game.BlackBoxBrake)Reader.ReadInt16();
                            Game.BlackBoxEntries[i].EventToken = (Game.BlackBoxEventToken)Reader.ReadInt16();
                        }
                        Game.ScoreLogCount = Reader.ReadInt32();
                        Game.ScoreLogs = new Game.ScoreLog[Game.ScoreLogCount];
                        Game.CurrentScore.Value = 0;
                        for (int i = 0; i < Game.ScoreLogCount; i++) {
                            Game.ScoreLogs[i].Time = Reader.ReadDouble();
                            Game.ScoreLogs[i].Position = Reader.ReadDouble();
                            Game.ScoreLogs[i].Value = Reader.ReadInt32();
                            Game.ScoreLogs[i].TextToken = (Game.ScoreTextToken)Reader.ReadInt16();
                            Game.CurrentScore.Value += Game.ScoreLogs[i].Value;
                        }
                        Game.CurrentScore.Maximum = Reader.ReadInt32();
                        Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
                        Data = Reader.ReadBytes(Identifier.Length);
                        for (int i = 0; i < Identifier.Length; i++) {
                            if (Identifier[i] != Data[i]) throw new System.IO.InvalidDataException();
                        }
                        Reader.Close();
                    } Stream.Close();
                }
            } catch {
                Game.LogRouteName = "";
                Game.LogTrainName = "";
                Game.LogDateTime = DateTime.Now;
                Game.BlackBoxEntries = new Game.BlackBoxEntry[256];
                Game.BlackBoxEntryCount = 0;
                Game.ScoreLogs = new Game.ScoreLog[64];
                Game.ScoreLogCount = 0;
            }
        }

        // save logs
        internal static void SaveLogs() {
            string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
            string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
            if (!System.IO.Directory.Exists(Folder))
                System.IO.Directory.CreateDirectory(Folder);
            string File = Interface.GetCombinedFileName(Folder, "logs.bin");
            using (System.IO.FileStream Stream = new System.IO.FileStream(File, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
                using (System.IO.BinaryWriter Writer = new System.IO.BinaryWriter(Stream, System.Text.Encoding.UTF8)) {
                    byte[] Identifier = new byte[] { 111, 112, 101, 110, 66, 86, 69, 95, 76, 79, 71, 83 };
                    const short Version = 1;
                    Writer.Write(Identifier);
                    Writer.Write(Version);
                    Writer.Write(Game.LogRouteName);
                    Writer.Write(Game.LogTrainName);
                    Writer.Write(Game.LogDateTime.ToBinary());
                    Writer.Write((short)Game.CurrentMode);
                    Writer.Write(Game.BlackBoxEntryCount);
                    for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
                        Writer.Write(Game.BlackBoxEntries[i].Time);
                        Writer.Write(Game.BlackBoxEntries[i].Position);
                        Writer.Write(Game.BlackBoxEntries[i].Speed);
                        Writer.Write(Game.BlackBoxEntries[i].Acceleration);
                        Writer.Write(Game.BlackBoxEntries[i].ReverserDriver);
                        Writer.Write(Game.BlackBoxEntries[i].ReverserSecurity);
                        Writer.Write((short)Game.BlackBoxEntries[i].PowerDriver);
                        Writer.Write((short)Game.BlackBoxEntries[i].PowerSecurity);
                        Writer.Write((short)Game.BlackBoxEntries[i].BrakeDriver);
                        Writer.Write((short)Game.BlackBoxEntries[i].BrakeSecurity);
                        Writer.Write((short)Game.BlackBoxEntries[i].EventToken);
                    }
                    Writer.Write(Game.ScoreLogCount);
                    for (int i = 0; i < Game.ScoreLogCount; i++) {
                        Writer.Write(Game.ScoreLogs[i].Time);
                        Writer.Write(Game.ScoreLogs[i].Position);
                        Writer.Write(Game.ScoreLogs[i].Value);
                        Writer.Write((short)Game.ScoreLogs[i].TextToken);
                    }
                    Writer.Write(Game.CurrentScore.Maximum);
                    Identifier = new byte[] { 95, 102, 105, 108, 101, 69, 78, 68 };
                    Writer.Write(Identifier);
                    Writer.Close();
                } Stream.Close();
            }
        }

        // get score text
        internal static string GetScoreText(Game.ScoreTextToken TextToken) {
            switch (TextToken) {
                case Game.ScoreTextToken.Overspeed: return GetInterfaceString("score_overspeed");
                case Game.ScoreTextToken.PassedRedSignal: return GetInterfaceString("score_redsignal");
                case Game.ScoreTextToken.Toppling: return GetInterfaceString("score_toppling");
                case Game.ScoreTextToken.Derailed: return GetInterfaceString("score_derailed");
                case Game.ScoreTextToken.PassengerDiscomfort: return GetInterfaceString("score_discomfort");
                case Game.ScoreTextToken.DoorsOpened: return GetInterfaceString("score_doors");
                case Game.ScoreTextToken.ArrivedAtStation: return GetInterfaceString("score_station_arrived");
                case Game.ScoreTextToken.PerfectTimeBonus: return GetInterfaceString("score_station_perfecttime");
                case Game.ScoreTextToken.Late: return GetInterfaceString("score_station_late");
                case Game.ScoreTextToken.PerfectStopBonus: return GetInterfaceString("score_station_perfectstop");
                case Game.ScoreTextToken.Stop: return GetInterfaceString("score_station_stop");
                case Game.ScoreTextToken.PrematureDeparture: return GetInterfaceString("score_station_departure");
                case Game.ScoreTextToken.Total: return GetInterfaceString("score_station_total");
                default: return "?";
            }
        }

        // get black box text
        internal static string GetBlackBoxText(Game.BlackBoxEventToken EventToken) {
            switch (EventToken) {
                default: return "";
            }
        }

        // export score
        internal static void ExportScore(string File) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            System.Text.StringBuilder Builder = new System.Text.StringBuilder();
            string[][] Lines = new string[Game.ScoreLogCount + 1][];
            Lines[0] = new string[] {
                GetInterfaceString("log_time"),
                GetInterfaceString("log_position"),
                GetInterfaceString("log_value"),
                GetInterfaceString("log_cumulative"),
                GetInterfaceString("log_reason")
            };
            int Columns = Lines[0].Length;
            int TotalScore = 0;
            for (int i = 0; i < Game.ScoreLogCount; i++) {
                int j = i + 1;
                Lines[j] = new string[Columns];
                {
                    double x = Game.ScoreLogs[i].Time;
                    int h = (int)Math.Floor(x / 3600.0);
                    x -= (double)h * 3600.0;
                    int m = (int)Math.Floor(x / 60.0);
                    x -= (double)m * 60.0;
                    int s = (int)Math.Floor(x);
                    Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture);
                }
                Lines[j][1] = Game.ScoreLogs[i].Position.ToString("0", Culture);
                Lines[j][2] = Game.ScoreLogs[i].Value.ToString(Culture);
                TotalScore += Game.ScoreLogs[i].Value;
                Lines[j][3] = TotalScore.ToString(Culture);
                Lines[j][4] = GetScoreText(Game.ScoreLogs[i].TextToken);
            }
            int[] Widths = new int[Columns];
            for (int i = 0; i < Lines.Length; i++) {
                for (int j = 0; j < Columns; j++) {
                    if (Lines[i][j].Length > Widths[j]) {
                        Widths[j] = Lines[i][j].Length;
                    }
                }
            }
            { /// header rows
                int TotalWidth = 0;
                for (int j = 0; j < Columns; j++) {
                    TotalWidth += Widths[j] + 2;
                }
                TotalWidth += Columns - 1;
                Builder.Append('╔');
                Builder.Append('═', TotalWidth);
                Builder.Append("╗\n");
                {
                    Builder.Append('║');
                    Builder.Append((" " + GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
                    Builder.Append("║\n║");
                    Builder.Append((" " + GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
                    Builder.Append("║\n║");
                    Builder.Append((" " + GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
                    Builder.Append("║\n");
                }
                Builder.Append('╠');
                Builder.Append('═', TotalWidth);
                Builder.Append("╣\n");
                {
                    double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
                    if (ratio < 0.0) ratio = 0.0;
                    if (ratio > 1.0) ratio = 1.0;
                    int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
                    if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
                    string s;
                    switch (Game.CurrentMode) {
                        case Game.GameMode.Arcade: s = GetInterfaceString("mode_arcade"); break;
                        case Game.GameMode.Normal: s = GetInterfaceString("mode_normal"); break;
                        case Game.GameMode.Expert: s = GetInterfaceString("mode_expert"); break;
                        default: s = GetInterfaceString("mode_unknown"); break;
                    }
                    Builder.Append('║');
                    Builder.Append((" " + GetInterfaceString("log_mode") + " " + s).PadRight(TotalWidth, ' '));
                    Builder.Append("║\n║");
                    Builder.Append((" " + GetInterfaceString("log_score") + " " + Game.CurrentScore.Value.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture)).PadRight(TotalWidth, ' '));
                    Builder.Append("║\n║");
                    Builder.Append((" " + GetInterfaceString("log_rating") + " " + GetInterfaceString("rating_" + index.ToString(Culture)) + " (" + (100.0 * ratio).ToString("0.00") + "%)").PadRight(TotalWidth, ' '));
                    Builder.Append("║\n");
                }
            }
            { /// top border row
                Builder.Append('╠');
                for (int j = 0; j < Columns; j++) {
                    if (j != 0) {
                        Builder.Append('╤');
                    } Builder.Append('═', Widths[j] + 2);
                } Builder.Append("╣\n");
            }
            for (int i = 0; i < Lines.Length; i++) {
                /// center border row
                if (i != 0) {
                    Builder.Append('╟');
                    for (int j = 0; j < Columns; j++) {
                        if (j != 0) {
                            Builder.Append('┼');
                        } Builder.Append('─', Widths[j] + 2);
                    } Builder.Append("╢\n");
                }
                /// cell content
                Builder.Append('║');
                for (int j = 0; j < Columns; j++) {
                    if (j != 0) Builder.Append('│');
                    Builder.Append(' ');
                    if (i != 0 & j <= 3) {
                        Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
                    } else {
                        Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
                    }
                    Builder.Append(' ');
                } Builder.Append("║\n");
            }
            { /// bottom border row
                Builder.Append('╚');
                for (int j = 0; j < Columns; j++) {
                    if (j != 0) {
                        Builder.Append('╧');
                    } Builder.Append('═', Widths[j] + 2);
                } Builder.Append('╝');
            }
            System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
        }

        // export black box
        internal enum BlackBoxFormat {
            CommaSeparatedValue = 0,
            FormattedText = 1
        }
        internal static void ExportBlackBox(string File, BlackBoxFormat Format) {
            switch (Format) {
                // comma separated value
                case BlackBoxFormat.CommaSeparatedValue: {
                        System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
                        System.Text.StringBuilder Builder = new System.Text.StringBuilder();
                        for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
                            Builder.Append(Game.BlackBoxEntries[i].Time.ToString(Culture) + ",");
                            Builder.Append(Game.BlackBoxEntries[i].Position.ToString(Culture) + ",");
                            Builder.Append(Game.BlackBoxEntries[i].Speed.ToString(Culture) + ",");
                            Builder.Append(Game.BlackBoxEntries[i].Acceleration.ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].ReverserDriver).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].ReverserSecurity).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].PowerDriver).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].PowerSecurity).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].BrakeDriver).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].BrakeSecurity).ToString(Culture) + ",");
                            Builder.Append(((short)Game.BlackBoxEntries[i].EventToken).ToString(Culture));
                            Builder.Append("\r\n");
                        }
                        System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
                    } break;
                // formatted text
                case BlackBoxFormat.FormattedText: {
                        System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
                        System.Text.StringBuilder Builder = new System.Text.StringBuilder();
                        string[][] Lines = new string[Game.BlackBoxEntryCount + 1][];
                        Lines[0] = new string[] {
                            GetInterfaceString("log_time"),
                            GetInterfaceString("log_position"),
                            GetInterfaceString("log_speed"),
                            GetInterfaceString("log_acceleration"),
                            GetInterfaceString("log_reverser"),
                            GetInterfaceString("log_power"),
                            GetInterfaceString("log_brake"),
                            GetInterfaceString("log_event"),
                        };
                        int Columns = Lines[0].Length;
                        for (int i = 0; i < Game.BlackBoxEntryCount; i++) {
                            int j = i + 1;
                            Lines[j] = new string[Columns];
                            {
                                double x = Game.BlackBoxEntries[i].Time;
                                int h = (int)Math.Floor(x / 3600.0);
                                x -= (double)h * 3600.0;
                                int m = (int)Math.Floor(x / 60.0);
                                x -= (double)m * 60.0;
                                int s = (int)Math.Floor(x);
                                x -= (double)s;
                                int n = (int)Math.Floor(1000.0 * x);
                                Lines[j][0] = h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture) + ":" + n.ToString("000", Culture);
                            }
                            Lines[j][1] = Game.BlackBoxEntries[i].Position.ToString("0.000", Culture);
                            Lines[j][2] = Game.BlackBoxEntries[i].Speed.ToString("0.0000", Culture);
                            Lines[j][3] = Game.BlackBoxEntries[i].Acceleration.ToString("0.0000", Culture);
                            {
                                string[] reverser = new string[2];
                                for (int k = 0; k < 2; k++) {
                                    short r = k == 0 ? Game.BlackBoxEntries[i].ReverserDriver : Game.BlackBoxEntries[i].ReverserSecurity;
                                    switch (r) {
                                        case -1:
                                            reverser[k] = QuickReferences.HandleBackward;
                                            break;
                                        case 0:
                                            reverser[k] = QuickReferences.HandleNeutral;
                                            break;
                                        case 1:
                                            reverser[k] = QuickReferences.HandleForward;
                                            break;
                                        default:
                                            reverser[k] = r.ToString(Culture);
                                            break;
                                    }
                                }
                                Lines[j][4] = reverser[0] + " → " + reverser[1];
                            }
                            {
                                string[] power = new string[2];
                                for (int k = 0; k < 2; k++) {
                                    Game.BlackBoxPower p = k == 0 ? Game.BlackBoxEntries[i].PowerDriver : Game.BlackBoxEntries[i].PowerSecurity;
                                    switch (p) {
                                        case Game.BlackBoxPower.PowerNull:
                                            power[k] = GetInterfaceString(QuickReferences.HandlePowerNull);
                                            break;
                                        default:
                                            power[k] = GetInterfaceString(QuickReferences.HandlePower) + ((short)p).ToString(Culture);
                                            break;
                                    }
                                }
                                Lines[j][5] = power[0] + " → " + power[1];
                            }
                            {
                                string[] brake = new string[2];
                                for (int k = 0; k < 2; k++) {
                                    Game.BlackBoxBrake b = k == 0 ? Game.BlackBoxEntries[i].BrakeDriver : Game.BlackBoxEntries[i].BrakeSecurity;
                                    switch (b) {
                                        case Game.BlackBoxBrake.BrakeNull:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleBrakeNull);
                                            break;
                                        case Game.BlackBoxBrake.Emergency:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleEmergency);
                                            break;
                                        case Game.BlackBoxBrake.HoldBrake:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleHoldBrake);
                                            break;
                                        case Game.BlackBoxBrake.Release:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleRelease);
                                            break;
                                        case Game.BlackBoxBrake.Lap:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleLap);
                                            break;
                                        case Game.BlackBoxBrake.Service:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleService);
                                            break;
                                        default:
                                            brake[k] = GetInterfaceString(QuickReferences.HandleBrake) + ((short)b).ToString(Culture);
                                            break;
                                    }
                                }
                                Lines[j][6] = brake[0] + " → " + brake[1];
                            }
                            Lines[j][7] = GetBlackBoxText(Game.BlackBoxEntries[i].EventToken);
                        }
                        int[] Widths = new int[Columns];
                        for (int i = 0; i < Lines.Length; i++) {
                            for (int j = 0; j < Columns; j++) {
                                if (Lines[i][j].Length > Widths[j]) {
                                    Widths[j] = Lines[i][j].Length;
                                }
                            }
                        }
                        { /// header rows
                            int TotalWidth = 0;
                            for (int j = 0; j < Columns; j++) {
                                TotalWidth += Widths[j] + 2;
                            }
                            TotalWidth += Columns - 1;
                            Builder.Append('╔');
                            Builder.Append('═', TotalWidth);
                            Builder.Append("╗\r\n");
                            {
                                Builder.Append('║');
                                Builder.Append((" " + GetInterfaceString("log_route") + " " + Game.LogRouteName).PadRight(TotalWidth, ' '));
                                Builder.Append("║\r\n║");
                                Builder.Append((" " + GetInterfaceString("log_train") + " " + Game.LogTrainName).PadRight(TotalWidth, ' '));
                                Builder.Append("║\r\n║");
                                Builder.Append((" " + GetInterfaceString("log_date") + " " + Game.LogDateTime.ToString("yyyy-MM-dd HH:mm:ss", Culture)).PadRight(TotalWidth, ' '));
                                Builder.Append("║\r\n");
                            }
                        }
                        { /// top border row
                            Builder.Append('╠');
                            for (int j = 0; j < Columns; j++) {
                                if (j != 0) {
                                    Builder.Append('╤');
                                } Builder.Append('═', Widths[j] + 2);
                            } Builder.Append("╣\r\n");
                        }
                        for (int i = 0; i < Lines.Length; i++) {
                            /// center border row
                            if (i != 0) {
                                Builder.Append('╟');
                                for (int j = 0; j < Columns; j++) {
                                    if (j != 0) {
                                        Builder.Append('┼');
                                    } Builder.Append('─', Widths[j] + 2);
                                } Builder.Append("╢\r\n");
                            }
                            /// cell content
                            Builder.Append('║');
                            for (int j = 0; j < Columns; j++) {
                                if (j != 0) Builder.Append('│');
                                Builder.Append(' ');
                                if (i != 0 & j <= 3) {
                                    Builder.Append(Lines[i][j].PadLeft(Widths[j], ' '));
                                } else {
                                    Builder.Append(Lines[i][j].PadRight(Widths[j], ' '));
                                }
                                Builder.Append(' ');
                            } Builder.Append("║\r\n");
                        }
                        { /// bottom border row
                            Builder.Append('╚');
                            for (int j = 0; j < Columns; j++) {
                                if (j != 0) {
                                    Builder.Append('╧');
                                } Builder.Append('═', Widths[j] + 2);
                            } Builder.Append('╝');
                        }
                        System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
                    } break;
            }
        }

        // ================================

        // interface strings
        private struct InterfaceString {
            internal string Name;
            internal string Text;
        }
        private static InterfaceString[] InterfaceStrings = new InterfaceString[16];
        private static int InterfaceStringCount = 0;
        private static int CurrentInterfaceStringIndex = 0;
        private static void AddInterfaceString(string Name, string Text) {
            if (InterfaceStringCount >= InterfaceStrings.Length) {
                Array.Resize<InterfaceString>(ref InterfaceStrings, InterfaceStrings.Length << 1);
            }
            InterfaceStrings[InterfaceStringCount].Name = Name;
            InterfaceStrings[InterfaceStringCount].Text = Text;
            InterfaceStringCount++;
        }
        internal static string GetInterfaceString(string Name) {
            int n = Name.Length;
            for (int k = 0; k < InterfaceStringCount; k++) {
                int i;
                if ((k & 1) == 0) {
                    i = (CurrentInterfaceStringIndex + (k >> 1) + InterfaceStringCount) % InterfaceStringCount;
                } else {
                    i = (CurrentInterfaceStringIndex - (k + 1 >> 1) + InterfaceStringCount) % InterfaceStringCount;
                }
                if (InterfaceStrings[i].Name.Length == n) {
                    if (InterfaceStrings[i].Name == Name) {
                        CurrentInterfaceStringIndex = (i + 1) % InterfaceStringCount;
                        return InterfaceStrings[i].Text;
                    }
                }
            } return Name;
        }
        internal struct InterfaceQuickReference {

            // ### TO BE REMOVED SOON
            internal string LampAts;
            internal string LampAtsOperation;
            internal string LampAtsPPower;
            internal string LampAtsPPattern;
            internal string LampAtsPBrakeOverride;
            internal string LampAtsPBrakeOperation;
            internal string LampAtsP;
            internal string LampAtsPFailure;
            internal string LampAtc;
            internal string LampAtcPower;
            internal string LampAtcUse;
            internal string LampAtcEmergency;
            internal string LampEb;
            internal string LampConstSpeed;

            internal string HandleForward;
            internal string HandleNeutral;
            internal string HandleBackward;
            internal string HandlePower;
            internal string HandlePowerNull;
            internal string HandleBrake;
            internal string HandleBrakeNull;
            internal string HandleRelease;
            internal string HandleLap;
            internal string HandleService;
            internal string HandleEmergency;
            internal string HandleHoldBrake;
            internal string DoorsLeft;
            internal string DoorsRight;
            internal string Score;
        }
        internal static InterfaceQuickReference QuickReferences;
        internal static int RatingsCount = 10;

        // load language
        internal static void LoadLanguage(string FileName) {
            string[] Lines = System.IO.File.ReadAllLines(FileName, new System.Text.UTF8Encoding());
            string Section = "";
            InterfaceStrings = new InterfaceString[16];
            InterfaceStringCount = 0;
            Renderer.FileLamp = "lamp_80.png";
            Renderer.FileReverser = "handle_32.png";
            Renderer.FilePower = "handle_32.png";
            Renderer.FileBrake = "handle_32.png";
            Renderer.FileDoors = "handle_32.png";
            Renderer.FilePause = "pause.png";
            Renderer.FileDriver = "driver.png";

            // ### TO BE REMOVED SOON
            QuickReferences.LampAts = "ATS";
            QuickReferences.LampAtsOperation = "ATS 作動";
            QuickReferences.LampAtsPPower = "P 電源";
            QuickReferences.LampAtsPPattern = "パターン接近";
            QuickReferences.LampAtsPBrakeOverride = "ブレーキ開放";
            QuickReferences.LampAtsPBrakeOperation = "ブレーキ動作";
            QuickReferences.LampAtsP = "ATS-P";
            QuickReferences.LampAtsPFailure = "故障";
            QuickReferences.LampAtc = "ATC";
            QuickReferences.LampAtcPower = "ATC 電源";
            QuickReferences.LampAtcUse = "ATC 常用";
            QuickReferences.LampAtcEmergency = "ATC 非常";
            QuickReferences.LampEb = "EB";
            QuickReferences.LampConstSpeed = "定速";

            QuickReferences.HandleForward = "F";
            QuickReferences.HandleNeutral = "N";
            QuickReferences.HandleBackward = "B";
            QuickReferences.HandlePower = "P";
            QuickReferences.HandlePowerNull = "N";
            QuickReferences.HandleBrake = "B";
            QuickReferences.HandleBrakeNull = "N";
            QuickReferences.HandleRelease = "RL";
            QuickReferences.HandleLap = "LP";
            QuickReferences.HandleService = "SV";
            QuickReferences.HandleEmergency = "EM";
            QuickReferences.HandleHoldBrake = "HB";
            QuickReferences.DoorsLeft = "L";
            QuickReferences.DoorsRight = "R";
            QuickReferences.Score = "Score: ";
            for (int i = 0; i < Lines.Length; i++) {
                Lines[i] = Lines[i].Trim();
                if (!Lines[i].StartsWith(";")) {
                    if (Lines[i].StartsWith("[", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.OrdinalIgnoreCase)) {
                        Section = Lines[i].Substring(1, Lines[i].Length - 2).Trim().ToLowerInvariant();
                    } else {
                        int j = Lines[i].IndexOf('=');
                        if (j >= 0) {
                            string a = Lines[i].Substring(0, j).TrimEnd().ToLowerInvariant();
                            string b = Interface.Unescape(Lines[i].Substring(j + 1).TrimStart());
                            switch (Section) {

                                // ### TO BE REMOVED SOON
                                case "lamps":
                                    switch (a) {
                                        case "file": Renderer.FileLamp = b; break;
                                        case "ats": Interface.QuickReferences.LampAts = b; break;
                                        case "atsoperation": Interface.QuickReferences.LampAtsOperation = b; break;
                                        case "atsppower": Interface.QuickReferences.LampAtsPPower = b; break;
                                        case "atsppattern": Interface.QuickReferences.LampAtsPPattern = b; break;
                                        case "atspbrakeoverride": Interface.QuickReferences.LampAtsPBrakeOverride = b; break;
                                        case "atspbrakeoperation": Interface.QuickReferences.LampAtsPBrakeOperation = b; break;
                                        case "atsp": Interface.QuickReferences.LampAtsP = b; break;
                                        case "atspfailure": Interface.QuickReferences.LampAtsPFailure = b; break;
                                        case "atc": Interface.QuickReferences.LampAtc = b; break;
                                        case "atcpower": Interface.QuickReferences.LampAtcPower = b; break;
                                        case "atcuse": Interface.QuickReferences.LampAtcUse = b; break;
                                        case "atcemergency": Interface.QuickReferences.LampAtcEmergency = b; break;
                                        case "eb": Interface.QuickReferences.LampEb = b; break;
                                        case "constspeed": Interface.QuickReferences.LampConstSpeed = b; break;
                                    } break;

                                case "handles":
                                    switch (a) {
                                        case "file_reverser": Renderer.FileReverser = b; break;
                                        case "file_power": Renderer.FilePower = b; break;
                                        case "file_brake": Renderer.FileBrake = b; break;
                                        case "file_single": Renderer.FileSingle = b; break;
                                        case "forward": Interface.QuickReferences.HandleForward = b; break;
                                        case "neutral": Interface.QuickReferences.HandleNeutral = b; break;
                                        case "backward": Interface.QuickReferences.HandleBackward = b; break;
                                        case "power": Interface.QuickReferences.HandlePower = b; break;
                                        case "powernull": Interface.QuickReferences.HandlePowerNull = b; break;
                                        case "brake": Interface.QuickReferences.HandleBrake = b; break;
                                        case "brakenull": Interface.QuickReferences.HandleBrakeNull = b; break;
                                        case "release": Interface.QuickReferences.HandleRelease = b; break;
                                        case "lap": Interface.QuickReferences.HandleLap = b; break;
                                        case "service": Interface.QuickReferences.HandleService = b; break;
                                        case "emergency": Interface.QuickReferences.HandleEmergency = b; break;
                                        case "holdbrake": Interface.QuickReferences.HandleHoldBrake = b; break;
                                    } break;
                                case "doors":
                                    switch (a) {
                                        case "file": Renderer.FileDoors = b; break;
                                        case "left": Interface.QuickReferences.DoorsLeft = b; break;
                                        case "right": Interface.QuickReferences.DoorsRight = b; break;
                                    } break;
                                case "misc":
                                    switch (a) {
                                        case "file_pause": Renderer.FilePause = b; break;
                                        case "file_driver": Renderer.FileDriver = b; break;
                                        case "score": Interface.QuickReferences.Score = b; break;
                                    } break;
                                case "commands": {
                                        for (int k = 0; k < CommandInfos.Length; k++) {
                                            if (string.Compare(CommandInfos[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
                                                CommandInfos[k].Description = b;
                                                break;
                                            }
                                        }
                                    } break;
                                case "keys": {
                                        for (int k = 0; k < Keys.Length; k++) {
                                            if (string.Compare(Keys[k].Name, a, StringComparison.OrdinalIgnoreCase) == 0) {
                                                Keys[k].Description = b;
                                                break;
                                            }
                                        }
                                    } break;
                                default:
                                    AddInterfaceString(Section + "_" + a, b);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        // ================================

        // commands
        internal enum Command {
            None = 0,
            PowerIncrease, PowerDecrease, PowerHalfAxis, PowerFullAxis,
            BrakeIncrease, BrakeDecrease, BrakeEmergency, BrakeHalfAxis, BrakeFullAxis,
            SinglePower, SingleNeutral, SingleBrake, SingleEmergency, SingleFullAxis,
            ReverserForward, ReverserBackward, ReverserFullAxis,
            DoorsLeft, DoorsRight,
            HornPrimary, HornSecondary, HornMusic,
            DeviceConstSpeed,
            SecurityPower, SecurityS, SecurityA1, SecurityA2, SecurityB1, SecurityB2, SecurityC1, SecurityC2,
            SecurityD, SecurityE, SecurityF, SecurityG, SecurityH, SecurityI, SecurityJ, SecurityK, SecurityL,
            CameraInterior, CameraExterior, CameraTrack, CameraFlyBy,
            CameraMoveForward, CameraMoveBackward, CameraMoveLeft, CameraMoveRight, CameraMoveUp, CameraMoveDown,
            CameraRotateLeft, CameraRotateRight, CameraRotateUp, CameraRotateDown, CameraRotateCCW, CameraRotateCW,
            CameraZoomIn, CameraZoomOut, CameraPreviousPOI, CameraNextPOI, CameraReset, CameraRestriction,
            TimetableToggle, TimetableUp, TimetableDown,
            MiscClock, MiscSpeed, MiscAI, MiscInterfaceMode, MiscBackfaceCulling, MiscCPUMode, MiscPause, MiscFullscreen, MiscQuit,
            MenuActivate, MenuUp, MenuDown, MenuEnter, MenuBack,
            DebugWireframe, DebugNormals, DebugBrakeSystems
        }
        internal enum CommandType { Digital, AnalogHalf, AnalogFull }
        internal struct CommandInfo {
            internal Command Command;
            internal CommandType Type;
            internal string Name;
            internal string Description;
            internal CommandInfo(Command Command, CommandType Type, string Name, string Description) {
                this.Command = Command;
                this.Type = Type;
                this.Name = Name;
                this.Description = Description;
            }
        }

        // key infos
        internal struct KeyInfo {
            internal int Value;
            internal string Name;
            internal string Description;
            internal KeyInfo(int Value, string Name, string Description) {
                this.Value = Value;
                this.Name = Name;
                this.Description = Description;
            }
        }
        internal static KeyInfo[] Keys = new KeyInfo[] {
            new KeyInfo(Sdl.SDLK_0, "0", "0"),
            new KeyInfo(Sdl.SDLK_1, "1", "1"),
            new KeyInfo(Sdl.SDLK_2, "2", "2"),
            new KeyInfo(Sdl.SDLK_3, "3", "3"),
            new KeyInfo(Sdl.SDLK_4, "4", "4"),
            new KeyInfo(Sdl.SDLK_5, "5", "5"),
            new KeyInfo(Sdl.SDLK_6, "6", "6"),
            new KeyInfo(Sdl.SDLK_7, "7", "7"),
            new KeyInfo(Sdl.SDLK_8, "8", "8"),
            new KeyInfo(Sdl.SDLK_9, "9", "9"),
            new KeyInfo(Sdl.SDLK_AMPERSAND, "AMPERSAND", "Ampersand"),
            new KeyInfo(Sdl.SDLK_ASTERISK, "ASTERISK", "Asterisk"),
            new KeyInfo(Sdl.SDLK_AT, "AT", "At"),
            new KeyInfo(Sdl.SDLK_BACKQUOTE, "BACKQUOTE", "Backquote"),
            new KeyInfo(Sdl.SDLK_BACKSLASH, "BACKSLASH", "Backslash"),
            new KeyInfo(Sdl.SDLK_BACKSPACE, "BACKSPACE", "Backspace"),
            new KeyInfo(Sdl.SDLK_BREAK, "BREAK", "Break"),
            new KeyInfo(Sdl.SDLK_CAPSLOCK, "CAPSLOCK", "Capslock"),
            new KeyInfo(Sdl.SDLK_CARET, "CARET", "Caret"),
            new KeyInfo(Sdl.SDLK_CLEAR, "CLEAR", "Clear"),
            new KeyInfo(Sdl.SDLK_COLON, "COLON", "Colon"),
            new KeyInfo(Sdl.SDLK_COMMA, "COMMA", "Comma"),
            new KeyInfo(Sdl.SDLK_DELETE, "DELETE", "Delete"),
            new KeyInfo(Sdl.SDLK_DOLLAR, "DOLLAR", "Dollar"),
            new KeyInfo(Sdl.SDLK_DOWN, "DOWN", "Down"),
            new KeyInfo(Sdl.SDLK_END, "END", "End"),
            new KeyInfo(Sdl.SDLK_EQUALS, "EQUALS", "Equals"),
            new KeyInfo(Sdl.SDLK_ESCAPE, "ESCAPE", "Escape"),
            new KeyInfo(Sdl.SDLK_EURO, "EURO", "Euro"),
            new KeyInfo(Sdl.SDLK_EXCLAIM, "EXCLAIM", "Exclamation"),
            new KeyInfo(Sdl.SDLK_F1, "F1", "F1"),
            new KeyInfo(Sdl.SDLK_F2, "F2", "F2"),
            new KeyInfo(Sdl.SDLK_F3, "F3", "F3"),
            new KeyInfo(Sdl.SDLK_F4, "F4", "F4"),
            new KeyInfo(Sdl.SDLK_F5, "F5", "F5"),
            new KeyInfo(Sdl.SDLK_F6, "F6", "F6"),
            new KeyInfo(Sdl.SDLK_F7, "F7", "F7"),
            new KeyInfo(Sdl.SDLK_F8, "F8", "F8"),
            new KeyInfo(Sdl.SDLK_F9, "F9", "F9"),
            new KeyInfo(Sdl.SDLK_F10, "F10", "F10"),
            new KeyInfo(Sdl.SDLK_F11, "F11", "F11"),
            new KeyInfo(Sdl.SDLK_F12, "F12", "F12"),
            new KeyInfo(Sdl.SDLK_F13, "F13", "F13"),
            new KeyInfo(Sdl.SDLK_F14, "F14", "F14"),
            new KeyInfo(Sdl.SDLK_F15, "F15", "F15"),
            new KeyInfo(Sdl.SDLK_GREATER, "GREATER", "Greater"),
            new KeyInfo(Sdl.SDLK_HASH, "HASH", "Hash"),
            new KeyInfo(Sdl.SDLK_HELP, "HELP", "Help"),
            new KeyInfo(Sdl.SDLK_HOME, "HOME", "Home"),
            new KeyInfo(Sdl.SDLK_INSERT, "INSERT", "Insert"),
            new KeyInfo(Sdl.SDLK_KP0, "KP0", "Keypad 0"),
            new KeyInfo(Sdl.SDLK_KP1, "KP1", "Keypad 1"),
            new KeyInfo(Sdl.SDLK_KP2, "KP2", "Keypad 2"),
            new KeyInfo(Sdl.SDLK_KP3, "KP3", "Keypad 3"),
            new KeyInfo(Sdl.SDLK_KP4, "KP4", "Keypad 4"),
            new KeyInfo(Sdl.SDLK_KP5, "KP5", "Keypad 5"),
            new KeyInfo(Sdl.SDLK_KP6, "KP6", "Keypad 6"),
            new KeyInfo(Sdl.SDLK_KP7, "KP7", "Keypad 7"),
            new KeyInfo(Sdl.SDLK_KP8, "KP8", "Keypad 8"),
            new KeyInfo(Sdl.SDLK_KP9, "KP9", "Keypad 9"),
            new KeyInfo(Sdl.SDLK_KP_DIVIDE, "KP_DIVIDE", "Keypad Divide"),
            new KeyInfo(Sdl.SDLK_KP_ENTER, "KP_ENTER", "Keypad Enter"),
            new KeyInfo(Sdl.SDLK_KP_EQUALS, "KP_EQUALS", "Keypad Equals"),
            new KeyInfo(Sdl.SDLK_KP_MINUS, "KP_MINUS", "Keypad Minus"),
            new KeyInfo(Sdl.SDLK_KP_MULTIPLY, "KP_MULTIPLY", "Keypad Multiply"),
            new KeyInfo(Sdl.SDLK_KP_PERIOD, "KP_PERIOD", "Keypad Period"),
            new KeyInfo(Sdl.SDLK_KP_PLUS, "KP_PLUS", "Keypad Plus"),
            new KeyInfo(Sdl.SDLK_LALT, "LALT", "Left Alt"),
            new KeyInfo(Sdl.SDLK_LCTRL, "LCTRL", "Left Ctrl"),
            new KeyInfo(Sdl.SDLK_LEFT, "LEFT", "Left"),
            new KeyInfo(Sdl.SDLK_LEFTBRACKET, "LEFTBRACKET", "Left bracket"),
            new KeyInfo(Sdl.SDLK_LEFTPAREN, "LEFTPAREN", "Left parantheses"),
            new KeyInfo(Sdl.SDLK_LESS, "LESS", "Less"),
            new KeyInfo(Sdl.SDLK_LMETA, "LMETA", "Left Meta"),
            new KeyInfo(Sdl.SDLK_LSHIFT, "LSHIFT", "Left Shift"),
            new KeyInfo(Sdl.SDLK_LSUPER, "LSUPER", "Left Application"),
            new KeyInfo(Sdl.SDLK_MENU, "MENU", "Menu"),
            new KeyInfo(Sdl.SDLK_MINUS, "MINUS", "Minus"),
            new KeyInfo(Sdl.SDLK_MODE, "MODE", "Alt Gr"),
            new KeyInfo(Sdl.SDLK_NUMLOCK, "NUMLOCK", "Numlock"),
            new KeyInfo(Sdl.SDLK_PAGEDOWN, "PAGEDOWN", "Page down"),
            new KeyInfo(Sdl.SDLK_PAGEUP, "PAGEUP", "Page up"),
            new KeyInfo(Sdl.SDLK_PAUSE, "PAUSE", "Pause"),
            new KeyInfo(Sdl.SDLK_PERIOD, "PERIOD", "Period"),
            new KeyInfo(Sdl.SDLK_PLUS, "PLUS", "Plus"),
            new KeyInfo(Sdl.SDLK_POWER, "POWER", "Power"),
            new KeyInfo(Sdl.SDLK_PRINT, "PRINT", "Print"),
            new KeyInfo(Sdl.SDLK_QUESTION, "QUESTION", "Question"),
            new KeyInfo(Sdl.SDLK_QUOTE, "QUOTE", "Quote"),
            new KeyInfo(Sdl.SDLK_QUOTEDBL, "QUOTEDBL", "Quote double"),
            new KeyInfo(Sdl.SDLK_RALT, "RALT", "Right Alt"),
            new KeyInfo(Sdl.SDLK_RCTRL, "RCTRL", "Right Ctrl"),
            new KeyInfo(Sdl.SDLK_RETURN, "RETURN", "Return"),
            new KeyInfo(Sdl.SDLK_RIGHT, "RIGHT", "Right"),
            new KeyInfo(Sdl.SDLK_RIGHTBRACKET, "RIGHTBRACKET", "Right bracket"),
            new KeyInfo(Sdl.SDLK_RIGHTPAREN, "RIGHTPAREN", "Right parantheses"),
            new KeyInfo(Sdl.SDLK_RMETA, "RMETA", "Right Meta"),
            new KeyInfo(Sdl.SDLK_RSHIFT, "RSHIFT", "Right Shift"),
            new KeyInfo(Sdl.SDLK_RSUPER, "RSUPER", "Right Application"),
            new KeyInfo(Sdl.SDLK_SCROLLOCK, "SCROLLLOCK", "Scrolllock"),
            new KeyInfo(Sdl.SDLK_SEMICOLON, "SEMICOLON", "Semicolon"),
            new KeyInfo(Sdl.SDLK_SLASH, "SLASH", "Slash"),
            new KeyInfo(Sdl.SDLK_SPACE, "SPACE", "Space"),
            new KeyInfo(Sdl.SDLK_SYSREQ, "SYSREQ", "SysRq"),
            new KeyInfo(Sdl.SDLK_TAB, "TAB", "Tab"),
            new KeyInfo(Sdl.SDLK_UNDERSCORE, "UNDERSCORE", "Underscore"),
            new KeyInfo(Sdl.SDLK_UP, "UP", "Up"),
            new KeyInfo(Sdl.SDLK_a, "a", "A"),
            new KeyInfo(Sdl.SDLK_b, "b", "B"),
            new KeyInfo(Sdl.SDLK_c, "c", "C"),
            new KeyInfo(Sdl.SDLK_d, "d", "D"),
            new KeyInfo(Sdl.SDLK_e, "e", "E"),
            new KeyInfo(Sdl.SDLK_f, "f", "F"),
            new KeyInfo(Sdl.SDLK_g, "g", "G"),
            new KeyInfo(Sdl.SDLK_h, "h", "H"),
            new KeyInfo(Sdl.SDLK_i, "i", "I"),
            new KeyInfo(Sdl.SDLK_j, "j", "J"),
            new KeyInfo(Sdl.SDLK_k, "k", "K"),
            new KeyInfo(Sdl.SDLK_l, "l", "L"),
            new KeyInfo(Sdl.SDLK_m, "m", "M"),
            new KeyInfo(Sdl.SDLK_n, "n", "N"),
            new KeyInfo(Sdl.SDLK_o, "o", "O"),
            new KeyInfo(Sdl.SDLK_p, "p", "P"),
            new KeyInfo(Sdl.SDLK_q, "q", "Q"),
            new KeyInfo(Sdl.SDLK_r, "r", "R"),
            new KeyInfo(Sdl.SDLK_s, "s", "S"),
            new KeyInfo(Sdl.SDLK_t, "t", "T"),
            new KeyInfo(Sdl.SDLK_u, "u", "U"),
            new KeyInfo(Sdl.SDLK_v, "v", "V"),
            new KeyInfo(Sdl.SDLK_w, "w", "W"),
            new KeyInfo(Sdl.SDLK_x, "x", "X"),
            new KeyInfo(Sdl.SDLK_y, "y", "Y"),
            new KeyInfo(Sdl.SDLK_z, "z", "Z")
        };

        // controls
        internal enum ControlMethod {
            Invalid = 0,
            Keyboard = 1,
            Joystick = 2
        }
        internal enum KeyboardModifier {
            None = 0,
            Shift = 1,
            Ctrl = 2,
            Alt = 4
        }
        internal enum JoystickComponent { Invalid, Axis, Ball, Hat, Button }
        internal enum DigitalControlState {
            ReleasedAcknowledged = 0,
            Released = 1,
            Pressed = 2,
            PressedAcknowledged = 3
        }
        internal struct Control {
            internal Command Command;
            internal CommandType InheritedType;
            internal ControlMethod Method;
            internal KeyboardModifier Modifier;
            internal int Device;
            internal JoystickComponent Component;
            internal int Element;
            internal int Direction;
            internal DigitalControlState DigitalState;
            internal double AnalogState;
        }
        internal struct Joystick {
            internal IntPtr SdlHandle;
            internal string Name;
        }

        // control descriptions
        internal static string[] ControlDescriptions = new string[] { };
        internal static CommandInfo[] CommandInfos = new CommandInfo[] {
            new CommandInfo(Command.PowerIncrease, CommandType.Digital, "POWER_INCREASE", "Increases power by one notch for trains with two handles"),
            new CommandInfo(Command.PowerDecrease, CommandType.Digital, "POWER_DECREASE", "Decreases power by one notch for trains with two handles"),
            new CommandInfo(Command.PowerHalfAxis, CommandType.AnalogHalf, "POWER_HALFAXIS", "Controls power for trains with two handles on half of a joystick axis"),
            new CommandInfo(Command.PowerFullAxis, CommandType.AnalogFull, "POWER_FULLAXIS", "Controls power for trains with two handles on a full joystick axis"),
            new CommandInfo(Command.BrakeDecrease, CommandType.Digital, "BRAKE_DECREASE", "Decreases brake by one notch for trains with two handles"),
            new CommandInfo(Command.BrakeIncrease, CommandType.Digital, "BRAKE_INCREASE", "Increases brake by one notch for trains with two handles"),
            new CommandInfo(Command.BrakeHalfAxis, CommandType.AnalogHalf, "BRAKE_HALFAXIS", "Controls brake for trains with two handles on half of a joystick axis"),
            new CommandInfo(Command.BrakeFullAxis, CommandType.AnalogFull, "BRAKE_FULLAXIS", "Controls brake for trains with two handles on a full joystick axis"),
            new CommandInfo(Command.BrakeEmergency, CommandType.Digital, "BRAKE_EMERGENCY", "Activates emergency brakes for trains with two handles"),
            new CommandInfo(Command.SinglePower, CommandType.Digital, "SINGLE_POWER", "Moves handle toward power by one notch for trains with one handle"),
            new CommandInfo(Command.SingleNeutral, CommandType.Digital, "SINGLE_NEUTRAL", "Moves handle toward neutral by one notch for trains with one handle"),
            new CommandInfo(Command.SingleBrake, CommandType.Digital, "SINGLE_BRAKE", "Moves handle toward brake by one notch for trains with one handle"),
            new CommandInfo(Command.SingleEmergency, CommandType.Digital, "SINGLE_EMERGENCY", "Activates emergency brake for trains with one handle"),
            new CommandInfo(Command.SingleFullAxis, CommandType.AnalogFull, "SINGLE_FULLAXIS", "Controls power and brake for trains with one handle on a full joystick axis"),
            new CommandInfo(Command.ReverserForward, CommandType.Digital, "REVERSER_FORWARD", "Moves reverser toward forward by one notch"),
            new CommandInfo(Command.ReverserBackward, CommandType.Digital, "REVERSER_BACKWARD", "Moves reverser toward backward by one notch"),
            new CommandInfo(Command.ReverserFullAxis, CommandType.AnalogFull, "REVERSER_FULLAXIS", "Controls reverser on a full joystick axis"),
            new CommandInfo(Command.DoorsLeft, CommandType.Digital, "DOORS_LEFT", "Opens or closes the left doors"),
            new CommandInfo(Command.DoorsRight, CommandType.Digital, "DOORS_RIGHT", "Opens or closes the right doors"),
            new CommandInfo(Command.HornPrimary, CommandType.Digital, "HORN_PRIMARY", "Plays the primary horn"),
            new CommandInfo(Command.HornSecondary, CommandType.Digital, "HORN_SECONDARY", "Plays the secondary horn"),
            new CommandInfo(Command.HornMusic, CommandType.Digital, "HORN_MUSIC", "Plays or stops the music horn"),
            new CommandInfo(Command.DeviceConstSpeed, CommandType.Digital, "DEVICE_CONSTSPEED", "Activates or deactivates the constant speed device"),
            new CommandInfo(Command.SecurityPower, CommandType.Digital, "SECURITY_POWER", "Activates or deactivates the security system on some trains"),
            new CommandInfo(Command.SecurityS, CommandType.Digital, "SECURITY_S", "The S function of the security system"),
            new CommandInfo(Command.SecurityA1, CommandType.Digital, "SECURITY_A1", "The A1 function of the security system"),
            new CommandInfo(Command.SecurityA2, CommandType.Digital, "SECURITY_A2", "The A2 function of the security system"),
            new CommandInfo(Command.SecurityB1, CommandType.Digital, "SECURITY_B1", "The B1 function of the security system"),
            new CommandInfo(Command.SecurityB2, CommandType.Digital, "SECURITY_B2", "The B2 function of the security system"),
            new CommandInfo(Command.SecurityC1, CommandType.Digital, "SECURITY_C1", "The C1 function of the security system"),
            new CommandInfo(Command.SecurityC2, CommandType.Digital, "SECURITY_C2", "The C2 function of the security system"),
            new CommandInfo(Command.SecurityD, CommandType.Digital, "SECURITY_D", "The D function of the security system"),
            new CommandInfo(Command.SecurityE, CommandType.Digital, "SECURITY_E", "The E function of the security system"),
            new CommandInfo(Command.SecurityF, CommandType.Digital, "SECURITY_F", "The F function of the security system"),
            new CommandInfo(Command.SecurityG, CommandType.Digital, "SECURITY_G", "The G function of the security system"),
            new CommandInfo(Command.SecurityH, CommandType.Digital, "SECURITY_H", "The H function of the security system"),
            new CommandInfo(Command.SecurityI, CommandType.Digital, "SECURITY_I", "The I function of the security system"),
            new CommandInfo(Command.SecurityJ, CommandType.Digital, "SECURITY_J", "The J function of the security system"),
            new CommandInfo(Command.SecurityK, CommandType.Digital, "SECURITY_K", "The K function of the security system"),
            new CommandInfo(Command.SecurityL, CommandType.Digital, "SECURITY_L", "The L function of the security system"),
            new CommandInfo(Command.CameraInterior, CommandType.Digital, "CAMERA_INTERIOR", "Switches to the train's interior view"),
            new CommandInfo(Command.CameraExterior, CommandType.Digital, "CAMERA_EXTERIOR", "Switches to the train's exterior view"),
            new CommandInfo(Command.CameraTrack, CommandType.Digital, "CAMERA_TRACK", "Switches to the track view"),
            new CommandInfo(Command.CameraFlyBy, CommandType.Digital, "CAMERA_FLYBY", "Switches between different fly-by views"),
            new CommandInfo(Command.CameraMoveForward, CommandType.AnalogHalf, "CAMERA_MOVE_FORWARD", "Moves the camera forward"),
            new CommandInfo(Command.CameraMoveBackward, CommandType.AnalogHalf, "CAMERA_MOVE_BACKWARD", "Moves the camera backward"),
            new CommandInfo(Command.CameraMoveLeft, CommandType.AnalogHalf, "CAMERA_MOVE_LEFT", "Moves the camera left"),
            new CommandInfo(Command.CameraMoveRight, CommandType.AnalogHalf, "CAMERA_MOVE_RIGHT", "Moves the camera right"),
            new CommandInfo(Command.CameraMoveUp, CommandType.AnalogHalf, "CAMERA_MOVE_UP", "Moves the camera up"),
            new CommandInfo(Command.CameraMoveDown, CommandType.AnalogHalf, "CAMERA_MOVE_DOWN", "Moves the camera down"),
            new CommandInfo(Command.CameraRotateLeft, CommandType.AnalogHalf, "CAMERA_ROTATE_LEFT", "Rotates the camera left"),
            new CommandInfo(Command.CameraRotateRight, CommandType.AnalogHalf, "CAMERA_ROTATE_RIGHT", "Rotates the camera right"),
            new CommandInfo(Command.CameraRotateUp, CommandType.AnalogHalf, "CAMERA_ROTATE_UP", "Rotates the camera up"),
            new CommandInfo(Command.CameraRotateDown, CommandType.AnalogHalf, "CAMERA_ROTATE_DOWN", "Rotates the camera down"),
            new CommandInfo(Command.CameraRotateCCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CCW", "Rotates the camera counter-clockwise"),
            new CommandInfo(Command.CameraRotateCW, CommandType.AnalogHalf, "CAMERA_ROTATE_CW", "Rotates the camera clockwise"),
            new CommandInfo(Command.CameraZoomIn, CommandType.AnalogHalf, "CAMERA_ZOOM_IN", "Zooms the camera in"),
            new CommandInfo(Command.CameraZoomOut, CommandType.AnalogHalf, "CAMERA_ZOOM_OUT", "Zooms the camera out"),
            new CommandInfo(Command.CameraPreviousPOI, CommandType.Digital, "CAMERA_POI_PREVIOUS", "Jumps to the previous point of interest in the route"),
            new CommandInfo(Command.CameraNextPOI, CommandType.Digital, "CAMERA_POI_NEXT", "Jumps to the next point of interest in the route"),
            new CommandInfo(Command.CameraReset, CommandType.Digital, "CAMERA_RESET", "Resets the camera view to default values"),
            new CommandInfo(Command.CameraRestriction, CommandType.Digital, "CAMERA_RESTRICTION", "Activates or deactivates interior view camera restriction"),
            new CommandInfo(Command.TimetableToggle, CommandType.Digital, "TIMETABLE_TOGGLE", "Toggles through different timetable modes"),
            new CommandInfo(Command.TimetableUp, CommandType.AnalogHalf, "TIMETABLE_UP", "Scrolls the timetable up"),
            new CommandInfo(Command.TimetableDown, CommandType.AnalogHalf, "TIMETABLE_DOWN", "Scrolls the timetable down"),
            new CommandInfo(Command.MenuActivate, CommandType.Digital, "MENU_ACTIVATE", "Displays the in-game menu"),
            new CommandInfo(Command.MenuUp, CommandType.Digital, "MENU_UP", "Moves the cursor up within the in-game menu"),
            new CommandInfo(Command.MenuDown, CommandType.Digital, "MENU_DOWN", "Moves the cursor down within the in-game menu"),
            new CommandInfo(Command.MenuEnter, CommandType.Digital, "MENU_ENTER", "Performs the selected command within the in-game menu"),
            new CommandInfo(Command.MenuBack, CommandType.Digital, "MENU_BACK", "Goes back within the in-game menu"),
            new CommandInfo(Command.MiscClock, CommandType.Digital, "MISC_CLOCK", "Shows or hides the clock"),
            new CommandInfo(Command.MiscSpeed, CommandType.Digital, "MISC_SPEED", "Toggles through different speed display modes"),
            new CommandInfo(Command.MiscAI, CommandType.Digital, "MISC_AI", "Activates or deactivates the virtual driver (AI)"),
            new CommandInfo(Command.MiscFullscreen, CommandType.Digital, "MISC_FULLSCREEN", "Toggles to or from full-screen mode"),
            new CommandInfo(Command.MiscPause, CommandType.Digital, "MISC_PAUSE", "Pauses or resumes the simulation"),
            new CommandInfo(Command.MiscQuit, CommandType.Digital, "MISC_QUIT", "Quits the simulation"),
            new CommandInfo(Command.MiscInterfaceMode, CommandType.Digital, "MISC_INTERFACE", "Toggles through different interface modes"),
            new CommandInfo(Command.MiscBackfaceCulling, CommandType.Digital, "MISC_BACKFACE", "Activates or deactivates backface culling"),
            new CommandInfo(Command.MiscCPUMode, CommandType.Digital, "MISC_CPUMODE", "Switches to or from reduced CPU mode"),
            new CommandInfo(Command.DebugWireframe, CommandType.Digital, "DEBUG_WIREFRAME", "Activates or deactivates wireframe mode"),
            new CommandInfo(Command.DebugNormals, CommandType.Digital, "DEBUG_NORMALS", "Shows or hides vertex normals"),
            new CommandInfo(Command.DebugBrakeSystems, CommandType.Digital, "DEBUG_BRAKE", "Shows or hides brake system debug output"),
        };
        internal static Control[] Controls = new Control[] { };
        internal static Joystick[] Joysticks = new Joystick[] { };

        // try get command info
        internal static bool TryGetCommandInfo(Command Value, out CommandInfo Info) {
            for (int i = 0; i < CommandInfos.Length; i++) {
                if (CommandInfos[i].Command == Value) {
                    Info = CommandInfos[i];
                    return true;
                }
            }
            Info.Command = Value;
            Info.Type = CommandType.Digital;
            Info.Name = "?";
            Info.Description = "?";
            return false;
        }

        // save controls
        internal static void SaveControls(string FileOrNull) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            System.Text.StringBuilder Builder = new System.Text.StringBuilder();
            Builder.AppendLine("; Controls");
            Builder.AppendLine("; ========");
            Builder.AppendLine("; This file was automatically generated.");
            Builder.AppendLine("; Please do not modify directly.");
            Builder.AppendLine();
            for (int i = 0; i < Controls.Length; i++) {
                CommandInfo Info;
                TryGetCommandInfo(Controls[i].Command, out Info);
                Builder.Append(Info.Name + ", ");
                switch (Controls[i].Method) {
                    case ControlMethod.Keyboard:
                        Builder.Append("keyboard, " + Controls[i].Element.ToString(Culture) + ", " + ((int)Controls[i].Modifier).ToString(Culture));
                        break;
                    case ControlMethod.Joystick:
                        Builder.Append("joystick, " + Controls[i].Device.ToString(Culture) + ", ");
                        switch (Controls[i].Component) {
                            case JoystickComponent.Axis:
                                Builder.Append("axis, " + Controls[i].Element.ToString(Culture) + ", " + Controls[i].Direction.ToString(Culture));
                                break;
                            case JoystickComponent.Ball:
                                Builder.Append("ball, " + Controls[i].Element.ToString(Culture) + ", " + Controls[i].Direction.ToString(Culture));
                                break;
                            case JoystickComponent.Hat:
                                Builder.Append("hat, " + Controls[i].Element.ToString(Culture) + ", " + Controls[i].Direction.ToString(Culture));
                                break;
                            case JoystickComponent.Button:
                                Builder.Append("button, " + Controls[i].Element.ToString(Culture));
                                break;
                            default:
                                Builder.Append("invalid");
                                break;
                        }
                        break;
                    default:
                        break;
                }
                Builder.Append("\n");
            }
            string File;
            if (FileOrNull == null) {
                string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
                string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
                if (!System.IO.Directory.Exists(Folder))
                    System.IO.Directory.CreateDirectory(Folder);
                File = Interface.GetCombinedFileName(Folder, "controls.cfg");
            } else {
                File = FileOrNull;
            }
            System.IO.File.WriteAllText(File, Builder.ToString(), new System.Text.UTF8Encoding(true));
        }

        // load controls
        internal static void LoadControls(string FileOrNull) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            Controls = new Control[] { };
            string File;
            if (FileOrNull == null) {
                string ConfigDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );
                string Folder = Interface.GetCombinedFileName(ConfigDir, "OpenBVE");
                File = Interface.GetCombinedFileName(Folder, "controls.cfg");
                if (!System.IO.File.Exists(File)) {
                    Folder = Interface.GetCombinedFolderName(System.Windows.Forms.Application.StartupPath, "Controls");
                    File = Interface.GetCombinedFileName(Folder, "Default keyboard assignment.controls");
                }
            } else {
                File = FileOrNull;
            }
            string[] Lines = System.IO.File.ReadAllLines(File, new System.Text.UTF8Encoding());
            for (int i = 0; i < Lines.Length; i++) {
                Lines[i] = Lines[i].Trim();
                if (Lines[i].Length != 0 && !Lines[i].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
                    string[] Terms = Lines[i].Split(new char[] { ',' });
                    for (int j = 0; j < Terms.Length; j++) {
                        Terms[j] = Terms[j].Trim();
                    }
                    if (Terms.Length >= 2) {
                        int n = Controls.Length;
                        Array.Resize<Control>(ref Controls, n + 1);
                        int j;
                        for (j = 0; j < CommandInfos.Length; j++) {
                            if (string.Compare(CommandInfos[j].Name, Terms[0], StringComparison.OrdinalIgnoreCase) == 0) break;
                        }
                        if (j == CommandInfos.Length) {
                            Controls[n].Command = Command.None;
                            Controls[n].InheritedType = CommandType.Digital;
                            Controls[n].Method = ControlMethod.Invalid;
                            Controls[n].Device = -1;
                            Controls[n].Component = JoystickComponent.Invalid;
                            Controls[n].Element = -1;
                            Controls[n].Direction = 0;
                            Controls[n].Modifier = KeyboardModifier.None;
                        } else {
                            Controls[n].Command = CommandInfos[j].Command;
                            Controls[n].InheritedType = CommandInfos[j].Type;
                            string Method = Terms[1].ToLowerInvariant();
                            bool Valid = false;
                            if (Method == "keyboard" & Terms.Length == 4) {
                                int Element, Modifiers;
                                if (int.TryParse(Terms[2], System.Globalization.NumberStyles.Integer, Culture, out Element)) {
                                    if (int.TryParse(Terms[3], System.Globalization.NumberStyles.Integer, Culture, out Modifiers)) {
                                        Controls[n].Method = ControlMethod.Keyboard;
                                        Controls[n].Device = -1;
                                        Controls[n].Component = JoystickComponent.Invalid;
                                        Controls[n].Element = Element;
                                        Controls[n].Direction = 0;
                                        Controls[n].Modifier = (KeyboardModifier)Modifiers;
                                        Valid = true;
                                    }
                                }
                            } else if (Method == "joystick" & Terms.Length >= 4) {
                                int Device;
                                if (int.TryParse(Terms[2], System.Globalization.NumberStyles.Integer, Culture, out Device)) {
                                    string Component = Terms[3].ToLowerInvariant();
                                    if (Component == "axis" & Terms.Length == 6) {
                                        int Element, Direction;
                                        if (int.TryParse(Terms[4], System.Globalization.NumberStyles.Integer, Culture, out Element)) {
                                            if (int.TryParse(Terms[5], System.Globalization.NumberStyles.Integer, Culture, out Direction)) {
                                                Controls[n].Method = ControlMethod.Joystick;
                                                Controls[n].Device = Device;
                                                Controls[n].Component = JoystickComponent.Axis;
                                                Controls[n].Element = Element;
                                                Controls[n].Direction = Direction;
                                                Controls[n].Modifier = KeyboardModifier.None;
                                                Valid = true;
                                            }
                                        }
                                    } else if (Component == "ball" & Terms.Length == 6) {
                                        int Element, Direction;
                                        if (int.TryParse(Terms[4], System.Globalization.NumberStyles.Integer, Culture, out Element)) {
                                            if (int.TryParse(Terms[5], System.Globalization.NumberStyles.Integer, Culture, out Direction)) {
                                                Controls[n].Method = ControlMethod.Joystick;
                                                Controls[n].Device = Device;
                                                Controls[n].Component = JoystickComponent.Ball;
                                                Controls[n].Element = Element;
                                                Controls[n].Direction = Direction;
                                                Controls[n].Modifier = KeyboardModifier.None;
                                                Valid = true;
                                            }
                                        }
                                    } else if (Component == "hat" & Terms.Length == 6) {
                                        int Element, Direction;
                                        if (int.TryParse(Terms[4], System.Globalization.NumberStyles.Integer, Culture, out Element)) {
                                            if (int.TryParse(Terms[5], System.Globalization.NumberStyles.Integer, Culture, out Direction)) {
                                                Controls[n].Method = ControlMethod.Joystick;
                                                Controls[n].Device = Device;
                                                Controls[n].Component = JoystickComponent.Hat;
                                                Controls[n].Element = Element;
                                                Controls[n].Direction = Direction;
                                                Controls[n].Modifier = KeyboardModifier.None;
                                                Valid = true;
                                            }
                                        }
                                    } else if (Component == "button" & Terms.Length == 5) {
                                        int Element;
                                        if (int.TryParse(Terms[4], System.Globalization.NumberStyles.Integer, Culture, out Element)) {
                                            Controls[n].Method = ControlMethod.Joystick;
                                            Controls[n].Device = Device;
                                            Controls[n].Component = JoystickComponent.Button;
                                            Controls[n].Element = Element;
                                            Controls[n].Direction = 0;
                                            Controls[n].Modifier = KeyboardModifier.None;
                                            Valid = true;

                                        }
                                    }
                                }
                            }
                            if (!Valid) {
                                Controls[n].Method = ControlMethod.Invalid;
                                Controls[n].Device = -1;
                                Controls[n].Component = JoystickComponent.Invalid;
                                Controls[n].Element = -1;
                                Controls[n].Direction = 0;
                                Controls[n].Modifier = KeyboardModifier.None;
                            }
                        }
                    }
                }
            }
        }

        // ================================

        // hud elements
        internal struct HudVector {
            internal int X;
            internal int Y;
        }
        internal struct HudVectorF {
            internal float X;
            internal float Y;
        }
        internal struct HudImage {
            internal int BackgroundTextureIndex;
            internal int OverlayTextureIndex;
        }
        internal enum HudTransition {
            None = 0,
            Move = 1,
            Fade = 2,
            MoveAndFade = 3
        }
        internal class HudElement {
            internal string Subject;
            internal HudVectorF Position;
            internal HudVector Alignment;
            internal HudImage TopLeft;
            internal HudImage TopMiddle;
            internal HudImage TopRight;
            internal HudImage CenterLeft;
            internal HudImage CenterMiddle;
            internal HudImage CenterRight;
            internal HudImage BottomLeft;
            internal HudImage BottomMiddle;
            internal HudImage BottomRight;
            internal World.ColorRGBA BackgroundColor;
            internal World.ColorRGBA OverlayColor;
            internal World.ColorRGBA TextColor;
            internal HudVectorF TextPosition;
            internal HudVector TextAlignment;
            internal Fonts.FontType TextSize;
            internal bool TextShadow;
            internal string Text;
            internal float Value;
            internal HudTransition Transition;
            internal HudVectorF TransitionVector;
            internal double TransitionState;
            internal HudElement() {
                this.Subject = null;
                this.Position.X = 0.0f;
                this.Position.Y = 0.0f;
                this.Alignment.X = -1;
                this.Alignment.Y = -1;
                this.TopLeft.BackgroundTextureIndex = -1;
                this.TopLeft.OverlayTextureIndex = -1;
                this.TopMiddle.BackgroundTextureIndex = -1;
                this.TopMiddle.OverlayTextureIndex = -1;
                this.TopRight.BackgroundTextureIndex = -1;
                this.TopRight.OverlayTextureIndex = -1;
                this.CenterLeft.BackgroundTextureIndex = -1;
                this.CenterLeft.OverlayTextureIndex = -1;
                this.CenterMiddle.BackgroundTextureIndex = -1;
                this.CenterMiddle.OverlayTextureIndex = -1;
                this.CenterRight.BackgroundTextureIndex = -1;
                this.CenterRight.OverlayTextureIndex = -1;
                this.BottomLeft.BackgroundTextureIndex = -1;
                this.BottomLeft.OverlayTextureIndex = -1;
                this.BottomMiddle.BackgroundTextureIndex = -1;
                this.BottomMiddle.OverlayTextureIndex = -1;
                this.BottomRight.BackgroundTextureIndex = -1;
                this.BottomRight.OverlayTextureIndex = -1;
                this.BackgroundColor = new World.ColorRGBA(255, 255, 255, 255);
                this.OverlayColor = new World.ColorRGBA(255, 255, 255, 255);
                this.TextColor = new World.ColorRGBA(255, 255, 255, 255);
                this.TextPosition.X = 0.0f;
                this.TextPosition.Y = 0.0f;
                this.TextAlignment.X = -1;
                this.TextAlignment.Y = 0;
                this.TextSize = Fonts.FontType.ExtraSmall;
                this.TextShadow = true;
                this.Text = null;
                this.Value = 0.0f;
                this.Transition = HudTransition.None;
                this.TransitionState = 0.0;
            }
        }
        internal static HudElement[] CurrentHudElements = new HudElement[] { };

        // load hud
        internal static void LoadHUD(string FileOrNull) {

            // ### TO BE REMOVED SOON
            CurrentHudElements = new HudElement[] { };
            return;

            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            string Folder;
            if (FileOrNull == null) {
                Folder = Interface.GetCombinedFolderName(System.Windows.Forms.Application.StartupPath, "Interface");
                Folder = Interface.GetCombinedFolderName(Folder, "HUD");
                Folder = Interface.GetCombinedFolderName(Folder, "Default");
                FileOrNull = Interface.GetCombinedFileName(Folder, "hud.cfg");
            } else {
                Folder = System.IO.Path.GetDirectoryName(FileOrNull);
            }
            CurrentHudElements = new HudElement[16];
            int Length = 0;
            if (System.IO.File.Exists(FileOrNull)) {
                string[] Lines = System.IO.File.ReadAllLines(FileOrNull, new System.Text.UTF8Encoding());
                for (int i = 0; i < Lines.Length; i++) {
                    int j = Lines[i].IndexOf(';');
                    if (j >= 0) {
                        Lines[i] = Lines[i].Substring(0, j).Trim();
                    } else {
                        Lines[i] = Lines[i].Trim();
                    }
                    if (Lines[i].Length != 0) {
                        if (!Lines[i].StartsWith(";", StringComparison.Ordinal)) {
                            if (Lines[i].Equals("[element]", StringComparison.OrdinalIgnoreCase)) {
                                Length++;
                                if (Length > CurrentHudElements.Length) {
                                    Array.Resize<HudElement>(ref CurrentHudElements, CurrentHudElements.Length << 1);
                                }
                                CurrentHudElements[Length - 1] = new HudElement();
                            } else if (Length == 0) {
                                System.Windows.Forms.MessageBox.Show("Line outside of [element] structure encountered at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                            } else {
                                j = Lines[i].IndexOf("=", StringComparison.Ordinal);
                                if (j >= 0) {
                                    string Command = Lines[i].Substring(0, j).TrimEnd();
                                    string[] Arguments = Lines[i].Substring(j + 1).TrimStart().Split(new char[] { ',' }, StringSplitOptions.None);
                                    for (j = 0; j < Arguments.Length; j++) {
                                        Arguments[j] = Arguments[j].Trim();
                                    }
                                    switch (Command.ToLowerInvariant()) {
                                        case "subject":
                                            if (Arguments.Length == 1) {
                                                CurrentHudElements[Length - 1].Subject = Arguments[0];
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "position":
                                            if (Arguments.Length == 2) {
                                                float x, y;
                                                if (!float.TryParse(Arguments[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                    System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!float.TryParse(Arguments[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                    System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].Position.X = x;
                                                    CurrentHudElements[Length - 1].Position.Y = y;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "alignment":
                                            if (Arguments.Length == 2) {
                                                int x, y;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out x)) {
                                                    System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[1], System.Globalization.NumberStyles.Integer, Culture, out y)) {
                                                    System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].Alignment.X = Math.Sign(x);
                                                    CurrentHudElements[Length - 1].Alignment.Y = Math.Sign(y);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "topleft":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopLeft.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopLeft.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "topmiddle":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopMiddle.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopMiddle.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "topright":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopRight.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].TopRight.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "centerleft":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterLeft.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterLeft.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "centermiddle":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterMiddle.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterMiddle.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "centerright":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterRight.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].CenterRight.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "bottomleft":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomLeft.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomLeft.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "bottommiddle":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomMiddle.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomMiddle.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "bottomright":
                                            if (Arguments.Length == 2) {
                                                if (Arguments[0].Length != 0 & !Arguments[0].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomRight.BackgroundTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[0]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                                if (Arguments[1].Length != 0 & !Arguments[1].Equals("null", StringComparison.OrdinalIgnoreCase)) {
                                                    CurrentHudElements[Length - 1].BottomRight.OverlayTextureIndex = TextureManager.RegisterTexture(GetCombinedFileName(Folder, Arguments[1]), TextureManager.TextureWrapMode.ClampToEdge, true);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "backcolor":
                                            if (Arguments.Length == 4) {
                                                int r, g, b, a;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out r)) {
                                                    System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[1], System.Globalization.NumberStyles.Integer, Culture, out g)) {
                                                    System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[2], System.Globalization.NumberStyles.Integer, Culture, out b)) {
                                                    System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[3], System.Globalization.NumberStyles.Integer, Culture, out a)) {
                                                    System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    r = r < 0 ? 0 : r > 255 ? 255 : r;
                                                    g = g < 0 ? 0 : g > 255 ? 255 : g;
                                                    b = b < 0 ? 0 : b > 255 ? 255 : b;
                                                    a = a < 0 ? 0 : a > 255 ? 255 : a;
                                                    CurrentHudElements[Length - 1].BackgroundColor = new World.ColorRGBA((byte)r, (byte)g, (byte)b, (byte)a);
                                                } break;
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "overlaycolor":
                                            if (Arguments.Length == 4) {
                                                int r, g, b, a;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out r)) {
                                                    System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[1], System.Globalization.NumberStyles.Integer, Culture, out g)) {
                                                    System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[2], System.Globalization.NumberStyles.Integer, Culture, out b)) {
                                                    System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[3], System.Globalization.NumberStyles.Integer, Culture, out a)) {
                                                    System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    r = r < 0 ? 0 : r > 255 ? 255 : r;
                                                    g = g < 0 ? 0 : g > 255 ? 255 : g;
                                                    b = b < 0 ? 0 : b > 255 ? 255 : b;
                                                    a = a < 0 ? 0 : a > 255 ? 255 : a;
                                                    CurrentHudElements[Length - 1].OverlayColor = new World.ColorRGBA((byte)r, (byte)g, (byte)b, (byte)a);
                                                } break;
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "textcolor":
                                            if (Arguments.Length == 4) {
                                                int r, g, b, a;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out r)) {
                                                    System.Windows.Forms.MessageBox.Show("R is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[1], System.Globalization.NumberStyles.Integer, Culture, out g)) {
                                                    System.Windows.Forms.MessageBox.Show("G is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[2], System.Globalization.NumberStyles.Integer, Culture, out b)) {
                                                    System.Windows.Forms.MessageBox.Show("B is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[3], System.Globalization.NumberStyles.Integer, Culture, out a)) {
                                                    System.Windows.Forms.MessageBox.Show("A is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    r = r < 0 ? 0 : r > 255 ? 255 : r;
                                                    g = g < 0 ? 0 : g > 255 ? 255 : g;
                                                    b = b < 0 ? 0 : b > 255 ? 255 : b;
                                                    a = a < 0 ? 0 : a > 255 ? 255 : a;
                                                    CurrentHudElements[Length - 1].TextColor = new World.ColorRGBA((byte)r, (byte)g, (byte)b, (byte)a);
                                                } break;
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "textposition":
                                            if (Arguments.Length == 2) {
                                                float x, y;
                                                if (!float.TryParse(Arguments[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                    System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!float.TryParse(Arguments[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                    System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].TextPosition.X = x;
                                                    CurrentHudElements[Length - 1].TextPosition.Y = y;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "textalignment":
                                            if (Arguments.Length == 2) {
                                                int x, y;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out x)) {
                                                    System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!int.TryParse(Arguments[1], System.Globalization.NumberStyles.Integer, Culture, out y)) {
                                                    System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].TextAlignment.X = Math.Sign(x);
                                                    CurrentHudElements[Length - 1].TextAlignment.Y = Math.Sign(y);
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "textsize":
                                            if (Arguments.Length == 1) {
                                                int s;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out s)) {
                                                    System.Windows.Forms.MessageBox.Show("SIZE is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].TextSize = (Fonts.FontType)s;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "textshadow":
                                            if (Arguments.Length == 1) {
                                                int s;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out s)) {
                                                    System.Windows.Forms.MessageBox.Show("SHADOW is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].TextShadow = s != 0;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "text":
                                            if (Arguments.Length == 1) {
                                                CurrentHudElements[Length - 1].Text = Arguments[0];
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "value":
                                            if (Arguments.Length == 1) {
                                                int n;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out n)) {
                                                    System.Windows.Forms.MessageBox.Show("VALUE is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].Value = n;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "transition":
                                            if (Arguments.Length == 1) {
                                                int n;
                                                if (!int.TryParse(Arguments[0], System.Globalization.NumberStyles.Integer, Culture, out n)) {
                                                    System.Windows.Forms.MessageBox.Show("TRANSITION is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].Transition = (HudTransition)n;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        case "transitionvector":
                                            if (Arguments.Length == 2) {
                                                float x, y;
                                                if (!float.TryParse(Arguments[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                    System.Windows.Forms.MessageBox.Show("X is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else if (!float.TryParse(Arguments[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                    System.Windows.Forms.MessageBox.Show("Y is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                                } else {
                                                    CurrentHudElements[Length - 1].TransitionVector.X = x;
                                                    CurrentHudElements[Length - 1].TransitionVector.Y = y;
                                                }
                                            } else {
                                                System.Windows.Forms.MessageBox.Show("Incorrect number of arguments supplied in " + Command + " at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            } break;
                                        default:
                                            System.Windows.Forms.MessageBox.Show("Invalid command encountered at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                            break;
                                    }
                                } else {
                                    System.Windows.Forms.MessageBox.Show("Invalid statement encountered at line " + (i + 1).ToString(Culture) + " in " + FileOrNull);
                                }
                            }
                        }
                    }
                }
            }
            Array.Resize<HudElement>(ref CurrentHudElements, Length);
        }

        // ================================

        // encodings
        internal enum Encoding {
            Unknown = 0,
            Utf8 = 1,
            Utf16Le = 2,
            Utf16Be = 3,
            Utf32Le = 4,
            Utf32Be = 5,
        }
        internal static Encoding GetEncodingFromFile(string File) {
            try {
                byte[] Data = System.IO.File.ReadAllBytes(File);
                if (Data.Length >= 3) {
                    if (Data[0] == 0xEF & Data[1] == 0xBB & Data[2] == 0xBF) return Encoding.Utf8;
                }
                if (Data.Length >= 2) {
                    if (Data[0] == 0xFE & Data[1] == 0xFF) return Encoding.Utf16Be;
                    if (Data[0] == 0xFF & Data[1] == 0xFE) return Encoding.Utf16Le;
                }
                if (Data.Length >= 4) {
                    if (Data[0] == 0x00 & Data[1] == 0x00 & Data[2] == 0xFE & Data[3] == 0xFF) return Encoding.Utf32Be;
                    if (Data[0] == 0xFF & Data[1] == 0xFE & Data[2] == 0x00 & Data[3] == 0x00) return Encoding.Utf32Le;
                }
                return Encoding.Unknown;
            } catch {
                return Encoding.Unknown;
            }
        }
        internal static Encoding GetEncodingFromFile(string Folder, string File) {
            return GetEncodingFromFile(GetCombinedFileName(Folder, File));
        }

        // ================================

        // try parse vb6
        internal static bool TryParseDoubleVb6(string Expression, out double Value) {
            Expression = TrimInside(Expression);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            for (int n = Expression.Length; n > 0; n--) {
                double a;
                if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
                    Value = a;
                    return true;
                }
            }
            Value = 0.0;
            return false;
        }
        internal static bool TryParseFloatVb6(string Expression, out float Value) {
            Expression = TrimInside(Expression);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            for (int n = Expression.Length; n > 0; n--) {
                float a;
                if (float.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
                    Value = a;
                    return true;
                }
            }
            Value = 0.0f;
            return false;
        }
        internal static bool TryParseIntVb6(string Expression, out int Value) {
            Expression = TrimInside(Expression);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            for (int n = Expression.Length; n > 0; n--) {
                double a;
                if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
                    if (a >= -2147483648.0 & a <= 2147483647.0) {
                        Value = (int)Math.Round(a);
                        return true;
                    } else break;
                }
            }
            Value = 0;
            return false;
        }
        internal static bool TryParseByteVb6(string Expression, out byte Value) {
            Expression = TrimInside(Expression);
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            for (int n = Expression.Length; n > 0; n--) {
                double a;
                if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
                    if (a >= 0.0 & a <= 255.0) {
                        Value = (byte)Math.Round(a);
                        return true;
                    } else break;
                }
            }
            Value = 0;
            return false;
        }

        // try parse time
        internal static bool TryParseTime(string Expression, out double Value) {
            Expression = TrimInside(Expression);
            if (Expression.Length != 0) {
                System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
                int i = Expression.IndexOf('.');
                if (i >= 1) {
                    int h; if (int.TryParse(Expression.Substring(0, i), System.Globalization.NumberStyles.Integer, Culture, out h)) {
                        int n = Expression.Length - i - 1;
                        if (n == 1 | n == 2) {
                            uint m; if (uint.TryParse(Expression.Substring(i + 1, n), System.Globalization.NumberStyles.None, Culture, out m)) {
                                Value = 3600.0 * (double)h + 60.0 * (double)m;
                                return true;
                            }
                        } else if (n == 3 | n == 4) {
                            uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), System.Globalization.NumberStyles.None, Culture, out m)) {
                                uint s; if (uint.TryParse(Expression.Substring(i + 3, n - 2), System.Globalization.NumberStyles.None, Culture, out s)) {
                                    Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
                                    return true;
                                }
                            }
                        }
                    }
                } else if (i == -1) {
                    int h; if (int.TryParse(Expression, System.Globalization.NumberStyles.Integer, Culture, out h)) {
                        Value = 3600.0 * (double)h;
                        return true;
                    }
                }
            }
            Value = 0.0;
            return false;
        }

        // try parse hex color
        internal static bool TryParseHexColor(string Expression, out World.ColorRGB Color) {
            if (Expression.StartsWith("#")) {
                string a = Expression.Substring(1).TrimStart();
                int x; if (int.TryParse(a, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x)) {
                    int r = (x >> 16) & 0xFF;
                    int g = (x >> 8) & 0xFF;
                    int b = x & 0xFF;
                    if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
                        Color = new World.ColorRGB((byte)r, (byte)g, (byte)b);
                        return true;
                    } else {
                        Color = new World.ColorRGB(0, 0, 255);
                        return false;
                    }
                } else {
                    Color = new World.ColorRGB(0, 0, 255);
                    return false;
                }
            } else {
                Color = new World.ColorRGB(0, 0, 255);
                return false;
            }
        }
        internal static bool TryParseHexColor(string Expression, out World.ColorRGBA Color) {
            if (Expression.StartsWith("#")) {
                string a = Expression.Substring(1).TrimStart();
                int x; if (int.TryParse(a, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x)) {
                    int r = (x >> 16) & 0xFF;
                    int g = (x >> 8) & 0xFF;
                    int b = x & 0xFF;
                    if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
                        Color = new World.ColorRGBA((byte)r, (byte)g, (byte)b, 255);
                        return true;
                    } else {
                        Color = new World.ColorRGBA(0, 0, 255, 255);
                        return false;
                    }
                } else {
                    Color = new World.ColorRGBA(0, 0, 255, 255);
                    return false;
                }
            } else {
                Color = new World.ColorRGBA(0, 0, 255, 255);
                return false;
            }
        }

        // try parse vb6 (with unit factors)
        internal static bool TryParseDoubleVb6(string Expression, double[] UnitFactors, out double Value) {
            double a; if (double.TryParse(Expression, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out a)) {
                Value = a;
                return true;
            } else {
                int j = 0, n = 0; Value = 0;
                for (int i = 0; i < Expression.Length; i++) {
                    if (Expression[i] == ':') {
                        string t = Expression.Substring(j, i - j);
                        if (TryParseDoubleVb6(t, out a)) {
                            if (n < UnitFactors.Length) {
                                Value += a * UnitFactors[n];
                            } else {
                                return n > 0;
                            }
                        } else {
                            return n > 0;
                        } j = i + 1; n++;
                    }
                }
                {
                    string t = Expression.Substring(j);
                    if (TryParseDoubleVb6(t, out a)) {
                        if (n < UnitFactors.Length) {
                            Value += a * UnitFactors[n];
                            return true;
                        } else {
                            return n > 0;
                        }
                    } else {
                        return n > 0;
                    }
                }
            }
        }

        // trim inside
        private static string TrimInside(string Expression) {
            System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
            for (int i = 0; i < Expression.Length; i++) {
                char c = Expression[i];
                if (!char.IsWhiteSpace(c)) {
                    Builder.Append(c);
                }
            } return Builder.ToString();
        }

        // is japanese
        internal static bool IsJapanese(string Name) {
            for (int i = 0; i < Name.Length; i++) {
                int a = char.ConvertToUtf32(Name, i);
                if (a < 0x10000) {
                    bool q = false;
                    while (true) {
                        if (a >= 0x2E80 & a <= 0x2EFF) break;
                        if (a >= 0x3000 & a <= 0x30FF) break;
                        if (a >= 0x31C0 & a <= 0x4DBF) break;
                        if (a >= 0x4E00 & a <= 0x9FFF) break;
                        if (a >= 0xF900 & a <= 0xFAFF) break;
                        if (a >= 0xFE30 & a <= 0xFE4F) break;
                        if (a >= 0xFF00 & a <= 0xFFEF) break;
                        q = true; break;
                    } if (q) return false;
                } else {
                    return false;
                }
            } return true;
        }

        // unescape
        internal static string Unescape(string Text) {
            System.Text.StringBuilder Builder = new System.Text.StringBuilder(Text.Length);
            int Start = 0;
            for (int i = 0; i < Text.Length; i++) {
                if (Text[i] == '\\') {
                    Builder.Append(Text, Start, i - Start);
                    if (i + 1 <= Text.Length) {
                        switch (Text[i + 1]) {
                            case 'a': Builder.Append('\a'); break;
                            case 'b': Builder.Append('\b'); break;
                            case 't': Builder.Append('\t'); break;
                            case 'n': Builder.Append('\n'); break;
                            case 'v': Builder.Append('\v'); break;
                            case 'f': Builder.Append('\f'); break;
                            case 'r': Builder.Append('\r'); break;
                            case 'e': Builder.Append('\x1B'); break;
                            case 'c':
                                if (i + 2 < Text.Length) {
                                    int CodePoint = char.ConvertToUtf32(Text, i + 2);
                                    if (CodePoint >= 0x40 & CodePoint <= 0x5F) {
                                        Builder.Append(char.ConvertFromUtf32(CodePoint - 64));
                                    } else if (CodePoint == 0x3F) {
                                        Builder.Append('\x7F');
                                    } else {
                                        Interface.AddMessage(MessageType.Error, false, "Unrecognized control character found in " + Text.Substring(i, 3));
                                        return Text;
                                    } i++;
                                } else {
                                    Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode control character escape sequence");
                                    return Text;
                                } break;
                            case '"': Builder.Append('"'); break;
                            case '\\': Builder.Append('\\'); break;
                            case 'x':
                                if (i + 3 < Text.Length) {
                                    Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 2), 16)));
                                    i += 2;
                                } else {
                                    Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
                                    return Text;
                                } break;
                            case 'u':
                                if (i + 5 < Text.Length) {
                                    Builder.Append(char.ConvertFromUtf32(Convert.ToInt32(Text.Substring(i + 2, 4), 16)));
                                    i += 4;
                                } else {
                                    Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode hexadecimal escape sequence.");
                                    return Text;
                                } break;
                            default:
                                Interface.AddMessage(MessageType.Error, false, "Unrecognized escape sequence found in " + Text + ".");
                                return Text;
                        }
                        i++; Start = i + 1;
                    } else {
                        Interface.AddMessage(MessageType.Error, false, "Insufficient characters available in " + Text + " to decode escape sequence.");
                        return Text;
                    }
                }
            }
            Builder.Append(Text, Start, Text.Length - Start);
            return Builder.ToString();
        }

        // ================================

        // round to power of two
        internal static int RoundToPowerOfTwo(int Value) {
            Value -= 1;
            for (int i = 1; i < sizeof(int) * 8; i *= 2) {
                Value = Value | Value >> i;
            } return Value + 1;
        }

        // convert newlines to crlf
        internal static string ConvertNewlinesToCrLf(string Text) {
            System.Text.StringBuilder Builder = new System.Text.StringBuilder();
            for (int i = 0; i < Text.Length; i++) {
                int a = char.ConvertToUtf32(Text, i);
                if (a == 0xD & i < Text.Length - 1) {
                    int b = char.ConvertToUtf32(Text, i + 1);
                    if (b == 0xA) {
                        Builder.Append("\r\n");
                        i++;
                    } else {
                        Builder.Append("\r\n");
                    }
                } else if (a == 0xA | a == 0xC | a == 0xD | a == 0x85 | a == 0x2028 | a == 0x2029) {
                    Builder.Append("\r\n");
                } else if (a < 0x10000) {
                    Builder.Append(Text[i]);
                } else {
                    Builder.Append(Text.Substring(i, 2));
                    i++;
                }
            } return Builder.ToString();
        }

        // ================================

        // get corrected path separation
        internal static string GetCorrectedPathSeparation(string Expression) {
            if (Program.CurrentPlatform == Program.Platform.Windows) {
                if (Expression.Length != 0 && Expression[0] == '\\') {
                    return Expression.Substring(1);
                } else {
                    return Expression;
                }
            } else {
                if (Expression.Length != 0 && Expression[0] == '\\') {
                    return Expression.Substring(1).Replace("\\", new string(new char[] { System.IO.Path.DirectorySeparatorChar }));
                } else {
                    return Expression.Replace("\\", new string(new char[] { System.IO.Path.DirectorySeparatorChar }));
                }
            }
        }

        // get corected folder and file names
        internal static string GetCorrectedFolderName(string Folder) {
            if (Program.CurrentPlatform == Program.Platform.Linux) {
                /// find folder case-insensitively
                if (System.IO.Directory.Exists(Folder)) {
                    return Folder;
                } else {
                    string Parent = GetCorrectedFolderName(System.IO.Path.GetDirectoryName(Folder));
                    Folder = System.IO.Path.Combine(Parent, System.IO.Path.GetFileName(Folder));
                    if (Folder != null && System.IO.Directory.Exists(Parent)) {
                        if (System.IO.Directory.Exists(Folder)) {
                            return Folder;
                        } else {
                            string[] Folders = System.IO.Directory.GetDirectories(Parent);
                            for (int i = 0; i < Folders.Length; i++) {
                                if (string.Compare(Folder, Folders[i], StringComparison.OrdinalIgnoreCase) == 0) {
                                    return Folders[i];
                                }
                            }
                        }
                    }
                    return Folder;
                }
            } else {
                return Folder;
            }
        }
        internal static string GetCorrectedFileName(string File) {
            if (Program.CurrentPlatform == Program.Platform.Linux) {
                /// find file case-insensitively
                if (System.IO.File.Exists(File)) {
                    return File;
                } else {
                    string Folder = GetCorrectedFolderName(System.IO.Path.GetDirectoryName(File));
                    File = System.IO.Path.Combine(Folder, System.IO.Path.GetFileName(File));
                    if (System.IO.Directory.Exists(Folder)) {
                        if (System.IO.File.Exists(File)) {
                            return File;
                        } else {
                            string[] Files = System.IO.Directory.GetFiles(Folder);
                            for (int i = 0; i < Files.Length; i++) {
                                if (string.Compare(File, Files[i], StringComparison.OrdinalIgnoreCase) == 0) {
                                    return Files[i];
                                }
                            }
                        }
                    }
                    return File;
                }
            } else {
                return File;
            }
        }

        // get combined file name
        internal static string GetCombinedFileName(string SafeFolderPart, string UnsafeFilePart) {
            return GetCorrectedFileName(System.IO.Path.Combine(SafeFolderPart, GetCorrectedPathSeparation(UnsafeFilePart)));
        }
        // get combined folder name
        internal static string GetCombinedFolderName(string SafeFolderPart, string UnsafeFolderPart) {
            return GetCorrectedFolderName(System.IO.Path.Combine(SafeFolderPart, GetCorrectedPathSeparation(UnsafeFolderPart)));
        }

    }
}