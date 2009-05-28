using System;

namespace OpenBve {
	internal static class CsvB3dObjectParser {

		// structures
		private class Material {
			internal World.ColorRGBA Color;
			internal World.ColorRGB EmissiveColor;
			internal bool EmissiveColorUsed;
			internal World.ColorRGB TransparentColor;
			internal bool TransparentColorUsed;
			internal string DaytimeTexture;
			internal string NighttimeTexture;
			internal World.MeshMaterialBlendMode BlendMode;
			internal ushort GlowAttenuationData;
			internal Material() {
				this.Color = new World.ColorRGBA(255, 255, 255, 255);
				this.EmissiveColor = new World.ColorRGB(0, 0, 0);
				this.EmissiveColorUsed = false;
				this.TransparentColor = new World.ColorRGB(0, 0, 0);
				this.TransparentColorUsed = false;
				this.DaytimeTexture = null;
				this.NighttimeTexture = null;
				this.BlendMode = World.MeshMaterialBlendMode.Normal;
				this.GlowAttenuationData = 0;
			}
			internal Material(Material Prototype) {
				this.Color = Prototype.Color;
				this.EmissiveColor = Prototype.EmissiveColor;
				this.EmissiveColorUsed = Prototype.EmissiveColorUsed;
				this.TransparentColor = Prototype.TransparentColor;
				this.TransparentColorUsed = Prototype.TransparentColorUsed;
				this.DaytimeTexture = Prototype.DaytimeTexture;
				this.NighttimeTexture = Prototype.NighttimeTexture;
				this.BlendMode = Prototype.BlendMode;
				this.GlowAttenuationData = Prototype.GlowAttenuationData;
			}
		}
		private class MeshBuilder {
			internal World.Vertex[] Vertices;
			internal World.MeshFace[] Faces;
			internal Material[] Materials;
			internal MeshBuilder() {
				this.Vertices = new World.Vertex[] { };
				this.Faces = new World.MeshFace[] { };
				this.Materials = new Material[] { new Material() };
			}
		}

