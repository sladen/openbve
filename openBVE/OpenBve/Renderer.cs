using System;
using Tao.OpenGl;

namespace OpenBve {
    internal static class Renderer {

        // screen (output window)
        internal static int ScreenWidth;
        internal static int ScreenHeight;

        // first frame behavior
        internal enum LoadTextureImmediatelyMode { NotYet, Yes, NoLonger }
        internal static LoadTextureImmediatelyMode LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
        internal enum TransparencyMode { Sharp, Smooth }

        // object list
        private struct Object {
            internal int ObjectIndex;
            internal int[][] FaceListIndices;
            internal bool Overlay;
        }
        private static Object[] ObjectList = new Object[256];
        private static int ObjectListCount = 0;

        // face lists
        private struct ObjectFace {
            internal int ObjectListIndex;
            internal int ObjectIndex;
            internal int MeshIndex;
            internal int FaceIndex;
        }
        /// opaque
        private static ObjectFace[] OpaqueList = new ObjectFace[256];
        internal static int OpaqueListCount = 0;
        /// transparent color
        private static ObjectFace[] TransparentColorList = new ObjectFace[256];
        private static double[] TransparentColorListDistance = new double[256];
        internal static int TransparentColorListCount = 0;
        /// alpha
        private static ObjectFace[] AlphaList = new ObjectFace[256];
        private static double[] AlphaListDistance = new double[256];
        internal static int AlphaListCount = 0;
        /// overlay
        private static ObjectFace[] OverlayList = new ObjectFace[256];
        private static double[] OverlayListDistance = new double[256];
        internal static int OverlayListCount = 0;

