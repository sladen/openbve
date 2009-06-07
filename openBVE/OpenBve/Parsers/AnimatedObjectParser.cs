using System;

namespace OpenBve {
	internal static class AnimatedObjectParser {

		// parse animated object config
		/// <summary>Loads a collection of animated objects from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <returns>The collection of animated objects.</returns>
		internal static ObjectManager.AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection();
			Result.Objects = new ObjectManager.AnimatedObject[] { };
			int Objects = 0;
			// load file
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			for (int i = 0; i < Lines.Length; i++) {
				int j = Lines[i].IndexOf(';');
				if (j >= 0) {
					Lines[i] = Lines[i].Substring(0, j).Trim();
				} else {
					Lines[i] = Lines[i].Trim();
				}
			}
			for (int i = 0; i < Lines.Length; i++) {
				if (Lines[i].Length != 0) {
					switch (Lines[i].ToLowerInvariant()) {
						case "[object]":
							i++;
							Array.Resize<ObjectManager.AnimatedObject>(ref Result.Objects, Objects + 1);
							Result.Objects[Objects] = new ObjectManager.AnimatedObject();
							Result.Objects[Objects].States = new ObjectManager.AnimatedObjectState[] { };
							Result.Objects[Objects].TranslateXDirection = new World.Vector3D(1.0, 0.0, 0.0);
							Result.Objects[Objects].TranslateYDirection = new World.Vector3D(0.0, 1.0, 0.0);
							Result.Objects[Objects].TranslateZDirection = new World.Vector3D(0.0, 0.0, 1.0);
							Result.Objects[Objects].RotateXDirection = new World.Vector3D(1.0, 0.0, 0.0);
							Result.Objects[Objects].RotateYDirection = new World.Vector3D(0.0, 1.0, 0.0);
							Result.Objects[Objects].RotateZDirection = new World.Vector3D(0.0, 0.0, 1.0);
							Result.Objects[Objects].TextureShiftXDirection = new World.Vector2D(1.0, 0.0);
							Result.Objects[Objects].TextureShiftYDirection = new World.Vector2D(0.0, 1.0);
							Result.Objects[Objects].RefreshRate = 0.0;
							Result.Objects[Objects].ObjectIndex = -1;
							World.Vector3D Position = new World.Vector3D(0.0, 0.0, 0.0);
							string[] StateFiles = null;
							while (i < Lines.Length && !(Lines[i].StartsWith("[", StringComparison.Ordinal) & Lines[i].EndsWith("]", StringComparison.Ordinal))) {
								if (Lines[i].Length != 0) {
									int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
									if (j > 0) {
										string a = Lines[i].Substring(0, j).TrimEnd();
										string b = Lines[i].Substring(j + 1).TrimStart();
										switch (a.ToLowerInvariant()) {
											case "position":
												{
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
											case "states":
												{
													string[] s = b.Split(',');
													if (s.Length >= 1) {
														string Folder = System.IO.Path.GetDirectoryName(FileName);
														StateFiles = new string[s.Length];
														for (int k = 0; k < s.Length; k++) {
															s[k] = s[k].Trim();
															if (Interface.ContainsInvalidPathChars(s[k])) {
																Interface.AddMessage(Interface.MessageType.Error, false, "File" + k.ToString(Culture) + " contains illegal characters in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																StateFiles[k] = null;
															} else {
																StateFiles[k] = Interface.GetCombinedFileName(Folder, s[k]);
																if (!System.IO.File.Exists(StateFiles[k])) {
																	Interface.AddMessage(Interface.MessageType.Error, true, "File " + StateFiles[k] + " not found in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	StateFiles[k] = null;
																}
															}
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "At least one argument is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														return null;
													}
												} break;
											case "statefunction":
												try {
													Result.Objects[Objects].StateFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "statefunctionrpn":
												try {
													Result.Objects[Objects].StateFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translatexdirection":
											case "translateydirection":
											case "translatezdirection":
												{
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
																	Result.Objects[Objects].TranslateXDirection = new World.Vector3D(x, y, z);
																	break;
																case "translateydirection":
																	Result.Objects[Objects].TranslateYDirection = new World.Vector3D(x, y, z);
																	break;
																case "translatezdirection":
																	Result.Objects[Objects].TranslateZDirection = new World.Vector3D(x, y, z);
																	break;
															}
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} break;
											case "translatexfunction":
												try {
													Result.Objects[Objects].TranslateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translateyfunction":
												try {
													Result.Objects[Objects].TranslateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translatezfunction":
												try {
													Result.Objects[Objects].TranslateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translatexfunctionrpn":
												try {
													Result.Objects[Objects].TranslateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translateyfunctionrpn":
												try {
													Result.Objects[Objects].TranslateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "translatezfunctionrpn":
												try {
													Result.Objects[Objects].TranslateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotatexdirection":
											case "rotateydirection":
											case "rotatezdirection":
												{
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
																	Result.Objects[Objects].RotateXDirection = new World.Vector3D(x, y, z);
																	break;
																case "rotateydirection":
																	Result.Objects[Objects].RotateYDirection = new World.Vector3D(x, y, z);
																	break;
																case "rotatezdirection":
																	Result.Objects[Objects].RotateZDirection = new World.Vector3D(x, y, z);
																	break;
															}
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 3 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} break;
											case "rotatexfunction":
												try {
													Result.Objects[Objects].RotateXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotateyfunction":
												try {
													Result.Objects[Objects].RotateYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotatezfunction":
												try {
													Result.Objects[Objects].RotateZFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotatexfunctionrpn":
												try {
													Result.Objects[Objects].RotateXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotateyfunctionrpn":
												try {
													Result.Objects[Objects].RotateYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotatezfunctionrpn":
												try {
													Result.Objects[Objects].RotateZFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "rotatexdamping":
											case "rotateydamping":
											case "rotatezdamping":
												{
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
																	Result.Objects[Objects].RotateXDamping = new ObjectManager.Damping(nf, dr);
																	break;
																case "rotateydamping":
																	Result.Objects[Objects].RotateYDamping = new ObjectManager.Damping(nf, dr);
																	break;
																case "rotatezdamping":
																	Result.Objects[Objects].RotateZDamping = new ObjectManager.Damping(nf, dr);
																	break;
															}
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} break;
											case "textureshiftxdirection":
											case "textureshiftydirection":
												{
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
																	Result.Objects[Objects].TextureShiftXDirection = new World.Vector2D(x, y);
																	break;
																case "textureshiftydirection":
																	Result.Objects[Objects].TextureShiftYDirection = new World.Vector2D(x, y);
																	break;
															}
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Exactly 2 arguments are expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} break;
											case "textureshiftxfunction":
												try {
													Result.Objects[Objects].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "textureshiftyfunction":
												try {
													Result.Objects[Objects].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromInfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "textureshiftxfunctionrpn":
												try {
													Result.Objects[Objects].TextureShiftXFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "textureshiftyfunctionrpn":
												try {
													Result.Objects[Objects].TextureShiftYFunction = FunctionScripts.GetFunctionScriptFromPostfixNotation(b);
												} catch (Exception ex) {
													Interface.AddMessage(Interface.MessageType.Error, false, ex.Message + " in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												} break;
											case "refreshrate":
												{
													double r;
													if (!double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out r)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Value is invalid in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else if (r < 0.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be non-negative in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														Result.Objects[Objects].RefreshRate = r;
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
								}
								i++;
							}
							i--;
							if (StateFiles != null) {
								Result.Objects[Objects].States = new ObjectManager.AnimatedObjectState[StateFiles.Length];
								bool ForceTextureRepeatX = Result.Objects[Objects].TextureShiftXFunction != null & Result.Objects[Objects].TextureShiftXDirection.X != 0.0 |
									Result.Objects[Objects].TextureShiftYFunction != null & Result.Objects[Objects].TextureShiftYDirection.Y != 0.0;
								bool ForceTextureRepeatY = Result.Objects[Objects].TextureShiftXFunction != null & Result.Objects[Objects].TextureShiftXDirection.X != 0.0 |
									Result.Objects[Objects].TextureShiftYFunction != null & Result.Objects[Objects].TextureShiftYDirection.Y != 0.0;
								for (int k = 0; k < StateFiles.Length; k++) {
									Result.Objects[Objects].States[k].Position = new World.Vector3D(0.0, 0.0, 0.0);
									if (StateFiles[k] != null) {
										Result.Objects[Objects].States[k].Object = ObjectManager.LoadStaticObject(StateFiles[k], Encoding, LoadMode, false, ForceTextureRepeatX, ForceTextureRepeatY);
									} else {
										Result.Objects[Objects].States[k].Object = null;
									}
									for (int j = 0; j < Result.Objects[Objects].States.Length; j++) {
										Result.Objects[Objects].States[j].Position = Position;
									}
								}
							} else {
								Result.Objects[Objects].States = new ObjectManager.AnimatedObjectState[] { };
							}
							Objects++;
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