		// read object
		/// <summary>Loads a CSV or B3D object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <param name="ForceTextureRepeat">Whether to force TextureWrapMode.Repeat for referenced textures.</param>
		/// <returns>The object loaded.</returns>
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeat) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			bool IsB3D = string.Equals(System.IO.Path.GetExtension(FileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			// initialize object
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new World.Vertex[] { };
			// read lines
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			// parse lines
			MeshBuilder Builder = new MeshBuilder();
			for (int i = 0; i < Lines.Length; i++) {
				{ // strip away comments
					int j = Lines[i].IndexOf(';');
					if (j >= 0) {
						Lines[i] = Lines[i].Substring(0, j).TrimEnd();
					}
				}
				// collect arguments
				string[] Arguments = Lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
				for (int j = 0; j < Arguments.Length; j++) {
					Arguments[j] = Arguments[j].Trim();
				}
				{ // remove unused Arguments at the end of the chain
					int j;
					for (j = Arguments.Length - 1; j >= 0; j--) {
						if (Arguments[j].Length != 0) break;
					}
					Array.Resize<string>(ref Arguments, j + 1);
				}
				// style
				string Command;
				if (IsB3D & Arguments.Length != 0) {
					// b3d
					int j = Arguments[0].IndexOf(' ');
					if (j >= 0) {
						Command = Arguments[0].Substring(0, j).TrimEnd();
						Arguments[0] = Arguments[0].Substring(j + 1).TrimStart();
					} else {
						Command = Arguments[0];
						Arguments = new string[] { };
					}
				} else if (Arguments.Length != 0) {
					// csv
					Command = Arguments[0];
					for (int j = 0; j < Arguments.Length - 1; j++) {
						Arguments[j] = Arguments[j + 1];
					}
					Array.Resize<string>(ref Arguments, Arguments.Length - 1);
				} else {
					// empty
					Command = null;
				}
				// parse terms
				if (Command != null) {
					switch (Command.ToLowerInvariant()) {
						case "createmeshbuilder":
						case "[meshbuilder]":
							{
								if (Arguments.Length > 0) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "0 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								ApplyMeshBuilder(ref Object, Builder, LoadMode, ForceTextureRepeat);
								Builder = new MeshBuilder();
							} break;
						case "addvertex":
						case "vertex":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								Array.Resize<World.Vertex>(ref Builder.Vertices, Builder.Vertices.Length + 1);
								Builder.Vertices[Builder.Vertices.Length - 1].Coordinates = new World.Vector3D(x, y, z);
							} break;
						case "addface":
						case "addface2":
						case "face":
						case "face2":
							{
								if (Arguments.Length < 3) {
									Interface.AddMessage(Interface.MessageType.Error, false, "At least 3 arguments are required in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									bool q = true;
									int[] a = new int[Arguments.Length];
									for (int j = 0; j < Arguments.Length; j++) {
										if (!Interface.TryParseIntVb6(Arguments[j], out a[j])) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] < 0 | a[j] >= Builder.Vertices.Length) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " references a non-existing vertex " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] > 65535) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " indexes a vertex index above 65535 which the current implementation does not support in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										}
									}
									if (q) {
										int f = Builder.Faces.Length;
										Array.Resize<World.MeshFace>(ref Builder.Faces, f + 1);
										Builder.Faces[f] = new World.MeshFace();
										Builder.Faces[f].Vertices = new World.MeshFaceVertex[Arguments.Length];
										for (int j = 0; j < Arguments.Length; j++) {
											Builder.Faces[f].Vertices[j].Index = (ushort)a[j];
											Builder.Faces[f].Vertices[j].Normal = new World.Vector3Df(0.0f, 0.0f, 0.0f);
										}
										switch (Command.ToLowerInvariant()) {
											case "face2":
											case "addface2":
												Builder.Faces[f].Flags = (byte)World.MeshFace.Face2Mask;
												break;
											default:
												Builder.Faces[f].Flags = 0;
												break;
										}
									}
								}
							} break;
						case "cube":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfWidth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								double y = x, z = x;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfHeight in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfDepth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								CreateCube(ref Builder, x, y, z);
							} break;
						case "cylinder":
							{
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int n = 8;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out n)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument n in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								if (n < 2) {
									Interface.AddMessage(Interface.MessageType.Error, false, "n is expected to be at least 2 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								double r1 = 0.0, r2 = 0.0, h = 1.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out r1)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument UpperRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r1 = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out r2)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument LowerRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r2 = 1.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], out h)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Height in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									h = 1.0;
								}
								CreateCylinder(ref Builder, n, r1, r2, h);
							} break;
						case "translate":
						case "translateall":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								ApplyTranslation(Builder, x, y, z);
								if (Command.ToLowerInvariant() == "translateall") {
									ApplyTranslation(Object, x, y, z);
								}
							} break;
						case "scale":
						case "scaleall":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 1.0, y = 1.0, z = 1.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								} else if (x == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "X is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								} else if (y == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Y is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								} else if (z == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Z is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								ApplyScale(Builder, x, y, z);
								if (Command.ToLowerInvariant() == "scaleall") {
									ApplyScale(Object, x, y, z);
								}
							} break;
						case "rotate":
						case "rotateall":
							{
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0, a = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[3], out a)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Angle in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 0.0;
								}
								double t = x * x + y * y + z * z;
								if (t == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "The direction indicated by X, Y and Z is expected to be non-zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
									y = 0.0;
									z = 0.0;
									t = 1.0;
								}
								if (a != 0.0) {
									a *= 0.0174532925199433;
									t = 1.0 / t;
									x *= t; y *= t; z *= t;
									ApplyRotation(Builder, x, y, z, a);
									if (Command.ToLowerInvariant() == "rotateall") {
										ApplyRotation(Object, x, y, z, a);
									}
								}
							} break;
						case "generatenormals":
						case "[texture]":
							break;
						case "setcolor":
						case "color":
							{
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !Interface.TryParseIntVb6(Arguments[3], out a)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Alpha in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255;
								} else if (a < 0 | a > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Alpha is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = a < 0 ? 0 : 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize<Material>(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m]);
									Builder.Materials[j].Color = new World.ColorRGBA((byte)r, (byte)g, (byte)b, (byte)a);
									Builder.Materials[j].BlendMode = Builder.Materials[0].BlendMode;
									Builder.Materials[j].GlowAttenuationData = Builder.Materials[0].GlowAttenuationData;
									Builder.Materials[j].DaytimeTexture = Builder.Materials[0].DaytimeTexture;
									Builder.Materials[j].NighttimeTexture = Builder.Materials[0].NighttimeTexture;
									Builder.Materials[j].TransparentColor = Builder.Materials[0].TransparentColor;
									Builder.Materials[j].TransparentColorUsed = Builder.Materials[0].TransparentColorUsed;
								}
								for (int j = 0; j < Builder.Faces.Length; j++) {
									Builder.Faces[j].Material += (ushort)m;
								}
							} break;
						case "setemissivecolor":
						case "emissivecolor":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize<Material>(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m]);
									Builder.Materials[j].EmissiveColor = new World.ColorRGB((byte)r, (byte)g, (byte)b);
									Builder.Materials[j].EmissiveColorUsed = true;
									Builder.Materials[j].BlendMode = Builder.Materials[0].BlendMode;
									Builder.Materials[j].GlowAttenuationData = Builder.Materials[0].GlowAttenuationData;
									Builder.Materials[j].DaytimeTexture = Builder.Materials[0].DaytimeTexture;
									Builder.Materials[j].NighttimeTexture = Builder.Materials[0].NighttimeTexture;
									Builder.Materials[j].TransparentColor = Builder.Materials[0].TransparentColor;
									Builder.Materials[j].TransparentColorUsed = Builder.Materials[0].TransparentColorUsed;
								}
								for (int j = 0; j < Builder.Faces.Length; j++) {
									Builder.Faces[j].Material += (ushort)m;
								}
							} break;
						case "setdecaltransparentcolor":
						case "transparent":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].TransparentColor = new World.ColorRGB((byte)r, (byte)g, (byte)b);
									Builder.Materials[j].TransparentColorUsed = true;
								}
							} break;
						case "setblendmode":
						case "blendmode":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								World.MeshMaterialBlendMode blendmode = World.MeshMaterialBlendMode.Normal;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
									switch (Arguments[0].ToLowerInvariant()) {
											case "normal": blendmode = World.MeshMaterialBlendMode.Normal; break;
											case "additive": blendmode = World.MeshMaterialBlendMode.Additive; break;
										default:
											Interface.AddMessage(Interface.MessageType.Error, false, "BlendMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											blendmode = World.MeshMaterialBlendMode.Normal;
											break;
									}
								}
								double glowhalfdistance = 0.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseDoubleVb6(Arguments[1], out glowhalfdistance)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument GlowHalfDistance in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									glowhalfdistance = 0;
								}
								World.GlowAttenuationMode glowmode = World.GlowAttenuationMode.DivisionExponent4;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
									switch (Arguments[2].ToLowerInvariant()) {
											case "divideexponent2": glowmode = World.GlowAttenuationMode.DivisionExponent2; break;
											case "divideexponent4": glowmode = World.GlowAttenuationMode.DivisionExponent4; break;
										default:
											Interface.AddMessage(Interface.MessageType.Error, false, "The indicated GlowAttenuationMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].BlendMode = blendmode;
									Builder.Materials[j].GlowAttenuationData = World.GetGlowAttenuationData(glowhalfdistance, glowmode);
								}
							} break;
						case "loadtexture":
						case "load":
							{
								if (Arguments.Length > 2) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 2 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								string tday = null, tnight = null;
								if (Arguments.Length >= 1 && Arguments[0].Length != 0) {
									if (Interface.ContainsInvalidPathChars(Arguments[0])) {
										Interface.AddMessage(Interface.MessageType.Error, false, "DaytimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										tday = Interface.GetCombinedFileName(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
										if (!System.IO.File.Exists(tday)) {
											Interface.AddMessage(Interface.MessageType.Error, true, "DaytimeTexture " + tday + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											tday = null;
										}
									}
								}
								if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
									if (Arguments[0].Length == 0) {
										Interface.AddMessage(Interface.MessageType.Error, true, "DaytimeTexture is required to be specified in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (Interface.ContainsInvalidPathChars(Arguments[1])) {
											Interface.AddMessage(Interface.MessageType.Error, false, "NighttimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										} else {
											tnight = Interface.GetCombinedFileName(System.IO.Path.GetDirectoryName(FileName), Arguments[1]);
											if (!System.IO.File.Exists(tnight)) {
												Interface.AddMessage(Interface.MessageType.Error, true, "The NighttimeTexture " + tnight + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												tnight = null;
											}
										}
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].DaytimeTexture = tday;
									Builder.Materials[j].NighttimeTexture = tnight;
								}
							} break;
						case "settexturecoordinates":
						case "coordinates":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int j = 0; float x = 0.0f, y = 0.0f;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !Interface.TryParseIntVb6(Arguments[0], out j)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument VertexIndex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									j = 0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !Interface.TryParseFloatVb6(Arguments[1], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0f;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !Interface.TryParseFloatVb6(Arguments[2], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0f;
								}
								if (j >= 0 & j < Builder.Vertices.Length) {
									Builder.Vertices[j].TextureCoordinates = new World.Vector2Df(x, y);
								} else {
									Interface.AddMessage(Interface.MessageType.Error, false, "VertexIndex references a non-existing vertex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} break;
						default:
							Interface.AddMessage(Interface.MessageType.Warning, false, "The command " + Command + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							break;
					}
				}
			}
			// finalize object
			ApplyMeshBuilder(ref Object, Builder, LoadMode, ForceTextureRepeat);
			return Object;
		}

		// create cube
		private static void CreateCube(ref MeshBuilder Builder, double sx, double sy, double sz) {
			int v = Builder.Vertices.Length;
			Array.Resize<World.Vertex>(ref Builder.Vertices, v + 8);
			Builder.Vertices[v + 0].Coordinates = new World.Vector3D(sx, sy, -sz);
			Builder.Vertices[v + 1].Coordinates = new World.Vector3D(sx, -sy, -sz);
			Builder.Vertices[v + 2].Coordinates = new World.Vector3D(-sx, -sy, -sz);
			Builder.Vertices[v + 3].Coordinates = new World.Vector3D(-sx, sy, -sz);
			Builder.Vertices[v + 4].Coordinates = new World.Vector3D(sx, sy, sz);
			Builder.Vertices[v + 5].Coordinates = new World.Vector3D(sx, -sy, sz);
			Builder.Vertices[v + 6].Coordinates = new World.Vector3D(-sx, -sy, sz);
			Builder.Vertices[v + 7].Coordinates = new World.Vector3D(-sx, sy, sz);
			int f = Builder.Faces.Length;
			Array.Resize<World.MeshFace>(ref Builder.Faces, f + 6);
			Builder.Faces[f + 0].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 1), new World.MeshFaceVertex(v + 2), new World.MeshFaceVertex(v + 3) };
			Builder.Faces[f + 1].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 4), new World.MeshFaceVertex(v + 5), new World.MeshFaceVertex(v + 1) };
			Builder.Faces[f + 2].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 3), new World.MeshFaceVertex(v + 7), new World.MeshFaceVertex(v + 4) };
			Builder.Faces[f + 3].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 5), new World.MeshFaceVertex(v + 4), new World.MeshFaceVertex(v + 7) };
			Builder.Faces[f + 4].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 7), new World.MeshFaceVertex(v + 3), new World.MeshFaceVertex(v + 2) };
			Builder.Faces[f + 5].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 2), new World.MeshFaceVertex(v + 1), new World.MeshFaceVertex(v + 5) };
		}

		// create cylinder
		private static void CreateCylinder(ref MeshBuilder Builder, int n, double r1, double r2, double h) {
			// parameters
			bool uppercap = r1 > 0.0;
			bool lowercap = r2 > 0.0;
			int m = (uppercap ? 1 : 0) + (lowercap ? 1 : 0);
			r1 = Math.Abs(r1);
			r2 = Math.Abs(r2);
			double ns = h >= 0.0 ? 1.0 : -1.0;
			// initialization
			int v = Builder.Vertices.Length;
			Array.Resize<World.Vertex>(ref Builder.Vertices, v + 2 * n);
			World.Vector3Df[] Normals = new World.Vector3Df[2 * n];
			double d = 2.0 * Math.PI / (double)n;
			double g = 0.5 * h;
			double t = 0.0;
			double a = h != 0.0 ? Math.Atan((r2 - r1) / h) : 0.0;
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			// vertices and normals
			for (int i = 0; i < n; i++) {
				double dx = Math.Cos(t);
				double dz = Math.Sin(t);
				double lx = dx * r2;
				double lz = dz * r2;
				double ux = dx * r1;
				double uz = dz * r1;
				Builder.Vertices[v + 2 * i + 0].Coordinates = new World.Vector3D(ux, g, uz);
				Builder.Vertices[v + 2 * i + 1].Coordinates = new World.Vector3D(lx, -g, lz);
				double nx = dx * ns, ny = 0.0, nz = dz * ns;
				double sx, sy, sz;
				World.Cross(nx, ny, nz, 0.0, 1.0, 0.0, out sx, out sy, out sz);
				World.Rotate(ref nx, ref ny, ref nz, sx, sy, sz, cosa, sina);
				Normals[2 * i + 0] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
				Normals[2 * i + 1] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
				t += d;
			}
			// faces
			int f = Builder.Faces.Length;
			Array.Resize<World.MeshFace>(ref Builder.Faces, f + n + m);
			for (int i = 0; i < n; i++) {
				Builder.Faces[f + i].Flags = 0;
				int i0 = (2 * i + 2) % (2 * n);
				int i1 = (2 * i + 3) % (2 * n);
				int i2 = 2 * i + 1;
				int i3 = 2 * i;
				Builder.Faces[f + i].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + i0, Normals[i0]), new World.MeshFaceVertex(v + i1, Normals[i1]), new World.MeshFaceVertex(v + i2, Normals[i2]), new World.MeshFaceVertex(v + i3, Normals[i3]) };
				Builder.Faces[f + i].Flags = World.MeshFace.NormalValueAllExplicit;
			}
			for (int i = 0; i < m; i++) {
				Builder.Faces[f + n + i].Vertices = new World.MeshFaceVertex[n];
				for (int j = 0; j < n; j++) {
					if (i == 0 & lowercap) {
						// lower cap
						Builder.Faces[f + n + i].Vertices[j] = new World.MeshFaceVertex(v + 2 * (n - j - 1));
					} else {
						// upper cap
						Builder.Faces[f + n + i].Vertices[j] = new World.MeshFaceVertex(v + 2 * j + 1);
					}
				}
			}
		}

		// apply translation
		private static void ApplyTranslation(MeshBuilder Builder, double x, double y, double z) {
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				Builder.Vertices[i].Coordinates.X += x;
				Builder.Vertices[i].Coordinates.Y += y;
				Builder.Vertices[i].Coordinates.Z += z;
			}
		}
		private static void ApplyTranslation(ObjectManager.StaticObject Object, double x, double y, double z) {
			for (int i = 0; i < Object.Mesh.Vertices.Length; i++) {
				Object.Mesh.Vertices[i].Coordinates.X += x;
				Object.Mesh.Vertices[i].Coordinates.Y += y;
				Object.Mesh.Vertices[i].Coordinates.Z += z;
			}
		}

		// apply scale
		private static void ApplyScale(MeshBuilder Builder, double x, double y, double z) {
			float rx = (float)(1.0 / x);
			float ry = (float)(1.0 / y);
			float rz = (float)(1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				Builder.Vertices[i].Coordinates.X *= x;
				Builder.Vertices[i].Coordinates.Y *= y;
				Builder.Vertices[i].Coordinates.Z *= z;
			}
			for (int i = 0; i < Builder.Faces.Length; i++) {
				for (int j = 0; j < Builder.Faces[i].Vertices.Length; j++) {
					float nx2 = Builder.Faces[i].Vertices[j].Normal.X * Builder.Faces[i].Vertices[j].Normal.X;
					float ny2 = Builder.Faces[i].Vertices[j].Normal.Y * Builder.Faces[i].Vertices[j].Normal.Y;
					float nz2 = Builder.Faces[i].Vertices[j].Normal.Z * Builder.Faces[i].Vertices[j].Normal.Z;
					float u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0) {
						u = (float)Math.Sqrt((double)((nx2 + ny2 + nz2) / u));
						Builder.Faces[i].Vertices[j].Normal.X *= rx * u;
						Builder.Faces[i].Vertices[j].Normal.Y *= ry * u;
						Builder.Faces[i].Vertices[j].Normal.Z *= rz * u;
					}
				}
			}
			if (x * y * z < 0.0) {
				// reverse face vertex order
				for (int i = 0; i < Builder.Faces.Length; i++) {
					for (int j = 0; j < (Builder.Faces[i].Vertices.Length >> 1); j++) {
						int k = Builder.Faces[i].Vertices.Length - j - 1;
						World.MeshFaceVertex v = Builder.Faces[i].Vertices[j];
						Builder.Faces[i].Vertices[j] = Builder.Faces[i].Vertices[k];
						Builder.Faces[i].Vertices[k] = v;
					}
				}
			}
		}
		private static void ApplyScale(ObjectManager.StaticObject Object, double x, double y, double z) {
			float rx = (float)(1.0 / x);
			float ry = (float)(1.0 / y);
			float rz = (float)(1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			bool reverse = x * y * z < 0.0;
			for (int j = 0; j < Object.Mesh.Vertices.Length; j++) {
				Object.Mesh.Vertices[j].Coordinates.X *= x;
				Object.Mesh.Vertices[j].Coordinates.Y *= y;
				Object.Mesh.Vertices[j].Coordinates.Z *= z;
			}
			for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
				for (int k = 0; k < Object.Mesh.Faces[j].Vertices.Length; k++) {
					float nx2 = Object.Mesh.Faces[j].Vertices[k].Normal.X * Object.Mesh.Faces[j].Vertices[k].Normal.X;
					float ny2 = Object.Mesh.Faces[j].Vertices[k].Normal.Y * Object.Mesh.Faces[j].Vertices[k].Normal.Y;
					float nz2 = Object.Mesh.Faces[j].Vertices[k].Normal.Z * Object.Mesh.Faces[j].Vertices[k].Normal.Z;
					float u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0) {
						u = (float)Math.Sqrt((double)((nx2 + ny2 + nz2) / u));
						Object.Mesh.Faces[j].Vertices[k].Normal.X *= rx * u;
						Object.Mesh.Faces[j].Vertices[k].Normal.Y *= ry * u;
						Object.Mesh.Faces[j].Vertices[k].Normal.Z *= rz * u;
					}

				}
			}
			if (reverse) {
				// reverse face vertex order
				for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
					for (int k = 0; k < (Object.Mesh.Faces[j].Vertices.Length >> 1); k++) {
						int h = Object.Mesh.Faces[j].Vertices.Length - k - 1;
						World.MeshFaceVertex v = Object.Mesh.Faces[j].Vertices[k];
						Object.Mesh.Faces[j].Vertices[k] = Object.Mesh.Faces[j].Vertices[h];
						Object.Mesh.Faces[j].Vertices[h] = v;
					}
				}
			}
		}

		// apply rotation
		private static void ApplyRotation(MeshBuilder Builder, double x, double y, double z, double a) {
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				World.Rotate(ref Builder.Vertices[i].Coordinates.X, ref Builder.Vertices[i].Coordinates.Y, ref Builder.Vertices[i].Coordinates.Z, x, y, z, cosa, sina);
			}
			for (int i = 0; i < Builder.Faces.Length; i++) {
				for (int j = 0; j < Builder.Faces[i].Vertices.Length; j++) {
					World.Rotate(ref Builder.Faces[i].Vertices[j].Normal.X, ref Builder.Faces[i].Vertices[j].Normal.Y, ref Builder.Faces[i].Vertices[j].Normal.Z, x, y, z, cosa, sina);
				}
			}
		}
		private static void ApplyRotation(ObjectManager.StaticObject Object, double x, double y, double z, double a) {
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			for (int j = 0; j < Object.Mesh.Vertices.Length; j++) {
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, x, y, z, cosa, sina);
			}
			for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
				for (int k = 0; k < Object.Mesh.Faces[j].Vertices.Length; k++) {
					World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, x, y, z, cosa, sina);
				}
			}
		}

		// apply mesh builder
		private static void ApplyMeshBuilder(ref ObjectManager.StaticObject Object, MeshBuilder Builder, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeat) {
			if (Builder.Faces.Length != 0) {
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Builder.Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Builder.Materials.Length);
				Array.Resize<World.Vertex>(ref Object.Mesh.Vertices, mv + Builder.Vertices.Length);
				for (int i = 0; i < Builder.Vertices.Length; i++) {
					Object.Mesh.Vertices[mv + i] = Builder.Vertices[i];
				}
				for (int i = 0; i < Builder.Faces.Length; i++) {
					Object.Mesh.Faces[mf + i] = Builder.Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++) {
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Object.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Builder.Materials.Length; i++) {
					Object.Mesh.Materials[mm + i].Flags = (byte)((Builder.Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | (Builder.Materials[i].TransparentColorUsed ? World.MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Builder.Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Builder.Materials[i].TransparentColor;
					TextureManager.TextureWrapMode WrapX, WrapY;
					if (ForceTextureRepeat) {
						WrapX = TextureManager.TextureWrapMode.Repeat;
						WrapY = TextureManager.TextureWrapMode.Repeat;
					} else {
						WrapX = TextureManager.TextureWrapMode.ClampToEdge;
						WrapY = TextureManager.TextureWrapMode.ClampToEdge;
						for (int j = 0; j < Builder.Vertices.Length; j++) {
							if (Builder.Vertices[j].TextureCoordinates.X < 0.0 | Builder.Vertices[j].TextureCoordinates.X > 1.0) {
								WrapX = TextureManager.TextureWrapMode.Repeat;
							}
							if (Builder.Vertices[j].TextureCoordinates.Y < 0.0 | Builder.Vertices[j].TextureCoordinates.Y > 1.0) {
								WrapY = TextureManager.TextureWrapMode.Repeat;
							}
						}
					}
					if (Builder.Materials[i].DaytimeTexture != null) {
						int tday = TextureManager.RegisterTexture(Builder.Materials[i].DaytimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);
						if (LoadMode == ObjectManager.ObjectLoadMode.PreloadTextures) {
							TextureManager.UseTexture(tday, TextureManager.UseMode.Normal);
						}
						Object.Mesh.Materials[mm + i].DaytimeTextureIndex = tday;
					} else {
						Object.Mesh.Materials[mm + i].DaytimeTextureIndex = -1;
					}
					Object.Mesh.Materials[mm + i].EmissiveColor = Builder.Materials[i].EmissiveColor;
					if (Builder.Materials[i].NighttimeTexture != null) {
						int tnight = TextureManager.RegisterTexture(Builder.Materials[i].NighttimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);
						if (LoadMode == ObjectManager.ObjectLoadMode.PreloadTextures) {
							TextureManager.UseTexture(tnight, TextureManager.UseMode.Normal);
						}
						Object.Mesh.Materials[mm + i].NighttimeTextureIndex = tnight;
					} else {
						Object.Mesh.Materials[mm + i].NighttimeTextureIndex = -1;
					}
					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Builder.Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Builder.Materials[i].GlowAttenuationData;
				}
			}
		}

	}
}