        // current opengl data
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
        internal static World.Vector3Df OptionLightPosition = new World.Vector3Df(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
        internal static float OptionLightingResultingAmount = 1.0f;
        internal static bool OptionNormals = false;
        internal static bool OptionWireframe = false;
        internal static bool OptionBackfaceCulling = true;

        // interface options
        internal static bool OptionTimetable = false;
        internal static double OptionTimetablePosition = 0.0;
        internal static bool OptionClock = false;
        internal enum SpeedDisplayMode { None, Kmph, Mph }
        internal static SpeedDisplayMode OptionSpeed = SpeedDisplayMode.None;
        internal static bool OptionBrakeSystems = false;

        // textures
        private static int TextureLogo = -1;
        private static int TexturePause = -1;
        private static int TextureDriver = -1;
        private static int TextureStopMeterBar = -1;
        private static int TextureStopMeterTick = -1;
        private static int TextureStopMeterNo = -1;
        private static int TextureReverser = -1;
        private static int TexturePower = -1;
        private static int TextureBrake = -1;
        private static int TextureSingle = -1;
        private static int TextureDoors = -1;
        private static int TextureLamp = -1;
        internal static string FilePause = "pause.png";
        internal static string FileDriver = "driver.png";
        internal static string FileReverser = "lamp_32.png";
        internal static string FilePower = "lamp_32.png";
        internal static string FileBrake = "lamp_32.png";
        internal static string FileSingle = "lamp_32.png";
        internal static string FileDoors = "lamp_32.png";
        internal static string FileLamp = "lamp_112.png";

        // constants
        private const float inv255 = 1.0f / 255.0f;

        // reset
        internal static void Reset() {
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NotYet;
            ObjectList = new Object[256];
            ObjectListCount = 0;
            OpaqueList = new ObjectFace[256];
            OpaqueListCount = 0;
            TransparentColorList = new ObjectFace[256];
            TransparentColorListDistance = new double[256];
            TransparentColorListCount = 0;
            AlphaList = new ObjectFace[256];
            AlphaListDistance = new double[256];
            AlphaListCount = 0;
            OverlayList = new ObjectFace[256];
            OverlayListDistance = new double[256];
            OverlayListCount = 0;
            OptionLighting = true;
            OptionAmbientColor = new World.ColorRGB(160, 160, 160);
            OptionDiffuseColor = new World.ColorRGB(160, 160, 160);
            OptionLightPosition = new World.Vector3Df(0.215920077052065f, 0.875724044222352f, -0.431840154104129f);
            OptionLightingResultingAmount = 1.0f;
            OptionTimetablePosition = 0.0;
            OptionClock = false;
            OptionBrakeSystems = false;
        }

        // initialize
        internal static void Initialize() {
            // opengl
            Gl.glShadeModel(Gl.GL_DECAL);
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
            // textures
            string Path = Interface.GetCombinedFolderName(System.Windows.Forms.Application.StartupPath, "Graphics");
            TextureLogo = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "logo.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
            TexturePause = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FilePause), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureDriver = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileDriver), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureStopMeterBar = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "stopmeter_bar.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureStopMeterTick = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "stopmeter_tick.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureStopMeterNo = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "stopmeter_no.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureReverser = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileReverser), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TexturePower = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FilePower), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureBrake = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileBrake), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureSingle = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileSingle), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureDoors = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileDoors), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureLamp = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, FileLamp), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, true);
            TextureManager.ValidateTexture(ref TextureLogo);
            TextureManager.ValidateTexture(ref TexturePause);
            TextureManager.ValidateTexture(ref TextureDriver);
            TextureManager.ValidateTexture(ref TextureReverser);
            TextureManager.ValidateTexture(ref TexturePower);
            TextureManager.ValidateTexture(ref TextureBrake);
            TextureManager.ValidateTexture(ref TextureSingle);
            TextureManager.ValidateTexture(ref TextureDoors);
            TextureManager.ValidateTexture(ref TextureStopMeterBar);
            TextureManager.ValidateTexture(ref TextureStopMeterTick);
            TextureManager.ValidateTexture(ref TextureStopMeterNo);
            TextureManager.ValidateTexture(ref TextureLamp);
            // opengl
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glPushMatrix();
            Gl.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            Glu.gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 0.0);
            Gl.glPopMatrix();
            TransparentColorDepthSorting = Interface.CurrentOptions.TransparencyMode == TransparencyMode.Smooth & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.NearestNeighbor & Interface.CurrentOptions.Interpolation != TextureManager.InterpolationMode.Bilinear;
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
            double size = ScreenWidth > ScreenHeight ? ScreenWidth : ScreenHeight;
            Gl.glColor3f(1.0f, 1.0f, 1.0f);
            RenderOverlayTexture(TextureLogo, 0.5 * (ScreenWidth - size), 0.5 * (ScreenHeight - size), 0.5 * (ScreenWidth + size), 0.5 * (ScreenHeight + size));
            RenderString(0.5 * (double)ScreenWidth, (double)ScreenHeight - 24.0, Fonts.FontType.Small, Interface.GetInterfaceString("message_loading"), 0, 255, 255, 255, true);
            // finalize
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glDisable(Gl.GL_BLEND);
        }

        // initialize lighting
        internal static void InitializeLighting() {
            if (OptionAmbientColor.R == 255 & OptionAmbientColor.G == 255 & OptionAmbientColor.B == 255 & OptionDiffuseColor.R == 0 & OptionDiffuseColor.G == 0 & OptionDiffuseColor.B == 0) {
                OptionLighting = false;
            } else {
                OptionLighting = true;
            }
            if (OptionLighting) {
                if (Interface.CurrentOptions.AlternativeLighting) {
                    float ar = inv255 * (float)OptionAmbientColor.R;
                    float ag = inv255 * (float)OptionAmbientColor.G;
                    float ab = inv255 * (float)OptionAmbientColor.B;
                    float dr = inv255 * (float)OptionDiffuseColor.R;
                    float dg = inv255 * (float)OptionDiffuseColor.G;
                    float db = inv255 * (float)OptionDiffuseColor.B;
                    if (ar < 0.0f) ar = 0.0f;
                    if (ar > 1.0f) ar = 1.0f;
                    if (ag < 0.0f) ag = 0.0f;
                    if (ag > 1.0f) ag = 1.0f;
                    if (ab < 0.0f) ab = 0.0f;
                    if (ab > 1.0f) ab = 1.0f;
                    if (dr != 0.0 & ar + dr > 1.0) {
                        float f = (1.0f - ar) / dr;
                        dr *= f; dg *= f; db *= f;
                    }
                    if (dg != 0.0 & ag + dg > 1.0) {
                        float f = (1.0f - ag) / dg;
                        dr *= f; dg *= f; db *= f;
                    }
                    if (db != 0.0 & ab + db > 1.0) {
                        float f = (1.0f - ab) / db;
                        dr *= f; dg *= f; db *= f;
                    }
                    if (dr < 0.0f) dr = 0.0f;
                    if (dr > 1.0f) dr = 1.0f;
                    if (dg < 0.0f) dg = 0.0f;
                    if (dg > 1.0f) dg = 1.0f;
                    if (db < 0.0f) db = 0.0f;
                    if (db > 1.0f) db = 1.0f;
                    Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { ar, ag, ab, 1.0f });
                    Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { dr, dg, db, 1.0f });
                } else {
                    Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, new float[] { inv255 * (float)OptionAmbientColor.R, inv255 * (float)OptionAmbientColor.G, inv255 * (float)OptionAmbientColor.B, 1.0f });
                    Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, new float[] { inv255 * (float)OptionDiffuseColor.R, inv255 * (float)OptionDiffuseColor.G, inv255 * (float)OptionDiffuseColor.B, 1.0f });
                }
                Gl.glLightModelfv(Gl.GL_LIGHT_MODEL_AMBIENT, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                Gl.glCullFace(Gl.GL_FRONT); CullEnabled = true;
                Gl.glEnable(Gl.GL_LIGHTING); LightingEnabled = true;
                Gl.glEnable(Gl.GL_LIGHT0);
                Gl.glEnable(Gl.GL_COLOR_MATERIAL);
                Gl.glColorMaterial(Gl.GL_FRONT_AND_BACK, Gl.GL_AMBIENT_AND_DIFFUSE);
                Gl.glShadeModel(Gl.GL_SMOOTH);
                OptionLightingResultingAmount = 0.00208333333333333f * ((float)OptionAmbientColor.R + (float)OptionAmbientColor.G + (float)OptionAmbientColor.B);
                if (OptionLightingResultingAmount > 1.0f) OptionLightingResultingAmount = 1.0f;
            } else {
                Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
            }
            Gl.glDepthFunc(Gl.GL_LEQUAL);
        }

        // render scene
        internal static byte[] PixelBuffer = null;
        internal static int PixelBufferOpenGlTextureIndex = 0;
        internal static void RenderScene(double TimeElapsed) {
            // initialize
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthMask(Gl.GL_TRUE);
            if (OptionWireframe | World.CurrentBackground.Texture == -1) {
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            } else {
                int OpenGlTextureIndex = TextureManager.UseTexture(World.CurrentBackground.Texture, TextureManager.UseMode.Normal);
                if (OpenGlTextureIndex > 0) {
                    Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);
                } else {
                    Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
                }
            }
            Gl.glPushMatrix();
            if (LoadTexturesImmediately == LoadTextureImmediatelyMode.NotYet) {
                LoadTexturesImmediately = LoadTextureImmediatelyMode.Yes;
                ReAddObjects();
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
                Gl.glClearColor(inv255 * (float)Game.CurrentFog.Color.R, inv255 * (float)Game.CurrentFog.Color.G, inv255 * (float)Game.CurrentFog.Color.B, 1.0f);
            } else if (FogEnabled) {
                Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
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
            if (OptionLighting) {
                Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { OptionLightPosition.X, OptionLightPosition.Y, OptionLightPosition.Z, 0.0f });
            }
            // render background
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            RenderBackground(dx, dy, dz, TimeElapsed);
            // render polygons
            if (OptionLighting) {
                if (!LightingEnabled) {
                    Gl.glEnable(Gl.GL_LIGHTING);
                    LightingEnabled = true;
                }
            } else if (LightingEnabled) {
                Gl.glDisable(Gl.GL_LIGHTING);
                LightingEnabled = false;
            }
            AlphaFuncValue = 0.9f; Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
            BlendEnabled = false; Gl.glDisable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthMask(Gl.GL_TRUE);
            LastBoundTexture = 0;
            // opaque list
            for (int i = 0; i < OpaqueListCount; i++) {
                RenderFace(ref OpaqueList[i], cx, cy, cz);
            }
            // transparent color list
            if (TransparentColorDepthSorting) {
                AlphaFuncValue = 0.0f; Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
                BlendEnabled = true; Gl.glEnable(Gl.GL_BLEND);
                SortPolygons(TransparentColorList, TransparentColorListCount, TransparentColorListDistance, 1, TimeElapsed);
            }
            for (int i = 0; i < TransparentColorListCount; i++) {
                RenderFace(ref TransparentColorList[i], cx, cy, cz);
            }
            // alpha list
            if (!TransparentColorDepthSorting) {
                AlphaFuncValue = 0.0f; Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
                BlendEnabled = true; Gl.glEnable(Gl.GL_BLEND);
            }
            Gl.glDepthMask(Gl.GL_FALSE);
            SortPolygons(AlphaList, AlphaListCount, AlphaListDistance, 2, TimeElapsed);
            for (int i = 0; i < AlphaListCount; i++) {
                RenderFace(ref AlphaList[i], cx, cy, cz);
            }
            // motion blur
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glDepthMask(Gl.GL_FALSE);
            if (Interface.CurrentOptions.MotionBlur != Interface.MotionBlurMode.None) {
                if (LightingEnabled) {
                    Gl.glDisable(Gl.GL_LIGHTING);
                    LightingEnabled = false;
                }
                RenderFullscreenMotionBlur();
            }
            // overlay list
            if (FogEnabled) {
                Gl.glDisable(Gl.GL_FOG); FogEnabled = false;
            }
            SortPolygons(OverlayList, OverlayListCount, OverlayListDistance, 3, TimeElapsed);
            Gl.glDisable(Gl.GL_LIGHTING); LightingEnabled = false;
            bool lighting = OptionLighting;
            OptionLighting = false;
            for (int i = 0; i < OverlayListCount; i++) {
                RenderFace(ref OverlayList[i], cx, cy, cz);
            }
            OptionLighting = lighting;
            // render overlays
            BlendEnabled = false; Gl.glDisable(Gl.GL_BLEND);
            AlphaFuncValue = 0.9f; Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
            AlphaTestEnabled = false; Gl.glDisable(Gl.GL_ALPHA_TEST);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            RenderOverlays(TimeElapsed);
            // finalize rendering
            Gl.glPopMatrix();
            LoadTexturesImmediately = LoadTextureImmediatelyMode.NoLonger;
        }

        // render face
        private static int LastBoundTexture = 0;
        private static void RenderFace(ref ObjectFace Face, double CameraX, double CameraY, double CameraZ) {
            if (CullEnabled) {
                if (!OptionBackfaceCulling || (ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) != 0) {
                    Gl.glDisable(Gl.GL_CULL_FACE);
                    CullEnabled = false;
                }
            } else if (OptionBackfaceCulling) {
                if ((ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Faces[Face.FaceIndex].Flags & World.MeshFace.Face2Mask) == 0) {
                    Gl.glEnable(Gl.GL_CULL_FACE);
                    CullEnabled = true;
                }
            }
            int r = (int)ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Faces[Face.FaceIndex].Material;
            RenderFace(ref ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Materials[r], ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Vertices, ref ObjectManager.Objects[Face.ObjectIndex].Meshes[Face.MeshIndex].Faces[Face.FaceIndex], CameraX, CameraY, CameraZ);
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
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
                if (FogEnabled) {
                    Gl.glDisable(Gl.GL_FOG);
                }
            } else if (OpenGlNighttimeTextureIndex == 0) {
                float blend = inv255 * (float)Material.DaytimeNighttimeBlend + 1.0f - OptionLightingResultingAmount;
                if (blend > 1.0f) blend = 1.0f;
                factor = 1.0f - 0.75f * blend;
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
            Gl.glBegin(Gl.GL_POLYGON);
            if (Material.GlowAttenuationData != 0) {
                float alphafactor = (float)GetDistanceFactor(Vertices, ref Face, Material.GlowAttenuationData, CameraX, CameraY, CameraZ);
                Gl.glColor4d(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
            } else {
                Gl.glColor4d(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A);
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
                        Gl.glNormal3fv(Face.Vertices[j].Normal.Array());
                        Gl.glTexCoord2fv(Vertices[Face.Vertices[j].Index].TextureCoordinates.Array());
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    }
                } else {
                    for (int j = 0; j < Face.Vertices.Length; j++) {
                        Gl.glTexCoord2fv(Vertices[Face.Vertices[j].Index].TextureCoordinates.Array());
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    }
                }
            } else {
                if (LightingEnabled) {
                    for (int j = 0; j < Face.Vertices.Length; j++) {
                        Gl.glNormal3fv(Face.Vertices[j].Normal.Array());
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    }
                } else {
                    for (int j = 0; j < Face.Vertices.Length; j++) {
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
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
                Gl.glBegin(Gl.GL_POLYGON);
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
                Gl.glColor4d(inv255 * (float)Material.Color.R * factor, inv255 * Material.Color.G * factor, inv255 * (float)Material.Color.B * factor, inv255 * (float)Material.Color.A * alphafactor);
                if ((Material.Flags & World.MeshMaterial.EmissiveColorMask) != 0) {
                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { inv255 * (float)Material.EmissiveColor.R, inv255 * (float)Material.EmissiveColor.G, inv255 * (float)Material.EmissiveColor.B, 1.0f });
                    EmissiveEnabled = true;
                } else if (EmissiveEnabled) {
                    Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_EMISSION, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                    EmissiveEnabled = false;
                }
                for (int j = 0; j < Face.Vertices.Length; j++) {
                    Gl.glTexCoord2fv(Vertices[Face.Vertices[j].Index].TextureCoordinates.Array());
                    Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                }
                Gl.glEnd();
                if (AlphaFuncValue != 0.0) {
                    Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
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
                    Gl.glColor4d(inv255 * (float)Material.Color.R, inv255 * (float)Material.Color.G, inv255 * (float)Material.Color.B, 1.0f);
                    Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X + Face.Vertices[j].Normal.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y + Face.Vertices[j].Normal.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z + Face.Vertices[j].Normal.Z - CameraZ);
                    Gl.glEnd();
                }
            }
            // finalize
            if (Material.BlendMode == World.MeshMaterialBlendMode.Additive) {
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
                if (FogEnabled) {
                    Gl.glEnable(Gl.GL_FOG);
                }
            }
        }

        // render background
        private static void RenderBackground(double dx, double dy, double dz, double TimeElapsed) {
            if (World.TargetBackgroundCountdown >= 0.0) {
                World.TargetBackgroundCountdown -= TimeElapsed;
                if (World.TargetBackgroundCountdown < 0.0) {
                    World.CurrentBackground = World.TargetBackground;
                    World.TargetBackgroundCountdown = -1.0;
                    RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
                } else {
                    RenderBackground(World.CurrentBackground, dx, dy, dz, 1.0f);
                    AlphaFuncValue = 0.0f; Gl.glAlphaFunc(Gl.GL_GREATER, AlphaFuncValue);
                    float Alpha = (float)(1.0 - World.TargetBackgroundCountdown / World.TargetBackgroundDefaultCountdown);
                    RenderBackground(World.TargetBackground, dx, dy, dz, Alpha);
                }
            } else {
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
                    double y0, y1;
                    if (Data.KeepAspectRatio) {
                        int tw = TextureManager.Textures[Data.Texture].Width;
                        int th = TextureManager.Textures[Data.Texture].Height;
                        double hh = Math.PI * World.BackgroundImageDistance * (double)th / ((double)tw * (double)Data.Repetition);
                        double yc = 0.125 * World.BackgroundImageDistance;
                        y0 = yc - hh;
                        y1 = yc + hh;
                    } else {
                        y0 = -0.125 * World.BackgroundImageDistance;
                        y1 = 0.375 * World.BackgroundImageDistance;
                    }
                    const int n = 32;
                    World.Vector3D[] bottom = new World.Vector3D[n];
                    World.Vector3D[] top = new World.Vector3D[n];
                    double angleValue = 2.61799387799149 - 3.14159265358979 / (double)n;
                    double angleIncrement = 6.28318530717958 / (double)n;
                    for (int i = 0; i < n; i++) {
                        double x = World.BackgroundImageDistance * Math.Cos(angleValue);
                        double z = World.BackgroundImageDistance * Math.Sin(angleValue);
                        bottom[i] = new World.Vector3D(x, y0, z);
                        top[i] = new World.Vector3D(x, y1, z);
                        angleValue += angleIncrement;
                    }
                    float textureStart = 0.5f * (float)Data.Repetition / (float)n;
                    float textureIncrement = -(float)Data.Repetition / (float)n;
                    double textureX = textureStart;
                    for (int i = 0; i < n; i++) {
                        int j = (i + 1) % n;
                        /// side wall
                        Gl.glBegin(Gl.GL_QUADS);
                        Gl.glTexCoord2d(textureX, 0.005f);
                        Gl.glVertex3d(top[i].X, top[i].Y, top[i].Z);
                        Gl.glTexCoord2d(textureX, 0.995f);
                        Gl.glVertex3d(bottom[i].X, bottom[i].Y, bottom[i].Z);
                        Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
                        Gl.glVertex3d(bottom[j].X, bottom[j].Y, bottom[j].Z);
                        Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
                        Gl.glVertex3d(top[j].X, top[j].Y, top[j].Z);
                        Gl.glEnd();
                        /// top cap
                        Gl.glBegin(Gl.GL_TRIANGLES);
                        Gl.glTexCoord2d(textureX, 0.005f);
                        Gl.glVertex3d(top[i].X, top[i].Y, top[i].Z);
                        Gl.glTexCoord2d(textureX + textureIncrement, 0.005f);
                        Gl.glVertex3d(top[j].X, top[j].Y, top[j].Z);
                        Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.1f);
                        Gl.glVertex3d(0.0, top[i].Y, 0.0);
                        /// bottom cap
                        Gl.glTexCoord2d(textureX + 0.5 * textureIncrement, 0.9f);
                        Gl.glVertex3d(0.0, bottom[i].Y, 0.0);
                        Gl.glTexCoord2d(textureX + textureIncrement, 0.995f);
                        Gl.glVertex3d(bottom[j].X, bottom[j].Y, bottom[j].Z);
                        Gl.glTexCoord2d(textureX, 0.995f);
                        Gl.glVertex3d(bottom[i].X, bottom[i].Y, bottom[i].Z);
                        Gl.glEnd();
                        /// finish
                        textureX += textureIncrement;
                    }
                    Gl.glDisable(Gl.GL_TEXTURE_2D);
                    TexturingEnabled = false;
                    if (!BlendEnabled) {
                        Gl.glEnable(Gl.GL_BLEND);
                        BlendEnabled = true;
                    }
                    textureX = textureStart;
                    for (int i = 0; i < n; i++) {
                        int j = (i + 1) % n;
                        ///// top cap color overlay
                        //Gl.glBegin(Gl.GL_POLYGON);
                        //Gl.glColor4f(OptionLightingResultingAmount, OptionLightingResultingAmount, OptionLightingResultingAmount, 0.0f);
                        //Gl.glVertex3d(top[i].X, top[i].Y, top[i].Z);
                        //Gl.glColor4f(OptionLightingResultingAmount, OptionLightingResultingAmount, OptionLightingResultingAmount, 0.0f);
                        //Gl.glVertex3d(top[j].X, top[j].Y, top[j].Z);
                        //Gl.glColor4f(OptionLightingResultingAmount, OptionLightingResultingAmount, OptionLightingResultingAmount, 1.0f);
                        //Gl.glVertex3d(0.0, top[i].Y, 0.0);
                        //Gl.glEnd();
                        ///// bottom cap (black overlay)
                        //Gl.glBegin(Gl.GL_POLYGON);
                        //Gl.glColor4f(0.0f, 0.0f, 0.0f, 1.0f);
                        //Gl.glVertex3d(0.0, bottom[i].Y, 0.0);
                        //Gl.glColor4f(0.0f, 0.0f, 0.0f, 0.0f);
                        //Gl.glVertex3d(bottom[j].X, bottom[j].Y, bottom[j].Z);
                        //Gl.glColor4f(0.0f, 0.0f, 0.0f, 0.0f);
                        //Gl.glVertex3d(bottom[i].X, bottom[i].Y, bottom[i].Z);
                        //Gl.glEnd();
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
                double factor;
                if (denominator > 0.001) {
                    factor = Math.Exp(-1.0 / denominator);
                } else {
                    factor = 0.0;
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
                Gl.glColor4d(1.0, 1.0, 1.0, factor);
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

        // render overlays
        private static double FadeDriver = 0.0;
        private static double FadeLeftStopMarker = 0.0;
        private static double FadeRightStopMarker = 0.0;
        private static double FadeDoorIndicator = 0.0;
        private static double FadeLogo = 8.0;
        private enum Lamp {
            None,
            Ats, AtsOperation,
            AtsPPower, AtsPPattern, AtsPBrakeOverride, AtsPBrakeOperation, AtsP, AtsPFailure,
            Atc, AtcPower, AtcUse, AtcEmergency,
            Eb, ConstSpeed,
            Plugin
        }
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
            // messages
            if (Game.InfoOutputMode == Game.OutputMode.Default) {
                for (int i = Game.Messages.Length - 1; i >= 0; i--) {
                    double p = Game.CurrentMode == Game.GameMode.Arcade ? (double)i + 1.0 : (double)i;
                    if (Game.Messages[i].RendererPosition < p) {
                        double d = p - Game.Messages[i].RendererPosition;
                        Game.Messages[i].RendererPosition += 4.0 * (0.1 + d * d) * TimeElapsed;
                        if (Game.Messages[i].RendererPosition > p) Game.Messages[i].RendererPosition = p;
                    } else if (Game.Messages[i].RendererPosition > p) {
                        double d = Game.Messages[i].RendererPosition - p;
                        Game.Messages[i].RendererPosition -= 4.0 * (0.1 + d * d) * TimeElapsed;
                        if (Game.Messages[i].RendererPosition < p) Game.Messages[i].RendererPosition = p;
                    }
                    double y = 8.0 + Game.Messages[i].RendererPosition * 22.0;
                    float a; if (Game.SecondsSinceMidnight >= Game.Messages[i].Timeout) {
                        a = 1.0f - (float)((Game.SecondsSinceMidnight - Game.Messages[i].Timeout) / Game.MessageFadeOutTime);
                        if (a < 0.0f) a = 0.0f;
                    } else {
                        a = 1.0f;
                    }
                    RenderString(16.0, y, Fonts.FontType.Medium, Game.Messages[i].DisplayText, -1, inv255 * (float)Game.Messages[i].Color.R, inv255 * (float)Game.Messages[i].Color.G, inv255 * (float)Game.Messages[i].Color.B, a, true);
                }
            }
            // score
            if (Game.CurrentMode == Game.GameMode.Arcade & Game.InfoOutputMode != Game.OutputMode.None) {
                {
                    float r, g, b;
                    if (Game.CurrentScore.Value < 0) {
                        r = 1.0f; g = 0.0f; b = 0.0f;
                    } else {
                        r = 0.0f; g = 1.0f; b = 0.0f;
                    }
                    string s = Interface.QuickReferences.Score + Game.CurrentScore.Value.ToString(Culture) + " / " + Game.CurrentScore.Maximum.ToString(Culture);
                    RenderString(0.5 * (double)ScreenWidth - 64.0, 8.0, Fonts.FontType.Medium, s, -1, r, g, b, 1.0f, true);
                }
                for (int i = 0; i < Game.ScoreMessages.Length; i++) {
                    float r, g, b, a = (float)Game.ScoreMessages[i].Alpha;
                    if (Game.ScoreMessages[i].Value < 0) {
                        r = 1.0f; g = 0.0f; b = 0.0f;
                    } else if (Game.ScoreMessages[i].Value > 0) {
                        r = 0.0f; g = 1.0f; b = 0.0f;
                    } else {
                        r = 1.0f; g = 1.0f; b = 1.0f;
                    }
                    double p = Game.ScoreMessages[i].RendererPosition;
                    if (Game.ScoreMessages[i].Text.Length == 0) {
                        double y = 0.5 * (double)ScreenHeight - 128 + 28.0 * (p + 0.5);
                        Gl.glColor4f(1.0f, 1.0f, 1.0f, a);
                        RenderOverlaySolid(0.5 * (double)ScreenWidth - 128.0, y - 1.0, 0.5 * (double)ScreenWidth + 128.0, y + 1.0);
                    } else {
                        {
                            string s;
                            if (Game.ScoreMessages[i].Value == 0) {
                                s = "▶";
                            } else {
                                s = Game.ScoreMessages[i].Value.ToString(Culture);
                            }
                            RenderString(0.5 * (double)ScreenWidth - 128.0, 0.5 * (double)ScreenHeight - 128 + 28.0 * p, Fonts.FontType.Large, s, -1, r, g, b, a, true);
                        }
                        {
                            string s = Game.ScoreMessages[i].Text;
                            RenderString(0.5 * (double)ScreenWidth - 56.0, 0.5 * (double)ScreenHeight - 128 + 28.0 * p, Fonts.FontType.Large, s, -1, r, g, b, a, true);
                        }
                    }
                }
            }
            // marker
            if (Game.CurrentMode != Game.GameMode.Expert & Game.InfoOutputMode != Game.OutputMode.None) {
                double y = 8.0;
                for (int i = 0; i < Game.MarkerTextures.Length; i++) {
                    int t = TextureManager.UseTexture(Game.MarkerTextures[i], TextureManager.UseMode.LoadImmediately);
                    if (t >= 0) {
                        double w = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipWidth;
                        double h = (double)TextureManager.Textures[Game.MarkerTextures[i]].ClipHeight;
                        Gl.glColor4d(1.0, 1.0, 1.0, 1.0);
                        RenderOverlayTexture(Game.MarkerTextures[i], (double)ScreenWidth - w - 8.0, y, (double)ScreenWidth - 8.0, y + h);
                        y += h + 8.0;
                    }
                }
            }
            // stop meter
            if (Game.CurrentMode != Game.GameMode.Expert & Game.InfoOutputMode != Game.OutputMode.None) {
                int s = TrainManager.Trains[TrainManager.PlayerTrain].Station;
                bool showatall = s >= 0 && Game.Stations[s].StopAtStation & TrainManager.Trains[TrainManager.PlayerTrain].StationState == TrainManager.TrainStopState.Pending;
                bool showleft = showatall && (Game.Stations[s].OpenLeftDoors | !Game.Stations[s].OpenRightDoors);
                bool showright = showatall && (Game.Stations[s].OpenRightDoors | !Game.Stations[s].OpenLeftDoors);
                if (showleft) {
                    double d = 1.0 - FadeLeftStopMarker;
                    FadeLeftStopMarker += 5.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeLeftStopMarker > 1.0) FadeLeftStopMarker = 1.0;
                } else {
                    double d = 1.0 - FadeLeftStopMarker;
                    FadeLeftStopMarker -= 5.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeLeftStopMarker < 0.0) FadeLeftStopMarker = 0.0;
                }
                if (showright) {
                    double d = 1.0 - FadeRightStopMarker;
                    FadeRightStopMarker += 5.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeRightStopMarker > 1.0) FadeRightStopMarker = 1.0;
                } else {
                    double d = 1.0 - FadeRightStopMarker;
                    FadeRightStopMarker -= 5.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeRightStopMarker < 0.0) FadeRightStopMarker = 0.0;
                }
                if (FadeLeftStopMarker > 0.0 | FadeRightStopMarker > 0) {
                    // meter
                    double w1, h1;
                    if (TextureStopMeterBar >= 0) {
                        w1 = (double)TextureManager.Textures[TextureStopMeterBar].ClipWidth;
                        h1 = (double)TextureManager.Textures[TextureStopMeterBar].ClipHeight;
                    } else {
                        w1 = 32.0;
                        h1 = 256.0;
                    }
                    if (FadeLeftStopMarker > 0.0) {
                        Gl.glColor3f(1.0f, 1.0f, 1.0f);
                        RenderOverlayTexture(TextureStopMeterBar, -w1 + FadeLeftStopMarker * (w1 + 8.0), 0.5 * ((double)ScreenHeight - h1), FadeLeftStopMarker * (w1 + 8.0), 0.5 * ((double)ScreenHeight + h1));
                        if (s >= 0 && !Game.Stations[s].OpenLeftDoors) {
                            if (TextureStopMeterNo >= 0) {
                                double w2 = (double)TextureManager.Textures[TextureStopMeterNo].ClipWidth;
                                double h2 = (double)TextureManager.Textures[TextureStopMeterNo].ClipHeight;
                                RenderOverlayTexture(TextureStopMeterNo, FadeLeftStopMarker * (8.0 + w1) - 0.5 * (w2 + w1), 0.5 * ((double)ScreenHeight - h1) - h2 - 16, FadeLeftStopMarker * (8.0 + w1) + 0.5 * (w2 - w1), 0.5 * ((double)ScreenHeight - h1) - 16);
                            } else {
                                RenderString(FadeLeftStopMarker * (w1 + 8.0) - 0.5 * w1, 0.5 * ((double)ScreenHeight - h1) - 32.0, Fonts.FontType.Medium, "X", 0, 1.0f, 0.0f, 0.0f, true);
                            }
                        }
                    }
                    if (FadeRightStopMarker > 0.0) {
                        Gl.glColor3f(1.0f, 1.0f, 1.0f);
                        RenderOverlayTexture(TextureStopMeterBar, (double)ScreenWidth - FadeRightStopMarker * (w1 + 8.0), 0.5 * ((double)ScreenHeight - h1), (double)ScreenWidth - FadeRightStopMarker * (w1 + 8.0) + w1, 0.5 * ((double)ScreenHeight + h1));
                        if (s >= 0 && !Game.Stations[s].OpenRightDoors) {
                            if (TextureStopMeterNo >= 0) {
                                double w2 = (double)TextureManager.Textures[TextureStopMeterNo].ClipWidth;
                                double h2 = (double)TextureManager.Textures[TextureStopMeterNo].ClipHeight;
                                RenderOverlayTexture(TextureStopMeterNo, (double)ScreenWidth - FadeRightStopMarker * (8.0 + w1) + 0.5 * (w1 - w2), 0.5 * ((double)ScreenHeight - h1) - h2 - 16, (double)ScreenWidth - FadeRightStopMarker * (8.0 + w1) + 0.5 * (w1 + w2), 0.5 * ((double)ScreenHeight - h1) - 16);
                            } else {
                                RenderString((double)ScreenWidth - FadeRightStopMarker * (w1 + 8.0) + 0.5 * w1, 0.5 * ((double)ScreenHeight - h1) - 32.0, Fonts.FontType.Medium, "X", 0, 1.0f, 0.0f, 0.0f, true);
                            }
                        }
                    }
                    // tick
                    if (FadeLeftStopMarker == 1.0 | FadeRightStopMarker == 1.0) {
                        double d = TrainManager.Trains[TrainManager.PlayerTrain].StationStopDifference;
                        int c = TrainManager.Trains[TrainManager.PlayerTrain].Cars.Length;
                        int p = Game.GetStopIndex(s, c);
                        if (p >= 0) {
                            double r;
                            if (d >= 0) {
                                r = (Game.Stations[s].Stops[p].BackwardTolerance - d) / (2.0 * Game.Stations[s].Stops[p].BackwardTolerance);
                                if (r < 0.0) r = 0.0;
                            } else {
                                r = (Game.Stations[s].Stops[p].ForwardTolerance - d) / (2.0 * Game.Stations[s].Stops[p].ForwardTolerance);
                                if (r >= 1.0) r = 1.0;
                            }
                            double w2, h2;
                            if (TextureStopMeterTick >= 0) {
                                w2 = (double)TextureManager.Textures[TextureStopMeterTick].ClipWidth;
                                h2 = (double)TextureManager.Textures[TextureStopMeterTick].ClipHeight;
                                Gl.glColor3f(1.0f, 1.0f, 1.0f);
                            } else {
                                w2 = 32.0;
                                h2 = 16.0;
                                Gl.glColor3d(0.0, 0.0, 0.0);
                            }
                            if (FadeLeftStopMarker == 1.0) {
                                RenderOverlayTexture(TextureStopMeterTick, 0.5 * (w1 - w2) + 8.0, 0.5 * ((double)ScreenHeight - h1 - h2 + 2.0 * h1 * r), 0.5 * (w1 + w2) + 8.0, 0.5 * ((double)ScreenHeight - h1 + h2 + 2.0 * h1 * r));
                            }
                            if (FadeRightStopMarker == 1.0) {
                                RenderOverlayTexture(TextureStopMeterTick, (double)ScreenWidth - 0.5 * (w1 + w2) - 8.0, 0.5 * ((double)ScreenHeight - h1 - h2 + 2.0 * h1 * r), (double)ScreenWidth - 0.5 * (w1 - w2) - 8.0, 0.5 * ((double)ScreenHeight - h1 + h2 + 2.0 * h1 * r));
                            }
                        }
                    }
                }
            }
            // door indicator
            if (Game.InfoOutputMode != Game.OutputMode.None & TrainManager.Trains[TrainManager.PlayerTrain].Specs.DoorCloseMode != TrainManager.DoorMode.Automatic) {
                bool leftopened = false, rightopened = false;
                bool leftclosed = false, rightclosed = false;
                for (int i = 0; i < TrainManager.Trains[TrainManager.PlayerTrain].Cars.Length; i++) {
                    for (int j = 0; j < TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors.Length; j++) {
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].Direction <= 0) {
                            if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].State == 0.0) {
                                leftclosed = true;
                            } else if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].State == 1.0) {
                                leftopened = true;
                            } else {
                                leftclosed = true;
                                leftopened = true;
                            }
                        }
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].Direction >= 0) {
                            if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].State == 0.0) {
                                rightclosed = true;
                            } else if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.Doors[j].State == 1.0) {
                                rightopened = true;
                            } else {
                                rightclosed = true;
                                rightopened = true;
                            }
                        }
                    }
                }
                if (leftopened | rightopened) {
                    double d = 1.0 - FadeDoorIndicator;
                    FadeDoorIndicator += 4.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeDoorIndicator > 1.0) FadeDoorIndicator = 1.0;
                } else {
                    double d = 1.0 - FadeDoorIndicator;
                    FadeDoorIndicator -= 4.0 * (0.1 + d * d) * TimeElapsed;
                    if (FadeDoorIndicator < 0.0) FadeDoorIndicator = 0.0;
                }
                if (FadeDoorIndicator > 0.0) {
                    double w, h;
                    if (TextureDoors >= 0) {
                        w = (double)TextureManager.Textures[TextureDoors].ClipWidth;
                        h = (double)TextureManager.Textures[TextureDoors].ClipHeight;
                    } else {
                        w = 32.0;
                        h = 16.0;
                    }
                    double x = 0.5 * ((double)ScreenWidth - w);
                    double y = (double)ScreenHeight - FadeDoorIndicator * (h + 4.0);
                    // left
                    if (leftopened & leftclosed) {
                        Gl.glColor4d(1.0, 0.5, 0.0, 0.5);
                    } else if (leftopened & TrainManager.Trains[TrainManager.PlayerTrain].Specs.DoorCloseMode != TrainManager.DoorMode.Manual) {
                        Gl.glColor4d(0.0, 0.75, 1.0, 0.5);
                    } else if (leftopened) {
                        Gl.glColor4d(0.0, 1.0, 0.0, 0.5);
                    } else {
                        Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                    }
                    RenderOverlayTexture(TextureDoors, x - w - 2.0, y, x - 2.0, y + h);
                    RenderString(x - w - 2.0 + 0.5 * w, y, Fonts.FontType.Small, Interface.QuickReferences.DoorsLeft, 0, 0.0f, 0.0f, 0.0f, false);
                    // right
                    if (rightopened & rightclosed) {
                        Gl.glColor4d(1.0, 0.5, 0.0, 0.5);
                    } else if (rightopened & TrainManager.Trains[TrainManager.PlayerTrain].Specs.DoorCloseMode != TrainManager.DoorMode.Manual) {
                        Gl.glColor4d(0.0, 0.75, 1.0, 0.5);
                    } else if (rightopened) {
                        Gl.glColor4d(0.0, 1.0, 0.0, 0.5);
                    } else {
                        Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                    }
                    RenderOverlayTexture(TextureDoors, x + 2.0, y, x + w + 2.0, y + h);
                    RenderString(x + 2.0 + 0.5 * w, y, Fonts.FontType.Small, Interface.QuickReferences.DoorsRight, 0, 0.0f, 0.0f, 0.0f, false);
                }
            }
            // lamps
            if (Game.InfoOutputMode != Game.OutputMode.None) {
                Lamp[] Lamps; int LampsUsed = 0;
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
                    if (Game.InfoOutputMode == Game.OutputMode.Debug) {
                        Lamps = new Lamp[1];
                        Lamps[0] = Lamp.Plugin;
                        LampsUsed = 1;
                    } else {
                        Lamps = new Lamp[] { };
                    }
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsPAvailable & TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.Available) {
                    Lamps = new Lamp[17];
                    Lamps[0] = Lamp.Ats;
                    Lamps[1] = Lamp.AtsOperation;
                    Lamps[2] = Lamp.None;
                    Lamps[3] = Lamp.AtsPPower;
                    Lamps[4] = Lamp.AtsPPattern;
                    Lamps[5] = Lamp.AtsPBrakeOverride;
                    Lamps[6] = Lamp.AtsPBrakeOperation;
                    Lamps[7] = Lamp.AtsP;
                    Lamps[8] = Lamp.AtsPFailure;
                    Lamps[9] = Lamp.None;
                    Lamps[10] = Lamp.Atc;
                    Lamps[11] = Lamp.AtcPower;
                    Lamps[12] = Lamp.AtcUse;
                    Lamps[13] = Lamp.AtcEmergency;
                    LampsUsed = 14;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsPAvailable) {
                    Lamps = new Lamp[12];
                    Lamps[0] = Lamp.Ats;
                    Lamps[1] = Lamp.AtsOperation;
                    Lamps[2] = Lamp.None;
                    Lamps[3] = Lamp.AtsPPower;
                    Lamps[4] = Lamp.AtsPPattern;
                    Lamps[5] = Lamp.AtsPBrakeOverride;
                    Lamps[6] = Lamp.AtsPBrakeOperation;
                    Lamps[7] = Lamp.AtsP;
                    Lamps[8] = Lamp.AtsPFailure;
                    LampsUsed = 9;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.Available & TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsAvailable) {
                    Lamps = new Lamp[10];
                    Lamps[0] = Lamp.Ats;
                    Lamps[1] = Lamp.AtsOperation;
                    Lamps[2] = Lamp.None;
                    Lamps[3] = Lamp.Atc;
                    Lamps[4] = Lamp.AtcPower;
                    Lamps[5] = Lamp.AtcUse;
                    Lamps[6] = Lamp.AtcEmergency;
                    LampsUsed = 7;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.Available) {
                    Lamps = new Lamp[7];
                    Lamps[0] = Lamp.Atc;
                    Lamps[1] = Lamp.AtcPower;
                    Lamps[2] = Lamp.AtcUse;
                    Lamps[3] = Lamp.AtcEmergency;
                    LampsUsed = 4;
                } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsAvailable) {
                    Lamps = new Lamp[5];
                    Lamps[0] = Lamp.Ats;
                    Lamps[1] = Lamp.AtsOperation;
                    LampsUsed = 2;
                } else {
                    Lamps = new Lamp[3];
                    LampsUsed = 0;
                }
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode != TrainManager.SecuritySystem.Bve4Plugin) {
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Eb.Available | TrainManager.Trains[TrainManager.PlayerTrain].Specs.HasConstSpeed) {
                        Lamps[LampsUsed] = Lamp.None;
                        LampsUsed++;
                    }
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Eb.Available) {
                        Lamps[LampsUsed] = Lamp.Eb;
                        LampsUsed++;
                    }
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.HasConstSpeed) {
                        Lamps[LampsUsed] = Lamp.ConstSpeed;
                        LampsUsed++;
                    }
                }
                double w, h;
                if (TextureLamp >= 0) {
                    w = (double)TextureManager.Textures[TextureLamp].ClipWidth;
                    h = (double)TextureManager.Textures[TextureLamp].ClipHeight;
                } else {
                    w = 80.0;
                    h = 16.0;
                }
                double x = (double)ScreenWidth - w - 8.0;
                double y = (double)ScreenHeight - 8.0;
                for (int i = LampsUsed - 1; i >= 0; i--) {
                    if (Lamps[i] != Lamp.None) {
                        const double a = 0.5;
                        Gl.glColor4d(0.5, 0.5, 0.5, a);
                        string s = "";
                        switch (Lamps[i]) {
                            // ats
                            case Lamp.Ats: s = Interface.QuickReferences.LampAts;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Normal | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Initialization) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            case Lamp.AtsOperation: s = Interface.QuickReferences.LampAtsOperation;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Ringing) {
                                        Gl.glColor4d(1.0, 0.0, 0.0, a);
                                    } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Emergency | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Pattern | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Service) {
                                        if (((int)Math.Floor(2.0 * Game.SecondsSinceMidnight) & 1) == 0) {
                                            Gl.glColor4d(1.0, 0.0, 0.0, a);
                                        }
                                    }
                                } break;
                            // ats-p
                            case Lamp.AtsPPower: s = Interface.QuickReferences.LampAtsPPower;
                                if ((TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) & TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsPAvailable) {
                                    Gl.glColor4d(0.0, 1.0, 0.0, a);
                                } break;
                            case Lamp.AtsPPattern: s = Interface.QuickReferences.LampAtsPPattern;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Pattern | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Service) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            case Lamp.AtsPBrakeOverride: s = Interface.QuickReferences.LampAtsPBrakeOverride;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsPOverride) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            case Lamp.AtsPBrakeOperation: s = Interface.QuickReferences.LampAtsPBrakeOperation;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Service & !TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Ats.AtsPOverride) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            case Lamp.AtsP: s = Interface.QuickReferences.LampAtsP;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                    Gl.glColor4d(0.0, 1.0, 0.0, a);
                                } break;
                            case Lamp.AtsPFailure: s = Interface.QuickReferences.LampAtsPFailure;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode != TrainManager.SecuritySystem.None) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Initialization) {
                                        Gl.glColor4d(1.0, 0.0, 0.0, a);
                                    } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Ringing | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Emergency) {
                                            Gl.glColor4d(1.0, 0.0, 0.0, a);
                                        }
                                    }
                                } break;
                            // atc
                            case Lamp.Atc: s = Interface.QuickReferences.LampAtc;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    Gl.glColor4d(1.0, 0.5, 0.0, a);
                                } break;
                            case Lamp.AtcPower: s = Interface.QuickReferences.LampAtcPower;
                                if ((TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Atc | TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode != TrainManager.SecuritySystem.None & TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.AutomaticSwitch)) {
                                    Gl.glColor4d(1.0, 0.5, 0.0, a);
                                } break;
                            case Lamp.AtcUse: s = Interface.QuickReferences.LampAtcUse;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.State == TrainManager.SecurityState.Service) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            case Lamp.AtcEmergency: s = Interface.QuickReferences.LampAtcEmergency;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Security != TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                                        Gl.glColor4d(1.0, 0.0, 0.0, a);
                                    }
                                } break;
                            // eb
                            case Lamp.Eb: s = Interface.QuickReferences.LampEb;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode != TrainManager.SecuritySystem.None) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Eb.BellState == TrainManager.SecurityState.Ringing) {
                                        Gl.glColor4d(0.0, 1.0, 0.0, a);
                                    }
                                } break;
                            // const speed
                            case Lamp.ConstSpeed: s = Interface.QuickReferences.LampConstSpeed;
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.HasConstSpeed) {
                                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentConstSpeed) {
                                        Gl.glColor4d(1.0, 0.5, 0.0, a);
                                    }
                                } break;
                            // plugin
                            case Lamp.Plugin: s = Interface.QuickReferences.LampPlugin;
                                if (PluginManager.PluginValid) {
                                    Gl.glColor4d(0.0, 1.0, 0.0, a);
                                } else {
                                    Gl.glColor4d(1.0, 0.0, 0.0, a);
                                } break;
                        }
                        y -= h;
                        RenderOverlayTexture(TextureLamp, x, y, x + w, y + h);
                        RenderString(x + 0.5 * w, y + 0.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                    } else {
                        y -= 0.5 * h;
                    }
                }
            }
            // clock
            if (OptionClock & Game.InfoOutputMode != Game.OutputMode.None) {
                double x;
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode != TrainManager.SecuritySystem.Bve4Plugin | Game.InfoOutputMode == Game.OutputMode.Debug) {
                    if (TextureLamp >= 0) {
                        double w = (double)TextureManager.Textures[TextureLamp].ClipWidth;
                        x = (double)ScreenWidth - w - 20.0;
                    } else {
                        x = (double)ScreenWidth - 100.0;
                    }
                } else {
                    x = (double)ScreenWidth - 12.0;
                }
                double y = (double)ScreenHeight - 22.0;
                double s = Game.SecondsSinceMidnight;
                int hours = (int)Math.Floor(s / 3600.0);
                s -= (double)hours * 3600.0;
                int minutes = (int)Math.Floor(s / 60.0);
                s -= (double)minutes * 60.0;
                int seconds = (int)Math.Floor(s);
                hours %= 24;
                string t = hours.ToString("00", Culture) + ":" + minutes.ToString("00", Culture) + ":" + seconds.ToString("00", Culture);
                RenderString(x, y, Fonts.FontType.Small, t, 1, 1.0f, 1.0f, 1.0f, true);
            }
            // speed
            if (OptionSpeed != SpeedDisplayMode.None & Game.InfoOutputMode != Game.OutputMode.None) {
                double x = 4.0;
                double y = (double)ScreenHeight - 40.0;
                double s = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageSpeed;
                string g;
                switch (OptionSpeed) {
                    case SpeedDisplayMode.Kmph:
                        s *= 3.6;
                        g = " km/h";
                        break;
                    case SpeedDisplayMode.Mph:
                        s *= 2.25;
                        g = " mph";
                        break;
                    default:
                        g = " m/s";
                        break;
                }
                string t = s.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + g;
                RenderString(x, y, Fonts.FontType.Small, t, -1, 1.0f, 1.0f, 1.0f, true);
            }
            // ai logo
            if (Game.InfoOutputMode != Game.OutputMode.None) {
                if (TrainManager.Trains[TrainManager.PlayerTrain].AI != null) {
                    FadeDriver += 3.0 * TimeElapsed;
                    if (FadeDriver > 1.0) FadeDriver = 1.0;
                } else {
                    FadeDriver -= 3.0 * TimeElapsed;
                    if (FadeDriver < 0.0) FadeDriver = 0.0;
                }
                if (FadeDriver > 0.0) {
                    double y = ScreenHeight - 24.0;
                    if (OptionSpeed != SpeedDisplayMode.None) y -= 16.0;
                    if (TextureDriver >= 0) {
                        TextureManager.UseTexture(TextureDriver, TextureManager.UseMode.LoadImmediately);
                        double w = (double)TextureManager.Textures[TextureDriver].ClipWidth;
                        double h = (double)TextureManager.Textures[TextureDriver].ClipHeight;
                        Gl.glColor3f(1.0f, 1.0f, 1.0f);
                        RenderOverlayTexture(TextureDriver, -w + FadeDriver * (8.0 + w), y - h, FadeDriver * (8.0 + w), y);
                    } else {
                        RenderString(4.0, y - 16.0, Fonts.FontType.Small, "AI", -1, 1.0f, 1.0f, 1.0f, true);
                    }
                }
            }
            // handles
            if (Game.InfoOutputMode != Game.OutputMode.None) {
                TextureManager.UseTexture(TextureReverser, TextureManager.UseMode.LoadImmediately);
                TextureManager.UseTexture(TexturePower, TextureManager.UseMode.LoadImmediately);
                TextureManager.UseTexture(TextureBrake, TextureManager.UseMode.LoadImmediately);
                string s; int n;
                double wr, hr;
                if (TextureReverser >= 0) {
                    wr = (double)TextureManager.Textures[TextureReverser].ClipWidth;
                    hr = (double)TextureManager.Textures[TextureReverser].ClipHeight;
                } else {
                    wr = 32.0;
                    hr = 16.0;
                }
                double x = 4.0;
                // reverser
                n = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentReverser.Driver;
                s = n < 0 ? Interface.QuickReferences.HandleBackward : n > 0 ? Interface.QuickReferences.HandleForward : Interface.QuickReferences.HandleNeutral;
                if (n == -1) {
                    Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                } else if (n == 1) {
                    Gl.glColor4d(0.5, 1.0, 0.5, 0.5);
                } else {
                    Gl.glColor4d(1.0, 1.0, 1.0, 0.5);
                }
                RenderOverlayTexture(TextureReverser, x, (double)ScreenHeight - 4.0 - hr, x + wr, (double)ScreenHeight - 4.0);
                RenderString(x + 0.5 * wr, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                x += wr + 4.0;
                // handles
                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.SingleHandle) {
                    // texture
                    double ws, hs;
                    if (TextureSingle >= 0) {
                        ws = (double)TextureManager.Textures[TextureSingle].ClipWidth;
                        hs = (double)TextureManager.Textures[TextureSingle].ClipHeight;
                    } else {
                        ws = 32.0;
                        hs = 16.0;
                    }
                    // one handle
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                        s = Interface.QuickReferences.HandleEmergency;
                        Gl.glColor4d(1.0, 0.5, 0.5, 0.5);
                    } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentHoldBrake.Driver) {
                        s = Interface.QuickReferences.HandleHoldBrake;
                        Gl.glColor4d(0.5, 0.75, 1.0, 0.5);
                    } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentBrakeNotch.Driver > 0) {
                        n = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentBrakeNotch.Driver;
                        s = Interface.QuickReferences.HandleBrake + n.ToString(Culture);
                        Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                    } else {
                        n = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentPowerNotch.Driver;
                        s = n > 0 ? Interface.QuickReferences.HandlePower + n.ToString(Culture) : Interface.QuickReferences.HandlePowerNull;
                        if (n > 0) {
                            Gl.glColor4d(0.5, 1.0, 0.5, 0.5);
                        } else {
                            Gl.glColor4d(1.0, 1.0, 1.0, 0.5);
                        }
                    }
                    RenderOverlayTexture(TextureSingle, x, (double)ScreenHeight - 4.0 - hs, x + ws, (double)ScreenHeight - 4.0);
                    RenderString(x + 0.5 * ws, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                } else {
                    // textures
                    double wp, hp, wb, hb;
                    if (TexturePower >= 0) {
                        wp = (double)TextureManager.Textures[TexturePower].ClipWidth;
                        hp = (double)TextureManager.Textures[TexturePower].ClipHeight;
                    } else {
                        wp = 32.0;
                        hp = 16.0;
                    }
                    if (TextureBrake >= 0) {
                        wb = (double)TextureManager.Textures[TextureBrake].ClipWidth;
                        hb = (double)TextureManager.Textures[TextureBrake].ClipHeight;
                    } else {
                        wb = 32.0;
                        hb = 16.0;
                    }
                    // power notch
                    n = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentPowerNotch.Driver;
                    s = n > 0 ? Interface.QuickReferences.HandlePower + n.ToString(Culture) : Interface.QuickReferences.HandlePowerNull;
                    if (n > 0) {
                        Gl.glColor4d(0.5, 1.0, 0.5, 0.5);
                    } else {
                        Gl.glColor4d(1.0, 1.0, 1.0, 0.5);
                    }
                    RenderOverlayTexture(TexturePower, x, (double)ScreenHeight - 4.0 - hp, x + wp, (double)ScreenHeight - 4.0);
                    RenderString(x + 0.5 * wp, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                    int d = TrainManager.Trains[TrainManager.PlayerTrain].DriverCar;
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[d].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                        // air brake
                        x += wp + 4.0;
                        s = Interface.QuickReferences.HandleRelease;
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
                            Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                        } else {
                            Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                        }
                        RenderOverlayTexture(TextureBrake, x, (double)ScreenHeight - 4.0 - hb, x + wb, (double)ScreenHeight - 4.0);
                        RenderString(x + 0.5 * wb, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                        x += wb;
                        s = Interface.QuickReferences.HandleLap;
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
                            Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                        } else {
                            Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                        }
                        RenderOverlayTexture(TextureBrake, x, (double)ScreenHeight - 4.0 - hb, x + wb, (double)ScreenHeight - 4.0);
                        RenderString(x + 0.5 * wb, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                        x += wb;
                        s = Interface.QuickReferences.HandleService;
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Service & !TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                            Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                        } else {
                            Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                        }
                        RenderOverlayTexture(TextureBrake, x, (double)ScreenHeight - 4.0 - hb, x + wb, (double)ScreenHeight - 4.0);
                        RenderString(x + 0.5 * wb, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                        x += wb;
                        s = Interface.QuickReferences.HandleEmergency;
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                            Gl.glColor4d(1.0, 0.5, 0.5, 0.5);
                        } else {
                            Gl.glColor4d(0.5, 0.5, 0.5, 0.5);
                        }
                        RenderOverlayTexture(TextureBrake, x, (double)ScreenHeight - 4.0 - hb, x + wb, (double)ScreenHeight - 4.0);
                        RenderString(x + 0.5 * wb, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                    } else {
                        // brake notch
                        x += wp;
                        if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentEmergencyBrake.Driver) {
                            s = Interface.QuickReferences.HandleEmergency;
                            Gl.glColor4d(1.0, 0.5, 0.5, 0.5);
                        } else if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentHoldBrake.Driver) {
                            s = Interface.QuickReferences.HandleHoldBrake;
                            Gl.glColor4d(0.5, 0.75, 1.0, 0.5);
                        } else {
                            n = TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentBrakeNotch.Driver;
                            s = n > 0 ? Interface.QuickReferences.HandleBrake + n.ToString(Culture) : Interface.QuickReferences.HandleBrakeNull;
                            if (n > 0) {
                                Gl.glColor4d(1.0, 1.0, 0.5, 0.5);
                            } else {
                                Gl.glColor4d(1.0, 1.0, 1.0, 0.5);
                            }
                        }
                        RenderOverlayTexture(TextureBrake, x, (double)ScreenHeight - 4.0 - hb, x + wb, (double)ScreenHeight - 4.0);
                        RenderString(x + 0.5 * wb, (double)ScreenHeight - 20.0, Fonts.FontType.Small, s, 0, 0.0f, 0.0f, 0.0f, false);
                    }
                }
            }
            // timetable
            if (OptionTimetable) {
                int t = Timetable.TimetableTexture;
                if (t >= 0) {
                    int w = TextureManager.Textures[t].ClipWidth;
                    int h = TextureManager.Textures[t].ClipHeight;
                    Gl.glColor4d(1.0, 1.0, 1.0, 1.0);
                    RenderOverlayTexture(t, (double)(ScreenWidth - w), OptionTimetablePosition, (double)ScreenWidth, (double)h + OptionTimetablePosition);
                }
            }
            // air brake debug output
            if (Game.CurrentMode != Game.GameMode.Expert & OptionBrakeSystems) {
                double oy = 64.0, y = oy, h = 16.0;
                bool[] heading = new bool[6];
                for (int i = 0; i < TrainManager.Trains[TrainManager.PlayerTrain].Cars.Length; i++) {
                    double x = 96.0, w = 128.0;
                    /// brake pipe
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake) {
                        if (!heading[0]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Brake pipe", -1, 1.0f, 1.0f, 0.0f, true);
                            heading[0] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.BrakePipeCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.BrakePipeNormalPressure;
                        Gl.glColor3f(1.0f, 1.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    /// auxillary reservoir
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake | TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake) {
                        if (!heading[1]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Auxillary reservoir", -1, 0.75f, 0.75f, 0.75f, true);
                            heading[1] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.AuxillaryReservoirCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.AuxillaryReservoirMaximumPressure;
                        Gl.glColor3f(0.5f, 0.5f, 0.5f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    /// brake cylinder
                    {
                        if (!heading[2]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Brake cylinder", -1, 0.75f, 0.5f, 0.25f, true);
                            heading[2] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.BrakeCylinderCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
                        Gl.glColor3f(0.75f, 0.5f, 0.25f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    /// main reservoir
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
                        if (!heading[3]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Main reservoir", -1, 1.0f, 0.0f, 0.0f, true);
                            heading[3] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.MainReservoirCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.AirCompressorMaximumPressure;
                        Gl.glColor3f(1.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    /// equalizing reservoir
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
                        if (!heading[4]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Equalizing reservoir", -1, 0.0f, 0.75f, 0.0f, true);
                            heading[4] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.EqualizingReservoirCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.EqualizingReservoirNormalPressure;
                        Gl.glColor3f(0.0f, 0.75f, 0.0f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    /// straight air pipe
                    if (TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.BrakeType == TrainManager.CarBrakeType.ElectromagneticStraightAirBrake & TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.Type == TrainManager.AirBrakeType.Main) {
                        if (!heading[5]) {
                            RenderString(x, oy - 16.0, Fonts.FontType.Small, "Straight air pipe", -1, 0.0f, 0.75f, 1.0f, true);
                            heading[5] = true;
                        }
                        Gl.glColor3f(0.0f, 0.0f, 0.0f);
                        RenderOverlaySolid(x, y, x + w, y + h);
                        double p = TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.StraightAirPipeCurrentPressure;
                        double r = p / TrainManager.Trains[TrainManager.PlayerTrain].Cars[i].Specs.AirBrake.BrakeCylinderEmergencyMaximumPressure;
                        Gl.glColor3f(0.0f, 0.75f, 1.0f);
                        RenderOverlaySolid(x, y, x + r * w, y + h);
                    } x += w + 8.0;
                    Gl.glColor3f(0.0f, 0.0f, 0.0f);
                    y += h + 8.0;
                }
            }
            // debug output
            if (Game.InfoOutputMode == Game.OutputMode.Fps | Game.InfoOutputMode == Game.OutputMode.Debug) {
                for (int y = 0; y < 11; y++) {
                    if (Game.InfoOutputMode == Game.OutputMode.Fps & y > 0) break;
                    string t = "";
                    switch (y) {
                        case 0:
                            t = Game.InfoFrameRate.ToString("0.0", Culture) + " frames/s" + (MainLoop.LimitFramerate ? " (low cpu)" : "");
                            break;
                        case 1:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t += "speed: " + (Math.Abs(TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageSpeed) * 3.6).ToString("0.00", Culture) + " km/h - power: " + TrainManager.Trains[TrainManager.PlayerTrain].Cars[TrainManager.Trains[TrainManager.PlayerTrain].DriverCar].Specs.CurrentAccelerationOutput.ToString("0.0000", Culture) + " m/s² - acc: " + TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²";
                            } break;
                        case 2:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t = "route limit: " + (TrainManager.Trains[TrainManager.PlayerTrain].CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.Trains[TrainManager.PlayerTrain].CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h"));
                                t += ", signal limit: " + (TrainManager.Trains[TrainManager.PlayerTrain].CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.Trains[TrainManager.PlayerTrain].CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h"));
                                if (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    t += ", atc limit: " + (TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.SpeedRestriction == double.PositiveInfinity ? "unlimited" : ((TrainManager.Trains[TrainManager.PlayerTrain].Specs.Security.Atc.SpeedRestriction * 3.6).ToString("0.0", Culture) + " km/h"));
                                }
                            } break;
                        case 3:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t = "score: " + Game.CurrentScore.Value.ToString(Culture);
                            } break;
                        case 4:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t = "position: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m";
                            } break;
                        case 5:
                            t = "objects: " + ObjectManager.ObjectsUsed.ToString(Culture) + " normal + " + ObjectManager.AnimatedWorldObjectsUsed.ToString(Culture) + " animated";
                            break;
                        case 6:
                            t = "polygons: " + OpaqueListCount.ToString(Culture) + " opaque + " + TransparentColorListCount.ToString(Culture) + " transparent + " + AlphaListCount.ToString(Culture) + " alpha + " + OverlayListCount.ToString(Culture) + " overlay";
                            break;
                        case 7:
                            t = "textures: " + Game.InfoTexturesLoaded.ToString(Culture) + " loaded / " + Game.InfoTexturesRegistered.ToString(Culture) + " registered";
                            break;
                        case 8:
                            t = "sound sources: " + Game.InfoSoundSourcesPlaying.ToString(Culture) + " playing / " + Game.InfoSoundSourcesRegistered.ToString(Culture) + " registered, outerradiusfactor: " + SoundManager.OuterRadiusFactor.ToString("0.00");
                            break;
                        case 9:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                int d = TrainManager.Trains[TrainManager.PlayerTrain].DriverCar;
                                t = "elevation: " + (Game.RouteInitialElevation + TrainManager.Trains[TrainManager.PlayerTrain].Cars[d].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m, temperature: " + (TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + "[" + (Game.RouteSeaLevelAirTemperature - 273.15).ToString("0.00", Culture) + "] °C, pressure: " + (0.001 * TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAirPressure).ToString("0.00", Culture) + "[" + (0.001 * Game.RouteSeaLevelAirPressure).ToString("0.00", Culture) + "] kPa, density: " + TrainManager.Trains[TrainManager.PlayerTrain].Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³";
                            } break;
                        case 10:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t = Game.InfoDebugString;
                            } break;
                    }
                    RenderString(2.0, 2.0 + 14.0 * y, Fonts.FontType.Small, t, -1, 1.0f, 1.0f, 1.0f, true);
                }
            }
            // interface
            if (Game.CurrentInterface == Game.InterfaceType.Pause) {
                // pause
                Gl.glColor4d(0.0, 0.0, 0.0, 0.5);
                RenderOverlaySolid(0.0, 0.0, (double)ScreenWidth, (double)ScreenHeight);
                Gl.glColor4d(1.0, 1.0, 1.0, 1.0);
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
                Gl.glColor4d(0.0, 0.0, 0.0, 0.5);
                RenderOverlaySolid(0.0, 0.0, (double)ScreenWidth, (double)ScreenHeight);
                Gl.glColor4d(1.0, 1.0, 1.0, 1.0);
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
            // logo
            if (FadeLogo > 0.0) {
                double size = ScreenWidth > ScreenHeight ? ScreenWidth : ScreenHeight;
                if (FadeLogo > 1.0) {
                    Gl.glColor3d(1.0, 1.0, 1.0);
                    FadeLogo -= 1.0;
                } else {
                    FadeLogo -= TimeElapsed;
                    if (FadeLogo < 0.0) FadeLogo = 0.0;
                    Gl.glColor4d(1.0, 1.0, 1.0, FadeLogo);
                }
                RenderOverlayTexture(TextureLogo, 0.5 * (ScreenWidth - size), 0.5 * (ScreenHeight - size), 0.5 * (ScreenWidth + size), 0.5 * (ScreenHeight + size));
            }
            // finalize
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glPopMatrix();
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glDisable(Gl.GL_BLEND);
        }

        // render string
        private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, bool Shadow) {
            RenderString(PixelLeft, PixelTop, FontType, Text, Orientation, R, G, B, 1.0f, Shadow);
        }
        private static void RenderString(double PixelLeft, double PixelTop, Fonts.FontType FontType, string Text, int Orientation, float R, float G, float B, float A, bool Shadow) {
            int Font = (int)FontType;
            double c = 1;// Font == 2 ? 2 : 1;
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
                    //if (FontType == Fonts.FontType.Large) {
                    //    RenderOverlayTexture(t, x - c, y - c, x + w, y + h);
                    //}
                    RenderOverlayTexture(t, x + c, y + c, x + w, y + h);
                }
                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE);
                Gl.glColor4f(R, G, B, A);
                RenderOverlayTexture(t, x, y, x + w, y + h);
                x += Fonts.Characters[Font][b].Width;
            }
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
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

        // readd objects
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
            if (ObjectManager.Objects[ObjectIndex].RendererIndex == 0) {
                if (ObjectListCount >= ObjectList.Length) {
                    Array.Resize<Object>(ref ObjectList, ObjectList.Length << 1);
                }
                int m = ObjectManager.Objects[ObjectIndex].Meshes.Length;
                ObjectList[ObjectListCount].ObjectIndex = ObjectIndex;
                ObjectList[ObjectListCount].Overlay = Overlay;
                ObjectList[ObjectListCount].FaceListIndices = new int[m][];
                for (int i = 0; i < m; i++) {
                    int f = ObjectManager.Objects[ObjectIndex].Meshes[i].Faces.Length;
                    ObjectList[ObjectListCount].FaceListIndices[i] = new int[f];
                    for (int j = 0; j < f; j++) {
                        if (Overlay) {
                            /// overlay
                            if (OverlayListCount >= OverlayList.Length) {
                                Array.Resize(ref OverlayList, OverlayList.Length << 1);
                                Array.Resize(ref OverlayListDistance, OverlayList.Length);
                            }
                            OverlayList[OverlayListCount].ObjectIndex = ObjectIndex;
                            OverlayList[OverlayListCount].MeshIndex = i;
                            OverlayList[OverlayListCount].FaceIndex = j;
                            OverlayList[OverlayListCount].ObjectListIndex = ObjectListCount;
                            ObjectList[ObjectListCount].FaceListIndices[i][j] = (OverlayListCount << 2) + 3;
                            OverlayListCount++;
                        } else {
                            int k = ObjectManager.Objects[ObjectIndex].Meshes[i].Faces[j].Material;
                            bool transparentcolor = false, alpha = false;
                            if (ObjectManager.Objects[ObjectIndex].Meshes[i].Materials[k].Color.A != 255) {
                                alpha = true;
                            } else if (ObjectManager.Objects[ObjectIndex].Meshes[i].Materials[k].BlendMode == World.MeshMaterialBlendMode.Additive) {
                                alpha = true;
                            } else if (ObjectManager.Objects[ObjectIndex].Meshes[i].Materials[k].GlowAttenuationData != 0) {
                                alpha = true;
                            } else {
                                int tday = ObjectManager.Objects[ObjectIndex].Meshes[i].Materials[k].DaytimeTextureIndex;
                                if (tday >= 0) {
                                    TextureManager.UseTexture(tday, TextureManager.UseMode.Normal);
                                    if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
                                        alpha = true;
                                    } else if (TextureManager.Textures[tday].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
                                        transparentcolor = true;
                                    }
                                }
                                int tnight = ObjectManager.Objects[ObjectIndex].Meshes[i].Materials[k].NighttimeTextureIndex;
                                if (tnight >= 0) {
                                    TextureManager.UseTexture(tnight, TextureManager.UseMode.Normal);
                                    if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.Alpha) {
                                        alpha = true;
                                    } else if (TextureManager.Textures[tnight].Transparency == TextureManager.TextureTransparencyMode.TransparentColor) {
                                        transparentcolor = true;
                                    }
                                }
                            }
                            if (alpha) {
                                /// alpha
                                if (AlphaListCount >= AlphaList.Length) {
                                    Array.Resize(ref AlphaList, AlphaList.Length << 1);
                                    Array.Resize(ref AlphaListDistance, AlphaList.Length);
                                }
                                AlphaList[AlphaListCount].ObjectIndex = ObjectIndex;
                                AlphaList[AlphaListCount].MeshIndex = i;
                                AlphaList[AlphaListCount].FaceIndex = j;
                                AlphaList[AlphaListCount].ObjectListIndex = ObjectListCount;
                                ObjectList[ObjectListCount].FaceListIndices[i][j] = (AlphaListCount << 2) + 2;
                                AlphaListCount++;
                            } else if (transparentcolor) {
                                /// transparent color
                                if (TransparentColorListCount >= TransparentColorList.Length) {
                                    Array.Resize(ref TransparentColorList, TransparentColorList.Length << 1);
                                    Array.Resize(ref TransparentColorListDistance, TransparentColorList.Length);
                                }
                                TransparentColorList[TransparentColorListCount].ObjectIndex = ObjectIndex;
                                TransparentColorList[TransparentColorListCount].MeshIndex = i;
                                TransparentColorList[TransparentColorListCount].FaceIndex = j;
                                TransparentColorList[TransparentColorListCount].ObjectListIndex = ObjectListCount;
                                ObjectList[ObjectListCount].FaceListIndices[i][j] = (TransparentColorListCount << 2) + 1;
                                TransparentColorListCount++;
                            } else {
                                /// opaque
                                if (OpaqueListCount >= OpaqueList.Length) {
                                    Array.Resize(ref OpaqueList, OpaqueList.Length << 1);
                                }
                                OpaqueList[OpaqueListCount].ObjectIndex = ObjectIndex;
                                OpaqueList[OpaqueListCount].MeshIndex = i;
                                OpaqueList[OpaqueListCount].FaceIndex = j;
                                OpaqueList[OpaqueListCount].ObjectListIndex = ObjectListCount;
                                ObjectList[ObjectListCount].FaceListIndices[i][j] = OpaqueListCount << 2;
                                OpaqueListCount++;
                            }
                        }
                    }
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = ObjectListCount + 1;
                ObjectListCount++;
            }
        }

        // hide object
        internal static void HideObject(int ObjectIndex) {
            int k = ObjectManager.Objects[ObjectIndex].RendererIndex - 1;
            if (k >= 0) {
                // remove faces
                for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
                    for (int j = 0; j < ObjectList[k].FaceListIndices[i].Length; j++) {
                        int h = ObjectList[k].FaceListIndices[i][j];
                        int hi = h >> 2;
                        switch (h & 3) {
                            case 0:
                                /// opaque
                                OpaqueList[hi] = OpaqueList[OpaqueListCount - 1];
                                OpaqueListCount--;
                                ObjectList[OpaqueList[hi].ObjectListIndex].FaceListIndices[OpaqueList[hi].MeshIndex][OpaqueList[hi].FaceIndex] = h;
                                break;
                            case 1:
                                /// transparent color
                                TransparentColorList[hi] = TransparentColorList[TransparentColorListCount - 1];
                                TransparentColorListCount--;
                                ObjectList[TransparentColorList[hi].ObjectListIndex].FaceListIndices[TransparentColorList[hi].MeshIndex][TransparentColorList[hi].FaceIndex] = h;
                                break;
                            case 2:
                                /// alpha
                                AlphaList[hi] = AlphaList[AlphaListCount - 1];
                                AlphaListCount--;
                                ObjectList[AlphaList[hi].ObjectListIndex].FaceListIndices[AlphaList[hi].MeshIndex][AlphaList[hi].FaceIndex] = h;
                                break;
                            case 3:
                                /// overlay
                                OverlayList[hi] = OverlayList[OverlayListCount - 1];
                                OverlayListCount--;
                                ObjectList[OverlayList[hi].ObjectListIndex].FaceListIndices[OverlayList[hi].MeshIndex][OverlayList[hi].FaceIndex] = h;
                                break;
                        }
                    }
                }
                // remove object
                if (k == ObjectListCount - 1) {
                    ObjectListCount--;
                } else {
                    ObjectList[k] = ObjectList[ObjectListCount - 1];
                    ObjectListCount--;
                    for (int i = 0; i < ObjectList[k].FaceListIndices.Length; i++) {
                        for (int j = 0; j < ObjectList[k].FaceListIndices[i].Length; j++) {
                            int h = ObjectList[k].FaceListIndices[i][j];
                            int hi = h >> 2;
                            switch (h & 3) {
                                case 0:
                                    OpaqueList[hi].ObjectListIndex = k;
                                    break;
                                case 1:
                                    TransparentColorList[hi].ObjectListIndex = k;
                                    break;
                                case 2:
                                    AlphaList[hi].ObjectListIndex = k;
                                    break;
                                case 3:
                                    OverlayList[hi].ObjectListIndex = k;
                                    break;
                            }
                        }
                    }
                    ObjectManager.Objects[ObjectList[k].ObjectIndex].RendererIndex = k + 1;
                }
                ObjectManager.Objects[ObjectIndex].RendererIndex = 0;
            }
        }

        // sort polygons
        private static void SortPolygons(ObjectFace[] List, int ListCount, double[] ListDistance, int ListOffset, double TimeElapsed) {
            // calculate distance
            double cx = World.AbsoluteCameraPosition.X;
            double cy = World.AbsoluteCameraPosition.Y;
            double cz = World.AbsoluteCameraPosition.Z;
            for (int i = 0; i < ListCount; i++) {
                int o = List[i].ObjectIndex;
                int m = List[i].MeshIndex;
                int f = List[i].FaceIndex;
                if (ObjectManager.Objects[o].Meshes[m].Faces[f].Vertices.Length >= 3) {
                    int v0 = ObjectManager.Objects[o].Meshes[m].Faces[f].Vertices[0].Index;
                    int v1 = ObjectManager.Objects[o].Meshes[m].Faces[f].Vertices[1].Index;
                    int v2 = ObjectManager.Objects[o].Meshes[m].Faces[f].Vertices[2].Index;
                    double v0x = ObjectManager.Objects[o].Meshes[m].Vertices[v0].Coordinates.X;
                    double v0y = ObjectManager.Objects[o].Meshes[m].Vertices[v0].Coordinates.Y;
                    double v0z = ObjectManager.Objects[o].Meshes[m].Vertices[v0].Coordinates.Z;
                    double v1x = ObjectManager.Objects[o].Meshes[m].Vertices[v1].Coordinates.X;
                    double v1y = ObjectManager.Objects[o].Meshes[m].Vertices[v1].Coordinates.Y;
                    double v1z = ObjectManager.Objects[o].Meshes[m].Vertices[v1].Coordinates.Z;
                    double v2x = ObjectManager.Objects[o].Meshes[m].Vertices[v2].Coordinates.X;
                    double v2y = ObjectManager.Objects[o].Meshes[m].Vertices[v2].Coordinates.Y;
                    double v2z = ObjectManager.Objects[o].Meshes[m].Vertices[v2].Coordinates.Z;
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
                ObjectList[List[i].ObjectListIndex].FaceListIndices[List[i].MeshIndex][List[i].FaceIndex] = (i << 2) + ListOffset;
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
                    case World.GlowAttenuationMode.DivisionExponent2: {
                            double t = dx * dx + dy * dy + dz * dz;
                            return t / (t + halfdistance * halfdistance);
                        }
                    case World.GlowAttenuationMode.DivisionExponent4: {
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