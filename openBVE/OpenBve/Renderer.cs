using System;
using Tao.OpenGl;

namespace OpenBve {
	internal static class Renderer {

		// screen (output window)
		internal static int ScreenWidth = 0;
		internal static int ScreenHeight = 0;

		// first frame behavior
		internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
		internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;

		// transparency
		internal enum TransparencyMode {
			/// <summary>Produces a crisp result for color-key textures and takes no special provisions for alpha textures.</summary>
			Sharp = 0,
			/// <summary>Produces a smooth result for color-key textures and tries to eliminate fringes occuring as a result of bad depth sorting.</summary>
			Smooth = 1
		}

		// output mode
		internal enum OutputMode {
			Default = 0,
			Debug = 1,
			None = 2
		}
		internal static OutputMode CurrentOutputMode = OutputMode.Default;

		// object list
		private struct Object {
			internal int ObjectIndex;
			internal int[] FaceListIndices;
			internal bool Overlay;
		}
		private static Object[] ObjectList = new Object[256];
		private static int ObjectListCount = 0;

		// face lists
		private struct ObjectFace {
			internal int ObjectListIndex;
			internal int ObjectIndex;
			internal int FaceIndex;
		}
		// layers
		private const int WorldLayer = 0;
		private const int OverlayLayer = 1;
		// opaque
		private static ObjectFace[][] OpaqueList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
		internal static int[] OpaqueListCount = new int[] { 0, 0 };
		// transparent color
		private static ObjectFace[][] TransparentColorList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
		private static double[][] TransparentColorListDistance = new double[][] { new double[256] , new double[256] };
		internal static int[] TransparentColorListCount = new int[] { 0, 0 };
		// alpha
		private static ObjectFace[][] AlphaList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
		private static double[][] AlphaListDistance = new double[][] { new double[256] , new double[256] };
		internal static int[] AlphaListCount = new int[] { 0, 0 };

		// current opengl data
		private static int AlphaFuncComparison = 0;
		private static float AlphaFuncValue = 0.0f;
		private static bool BlendEnabled = false;
		private static bool AlphaTestEnabled = false;
		private static bool CullEnabled = true;
		internal static bool LightingEnabled = false;
		internal static bool FogEnabled = false;
		private static bool TexturingEnabled = false;
		private static bool EmissiveEnabled = false;
		private static bool TransparentColorDepthSorting = false;

		// options
		internal static bool OptionLighting = true;
		internal static World.ColorRGB OptionAmbientColor = new World.ColorRGB(160, 160, 160);
		internal static World.ColorRGB OptionDiffuseColor = new World.ColorRGB(160, 160, 160);
		internal static World.Vector3Df OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
		internal static float OptionLightingResultingAmount = 1.0f;
		internal static bool OptionNormals = false;
		internal static bool OptionWireframe = false;
		internal static bool OptionBackfaceCulling = true;

		// interface options
		internal const bool OptionHeadlight = false; // for testing purposes
		internal static bool OptionClock = false;
		internal enum SpeedDisplayMode { None, Kmph, Mph }
		internal static SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
		internal static bool OptionFrameRates = false;
		internal static bool OptionBrakeSystems = false;

		// textures
		private static int TextureLogo = -1;
		private static int TexturePause = -1;

		// constants
		private const float inv255 = 1.0f / 255.0f;

		// reset
		internal static void Reset() {
			LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
			ObjectList = new Object[256];
			ObjectListCount = 0;
			OpaqueList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
			OpaqueListCount = new int[] { 0, 0 };
			TransparentColorList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
			TransparentColorListDistance = new double[][] { new double[256], new double[256] };
			TransparentColorListCount = new int[] { 0, 0 };
			AlphaList = new ObjectFace[][] { new ObjectFace[256], new ObjectFace[256]};
			AlphaListDistance = new double[][] { new double[256], new double[256] };
			AlphaListCount = new int[] { 0, 0 };
			OptionLighting = true;
			OptionAmbientColor = new World.ColorRGB(160, 160, 160);
			OptionDiffuseColor = new World.ColorRGB(160, 160, 160);
			OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
			OptionLightingResultingAmount = 1.0f;
			OptionClock = false;
			OptionBrakeSystems = false;
		}

		// initialize
		internal static void Initialize() {
			// opengl
			Gl.glShadeModel(Gl.GL_SMOOTH);
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glDepthFunc(Gl.GL_LEQUAL);
			Gl.glHint(Gl.GL_FOG_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_LINE_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_POINT_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_FASTEST);
			Gl.glHint(Gl.GL_GENERATE_MIPMAP_HINT, Gl.GL_NICEST);
			Gl.glDisable(Gl.GL_DITHER);
			Gl.glCullFace(Gl.GL_FRONT);
			Gl.glEnable(Gl.GL_CULL_FACE); CullEnabled = true;
			Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
			Gl.glDisable(Gl.GL_TEXTURE_2D); TexturingEnabled = false;
			// hud
			Interface.LoadHUD();
			string Path = Interface.GetDataFolder("In-game");
			TextureLogo = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "logo.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, TextureManager.TextureWrapMode.ClampToEdge, false);
			TextureManager.ValidateTexture(ref TextureLogo);
			// opengl
			Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			Gl.glPushMatrix();
			Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			Glu.gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
			Gl.glPopMatrix();
			TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == Renderer.TransparencyMode.Smooth & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.Bilinear;
			// prepare rendering logo
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
			Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			Gl.glOrtho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			// render logo
			double size = ScreenWidth < ScreenHeight ? ScreenWidth : ScreenHeight;
			Gl.glColor3f(1.0f, 1.0f, 1.0f);
			RenderOverlayTexture(TextureLogo, 0.5 * (ScreenWidth - size), 0.5 * (ScreenHeight - size), 0.5 * (ScreenWidth + size), 0.5 * (ScreenHeight + size));
			//RenderString(0.5 * (double)ScreenWidth, (double)ScreenHeight - 24.0, Fonts.FontType.Small, Interface.GetInterfaceString("message_loading"), 0, 255, 255, 255, true);
			// finalize
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glDisable(Gl.GL_BLEND);
		}
		
		// deinitialize
		internal static void Deinitialize() { }

