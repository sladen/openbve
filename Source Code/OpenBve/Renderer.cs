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
        internal static World.Vector3Df OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
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
            OptionLightPosition = new World.Vector3Df(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);
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
            // hud
            Interface.LoadHUD();
            string Path = Interface.GetDataFolder("Graphics");
            TextureLogo = TextureManager.RegisterTexture(Interface.GetCombinedFileName(Path, "logo.png"), new World.ColorRGB(0, 0, 0), 0, TextureManager.TextureWrapMode.ClampToEdge, false);
            TextureManager.ValidateTexture(ref TextureLogo);
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
                        Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
                        Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    }
                } else {
                    for (int j = 0; j < Face.Vertices.Length; j++) {
                        Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
                        Gl.glVertex3d(Vertices[Face.Vertices[j].Index].Coordinates.X - CameraX, Vertices[Face.Vertices[j].Index].Coordinates.Y - CameraY, Vertices[Face.Vertices[j].Index].Coordinates.Z - CameraZ);
                    }
                }
            } else {
                if (LightingEnabled) {
                    for (int j = 0; j < Face.Vertices.Length; j++) {
                        Gl.glNormal3f(Face.Vertices[j].Normal.X, Face.Vertices[j].Normal.Y, Face.Vertices[j].Normal.Z);
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
                    Gl.glTexCoord2f(Vertices[Face.Vertices[j].Index].TextureCoordinates.X, Vertices[Face.Vertices[j].Index].TextureCoordinates.Y);
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
            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Bve4Plugin) {
                Count = 0;
            } else if (TrainManager.PlayerTrain.Specs.Security.Ats.AtsPAvailable & TrainManager.PlayerTrain.Specs.Security.Atc.Available) {
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
            } else if (TrainManager.PlayerTrain.Specs.Security.Ats.AtsPAvailable) {
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
            } else if (TrainManager.PlayerTrain.Specs.Security.Atc.Available & TrainManager.PlayerTrain.Specs.Security.Ats.AtsAvailable) {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.None);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.Atc);
                CurrentLampCollection.Lamps[4] = new Lamp(LampType.AtcPower);
                CurrentLampCollection.Lamps[5] = new Lamp(LampType.AtcUse);
                CurrentLampCollection.Lamps[6] = new Lamp(LampType.AtcEmergency);
                Count = 7;
            } else if (TrainManager.PlayerTrain.Specs.Security.Atc.Available) {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Atc);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtcPower);
                CurrentLampCollection.Lamps[2] = new Lamp(LampType.AtcUse);
                CurrentLampCollection.Lamps[3] = new Lamp(LampType.AtcEmergency);
                Count = 4;
            } else if (TrainManager.PlayerTrain.Specs.Security.Ats.AtsAvailable) {
                CurrentLampCollection.Lamps[0] = new Lamp(LampType.Ats);
                CurrentLampCollection.Lamps[1] = new Lamp(LampType.AtsOperation);
                Count = 2;
            } else {
                Count = 0;
            }
            if (TrainManager.PlayerTrain.Specs.Security.Mode != TrainManager.SecuritySystem.Bve4Plugin) {
                if (TrainManager.PlayerTrain.Specs.Security.Eb.Available | TrainManager.PlayerTrain.Specs.HasConstSpeed) {
                    CurrentLampCollection.Lamps[Count] = new Lamp(LampType.None);
                    Count++;
                }
                if (TrainManager.PlayerTrain.Specs.Security.Eb.Available) {
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
            // hud
            TrainManager.TrainDoorState LeftDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, true, false);
            TrainManager.TrainDoorState RightDoors = TrainManager.GetDoorsState(TrainManager.PlayerTrain, false, true);
            for (int i = 0; i < Interface.CurrentHudElements.Length; i++) {
                string Command = Interface.CurrentHudElements[i].Subject.ToLowerInvariant();
                switch (Command) {
                    case "ats": {
                            // ats lamps
                            if (CurrentLampCollection.Lamps == null) InitializeLamps();
                            double lcrh = 0.0;
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
                            double w = (double)CurrentLampCollection.Width + lw + rw;
                            double h = lcrh * CurrentLampCollection.Lamps.Length - Interface.CurrentHudElements[i].Value * (CurrentLampCollection.Lamps.Length - 1);
                            double x = Interface.CurrentHudElements[i].Alignment.X < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.X > 0 ? ScreenWidth - w : 0.5 * (ScreenWidth - w);
                            double y = Interface.CurrentHudElements[i].Alignment.Y < 0 ? 0.0 : Interface.CurrentHudElements[i].Alignment.Y > 0 ? ScreenHeight - h : 0.5 * (ScreenHeight - h);
                            x += Interface.CurrentHudElements[i].Position.X;
                            y += Interface.CurrentHudElements[i].Position.Y;
                            float ba = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.A;
                            float or = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.R;
                            float og = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.G;
                            float ob = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.B;
                            float oa = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.A;
                            int n = CurrentLampCollection.Lamps.Length;
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
                                    float br = 0.4f, bg = 0.4f, bb = 0.4f;
                                    switch (CurrentLampCollection.Lamps[j].Type) {
                                        case LampType.Ats:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Normal | TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Initialization) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.AtsOperation:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Ringing) {
                                                    br = 1.0f; bg = 0.0f; bb = 0.0f;
                                                } else if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Emergency | TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Pattern | TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Service) {
                                                    if (((int)Math.Floor(2.0 * Game.SecondsSinceMidnight) & 1) == 0) {
                                                        br = 1.0f; bg = 0.0f; bb = 0.0f;
                                                    }
                                                }
                                            } break;
                                        case LampType.AtsPPower:
                                            if ((TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsSN | TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) & TrainManager.PlayerTrain.Specs.Security.Ats.AtsPAvailable) {
                                                br = 0.3f; bg = 1.0f; bb = 0.0f;
                                            } break;
                                        case LampType.AtsPPattern:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Pattern | TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Service) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.AtsPBrakeOverride:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                                if (TrainManager.PlayerTrain.Specs.Security.Ats.AtsPOverride) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.AtsPBrakeOperation:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Service & !TrainManager.PlayerTrain.Specs.Security.Ats.AtsPOverride) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.AtsP:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                                br = 0.3f; bg = 1.0f; bb = 0.0f;
                                            } break;
                                        case LampType.AtsPFailure:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode != TrainManager.SecuritySystem.None) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Initialization) {
                                                    br = 1.0f; bg = 0.0f; bb = 0.0f;
                                                } else if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.AtsP) {
                                                    if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Ringing | TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Emergency) {
                                                        br = 1.0f; bg = 0.0f; bb = 0.0f;
                                                    }
                                                }
                                            } break;
                                        case LampType.Atc:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                                br = 1.0f; bg = 0.7f; bb = 0.0f;
                                            } break;
                                        case LampType.AtcPower:
                                            if ((TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc | TrainManager.PlayerTrain.Specs.Security.Mode != TrainManager.SecuritySystem.None & TrainManager.PlayerTrain.Specs.Security.Atc.AutomaticSwitch)) {
                                                br = 1.0f; bg = 0.7f; bb = 0.0f;
                                            } break;
                                        case LampType.AtcUse:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                                if (TrainManager.PlayerTrain.Specs.Security.State == TrainManager.SecurityState.Service) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.AtcEmergency:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                                if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Security != TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
                                                    br = 1.0f; bg = 0.0f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.Eb:
                                            if (TrainManager.PlayerTrain.Specs.Security.Mode != TrainManager.SecuritySystem.None) {
                                                if (TrainManager.PlayerTrain.Specs.Security.Eb.BellState == TrainManager.SecurityState.Ringing) {
                                                    br = 0.3f; bg = 1.0f; bb = 0.0f;
                                                }
                                            } break;
                                        case LampType.ConstSpeed:
                                            if (TrainManager.PlayerTrain.Specs.HasConstSpeed) {
                                                if (TrainManager.PlayerTrain.Specs.CurrentConstSpeed) {
                                                    br = 1.0f; bg = 0.7f; bb = 0.0f;
                                                }
                                            } break;
                                    }
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
                                        float r = inv255 * (float)Interface.CurrentHudElements[i].TextColor.R;
                                        float g = inv255 * (float)Interface.CurrentHudElements[i].TextColor.G;
                                        float b = inv255 * (float)Interface.CurrentHudElements[i].TextColor.B;
                                        float a = inv255 * (float)Interface.CurrentHudElements[i].TextColor.A;
                                        double u = CurrentLampCollection.Lamps[j].Width;
                                        double v = CurrentLampCollection.Lamps[j].Height;
                                        double p = Interface.CurrentHudElements[i].TextAlignment.X < 0 ? x : Interface.CurrentHudElements[i].TextAlignment.X > 0 ? x + w - u : x + 0.5 * (w - u);
                                        double q = Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? y : Interface.CurrentHudElements[i].TextAlignment.Y > 0 ? y + lcrh - v : y + 0.5 * (lcrh - v);
                                        p += Interface.CurrentHudElements[i].TextPosition.X;
                                        q += Interface.CurrentHudElements[i].TextPosition.Y;
                                        RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, r, g, b, a, Interface.CurrentHudElements[i].TextShadow);
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
                                y += lcrh - (double)Interface.CurrentHudElements[i].Value;
                            }
                        } break;
                    default: {
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
                            float br, bg, bb, ba = 1.0f;
                            string t;
                            switch (Command) {
                                case "reverser":
                                    if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver < 0) {
                                        br = 1.0f; bg = 0.7f; bb = 0.0f; t = Interface.QuickReferences.HandleBackward;
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentReverser.Driver > 0) {
                                        br = 0.0f; bg = 0.7f; bb = 1.0f; t = Interface.QuickReferences.HandleForward;
                                    } else {
                                        br = 0.4f; bg = 0.4f; bb = 0.4f; t = Interface.QuickReferences.HandleNeutral;
                                    } break;
                                case "power":
                                    if (TrainManager.PlayerTrain.Specs.SingleHandle) {
                                        continue;
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver == 0) {
                                        br = 0.4f; bg = 0.4f; bb = 0.4f; t = Interface.QuickReferences.HandlePowerNull;
                                    } else {
                                        br = 0.0f; bg = 0.7f; bb = 1.0f; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
                                    } break;
                                case "brake":
                                    if (TrainManager.PlayerTrain.Specs.SingleHandle) {
                                        continue;
                                    } else if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.BrakeType == TrainManager.CarBrakeType.AutomaticAirBrake) {
                                        if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
                                            br = 1.0f; bg = 0.0f; bb = 0.0f; t = Interface.QuickReferences.HandleEmergency;
                                        } else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Release) {
                                            br = 0.4f; bg = 0.4f; bb = 0.4f; t = Interface.QuickReferences.HandleRelease;
                                        } else if (TrainManager.PlayerTrain.Specs.AirBrake.Handle.Driver == TrainManager.AirBrakeHandleState.Lap) {
                                            br = 0.0f; bg = 0.7f; bb = 1.0f; t = Interface.QuickReferences.HandleLap;
                                        } else {
                                            br = 1.0f; bg = 0.7f; bb = 0.0f; t = Interface.QuickReferences.HandleService;
                                        }
                                    } else {
                                        if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
                                            br = 1.0f; bg = 0.0f; bb = 0.0f; t = Interface.QuickReferences.HandleEmergency;
                                        } else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
                                            br = 1.0f; bg = 0.7f; bb = 0.0f; t = Interface.QuickReferences.HandleHoldBrake;
                                        } else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver == 0) {
                                            br = 0.4f; bg = 0.4f; bb = 0.4f; t = Interface.QuickReferences.HandleBrakeNull;
                                        } else {
                                            br = 0.0f; bg = 0.7f; bb = 1.0f; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
                                        }
                                    } break;
                                case "single":
                                    if (!TrainManager.PlayerTrain.Specs.SingleHandle) {
                                        continue;
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentEmergencyBrake.Driver) {
                                        br = 1.0f; bg = 0.0f; bb = 0.0f; t = Interface.QuickReferences.HandleEmergency;
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentHoldBrake.Driver) {
                                        br = 0.3f; bg = 1.0f; bb = 0.0f; t = Interface.QuickReferences.HandleHoldBrake;
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver > 0) {
                                        br = 1.0f; bg = 0.7f; bb = 0.0f; t = Interface.QuickReferences.HandleBrake + TrainManager.PlayerTrain.Specs.CurrentBrakeNotch.Driver.ToString(Culture);
                                    } else if (TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver > 0) {
                                        br = 0.0f; bg = 0.7f; bb = 1.0f; t = Interface.QuickReferences.HandlePower + TrainManager.PlayerTrain.Specs.CurrentPowerNotch.Driver.ToString(Culture);
                                    } else {
                                        br = 0.4f; bg = 0.4f; bb = 0.4f; t = Interface.QuickReferences.HandlePowerNull;
                                    } break;
                                case "doorsleft":
                                case "doorsright": {
                                        if ((LeftDoors & TrainManager.TrainDoorState.AllClosed) == 0 | (RightDoors & TrainManager.TrainDoorState.AllClosed) == 0) {
                                            Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
                                            if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
                                        } else {
                                            Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
                                            if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
                                        }
                                        TrainManager.TrainDoorState Doors = Command == "doorsleft" ? LeftDoors : RightDoors;
                                        if ((Doors & TrainManager.TrainDoorState.Mixed) != 0) {
                                            br = 1.0f; bg = 0.7f; bb = 0.0f;
                                        } else if ((Doors & TrainManager.TrainDoorState.AllClosed) != 0) {
                                            br = 0.4f; bg = 0.4f; bb = 0.4f;
                                        } else {
                                            if (TrainManager.PlayerTrain.Specs.DoorCloseMode == TrainManager.DoorMode.Manual) {
                                                br = 0.3f; bg = 1.0f; bb = 0.0f;
                                            } else {
                                                br = 0.0f; bg = 0.7f; bb = 1.0f;
                                            }
                                        }
                                        t = Command == "doorsleft" ? Interface.QuickReferences.DoorsLeft : Interface.QuickReferences.DoorsRight;
                                    } break;
                                case "stopleft":
                                case "stopright":
                                case "stopnone": {
                                        int s = TrainManager.PlayerTrain.Station;
                                        if (s >= 0) {
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
                                        br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                        bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                        bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
                                        t = Interface.CurrentHudElements[i].Text;
                                    } break;
                                case "stoplefttick":
                                case "stoprighttick":
                                case "stopnonetick": {
                                        int s = TrainManager.PlayerTrain.Station;
                                        if (s >= 0) {
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
                                                double d = TrainManager.PlayerTrain.StationStopDifference;
                                                double r;
                                                if (d < 0.0) {
                                                    r = d / Game.Stations[s].Stops[c].BackwardTolerance;
                                                } else {
                                                    r = d / Game.Stations[s].Stops[c].ForwardTolerance;
                                                }
                                                if (r < -1.0) r = -1.0;
                                                if (r > 1.0) r = 1.0;
                                                y -= r * (double)Interface.CurrentHudElements[i].Value;
                                            } else {
                                                Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
                                                if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
                                            }
                                        } else {
                                            Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
                                            if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
                                        }
                                        br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                        bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                        bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
                                        t = Interface.CurrentHudElements[i].Text;
                                    } break;
                                case "clock": {
                                        br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                        bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                        bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
                                        int hours = (int)Math.Floor(Game.SecondsSinceMidnight);
                                        int seconds = hours % 60; hours /= 60;
                                        int minutes = hours % 60; hours /= 60;
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
                                    br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                    bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                    bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
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
                                    br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                    bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                    bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
                                    int fps = (int)Math.Round(Game.InfoFrameRate);
                                    t = fps.ToString(Culture) + " fps";
                                    if (OptionFrameRates) {
                                        Interface.CurrentHudElements[i].TransitionState -= speed * TimeElapsed;
                                        if (Interface.CurrentHudElements[i].TransitionState < 0.0) Interface.CurrentHudElements[i].TransitionState = 0.0;
                                    } else {
                                        Interface.CurrentHudElements[i].TransitionState += speed * TimeElapsed;
                                        if (Interface.CurrentHudElements[i].TransitionState > 1.0) Interface.CurrentHudElements[i].TransitionState = 1.0;
                                    } break;
                                default:
                                    br = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.R;
                                    bg = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.G;
                                    bb = inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.B;
                                    t = Interface.CurrentHudElements[i].Text;
                                    break;
                            }
                            /// transitions
                            float alpha = 1.0f;
                            if (Interface.CurrentHudElements[i].Transition == Interface.HudTransition.Move | Interface.CurrentHudElements[i].Transition == Interface.HudTransition.MoveAndFade) {
                                double s = Interface.CurrentHudElements[i].TransitionState;
                                x += Interface.CurrentHudElements[i].TransitionVector.X * s * s;
                                y += Interface.CurrentHudElements[i].TransitionVector.Y * s * s;
                            }
                            if (Interface.CurrentHudElements[i].Transition == Interface.HudTransition.Fade | Interface.CurrentHudElements[i].Transition == Interface.HudTransition.MoveAndFade) {
                                alpha = (float)(1.0 - Interface.CurrentHudElements[i].TransitionState);
                            }
                            /// render
                            if (alpha != 0.0f) {
                                /// background
                                if (Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex >= 0) {
                                    int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, TextureManager.UseMode.LoadImmediately);
                                    ba *= inv255 * (float)Interface.CurrentHudElements[i].BackgroundColor.A * alpha;
                                    Gl.glColor4f(br, bg, bb, ba);
                                    RenderOverlayTexture(Interface.CurrentHudElements[i].CenterMiddle.BackgroundTextureIndex, x, y, x + w, y + h);
                                }
                                { /// text
                                    float r = inv255 * (float)Interface.CurrentHudElements[i].TextColor.R;
                                    float g = inv255 * (float)Interface.CurrentHudElements[i].TextColor.G;
                                    float b = inv255 * (float)Interface.CurrentHudElements[i].TextColor.B;
                                    float a = inv255 * (float)Interface.CurrentHudElements[i].TextColor.A * alpha;
                                    float u, v;
                                    MeasureString(t, Interface.CurrentHudElements[i].TextSize, out u, out v);
                                    double p = Interface.CurrentHudElements[i].TextAlignment.X < 0 ? x : Interface.CurrentHudElements[i].TextAlignment.X == 0 ? x + 0.5 * (w - u) : x + w - u;
                                    double q = Interface.CurrentHudElements[i].TextAlignment.Y < 0 ? y : Interface.CurrentHudElements[i].TextAlignment.Y == 0 ? y + 0.5 * (h - v) : y + h - v;
                                    p += Interface.CurrentHudElements[i].TextPosition.X;
                                    q += Interface.CurrentHudElements[i].TextPosition.Y;
                                    RenderString(p, q, Interface.CurrentHudElements[i].TextSize, t, -1, r, g, b, a, Interface.CurrentHudElements[i].TextShadow);
                                }
                                /// overlay
                                if (Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex >= 0) {
                                    int OpenGlTextureIndex = TextureManager.UseTexture(Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex, TextureManager.UseMode.LoadImmediately);
                                    float r, g, b, a;
                                    r = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.R;
                                    g = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.G;
                                    b = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.B;
                                    a = inv255 * (float)Interface.CurrentHudElements[i].OverlayColor.A * alpha;
                                    Gl.glColor4f(r, g, b, a);
                                    RenderOverlayTexture(Interface.CurrentHudElements[i].CenterMiddle.OverlayTextureIndex, x, y, x + w, y + h);
                                }
                            }
                        } break;
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
                for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++) {
                    double x = 96.0, w = 128.0;
                    /// brake pipe
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
                    /// auxillary reservoir
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
                    /// brake cylinder
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
                    /// main reservoir
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
                    /// equalizing reservoir
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
                    /// straight air pipe
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
                                t += "speed: " + (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6).ToString("0.00", Culture) + " km/h - power: " + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Specs.CurrentAccelerationOutput.ToString("0.0000", Culture) + " m/s² - acc: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²";
                            } break;
                        case 2:
                            if (Game.CurrentMode != Game.GameMode.Expert) {
                                t = "route limit: " + (TrainManager.PlayerTrain.CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h"));
                                t += ", signal limit: " + (TrainManager.PlayerTrain.CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h"));
                                if (TrainManager.PlayerTrain.Specs.Security.Mode == TrainManager.SecuritySystem.Atc) {
                                    t += ", atc limit: " + (TrainManager.PlayerTrain.Specs.Security.Atc.SpeedRestriction == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.Specs.Security.Atc.SpeedRestriction * 3.6).ToString("0.0", Culture) + " km/h"));
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
                                int d = TrainManager.PlayerTrain.DriverCar;
                                t = "elevation: " + (Game.RouteInitialElevation + TrainManager.PlayerTrain.Cars[d].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m, temperature: " + (TrainManager.PlayerTrain.Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + "[" + (Game.RouteSeaLevelAirTemperature - 273.15).ToString("0.00", Culture) + "] °C, pressure: " + (0.001 * TrainManager.PlayerTrain.Specs.CurrentAirPressure).ToString("0.00", Culture) + "[" + (0.001 * Game.RouteSeaLevelAirPressure).ToString("0.00", Culture) + "] kPa, density: " + TrainManager.PlayerTrain.Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³";
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
            //if (FadeLogo > 0.0) {
            //    double size = ScreenWidth > ScreenHeight ? ScreenWidth : ScreenHeight;
            //    if (FadeLogo > 1.0) {
            //        Gl.glColor3d(1.0, 1.0, 1.0);
            //        FadeLogo -= 1.0;
            //    } else {
            //        FadeLogo -= TimeElapsed;
            //        if (FadeLogo < 0.0) FadeLogo = 0.0;
            //        Gl.glColor4d(1.0, 1.0, 1.0, FadeLogo);
            //    }
            //    RenderOverlayTexture(TextureLogo, 0.5 * (ScreenWidth - size), 0.5 * (ScreenHeight - size), 0.5 * (ScreenWidth + size), 0.5 * (ScreenHeight + size));
            //}
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
            if (Text == null) return;
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