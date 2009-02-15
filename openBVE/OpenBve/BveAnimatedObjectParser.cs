using System;

namespace OpenBve {
    internal static class BveAnimatedObjectParser {

        // parse animated object config
        internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode) {
            System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
            ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
            Result.Objects = new ObjectManager.AnimatedObject[] { };
            int n = 0;
            // load file
            string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
            for (int i = 0; i < Lines.Length; i++) {
                Lines[i] = Lines[i].Trim();
                int h = Lines[i].IndexOf(';');
                if (h >= 0) {
                    Lines[i] = Lines[i].Substring(0, h).TrimEnd();
                }
            }
            for (int i = 0; i < Lines.Length; i++) {
                if (Lines[i].Length != 0) {
                    switch (Lines[i].ToLowerInvariant()) {
                        case "[object]":
                            i++;
                            Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, n + 1);
                            Result.Objects[n] = new ObjectManager.AnimatedObject();
                            Result.Objects[n].States = new ObjectManager.AnimatedObjectState[] { };
                            Result.Objects[n].TranslateXDirection = new World.Vector3D(1.0, 0.0, 0.0);
                            Result.Objects[n].TranslateYDirection = new World.Vector3D(0.0, 1.0, 0.0);
                            Result.Objects[n].TranslateZDirection = new World.Vector3D(0.0, 0.0, 1.0);
                            Result.Objects[n].RotateXDirection = new World.Vector3D(1.0, 0.0, 0.0);
                            Result.Objects[n].RotateYDirection = new World.Vector3D(0.0, 1.0, 0.0);
                            Result.Objects[n].RotateZDirection = new World.Vector3D(0.0, 0.0, 1.0);
                            Result.Objects[n].TextureShiftXDirection = new World.Vector2D(1.0, 0.0);
                            Result.Objects[n].TextureShiftYDirection = new World.Vector2D(0.0, 1.0);
                            Result.Objects[n].RefreshRate = 0.0;
                            Result.Objects[n].ObjectIndex = -1;
                            World.Vector3D Position = new World.Vector3D(0.0, 0.0, 0.0);
                            while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.OrdinalIgnoreCase))) {
                                if (Lines[i].Length != 0) {
                                    int j = Lines[i].IndexOf("=", StringComparison.OrdinalIgnoreCase);
                                    if (j > 0) {
                                        string a = Lines[i].Substring(0, j).TrimEnd();
                                        string b = Lines[i].Substring(j + 1).TrimStart();
                                        switch (a.ToLowerInvariant()) {
                                            case "position": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length == 3) {
                                                        double x, y, z;
                                                        if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            Position = new World.Vector3D(x, y, z);
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    }
                                                } break;
                                            case "states": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length >= 1) {
                                                        string Folder = System.IO.Path.GetDirectoryName(FileName);
                                                        Result.Objects[n].States = new ObjectManager.AnimatedObjectState[s.Length];
                                                        for (int k = 0; k < s.Length; k++) {
                                                            string f = Interface.GetCombinedFileName(Folder, s[k].Trim());
                                                            Result.Objects[n].States[k].Position = new World.Vector3D(0.0, 0.0, 0.0);
                                                            if (System.IO.File.Exists(f)) {
                                                                Result.Objects[n].States[k].Object = ObjectManager.LoadStaticObject(f, Encoding, LoadMode, false, true);
                                                            } else {
                                                                Interface.AddMessage(Interface.MessageType.Error, true, "Object file " + f + " not found in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                            }
                                                            if (Result.Objects[n].States[k].Object == null) {
                                                                Result.Objects[n].States[k].Object = new ObjectManager.StaticObject();
                                                                Result.Objects[n].States[k].Object.Meshes = new World.Mesh[] { };
                                                            }
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "At least one argument is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        return null;
                                                    }
                                                } break;
                                            case "statefunction":
                                                try {
                                                    Result.Objects[n].StateFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "statefunctionrpn":
                                                try {
                                                    Result.Objects[n].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translatexdirection":
                                            case "translateydirection":
                                            case "translatezdirection": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length == 3) {
                                                        double x, y, z;
                                                        if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            switch (a.ToLowerInvariant()) {
                                                                case "translatexdirection":
                                                                    Result.Objects[n].TranslateXDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                                case "translateydirection":
                                                                    Result.Objects[n].TranslateYDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                                case "translatezdirection":
                                                                    Result.Objects[n].TranslateZDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    }
                                                } break;
                                            case "translatexfunction":
                                                try {
                                                    Result.Objects[n].TranslateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translateyfunction":
                                                try {
                                                    Result.Objects[n].TranslateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translatezfunction":
                                                try {
                                                    Result.Objects[n].TranslateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translatexfunctionrpn":
                                                try {
                                                    Result.Objects[n].TranslateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translateyfunctionrpn":
                                                try {
                                                    Result.Objects[n].TranslateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "translatezfunctionrpn":
                                                try {
                                                    Result.Objects[n].TranslateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotatexdirection":
                                            case "rotateydirection":
                                            case "rotatezdirection": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length == 3) {
                                                        double x, y, z;
                                                        if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[2], System.Globalization.NumberStyles.Float, Culture, out z)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Z is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (x == 0.0 & y == 0.0 & z == 0.0) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "The direction indicated by X, Y and Z is expected to be non-zero in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            switch (a.ToLowerInvariant()) {
                                                                case "rotatexdirection":
                                                                    Result.Objects[n].RotateXDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                                case "rotateydirection":
                                                                    Result.Objects[n].RotateYDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                                case "rotatezdirection":
                                                                    Result.Objects[n].RotateZDirection = new World.Vector3D(x, y, z);
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    }
                                                } break;
                                            case "rotatexfunction":
                                                try {
                                                    Result.Objects[n].RotateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotateyfunction":
                                                try {
                                                    Result.Objects[n].RotateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotatezfunction":
                                                try {
                                                    Result.Objects[n].RotateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotatexfunctionrpn":
                                                try {
                                                    Result.Objects[n].RotateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotateyfunctionrpn":
                                                try {
                                                    Result.Objects[n].RotateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotatezfunctionrpn":
                                                try {
                                                    Result.Objects[n].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "rotatexdamping":
                                            case "rotateydamping":
                                            case "rotatezdamping": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length == 2) {
                                                        double nf, dr;
                                                        if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out nf)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out dr)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (nf <= 0.0) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "NaturalFrequency is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (dr <= 0.0) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "DampingRatio is expected to be positive in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            switch (a.ToLowerInvariant()) {
                                                                case "rotatexdamping":
                                                                    Result.Objects[n].RotateXDamping = new ObjectManager.Damping(nf, dr);
                                                                    break;
                                                                case "rotateydamping":
                                                                    Result.Objects[n].RotateYDamping = new ObjectManager.Damping(nf, dr);
                                                                    break;
                                                                case "rotatezdamping":
                                                                    Result.Objects[n].RotateZDamping = new ObjectManager.Damping(nf, dr);
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    }
                                                } break;
                                            case "textureshiftxdirection":
                                            case "textureshiftydirection": {
                                                    string[] s = b.Split(',');
                                                    if (s.Length == 2) {
                                                        double x, y;
                                                        if (!double.TryParse(s[0], System.Globalization.NumberStyles.Float, Culture, out x)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "X is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else if (!double.TryParse(s[1], System.Globalization.NumberStyles.Float, Culture, out y)) {
                                                            Interface.AddMessage(Interface.MessageType.Error, false, "Y is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                        } else {
                                                            switch (a.ToLowerInvariant()) {
                                                                case "textureshiftxdirection":
                                                                    Result.Objects[n].TextureShiftXDirection = new World.Vector2D(x, y);
                                                                    break;
                                                                case "textureshiftydirection":
                                                                    Result.Objects[n].TextureShiftYDirection = new World.Vector2D(x, y);
                                                                    break;
                                                            }
                                                        }
                                                    } else {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    }
                                                } break;
                                            case "textureshiftxfunction":
                                                try {
                                                    Result.Objects[n].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "textureshiftyfunction":
                                                try {
                                                    Result.Objects[n].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "textureshiftxfunctionrpn":
                                                try {
                                                    Result.Objects[n].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "textureshiftyfunctionrpn":
                                                try {
                                                    Result.Objects[n].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
                                                } catch (Exception ex) {
                                                    Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                } break;
                                            case "refreshrate": {
                                                    double r;
                                                    if (!double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out r)) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    } else if (r < 0.0) {
                                                        Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be non-negative in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                    } else {
                                                        Result.Objects[n].RefreshRate = r;
                                                    }
                                                } break;
                                            default:
                                                Interface.AddMessage(Interface.MessageType.Error, false, "The attribute " + a + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                                break;
                                        }
                                    } else {
                                        Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                                        return null;
                                    }
                                } i++;
                            } i--;
                            for (int j = 0; j < Result.Objects[n].States.Length; j++) {
                                Result.Objects[n].States[j].Position = Position;
                            } n++;
                            break;
                        default:
                            Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
                            return null;
                    }
                }
            }
            return Result;
        }

    }
}