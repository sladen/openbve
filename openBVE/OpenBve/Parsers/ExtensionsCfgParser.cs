using System;

namespace OpenBve {
	internal static class ExtensionsCfgParser {

		// parse extensions config
		internal static void ParseExtensionsConfig(string TrainPath, System.Text.Encoding Encoding, out ObjectManager.UnifiedObject[] CarObjects, TrainManager.Train Train) {
			CarObjects = new ObjectManager.UnifiedObject[Train.Cars.Length];
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string FileName = Interface.GetCombinedFileName(TrainPath, "extensions.cfg");
			if (System.IO.File.Exists(FileName)) {
				// load file
				string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
				for (int i = 0; i < Lines.Length; i++) {
					Lines[i] = Lines[i].Trim();
					int h = Lines[i].IndexOf(";");
					if (h >= 0) {
						Lines[i].Substring(0, h).TrimEnd();
					}
					if (Lines[i].Length != 0) {
						switch (Lines[i].ToLowerInvariant()) {
							case "[exterior]":
								// exterior
								i++;
								while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									if (Lines[i].Length != 0) {
										int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
										if (j >= 0) {
											string a = Lines[i].Substring(0, j).TrimEnd();
											string b = Lines[i].Substring(j + 1).TrimStart();
											int n;
											if (int.TryParse(a, System.Globalization.NumberStyles.Integer, Culture, out n)) {
												if (n >= 0 & n < Train.Cars.Length) {
													if (Interface.ContainsInvalidPathChars(b)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													} else {
														string File = Interface.GetCombinedFileName(TrainPath, b);
														if (System.IO.File.Exists(File)) {
															CarObjects[n] = ObjectManager.LoadObject(File, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
														} else {
															Interface.AddMessage(Interface.MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
														}
													}
												} else {
													Interface.AddMessage(Interface.MessageType.Error, false, "The car index " + a + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												}
											} else {
												Interface.AddMessage(Interface.MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											}
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									}
									i++;
								}
								i--;
								break;
							default:
								if (Lines[i].StartsWith("[car", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// car
									string t = Lines[i].Substring(4, Lines[i].Length - 5);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Cars.Length) {
											bool DefinedLength = false;
											bool DefinedAxles = false;
											i++;
											while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "object":
																if (Interface.ContainsInvalidPathChars(b)) {
																	Interface.AddMessage(Interface.MessageType.Error, false, "File contains illegal characters at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																} else {
																	string File = Interface.GetCombinedFileName(TrainPath, b);
																	if (System.IO.File.Exists(File)) {
																		CarObjects[n] = ObjectManager.LoadObject(File, Encoding, ObjectManager.ObjectLoadMode.Normal, false, false);
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, true, "The car object " + File + " does not exist at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																}
																break;
															case "length":
																{
																	double m;
																	if (double.TryParse(b, System.Globalization.NumberStyles.Float, Culture, out m)) {
																		if (m > 0.0) {
																			Train.Cars[n].Length = m;
																			DefinedLength = true;
																		} else {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "Value is expected to be a positive floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															case "axles":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double x, y;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out x)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Rear is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out y)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Front is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (x >= y) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Rear is expected to be less than Front in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Cars[n].RearAxlePosition = x;
																			Train.Cars[n].FrontAxlePosition = y;
																			DefinedAxles = true;
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															default:
																Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} i++;
											} i--;
											if (DefinedLength & !DefinedAxles) {
												double AxleDistance = 0.4 * Train.Cars[n].Length;
												Train.Cars[n].RearAxlePosition = -AxleDistance;
												Train.Cars[n].FrontAxlePosition = AxleDistance;
											}
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "The car index " + t + " does not reference an existing car at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "The car index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else if (Lines[i].StartsWith("[coupler", StringComparison.OrdinalIgnoreCase) & Lines[i].EndsWith("]", StringComparison.Ordinal)) {
									// coupler
									string t = Lines[i].Substring(8, Lines[i].Length - 9);
									int n; if (int.TryParse(t, System.Globalization.NumberStyles.Integer, Culture, out n)) {
										if (n >= 0 & n < Train.Couplers.Length) {
											i++; while (i < Lines.Length && !Lines[i].StartsWith("[", StringComparison.Ordinal) & !Lines[i].EndsWith("]", StringComparison.Ordinal)) {
												if (Lines[i].Length != 0) {
													int j = Lines[i].IndexOf("=", StringComparison.Ordinal);
													if (j >= 0) {
														string a = Lines[i].Substring(0, j).TrimEnd();
														string b = Lines[i].Substring(j + 1).TrimStart();
														switch (a.ToLowerInvariant()) {
															case "distances":
																{
																	int k = b.IndexOf(',');
																	if (k >= 0) {
																		string c = b.Substring(0, k).TrimEnd();
																		string d = b.Substring(k + 1).TrimStart();
																		double x, y;
																		if (!double.TryParse(c, System.Globalization.NumberStyles.Float, Culture, out x)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (!double.TryParse(d, System.Globalization.NumberStyles.Float, Culture, out y)) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Maximum is expected to be a floating-point number in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else if (x > y) {
																			Interface.AddMessage(Interface.MessageType.Error, false, "Minimum is expected to be less than Maximum in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																		} else {
																			Train.Couplers[n].MinimumDistanceBetweenCars = x;
																			Train.Couplers[n].MaximumDistanceBetweenCars = y;
																		}
																	} else {
																		Interface.AddMessage(Interface.MessageType.Error, false, "An argument-separating comma is expected in " + a + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																	}
																} break;
															default:
																Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported key-value pair " + a + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
																break;
														}
													} else {
														Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													}
												} i++;
											} i--;
										} else {
											Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index " + t + " does not reference an existing coupler at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "The coupler index is expected to be an integer at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else {
									// default
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid statement " + Lines[i] + " encountered at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} break;
						}
					}
				}
			}
		}

	}
}