		// initialize lighting
		internal static void InitializeLighting() {
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
			if (OptionHeadlight) {
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, new float[] { 0.0f, 0.0f, 0.0f, 0.0f });
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPOT_DIRECTION, new float[] { 0.0f, 0.0f, -1.0f });
				Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_CUTOFF, 45.0f);
				Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_SPOT_EXPONENT, 128.0f);
				Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_CONSTANT_ATTENUATION, 0.0f);
				Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_LINEAR_ATTENUATION, 0.0f);
				Gl.glLightf(Gl.GL_LIGHT1, Gl.GL_QUADRATIC_ATTENUATION, 0.001f);
			}
			Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
			Gl.glCullFace(Gl.GL_FRONT); CullEnabled = true; // possibly undocumented, but required for correct lighting
			Gl.glEnable(Gl.GL_LIGHT0);
			if (OptionHeadlight) {
				Gl.glEnable(Gl.GL_LIGHT1);
			}
			Gl.glEnable(Gl.GL_COLOR_MATERIAL);
			Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);
			Gl.glShadeModel(Gl.GL_SMOOTH);
			float x = ((float)OptionAmbientColor.R + (float)OptionAmbientColor.G + (float)OptionAmbientColor.B);
			float y = ((float)OptionDiffuseColor.R + (float)OptionDiffuseColor.G + (float)OptionDiffuseColor.B);
			if (x < y) x = y;
			OptionLightingResultingAmount = 0.00208333333333333f * x;
			if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
			Gl.glEnable(Gl.GL_LIGHTING); LightingEnabled = true;
			Gl.glDepthFunc(Gl.GL_LEQUAL);
		}
		
		// render scene
		internal static byte[] PixelBuffer = null;
		internal static int PixelBufferOpenGlTextureIndex = 0;
		internal static void RenderScene(double TimeElapsed) {
			// initialize
			Gl.glEnable(Gl.GL_DEPTH_TEST);
			Gl.glDepthMask(true);
			int OpenGlTextureIndex = 0;
			if (World.CurrentBackground.Texture >= 0) {
				OpenGlTextureIndex = TextureManager.UseTexture(World.CurrentBackground.Texture, TextureManager.UseMode.Normal);
			}
			if (OptionWireframe | OpenGlTextureIndex == 0) {
				if (Game.CurrentFog.Start < Game.CurrentFog.End) {
					const float fogdistance = 600.0f;
					float n = (fogdistance - Game.CurrentFog.Start) / (Game.CurrentFog.End - Game.CurrentFog.Start);
					float cr = n * inv255 * (float)Game.CurrentFog.Color.R;
					float cg = n * inv255 * (float)Game.CurrentFog.Color.G;
					float cb = n * inv255 * (float)Game.CurrentFog.Color.B;
					Gl.glClearColor(cr, cg, cb, 1.0f);
				} else {
					Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
				}
				Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
			} else {
				Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
			}
			Gl.glPushMatrix();
			if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
				MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.ChangeToScenery);
			}
			if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet) {
				LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
				ReAddObjects();
			}
			// setup camera
			double cx = World.AbsoluteCameraPosition.X;
			double cy = World.AbsoluteCameraPosition.Y;
			double cz = World.AbsoluteCameraPosition.Z;
			double dx = World.AbsoluteCameraDirection.X;
			double dy = World.AbsoluteCameraDirection.Y;
			double dz = World.AbsoluteCameraDirection.Z;
			double ux = World.AbsoluteCameraUp.X;
			double uy = World.AbsoluteCameraUp.Y;
			double uz = World.AbsoluteCameraUp.Z;
			Glu.gluLookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
			Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { OptionLightPosition.X, OptionLightPosition.Y, OptionLightPosition.Z, 0.0f });
			if (OptionHeadlight) {
				World.Vector3D trainPosition = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.WorldPosition;
				World.Vector3D trainDirection = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.WorldDirection;
				trainPosition.X -= cx;
				trainPosition.Y -= cy;
				trainPosition.Z -= cz;
				World.Vector3D direction = TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.WorldDirection;
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, new float[] { (float)trainPosition.X, (float)trainPosition.Y, (float)trainPosition.Z, 1.0f });
				Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPOT_DIRECTION, new float[] { (float)direction.X, (float)direction.Y, (float)direction.Z });
			}
			// fog
			double fd = Game.NextFog.TrackPosition - Game.PreviousFog.TrackPosition;
			if (fd != 0.0) {
				float fr = (float)((World.CameraTrackFollower.TrackPosition - Game.PreviousFog.TrackPosition) / fd);
				float frc = 1.0f - fr;
				Game.CurrentFog.Start = Game.PreviousFog.Start * frc + Game.NextFog.Start * fr;
				Game.CurrentFog.End = Game.PreviousFog.End * frc + Game.NextFog.End * fr;
				Game.CurrentFog.Color.R = (byte)((float)Game.PreviousFog.Color.R * frc + (float)Game.NextFog.Color.R * fr);
				Game.CurrentFog.Color.G = (byte)((float)Game.PreviousFog.Color.G * frc + (float)Game.NextFog.Color.G * fr);
				Game.CurrentFog.Color.B = (byte)((float)Game.PreviousFog.Color.B * frc + (float)Game.NextFog.Color.B * fr);
			} else {
				Game.CurrentFog = Game.PreviousFog;
			}
			// render background
			if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			RenderBackground(dx, dy, dz, TimeElapsed);
			// fog
			double aa = Game.CurrentFog.Start;
			double bb = Game.CurrentFog.End;
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < World.BackgroundImageDistance) {
				if (!FogEnabled) {
					Gl.glFogi(Gl.GL_FOG_MODE, Gl.GL_LINEAR);
				}
				Gl.glFogf(Gl.GL_FOG_START, Game.CurrentFog.Start);
				Gl.glFogf(Gl.GL_FOG_END, Game.CurrentFog.End);
				Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f });
				if (!FogEnabled) {
					Gl.glEnable(Gl.GL_FOG); FogEnabled = true;
				}
			} else if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			// render polygons
			bool optionLighting = OptionLighting;
			for (int k = 0; k < 2; k++) {
				// initialize
				LastBoundTexture = 0;
				if (k == 0) {
					// world
					if (OptionLighting) {
						if (!LightingEnabled) {
							Gl.glEnable(Gl.GL_LIGHTING); LightingEnabled = true;
						}
						if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
							Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
							Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
						}
					} else if (LightingEnabled) {
						Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
					}
				} else {
					// overlay
					if (FogEnabled) {
						Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
					}
					if (World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
						// 3d cab
						Gl.glLoadIdentity();
						MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.ChangeToCab);
						Glu.gluLookAt(0.0, 0.0, 0.0, dx, dy, dz, ux, uy, uz);
						Gl.glDepthMask(true);
						Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
						if (!LightingEnabled) {
							Gl.glEnable(Gl.GL_LIGHTING); LightingEnabled = true;
						}
						OptionLighting = true;
						Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { 0.6f, 0.6f, 0.6f, 1.0f });
						Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { 0.6f, 0.6f, 0.6f, 1.0f });
					} else {
						// not a 3d cab
						if (LightingEnabled) {
							Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = true;
						}
						OptionLighting = false;
						if (!BlendEnabled) {
							Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
						}
						Gl.glDepthMask(false);
						SetAlphaFunc(Gl.GL_GREATER, 0.0f);
						SortPolygons(AlphaList[1], AlphaListCount[1], AlphaListDistance[1], 6, TimeElapsed);
						for (int i = 0; i < AlphaListCount[1]; i++) {
							RenderFace(ref AlphaList[1][i], cx, cy, cz);
						}
						continue;
					}
				}
				SetAlphaFunc(Gl.GL_GREATER, 0.9f);
				if (BlendEnabled) {
					Gl.glDisable(Gl.GL_BLEND); BlendEnabled = false;
				}
				Gl.glEnable(Gl.GL_DEPTH_TEST);
				Gl.glDepthMask(true);
				// opaque list
				for (int i = 0; i < OpaqueListCount[k]; i++) {
					RenderFace(ref OpaqueList[k][i], cx, cy, cz);
				}
				// transparent color list
				if (TransparentColorDepthSorting) {
					SortPolygons(TransparentColorList[k], TransparentColorListCount[k], TransparentColorListDistance[k], (k << 2) + 1, TimeElapsed);
					if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
					}
					for (int i = 0; i < TransparentColorListCount[k]; i++) {
						Gl.glDepthMask(false);
						SetAlphaFunc(Gl.GL_LESS, 1.0f);
						RenderFace(ref TransparentColorList[k][i], cx, cy, cz);
						Gl.glDepthMask(true);
						SetAlphaFunc(Gl.GL_EQUAL, 1.0f);
						RenderFace(ref TransparentColorList[k][i], cx, cy, cz);
					}
				} else {
					for (int i = 0; i < TransparentColorListCount[k]; i++) {
						RenderFace(ref TransparentColorList[k][i], cx, cy, cz);
					}
				}
				// alpha list
				SortPolygons(AlphaList[k], AlphaListCount[k], AlphaListDistance[k], (k << 2) + 2, TimeElapsed);
				if (Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth) {
					if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
					}
					bool depthMask = true;
					for (int i = 0; i < AlphaListCount[k]; i++) {
						int r = (int)ObjectManager.Objects[AlphaList[k][i].ObjectIndex].Mesh.Faces[AlphaList[k][i].FaceIndex].Material;
						if (ObjectManager.Objects[AlphaList[k][i].ObjectIndex].Mesh.Materials[r].BlendMode == World.MeshMaterialBlendMode.Additive) {
							if (depthMask) {
								Gl.glDepthMask(false);
								depthMask = false;
							}
							SetAlphaFunc(Gl.GL_GREATER, 0.0f);
							RenderFace(ref AlphaList[k][i], cx, cy, cz);
						} else {
							if (depthMask) {
								Gl.glDepthMask(false);
								depthMask = false;
							}
							SetAlphaFunc(Gl.GL_LESS, 1.0f);
							RenderFace(ref AlphaList[k][i], cx, cy, cz);
							Gl.glDepthMask(true);
							depthMask = true;
							SetAlphaFunc(Gl.GL_EQUAL, 1.0f);
							RenderFace(ref AlphaList[k][i], cx, cy, cz);
						}
					}
				} else {
					if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
					}
					Gl.glDepthMask(false);
					SetAlphaFunc(Gl.GL_GREATER, 0.0f);
					for (int i = 0; i < AlphaListCount[k]; i++) {
						RenderFace(ref AlphaList[k][i], cx, cy, cz);
					}
				}
				// motion blur
				if (k == 0) {
					Gl.glDisable(Gl.GL_DEPTH_TEST);
					Gl.glDepthMask(false);
					SetAlphaFunc(Gl.GL_GREATER, 0.0f);
					if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None) {
						if (LightingEnabled) {
							Gl.glDisable(Gl.GL_LIGHTING);
							LightingEnabled = false;
						}
						RenderFullscreenMotionBlur();
					}
				}
			}
			OptionLighting = optionLighting;
			// render overlays
			if (LightingEnabled) {
				Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
			}
			if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			if (BlendEnabled) {
				Gl.glDisable(Gl.GL_BLEND); BlendEnabled = false;
			}
			if (AlphaTestEnabled) {
				Gl.glDisable(Gl.GL_ALPHA_TEST); AlphaTestEnabled = false;
			}
			SetAlphaFunc(Gl.GL_GREATER, 0.9f);
			Gl.glDisable(Gl.GL_DEPTH_TEST);
			RenderOverlays(TimeElapsed);
			// finalize rendering
			Gl.glPopMatrix();
			LoadTexturesImmediately = LoadTextureImmediatelyMode.NoLonger;
		}
		
		// set alpha func
		private static void SetAlphaFunc(int Comparison, float Value) {
			AlphaFuncComparison = Comparison;
			AlphaFuncValue = Value;
			Gl.glAlphaFunc(Comparison, Value);
		}

		// render face
		private static int LastBoundTexture = 0;
		private static void RenderFace(ref ObjectFace Face, double CameraX, double CameraY, double CameraZ) {
			if (CullEnabled) {
				if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) != 0) {
					Gl.glDisable(Gl.GL_CULL_FACE);
					CullEnabled = false;
				}
			} else if (OptionBackfaceCulling) {
				if ((ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) == 0) {
					Gl.glEnable(Gl.GL_CULL_FACE);
					CullEnabled = true;
				}
			}
			int r = (int)ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex].Material;
			RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Materials[r], ObjectManager.Objects[Face.ObjectIndex].Mesh.Vertices, ref ObjectManager.Objects[Face.ObjectIndex].Mesh.Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
		}
		private static void RenderFace(ref World.MeshMaterial Material, World.Vertex[] Vertices, ref World.MeshFace Face, double CameraX, double CameraY, double CameraZ) {
			// texture
			int OpenGlNighttimeTextureIndex = Material.NighttimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.NighttimeTextureIndex, TextureManager.UseMode.Normal) : 0;
			int OpenGlDaytimeTextureIndex = Material.DaytimeTextureIndex >= 0 ? TextureManager.UseTexture(Material.DaytimeTextureIndex, TextureManager.UseMode.Normal) : 0;
			if (OpenGlDaytimeTextureIndex != 0) {
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				if (OpenGlDaytimeTextureIndex != LastBoundTexture) {
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlDaytimeTextureIndex);
					LastBoundTexture = OpenGlDaytimeTextureIndex;
				}
				if (TextureManager.Textures[Material.DaytimeTextureIndex].Transparency != TextureManager.TextureTransparencyMode.None) {
					if (!AlphaTestEnabled) {
						Gl.glEnable(Gl.GL_ALPHA_TEST);
						AlphaTestEnabled = true;
					}
				} else if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
			} else {
				if (TexturingEnabled) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
					LastBoundTexture = 0;
				}
				if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
			}
			// blend mode
			float factor;
			if (Material.BlendMode == World.MeshMaterialBlendMode.Additive) {
				factor = 1.0f;
				if (!BlendEnabled) Gl.glEnable(Gl.GL_BLEND);
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
				if (FogEnabled) {
					Gl.glDisable(Gl.GL_FOG);
				}
			} else if (OpenGlNighttimeTextureIndex == 0) {
				float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
				if (blend > 1.0f) blend = 1.0f;
				factor = 1.0f - 0.8f * blend;
			} else {
				factor = 1.0f;
			}
			if (OpenGlNighttimeTextureIndex != 0) {
				if (LightingEnabled) {
					Gl.glDisable(Gl.GL_LIGHTING);
					LightingEnabled = false;
				}
			} else {
				if (OptionLighting & !LightingEnabled) {
					Gl.glEnable(Gl.GL_LIGHTING);
					LightingEnabled = true;
				}
			}
			// render daytime polygon
			int FaceType = Face.Flags & World.MeshFace.FaceTypeMask;
			switch (FaceType) {
				case World.MeshFace.FaceTypeTriangles:
					Gl.glBegin(Gl.GL_TRIANGLES);
					break;
				case World.MeshFace.FaceTypeTriangleStrip:
					Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
					break;
				case World.MeshFace.FaceTypeQuads:
					Gl.glBegin(Gl.GL_QUADS);
					break;
				case World.MeshFace.FaceTypeQuadStrip:
					Gl.glBegin(Gl.GL_QUAD_STRIP);
					break;
				default:
					Gl.glBegin(Gl.GL_POLYGON);
					break;
			}
			if (Material.GlowAttenuationData != 0) {
				float alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
			} else {
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
			}
			if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0) {
				Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
				EmissiveEnabled = true;
			} else if (EmissiveEnabled) {
				Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
				EmissiveEnabled = false;
			}
			if (OpenGlDaytimeTextureIndex != 0) {
				if (LightingEnabled) {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				} else {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				}
			} else {
				if (LightingEnabled) {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				} else {
					for (int j = 0; j < Face.Vertices.Length; j++) {
						Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					}
				}
			}
			Gl.glEnd();
			// render nighttime polygon
			if (OpenGlNighttimeTextureIndex != 0) {
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				if (!BlendEnabled) {
					Gl.glEnable(Gl.GL_BLEND);
				}
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlNighttimeTextureIndex);
				LastBoundTexture = 0;
				Gl.glAlphaFunc(Gl.GL_GREATER, 0.0f);
				switch (FaceType) {
					case World.MeshFace.FaceTypeTriangles:
						Gl.glBegin(Gl.GL_TRIANGLES);
						break;
					case World.MeshFace.FaceTypeTriangleStrip:
						Gl.glBegin(Gl.GL_TRIANGLE_STRIP);
						break;
					case World.MeshFace.FaceTypeQuads:
						Gl.glBegin(Gl.GL_QUADS);
						break;
					case World.MeshFace.FaceTypeQuadStrip:
						Gl.glBegin(Gl.GL_QUAD_STRIP);
						break;
					default:
						Gl.glBegin(Gl.GL_POLYGON);
						break;
				}
				float alphafactor;
				if (Material.GlowAttenuationData != 0) {
					alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
					float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (blend > 1.0f) blend = 1.0f;
					alphafactor *= blend;
				} else {
					alphafactor = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
					if (alphafactor > 1.0f) alphafactor = 1.0f;
				}
				Gl.glColor4f(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
				if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0) {
					Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
					EmissiveEnabled = true;
				} else if (EmissiveEnabled) {
					Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
					EmissiveEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++) {
					Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
				}
				Gl.glEnd();
				if (AlphaFuncValue != 0.0) {
					Gl.glAlphaFunc(AlphaFuncComparison, AlphaFuncValue);
				}
				if (!BlendEnabled) {
					Gl.glDisable(Gl.GL_BLEND);
				}
			}
			// normals
			if (OptionNormals) {
				if (TexturingEnabled) {
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
				}
				if (AlphaTestEnabled) {
					Gl.glDisable(Gl.GL_ALPHA_TEST);
					AlphaTestEnabled = false;
				}
				for (int j = 0; j < Face.Vertices.Length; j++) {
					Gl.glBegin(Gl.GL_LINES);
					Gl.glColor4f(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ));
					Gl.glVertex3f((float)(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - CameraX), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - CameraY), (float)(Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - CameraZ));
					Gl.glEnd();
				}
			}
			// finalize
			if (Material.BlendMode == World.MeshMaterialBlendMode.Additive) {
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
				if (!BlendEnabled) Gl.glDisable(Gl.GL_BLEND);
				if (FogEnabled) {
					Gl.glEnable(Gl.GL_FOG);
				}
			}
		}

		// render background
		private static void RenderBackground(double dx, double dy, double dz, double TimeElapsed) {
			// fog
			const float fogdistance = 600.0f;
			if (Game.CurrentFog.Start < Game.CurrentFog.End & Game.CurrentFog.Start < fogdistance) {
				float cr = inv255 * (float)Game.CurrentFog.Color.R;
				float cg = inv255 * (float)Game.CurrentFog.Color.G;
				float cb = inv255 * (float)Game.CurrentFog.Color.B;
				if (!FogEnabled) {
					Gl.glFogi(Gl.GL_FOG_MODE, Gl.GL_LINEAR);
				}
				float ratio = (float)World.BackgroundImageDistance / fogdistance;
				Gl.glFogf(Gl.GL_FOG_START, Game.CurrentFog.Start * ratio);
				Gl.glFogf(Gl.GL_FOG_END, Game.CurrentFog.End * ratio);
				Gl.glFogfv(Gl.GL_FOG_COLOR, new float[] { cr, cg, cb, 1.0f });
				if (!FogEnabled) {
					Gl.glEnable(Gl.GL_FOG); FogEnabled = true;
				}
			} else if (FogEnabled) {
				Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
			}
			// render
			if (World.TargetBackgroundCountdown >= 0.0) {
				// fade
				World.TargetBackgroundCountdown -= TimeElapsed;
				if (World.TargetBackgroundCountdown < 0.0) {
					World.CurrentBackground = World.TargetBackground;
					World.TargetBackgroundCountdown = -1.0;
					RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
				} else {
					RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
					AlphaFuncValue = 0.0f; Gl.glAlphaFunc(AlphaFuncComparison, AlphaFuncValue);
					float Alpha = (float)(1.0 - World.TargetBackgroundCountdown / World.TargetBackgroundDefaultCountdown);
					RenderBackground(World.TargetBackground, dx, dy, dz, Alpha);
				}
			} else {
				// single
				RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
			}
		}
		private static void RenderBackground(World.Background Data, double dx, double dy, double dz, float Alpha) {
			if (Data.Texture >= 0) {
				int OpenGlTextureIndex = TextureManager.UseTexture(Data.Texture, TextureManager.UseMode.LoadImmediately);
				if (OpenGlTextureIndex > 0) {
					if (LightingEnabled) {
						Gl.glDisable(Gl.GL_LIGHTING);
						LightingEnabled = false;
					}
					if (!TexturingEnabled) {
						Gl.glEnable(Gl.GL_TEXTURE_2D);
						TexturingEnabled = true;
					}
					if (Alpha == 1.0f) {
						if (BlendEnabled) {
							Gl.glDisable(Gl.GL_BLEND);
							BlendEnabled = false;
						}
					} else if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND);
						BlendEnabled = true;
					}
					Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlTextureIndex);
					Gl.glColor4f(1.0f, 1.0f, 1.0f, Alpha);
					float y0, y1;
					if (Data.KeepAspectRatio) {
						int tw = TextureManager.Textures[Data.Texture].Width;
						int th = TextureManager.Textures[Data.Texture].Height;
						double hh = Math.PI * World.BackgroundImageDistance * (double)th / ((double)tw * (double)Data.Repetition);
						y0 = (float)(-0.5 * hh);
						y1 = (float)(1.5 * hh);
					} else {
						y0 = (float)(-0.125 * World.BackgroundImageDistance);
						y1 = (float)(0.375 * World.BackgroundImageDistance);
					}
					const int n = 32;
					World.Vector3Df[] bottom = new World.Vector3Df[n];
					World.Vector3Df[] top = new World.Vector3Df[n];
					double angleValue = 2.61799387799149 - 3.14159265358979 / (double)n;
					double angleIncrement = 6.28318530717958 / (double)n;
					for (int i = 0; i < n; i++) {
						float x = (float)(World.BackgroundImageDistance * Math.Cos(angleValue));
						float z = (float)(World.BackgroundImageDistance * Math.Sin(angleValue));
						bottom[i] = new World.Vector3Df(x, y0, z);
						top[i] = new World.Vector3Df(x, y1, z);
						angleValue += angleIncrement;
					}
					float textureStart = 0.5f * (float)Data.Repetition / (float)n;
					float textureIncrement = -(float)Data.Repetition / (float)n;
					double textureX = textureStart;
					for (int i = 0; i < n; i++) {
						int j = (i + 1) % n;
						// side wall
						Gl.glBegin(Gl.GL_QUADS);
						Gl.glTexCoord2d(textureX, 0.005f);
						Gl.glVertex3f(top[i].X, top[i].Y, top[i].Z);
						Gl.glTexCoord2d(textureX, 0.995f);
						Gl.glVertex3f(bottom[i].X, bottom[i].Y, bottom[i].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
						Gl.glVertex3f(bottom[j].X, bottom[j].Y, bottom[j].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
						Gl.glVertex3f(top[j].X, top[j].Y, top[j].Z);
						Gl.glEnd();
						// top cap
						Gl.glBegin(Gl.GL_TRIANGLES);
						Gl.glTexCoord2d(textureX, 0.005f);
						Gl.glVertex3f(top[i].X, top[i].Y, top[i].Z);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
						Gl.glVertex3f(top[j].X, top[j].Y, top[j].Z);
						Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.1f);
						Gl.glVertex3f(0.0f, top[i].Y, 0.0f);
						// bottom cap
						Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.9f);
						Gl.glVertex3f(0.0f, bottom[i].Y, 0.0f);
						Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
						Gl.glVertex3f(bottom[j].X, bottom[j].Y, bottom[j].Z);
						Gl.glTexCoord2d(textureX, 0.995f);
						Gl.glVertex3f(bottom[i].X, bottom[i].Y, bottom[i].Z);
						Gl.glEnd();
						// finish
						textureX += textureIncrement;
					}
					Gl.glDisable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = false;
					if (!BlendEnabled) {
						Gl.glEnable(Gl.GL_BLEND);
						BlendEnabled = true;
					}
				}
			}
		}

		// render fullscreen motion blur
		private static void RenderFullscreenMotionBlur() {
			int w = Interface.RoundToPowerOfTwo(ScreenWidth);
			int h = Interface.RoundToPowerOfTwo(ScreenHeight);
			// render
			if (PixelBufferOpenGlTextureIndex >= 0) {
				double strength; switch (Interface.CurrentOptions.MotionBlur) {
						case Interface.MotionBlurMode.Low: strength = 0.0025; break;
						case Interface.MotionBlurMode.Medium: strength = 0.0040; break;
						case Interface.MotionBlurMode.High: strength = 0.0064; break;
						default: strength = 0.0040; break;
				}
				double speed = Math.Abs(World.CameraSpeed);
				double denominator = strength * Game.InfoFrameRate * Math.Sqrt(speed);
				float factor;
				if (denominator > 0.001) {
					factor = (float)Math.Exp(-1.0 / denominator);
				} else {
					factor = 0.0f;
				}
				// initialize
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
				if (!BlendEnabled) {
					Gl.glEnable(Gl.GL_BLEND);
					BlendEnabled = true;
				}
				if (LightingEnabled) {
					Gl.glDisable(Gl.GL_LIGHTING);
					LightingEnabled = false;
				}
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();
				Gl.glOrtho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
				Gl.glPushMatrix();
				Gl.glLoadIdentity();
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				// render
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, PixelBufferOpenGlTextureIndex);
				Gl.glColor4f(1.0f, 1.0f, 1.0f, factor);
				Gl.glBegin(Gl.GL_POLYGON);
				Gl.glTexCoord2d(0.0, 0.0);
				Gl.glVertex2d(0.0, 0.0);
				Gl.glTexCoord2d(0.0, 1.0);
				Gl.glVertex2d(0.0, (double)h);
				Gl.glTexCoord2d(1.0, 1.0);
				Gl.glVertex2d((double)w, (double)h);
				Gl.glTexCoord2d(1.0, 0.0);
				Gl.glVertex2d((double)w, 0.0);
				Gl.glEnd();
				// finalize
				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_PROJECTION);
				Gl.glPopMatrix();
				Gl.glMatrixMode(Gl.GL_MODELVIEW);
			}
			// retrieve buffer
			{
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, PixelBufferOpenGlTextureIndex);
				Gl.glCopyTexImage2D(Gl.GL_TEXTURE_2D, 0, Gl.GL_RGB, 0, 0, w, h, 0);
			}
		}

		// lamps
		private enum LampType {
			None,
			Ats, AtsOperation,
			AtsPPower, AtsPPattern, AtsPBrakeOverride, AtsPBrakeOperation, AtsP, AtsPFailure,
			Atc, AtcPower, AtcUse, AtcEmergency,
			Eb, ConstSpeed
		}
		private struct Lamp {
			internal LampType Type;
			internal string Text;
			internal float Width;
			internal float Height;
			internal Lamp(LampType Type) {
				this.Type = Type;
				switch (Type) {
					case LampType.None:
						this.Text = null;
						break;
					case LampType.Ats:
						this.Text = Interface.GetInterfaceString("lamps_ats");
						break;
					case LampType.AtsOperation:
						this.Text = Interface.GetInterfaceString("lamps_atsoperation");
						break;
					case LampType.AtsPPower:
						this.Text = Interface.GetInterfaceString("lamps_atsppower");
						break;
					case LampType.AtsPPattern:
						this.Text = Interface.GetInterfaceString("lamps_atsppattern");
						break;
					case LampType.AtsPBrakeOverride:
						this.Text = Interface.GetInterfaceString("lamps_atspbrakeoverride");
						break;
					case LampType.AtsPBrakeOperation:
						this.Text = Interface.GetInterfaceString("lamps_atspbrakeoperation");
						break;
					case LampType.AtsP:
						this.Text = Interface.GetInterfaceString("lamps_atsp");
						break;
					case LampType.AtsPFailure:
						this.Text = Interface.GetInterfaceString("lamps_atspfailure");
						break;
					case LampType.Atc:
						this.Text = Interface.GetInterfaceString("lamps_atc");
						break;
					case LampType.AtcPower:
						this.Text = Interface.GetInterfaceString("lamps_atcpower");
						break;
					case LampType.AtcUse:
						this.Text = Interface.GetInterfaceString("lamps_atcuse");
						break;
					case LampType.AtcEmergency:
						this.Text = Interface.GetInterfaceString("lamps_atcemergency");
						break;
					case LampType.Eb:
						this.Text = Interface.GetInterfaceString("lamps_eb");
						break;
					case LampType.ConstSpeed:
						this.Text = Interface.GetInterfaceString("lamps_constspeed");
						break;
					default:
						this.Text = "TEXT";
						break;
				}
				Fonts.FontType s = Fonts.FontType.Small;
				for (int i = 0; i < Interface.CurrentHudElements.Length; i++) {
					if (Interface.CurrentHudElements[i].Subject.Equals("ats", StringComparison.OrdinalIgnoreCase)) {
						s = Interface.CurrentHudElements[i].TextSize;
						break;
					}
				}
				MeasureString(this.Text, s, out this.Width, out this.Height);
			}
		}
		private struct LampCollection {
			internal Lamp[] Lamps;
			internal float Width;
		}
		private static LampCollection CurrentLampCollection;
		private static void InitializeLamps() {
			CurrentLampCollection.Width = 0.0f;
			CurrentLampCollection.Lamps = new Lamp[17];
			int Count;
			if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Plugin | World.CameraRestriction == World.CameraRestrictionMode.NotAvailable) {
				Count = 0;
			} else if (TrainManager.PlayerTrain.Specs.Safety.Ats.AtsPAvailable & TrainManager.PlayerTrain.Specs.Safety.Atc.Available) {
				CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
				CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
				CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
				CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtsPPower);
				CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtsPPattern);
				CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
				CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
				CurrentLampCollection.Lamps[7] = new Lamp(LampType.AtsP);
				CurrentLampCollection.Lamps[8] = new Lamp(LampType.AtsPFailure);
				CurrentLampCollection.Lamps[9] = new Lamp(LampType.None);
				CurrentLampCollection.Lamps[10] = new Lamp(LampType.Atc);
				CurrentLampCollection.Lamps[11] = new Lamp(LampType.AtcPower);
				CurrentLampCollection.Lamps[12] = new Lamp(LampType.AtcUse);
				CurrentLampCollection.Lamps[13] = new Lamp(LampType.AtcEmergency);
				Count = 14;
			} else if (TrainManager.PlayerTrain.Specs.Safety.Ats.AtsPAvailable) {
				CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
				CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
				CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
				CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtsPPower);
				CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtsPPattern);
				CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtsPBrakeOverride);
				CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtsPBrakeOperation);
				CurrentLampCollection.Lamps[7] = new Lamp(LampType.AtsP);
				CurrentLampCollection.Lamps[8] = new Lamp(LampType.AtsPFailure);
				Count = 9;
			} else if (TrainManager.PlayerTrain.Specs.Safety.Atc.Available & TrainManager.PlayerTrain.Specs.Safety.Ats.AtsAvailable) {
				CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
				CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
				CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
				CurrentLampCollection.Lamps[3] = new Lamp(LampType.Atc);
				CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtcPower);
				CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtcUse);
				CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtcEmergency);
				Count = 7;
			} else if (TrainManager.PlayerTrain.Specs.Safety.Atc.Available) {
				CurrentLampCollection.Lamps[0] = new Lamp(LampType.Atc);
				CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtcPower);
				CurrentLampCollection.Lamps[2] = new Lamp(LampType.AtcUse);
				CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtcEmergency);
				Count = 4;
			} else if (TrainManager.PlayerTrain.Specs.Safety.Ats.AtsAvailable) {
				CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
				CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
				Count = 2;
			} else {
				Count = 0;
			}
			if (TrainManager.PlayerTrain.Specs.Safety.Mode != TrainManager.SafetySystem.Plugin & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
				if (TrainManager.PlayerTrain.Specs.Safety.Eb.Available & TrainManager.PlayerTrain.Specs.Safety.Ats.AtsAvailable | TrainManager.PlayerTrain.Specs.HasConstSpeed) {
					CurrentLampCollection.Lamps[Count] = new Lamp(LampType.None);
					Count++;
				}
				if (TrainManager.PlayerTrain.Specs.Safety.Eb.Available & TrainManager.PlayerTrain.Specs.Safety.Ats.AtsAvailable) {
					CurrentLampCollection.Lamps[Count] = new Lamp(LampType.Eb);
					Count++;
				}
				if (TrainManager.PlayerTrain.Specs.HasConstSpeed) {
					CurrentLampCollection.Lamps[Count] = new Lamp(LampType.ConstSpeed);
					Count++;
				}
			}
			Array.Resize<Lamp>(ref CurrentLampCollection.Lamps, Count);
			for (int i = 0; i < Count; i++) {
				if (CurrentLampCollection.Lamps[i].Width > CurrentLampCollection.Width) {
					CurrentLampCollection.Width = CurrentLampCollection.Lamps[i].Width;
				}
			}
		}

		// render overlays
		private static void RenderOverlays(double TimeElapsed) {
			// initialize
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
			Gl.glEnable(Gl.GL_BLEND); BlendEnabled = true;
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			Gl.glOrtho(0.0, (double)ScreenWidth, 0.0, (double)ScreenHeight, -1.0, 1.0);
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glPushMatrix();
			Gl.glLoadIdentity();
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			if (CurrentOutputMode == OutputMode.Default) {
				// hud
				TrainManager.TrainDoorState LeftDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false);
				TrainManager.TrainDoorState RightDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true);
				for (int i = 0; i < Interface.CurrentHudElements.Length; i++) {
					string Command = Interface.CurrentHudElements[i].Subject.ToLowerInvariant();
					switch (Command) {
						case "messages":
							{
								// messages
								int n = Game.Messages.Length;
								float totalwidth = 16.0f;
								float[] widths = new float[n];
								float[] heights = new float[n];
								for (int j = 0; j < n; j++) {
									MeasureString(Game.Messages[j].DisplayText, Interface.CurrentHudElements[i].TextSize, out widths[j], out heights[j]);
									float a = widths[j] - j * Interface.CurrentHudElements[i].Value1;
									if (a > totalwidth) totalwidth = a;
								}
								Game.MessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.MessagesRendererSize.X);
								totalwidth = (float)Game.MessagesRendererSize.X;
								double lcrh = 0.0;
								/// left width/height
								double lw = 0.0;
								if (Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// center height
								if (Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								/// right width/height
								double rw = 0.0;
								if (Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// start
								double w = totalwidth + lw + rw;
								double h = Interface.CurrentHudElements[i].Value2 * n;
								double x = Interface.CurrentHudElements[i].Alignment.X < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.X > 0 ? ScreenWidth - w : 0.5 * (ScreenWidth - w);
								double y = Interface.CurrentHudElements[i].Alignment.Y < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.Y > 0 ? ScreenHeight - h : 0.5 * (ScreenHeight - h);
								x += Interface.CurrentHudElements[i].Position.X;
								y += Interface.CurrentHudElements[i].Position.Y;
								int m = 0;
								for (int j = 0; j < n; j++) {
									float br, bg, bb, ba;
									CreateBackColor(Interface.CurrentHudElements[i].BackgroundColor, Game.Messages[j].Color, out br, out bg, out bb, out ba);
									float tr, tg, tb, ta;
									CreateTextColor(Interface.CurrentHudElements[i].TextColor, Game.Messages[j].Color, out tr, out tg, out tb, out ta);
									float or, og, ob, oa;
									CreateBackColor(Interface.CurrentHudElements[i].OverlayColor, Game.Messages[j].Color, out or, out og, out ob, out oa);
									double tx, ty;
									bool preserve = false;
									if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Move) != 0) {
										if (Game.SecondsSinceMidnight < Game.Messages[j].Timeout) {
											if (Game.Messages[j].RendererAlpha == 0.0) {
												Game.Messages[j].RendererPosition.X = x + Interface.CurrentHudElements[i].TransitionVector.X;
												Game.Messages[j].RendererPosition.Y = y + Interface.CurrentHudElements[i].TransitionVector.Y;
												Game.Messages[j].RendererAlpha = 1.0;
											}
											tx = x;
											ty = y + m * (Interface.CurrentHudElements[i].Value2);
											preserve = true;
										} else if (Interface.CurrentHudElements[i].Transition == Interface.HudTransition.MoveAndFade) {
											tx = x;
											ty = y + m * (Interface.CurrentHudElements[i].Value2);
										} else {
											tx = x + Interface.CurrentHudElements[i].TransitionVector.X;
											ty = y + (j + 1) * Interface.CurrentHudElements[i].TransitionVector.Y;
										}
										const double speed = 2.0;
										double dx = (speed * Math.Abs(tx - Game.Messages[j].RendererPosition.X) + 0.1) * TimeElapsed;
										double dy = (speed * Math.Abs(ty - Game.Messages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
										if (Math.Abs(tx - Game.Messages[j].RendererPosition.X) < dx) {
											Game.Messages[j].RendererPosition.X = tx;
										} else {
											Game.Messages[j].RendererPosition.X += Math.Sign(tx - Game.Messages[j].RendererPosition.X) * dx;
										}
										if (Math.Abs(ty - Game.Messages[j].RendererPosition.Y) < dy) {
											Game.Messages[j].RendererPosition.Y = ty;
										} else {
											Game.Messages[j].RendererPosition.Y += Math.Sign(ty - Game.Messages[j].RendererPosition.Y) * dy;
										}
									} else {
										tx = x;
										ty = y + m * (Interface.CurrentHudElements[i].Value2);
										Game.Messages[j].RendererPosition.X = 0.0;
										const double speed = 12.0;
										double dy = (speed * Math.Abs(ty - Game.Messages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
										Game.Messages[j].RendererPosition.X = x;
										if (Math.Abs(ty - Game.Messages[j].RendererPosition.Y) < dy) {
											Game.Messages[j].RendererPosition.Y = ty;
										} else {
											Game.Messages[j].RendererPosition.Y += Math.Sign(ty - Game.Messages[j].RendererPosition.Y) * dy;
										}
									}
									if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Fade) != 0) {
										if (Game.SecondsSinceMidnight >= Game.Messages[j].Timeout) {
											Game.Messages[j].RendererAlpha -= TimeElapsed;
											if (Game.Messages[j].RendererAlpha < 0.0) Game.Messages[j].RendererAlpha = 0.0;
										} else {
											Game.Messages[j].RendererAlpha += TimeElapsed;
											if (Game.Messages[j].RendererAlpha > 1.0) Game.Messages[j].RendererAlpha = 1.0;
											preserve = true;
										}
									} else if (Game.SecondsSinceMidnight > Game.Messages[j].Timeout) {
										if (Math.Abs(Game.Messages[j].RendererPosition.X - tx) < 0.1 & Math.Abs(Game.Messages[j].RendererPosition.Y - ty) < 0.1) {
											Game.Messages[j].RendererAlpha = 0.0;
										}
									}
									if (preserve) m++;
									double px = Game.Messages[j].RendererPosition.X + (double)j * (double)Interface.CurrentHudElements[i].Value1;
									double py = Game.Messages[j].RendererPosition.Y;
									float alpha = (float)(Game.Messages[j].RendererAlpha * Game.Messages[j].RendererAlpha);
									/// graphics
									Interface.HudImage Left = j == 0 ? Interface.CurrentHudElements[i].TopLeft : j < n - 1 ? Interface.CurrentHudElements[i].CenterLeft : Interface.CurrentHudElements[i].BottomLeft;
									Interface.HudImage Middle = j == 0 ? Interface.CurrentHudElements[i].TopMiddle : j < n - 1 ? Interface.CurrentHudElements[i].CenterMiddle : Interface.CurrentHudElements[i].BottomMiddle;
									Interface.HudImage Right = j == 0 ? Interface.CurrentHudElements[i].TopRight : j < n - 1 ? Interface.CurrentHudElements[i].CenterRight : Interface.CurrentHudElements[i].BottomRight;
									/// left background
									if (Left.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Left.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Left.BackgroundTextureIndex, px, py, px + u, py + v);
										}
									}
									/// right background
									if (Right.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Right.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Right.BackgroundTextureIndex, px + w - u, py, px + w, py + v);
										}
									}
									/// middle background
									if (Middle.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Middle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double v = (double)TextureManager.Textures[Middle.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Middle.BackgroundTextureIndex, px + lw, py, px + w - rw, py + v);
										}
									}
									{ /// text
										string t = Game.Messages[j].DisplayText;
										double u = widths[j];
										double v = heights[j];
										double p = Math.Round((Interface.CurrentHudElements[i].TextAlignment.X < 0 ? px : Interface.CurrentHudElements[i].TextAlignment.X > 0 ? px + w - u : px + 0.5 * (w - u)) - j * Interface.CurrentHudElements[i].Value1);
										double q = Math.Round(Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? py : Interface.CurrentHudElements[i].TextAlignment.Y > 0 ? py + lcrh - v : py + 0.5 * (lcrh - v));
										p += Interface.CurrentHudElements[i].TextPosition.X;
										q += Interface.CurrentHudElements[i].TextPosition.Y;
										RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, tr, tg, tb, ta * alpha, Interface.CurrentHudElements[i].TextShadow);
									}
									/// left overlay
									if (Left.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Left.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Left.OverlayTextureIndex, px, py, px + u, py + v);
										}
									}
									/// right overlay
									if (Right.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Right.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Right.OverlayTextureIndex, px + w - u, py, px + w, py + v);
										}
									}
									/// middle overlay
									if (Middle.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Middle.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double v = (double)TextureManager.Textures[Middle.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Middle.OverlayTextureIndex, px + lw, py, px + w - rw, py + v);
										}
									}
								}
							} break;
						case "scoremessages":
							{
								// score messages
								int n = Game.ScoreMessages.Length;
								float totalwidth = 16.0f;
								float[] widths = new float[n];
								float[] heights = new float[n];
								for (int j = 0; j < n; j++) {
									MeasureString(Game.ScoreMessages[j].Text, Interface.CurrentHudElements[i].TextSize, out widths[j], out heights[j]);
									float a = widths[j] - j * Interface.CurrentHudElements[i].Value1;
									if (a > totalwidth) totalwidth = a;
								}
								Game.ScoreMessagesRendererSize.X += 16.0 * TimeElapsed * ((double)totalwidth - Game.ScoreMessagesRendererSize.X);
								totalwidth = (float)Game.ScoreMessagesRendererSize.X;
								double lcrh = 0.0;
								/// left width/height
								double lw = 0.0;
								if (Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// center height
								if (Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								/// right width/height
								double rw = 0.0;
								if (Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// start
								double w = Interface.CurrentHudElements[i].Alignment.X == 0 ? lw + rw + 128 : totalwidth + lw + rw;
								double h = Interface.CurrentHudElements[i].Value2 * n;
								double x = Interface.CurrentHudElements[i].Alignment.X < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.X > 0 ? ScreenWidth - w : 0.5 * (ScreenWidth - w);
								double y = Interface.CurrentHudElements[i].Alignment.Y < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.Y > 0 ? ScreenHeight - h : 0.5 * (ScreenHeight - h);
								x += Interface.CurrentHudElements[i].Position.X;
								y += Interface.CurrentHudElements[i].Position.Y;
								int m = 0;
								for (int j = 0; j < n; j++) {
									float br, bg, bb, ba;
									CreateBackColor(Interface.CurrentHudElements[i].BackgroundColor, Game.ScoreMessages[j].Color, out br, out bg, out bb, out ba);
									float tr, tg, tb, ta;
									CreateTextColor(Interface.CurrentHudElements[i].TextColor, Game.ScoreMessages[j].Color, out tr, out tg, out tb, out ta);
									float or, og, ob, oa;
									CreateBackColor(Interface.CurrentHudElements[i].OverlayColor, Game.ScoreMessages[j].Color, out or, out og, out ob, out oa);
									double tx, ty;
									bool preserve = false;
									if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Move) != 0) {
										if (Game.SecondsSinceMidnight < Game.ScoreMessages[j].Timeout) {
											if (Game.ScoreMessages[j].RendererAlpha == 0.0) {
												Game.ScoreMessages[j].RendererPosition.X = x + Interface.CurrentHudElements[i].TransitionVector.X;
												Game.ScoreMessages[j].RendererPosition.Y = y + Interface.CurrentHudElements[i].TransitionVector.Y;
												Game.ScoreMessages[j].RendererAlpha = 1.0;
											}
											tx = x;
											ty = y + m * (Interface.CurrentHudElements[i].Value2);
											preserve = true;
										} else if (Interface.CurrentHudElements[i].Transition == Interface.HudTransition.MoveAndFade) {
											tx = x;
											ty = y + m * (Interface.CurrentHudElements[i].Value2);
										} else {
											tx = x + Interface.CurrentHudElements[i].TransitionVector.X;
											ty = y + (j + 1) * Interface.CurrentHudElements[i].TransitionVector.Y;
										}
										const double speed = 2.0;
										double dx = (speed * Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) + 0.1) * TimeElapsed;
										double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
										if (Math.Abs(tx - Game.ScoreMessages[j].RendererPosition.X) < dx) {
											Game.ScoreMessages[j].RendererPosition.X = tx;
										} else {
											Game.ScoreMessages[j].RendererPosition.X += Math.Sign(tx - Game.ScoreMessages[j].RendererPosition.X) * dx;
										}
										if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy) {
											Game.ScoreMessages[j].RendererPosition.Y = ty;
										} else {
											Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
										}
									} else {
										tx = x;
										ty = y + m * (Interface.CurrentHudElements[i].Value2);
										Game.ScoreMessages[j].RendererPosition.X = 0.0;
										const double speed = 12.0;
										double dy = (speed * Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) + 0.1) * TimeElapsed;
										Game.ScoreMessages[j].RendererPosition.X = x;
										if (Math.Abs(ty - Game.ScoreMessages[j].RendererPosition.Y) < dy) {
											Game.ScoreMessages[j].RendererPosition.Y = ty;
										} else {
											Game.ScoreMessages[j].RendererPosition.Y += Math.Sign(ty - Game.ScoreMessages[j].RendererPosition.Y) * dy;
										}
									}
									if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Fade) != 0) {
										if (Game.SecondsSinceMidnight >= Game.ScoreMessages[j].Timeout) {
											Game.ScoreMessages[j].RendererAlpha -= TimeElapsed;
											if (Game.ScoreMessages[j].RendererAlpha < 0.0) Game.ScoreMessages[j].RendererAlpha = 0.0;
										} else {
											Game.ScoreMessages[j].RendererAlpha += TimeElapsed;
											if (Game.ScoreMessages[j].RendererAlpha > 1.0) Game.ScoreMessages[j].RendererAlpha = 1.0;
											preserve = true;
										}
									} else if (Game.SecondsSinceMidnight > Game.ScoreMessages[j].Timeout) {
										if (Math.Abs(Game.ScoreMessages[j].RendererPosition.X - tx) < 0.1 & Math.Abs(Game.ScoreMessages[j].RendererPosition.Y - ty) < 0.1) {
											Game.ScoreMessages[j].RendererAlpha = 0.0;
										}
									}
									if (preserve) m++;
									double px = Game.ScoreMessages[j].RendererPosition.X + (double)j * (double)Interface.CurrentHudElements[i].Value1;
									double py = Game.ScoreMessages[j].RendererPosition.Y;
									float alpha = (float)(Game.ScoreMessages[j].RendererAlpha * Game.ScoreMessages[j].RendererAlpha);
									/// graphics
									Interface.HudImage Left = j == 0 ? Interface.CurrentHudElements[i].TopLeft : j < n - 1 ? Interface.CurrentHudElements[i].CenterLeft : Interface.CurrentHudElements[i].BottomLeft;
									Interface.HudImage Middle = j == 0 ? Interface.CurrentHudElements[i].TopMiddle : j < n - 1 ? Interface.CurrentHudElements[i].CenterMiddle : Interface.CurrentHudElements[i].BottomMiddle;
									Interface.HudImage Right = j == 0 ? Interface.CurrentHudElements[i].TopRight : j < n - 1 ? Interface.CurrentHudElements[i].CenterRight : Interface.CurrentHudElements[i].BottomRight;
									/// left background
									if (Left.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Left.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Left.BackgroundTextureIndex, px, py, px + u, py + v);
										}
									}
									/// right background
									if (Right.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Right.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Right.BackgroundTextureIndex, px + w - u, py, px + w, py + v);
										}
									}
									/// middle background
									if (Middle.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Middle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double v = (double)TextureManager.Textures[Middle.BackgroundTextureIndex].ClipHeight;
											Gl.glColor4f(br, bg, bb, ba * alpha);
											RenderOverlayTexture(Middle.BackgroundTextureIndex, px + lw, py, px + w - rw, py + v);
										}
									}
									{ /// text
										string t = Game.ScoreMessages[j].Text;
										double u = widths[j];
										double v = heights[j];
										double p = Math.Round((Interface.CurrentHudElements[i].TextAlignment.X < 0 ? px : Interface.CurrentHudElements[i].TextAlignment.X > 0 ? px + w - u : px + 0.5 * (w - u)) - j * Interface.CurrentHudElements[i].Value1);
										double q = Math.Round(Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? py : Interface.CurrentHudElements[i].TextAlignment.Y > 0 ? py + lcrh - v : py + 0.5 * (lcrh - v));
										p += Interface.CurrentHudElements[i].TextPosition.X;
										q += Interface.CurrentHudElements[i].TextPosition.Y;
										RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, tr, tg, tb, ta * alpha, Interface.CurrentHudElements[i].TextShadow);
									}
									/// left overlay
									if (Left.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Left.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Left.OverlayTextureIndex, px, py, px + u, py + v);
										}
									}
									/// right overlay
									if (Right.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Right.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double u = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipWidth;
											double v = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Right.OverlayTextureIndex, px + w - u, py, px + w, py + v);
										}
									}
									/// middle overlay
									if (Middle.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Middle.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										if (OpenGlTextureIndex != 0) {
											double v = (double)TextureManager.Textures[Middle.OverlayTextureIndex].ClipHeight;
											Gl.glColor4f(or, og, ob, oa * alpha);
											RenderOverlayTexture(Middle.OverlayTextureIndex, px + lw, py, px + w - rw, py + v);
										}
									}
								}
							} break;
						case "ats":
							{
								// ats lamps
								if (CurrentLampCollection.Lamps == null) InitializeLamps();
								double lcrh = 0.0;
								/// left width/height
								double lw = 0.0;
								if (Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomLeft.BackgroundTextureIndex].ClipHeight;
										if (u > lw) lw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// center height
								if (Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomMiddle.BackgroundTextureIndex].ClipHeight;
										if (v > lcrh) lcrh = v;
									}
								}
								/// right width/height
								double rw = 0.0;
								if (Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].TopRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								if (Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										double u = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipWidth;
										double v = (double)TextureManager.Textures[Interface.CurrentHudElements[i].BottomRight.BackgroundTextureIndex].ClipHeight;
										if (u > rw) rw = u;
										if (v > lcrh) lcrh = v;
									}
								}
								/// start
								int n = CurrentLampCollection.Lamps.Length;
								double w = (double)CurrentLampCollection.Width + lw + rw;
								double h = Interface.CurrentHudElements[i].Value2 * n;
								double x = Interface.CurrentHudElements[i].Alignment.X < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.X > 0 ? ScreenWidth - w : 0.5 * (ScreenWidth - w);
								double y = Interface.CurrentHudElements[i].Alignment.Y < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.Y > 0 ? ScreenHeight - h : 0.5 * (ScreenHeight - h);
								x += Interface.CurrentHudElements[i].Position.X;
								y += Interface.CurrentHudElements[i].Position.Y;
								for (int j = 0; j < n; j++) {
									if (CurrentLampCollection.Lamps[j].Type != LampType.None) {
										int o;
										if (j == 0) {
											o = -1;
										} else if (CurrentLampCollection.Lamps[j - 1].Type == LampType.None) {
											o = -1;
										} else if (j < n - 1 && CurrentLampCollection.Lamps[j + 1].Type == LampType.None) {
											o = 1;
										} else if (j == n - 1) {
											o = 1;
										} else {
											o = 0;
										}
										Interface.HudImage Left = o < 0 ? Interface.CurrentHudElements[i].TopLeft : o == 0 ? Interface.CurrentHudElements[i].CenterLeft : Interface.CurrentHudElements[i].BottomLeft;
										Interface.HudImage Middle = o < 0 ? Interface.CurrentHudElements[i].TopMiddle : o == 0 ? Interface.CurrentHudElements[i].CenterMiddle : Interface.CurrentHudElements[i].BottomMiddle;
										Interface.HudImage Right = o < 0 ? Interface.CurrentHudElements[i].TopRight : o == 0 ? Interface.CurrentHudElements[i].CenterRight : Interface.CurrentHudElements[i].BottomRight;
										Game.MessageColor sc = Game.MessageColor.Gray;
										switch (CurrentLampCollection.Lamps[j].Type) {
											case LampType.Ats:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Normal | TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Initialization) {
														sc = Game.MessageColor.Orange;
													}
												} break;
											case LampType.AtsOperation:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Ringing) {
														sc = Game.MessageColor.Red;
													} else if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Emergency | TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Pattern | TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Service) {
														if (((int)Math.Floor(2.0 * Game.SecondsSinceMidnight) & 1) == 0) {
															sc = Game.MessageColor.Red;
														}
													}
												} break;
											case LampType.AtsPPower:
												if ((TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsSn | TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) & TrainManager.PlayerTrain.Specs.Safety.Ats.AtsPAvailable) {
													sc = Game.MessageColor.Green;
												} break;
											case LampType.AtsPPattern:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Pattern | TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Service) {
														sc = Game.MessageColor.Orange;
													}
												} break;
											case LampType.AtsPBrakeOverride:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
													if (TrainManager.PlayerTrain.Specs.Safety.Ats.AtsPOverride) {
														sc = Game.MessageColor.Orange;
													}
												} break;
											case LampType.AtsPBrakeOperation:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Service & !TrainManager.PlayerTrain.Specs.Safety.Ats.AtsPOverride) {
														sc = Game.MessageColor.Orange;
													}
												} break;
											case LampType.AtsP:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
													sc = Game.MessageColor.Green;
												} break;
											case LampType.AtsPFailure:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode != TrainManager.SafetySystem.None) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Initialization) {
														sc = Game.MessageColor.Red;
													} else if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.AtsP) {
														if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Ringing | TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Emergency) {
															sc = Game.MessageColor.Red;
														}
													}
												} break;
											case LampType.Atc:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
													sc = Game.MessageColor.Orange;
												} break;
											case LampType.AtcPower:
												if ((TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Atc | TrainManager.PlayerTrain.Specs.Safety.Mode != TrainManager.SafetySystem.None & TrainManager.PlayerTrain.Specs.Safety.Atc.AutomaticSwitch)) {
													sc = Game.MessageColor.Orange;
												} break;
											case LampType.AtcUse:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
													if (TrainManager.PlayerTrain.Specs.Safety.State == TrainManager.SafetyState.Service) {
														sc = Game.MessageColor.Orange;
													}
												} break;
											case LampType.AtcEmergency:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Atc) {
													if (!TrainManager.PlayerTrain.Specs.Safety.Atc.Transmitting) {
														sc = Game.MessageColor.Red;
													}
												} break;
											case LampType.Eb:
												if (TrainManager.PlayerTrain.Specs.Safety.Mode != TrainManager.SafetySystem.None) {
													if (TrainManager.PlayerTrain.Specs.Safety.Eb.BellState == TrainManager.SafetyState.Ringing) {
														sc = Game.MessageColor.Green;
													}
												} break;
											case LampType.ConstSpeed:
												if (TrainManager.PlayerTrain.Specs.HasConstSpeed) {
													if (TrainManager.PlayerTrain.Specs.CurrentConstSpeed) {
														sc = Game.MessageColor.Orange;
													}
												} break;
										}
										/// colors
										float br, bg, bb, ba;
										CreateBackColor(Interface.CurrentHudElements[i].BackgroundColor, sc, out br, out bg, out bb, out ba);
										float tr, tg, tb, ta;
										CreateTextColor(Interface.CurrentHudElements[i].TextColor, sc, out tr, out tg, out tb, out ta);
										float or, og, ob, oa;
										CreateBackColor(Interface.CurrentHudElements[i].OverlayColor, sc, out or, out og, out ob, out oa);
										/// left background
										if (Left.BackgroundTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Left.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double u = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipWidth;
												double v = (double)TextureManager.Textures[Left.BackgroundTextureIndex].ClipHeight;
												Gl.glColor4f(br, bg, bb, ba);
												RenderOverlayTexture(Left.BackgroundTextureIndex, x, y, x + u, y + v);
											}
										}
										/// right background
										if (Right.BackgroundTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Right.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double u = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipWidth;
												double v = (double)TextureManager.Textures[Right.BackgroundTextureIndex].ClipHeight;
												Gl.glColor4f(br, bg, bb, ba);
												RenderOverlayTexture(Right.BackgroundTextureIndex, x + w - u, y, x + w, y + v);
											}
										}
										/// middle background
										if (Middle.BackgroundTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Middle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double v = (double)TextureManager.Textures[Middle.BackgroundTextureIndex].ClipHeight;
												Gl.glColor4f(br, bg, bb, ba);
												RenderOverlayTexture(Middle.BackgroundTextureIndex, x + lw, y, x + w - rw, y + v);
											}
										}
										{ /// text
											string t = CurrentLampCollection.Lamps[j].Text;
											double u = CurrentLampCollection.Lamps[j].Width;
											double v = CurrentLampCollection.Lamps[j].Height;
											double p = Math.Round(Interface.CurrentHudElements[i].TextAlignment.X < 0 ? x : Interface.CurrentHudElements[i].TextAlignment.X > 0 ? x + w - u : x + 0.5 * (w - u));
											double q = Math.Round(Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? y : Interface.CurrentHudElements[i].TextAlignment.Y > 0 ? y + lcrh - v : y + 0.5 * (lcrh - v));
											p += Interface.CurrentHudElements[i].TextPosition.X;
											q += Interface.CurrentHudElements[i].TextPosition.Y;
											RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, tr, tg, tb, ta, Interface.CurrentHudElements[i].TextShadow);
										}
										/// left overlay
										if (Left.OverlayTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Left.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double u = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipWidth;
												double v = (double)TextureManager.Textures[Left.OverlayTextureIndex].ClipHeight;
												Gl.glColor4f(or, og, ob, oa);
												RenderOverlayTexture(Left.OverlayTextureIndex, x, y, x + u, y + v);
											}
										}
										/// right overlay
										if (Right.OverlayTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Right.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double u = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipWidth;
												double v = (double)TextureManager.Textures[Right.OverlayTextureIndex].ClipHeight;
												Gl.glColor4f(or, og, ob, oa);
												RenderOverlayTexture(Right.OverlayTextureIndex, x + w - u, y, x + w, y + v);
											}
										}
										/// middle overlay
										if (Middle.OverlayTextureIndex >= 0) {
											int OpenGlTextureIndex = TextureManager.UseTexture(Middle.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
											if (OpenGlTextureIndex != 0) {
												double v = (double)TextureManager.Textures[Middle.OverlayTextureIndex].ClipHeight;
												Gl.glColor4f(or, og, ob, oa);
												RenderOverlayTexture(Middle.OverlayTextureIndex, x + lw, y, x + w - rw, y + v);
											}
										}
									}
									y += (double)Interface.CurrentHudElements[i].Value2;
								}
							} break;
						default:
							{
								// default
								double w, h;
								if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
									int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
									if (OpenGlTextureIndex != 0) {
										w = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex].ClipWidth;
										h = (double)TextureManager.Textures[Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex].ClipHeight;
									} else {
										w = 0.0; h = 0.0;
									}
								} else {
									w = 0.0; h = 0.0;
								}
								double x = Interface.CurrentHudElements[i].Alignment.X < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.X == 0 ? 0.5 * (ScreenWidth - w) : ScreenWidth - w;
								double y = Interface.CurrentHudElements[i].Alignment.Y < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.Y == 0 ? 0.5 * (ScreenHeight - h) : ScreenHeight - h;
								x += Interface.CurrentHudElements[i].Position.X;
								y += Interface.CurrentHudElements[i].Position.Y;
								/// command
								const double speed = 1.0;
								Game.MessageColor sc = Game.MessageColor.None;
								string t;
								switch (Command) {
									case "reverser":
										if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 0) {
											sc = Game.MessageColor.Orange; t = Interface.QuickReferences.HandleBackward;
										} else if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > 0) {
											sc = Game.MessageColor.Blue; t = Interface.QuickReferences.HandleForward;
										} else {
											sc = Game.MessageColor.Gray; t = Interface.QuickReferences.HandleNeutral;
										}
										Interface.CurrentHudElements[i].TransitionState = 0.0;
										break;
									case "power":
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											continue;
										} else if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver == 0) {
											sc = Game.MessageColor.Gray; t = Interface.QuickReferences.HandlePowerNull;
										} else {
											sc = Game.MessageColor.Blue; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
										}
										Interface.CurrentHudElements[i].TransitionState = 0.0;
										break;
									case "brake":
										if (TrainManager.PlayerTrain.Specs.SingleHandle) {
											continue;
										} else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
											if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
												sc = Game.MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
											} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
												sc = Game.MessageColor.Gray; t = Interface.QuickReferences.HandleRelease;
											} else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
												sc = Game.MessageColor.Blue; t = Interface.QuickReferences.HandleLap;
											} else {
												sc = Game.MessageColor.Orange; t = Interface.QuickReferences.HandleService;
											}
										} else {
											if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
												sc = Game.MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
											} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
												sc = Game.MessageColor.Green; t = Interface.QuickReferences.HandleHoldBrake;
											} else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver == 0) {
												sc = Game.MessageColor.Gray; t = Interface.QuickReferences.HandleBrakeNull;
											} else {
												sc = Game.MessageColor.Orange; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
											}
										}
										Interface.CurrentHudElements[i].TransitionState = 0.0;
										break;
									case "single":
										if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
											continue;
										} else if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
											sc = Game.MessageColor.Red; t = Interface.QuickReferences.HandleEmergency;
										} else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
											sc = Game.MessageColor.Green; t = Interface.QuickReferences.HandleHoldBrake;
										} else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver > 0) {
											sc = Game.MessageColor.Orange; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
										} else if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver > 0) {
											sc = Game.MessageColor.Blue; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
										} else {
											sc = Game.MessageColor.Gray; t = Interface.QuickReferences.HandlePowerNull;
										}
										Interface.CurrentHudElements[i].TransitionState = 0.0;
										break;
									case "doorsleft":
									case "doorsright":
										{
											if ((LeftDoors & TrainManager.TrainDoorState.AllClosed) == 0 | (RightDoors & TrainManager.TrainDoorState.AllClosed) == 0) {
												Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
											} else {
												Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
											}
											TrainManager.TrainDoorState Doors = Command == "doorsleft" ? LeftDoors : RightDoors;
											if ((Doors & TrainManager.TrainDoorState.Mixed) != 0) {
												sc = Game.MessageColor.Orange;
											} else if ((Doors & TrainManager.TrainDoorState.AllClosed) != 0) {
												sc = Game.MessageColor.Gray;
											} else if (TrainManager.PlayerTrain.Specs.DoorCloseMode == TrainManager.DoorMode.Manual) {
												sc = Game.MessageColor.Green;
											} else {
												sc = Game.MessageColor.Blue;
											}
											t = Command == "doorsleft" ? Interface.QuickReferences.DoorsLeft : Interface.QuickReferences.DoorsRight;
										} break;
									case "stopleft":
									case "stopright":
									case "stopnone":
										{
											int s = TrainManager.PlayerTrain.Station;
											if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert) {
												bool cond;
												if (Command == "stopleft") {
													cond = Game.Stations[s].OpenLeftDoors;
												} else if (Command == "stopright") {
													cond = Game.Stations[s].OpenRightDoors;
												} else {
													cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
												}
												if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond) {
													Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
													if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
												} else {
													Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
													if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
												}
											} else {
												Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
											}
											t = Interface.CurrentHudElements[i].Text;
										} break;
									case "stoplefttick":
									case "stoprighttick":
									case "stopnonetick":
										{
											int s = TrainManager.PlayerTrain.Station;
											if (s >= 0 && Game.PlayerStopsAtStation(s) && Interface.CurrentOptions.GameMode != Interface.GameMode.Expert) {
												int c = Game.GetStopIndex(s, TrainManager.PlayerTrain.Cars.Length);
												if (c >= 0) {
													bool cond;
													if (Command == "stoplefttick") {
														cond = Game.Stations[s].OpenLeftDoors;
													} else if (Command == "stoprighttick") {
														cond = Game.Stations[s].OpenRightDoors;
													} else {
														cond = !Game.Stations[s].OpenLeftDoors & !Game.Stations[s].OpenRightDoors;
													}
													if (TrainManager.PlayerTrain.StationState == TrainManager.TrainStopState.Pending & cond) {
														Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
														if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
													} else {
														Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
														if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
													}
													double d = TrainManager.PlayerTrain.StationDistanceToStopPoint;
													double r;
													if (d > 0.0) {
														r = d / Game.Stations[s].Stops[c].BackwardTolerance;
													} else {
														r = d / Game.Stations[s].Stops[c].ForwardTolerance;
													}
													if (r < -1.0) r = -1.0;
													if (r > 1.0) r = 1.0;
													y -= r * (double)Interface.CurrentHudElements[i].Value1;
												} else {
													Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
													if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
												}
											} else {
												Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
											}
											t = Interface.CurrentHudElements[i].Text;
										} break;
									case "clock":
										{
											int hours = (int)Math.Floor(Game.SecondsSinceMidnight);
											int seconds = hours % 60; hours /= 60;
											int minutes = hours % 60; hours /= 60;
											hours %= 24;
											t = hours.ToString(Culture).PadLeft(2, '0') + ":" + minutes.ToString(Culture).PadLeft(2, '0') + ":" + seconds.ToString(Culture).PadLeft(2, '0');
											if (OptionClock) {
												Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
											} else {
												Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
												if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
											}
										} break;
									case "speed":
										if (OptionSpeed == SpeedDisplayMode.Kmph) {
											double kmph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6;
											t = kmph.ToString("0.00", Culture) + " km/h";
											Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
										} else if (OptionSpeed == SpeedDisplayMode.Mph) {
											double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
											t = mph.ToString("0.00", Culture) + " mph";
											Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
										} else {
											double mph = Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 2.2369362920544;
											t = mph.ToString("0.00", Culture) + " mph";
											Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
										} break;
									case "fps":
										int fps = (int)Math.Round(Game.InfoFrameRate);
										t = fps.ToString(Culture) + " fps";
										if (OptionFrameRates) {
											Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
										} else {
											Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
										} break;
									case "ai":
										t = "A.I.";
										if (TrainManager.PlayerTrain.AI != null) {
											Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
										} else {
											Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
											if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
										} break;
									case "score":
										if (Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade) {
											t = Game.CurrentScore.Value.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture);
											if (Game.CurrentScore.Value < 0) {
												sc = Game.MessageColor.Red;
											} else if (Game.CurrentScore.Value > 0) {
												sc = Game.MessageColor.Green;
											} else {
												sc = Game.MessageColor.Gray;
											}
											Interface.CurrentHudElements[i].TransitionState = 0.0;
										} else {
											Interface.CurrentHudElements[i].TransitionState = 1.0;
											t = "";
										} break;
									default:
										t = Interface.CurrentHudElements[i].Text;
										break;
								}
								// transitions
								float alpha = 1.0f;
								if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Move) != 0) {
									double s = Interface.CurrentHudElements[i].TransitionState;
									x += Interface.CurrentHudElements[i].TransitionVector.X * s * s;
									y += Interface.CurrentHudElements[i].TransitionVector.Y * s * s;
								}
								if ((Interface.CurrentHudElements[i].Transition & Interface.HudTransition.Fade) != 0) {
									alpha = (float)(1.0 - Interface.CurrentHudElements[i].TransitionState);
								} else if (Interface.CurrentHudElements[i].Transition == Interface.HudTransition.None) {
									alpha = (float)(1.0 - Interface.CurrentHudElements[i].TransitionState);
								}
								/// render
								if (alpha != 0.0f) {
									// background
									if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
										float r, g, b, a;
										CreateBackColor(Interface.CurrentHudElements[i].BackgroundColor, sc, out r, out g, out b, out a);
										Gl.glColor4f(r, g, b, a * alpha);
										RenderOverlayTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, x, y, x + w, y + h);
									}
									{ // text
										float u, v;
										MeasureString(t, Interface.CurrentHudElements[i].TextSize, out u, out v);
										double p = Math.Round(Interface.CurrentHudElements[i].TextAlignment.X < 0 ? x : Interface.CurrentHudElements[i].TextAlignment.X == 0 ? x + 0.5 * (w - u) : x + w - u);
										double q = Math.Round(Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? y : Interface.CurrentHudElements[i].TextAlignment.Y == 0 ? y + 0.5 * (h - v) : y + h - v);
										p += Interface.CurrentHudElements[i].TextPosition.X;
										q += Interface.CurrentHudElements[i].TextPosition.Y;
										float r, g, b, a;
										CreateTextColor(Interface.CurrentHudElements[i].TextColor, sc, out r, out g, out b, out a);
										RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, r, g, b, a * alpha, Interface.CurrentHudElements[i].TextShadow);
									}
									// overlay
									if (Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex >= 0) {
										int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
										float r, g, b, a;
										CreateBackColor(Interface.CurrentHudElements[i].OverlayColor, sc, out r, out g, out b, out a);
										Gl.glColor4f(r, g, b, a * alpha);
										RenderOverlayTexture(Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex, x, y, x + w, y + h);
									}
								}
							} break;
					}
				}
				// marker
				if (Interface.CurrentOptions.GameMode != Interface.GameMode.Expert) {
					double y = 8.0;
					for (int i = 0; i < Game.MarkerTextures.Length; i++) {
						int t = TextureManager.UseTexture(Game.MarkerTextures[i], TextureManager.UseMode.LoadImmediately);
						if (t >= 0) {
							double w = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipWidth;
							double h = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipHeight;
							Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
							RenderOverlayTexture(Game.MarkerTextures[i], (double)ScreenWidth - w - 8.0, y, (double)ScreenWidth - 8.0, y + h);
							y += h + 8.0;
						}
					}
				}
				// timetable
				if (Timetable.CurrentTimetable == Timetable.TimetableState.Default) {
					// default
					int t = Timetable.DefaultTimetableTexture;
					if (t >= 0) {
						int w = TextureManager.Textures[t].ClipWidth;
						int h = TextureManager.Textures[t].ClipHeight;
						Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
						RenderOverlayTexture(t, (double)(ScreenWidth - w), Timetable.DefaultTimetablePosition, (double)ScreenWidth, (double)h + Timetable.DefaultTimetablePosition);
					}
				} else if (Timetable.CurrentTimetable == Timetable.TimetableState.Custom & Timetable.CustomObjectsUsed == 0) {
					// custom
					int td = Timetable.CurrentCustomTimetableDaytimeTextureIndex;
					if (td >= 0) {
						int w = TextureManager.Textures[td].ClipWidth;
						int h = TextureManager.Textures[td].ClipHeight;
						Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
						RenderOverlayTexture(td, (double)(ScreenWidth - w), Timetable.CustomTimetablePosition, (double)ScreenWidth, (double)h + Timetable.CustomTimetablePosition);
					}
					int tn = Timetable.CurrentCustomTimetableDaytimeTextureIndex;
					if (tn >= 0) {
						int w = TextureManager.Textures[tn].ClipWidth;
						int h = TextureManager.Textures[tn].ClipHeight;
						float alpha;
						if (td >= 0) {
							double t = (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].Brightness.PreviousTrackPosition) / (TrainManager.PlayerTrain.Cars[0].Brightness.NextTrackPosition - TrainManager.PlayerTrain.Cars[0].Brightness.PreviousTrackPosition);
							alpha = (float)((1.0 - t) * TrainManager.PlayerTrain.Cars[0].Brightness.PreviousBrightness + t * TrainManager.PlayerTrain.Cars[0].Brightness.NextBrightness);
						} else {
							alpha = 1.0f;
						}
						Gl.glColor4f(1.0f, 1.0f, 1.0f, alpha);
						RenderOverlayTexture(tn, (double)(ScreenWidth - w), Timetable.CustomTimetablePosition, (double)ScreenWidth, (double)h + Timetable.CustomTimetablePosition);
					}
				}
			} else if (CurrentOutputMode == OutputMode.Debug) {
				// debug
				Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
				RenderOverlaySolid(0.0f, 0.0f, (double)ScreenWidth, (double)ScreenHeight);
				// actual handles
				{
					string t = "actual: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == 1 ? "F" : "N");
					if (TrainManager.PlayerTrain.Specs.SingleHandle) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
					} else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Actual == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					} else {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Actual.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Actual.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : "N");
					}
					RenderString(2.0, ScreenHeight - 46.0, Fonts.FontType.Small, t, -1, 1.0f, 1.0f, 1.0f, true);
				}
				// safety handles
				{
					string t = "safety: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Actual == 1 ? "F" : "N");
					if (TrainManager.PlayerTrain.Specs.SingleHandle) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
					} else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Safety == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Safety == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					} else {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Safety.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Safety.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Actual ? "HLD" : "N");
					}
					RenderString(2.0, ScreenHeight - 32.0, Fonts.FontType.Small, t, -1, 1.0f, 1.0f, 1.0f, true);
				}
				// driver handles
				{
					string t = "driver: " + (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver == -1 ? "B" : TrainManager.PlayerTrain.Specs.CurrentReverser.Driver == 1 ? "F" : "N");
					if (TrainManager.PlayerTrain.Specs.SingleHandle) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver ? "HLD" : TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
					} else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					} else {
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver != 0 ? "P" + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture) : "N");
						t += " - " + (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver != 0 ? "B" + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture) : TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver ? "HLD" : "N");
					}
					RenderString(2.0, ScreenHeight - 18.0, Fonts.FontType.Small, t, -1, 1.0f, 1.0f, 1.0f, true);
				}
				// debug information
				int texturesLoaded = 0;
				int texturesRegistered = 0;
				for (int i = 0; i < TextureManager.Textures.Length; i++) {
					if (TextureManager.Textures[i] != null) {
						if (TextureManager.Textures[i].Loaded) {
							texturesLoaded++;
						}
						texturesRegistered++;
					}
				}
				int soundsPlaying = 0;
				int soundsRegistered = 0;
				for (int i = 0; i < SoundManager.SoundSources.Length; i++) {
					if (SoundManager.SoundSources[i] != null) {
						if (!SoundManager.SoundSources[i].Suppressed) {
							soundsPlaying++;
						}
						soundsRegistered++;
					}
				}
				int car = 0;
				for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++) {
					if (TrainManager.PlayerTrain.Cars[i].Specs.IsMotorCar) {
						car = i;
						break;
					}
				}
				double mass = 0.0;
				for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++) {
					mass += TrainManager.PlayerTrain.Cars[i].Specs.MassCurrent;
				}
				string[] Lines = new string[] {
					"=system",
					"fps: " + Game.InfoFrameRate.ToString("0.0", Culture) + (MainLoop.LimitFramerate ? " (low cpu)" : ""),
					"score: " + Game.CurrentScore.Value.ToString(Culture),
					"",
					"=train",
					"speed: " + (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6).ToString("0.00", Culture) + " km/h",
					"power (car " + car.ToString(Culture) +  "): " + (TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput < 0.0 ? TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)Math.Sign(TrainManager.PlayerTrain.Cars[car].Specs.CurrentSpeed) : TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)TrainManager.PlayerTrain.Specs.CurrentReverser.Actual).ToString("0.0000", Culture) + " m/s²",
					"acceleration: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²",
					"position: " + (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].FrontAxlePosition + 0.5 * TrainManager.PlayerTrain.Cars[0].Length).ToString("0.00", Culture) + " m",
					"elevation: " + (Game.RouteInitialElevation + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m",
					"temperature: " + (TrainManager.PlayerTrain.Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + " °C",
					"air pressure: " + (0.001 * TrainManager.PlayerTrain.Specs.CurrentAirPressure).ToString("0.00", Culture) + " kPa",
					"air density: " + TrainManager.PlayerTrain.Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³",
					"speed of sound: " + (Game.GetSpeedOfSound(TrainManager.PlayerTrain.Specs.CurrentAirDensity) * 3.6).ToString("0.00", Culture) + " km/h",
					"passenger ratio: " + TrainManager.PlayerTrain.Passengers.PassengerRatio.ToString("0.00"),
					"total mass: " + mass.ToString("0.00", Culture) + " kg",
					"plugin: " + (TrainManager.PlayerTrain.Specs.Safety.Mode == TrainManager.SafetySystem.Plugin ? (PluginManager.PluginValid ? "ok" : "error") : "n/a"),
					"",
					"=route",
					"track limit: " + (TrainManager.PlayerTrain.CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h")),
					"signal limit: " + (TrainManager.PlayerTrain.CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h")),
					"atc limit: " + (TrainManager.PlayerTrain.Specs.Safety.Atc.Available ? (TrainManager.PlayerTrain.Specs.Safety.Atc.SpeedRestriction == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.Specs.Safety.Atc.SpeedRestriction * 3.6).ToString("0.0", Culture) + " km/h")) : "n/a"),
					"total static objects: " + ObjectManager.ObjectsUsed.ToString(Culture),
					"total static GL_TRIANGLES: " + Game.InfoTotalTriangles.ToString(Culture),
					"total static GL_TRIANGLE_STRIP: " + Game.InfoTotalTriangleStrip.ToString(Culture),
					"total static GL_QUADS: " + Game.InfoTotalQuads.ToString(Culture),
					"total static GL_QUAD_STRIP: " + Game.InfoTotalQuadStrip.ToString(Culture),
					"total static GL_POLYGON: " + Game.InfoTotalPolygon.ToString(Culture),
					"total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed.ToString(Culture),
					"",
					"=renderer",
					"world opaque faces: " + OpaqueListCount[0].ToString(Culture),
					"world transparent faces: " + TransparentColorListCount[0].ToString(Culture),
					"world alpha faces: " + AlphaListCount[0].ToString(Culture),
					"cab opaque faces: " + OpaqueListCount[1].ToString(Culture),
					"cab transparent faces: " + TransparentColorListCount[1].ToString(Culture),
					"cab alpha faces: " + AlphaListCount[1].ToString(Culture),
					"textures loaded: " + texturesLoaded.ToString(Culture),
					"textures registered: " + texturesRegistered.ToString(Culture),
					"",
					"=camera",
					"position: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m",
					"curve radius: " + World.CameraTrackFollower.CurveRadius.ToString("0.00", Culture) + " m",
					"curve cant: " + (1000.0 * Math.Abs(World.CameraTrackFollower.CurveCant)).ToString("0.00", Culture) + " mm" + (World.CameraTrackFollower.CurveCant < 0.0 ? " (left)" : World.CameraTrackFollower.CurveCant > 0.0 ? " (right)" : ""),
					"",
					"=sound",
					"sounds playing: " + soundsPlaying.ToString(Culture),
					"sounds registered: " + soundsRegistered.ToString(Culture),
					"outer radius factor: " + SoundManager.OuterRadiusFactor.ToString("0.00"),
					"",
					Game.InfoDebugString ?? ""
				};
				double x = 4.0;
				double y = 4.0;
				for (int i = 0; i < Lines.Length; i++) {
					if (Lines[i].Length != 0) {
						if (Lines[i][0] == '=') {
							string text = Lines[i].Substring(1);
							float width, height;
							MeasureString(text, Fonts.FontType.Small, out width, out height);
							Gl.glColor4f(0.5f, 0.5f, 0.7f, 0.7f);
							RenderOverlaySolid(x, y, x + width + 4.0f, y + height + 2.0f);
							RenderString(x, y, Fonts.FontType.Small, text, -1, 1.0f, 1.0f, 1.0f, false);
						} else {
							RenderString(x, y, Fonts.FontType.Small, Lines[i], -1, 1.0f, 1.0f, 1.0f, true);
						}
						y += 14.0;
					} else if (y >= (double)ScreenHeight - 240.0) {
						x += 280.0;
						y = 4.0;
					} else {
						y += 14.0;
					}
				}
			}
			// air brake debug output
			if (Interface.CurrentOptions.GameMode != Interface.GameMode.Expert & OptionBrakeSystems) {
				double oy = 64.0, y = oy, h = 16.0;
				bool[] heading = new bool[6];
				for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++) {
					double x = 96.0, w = 128.0;
					// brake pipe
					if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake) {
						if (!heading[0]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Brake pipe", -1, 1.0f, 1.0f, 0.0f, true);
							heading[0] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakePipeCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
						Gl.glColor3f(1.0f, 1.0f, 0.0f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					// auxillary reservoir
					if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake) {
						if (!heading[1]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Auxillary reservoir", -1, 0.75f, 0.75f, 0.75f, true);
							heading[1] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
						Gl.glColor3f(0.5f, 0.5f, 0.5f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					// brake cylinder
					{
						if (!heading[2]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Brake cylinder", -1, 0.75f, 0.5f, 0.25f, true);
							heading[2] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						Gl.glColor3f(0.75f, 0.5f, 0.25f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					// main reservoir
					if (TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
						if (!heading[3]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Main reservoir", -1, 1.0f, 0.0f, 0.0f, true);
							heading[3] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.MainReservoirCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.AirCompressorMaximumPressure;
						Gl.glColor3f(1.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					// equalizing reservoir
					if (TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
						if (!heading[4]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Equalizing reservoir", -1, 0.0f, 0.75f, 0.0f, true);
							heading[4] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure;
						Gl.glColor3f(0.0f, 0.75f, 0.0f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					// straight air pipe
					if (TrainManager.PlayerTrain.Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake & TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
						if (!heading[5]) {
							RenderString(x, oy - 16.0, Fonts.FontType.Small, "Straight air pipe", -1, 0.0f, 0.75f, 1.0f, true);
							heading[5] = true;
						}
						Gl.glColor3f(0.0f, 0.0f, 0.0f);
						RenderOverlaySolid(x, y, x + w, y + h);
						double p = TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
						Gl.glColor3f(0.0f, 0.75f, 1.0f);
						RenderOverlaySolid(x, y, x + r * w, y + h);
					} x += w + 8.0;
					Gl.glColor3f(0.0f, 0.0f, 0.0f);
					y += h + 8.0;
				}
			}
			// interface
			if (Game.CurrentInterface == Game.InterfaceType.Pause) {
				// pause
				Gl.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
				RenderOverlaySolid(0.0, 0.0, (double)ScreenWidth, (double)ScreenHeight);
				Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
				if (TexturePause >= 0) {
					double w = (double)TextureManager.Textures[TexturePause].ClipWidth;
					double h = (double)TextureManager.Textures[TexturePause].ClipHeight;
					double x = 0.5 * (double)ScreenWidth;
					double y = 0.5 * (double)ScreenHeight;
					RenderOverlayTexture(TexturePause, x - 0.5 * w, y - 0.5 * h, x + 0.5 * w, y + 0.5 * h);
				} else {
					string s = "PAUSE";
					RenderString(0.5 * (double)ScreenWidth, 0.5 * (double)ScreenHeight, Fonts.FontType.Large, s, 0, 1.0f, 1.0f, 1.0f, true);
				}
			} else if (Game.CurrentInterface == Game.InterfaceType.Menu) {
				// menu
				Gl.glColor4f(0.0f, 0.0f, 0.0f, 0.5f);
				RenderOverlaySolid(0.0, 0.0, (double)ScreenWidth, (double)ScreenHeight);
				Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);
				Game.MenuEntry[] m = Game.CurrentMenu;
				const double ra = 32.0;
				const double rb = 96.0;
				const double rc = 144.0;
				const double w = 256.0;
				const double h = 28.0;
				double x = 0.5 * (double)ScreenWidth - w;
				for (int i = 0; i < Game.CurrentMenuSelection.Length; i++) {
					double o = Game.CurrentMenuOffsets[i];
					double oc = o == double.NegativeInfinity ? 0.0 : o;
					double y = 0.5 * ((double)ScreenHeight - (double)m.Length * h) + oc;
					double ys = y + (double)Game.CurrentMenuSelection[i] * h;
					double ot;
					if (ys < rc) {
						ot = oc + rc - ys;
					} else if (ys > (double)ScreenHeight - rc) {
						ot = oc + (double)ScreenHeight - rc - ys;
					} else {
						ot = oc;
					}
					if (o == double.NegativeInfinity) {
						o = ot;
					} else if (o < ot) {
						double d = ot - o;
						o += (0.03 * d * d + 0.1) * TimeElapsed;
						if (o > ot) o = ot;
					} else if (o > ot) {
						double d = o - ot;
						o -= (0.03 * d * d + 0.1) * TimeElapsed;
						if (o < ot) o = ot;
					}
					Game.CurrentMenuOffsets[i] = o;
					for (int j = 0; j < m.Length; j++) {
						double ta;
						if (y < rb) {
							ta = (y - ra) / (rb - ra);
							if (ta < 0.0) ta = 0.0;
							if (ta > 1.0) ta = 1.0;
						} else if (y > (double)ScreenHeight - rb) {
							ta = ((double)ScreenHeight - y - ra) / (rb - ra);
							if (ta < 0.0) ta = 0.0;
							if (ta > 1.0) ta = 1.0;
						} else {
							ta = 1.0;
						}
						if (ta < m[j].Alpha) {
							m[j].Alpha -= 4.0 * TimeElapsed;
							if (m[j].Alpha < ta) m[j].Alpha = ta;
						} else if (ta > m[j].Alpha) {
							m[j].Alpha += 4.0 * TimeElapsed;
							if (m[j].Alpha > ta) m[j].Alpha = ta;
						}
						if (j == Game.CurrentMenuSelection[i]) {
							m[j].Highlight = 1.0;
						} else {
							m[j].Highlight -= 4.0 * TimeElapsed;
							if (m[j].Highlight < 0.0) m[j].Highlight = 0.0;
						}
						float r = 1.0f;
						float g = 1.0f;
						float b = (float)(1.0 - m[j].Highlight);
						float a = (float)m[j].Alpha;
						if (j == Game.CurrentMenuSelection[i]) {
							RenderString(x, y, Fonts.FontType.Medium, "➢", -1, r, g, b, a, true);
						}
						if (m[j] is Game.MenuCaption) {
							RenderString(x + 24.0, y, Fonts.FontType.Medium, m[j].Text, -1, 0.5f, 0.75f, 1.0f, a, true);
						} else if (m[j] is Game.MenuCommand) {
							RenderString(x + 24.0, y, Fonts.FontType.Medium, m[j].Text, -1, r, g, b, a, true);
						} else {
							RenderString(x + 24.0, y, Fonts.FontType.Medium, m[j].Text + " ➟", -1, r, g, b, a, true);
						}
						y += h;
					}
					x += w;
					Game.MenuSubmenu n = m[Game.CurrentMenuSelection[i]] as Game.MenuSubmenu;
					m = n == null ? null : n.Entries;
				}
			}
			// finalize
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glPopMatrix();
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
			Gl.glDisable(Gl.GL_BLEND);
		}

		// get color
		private static void CreateBackColor(World.ColorRGBA Original, Game.MessageColor SystemColor, out float R, out float G, out float B, out float A) {
			if (Original.R == 0 & Original.G == 0 & Original.B == 0) {
				switch (SystemColor) {
					case Game.MessageColor.Black:
						R = 0.0f; G = 0.0f; B = 0.0f;
						break;
					case Game.MessageColor.Gray:
						R = 0.4f; G = 0.4f; B = 0.4f;
						break;
					case Game.MessageColor.White:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case Game.MessageColor.Red:
						R = 1.0f; G = 0.0f; B = 0.0f;
						break;
					case Game.MessageColor.Orange:
						R = 0.9f; G = 0.7f; B = 0.0f;
						break;
					case Game.MessageColor.Green:
						R = 0.2f; G = 0.8f; B = 0.0f;
						break;
					case Game.MessageColor.Blue:
						R = 0.0f; G = 0.7f; B = 1.0f;
						break;
					case Game.MessageColor.Magenta:
						R = 1.0f; G = 0.0f; B = 0.7f;
						break;
					default:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
				}
			} else {
				R = inv255 * (float)Original.R;
				G = inv255 * (float)Original.G;
				B = inv255 * (float)Original.B;
			}
			A = inv255 * (float)Original.A;
		}
		private static void CreateTextColor(World.ColorRGBA Original, Game.MessageColor SystemColor, out float R, out float G, out float B, out float A) {
			if (Original.R == 0 & Original.G == 0 & Original.B == 0) {
				switch (SystemColor) {
					case Game.MessageColor.Black:
						R = 0.0f; G = 0.0f; B = 0.0f;
						break;
					case Game.MessageColor.Gray:
						R = 0.4f; G = 0.4f; B = 0.4f;
						break;
					case Game.MessageColor.White:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case Game.MessageColor.Red:
						R = 1.0f; G = 0.0f; B = 0.0f;
						break;
					case Game.MessageColor.Orange:
						R = 0.9f; G = 0.7f; B = 0.0f;
						break;
					case Game.MessageColor.Green:
						R = 0.3f; G = 1.0f; B = 0.0f;
						break;
					case Game.MessageColor.Blue:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
					case Game.MessageColor.Magenta:
						R = 1.0f; G = 0.0f; B = 0.7f;
						break;
					default:
						R = 1.0f; G = 1.0f; B = 1.0f;
						break;
				}
			} else {
				R = inv255 * (float)Original.R;
				G = inv255 * (float)Original.G;
				B = inv255 * (float)Original.B;
			}
			A = inv255 * (float)Original.A;
		}

		// render string
		private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, bool Shadow) {
			RenderString(PixelLeft, PixelTop, FontType, Text, Orientation, R, G, B, 1.0f, Shadow);
		}
		private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, float A, bool Shadow) {
			if (Text == null) return;
			int Font = (int)FontType;
			double c = 1;
			double x = PixelLeft;
			double y = PixelTop;
			double tw = 0.0;
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				Fonts.GetTextureIndex(FontType, Text[i]);
				tw += Fonts.Characters[Font][a].Width;
			}
			if (Orientation == 0) {
				x -= 0.5 * tw;
			} else if (Orientation == 1) {
				x -= tw;
			}
			for (int i = 0; i < Text.Length; i++) {
				int b = char.ConvertToUtf32(Text, i);
				int t = Fonts.GetTextureIndex(FontType, Text[i]);
				double w = (double)TextureManager.Textures[t].ClipWidth;
				double h = (double)TextureManager.Textures[t].ClipHeight;
				Gl.glBlendFunc(Gl.GL_ZERO, Gl.GL_ONE_MINUS_SRC_COLOR);
				Gl.glColor3f(A, A, A);
				RenderOverlayTexture(t, x, y, x + w, y + h);
				if (Shadow) {
					RenderOverlayTexture(t, x + c, y + c, x + w, y + h);
				}
				Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
				Gl.glColor4f(R, G, B, A);
				RenderOverlayTexture(t, x, y, x + w, y + h);
				x += Fonts.Characters[Font][b].Width;
			}
			Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
		}
		private static void MeasureString(string Text, Fonts.FontType FontType, out float Width, out float Height) {
			Width = 0.0f;
			Height = 0.0f;
			if (Text == null) return;
			int Font = (int)FontType;
			for (int i = 0; i < Text.Length; i++) {
				int Codepoint = char.ConvertToUtf32(Text, i);
				int Texture = Fonts.GetTextureIndex(FontType, Text[i]);
				Width += Fonts.Characters[Font][Codepoint].Width;
				if (Fonts.Characters[Font][Codepoint].Height > Height) {
					Height = Fonts.Characters[Font][Codepoint].Height;
				}
			}
		}

		// render overlay texture
		private static void RenderOverlayTexture(int TextureIndex, double ax, double ay, double bx, double by) {
			double nay = (double)ScreenHeight - ay;
			double nby = (double)ScreenHeight - by;
			TextureManager.UseTexture(TextureIndex, TextureManager.UseMode.LoadImmediately);
			if (TextureIndex >= 0) {
				int OpenGlTextureIndex = TextureManager.Textures[TextureIndex].OpenGlTextureIndex;
				if (!TexturingEnabled) {
					Gl.glEnable(Gl.GL_TEXTURE_2D);
					TexturingEnabled = true;
				}
				Gl.glBindTexture(Gl.GL_TEXTURE_2D, OpenGlTextureIndex);
			} else if (TexturingEnabled) {
				Gl.glDisable(Gl.GL_TEXTURE_2D);
				TexturingEnabled = false;
			}
			Gl.glBegin(Gl.GL_QUADS);
			Gl.glTexCoord2d(0.0, 1.0);
			Gl.glVertex2d(ax, nby);
			Gl.glTexCoord2d(0.0, 0.0);
			Gl.glVertex2d(ax, nay);
			Gl.glTexCoord2d(1.0, 0.0);
			Gl.glVertex2d(bx, nay);
			Gl.glTexCoord2d(1.0, 1.0);
			Gl.glVertex2d(bx, nby);
			Gl.glEnd();
		}

		// render overlay solid
		private static void RenderOverlaySolid(double ax, double ay, double bx, double by) {
			double nay = (double)ScreenHeight - ay;
			double nby = (double)ScreenHeight - by;
			if (TexturingEnabled) {
				Gl.glDisable(Gl.GL_TEXTURE_2D);
				TexturingEnabled = false;
			}
			Gl.glBegin(Gl.GL_QUADS);
			Gl.glVertex2d(ax, nby);
			Gl.glVertex2d(ax, nay);
			Gl.glVertex2d(bx, nay);
			Gl.glVertex2d(bx, nby);
			Gl.glEnd();
		}

		// re-add objects
		private static void ReAddObjects() {
			Object[] List = new Object[ObjectListCount];
			for (int i = 0; i < ObjectListCount; i++) {
				List[i] = ObjectList[i];
			}
			for (int i = 0; i < List.Length; i++) {
				HideObject(List[i].ObjectIndex);
			}
			for (int i = 0; i < List.Length; i++) {
				ShowObject(List[i].ObjectIndex, List[i].Overlay);
			}
		}

		// show object
		internal static void ShowObject(int ObjectIndex, bool Overlay) {
			if (ObjectManager.Objects[ObjectIndex] == null) return;
			if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0) {
				if (ObjectListCount >= ObjectList.Length) {
					Array.Resize<Object>(ref ObjectList, ObjectList.Length << 1);
				}
				ObjectList[ObjectListCount].ObjectIndex = ObjectIndex;
				ObjectList[ObjectListCount].Overlay = Overlay;

				int f = ObjectManager.Objects[ObjectIndex].Mesh.Faces.Length;
				ObjectList[ObjectListCount].FaceListIndices = new int[f];
				for (int i = 0; i < f; i++) {
					bool transparentcolor = false, alpha = false;
					if (Overlay & World.CameraRestriction != World.CameraRestrictionMode.NotAvailable) {
						alpha = true;
					} else {
						int k = ObjectManager.Objects[ObjectIndex].Mesh.Faces[i].Material;
						if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].Color.A != 255) {
							alpha = true;
						} else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].BlendMode == World.MeshMaterialBlendMode.Additive) {
							alpha = true;
						} else if (ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].GlowAttenuationData != 0) {
							alpha = true;
						} else {
							int tday = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].DaytimeTextureIndex;
							if (tday >= 0) {
								TextureManager.UseTexture(tday, TextureManager.UseMode.Normal);
								if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
									alpha = true;
								} else if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
									transparentcolor = true;
								}
							}
							int tnight = ObjectManager.Objects[ObjectIndex].Mesh.Materials[k].NighttimeTextureIndex;
							if (tnight >= 0) {
								TextureManager.UseTexture(tnight, TextureManager.UseMode.Normal);
								if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
									alpha = true;
								} else if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
									transparentcolor = true;
								}
							}
						}
					}
					int listLayer = Overlay ? 1 : 0;
					int listOffset = listLayer << 2;
					if (alpha) {
						// alpha
						if (AlphaListCount[listLayer] >= AlphaList[listLayer].Length) {
							Array.Resize(ref AlphaList[listLayer], AlphaList[listLayer].Length << 1);
							Array.Resize(ref AlphaListDistance[listLayer], AlphaList[listLayer].Length);
						}
						AlphaList[listLayer][AlphaListCount[listLayer]].ObjectIndex = ObjectIndex;
						AlphaList[listLayer][AlphaListCount[listLayer]].FaceIndex = i;
						AlphaList[listLayer][AlphaListCount[listLayer]].ObjectListIndex = ObjectListCount;
						ObjectList[ObjectListCount].FaceListIndices[i] = (AlphaListCount[listLayer] << 3) + listOffset + 2;
						AlphaListCount[listLayer]++;
					} else if (transparentcolor) {
						// transparent color
						if (TransparentColorListCount[listLayer] >= TransparentColorList[listLayer].Length) {
							Array.Resize(ref TransparentColorList[listLayer], TransparentColorList[listLayer].Length << 1);
							Array.Resize(ref TransparentColorListDistance[listLayer], TransparentColorList[listLayer].Length);
						}
						TransparentColorList[listLayer][TransparentColorListCount[listLayer]].ObjectIndex = ObjectIndex;
						TransparentColorList[listLayer][TransparentColorListCount[listLayer]].FaceIndex = i;
						TransparentColorList[listLayer][TransparentColorListCount[listLayer]].ObjectListIndex = ObjectListCount;
						ObjectList[ObjectListCount].FaceListIndices[i] = (TransparentColorListCount[listLayer] << 3) + listOffset + 1;
						TransparentColorListCount[listLayer]++;
					} else {
						// opaque
						if (OpaqueListCount[listLayer] >= OpaqueList[listLayer].Length) {
							Array.Resize(ref OpaqueList[listLayer], OpaqueList[listLayer].Length << 1);
						}
						OpaqueList[listLayer][OpaqueListCount[listLayer]].ObjectIndex = ObjectIndex;
						OpaqueList[listLayer][OpaqueListCount[listLayer]].FaceIndex = i;
						OpaqueList[listLayer][OpaqueListCount[listLayer]].ObjectListIndex = ObjectListCount;
						ObjectList[ObjectListCount].FaceListIndices[i] = (OpaqueListCount[listLayer] << 3) + listOffset;
						OpaqueListCount[listLayer]++;
					}
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectListCount + 1;
				ObjectListCount++;
			}
		}

		// hide object
		internal static void HideObject(int ObjectIndex) {
			if (ObjectManager.Objects[ObjectIndex] == null) return;
			int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
			if (k >= 0) {
				// remove faces
				for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
					int listReference = ObjectList[k].FaceListIndices[i];
					int listLayer = (listReference & 7) >> 2;
					int listType = listReference & 3;
					int listIndex = listReference >> 3;
					switch (listType) {
						case 0:
							// opaque
							OpaqueList[listLayer][listIndex] = OpaqueList[listLayer][OpaqueListCount[listLayer] - 1];
							OpaqueListCount[listLayer]--;
							ObjectList[OpaqueList[listLayer][listIndex].ObjectListIndex].FaceListIndices[OpaqueList[listLayer][listIndex].FaceIndex] = listReference;
							break;
						case 1:
							// transparent color
							TransparentColorList[listLayer][listIndex] = TransparentColorList[listLayer][TransparentColorListCount[listLayer] - 1];
							TransparentColorListCount[listLayer]--;
							ObjectList[TransparentColorList[listLayer][listIndex].ObjectListIndex].FaceListIndices[TransparentColorList[listLayer][listIndex].FaceIndex] = listReference;
							break;
						case 2:
							// alpha
							AlphaList[listLayer][listIndex] = AlphaList[listLayer][AlphaListCount[listLayer] - 1];
							AlphaListCount[listLayer]--;
							ObjectList[AlphaList[listLayer][listIndex].ObjectListIndex].FaceListIndices[AlphaList[listLayer][listIndex].FaceIndex] = listReference;
							break;
					}
				}
				// remove object
				if (k == ObjectListCount - 1) {
					ObjectListCount--;
				} else {
					ObjectList[k] = ObjectList[ObjectListCount - 1];
					ObjectListCount--;
					for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
						int listReference = ObjectList[k].FaceListIndices[i];
						int listLayer = (listReference & 7) >> 2;
						int listType = listReference & 3;
						int listIndex = listReference >> 3;
						switch (listType) {
							case 0:
								OpaqueList[listLayer][listIndex].ObjectListIndex = k;
								break;
							case 1:
								TransparentColorList[listLayer][listIndex].ObjectListIndex = k;
								break;
							case 2:
								AlphaList[listLayer][listIndex].ObjectListIndex = k;
								break;
						}
					}
					ObjectManager.Objects[ObjectList[k].ObjectIndex].RendererIndex = k + 1;
				}
				ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
			}
		}

		// sort polygons
		private static void SortPolygons(ObjectFace[] List, int ListCount, double[] ListDistance, int ListIndex, double TimeElapsed) {
			// calculate distance
			double cx = World.AbsoluteCameraPosition.X;
			double cy = World.AbsoluteCameraPosition.Y;
			double cz = World.AbsoluteCameraPosition.Z;
			for (int i = 0; i < ListCount; i++) {
				int o = List[i].ObjectIndex;
				int f = List[i].FaceIndex;
				if (ObjectManager.Objects[o].Mesh.Faces[f].Vertices.Length >= 3) {
					int v0 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[0].Index;
					int v1 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[1].Index;
					int v2 = ObjectManager.Objects[o].Mesh.Faces[f].Vertices[2].Index;
					double v0x = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.X;
					double v0y = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Y;
					double v0z = ObjectManager.Objects[o].Mesh.Vertices[v0].Coordinates.Z;
					double v1x = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.X;
					double v1y = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Y;
					double v1z = ObjectManager.Objects[o].Mesh.Vertices[v1].Coordinates.Z;
					double v2x = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.X;
					double v2y = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Y;
					double v2z = ObjectManager.Objects[o].Mesh.Vertices[v2].Coordinates.Z;
					double w1x = v1x - v0x, w1y = v1y - v0y, w1z = v1z - v0z;
					double w2x = v2x - v0x, w2y = v2y - v0y, w2z = v2z - v0z;
					double dx = -w1z * w2y + w1y * w2z;
					double dy = w1z * w2x - w1x * w2z;
					double dz = -w1y * w2x + w1x * w2y;
					double t = dx * dx + dy * dy + dz * dz;
					if (t != 0.0) {
						t = 1.0 / Math.Sqrt(t);
						dx *= t; dy *= t; dz *= t;
						double w0x = v0x - cx, w0y = v0y - cy, w0z = v0z - cz;
						t = dx * w0x + dy * w0y + dz * w0z;
						ListDistance[i] = -t * t;
					}
				}
			}
			// sort
			Array.Sort<double, ObjectFace>(ListDistance, List, 0, ListCount);
			// update object list
			for (int i = 0; i < ListCount; i++) {
				ObjectList[List[i].ObjectListIndex].FaceListIndices[List[i].FaceIndex] = (i << 3) + ListIndex;
			}
		}

		// get distance factor
		private static double GetDistanceFactor(World.Vertex[] Vertices, ref World.MeshFace Face, ushort GlowAttenuationData, double CameraX, double CameraY, double CameraZ) {
			if (Face.Vertices.Length != 0) {
				World.GlowAttenuationMode mode; double halfdistance;
				World.SplitGlowAttenuationData(GlowAttenuationData, out mode, out halfdistance);
				int i = (int)Face.Vertices[0].Index;
				double dx = Vertices[i].Coordinates.X - CameraX;
				double dy = Vertices[i].Coordinates.Y - CameraY;
				double dz = Vertices[i].Coordinates.Z - CameraZ;
				switch (mode) {
					case World.GlowAttenuationMode.DivisionExponent2:
						{
							double t = dx * dx + dy * dy + dz * dz;
							return t / (t + halfdistance * halfdistance);
						}
					case World.GlowAttenuationMode.DivisionExponent4:
						{
							double t = dx * dx + dy * dy + dz * dz;
							t *= t; halfdistance *= halfdistance;
							return t / (t + halfdistance * halfdistance);
						}
					default:
						return 1.0;
				}
			} else {
				return 1.0;
			}
		}

	}
}