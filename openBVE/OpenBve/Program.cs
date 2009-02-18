using System;
using Tao.OpenGl;
using Tao.Sdl;
using System.Windows.Forms;

namespace OpenBve {
    internal static class Program {

        // members
        internal static string RestartProcessArguments = null;
        internal enum Platform { Windows, Linux, Mac }
        internal static Platform CurrentPlatform = Platform.Windows;
        internal static bool CurrentlyRunOnMono = false;

        // main
        [STAThread]
        internal static void Main(string[] Args) {
            int p = (int)Environment.OSVersion.Platform;
            if (p == 4 | p == 128) {
                /// general Unix
                CurrentPlatform = Platform.Linux;
            } else if (p == 6) {
                /// explicitly Mac
                CurrentPlatform = Platform.Mac;
            } else {
                /// non-Unix
                CurrentPlatform = Platform.Windows;
            }
            CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
#if DEBUG
            Start(Args);
#else
            try {
                Start(Args);
            } catch (Exception ex) {
                MessageBox.Show(GetExceptionText(ex, 5), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif
            Asynchronous.Deinitialize();
            PluginManager.UnloadPlugin();
            TextureManager.UnuseAllTextures();
            SoundManager.Deinitialize();
            // close sdl
            for (int i = 0; i < Interface.Joysticks.Length; i++) {
                Sdl.SDL_JoystickClose(Interface.Joysticks[i].SdlHandle);
                Interface.Joysticks[i].SdlHandle = IntPtr.Zero;
            }
            Sdl.SDL_Quit();
            // restart
            if (RestartProcessArguments != null) {
                System.Reflection.Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string File = Assembly.Location;
                System.Diagnostics.Process.Start(File, RestartProcessArguments);
            }
        }

        // get exception text
        private static string GetExceptionText(Exception ex, int Levels) {
            if (Levels > 0 & ex.InnerException != null) {
                return ex.Message + "\n\n" + GetExceptionText(ex.InnerException, Levels - 1);
            } else {
                return ex.Message;
            }
        }

        // start
        private static void Start(string[] Args) {
            // initialize sdl video and joysticks
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO) != 0) {
                MessageBox.Show("SDL failed to initialize the video subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            if (Sdl.SDL_Init(Sdl.SDL_INIT_JOYSTICK) != 0) {
                MessageBox.Show("SDL failed to initialize the joystick subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            {
                int n = Sdl.SDL_NumJoysticks();
                Interface.Joysticks = new Interface.Joystick[n];
                for (int i = 0; i < n; i++) {
                    Interface.Joysticks[i].SdlHandle = Sdl.SDL_JoystickOpen(i);
                    /// <info>Sdl.SDL_JoystickName seems to return mojibake, thus it has been disabled</info>
                    //Interface.Joysticks[i].Name = Sdl.SDL_JoystickName(i);
                    Interface.Joysticks[i].Name = "Joystick " + (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
                }
            }
            // command line arguments
            Interface.LoadOptions();
            Interface.LoadControls(null);
            formMain.MainDialogResult Result = new formMain.MainDialogResult();
            for (int i = 0; i < Args.Length; i++) {
                if (Args[i].StartsWith("/route=", StringComparison.OrdinalIgnoreCase)) {
                    Result.RouteFile = Args[i].Substring(7);
                    Result.RouteEncoding = System.Text.Encoding.UTF8;
                    for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
                        if (string.Compare(Interface.CurrentOptions.RouteEncodings[j].Value, Result.RouteFile, StringComparison.InvariantCultureIgnoreCase) == 0) {
                            Result.RouteEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.RouteEncodings[j].Codepage);
                            break;
                        }
                    }
                } else if (Args[i].StartsWith("/train=", StringComparison.OrdinalIgnoreCase)) {
                    Result.TrainFolder = Args[i].Substring(7);
                    Result.TrainEncoding = System.Text.Encoding.UTF8;
                    for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
                        if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
                            Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
                            break;
                        }
                    }
                }
            }
            // show main menu
            if (Result.RouteFile == null || Result.TrainFolder == null || !System.IO.File.Exists(Result.RouteFile) || !System.IO.Directory.Exists(Result.TrainFolder)) {
                Result = formMain.ShowMainDialog();
                if (!Result.Start) {
                    return;
                }
            }
            // screen
            int Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
            int Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
            Renderer.ScreenWidth = Width;
            Renderer.ScreenHeight = Height;
            World.AspectRatio = (double)Renderer.ScreenWidth / (double)Renderer.ScreenHeight;
            World.VerticalViewingAngle = 45.0 * 0.0174532925199433;
            World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
            World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
            World.ExtraViewingDistance = 50.0;
            World.ForwardViewingDistance = (double)Interface.CurrentOptions.ViewingDistance + World.ExtraViewingDistance;
            World.BackwardViewingDistance = 0.0;
            World.BackgroundImageDistance = (double)Interface.CurrentOptions.ViewingDistance;
            // load route and train
            if (!Loading.Load(Result.RouteFile, Result.RouteEncoding, Result.TrainFolder, Result.TrainEncoding)) {
                return;
            }
            Game.LogRouteName = System.IO.Path.GetFileName(Result.RouteFile);
            Game.LogTrainName = System.IO.Path.GetFileName(Result.TrainFolder);
            Game.LogDateTime = DateTime.Now;
            Game.CurrentMode = Result.Mode;
            // initialize sdl window
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 16);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
            Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, 8);
            Sdl.SDL_ShowCursor(Sdl.SDL_DISABLE);
            int Bits = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenBits : 32;
            // icon
            if (Program.CurrentPlatform == Platform.Windows) {
                string File = Interface.GetCombinedFileName(Interface.GetCombinedFolderName(Application.StartupPath, "Interface"), "icon.bmp");
                if (System.IO.File.Exists(File)) {
                    IntPtr Bitmap = Sdl.SDL_LoadBMP(File);
                    if (Bitmap != null) {
                        Sdl.SDL_Surface Surface = (Sdl.SDL_Surface)System.Runtime.InteropServices.Marshal.PtrToStructure(Bitmap, typeof(Sdl.SDL_Surface));
                        int ColorKey = Sdl.SDL_MapRGB(Surface.format, 0, 0, 255);
                        Sdl.SDL_SetColorKey(Bitmap, Sdl.SDL_SRCCOLORKEY, ColorKey);
                        Sdl.SDL_WM_SetIcon(Bitmap, null);
                    }
                }
            }
            // create window
            IntPtr video = Sdl.SDL_SetVideoMode(Width, Height, Bits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | (Interface.CurrentOptions.FullscreenMode ? Sdl.SDL_FULLSCREEN : 0));
            if (video != IntPtr.Zero) {
                // create window
                Sdl.SDL_WM_SetCaption(Application.ProductName, null);
                // anisotropic filtering
                string[] Extensions = Gl.glGetString(Gl.GL_EXTENSIONS).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
                for (int i = 0; i < Extensions.Length; i++) {
                    if (string.Compare(Extensions[i], "GL_EXT_texture_filter_anisotropic", StringComparison.OrdinalIgnoreCase) == 0) {
                        float n; Gl.glGetFloatv(Gl.GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT, out n);
                        Interface.CurrentOptions.AnisotropicFilteringMaximum = (int)Math.Round((double)n);
                        break;
                    }
                }
                if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0) {
                    Interface.CurrentOptions.AnisotropicFilteringMaximum = 0;
                    Interface.CurrentOptions.AnisotropicFilteringLevel = 0;
                } else if (Interface.CurrentOptions.AnisotropicFilteringLevel == 0 & Interface.CurrentOptions.AnisotropicFilteringMaximum > 0) {
                    Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
                } else if (Interface.CurrentOptions.AnisotropicFilteringLevel > Interface.CurrentOptions.AnisotropicFilteringMaximum) {
                    Interface.CurrentOptions.AnisotropicFilteringLevel = Interface.CurrentOptions.AnisotropicFilteringMaximum;
                }
                // module initialization
                Fonts.Initialize();
                Renderer.Initialize();
                Renderer.InitializeLighting();
                Sdl.SDL_GL_SwapBuffers();
                SoundManager.Initialize();
                Timetable.CreateTimetable();
                // camera
                MainLoop.UpdateViewport();
                MainLoop.InitializeMotionBlur();
                // start loop
                MainLoop.StartLoop();
            } else {
                MessageBox.Show("SDL failed to create the window.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

    }
}