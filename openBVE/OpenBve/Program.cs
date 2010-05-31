using System;
using Tao.OpenGl;
using Tao.Sdl;
using System.Windows.Forms;

namespace OpenBve {
	internal static partial class Program {

		// system
		internal static string RestartProcessArguments = null;
		internal enum Platform { Windows, Linux, Mac }
		internal static Platform CurrentPlatform = Platform.Windows;
		internal static bool CurrentlyRunOnMono = false;
		internal static bool UseFilesystemHierarchyStandard = false;
		internal enum ProgramType { OpenBve, RouteViewer, ObjectViewer, Other };
		internal const ProgramType CurrentProgramType = ProgramType.OpenBve;
		private static bool SdlWindowCreated = false;

		// main
		[STAThread]
		internal static void Main(string[] Args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			// platform and mono
			int p = (int)Environment.OSVersion.Platform;
			if (p == 4 | p == 128) {
				// general Unix
				CurrentPlatform = Platform.Linux;
			} else if (p == 6) {
				// Mac
				CurrentPlatform = Platform.Mac;
			} else {
				// non-Unix
				CurrentPlatform = Platform.Windows;
			}
			CurrentlyRunOnMono = Type.GetType("Mono.Runtime") != null;
			// file hierarchy standard
			if (CurrentPlatform != Platform.Windows) {
				for (int i = 0; i < Args.Length; i++) {
					if (Args[i].Equals("/fhs", StringComparison.OrdinalIgnoreCase)) {
						UseFilesystemHierarchyStandard = true;
						break;
					}
				}
			}
			string f = Interface.GetSettingsFolder();
			if (!System.IO.Directory.Exists(f)) {
				try {
					System.IO.Directory.CreateDirectory(f);
				} catch { }
			}
			// start
			#if DEBUG
			Start(Args);
			#else
			try {
				Start(Args);
			} catch (Exception ex) {
				string t = GetExceptionText(ex, 5);
				if (PluginManager.PluginError & PluginManager.PluginName != null) {
					MessageBox.Show("The " + PluginManager.PluginName + " plugin raised the following exception:\n\n" + t, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				} else {
					MessageBox.Show(t, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			#endif
			// deinitialize
			if(SdlWindowCreated & Interface.CurrentOptions.FullscreenMode) {
				Sdl.SDL_SetVideoMode(Interface.CurrentOptions.WindowWidth, Interface.CurrentOptions.WindowHeight, 32, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF);
			}
			Renderer.Deinitialize();
			TextureManager.UnuseAllTextures();
			Asynchronous.Deinitialize();
			PluginManager.UnloadPlugin();
			SoundManager.Deinitialize();
			// close sdl
			for (int i = 0; i < Interface.CurrentJoysticks.Length; i++) {
				Sdl.SDL_JoystickClose(Interface.CurrentJoysticks[i].SdlHandle);
				Interface.CurrentJoysticks[i].SdlHandle = IntPtr.Zero;
			}
			Sdl.SDL_Quit();
			// restart
			if (RestartProcessArguments != null) {
				System.Reflection.Assembly Assembly = System.Reflection.Assembly.GetExecutingAssembly();
				if (Program.UseFilesystemHierarchyStandard) {
					RestartProcessArguments += " /fhs";
				}
				System.Diagnostics.Process.Start(Assembly.Location, RestartProcessArguments);
			}
		}

		// get exception text
		/// <summary>Returns a textual representation of an exception and its inner exceptions.</summary>
		/// <param name="ex">The exception to serialize.</param>
		/// <param name="Levels">The amount of inner exceptions to include.</param>
		private static string GetExceptionText(Exception ex, int Levels) {
			if (Levels > 0 & ex.InnerException != null) {
				return ex.Message + "\n\n" + GetExceptionText(ex.InnerException, Levels - 1);
			} else {
				return ex.Message;
			}
		}

		// start
		private static void Start(string[] Args) {
			// initialize sdl video
			if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO) != 0) {
				MessageBox.Show("SDL failed to initialize the video subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			if (Sdl.SDL_Init(Sdl.SDL_INIT_JOYSTICK) != 0) {
				MessageBox.Show("SDL failed to initialize the joystick subsystem.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
			// initialize sdl joysticks
			{
				int n = Sdl.SDL_NumJoysticks();
				Interface.CurrentJoysticks = new Interface.Joystick[n];
				for (int i = 0; i < n; i++) {
					Interface.CurrentJoysticks[i].SdlHandle = Sdl.SDL_JoystickOpen(i);
					if (CurrentPlatform == Platform.Windows) {
						string s = Sdl.SDL_JoystickName(i);
						/* string returned is ascii packed in utf-16 (2 chars per codepoint) */
						System.Text.StringBuilder t = new System.Text.StringBuilder(s.Length << 1);
						for (int k = 0; k < s.Length; k++) {
							int a = (int)s[k];
							t.Append(char.ConvertFromUtf32(a & 0xFF) + char.ConvertFromUtf32(a >> 8));
						}
						Interface.CurrentJoysticks[i].Name = t.ToString();
					} else {
						Interface.CurrentJoysticks[i].Name = Sdl.SDL_JoystickName(i);
					}
				}
			}
			// load options and controls
			Interface.LoadOptions();
			Interface.LoadControls(null, out Interface.CurrentControls);
			{
				string f = Interface.GetCombinedFileName(Interface.GetDataFolder("Controls"), "Default keyboard assignment.controls");
				Interface.Control[] c;
				Interface.LoadControls(f, out c);
				Interface.AddControls(ref Interface.CurrentControls, c);
			}
			// command line arguments
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
			// train provided
			if (Result.TrainFolder != null) {
				if (System.IO.Directory.Exists(Result.TrainFolder)) {
					string File = Interface.GetCombinedFileName(Result.TrainFolder, "train.dat");
					if (System.IO.File.Exists(File)) {
						Result.TrainEncoding = System.Text.Encoding.UTF8;
						for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
								Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
							}
						}
					} else {
						Result.TrainFolder = null;
					}
				} else {
					Result.TrainFolder = null;
				}
			}
			// route provided
			if (Result.RouteFile != null) {
				if (!System.IO.File.Exists(Result.RouteFile)) {
					Result.RouteFile = null;
				}
			}
			// route provided but no train
			if (Result.RouteFile != null & Result.TrainFolder == null) {
				bool IsRW = string.Equals(System.IO.Path.GetExtension(Result.RouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
				CsvRwRouteParser.ParseRoute(Result.RouteFile, IsRW, Result.RouteEncoding, null, null, null, true);
				if (Game.TrainName != null && Game.TrainName.Length != 0) {
					string Folder = System.IO.Path.GetDirectoryName(Result.RouteFile);
					while (true) {
						string TrainFolder = Interface.GetCombinedFolderName(Folder, "Train");
						if (System.IO.Directory.Exists(TrainFolder)) {
							Folder = Interface.GetCombinedFolderName(TrainFolder, Game.TrainName);
							if (System.IO.Directory.Exists(Folder)) {
								string File = Interface.GetCombinedFileName(Folder, "train.dat");
								if (System.IO.File.Exists(File)) {
									// associated train found
									Result.TrainFolder = Folder;
									Result.TrainEncoding = System.Text.Encoding.UTF8;
									for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
										if (string.Compare(Interface.CurrentOptions.TrainEncodings[j].Value, Result.TrainFolder, StringComparison.InvariantCultureIgnoreCase) == 0) {
											Result.TrainEncoding = System.Text.Encoding.GetEncoding(Interface.CurrentOptions.TrainEncodings[j].Codepage);
											break;
										}
									}
								}
							} break;
						} else {
							System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
							if (Info != null) {
								Folder = Info.FullName;
							} else {
								break;
							}
						}
					}
				}
				Game.Reset(false);
			}
			// show main menu if applicable
			if (Result.RouteFile == null | Result.TrainFolder == null) {
				Result = formMain.ShowMainDialog();
				if (!Result.Start) return;
			}
			// screen
			int Width = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenWidth : Interface.CurrentOptions.WindowWidth;
			int Height = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenHeight : Interface.CurrentOptions.WindowHeight;
			if (Width < 16) Width = 16;
			if (Height < 16) Height = 16;
			Renderer.ScreenWidth = Width;
			Renderer.ScreenHeight = Height;
			World.AspectRatio = (double)Renderer.ScreenWidth / (double)Renderer.ScreenHeight;
			const double degree = 0.0174532925199433;
			World.VerticalViewingAngle = 45.0 * degree;
			World.HorizontalViewingAngle = 2.0 * Math.Atan(Math.Tan(0.5 * World.VerticalViewingAngle) * World.AspectRatio);
			World.OriginalVerticalViewingAngle = World.VerticalViewingAngle;
			World.ExtraViewingDistance = 50.0;
			World.ForwardViewingDistance = (double)Interface.CurrentOptions.ViewingDistance + World.ExtraViewingDistance;
			World.BackwardViewingDistance = 0.0;
			World.BackgroundImageDistance = (double)Interface.CurrentOptions.ViewingDistance;
			// load route and train
			SoundManager.Initialize();
			if (!Loading.Load(Result.RouteFile, Result.RouteEncoding, Result.TrainFolder, Result.TrainEncoding)) {
				return;
			}
			Game.LogRouteName = System.IO.Path.GetFileName(Result.RouteFile);
			Game.LogTrainName = System.IO.Path.GetFileName(Result.TrainFolder);
			Game.LogDateTime = DateTime.Now;
			// initialize sdl window
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DOUBLEBUFFER, 1);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_DEPTH_SIZE, 24);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_RED_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_GREEN_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_BLUE_SIZE, 8);
			//Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_ALPHA_SIZE, 8);
			Sdl.SDL_GL_SetAttribute(Sdl.SDL_GL_SWAP_CONTROL, Interface.CurrentOptions.VerticalSynchronization ? 1 : 0);
			Sdl.SDL_ShowCursor(Sdl.SDL_DISABLE);
			SdlWindowCreated = true;
			int Bits = Interface.CurrentOptions.FullscreenMode ? Interface.CurrentOptions.FullscreenBits : 32;
			// icon
			{
				string File = Interface.GetCombinedFileName(Interface.GetDataFolder(), "icon.bmp");
				if (System.IO.File.Exists(File)) {
					try {
						IntPtr Bitmap = Sdl.SDL_LoadBMP(File);
						if (Bitmap != null) {
							if (CurrentPlatform == Platform.Windows) {
								Sdl.SDL_Surface Surface = (Sdl.SDL_Surface)System.Runtime.InteropServices.Marshal.PtrToStructure(Bitmap, typeof(Sdl.SDL_Surface));
								int ColorKey = Sdl.SDL_MapRGB(Surface.format, 0, 0, 255);
								Sdl.SDL_SetColorKey(Bitmap, Sdl.SDL_SRCCOLORKEY, ColorKey);
								Sdl.SDL_WM_SetIcon(Bitmap, null);
							} else {
								Sdl.SDL_WM_SetIcon(Bitmap, null);
							}
						}
					} catch { }
				}
			}
			// create window
			int fullscreen = Interface.CurrentOptions.FullscreenMode ? Sdl.SDL_FULLSCREEN : 0;
			IntPtr video = Sdl.SDL_SetVideoMode(Width, Height, Bits, Sdl.SDL_OPENGL | Sdl.SDL_DOUBLEBUF | fullscreen);
			if (video != IntPtr.Zero) {
				// create window
				Sdl.SDL_WM_SetCaption(Application.ProductName, null);
				if (Interface.CurrentOptions.KeyRepeatDelay > 0 & Interface.CurrentOptions.KeyRepeatInterval > 0) {
					Sdl.SDL_EnableKeyRepeat(Interface.CurrentOptions.KeyRepeatDelay, Interface.CurrentOptions.KeyRepeatInterval);
				}
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
				Timetable.CreateTimetable();
				// camera
				MainLoop.UpdateViewport(MainLoop.ViewPortChangeMode.NoChange);
				MainLoop.InitializeMotionBlur();
				// start loop
				MainLoop.StartLoop();
			} else {
				// failed
				MessageBox.Show("SDL failed to create the window.", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

	}
}