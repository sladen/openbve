using System;
using System.Drawing;
using System.Windows.Forms;
using Tao.Sdl;
using System.Text;

namespace OpenBve {
	public partial class formMain : Form {
		public formMain() {
			InitializeComponent();
		}

		// show main dialog
		internal struct MainDialogResult {
			internal bool Start;
			internal string RouteFile;
			internal System.Text.Encoding RouteEncoding;
			internal string TrainFolder;
			internal System.Text.Encoding TrainEncoding;
		}
		internal static MainDialogResult ShowMainDialog() {
			formMain Dialog = new formMain();
			Dialog.ShowDialog();
			MainDialogResult Result = Dialog.Result;
			Dialog.Dispose();
			return Result;
		}

		// members
		private OpenBve.formMain.MainDialogResult Result = new MainDialogResult();
		private int[] EncodingCodepages = new int[0];
		private Image JoystickImage = null;
		private string[] LanguageFiles = new string[0];
		private string CurrentLanguageCode = "en-US";

		// ====
		// form
		// ====

		// load
		private void formMain_Load(object sender, EventArgs e) {
			this.MinimumSize = this.Size;
			if (Interface.CurrentOptions.MainMenuWidth == -1 & Interface.CurrentOptions.MainMenuHeight == -1) {
				this.WindowState = FormWindowState.Maximized;
			} else if (Interface.CurrentOptions.MainMenuWidth > 0 & Interface.CurrentOptions.MainMenuHeight > 0) {
				this.Size = new Size(Interface.CurrentOptions.MainMenuWidth, Interface.CurrentOptions.MainMenuHeight);
				this.CenterToScreen();
			}
			if (Program.IsDevelopmentVersion) {
				labelVersion.Text = "   v" + Application.ProductVersion + " (development)";
				labelVersion.Location = new Point(0, labelInfoTop.Bottom);
				labelVersion.Width = panelInfo.Width;
				labelVersion.BackColor = Color.Firebrick;
				labelInfoCenter.Location = new Point(0, labelVersion.Bottom);
				labelInfoCenter.Visible = true;
			} else {
				labelVersion.Text = "v" + Application.ProductVersion;
			}
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// form icon
			try {
				string File = Interface.GetCombinedFileName(Interface.GetDataFolder(), "icon.ico");
				this.Icon = new Icon(File);
			} catch { }
			// use button-style radio buttons on non-Mono
			if (!Program.CurrentlyRunOnMono) {
				radiobuttonStart.Appearance = Appearance.Button;
				radiobuttonStart.AutoSize = false;
				radiobuttonStart.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonStart.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonReview.Appearance = Appearance.Button;
				radiobuttonReview.AutoSize = false;
				radiobuttonReview.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonReview.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonControls.Appearance = Appearance.Button;
				radiobuttonControls.AutoSize = false;
				radiobuttonControls.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonControls.TextAlign = ContentAlignment.MiddleCenter;
				radiobuttonOptions.Appearance = Appearance.Button;
				radiobuttonOptions.AutoSize = false;
				radiobuttonOptions.Size = new Size(buttonClose.Width, buttonClose.Height);
				radiobuttonOptions.TextAlign = ContentAlignment.MiddleCenter;
			}
			// options
			Interface.LoadLogs();
			ListLanguages();
			{
				int Tab = 0;
				string[] Args = System.Environment.GetCommandLineArgs();
				for (int i = 1; i < Args.Length; i++) {
					switch (Args[i].ToLowerInvariant()) {
							case "/newgame": Tab = 0; break;
							case "/review": Tab = 1; break;
							case "/controls": Tab = 2; break;
							case "/options": Tab = 3; break;
					}
				}
				switch (Tab) {
						case 1: radiobuttonReview.Checked = true; break;
						case 2: radiobuttonControls.Checked = true; break;
						case 3: radiobuttonOptions.Checked = true; break;
						default: radiobuttonStart.Checked = true; break;
				}
			}
			// icons and images
			string MenuFolder = Interface.GetDataFolder("Menu");
			Image ParentIcon = LoadImage(MenuFolder, "icon_parent.png");
			Image FolderIcon = LoadImage(MenuFolder, "icon_folder.png");
			Image RouteIcon = LoadImage(MenuFolder, "icon_route.png");
			Image TrainIcon = LoadImage(MenuFolder, "icon_train.png");
			Image KeyboardIcon = LoadImage(MenuFolder, "icon_keyboard.png");
			Image MouseIcon = LoadImage(MenuFolder, "icon_mouse.png");
			Image JoystickIcon = LoadImage(MenuFolder, "icon_joystick.png");
			Image GamepadIcon = LoadImage(MenuFolder, "icon_gamepad.png");
			JoystickImage = LoadImage(MenuFolder, "joystick.png");
			{
				Image Logo = LoadImage(MenuFolder, "logo.png");
				if (Logo != null) pictureboxLogo.Image = Logo;
			}
			// route selection
			listviewRouteFiles.SmallImageList = new ImageList();
			listviewRouteFiles.SmallImageList.TransparentColor = Color.White;
			if (ParentIcon != null) listviewRouteFiles.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewRouteFiles.SmallImageList.Images.Add("folder", FolderIcon);
			if (RouteIcon != null) listviewRouteFiles.SmallImageList.Images.Add("route", RouteIcon);
			listviewRouteFiles.Columns.Clear();
			listviewRouteFiles.Columns.Add("");
			listviewRouteRecently.Items.Clear();
			listviewRouteRecently.Columns.Add("");
			listviewRouteRecently.SmallImageList = new ImageList();
			listviewRouteRecently.SmallImageList.TransparentColor = Color.White;
			if (RouteIcon != null) listviewRouteRecently.SmallImageList.Images.Add("route", RouteIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
				ListViewItem Item = listviewRouteRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedRoutes[i]));
				Item.ImageKey = "route";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedRoutes[i];
			}
			listviewRouteRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// train selection
			listviewTrainFolders.SmallImageList = new ImageList();
			listviewTrainFolders.SmallImageList.TransparentColor = Color.White;
			if (ParentIcon != null) listviewTrainFolders.SmallImageList.Images.Add("parent", ParentIcon);
			if (FolderIcon != null) listviewTrainFolders.SmallImageList.Images.Add("folder", FolderIcon);
			if (TrainIcon != null) listviewTrainFolders.SmallImageList.Images.Add("train", TrainIcon);
			listviewTrainFolders.Columns.Clear();
			listviewTrainFolders.Columns.Add("");
			listviewTrainRecently.Columns.Clear();
			listviewTrainRecently.Columns.Add("");
			listviewTrainRecently.SmallImageList = new ImageList();
			listviewTrainRecently.SmallImageList.TransparentColor = Color.White;
			if (TrainIcon != null) listviewTrainRecently.SmallImageList.Images.Add("train", TrainIcon);
			for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
				ListViewItem Item = listviewTrainRecently.Items.Add(System.IO.Path.GetFileName(Interface.CurrentOptions.RecentlyUsedTrains[i]));
				Item.ImageKey = "train";
				Item.Tag = Interface.CurrentOptions.RecentlyUsedTrains[i];
			}
			listviewTrainRecently.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			// text boxes
			if (Interface.CurrentOptions.RouteFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.RouteFolder)) {
				textboxRouteFolder.Text = Interface.CurrentOptions.RouteFolder;
			} else {
				textboxRouteFolder.Text = Interface.GetPersonalFolder();
			}
			if (Interface.CurrentOptions.TrainFolder.Length != 0 && System.IO.Directory.Exists(Interface.CurrentOptions.TrainFolder)) {
				textboxTrainFolder.Text = Interface.CurrentOptions.TrainFolder;
			} else {
				textboxTrainFolder.Text = Interface.GetPersonalFolder();
			}
			// encodings
			{
				System.Text.EncodingInfo[] Info = System.Text.Encoding.GetEncodings();
				EncodingCodepages = new int[Info.Length + 1];
				string[] EncodingDescriptions = new string[Info.Length + 1];
				EncodingCodepages[0] = System.Text.Encoding.UTF8.CodePage;
				EncodingDescriptions[0] = "(UTF-8)";
				for (int i = 0; i < Info.Length; i++) {
					EncodingCodepages[i + 1] = Info[i].CodePage;
					EncodingDescriptions[i + 1] = Info[i].DisplayName + " - " + Info[i].CodePage.ToString(Culture);
				}
				Array.Sort<string, int>(EncodingDescriptions, EncodingCodepages, 1, Info.Length);
				comboboxRouteEncoding.Items.Clear();
				comboboxTrainEncoding.Items.Clear();
				for (int i = 0; i < Info.Length + 1; i++) {
					comboboxRouteEncoding.Items.Add(EncodingDescriptions[i]);
					comboboxTrainEncoding.Items.Add(EncodingDescriptions[i]);
				}
			}
			// modes
			comboboxMode.Items.Clear();
			comboboxMode.Items.AddRange(new string[] { "", "", "" });
			comboboxMode.SelectedIndex = Interface.CurrentOptions.GameMode == Interface.GameMode.Arcade ? 0 : Interface.CurrentOptions.GameMode == Interface.GameMode.Expert ? 2 : 1;
			// review last game
			{
				if (Game.LogRouteName.Length == 0 | Game.LogTrainName.Length == 0) {
					radiobuttonReview.Enabled = false;
				} else {
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
					if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
					labelReviewRouteValue.Text = Game.LogRouteName;
					labelReviewTrainValue.Text = Game.LogTrainName;
					labelReviewDateValue.Text = Game.LogDateTime.ToString("yyyy-MM-dd", Culture);
					labelReviewTimeValue.Text = Game.LogDateTime.ToString("HH:mm:ss", Culture);
					switch (Interface.CurrentOptions.GameMode) {
							case Interface.GameMode.Arcade: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_arcade"); break;
							case Interface.GameMode.Normal: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_normal"); break;
							case Interface.GameMode.Expert: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_expert"); break;
							default: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_unkown"); break;
					}
					if (Game.CurrentScore.Maximum == 0) {
						labelRatingColor.BackColor = Color.Gray;
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_unkown");
					} else {
						Color[] Colors = new Color[] { Color.PaleVioletRed, Color.IndianRed, Color.Peru, Color.Goldenrod, Color.DarkKhaki, Color.YellowGreen, Color.MediumSeaGreen, Color.MediumAquamarine, Color.SkyBlue, Color.CornflowerBlue };
						if (index >= 0 & index < Colors.Length) {
							labelRatingColor.BackColor = Colors[index];
						} else {
							labelRatingColor.BackColor = Color.Gray;
						}
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_" + index.ToString(Culture));
					}
					labelRatingAchievedValue.Text = Game.CurrentScore.Value.ToString(Culture);
					labelRatingMaximumValue.Text = Game.CurrentScore.Maximum.ToString(Culture);
					labelRatingRatioValue.Text = (100.0 * ratio).ToString("0.00", Culture) + "%";
				}
			}
			comboboxBlackBoxFormat.Items.Clear();
			comboboxBlackBoxFormat.Items.AddRange(new string[] { "", "" });
			comboboxBlackBoxFormat.SelectedIndex = 1;
			if (Game.BlackBoxEntryCount == 0) {
				labelBlackBox.Enabled = false;
				labelBlackBoxFormat.Enabled = false;
				comboboxBlackBoxFormat.Enabled = false;
				buttonBlackBoxExport.Enabled = false;
			}
			// controls
			listviewControls.SmallImageList = new ImageList();
			listviewControls.SmallImageList.TransparentColor = Color.White;
			if (KeyboardIcon != null) listviewControls.SmallImageList.Images.Add("keyboard", KeyboardIcon);
			if (MouseIcon != null) listviewControls.SmallImageList.Images.Add("mouse", MouseIcon);
			if (JoystickIcon != null) listviewControls.SmallImageList.Images.Add("joystick", JoystickIcon);
			if (GamepadIcon != null) listviewControls.SmallImageList.Images.Add("gamepad", GamepadIcon);
			// options
			if (Interface.CurrentOptions.FullscreenMode) {
				radiobuttonFullscreen.Checked = true;
			} else {
				radiobuttonWindow.Checked = true;
			}
			comboboxVSync.Items.Clear();
			comboboxVSync.Items.Add("");
			comboboxVSync.Items.Add("");
			comboboxVSync.SelectedIndex = Interface.CurrentOptions.VerticalSynchronization ? 1 : 0;
			updownWindowWidth.Value = (decimal)Interface.CurrentOptions.WindowWidth;
			updownWindowHeight.Value = (decimal)Interface.CurrentOptions.WindowHeight;
			updownFullscreenWidth.Value = (decimal)Interface.CurrentOptions.FullscreenWidth;
			updownFullscreenHeight.Value = (decimal)Interface.CurrentOptions.FullscreenHeight;
			comboboxFullscreenBits.Items.Clear();
			comboboxFullscreenBits.Items.Add("16");
			comboboxFullscreenBits.Items.Add("32");
			comboboxFullscreenBits.SelectedIndex = Interface.CurrentOptions.FullscreenBits == 16 ? 0 : 1;
			comboboxInterpolation.Items.Clear();
			comboboxInterpolation.Items.AddRange(new string[] { "", "", "", "", "", "" });
			if ((int)Interface.CurrentOptions.Interpolation >= 0 & (int)Interface.CurrentOptions.Interpolation < comboboxInterpolation.Items.Count) {
				comboboxInterpolation.SelectedIndex = (int)Interface.CurrentOptions.Interpolation;
			} else {
				comboboxInterpolation.SelectedIndex = 3;
			}
			if (Interface.CurrentOptions.AnisotropicFilteringMaximum <= 0) {
				labelAnisotropic.Enabled = false;
				updownAnisotropic.Enabled = false;
				updownAnisotropic.Minimum = (decimal)0;
				updownAnisotropic.Maximum = (decimal)0;
			} else {
				updownAnisotropic.Minimum = (decimal)1;
				updownAnisotropic.Maximum = (decimal)Interface.CurrentOptions.AnisotropicFilteringMaximum;
				if ((decimal)Interface.CurrentOptions.AnisotropicFilteringLevel >= updownAnisotropic.Minimum & (decimal)Interface.CurrentOptions.AnisotropicFilteringLevel <= updownAnisotropic.Maximum) {
					updownAnisotropic.Value = (decimal)Interface.CurrentOptions.AnisotropicFilteringLevel;
				} else {
					updownAnisotropic.Value = updownAnisotropic.Minimum;
				}
			}
			updownDistance.Value = (decimal)Interface.CurrentOptions.ViewingDistance;
			comboboxMotionBlur.Items.Clear();
			comboboxMotionBlur.Items.AddRange(new string[] { "", "", "", "" });
			comboboxMotionBlur.SelectedIndex = (int)Interface.CurrentOptions.MotionBlur;
			trackbarTransparency.Value = (int)Interface.CurrentOptions.TransparencyMode;
			checkboxToppling.Checked = Interface.CurrentOptions.Toppling;
			checkboxCollisions.Checked = Interface.CurrentOptions.Collisions;
			checkboxDerailments.Checked = Interface.CurrentOptions.Derailments;
			checkboxBlackBox.Checked = Interface.CurrentOptions.BlackBox;
			checkboxJoysticksUsed.Checked = Interface.CurrentOptions.UseJoysticks;
			{
				double a = (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum) * Interface.CurrentOptions.JoystickAxisThreshold + (double)trackbarJoystickAxisThreshold.Minimum;
				int b = (int)Math.Round(a);
				if (b < trackbarJoystickAxisThreshold.Minimum) b = trackbarJoystickAxisThreshold.Minimum;
				if (b > trackbarJoystickAxisThreshold.Maximum) b = trackbarJoystickAxisThreshold.Maximum;
				trackbarJoystickAxisThreshold.Value = b;
			}
			comboboxSoundRange.Items.Clear();
			comboboxSoundRange.Items.AddRange(new string[] { "", "", "" });
			if ((int)Interface.CurrentOptions.SoundRange >= 0 & (int)Interface.CurrentOptions.SoundRange < comboboxSoundRange.Items.Count) {
				comboboxSoundRange.SelectedIndex = (int)Interface.CurrentOptions.SoundRange;
			} else {
				comboboxSoundRange.SelectedIndex = 0;
			}
			updownSoundNumber.Value = (decimal)Interface.CurrentOptions.SoundNumber;
			checkboxWarningMessages.Checked = Interface.CurrentOptions.ShowWarningMessages;
			checkboxErrorMessages.Checked = Interface.CurrentOptions.ShowErrorMessages;
			// language
			{
				string Folder = Interface.GetDataFolder("Languages");
				int j;
				for (j = 0; j < LanguageFiles.Length; j++) {
					string File = Interface.GetCombinedFileName(Folder, Interface.CurrentOptions.LanguageCode + ".cfg");
					if (string.Compare(File, LanguageFiles[j], StringComparison.OrdinalIgnoreCase) == 0) {
						comboboxLanguages.SelectedIndex = j;
						break;
					}
				}
				if (j == LanguageFiles.Length) {
					#if !DEBUG
					try {
						#endif
						string File = Interface.GetCombinedFileName(Folder, "en-US.cfg");
						Interface.LoadLanguage(File);
						ApplyLanguage();
						#if !DEBUG
					} catch (Exception ex) {
						MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
					}
					#endif
				}
			}
			// lists
			ShowScoreLog(checkboxScorePenalties.Checked);
			// result
			Result.Start = false;
			Result.RouteFile = null;
			Result.RouteEncoding = System.Text.Encoding.UTF8;
			Result.TrainFolder = null;
			Result.TrainEncoding = System.Text.Encoding.UTF8;
		}

		// apply language
		private string GetSomething(string text, string fallback) {
			text = text.Trim();
			int count = 0;
			for (int i = 0; i < text.Length; i++) {
				if (char.IsLetterOrDigit(text, i)) {
					count++;
				}
				if (char.IsSurrogatePair(text, i)) {
					i++;
				}
			}
			if (count >= 5 & count >= text.Length * 3 / 4) {
				return text;
			} else {
				return fallback;
			}
		}
		private void ApplyLanguage() {
			// panel
			radiobuttonStart.Text = Interface.GetInterfaceString("panel_start");
			radiobuttonReview.Text = Interface.GetInterfaceString("panel_review");
			radiobuttonControls.Text = Interface.GetInterfaceString("panel_controls");
			radiobuttonOptions.Text = Interface.GetInterfaceString("panel_options");
			linkHomepage.Text = GetSomething(Interface.GetInterfaceString("panel_homepage"), "Visit official homepage");
			linkUpdates.Text = Interface.GetInterfaceString("panel_updates");
			buttonClose.Text = Interface.GetInterfaceString("panel_close");
			// options
			labelOptionsTitle.Text = Interface.GetInterfaceString("options_title");
			groupboxDisplayMode.Text = Interface.GetInterfaceString("options_display_mode");
			radiobuttonWindow.Text = Interface.GetInterfaceString("options_display_mode_window");
			radiobuttonFullscreen.Text = Interface.GetInterfaceString("options_display_mode_fullscreen");
			labelVSync.Text = Interface.GetInterfaceString("options_display_vsync");
			comboboxVSync.Items[0] = Interface.GetInterfaceString("options_display_vsync_off");
			comboboxVSync.Items[1] = Interface.GetInterfaceString("options_display_vsync_on");
			groupboxWindow.Text = Interface.GetInterfaceString("options_display_window");
			labelWindowWidth.Text = Interface.GetInterfaceString("options_display_window_width");
			labelWindowHeight.Text = Interface.GetInterfaceString("options_display_window_height");
			groupboxFullscreen.Text = Interface.GetInterfaceString("options_display_fullscreen");
			labelFullscreenWidth.Text = Interface.GetInterfaceString("options_display_fullscreen_width");
			labelFullscreenHeight.Text = Interface.GetInterfaceString("options_display_fullscreen_height");
			labelFullscreenBits.Text = Interface.GetInterfaceString("options_display_fullscreen_bits");
			groupboxInterpolation.Text = Interface.GetInterfaceString("options_quality_interpolation");
			labelInterpolation.Text = Interface.GetInterfaceString("options_quality_interpolation_mode");
			comboboxInterpolation.Items[0] = Interface.GetInterfaceString("options_quality_interpolation_mode_nearest");
			comboboxInterpolation.Items[1] = Interface.GetInterfaceString("options_quality_interpolation_mode_bilinear");
			comboboxInterpolation.Items[2] = Interface.GetInterfaceString("options_quality_interpolation_mode_nearestmipmap");
			comboboxInterpolation.Items[3] = Interface.GetInterfaceString("options_quality_interpolation_mode_bilinearmipmap");
			comboboxInterpolation.Items[4] = Interface.GetInterfaceString("options_quality_interpolation_mode_trilinearmipmap");
			comboboxInterpolation.Items[5] = Interface.GetInterfaceString("options_quality_interpolation_mode_anisotropic");
			labelAnisotropic.Text = Interface.GetInterfaceString("options_quality_interpolation_anisotropic_level");
			labelTransparency.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency");
			labelTransparencyPerformance.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency_sharp");
			labelTransparencyQuality.Text = Interface.GetInterfaceString("options_quality_interpolation_transparency_smooth");
			groupboxDistance.Text = Interface.GetInterfaceString("options_quality_distance");
			labelDistance.Text = Interface.GetInterfaceString("options_quality_distance_viewingdistance");
			labelDistanceUnit.Text = Interface.GetInterfaceString("options_quality_distance_viewingdistance_meters");
			labelMotionBlur.Text = "options_quality_distance_motionblur";
			comboboxMotionBlur.Items[0] = Interface.GetInterfaceString("options_quality_distance_motionblur_none");
			comboboxMotionBlur.Items[1] = Interface.GetInterfaceString("options_quality_distance_motionblur_low");
			comboboxMotionBlur.Items[2] = Interface.GetInterfaceString("options_quality_distance_motionblur_medium");
			comboboxMotionBlur.Items[3] = Interface.GetInterfaceString("options_quality_distance_motionblur_high");
			labelMotionBlur.Text = Interface.GetInterfaceString("options_quality_distance_motionblur");
			groupboxSimulation.Text = Interface.GetInterfaceString("options_misc_simulation");
			checkboxToppling.Text = Interface.GetInterfaceString("options_misc_simulation_toppling");
			checkboxCollisions.Text = Interface.GetInterfaceString("options_misc_simulation_collisions");
			checkboxDerailments.Text = Interface.GetInterfaceString("options_misc_simulation_derailments");
			checkboxBlackBox.Text = Interface.GetInterfaceString("options_misc_simulation_blackbox");
			groupboxControls.Text = Interface.GetInterfaceString("options_misc_controls");
			checkboxJoysticksUsed.Text = Interface.GetInterfaceString("options_misc_controls_joysticks");
			labelJoystickAxisThreshold.Text = Interface.GetInterfaceString("options_misc_controls_threshold");
			groupboxSound.Text = Interface.GetInterfaceString("options_misc_sound");
			labelSoundRange.Text = Interface.GetInterfaceString("options_misc_sound_range");
			comboboxSoundRange.Items[0] = Interface.GetInterfaceString("options_misc_sound_range_low");
			comboboxSoundRange.Items[1] = Interface.GetInterfaceString("options_misc_sound_range_medium");
			comboboxSoundRange.Items[2] = Interface.GetInterfaceString("options_misc_sound_range_high");
			labelSoundNumber.Text = Interface.GetInterfaceString("options_misc_sound_number");
			groupboxVerbosity.Text = Interface.GetInterfaceString("options_verbosity");
			checkboxWarningMessages.Text = Interface.GetInterfaceString("options_verbosity_warningmessages");
			checkboxErrorMessages.Text = Interface.GetInterfaceString("options_verbosity_errormessages");
			// start
			labelStartTitle.Text = Interface.GetInterfaceString("start_title");
			labelRoute.Text = "▸ " + Interface.GetInterfaceString("start_route");
			groupboxRouteSelection.Text = Interface.GetInterfaceString("start_route_selection");
			tabpageRouteBrowse.Text = Interface.GetInterfaceString("start_route_browse");
			tabpageRouteRecently.Text = Interface.GetInterfaceString("start_route_recently");
			groupboxRouteDetails.Text = Interface.GetInterfaceString("start_route_details");
			tabpageRouteDescription.Text = Interface.GetInterfaceString("start_route_description");
			tabpageRouteMap.Text = Interface.GetInterfaceString("start_route_map");
			tabpageRouteGradient.Text = Interface.GetInterfaceString("start_route_gradient");
			tabpageRouteSettings.Text = Interface.GetInterfaceString("start_route_settings");
			labelRouteEncoding.Text = Interface.GetInterfaceString("start_route_settings_encoding");
			comboboxRouteEncoding.Items[0] = Interface.GetInterfaceString("(UTF-8)");
			labelRouteEncodingPreview.Text = Interface.GetInterfaceString("start_route_settings_encoding_preview");
			labelTrain.Text = "▸ " + Interface.GetInterfaceString("start_train");
			groupboxTrainSelection.Text = Interface.GetInterfaceString("start_train_selection");
			tabpageTrainBrowse.Text = Interface.GetInterfaceString("start_train_browse");
			tabpageTrainRecently.Text = Interface.GetInterfaceString("start_train_recently");
			tabpageTrainDefault.Text = Interface.GetInterfaceString("start_train_default");
			checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
			groupboxTrainDetails.Text = Interface.GetInterfaceString("start_train_details");
			tabpageTrainDescription.Text = Interface.GetInterfaceString("start_train_description");
			tabpageTrainSettings.Text = Interface.GetInterfaceString("start_train_settings");
			labelTrainEncoding.Text = Interface.GetInterfaceString("start_train_settings_encoding");
			comboboxTrainEncoding.Items[0] = Interface.GetInterfaceString("(UTF-8)");
			labelTrainEncodingPreview.Text = Interface.GetInterfaceString("start_train_settings_encoding_preview");
			labelStart.Text = "▸ " + Interface.GetInterfaceString("start_start");
			labelMode.Text = Interface.GetInterfaceString("start_start_mode");
			buttonStart.Text = Interface.GetInterfaceString("start_start_start");
			comboboxMode.Items[0] = Interface.GetInterfaceString("mode_arcade");
			comboboxMode.Items[1] = Interface.GetInterfaceString("mode_normal");
			comboboxMode.Items[2] = Interface.GetInterfaceString("mode_expert");
			// review
			labelReviewTitle.Text = Interface.GetInterfaceString("review_title");
			labelConditions.Text = "▸ " + Interface.GetInterfaceString("review_conditions");
			groupboxReviewRoute.Text = Interface.GetInterfaceString("review_conditions_route");
			labelReviewRouteCaption.Text = Interface.GetInterfaceString("review_conditions_route_file");
			groupboxReviewTrain.Text = Interface.GetInterfaceString("review_conditions_train");
			labelReviewTrainCaption.Text = Interface.GetInterfaceString("review_conditions_train_folder");
			groupboxReviewDateTime.Text = Interface.GetInterfaceString("review_conditions_datetime");
			labelReviewDateCaption.Text = Interface.GetInterfaceString("review_conditions_datetime_date");
			labelReviewTimeCaption.Text = Interface.GetInterfaceString("review_conditions_datetime_time");
			labelScore.Text = "▸ " + Interface.GetInterfaceString("review_score");
			groupboxRating.Text = Interface.GetInterfaceString("review_score_rating");
			labelRatingModeCaption.Text = Interface.GetInterfaceString("review_score_rating_mode");
			switch (Interface.CurrentOptions.GameMode) {
					case Interface.GameMode.Arcade: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_arcade"); break;
					case Interface.GameMode.Normal: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_normal"); break;
					case Interface.GameMode.Expert: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_expert"); break;
					default: labelRatingModeValue.Text = Interface.GetInterfaceString("mode_unkown"); break;
			}
			{
					double ratio = Game.CurrentScore.Maximum == 0 ? 0.0 : (double)Game.CurrentScore.Value / (double)Game.CurrentScore.Maximum;
					if (ratio < 0.0) ratio = 0.0;
					if (ratio > 1.0) ratio = 1.0;
					int index = (int)Math.Floor(ratio * (double)Interface.RatingsCount);
					if (index >= Interface.RatingsCount) index = Interface.RatingsCount - 1;
					if (Game.CurrentScore.Maximum == 0) {
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_unkown");
					} else {
						labelRatingDescription.Text = Interface.GetInterfaceString("rating_" + index.ToString(System.Globalization.CultureInfo.InvariantCulture));
					}
			}
			labelRatingAchievedCaption.Text = Interface.GetInterfaceString("review_score_rating_achieved");
			labelRatingMaximumCaption.Text = Interface.GetInterfaceString("review_score_rating_maximum");
			labelRatingRatioCaption.Text = Interface.GetInterfaceString("review_score_rating_ratio");
			groupboxScore.Text = Interface.GetInterfaceString("review_score_log");
			listviewScore.Columns[0].Text = Interface.GetInterfaceString("review_score_log_list_time");
			listviewScore.Columns[1].Text = Interface.GetInterfaceString("review_score_log_list_position");
			listviewScore.Columns[2].Text = Interface.GetInterfaceString("review_score_log_list_value");
			listviewScore.Columns[3].Text = Interface.GetInterfaceString("review_score_log_list_cumulative");
			listviewScore.Columns[4].Text = Interface.GetInterfaceString("review_score_log_list_reason");
			ShowScoreLog(checkboxScorePenalties.Checked);
			checkboxScorePenalties.Text = Interface.GetInterfaceString("review_score_log_penalties");
			buttonScoreExport.Text = Interface.GetInterfaceString("review_score_log_export");
			labelBlackBox.Text = "▸ " + Interface.GetInterfaceString("review_blackbox");
			labelBlackBoxFormat.Text = Interface.GetInterfaceString("review_blackbox_format");
			comboboxBlackBoxFormat.Items[0] = Interface.GetInterfaceString("review_blackbox_format_csv");
			comboboxBlackBoxFormat.Items[1] = Interface.GetInterfaceString("review_blackbox_format_text");
			buttonBlackBoxExport.Text = Interface.GetInterfaceString("review_blackbox_export");
			// controls
			for (int i = 0; i < listviewControls.SelectedItems.Count; i++) {
				listviewControls.SelectedItems[i].Selected = false;
			}
			labelControlsTitle.Text = Interface.GetInterfaceString("controls_title");
			listviewControls.Columns[0].Text = Interface.GetInterfaceString("controls_list_command");
			listviewControls.Columns[1].Text = Interface.GetInterfaceString("controls_list_type");
			listviewControls.Columns[2].Text = Interface.GetInterfaceString("controls_list_description");
			listviewControls.Columns[3].Text = Interface.GetInterfaceString("controls_list_assignment");
			buttonControlAdd.Text = Interface.GetInterfaceString("controls_add");
			buttonControlRemove.Text = Interface.GetInterfaceString("controls_remove");
			buttonControlsImport.Text = Interface.GetInterfaceString("controls_import");
			buttonControlsExport.Text = Interface.GetInterfaceString("controls_export");
			buttonControlUp.Text = Interface.GetInterfaceString("controls_up");
			buttonControlDown.Text = Interface.GetInterfaceString("controls_down");
			groupboxControl.Text = Interface.GetInterfaceString("controls_selection");
			labelCommand.Text = Interface.GetInterfaceString("controls_selection_command");
			radiobuttonKeyboard.Text = Interface.GetInterfaceString("controls_selection_keyboard");
			labelKeyboardKey.Text = Interface.GetInterfaceString("controls_selection_keyboard_key");
			labelKeyboardModifier.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers");
			checkboxKeyboardShift.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_shift");
			checkboxKeyboardCtrl.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_ctrl");
			checkboxKeyboardAlt.Text = Interface.GetInterfaceString("controls_selection_keyboard_modifiers_alt");
			radiobuttonJoystick.Text = Interface.GetInterfaceString("controls_selection_joystick");
			labelJoystickAssignmentCaption.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment");
			textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab");
			groupboxJoysticks.Text = Interface.GetInterfaceString("controls_attached");
			{
				listviewControls.Items.Clear();
				comboboxCommand.Items.Clear();
				for (int i = 0; i < Interface.CommandInfos.Length; i++) {
					comboboxCommand.Items.Add(Interface.CommandInfos[i].Name + " - " + Interface.CommandInfos[i].Description);
				}
				comboboxKeyboardKey.Items.Clear();
				for (int i = 0; i < Interface.Keys.Length; i++) {
					comboboxKeyboardKey.Items.Add(Interface.Keys[i].Description);
				}
				ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
				for (int i = 0; i < Interface.CurrentControls.Length; i++) {
					Items[i] = new ListViewItem(new string[] { "", "", "", "" });
					UpdateControlListElement(Items[i], i, false);
				}
				listviewControls.Items.AddRange(Items);
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		// form closing
		private void formMain_FormClosing(object sender, FormClosingEventArgs e) {
			Interface.CurrentOptions.LanguageCode = CurrentLanguageCode;
			Interface.CurrentOptions.FullscreenMode = radiobuttonFullscreen.Checked;
			Interface.CurrentOptions.VerticalSynchronization = comboboxVSync.SelectedIndex == 1;
			Interface.CurrentOptions.WindowWidth = (int)Math.Round(updownWindowWidth.Value);
			Interface.CurrentOptions.WindowHeight = (int)Math.Round(updownWindowHeight.Value);
			Interface.CurrentOptions.FullscreenWidth = (int)Math.Round(updownFullscreenWidth.Value);
			Interface.CurrentOptions.FullscreenHeight = (int)Math.Round(updownFullscreenHeight.Value);
			Interface.CurrentOptions.FullscreenBits = comboboxFullscreenBits.SelectedIndex == 0 ? 16 : 32;
			Interface.CurrentOptions.Interpolation = (TextureManager.InterpolationMode)comboboxInterpolation.SelectedIndex;
			Interface.CurrentOptions.AnisotropicFilteringLevel = (int)Math.Round(updownAnisotropic.Value);
			Interface.CurrentOptions.TransparencyMode = (Renderer.TransparencyMode)trackbarTransparency.Value;
			Interface.CurrentOptions.ViewingDistance = (int)Math.Round(updownDistance.Value);
			Interface.CurrentOptions.MotionBlur = (Interface.MotionBlurMode)comboboxMotionBlur.SelectedIndex;
			Interface.CurrentOptions.Toppling = checkboxToppling.Checked;
			Interface.CurrentOptions.Collisions = checkboxCollisions.Checked;
			Interface.CurrentOptions.Derailments = checkboxDerailments.Checked;
			Interface.CurrentOptions.GameMode = (Interface.GameMode)comboboxMode.SelectedIndex;
			Interface.CurrentOptions.BlackBox = checkboxBlackBox.Checked;
			Interface.CurrentOptions.UseJoysticks = checkboxJoysticksUsed.Checked;
			Interface.CurrentOptions.JoystickAxisThreshold = ((double)trackbarJoystickAxisThreshold.Value - (double)trackbarJoystickAxisThreshold.Minimum) / (double)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			Interface.CurrentOptions.SoundRange = (Interface.SoundRange)comboboxSoundRange.SelectedIndex;
			Interface.CurrentOptions.SoundNumber = (int)Math.Round(updownSoundNumber.Value);
			Interface.CurrentOptions.ShowWarningMessages = checkboxWarningMessages.Checked;
			Interface.CurrentOptions.ShowErrorMessages = checkboxErrorMessages.Checked;
			Interface.CurrentOptions.RouteFolder = textboxRouteFolder.Text;
			Interface.CurrentOptions.TrainFolder = textboxTrainFolder.Text;
			Interface.CurrentOptions.MainMenuWidth = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Width;
			Interface.CurrentOptions.MainMenuHeight = this.WindowState == FormWindowState.Maximized ? -1 : this.Size.Height;
			if (Result.Start) {
				// recently used routes
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0) {
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
						if (string.Compare(Result.RouteFile, Interface.CurrentOptions.RecentlyUsedRoutes[i], StringComparison.OrdinalIgnoreCase) == 0) {
							break;
						}
					} if (i == Interface.CurrentOptions.RecentlyUsedRoutes.Length) {
						if (Interface.CurrentOptions.RecentlyUsedRoutes.Length < Interface.CurrentOptions.RecentlyUsedLimit) {
							Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedRoutes, i + 1);
						} else {
							i--;
						}
					}
					for (int j = i; j > 0; j--) {
						Interface.CurrentOptions.RecentlyUsedRoutes[j] = Interface.CurrentOptions.RecentlyUsedRoutes[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedRoutes[0] = Result.RouteFile;
				}
				// recently used trains
				if (Interface.CurrentOptions.RecentlyUsedLimit > 0) {
					int i; for (i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
						if (string.Compare(Result.TrainFolder, Interface.CurrentOptions.RecentlyUsedTrains[i], StringComparison.OrdinalIgnoreCase) == 0) {
							break;
						}
					} if (i == Interface.CurrentOptions.RecentlyUsedTrains.Length) {
						if (Interface.CurrentOptions.RecentlyUsedTrains.Length < Interface.CurrentOptions.RecentlyUsedLimit) {
							Array.Resize<string>(ref Interface.CurrentOptions.RecentlyUsedTrains, i + 1);
						} else {
							i--;
						}
					}
					for (int j = i; j > 0; j--) {
						Interface.CurrentOptions.RecentlyUsedTrains[j] = Interface.CurrentOptions.RecentlyUsedTrains[j - 1];
					}
					Interface.CurrentOptions.RecentlyUsedTrains[0] = Result.TrainFolder;
				}
			}
			// remove non-existing recently used routes
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedRoutes.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedRoutes.Length; i++) {
					if (System.IO.File.Exists(Interface.CurrentOptions.RecentlyUsedRoutes[i])) {
						a[n] = Interface.CurrentOptions.RecentlyUsedRoutes[i];
						n++;
					}
				}
				Array.Resize<string>(ref a, n);
				Interface.CurrentOptions.RecentlyUsedRoutes = a;
			}
			// remove non-existing recently used trains
			{
				int n = 0;
				string[] a = new string[Interface.CurrentOptions.RecentlyUsedTrains.Length];
				for (int i = 0; i < Interface.CurrentOptions.RecentlyUsedTrains.Length; i++) {
					if (System.IO.Directory.Exists(Interface.CurrentOptions.RecentlyUsedTrains[i])) {
						a[n] = Interface.CurrentOptions.RecentlyUsedTrains[i];
						n++;
					}
				}
				Array.Resize<string>(ref a, n);
				Interface.CurrentOptions.RecentlyUsedTrains = a;
			}
			// remove non-existing route encoding mappings
			{
				int n = 0;
				Interface.EncodingValue[] a = new Interface.EncodingValue[Interface.CurrentOptions.RouteEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++) {
					if (System.IO.File.Exists(Interface.CurrentOptions.RouteEncodings[i].Value)) {
						a[n] = Interface.CurrentOptions.RouteEncodings[i];
						n++;
					}
				}
				Array.Resize<Interface.EncodingValue>(ref a, n);
				Interface.CurrentOptions.RouteEncodings = a;
			}
			// remove non-existing train encoding mappings
			{
				int n = 0;
				Interface.EncodingValue[] a = new Interface.EncodingValue[Interface.CurrentOptions.TrainEncodings.Length];
				for (int i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++) {
					if (System.IO.Directory.Exists(Interface.CurrentOptions.TrainEncodings[i].Value)) {
						a[n] = Interface.CurrentOptions.TrainEncodings[i];
						n++;
					}
				}
				Array.Resize<Interface.EncodingValue>(ref a, n);
				Interface.CurrentOptions.TrainEncodings = a;
			}
			// finish
			#if !DEBUG
			try {
				#endif
				Interface.SaveOptions();
				#if !DEBUG
			} catch { }
			#endif
			#if !DEBUG
			try {
				#endif
				Interface.SaveControls(null);
				#if !DEBUG
			} catch { }
			#endif
		}

		// resize
		private void formMain_Resize(object sender, EventArgs e) {
			try {
				int wt = panelStart.Width;
				int ox = labelStart.Left;
				int wa = (wt - 3 * ox) / 2;
				int wb = (wt - 3 * ox) / 2;
				groupboxRouteSelection.Width = wa;
				groupboxRouteDetails.Left = 2 * ox + wa;
				groupboxRouteDetails.Width = wb;
				groupboxTrainSelection.Width = wa;
				groupboxTrainDetails.Left = 2 * ox + wa;
				groupboxTrainDetails.Width = wb;
				int oy = (labelRoute.Top - labelStartTitleBackground.Height) / 2;
				int ht = (labelStart.Top - labelRoute.Top - 4 * oy) / 2 - labelRoute.Height - oy;
				groupboxRouteSelection.Height = ht;
				groupboxRouteDetails.Height = ht;
				labelTrain.Top = groupboxRouteSelection.Top + groupboxRouteSelection.Height + 2 * oy;
				groupboxTrainSelection.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainDetails.Top = labelTrain.Top + labelTrain.Height + oy;
				groupboxTrainSelection.Height = ht;
				groupboxTrainDetails.Height = ht;
				tabcontrolRouteSelection.Width = groupboxRouteSelection.Width - 2 * tabcontrolRouteSelection.Left;
				tabcontrolRouteSelection.Height = groupboxRouteSelection.Height - 3 * tabcontrolRouteSelection.Top / 2;
				tabcontrolRouteDetails.Width = groupboxRouteDetails.Width - 2 * tabcontrolRouteDetails.Left;
				tabcontrolRouteDetails.Height = groupboxRouteDetails.Height - 3 * tabcontrolRouteDetails.Top / 2;
				tabcontrolTrainSelection.Width = groupboxTrainSelection.Width - 2 * tabcontrolTrainSelection.Left;
				tabcontrolTrainSelection.Height = groupboxTrainSelection.Height - 3 * tabcontrolTrainSelection.Top / 2;
				tabcontrolTrainDetails.Width = groupboxTrainDetails.Width - 2 * tabcontrolTrainDetails.Left;
				tabcontrolTrainDetails.Height = groupboxTrainDetails.Height - 3 * tabcontrolTrainDetails.Top / 2;
			} catch { }
			try {
				int width = Math.Min((panelOptions.Width - 24) / 2, 420);
				panelOptionsLeft.Width = width;
				panelOptionsRight.Left = panelOptionsLeft.Left + width + 8;
				panelOptionsRight.Width = width;
			} catch { }
			try {
				int width = Math.Min((panelReview.Width - 32) / 3, 360);
				groupboxReviewRoute.Width = width;
				groupboxReviewTrain.Left = groupboxReviewRoute.Left + width + 8;
				groupboxReviewTrain.Width = width;
				groupboxReviewDateTime.Left = groupboxReviewTrain.Left + width + 8;
				groupboxReviewDateTime.Width = width;
			} catch { }
		}

		// shown
		private void formMain_Shown(object sender, EventArgs e) {
			if (radiobuttonStart.Checked) {
				listviewRouteFiles.Focus();
			} else if (radiobuttonReview.Checked) {
				listviewScore.Focus();
			} else if (radiobuttonControls.Checked) {
				listviewControls.Focus();
			} else if (radiobuttonOptions.Checked) {
				comboboxLanguages.Focus();
			}
			formMain_Resize(null, null);
			if (this.WindowState != FormWindowState.Maximized) {
				Size sss = this.ClientRectangle.Size ;
				Screen s = Screen.FromControl(this);
				if ((double)this.Width >= 0.95 * (double)s.WorkingArea.Width | (double)this.Height >= 0.95 * (double)s.WorkingArea.Height) {
					this.WindowState = FormWindowState.Maximized;
				}
			}
			// command line arguments
			if (Result.TrainFolder != null) {
				if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
				ShowTrain(false);
			}
			if (Result.RouteFile != null) {
				ShowRoute(false);
			}
		}

		// list languages
		private void ListLanguages() {
			string Folder = Interface.GetDataFolder("Languages");
			if (System.IO.Directory.Exists(Folder)) {
				string[] Files = System.IO.Directory.GetFiles(Folder);
				string[] LanguageNames = new string[Files.Length];
				LanguageFiles = new string[Files.Length];
				int n = 0;
				for (int i = 0; i < Files.Length; i++) {
					string Title = System.IO.Path.GetFileName(Files[i]);
					if (Title.EndsWith(".cfg", StringComparison.OrdinalIgnoreCase)) {
						string Code = Title.Substring(0, Title.Length - 4);
						string[] Lines = System.IO.File.ReadAllLines(Files[i], System.Text.Encoding.UTF8);
						string Section = "";
						string Name = Code;
						for (int j = 0; j < Lines.Length; j++) {
							Lines[j] = Lines[j].Trim();
							if (Lines[j].StartsWith("[", StringComparison.Ordinal) & Lines[j].EndsWith("]", StringComparison.Ordinal)) {
								Section = Lines[j].Substring(1, Lines[j].Length - 2).Trim().ToLowerInvariant();
							} else if (!Lines[j].StartsWith(";", StringComparison.OrdinalIgnoreCase)) {
								int k = Lines[j].IndexOf('=');
								if (k >= 0) {
									string Key = Lines[j].Substring(0, k).TrimEnd().ToLowerInvariant();
									string Value = Lines[j].Substring(k + 1).TrimStart();
									if (Section == "language" & Key == "name") {
										Name = Value;
										break;
									}
								}
							}
						}
						LanguageFiles[n] = Files[i];
						LanguageNames[n] = Name;
						n++;
					}
				}
				Array.Resize<string>(ref LanguageFiles, n);
				Array.Resize<string>(ref LanguageNames, n);
				Array.Sort<string, string>(LanguageNames, LanguageFiles);
				comboboxLanguages.Items.Clear();
				for (int i = 0; i < n; i++) {
					comboboxLanguages.Items.Add(LanguageNames[i]);
				}
			} else {
				LanguageFiles = new string[] { };
				comboboxLanguages.Items.Clear();
			}
		}

		// ========
		// top page
		// ========

		// page selection
		private void radiobuttonStart_CheckedChanged(object sender, EventArgs e) {
			panelStart.Visible = true;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelPanels.BackColor = labelStartTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonHighlight;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonReview_CheckedChanged(object sender, EventArgs e) {
			panelReview.Visible = true;
			panelStart.Visible = false;
			panelControls.Visible = false;
			panelOptions.Visible = false;
			panelPanels.BackColor = labelReviewTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonHighlight;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonControls_CheckedChanged(object sender, EventArgs e) {
			panelControls.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelOptions.Visible = false;
			panelPanels.BackColor = labelControlsTitle.BackColor;
			pictureboxJoysticks.Visible = true;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonHighlight;
			radiobuttonOptions.BackColor = SystemColors.ButtonFace;
			UpdateRadioButtonBackColor();
		}
		private void radiobuttonOptions_CheckedChanged(object sender, EventArgs e) {
			panelOptions.Visible = true;
			panelStart.Visible = false;
			panelReview.Visible = false;
			panelControls.Visible = false;
			panelPanels.BackColor = labelOptionsTitle.BackColor;
			pictureboxJoysticks.Visible = false;
			radiobuttonStart.BackColor = SystemColors.ButtonFace;
			radiobuttonReview.BackColor = SystemColors.ButtonFace;
			radiobuttonControls.BackColor = SystemColors.ButtonFace;
			radiobuttonOptions.BackColor = SystemColors.ButtonHighlight;
			UpdateRadioButtonBackColor();
		}
		private void UpdateRadioButtonBackColor() {
			// work-around for button-style radio buttons on Mono
			if (Program.CurrentlyRunOnMono) {
				radiobuttonStart.BackColor = panelPanels.BackColor;
				radiobuttonReview.BackColor = panelPanels.BackColor;
				radiobuttonControls.BackColor = panelPanels.BackColor;
				radiobuttonOptions.BackColor = panelPanels.BackColor;
			}
		}

		// homepage
		private void linkHomepage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			const string Url = "http://openbve.trainsimcentral.co.uk";
			try {
				System.Diagnostics.Process.Start(Url);
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		// updates
		private static bool CurrentlyCheckingForUpdates = false;
		private void linkUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			if (CurrentlyCheckingForUpdates) {
				return;
			}
			const string Url = "http://openbve.trainsimcentral.co.uk/common/version.txt";
			CurrentlyCheckingForUpdates = true;
			this.Cursor = Cursors.WaitCursor;
			Application.DoEvents();
			try {
				// download information
				byte[] Data = new byte[4096]; int Length = 0;
				using (System.Net.WebClient Client = new System.Net.WebClient()) {
					using (System.IO.Stream Stream = Client.OpenRead(Url)) {
						while (true) {
							if (Length + 256 >= Data.Length) Array.Resize<byte>(ref Data, Data.Length << 1);
							int n = Stream.Read(Data, Length, 256);
							if (n != 0) {
								Length += n;
							} else break;
						}
						Stream.Close();
					}
				}
				Array.Resize<byte>(ref Data, Length);
				// parse information
				System.Text.Encoding Encoding = new System.Text.UTF8Encoding();
				string Text = new string(Encoding.GetChars(Data));
				if (Text.Length != 0 && Text[0] == '\uFEFF') Text = Text.Substring(1);
				string[] Lines = Text.Split(new char[] { '\r', '\n' });
				if (Lines.Length == 0 || !Lines[0].Equals("$OpenBveVersionInformation", StringComparison.OrdinalIgnoreCase)) {
					MessageBox.Show(Interface.GetInterfaceString("panel_updates_invalid"), Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				} else {
					string StableVersion = "0.0.0.0";
					string StableDate = "0000-00-00";
					string DevelopmentVersion = "0.0.0.0";
					string DevelopmentDate = "0000-00-00";
					int i; for (i = 1; i < Lines.Length; i++) {
						if (Lines[i].Equals("----")) break;
						int h = Lines[i].IndexOf('=');
						if (h >= 0) {
							string a = Lines[i].Substring(0, h).TrimEnd();
							string b = Lines[i].Substring(h + 1).TrimStart();
							if (a.Equals("version", StringComparison.OrdinalIgnoreCase)) {
								StableVersion = b;
							} else if (a.Equals("date", StringComparison.OrdinalIgnoreCase)) {
								StableDate = b;
							} else if (a.Equals("developmentversion", StringComparison.OrdinalIgnoreCase)) {
								DevelopmentVersion = b;
							} else if (a.Equals("developmentdate", StringComparison.OrdinalIgnoreCase)) {
								DevelopmentDate = b;
							}
						}
					}
					StringBuilder StableText = new StringBuilder();
					StringBuilder DevelopmentText = new StringBuilder();
					int j; for (j = i + 1; j < Lines.Length; j++) {
						if (Lines[j].Equals("----")) break;
						StableText.AppendLine(Lines[j]);
					}
					for (int k = j + 1; k < Lines.Length; k++) {
						if (Lines[k].Equals("----")) break;
						DevelopmentText.AppendLine(Lines[k]);
					}
					bool Found = false;
					if (IsNewVersionHigher(Application.ProductVersion, StableVersion)) {
						string Message = Interface.GetInterfaceString("panel_updates_new") + StableText.ToString().Trim();
						Message = Message.Replace("[version]", StableVersion);
						Message = Message.Replace("[date]", StableDate);
						MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
						Found = true;
					}
					if (Program.IsDevelopmentVersion) {
						if (IsNewVersionHigher(Application.ProductVersion, DevelopmentVersion)) {
							string Message = Interface.GetInterfaceString("panel_updates_new") + DevelopmentText.ToString().Trim();
							Message = Message.Replace("[version]", DevelopmentVersion);
							Message = Message.Replace("[date]", DevelopmentDate);
							MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
							Found = true;
						}
					}
					if (!Found) {
						string Message = Interface.GetInterfaceString("panel_updates_old");
						MessageBox.Show(Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			this.Cursor = Cursors.Default;
			CurrentlyCheckingForUpdates = false;
		}
		private bool IsNewVersionHigher(string Current, string New) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string[] a = Current.Split('.');
			string[] b = New.Split('.');
			if (a.Length < b.Length) {
				Array.Resize<string>(ref a, b.Length);
			} else if (a.Length > b.Length) {
				Array.Resize<string>(ref b, a.Length);
			}
			for (int i = 0; i < a.Length; i++) {
				int x = a[i] != null ? int.Parse(a[i], Culture) : 0;
				int y = b[i] != null ? int.Parse(b[i], Culture) : 0;
				if (x < y) return true;
				if (x > y) return false;
			}
			return false;
		}

		// close
		private void buttonClose_Click(object sender, EventArgs e) {
			this.Close();
		}

		// ===============
		// route selection
		// ===============

		// route folder
		private void textboxRouteFolder_TextChanged(object sender, EventArgs e) {
			string Folder = textboxRouteFolder.Text;
			try {
				if (Folder.Length == 0) {
					// drives
					listviewRouteFiles.Items.Clear();
					System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();
					for (int i = 0; i < driveInfos.Length; i++) {
						ListViewItem Item = listviewRouteFiles.Items.Add(driveInfos[i].Name);
						Item.ImageKey = "folder";
						Item.Tag = driveInfos[i].RootDirectory.FullName;
						listviewRouteFiles.Tag = null;
					}
				} else if (System.IO.Directory.Exists(Folder)) {
					listviewRouteFiles.Items.Clear();
					// parent
					try {
						System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
						if (Info != null) {
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = Info.FullName;
							listviewRouteFiles.Tag = Info.FullName;
						} else {
							ListViewItem Item = listviewRouteFiles.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = "";
							listviewRouteFiles.Tag = "";
						}
					} catch { }
					// folders
					try {
						string[] Folders = System.IO.Directory.GetDirectories(Folder);
						Array.Sort<string>(Folders);
						for (int i = 0; i < Folders.Length; i++) {
							string Name = System.IO.Path.GetFileName(Folders[i]);
							if (Name.Length != 0 && Name[0] != '.') {
								ListViewItem Item = listviewRouteFiles.Items.Add(Name);
								Item.ImageKey = "folder";
								Item.Tag = Folders[i];
							}
						}
					} catch { }
					// files
					try {
						string[] Files = System.IO.Directory.GetFiles(Folder);
						Array.Sort<string>(Files);
						for (int i = 0; i < Files.Length; i++) {
							string Extension = System.IO.Path.GetExtension(Files[i]);
							switch (Extension.ToLowerInvariant()) {
								case ".rw":
								case ".csv":
									string Name = System.IO.Path.GetFileName(Files[i]);
									if (Name.Length != 0 && Name[0] != '.') {
										ListViewItem Item = listviewRouteFiles.Items.Add(Name);
										Item.ImageKey = "route";
										Item.Tag = Files[i];
									}
									break;
							}
						}
					} catch { }
				}
			} catch { }
			listviewRouteFiles.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		// route files
		private void listviewRouteFiles_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewRouteFiles.SelectedItems.Count == 1) {
				string t = listviewRouteFiles.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.File.Exists(t)) {
						Result.RouteFile = t;
						ShowRoute(false);
					}
				}
			}
		}
		private void listviewRouteFiles_DoubleClick(object sender, EventArgs e) {
			if (listviewRouteFiles.SelectedItems.Count == 1) {
				string t = listviewRouteFiles.SelectedItems[0].Tag as string;
				if (t != null) {
					if (t.Length == 0 || System.IO.Directory.Exists(t)) {
						textboxRouteFolder.Text = t;
					}
				}
			}
		}
		private void listviewRouteFiles_KeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					listviewRouteFiles_DoubleClick(null, null);
					break;
				case Keys.Back:
					string t = listviewRouteFiles.Tag as string;
					if (t != null) {
						if (t.Length == 0 || System.IO.Directory.Exists(t)) {
							textboxRouteFolder.Text = t;
						}
					} break;
			}
		}

		// route recently
		private void listviewRouteRecently_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewRouteRecently.SelectedItems.Count == 1) {
				string t = listviewRouteRecently.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.File.Exists(t)) {
						Result.RouteFile = t;
						ShowRoute(false);
					}
				}
			}
		}

		// =============
		// route details
		// =============

		// route image
		private void pictureboxRouteImage_Click(object sender, EventArgs e) {
			if (pictureboxRouteImage.Image != null) {
				formImage.ShowImageDialog(pictureboxRouteImage.Image);
			}
		}

		// route encoding
		private void comboboxRouteEncoding_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxRouteEncoding.Tag == null) {
				int i = comboboxRouteEncoding.SelectedIndex;
				if (i >= 0 & i < EncodingCodepages.Length) {
					Result.RouteEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[i]);
					if (i == 0) {
						// remove from cache
						for (int j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
							if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile) {
								Interface.CurrentOptions.RouteEncodings[j] = Interface.CurrentOptions.RouteEncodings[Interface.CurrentOptions.RouteEncodings.Length - 1];
								Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, Interface.CurrentOptions.RouteEncodings.Length - 1);
								break;
							}
						}
					} else {
						// add to cache
						int j; for (j = 0; j < Interface.CurrentOptions.RouteEncodings.Length; j++) {
							if (Interface.CurrentOptions.RouteEncodings[j].Value == Result.RouteFile) {
								Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.RouteEncodings.Length) {
							Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.RouteEncodings, j + 1);
							Interface.CurrentOptions.RouteEncodings[j].Codepage = EncodingCodepages[i];
							Interface.CurrentOptions.RouteEncodings[j].Value = Result.RouteFile;
						}
					}
					ShowRoute(true);
				}
			}
		}
		private void buttonRouteEncodingLatin1_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 1252) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonRouteEncodingShiftJis_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 932) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonRouteEncodingBig5_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 950) {
					comboboxRouteEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		// ===============
		// train selection
		// ===============

		// train folder
		private void textboxTrainFolder_TextChanged(object sender, EventArgs e) {
			string Folder = textboxTrainFolder.Text;
			try {
				if (Folder.Length == 0) {
					// drives
					listviewTrainFolders.Items.Clear();
					System.IO.DriveInfo[] driveInfos = System.IO.DriveInfo.GetDrives();
					for (int i = 0; i < driveInfos.Length; i++) {
						ListViewItem Item = listviewTrainFolders.Items.Add(driveInfos[i].Name);
						Item.ImageKey = "folder";
						Item.Tag = driveInfos[i].RootDirectory.FullName;
						listviewTrainFolders.Tag = null;
					}
				} else if (System.IO.Directory.Exists(Folder)) {
					listviewTrainFolders.Items.Clear();
					// parent
					try {
						System.IO.DirectoryInfo Info = System.IO.Directory.GetParent(Folder);
						if (Info != null) {
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = Info.FullName;
							listviewTrainFolders.Tag = Info.FullName;
						} else {
							ListViewItem Item = listviewTrainFolders.Items.Add("..");
							Item.ImageKey = "parent";
							Item.Tag = "";
							listviewTrainFolders.Tag = "";
						}
					} catch { }
					// folders
					try {
						string[] Folders = System.IO.Directory.GetDirectories(Folder);
						Array.Sort<string>(Folders);
						for (int i = 0; i < Folders.Length; i++) {
							try {
								string File = Interface.GetCombinedFileName(Folders[i], "train.dat");
								string Name = System.IO.Path.GetFileName(Folders[i]);
								if (Name.Length != 0 && Name[0] != '.') {
									ListViewItem Item = listviewTrainFolders.Items.Add(Name);
									if (System.IO.File.Exists(File)) {
										Item.ImageKey = "train";
									} else {
										Item.ImageKey = "folder";
									}
									Item.Tag = Folders[i];
								}
							} catch { }
						}
					} catch { }
				}
			} catch { }
			listviewTrainFolders.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		// train folders
		private void listviewTrainFolders_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				string t = listviewTrainFolders.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.Directory.Exists(t)) {
						string File = Interface.GetCombinedFileName(t, "train.dat");
						if (System.IO.File.Exists(File)) {
							Result.TrainFolder = t;
							ShowTrain(false);
							if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
						}
					}
				}
			}
		}
		private void listviewTrainFolders_DoubleClick(object sender, EventArgs e) {
			if (listviewTrainFolders.SelectedItems.Count == 1) {
				string t = listviewTrainFolders.SelectedItems[0].Tag as string;
				if (t != null) {
					if (t.Length == 0 || System.IO.Directory.Exists(t)) {
						textboxTrainFolder.Text = t;
					}
				}
			}
		}
		private void listviewTrainFolders_KeyDown(object sender, KeyEventArgs e) {
			switch (e.KeyCode) {
				case Keys.Return:
					listviewTrainFolders_DoubleClick(null, null);
					break;
				case Keys.Back:
					string t = listviewTrainFolders.Tag as string;
					if (t != null) {
						if (t.Length == 0 || System.IO.Directory.Exists(t)) {
							textboxTrainFolder.Text = t;
						}
					} break;
			}
		}

		// train recently
		private void listviewTrainRecently_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewTrainRecently.SelectedItems.Count == 1) {
				string t = listviewTrainRecently.SelectedItems[0].Tag as string;
				if (t != null) {
					if (System.IO.Directory.Exists(t)) {
						string File = Interface.GetCombinedFileName(t, "train.dat");
						if (System.IO.File.Exists(File)) {
							Result.TrainFolder = t;
							ShowTrain(false);
							if (checkboxTrainDefault.Checked) checkboxTrainDefault.Checked = false;
						}
					}
				}
			}
		}

		// train default
		private void checkboxTrainDefault_CheckedChanged(object sender, EventArgs e) {
			if (checkboxTrainDefault.Checked) {
				if (listviewTrainFolders.SelectedItems.Count == 1) {
					listviewTrainFolders.SelectedItems[0].Selected = false;
				}
				if (listviewTrainRecently.SelectedItems.Count == 1) {
					listviewTrainRecently.SelectedItems[0].Selected = false;
				}
				ShowDefaultTrain();
			}
		}

		// =============
		// train details
		// =============

		// train image
		private void pictureboxTrainImage_Click(object sender, EventArgs e) {
			if (pictureboxTrainImage.Image != null) {
				formImage.ShowImageDialog(pictureboxTrainImage.Image);
			}
		}

		// train encoding
		private void comboboxTrainEncoding_SelectedIndexChanged(object sender, EventArgs e) {
			if (comboboxTrainEncoding.Tag == null) {
				int i = comboboxTrainEncoding.SelectedIndex;
				if (i >= 0 & i < EncodingCodepages.Length) {
					Result.TrainEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[i]);
					if (i == 0) {
						// remove from cache
						for (int j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j] = Interface.CurrentOptions.TrainEncodings[Interface.CurrentOptions.TrainEncodings.Length - 1];
								Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, Interface.CurrentOptions.TrainEncodings.Length - 1);
								break;
							}
						}
					} else {
						// add to cache
						int j; for (j = 0; j < Interface.CurrentOptions.TrainEncodings.Length; j++) {
							if (Interface.CurrentOptions.TrainEncodings[j].Value == Result.TrainFolder) {
								Interface.CurrentOptions.TrainEncodings[j].Codepage = EncodingCodepages[i];
								break;
							}
						} if (j == Interface.CurrentOptions.TrainEncodings.Length) {
							Array.Resize<Interface.EncodingValue>(ref Interface.CurrentOptions.TrainEncodings, j + 1);
							Interface.CurrentOptions.TrainEncodings[j].Codepage = EncodingCodepages[i];
							Interface.CurrentOptions.TrainEncodings[j].Value = Result.TrainFolder;
						}
					}
					ShowTrain(true);
				}
			}
		}
		private void buttonTrainEncodingLatin1_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 1252) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonTrainEncodingShiftJis_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 932) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}
		private void buttonTrainEncodingBig5_Click(object sender, EventArgs e) {
			for (int i = 1; i < EncodingCodepages.Length; i++) {
				if (EncodingCodepages[i] == 950) {
					comboboxTrainEncoding.SelectedIndex = i;
					return;
				}
			}
			System.Media.SystemSounds.Hand.Play();
		}

		// =======
		// options
		// =======

		// language
		private void comboboxLanguages_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag != null) return;
			int i = comboboxLanguages.SelectedIndex;
			if (i >= 0 & i < LanguageFiles.Length) {
				string Code = System.IO.Path.GetFileNameWithoutExtension(LanguageFiles[i]);
				string Folder = Interface.GetDataFolder("Flags");
				#if !DEBUG
				try {
					#endif
					Interface.LoadLanguage(LanguageFiles[i]);
					#if !DEBUG
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
				#endif
				#if !DEBUG
				try {
					#endif
					string Flag = Interface.GetInterfaceString("language_flag");
					string File = Interface.GetCombinedFileName(Folder, Flag);
					if (!System.IO.File.Exists(File)) {
						File = Interface.GetCombinedFileName(Folder, "unknown.png");
					}
					if (System.IO.File.Exists(File)) {
						pictureboxLanguage.Image = Image.FromFile(File);
					} else {
						pictureboxLanguage.Image = null;
					}
					CurrentLanguageCode = Code;
					#if !DEBUG
				} catch { }
				#endif
				ApplyLanguage();
			}
		}

		// interpolation
		private void comboboxInterpolation_SelectedIndexChanged(object sender, EventArgs e) {
			int i = comboboxInterpolation.SelectedIndex;
			bool q = i == (int)TextureManager.InterpolationMode.AnisotropicFiltering;
			labelAnisotropic.Enabled = q;
			updownAnisotropic.Enabled = q;
			q = i != (int)TextureManager.InterpolationMode.NearestNeighbor & i != (int)TextureManager.InterpolationMode.Bilinear;
		}

		// =====
		// start
		// =====

		// start
		private void buttonStart_Click(object sender, EventArgs e) {
			if (Result.RouteFile != null & Result.TrainFolder != null) {
				if (System.IO.File.Exists(Result.RouteFile) & System.IO.Directory.Exists(Result.TrainFolder)) {
					Result.Start = true;
					this.Close();
				}
			}
		}

		// ========
		// controls
		// ========

		// controls
		private void listviewControls_SelectedIndexChanged(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				{
					this.Tag = new object();
					{ /// command
						int j; for (j = 0; j < Interface.CommandInfos.Length; j++) {
							if (Interface.CommandInfos[j].Command == Interface.CurrentControls[i].Command) {
								comboboxCommand.SelectedIndex = j;
								break;
							}
						} if (j == Interface.CommandInfos.Length) {
							comboboxCommand.SelectedIndex = -1;
						}
					}
					/// data
					if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Keyboard) {
						radiobuttonKeyboard.Checked = true;
					} else if (Interface.CurrentControls[i].Method == Interface.ControlMethod.Joystick) {
						radiobuttonJoystick.Checked = true;
					} else {
						radiobuttonKeyboard.Checked = false;
						radiobuttonJoystick.Checked = false;
					}
					panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
					if (radiobuttonKeyboard.Checked) {
						int j; for (j = 0; j < Interface.Keys.Length; j++) {
							if (Interface.Keys[j].Value == Interface.CurrentControls[i].Element) {
								comboboxKeyboardKey.SelectedIndex = j;
								break;
							}
						} if (j == Interface.Keys.Length) {
							comboboxKeyboardKey.SelectedIndex = -1;
						}
						checkboxKeyboardShift.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Shift) != 0;
						checkboxKeyboardCtrl.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Ctrl) != 0;
						checkboxKeyboardAlt.Checked = (Interface.CurrentControls[i].Modifier & Interface.KeyboardModifier.Alt) != 0;
					} else if (radiobuttonJoystick.Checked) {
						labelJoystickAssignmentValue.Text = GetControlDetails(i);
					} else {
						comboboxKeyboardKey.SelectedIndex = -1;
						checkboxKeyboardShift.Checked = false;
						checkboxKeyboardCtrl.Checked = false;
						checkboxKeyboardAlt.Checked = false;
					}
					panelJoystick.Enabled = radiobuttonJoystick.Checked;
					/// finalize
					this.Tag = null;
				}
				buttonControlRemove.Enabled = true;
				buttonControlUp.Enabled = i > 0;
				buttonControlDown.Enabled = i < Interface.CurrentControls.Length - 1;
				groupboxControl.Enabled = true;
			} else {
				this.Tag = new object();
				comboboxCommand.SelectedIndex = -1;
				radiobuttonKeyboard.Checked = false;
				radiobuttonJoystick.Checked = false;
				groupboxControl.Enabled = false;
				comboboxKeyboardKey.SelectedIndex = -1;
				checkboxKeyboardShift.Checked = false;
				checkboxKeyboardCtrl.Checked = false;
				checkboxKeyboardAlt.Checked = false;
				labelJoystickAssignmentValue.Text = "";
				this.Tag = null;
				buttonControlRemove.Enabled = false;
				buttonControlUp.Enabled = false;
				buttonControlDown.Enabled = false;
			}
		}
		private void UpdateControlListElement(ListViewItem Item, int Index, bool ResizeColumns) {
			Interface.CommandInfo Info;
			Interface.TryGetCommandInfo(Interface.CurrentControls[Index].Command, out Info);
			Item.SubItems[0].Text = Info.Name;
			switch (Info.Type) {
					case Interface.CommandType.Digital: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_digital"); break;
					case Interface.CommandType.AnalogHalf: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_analoghalf"); break;
					case Interface.CommandType.AnalogFull: Item.SubItems[1].Text = Interface.GetInterfaceString("controls_list_type_analogfull"); break;
					default: Item.SubItems[1].Text = Info.Type.ToString(); break;
			}
			Item.SubItems[2].Text = Info.Description;
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Keyboard) {
				Item.ImageKey = "keyboard";
			} else if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Joystick) {
				if (Info.Type == Interface.CommandType.AnalogHalf | Info.Type == Interface.CommandType.AnalogFull) {
					Item.ImageKey = "joystick";
				} else {
					Item.ImageKey = "gamepad";
				}
			} else {
				Item.ImageKey = null;
			}
			Item.SubItems[3].Text = GetControlDetails(Index);
			if (ResizeColumns) {
				listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			}
		}

		// get control details
		private string GetControlDetails(int Index) {
			Interface.CommandInfo Info;
			Interface.TryGetCommandInfo(Interface.CurrentControls[Index].Command, out Info);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			string Separator = Interface.GetInterfaceString("controls_assignment_separator");
			if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Keyboard) {
				string t = Interface.GetInterfaceString("controls_assignment_keyboard") + Separator;
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Shift) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_shift");
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Ctrl) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_ctrl");
				if ((Interface.CurrentControls[Index].Modifier & Interface.KeyboardModifier.Alt) != 0) t += Interface.GetInterfaceString("controls_assignment_keyboard_alt");
				int j; for (j = 0; j < Interface.Keys.Length; j++) {
					if (Interface.Keys[j].Value == Interface.CurrentControls[Index].Element) {
						t += Interface.Keys[j].Description;
						break;
					}
				} if (j == Interface.Keys.Length) {
					t += "{" + Interface.CurrentControls[Index].Element.ToString(Culture) + "}";
				}
				return t;
			} else if (Interface.CurrentControls[Index].Method == Interface.ControlMethod.Joystick) {
				string t = Interface.GetInterfaceString("controls_assignment_joystick").Replace("[index]", (Interface.CurrentControls[Index].Device + 1).ToString(Culture));
				switch (Interface.CurrentControls[Index].Component) {
					case Interface.JoystickComponent.Axis:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						if (Interface.CurrentControls[Index].Direction == -1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_negative");
						} else if (Interface.CurrentControls[Index].Direction == 1) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_positive");
						} else {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_axis_invalid");
						} break;
					case Interface.JoystickComponent.Button:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_button").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						break;
					case Interface.JoystickComponent.Hat:
						t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat").Replace("[index]", (Interface.CurrentControls[Index].Element + 1).ToString(Culture));
						if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_LEFT) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_left");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_LEFTUP) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_upleft");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_UP) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_up");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_RIGHTUP) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_upright");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_RIGHT) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_right");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_RIGHTDOWN) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_downright");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_DOWN) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_down");
						} else if (Interface.CurrentControls[Index].Direction == (int)Sdl.SDL_HAT_LEFTDOWN) {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_downleft");
						} else {
							t += Separator + Interface.GetInterfaceString("controls_assignment_joystick_hat_invalid");
						} break;
					default:
						break;
				}
				return t;
			} else {
				return Interface.GetInterfaceString("controls_assignment_invalid");
			}
		}

		// control add
		private void buttonControlAdd_Click(object sender, EventArgs e) {
			for (int i = 0; i < Interface.CurrentControls.Length; i++) {
				listviewControls.Items[i].Selected = false;
			}
			int n = Interface.CurrentControls.Length;
			Array.Resize<Interface.Control>(ref Interface.CurrentControls, n + 1);
			Interface.CurrentControls[n].Command = Interface.Command.None;
			ListViewItem Item = new ListViewItem(new string[] { "", "", "", "" });
			UpdateControlListElement(Item, n, true);
			listviewControls.Items.Add(Item);
			Item.Selected = true;
		}

		// control remove
		private void buttonControlRemove_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				for (int i = j; i < Interface.CurrentControls.Length - 1; i++) {
					Interface.CurrentControls[i] = Interface.CurrentControls[i + 1];
				}
				Array.Resize<Interface.Control>(ref Interface.CurrentControls, Interface.CurrentControls.Length - 1);
				listviewControls.Items[j].Remove();
			}
		}

		// control up
		private void buttonControlUp_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (j > 0) {
					Interface.Control c = Interface.CurrentControls[j];
					Interface.CurrentControls[j] = Interface.CurrentControls[j - 1];
					Interface.CurrentControls[j - 1] = c;
					ListViewItem v = listviewControls.Items[j];
					listviewControls.Items.RemoveAt(j);
					listviewControls.Items.Insert(j - 1, v);
				}
			}
		}

		// control down
		private void buttonControlDown_Click(object sender, EventArgs e) {
			if (listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (j < Interface.CurrentControls.Length - 1) {
					Interface.Control c = Interface.CurrentControls[j];
					Interface.CurrentControls[j] = Interface.CurrentControls[j + 1];
					Interface.CurrentControls[j + 1] = c;
					ListViewItem v = listviewControls.Items[j];
					listviewControls.Items.RemoveAt(j);
					listviewControls.Items.Insert(j + 1, v);
				}
			}
		}

		// command
		private void comboboxCommand_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxCommand.SelectedIndex;
				if (j >= 0) {
					Interface.CurrentControls[i].Command = Interface.CommandInfos[j].Command;
					Interface.CommandInfo Info;
					Interface.TryGetCommandInfo(Interface.CommandInfos[j].Command, out Info);
					Interface.CurrentControls[i].InheritedType = Info.Type;
					UpdateControlListElement(listviewControls.Items[i], i, true);
				}
			}
		}

		// ========
		// keyboard
		// ========

		// keyboard
		private void radiobuttonKeyboard_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = Interface.ControlMethod.Keyboard;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelKeyboard.Enabled = radiobuttonKeyboard.Checked;
		}

		// key
		private void comboboxKeyboardKey_SelectedIndexChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				int j = comboboxKeyboardKey.SelectedIndex;
				if (j >= 0) {
					Interface.CurrentControls[i].Element = Interface.Keys[j].Value;
				}
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}

		// modifiers
		private void checkboxKeyboardShift_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardCtrl_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}
		private void checkboxKeyboardAlt_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Modifier = (checkboxKeyboardShift.Checked ? Interface.KeyboardModifier.Shift : Interface.KeyboardModifier.None) |
					(checkboxKeyboardCtrl.Checked ? Interface.KeyboardModifier.Ctrl : Interface.KeyboardModifier.None) |
					(checkboxKeyboardAlt.Checked ? Interface.KeyboardModifier.Alt : Interface.KeyboardModifier.None);
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
		}

		// ================
		// review last game
		// ================

		// score save
		private void buttonScoreExport_Click(object sender, EventArgs e) {
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.OverwritePrompt = true;
			Dialog.Filter = Interface.GetInterfaceString("dialog_textfiles") + "|*.txt|" + Interface.GetInterfaceString("dialog_allfiles") + "|*";
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.ExportScore(Dialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// score penalties
		private void checkboxScorePenalties_CheckedChanged(object sender, EventArgs e) {
			ShowScoreLog(checkboxScorePenalties.Checked);
		}

		// black box export
		private void buttonBlackBoxExport_Click(object sender, EventArgs e) {
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.OverwritePrompt = true;
			if (comboboxBlackBoxFormat.SelectedIndex == 0) {
				Dialog.Filter = Interface.GetInterfaceString("dialog_csvfiles") + "|*.txt|" + Interface.GetInterfaceString("dialog_allfiles") + "|*";
			} else {
				Dialog.Filter = Interface.GetInterfaceString("dialog_textfiles") + "|*.txt|" + Interface.GetInterfaceString("dialog_allfiles") + "|*";
			}
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.ExportBlackBox(Dialog.FileName, (Interface.BlackBoxFormat)comboboxBlackBoxFormat.SelectedIndex);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// ========
		// joystick
		// ========

		// joystick
		private void radiobuttonJoystick_CheckedChanged(object sender, EventArgs e) {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int i = listviewControls.SelectedIndices[0];
				Interface.CurrentControls[i].Method = Interface.ControlMethod.Joystick;
				UpdateControlListElement(listviewControls.Items[i], i, true);
			}
			panelJoystick.Enabled = radiobuttonJoystick.Checked;
		}

		// details
		private void UpdateJoystickDetails() {
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				labelJoystickAssignmentValue.Text = GetControlDetails(j);

			}
		}

		// import
		private void buttonControlsImport_Click(object sender, EventArgs e) {
			OpenFileDialog Dialog = new OpenFileDialog();
			Dialog.CheckFileExists = true;
			Dialog.InitialDirectory = Interface.GetControlsFolder();
			Dialog.Filter = Interface.GetInterfaceString("dialog_controlsfiles") + "|*.controls|" + Interface.GetInterfaceString("dialog_allfiles") + "|*";
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.LoadControls(Dialog.FileName, out Interface.CurrentControls);
					for (int i = 0; i < listviewControls.SelectedItems.Count; i++) {
						listviewControls.SelectedItems[i].Selected = false;
					}
					listviewControls.Items.Clear();
					ListViewItem[] Items = new ListViewItem[Interface.CurrentControls.Length];
					for (int i = 0; i < Interface.CurrentControls.Length; i++) {
						Items[i] = new ListViewItem(new string[] { "", "", "", "" });
						UpdateControlListElement(Items[i], i, false);
					}
					listviewControls.Items.AddRange(Items);
					listviewControls.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// export
		private void buttonControlsExport_Click(object sender, EventArgs e) {
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.OverwritePrompt = true;
			Dialog.InitialDirectory = Interface.GetControlsFolder();
			Dialog.Filter = Interface.GetInterfaceString("dialog_controlsfiles") + "|*.controls|" + Interface.GetInterfaceString("dialog_allfiles") + "|*";
			if (Dialog.ShowDialog() == DialogResult.OK) {
				try {
					Interface.SaveControls(Dialog.FileName);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}

		// joystick grab
		private void textboxJoystickGrab_Enter(object sender, EventArgs e) {
			bool FullAxis = false;
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].InheritedType == Interface.CommandType.AnalogFull) {
					FullAxis = true;
				}
			}
			if (FullAxis) {
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab_fullaxis");
			} else {
				textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab_normal");
			}
			textboxJoystickGrab.BackColor = Color.Crimson;
			textboxJoystickGrab.ForeColor = Color.White;
		}
		private void textboxJoystickGrab_Leave(object sender, EventArgs e) {
			textboxJoystickGrab.Text = Interface.GetInterfaceString("controls_selection_joystick_assignment_grab");
			textboxJoystickGrab.BackColor = panelControls.BackColor;
			textboxJoystickGrab.ForeColor = Color.Black;
		}

		// attached joysticks
		private void pictureboxJoysticks_Paint(object sender, PaintEventArgs e) {
			int device = -1;
			Interface.JoystickComponent component = Interface.JoystickComponent.Invalid;
			int element = -1;
			int direction = -1;
			Interface.CommandType type = Interface.CommandType.Digital;
			if (this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				if (Interface.CurrentControls[j].Method == Interface.ControlMethod.Joystick) {
					device = Interface.CurrentControls[j].Device;
					component = Interface.CurrentControls[j].Component;
					element = Interface.CurrentControls[j].Element;
					direction = Interface.CurrentControls[j].Direction;
					type = Interface.CurrentControls[j].InheritedType;
				}
			}
			Sdl.SDL_JoystickUpdate();
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			Font f = new Font(this.Font.Name, 0.875f * this.Font.Size);
			float x = 2.0f, y = 2.0f;
			float threshold = ((float)trackbarJoystickAxisThreshold.Value - (float)trackbarJoystickAxisThreshold.Minimum) / (float)(trackbarJoystickAxisThreshold.Maximum - trackbarJoystickAxisThreshold.Minimum);
			for (int i = 0; i < Interface.CurrentJoysticks.Length; i++) {
				float w, h;
				if (JoystickImage != null) {
					e.Graphics.DrawImage(JoystickImage, x, y);
					w = (float)JoystickImage.Width;
					h = (float)JoystickImage.Height;
					if (h < 64.0f) h = 64.0f;
				} else {
					w = 64.0f; h = 64.0f;
					e.Graphics.DrawRectangle(new Pen(labelControlsTitle.BackColor), x, y, w, h);
				}
				{ /// joystick number
					e.Graphics.FillEllipse(Brushes.Gold, x + w - 16.0f, y, 16.0f, 16.0f);
					e.Graphics.DrawEllipse(Pens.Black, x + w - 16.0f, y, 16.0f, 16.0f);
					string t = (i + 1).ToString(Culture);
					SizeF s = e.Graphics.MeasureString(t, f);
					e.Graphics.DrawString(t, f, Brushes.Black, x + w - 8.0f - 0.5f * s.Width, y + 8.0f - 0.5f * s.Height);
				}
				{ /// joystick name
					e.Graphics.DrawString(Interface.CurrentJoysticks[i].Name, this.Font, Brushes.Black, x + w + 8.0f, y);
				}
				float m;
				if (groupboxJoysticks.Enabled) {
					m = x;
					Pen p = new Pen(Color.DarkGoldenrod, 2.0f);
					Pen ps = new Pen(Color.Firebrick, 2.0f);
					{ /// first row
						float u = x + w + 8.0f;
						float v = y + 24.0f;
						float g = h - 24.0f;
						{ /// trackballs
							int n = Sdl.SDL_JoystickNumBalls(Interface.CurrentJoysticks[i].SdlHandle);
							for (int j = 0; j < n; j++) {
								e.Graphics.DrawEllipse(Pens.Gray, u, v, g, g);
								string t = "L" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Gray, u + 0.5f * (g - s.Width), v + 0.5f * (g - s.Height));
								int dx, dy;
								Sdl.SDL_JoystickGetBall(Interface.CurrentJoysticks[i].SdlHandle, j, out dx, out dy);
								u += g + 8.0f;
							}
						}
						{ /// hats
							int n = Sdl.SDL_JoystickNumHats(Interface.CurrentJoysticks[i].SdlHandle);
							for (int j = 0; j < n; j++) {
								if (device == i & component == Interface.JoystickComponent.Hat & element == j) {
									e.Graphics.DrawEllipse(ps, u, v, g, g);
								} else {
									e.Graphics.DrawEllipse(p, u, v, g, g);
								}
								string t = "H" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + 0.5f * (g - s.Height));
								byte a = Sdl.SDL_JoystickGetHat(Interface.CurrentJoysticks[i].SdlHandle, j);
								if (a != Sdl.SDL_HAT_CENTERED) {
									double rx = (a & Sdl.SDL_HAT_LEFT) != 0 ? -1.0 : (a & Sdl.SDL_HAT_RIGHT) != 0 ? 1.0 : 0.0;
									double ry = (a & Sdl.SDL_HAT_UP) != 0 ? -1.0 : (a & Sdl.SDL_HAT_DOWN) != 0 ? 1.0 : 0.0;
									double rt = rx * rx + ry * ry;
									rt = 1.0 / Math.Sqrt(rt);
									rx *= rt; ry *= rt;
									float dx = (float)(0.5 * rx * (g - 8.0));
									float dy = (float)(0.5 * ry * (g - 8.0));
									e.Graphics.FillEllipse(Brushes.White, u + 0.5f * g + dx - 4.0f, v + 0.5f * g + dy - 4.0f, 8.0f, 8.0f);
									e.Graphics.DrawEllipse(new Pen(Color.Firebrick, 2.0f), u + 0.5f * g + dx - 4.0f, v + 0.5f * g + dy - 4.0f, 8.0f, 8.0f);
								}
								if (device == i & component == Interface.JoystickComponent.Hat & element == j) {
									double rx = (direction & Sdl.SDL_HAT_LEFT) != 0 ? -1.0 : (direction & Sdl.SDL_HAT_RIGHT) != 0 ? 1.0 : 0.0;
									double ry = (direction & Sdl.SDL_HAT_UP) != 0 ? -1.0 : (direction & Sdl.SDL_HAT_DOWN) != 0 ? 1.0 : 0.0;
									double rt = rx * rx + ry * ry;
									rt = 1.0 / Math.Sqrt(rt);
									rx *= rt; ry *= rt;
									float dx = (float)(0.5 * rx * (g - 8.0));
									float dy = (float)(0.5 * ry * (g - 8.0));
									e.Graphics.FillEllipse(Brushes.Firebrick, u + 0.5f * g + dx - 2.0f, v + 0.5f * g + dy - 2.0f, 4.0f, 4.0f);
								}
								u += g + 8.0f;
							}
						}
						if (u > m) m = u;
					}
					{ /// second row
						float u = x;
						float v = y + h + 8.0f;
						{ /// axes
							int n = Sdl.SDL_JoystickNumAxes(Interface.CurrentJoysticks[i].SdlHandle);
							float g = (float)pictureboxJoysticks.ClientRectangle.Height - v - 2.0f;
							for (int j = 0; j < n; j++) {
								float r = (float)Sdl.SDL_JoystickGetAxis(Interface.CurrentJoysticks[i].SdlHandle, j) / 32768.0f;
								float r0 = r < 0.0f ? r : 0.0f;
								float r1 = r > 0.0f ? r : 0.0f;
								if ((float)Math.Abs((double)r) < threshold) {
									e.Graphics.FillRectangle(Brushes.RosyBrown, u, v + 0.5f * g - 0.5f * r1 * g, 16.0f, 0.5f * g * (r1 - r0));
								} else {
									e.Graphics.FillRectangle(Brushes.Firebrick, u, v + 0.5f * g - 0.5f * r1 * g, 16.0f, 0.5f * g * (r1 - r0));
								}
								if (device == i & component == Interface.JoystickComponent.Axis & element == j) {
									if (direction == -1 & type != Interface.CommandType.AnalogFull) {
										e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
										e.Graphics.DrawRectangle(ps, u, v + 0.5f * g, 16.0f, 0.5f * g);
									} else if (direction == 1 & type != Interface.CommandType.AnalogFull) {
										e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
										e.Graphics.DrawRectangle(ps, u, v, 16.0f, 0.5f * g);
									} else {
										e.Graphics.DrawRectangle(ps, u, v, 16.0f, g);
									}
								} else {
									e.Graphics.DrawRectangle(p, u, v, 16.0f, g);
								}
								e.Graphics.DrawLine(p, u, v + (0.5f - 0.5f * threshold) * g, u + 16.0f, v + (0.5f - 0.5f * threshold) * g);
								e.Graphics.DrawLine(p, u, v + (0.5f + 0.5f * threshold) * g, u + 16.0f, v + (0.5f + 0.5f * threshold) * g);
								string t = "A" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (16.0f - s.Width), v + g - s.Height - 2.0f);
								u += 24.0f;
							}
						}
						{ /// buttons
							int n = Sdl.SDL_JoystickNumButtons(Interface.CurrentJoysticks[i].SdlHandle);
							float g = (float)0.5f * (pictureboxJoysticks.ClientRectangle.Height - v - 10.0f);
							for (int j = 0; j < n; j++) {
								bool q = Sdl.SDL_JoystickGetButton(Interface.CurrentJoysticks[i].SdlHandle, j) != 0;
								float dv = (float)(j & 1) * (g + 8.0f);
								if (q) e.Graphics.FillRectangle(Brushes.Firebrick, u, v + dv, g, g);
								if (device == i & component == Interface.JoystickComponent.Button & element == j) {
									e.Graphics.DrawRectangle(ps, u, v + dv, g, g);
								} else {
									e.Graphics.DrawRectangle(p, u, v + dv, g, g);
								}
								string t = "B" + (j + 1).ToString(Culture);
								SizeF s = e.Graphics.MeasureString(t, f);
								e.Graphics.DrawString(t, f, Brushes.Black, u + 0.5f * (g - s.Width), v + dv + 0.5f * (g - s.Height));
								if ((j & 1) != 0 | j == n - 1) u += g + 8.0f;
							}
						}
						if (u > m) m = u;
					}
				} else {
					m = x + w + 64.0f;
				}
				x = m + 8.0f;
			}
		}

		// =======
		// options
		// =======

		// joysticks enabled
		private void checkboxJoysticksUsed_CheckedChanged(object sender, EventArgs e) {
			groupboxJoysticks.Enabled = checkboxJoysticksUsed.Checked;
		}

		// ======
		// events
		// ======

		// tick
		private void timerEvents_Tick(object sender, EventArgs e) {
			if (textboxJoystickGrab.Focused & this.Tag == null & listviewControls.SelectedIndices.Count == 1) {
				int j = listviewControls.SelectedIndices[0];
				Sdl.SDL_Event Event;
				while (Sdl.SDL_PollEvent(out Event) != 0) {
					switch (Event.type) {
						case Sdl.SDL_JOYAXISMOTION:
							{
								double a = (double)Event.jaxis.val / 32768.0;
								if (a < -0.75) {
									Interface.CurrentControls[j].Device = (int)Event.jaxis.which;
									Interface.CurrentControls[j].Component = Interface.JoystickComponent.Axis;
									Interface.CurrentControls[j].Element = (int)Event.jaxis.axis;
									Interface.CurrentControls[j].Direction = -1;
									radiobuttonJoystick.Focus();
									UpdateJoystickDetails();
									UpdateControlListElement(listviewControls.Items[j], j, true);
								} else if (a > 0.75) {
									Interface.CurrentControls[j].Device = (int)Event.jaxis.which;
									Interface.CurrentControls[j].Component = Interface.JoystickComponent.Axis;
									Interface.CurrentControls[j].Element = (int)Event.jaxis.axis;
									Interface.CurrentControls[j].Direction = 1;
									radiobuttonJoystick.Focus();
									UpdateJoystickDetails();
									UpdateControlListElement(listviewControls.Items[j], j, true);
								}
							} break;
						case Sdl.SDL_JOYBALLMOTION:
							{
							} break;
						case Sdl.SDL_JOYHATMOTION:
							{
								Interface.CurrentControls[j].Device = (int)Event.jhat.which;
								Interface.CurrentControls[j].Component = Interface.JoystickComponent.Hat;
								Interface.CurrentControls[j].Element = (int)Event.jhat.hat;
								Interface.CurrentControls[j].Direction = (int)Event.jhat.val;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
							} break;
						case Sdl.SDL_JOYBUTTONDOWN:
							{
								Interface.CurrentControls[j].Device = (int)Event.jbutton.which;
								Interface.CurrentControls[j].Component = Interface.JoystickComponent.Button;
								Interface.CurrentControls[j].Element = (int)Event.jbutton.button;
								Interface.CurrentControls[j].Direction = 1;
								radiobuttonJoystick.Focus();
								UpdateJoystickDetails();
								UpdateControlListElement(listviewControls.Items[j], j, true);
							} break;
					}
				}
			} else {
				Sdl.SDL_Event Event;
				while (Sdl.SDL_PollEvent(out Event) != 0) {
				}
			}
			pictureboxJoysticks.Invalidate();
		}

		// =========
		// functions
		// =========

		// show route
		private void ShowRoute(bool UserSelectedEncoding) {
			if (Result.RouteFile != null) {
				this.Cursor = Cursors.WaitCursor;
				Application.DoEvents();
				// determine encoding
				if (!UserSelectedEncoding) {
					comboboxRouteEncoding.Tag = new object();
					comboboxRouteEncoding.SelectedIndex = 0;
					comboboxRouteEncoding.Items[0] = "(UTF-8)";
					comboboxRouteEncoding.Tag = null;
					Result.RouteEncoding = System.Text.Encoding.UTF8;
					switch (Interface.GetEncodingFromFile(Result.RouteFile)) {
						case Interface.Encoding.Utf8:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-8)";
							Result.RouteEncoding = System.Text.Encoding.UTF8;
							break;
						case Interface.Encoding.Utf16Le:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-16 little endian)";
							Result.RouteEncoding = System.Text.Encoding.Unicode;
							break;
						case Interface.Encoding.Utf16Be:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-16 big endian)";
							Result.RouteEncoding = System.Text.Encoding.BigEndianUnicode;
							break;
						case Interface.Encoding.Utf32Le:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-32 little endian)";
							Result.RouteEncoding = System.Text.Encoding.UTF32;
							break;
						case Interface.Encoding.Utf32Be:
							panelRouteEncoding.Enabled = false;
							comboboxRouteEncoding.SelectedIndex = 0;
							comboboxRouteEncoding.Items[0] = "(UTF-32 big endian)";
							Result.RouteEncoding = System.Text.Encoding.GetEncoding(12001);
							break;
					}
					panelRouteEncoding.Enabled = true;
					comboboxRouteEncoding.Tag = new object();
					int i;
					for (i = 0; i < Interface.CurrentOptions.RouteEncodings.Length; i++) {
						if (Interface.CurrentOptions.RouteEncodings[i].Value == Result.RouteFile) {
							int j;
							for (j = 1; j < EncodingCodepages.Length; j++) {
								if (EncodingCodepages[j] == Interface.CurrentOptions.RouteEncodings[i].Codepage) {
									comboboxRouteEncoding.SelectedIndex = j;
									Result.RouteEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[j]);
									break;
								}
							}
							if (j == EncodingCodepages.Length) {
								comboboxRouteEncoding.SelectedIndex = 0;
								Result.RouteEncoding = System.Text.Encoding.UTF8;
							}
							break;
						}
					}
					comboboxRouteEncoding.Tag = null;
				}
				// parse route
				try {
					Game.Reset(false);
					bool IsRW = string.Equals(System.IO.Path.GetExtension(Result.RouteFile), ".rw", StringComparison.OrdinalIgnoreCase);
					CsvRwRouteParser.ParseRoute(Result.RouteFile, IsRW, Result.RouteEncoding, null, null, null, true);
					pictureboxRouteMap.Image = Illustrations.CreateRouteMap(pictureboxRouteMap.Width, pictureboxRouteMap.Height);
					pictureboxRouteGradient.Image = Illustrations.CreateRouteGradientProfile(pictureboxRouteGradient.Width, pictureboxRouteGradient.Height);
					// image
					if (Game.RouteImage.Length != 0) {
						try {
							pictureboxRouteImage.Image = Image.FromFile(Game.RouteImage);
						} catch {
							TryLoadImage(pictureboxRouteImage, "route_error.png");
						}
					} else {
						string f = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Result.RouteFile), System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile));
						string[] e = new string[] { ".png", ".bmp", ".gif", ".tiff", ".tif", ".jpeg", ".jpg" };
						int i;
						for (i = 0; i < e.Length; i++) {
							string g = Interface.GetCorrectedFileName(f + e[i]);
							if (System.IO.File.Exists(g)) {
								TryLoadImage(pictureboxRouteImage, g);
								break;
							}
						}
						if (i == e.Length) {
							TryLoadImage(pictureboxRouteImage, "route_unknown.png");
						}
					}
					// description
					string Description = Interface.ConvertNewlinesToCrLf(Game.RouteComment);
					if (Description.Length != 0) {
						textboxRouteDescription.Text = Description;
					} else {
						textboxRouteDescription.Text = System.IO.Path.GetFileNameWithoutExtension(Result.RouteFile);
					}
					textboxRouteEncodingPreview.Text = Interface.ConvertNewlinesToCrLf(Description);
					if (Game.TrainName != null) {
						checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault") + " (" + Game.TrainName + ")";
					} else {
						checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
					}
				} catch (Exception ex) {
					// error
					TryLoadImage(pictureboxRouteImage, "route_error.png");
					textboxRouteDescription.Text = ex.Message;
					textboxRouteEncodingPreview.Text = "";
					pictureboxRouteMap.Image = null;
					pictureboxRouteGradient.Image = null;
					Result.RouteFile = null;
					checkboxTrainDefault.Text = Interface.GetInterfaceString("start_train_usedefault");
				}
				groupboxRouteDetails.Visible = true;
				if (checkboxTrainDefault.Checked) {
					ShowDefaultTrain();
				}
				this.Cursor = Cursors.Default;
				buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
			}
		}

		// show train
		private void ShowTrain(bool UserSelectedEncoding) {
			if (!UserSelectedEncoding) {
				comboboxTrainEncoding.Tag = new object();
				comboboxTrainEncoding.SelectedIndex = 0;
				comboboxTrainEncoding.Items[0] = "(UTF-8)";
				comboboxTrainEncoding.Tag = null;
				Result.TrainEncoding = System.Text.Encoding.UTF8;
				switch (Interface.GetEncodingFromFile(Result.TrainFolder, "train.txt")) {
					case Interface.Encoding.Utf8:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-8)";
						Result.TrainEncoding = System.Text.Encoding.UTF8;
						break;
					case Interface.Encoding.Utf16Le:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-16 little endian)";
						Result.TrainEncoding = System.Text.Encoding.Unicode;
						break;
					case Interface.Encoding.Utf16Be:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-16 big endian)";
						Result.TrainEncoding = System.Text.Encoding.BigEndianUnicode;
						break;
					case Interface.Encoding.Utf32Le:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-32 little endian)";
						Result.TrainEncoding = System.Text.Encoding.UTF32;
						break;
					case Interface.Encoding.Utf32Be:
						comboboxTrainEncoding.SelectedIndex = 0;
						comboboxTrainEncoding.Items[0] = "(UTF-32 big endian)";
						Result.TrainEncoding = System.Text.Encoding.GetEncoding(12001);
						break;
				}
				int i;
				for (i = 0; i < Interface.CurrentOptions.TrainEncodings.Length; i++) {
					if (Interface.CurrentOptions.TrainEncodings[i].Value == Result.TrainFolder) {
						int j;
						for (j = 1; j < EncodingCodepages.Length; j++) {
							if (EncodingCodepages[j] == Interface.CurrentOptions.TrainEncodings[i].Codepage) {
								comboboxTrainEncoding.SelectedIndex = j;
								Result.TrainEncoding = System.Text.Encoding.GetEncoding(EncodingCodepages[j]);
								break;
							}
						}
						if (j == EncodingCodepages.Length) {
							comboboxTrainEncoding.SelectedIndex = 0;
							Result.TrainEncoding = System.Text.Encoding.UTF8;
						}
						break;
					}
				}
				panelTrainEncoding.Enabled = true;
				comboboxTrainEncoding.Tag = null;
			}
			{
				// train image
				string File = Interface.GetCombinedFileName(Result.TrainFolder, "train.png");
				if (!System.IO.File.Exists(File)) {
					File = Interface.GetCombinedFileName(Result.TrainFolder, "train.bmp");
				}
				if (System.IO.File.Exists(File)) {
					try {
						pictureboxTrainImage.Image = Image.FromFile(File);
					} catch {
						pictureboxTrainImage.Image = null;
						TryLoadImage(pictureboxTrainImage, "train_error.png");
					}
				} else {
					TryLoadImage(pictureboxTrainImage, "train_unknown.png");
				}
			}
			{
				// train description
				string File = Interface.GetCombinedFileName(Result.TrainFolder, "train.txt");
				if (System.IO.File.Exists(File)) {
					try {
						string Text = System.IO.File.ReadAllText(File, Result.TrainEncoding);
						Text = Interface.ConvertNewlinesToCrLf(Text);
						textboxTrainDescription.Text = Text;
						textboxTrainEncodingPreview.Text = Text;
					} catch {
						textboxTrainDescription.Text = System.IO.Path.GetFileName(Result.TrainFolder);
						textboxTrainEncodingPreview.Text = "";
					}
				} else {
					textboxTrainDescription.Text = System.IO.Path.GetFileName(Result.TrainFolder);
					textboxTrainEncodingPreview.Text = "";
				}
			}
			groupboxTrainDetails.Visible = true;
			labelTrainEncoding.Enabled = true;
			labelTrainEncodingPreview.Enabled = true;
			textboxTrainEncodingPreview.Enabled = true;
			buttonStart.Enabled = Result.RouteFile != null & Result.TrainFolder != null;
		}

		// show default train
		private void ShowDefaultTrain() {
			if (Result.RouteFile != null && Result.RouteFile.Length != 0) {
				string Name = Game.TrainName;
				if (Name != null && Name.Length != 0) {
					string Folder = System.IO.Path.GetDirectoryName(Result.RouteFile);
					while (true) {
						string TrainFolder = Interface.GetCombinedFolderName(Folder, "Train");
						if (System.IO.Directory.Exists(TrainFolder)) {
							Folder = Interface.GetCombinedFolderName(TrainFolder, Name);
							if (System.IO.Directory.Exists(Folder)) {
								string File = Interface.GetCombinedFileName(Folder, "train.dat");
								if (System.IO.File.Exists(File)) {
									/// train found
									Result.TrainFolder = Folder;
									ShowTrain(false);
									return;
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
			}
			/// train not found
			Result.TrainFolder = null;
			TryLoadImage(pictureboxTrainImage, "train_error.png");
			textboxTrainDescription.Text = Interface.ConvertNewlinesToCrLf(Interface.GetInterfaceString("start_train_notfound") + Game.TrainName);
			comboboxTrainEncoding.Tag = new object();
			comboboxTrainEncoding.SelectedIndex = 0;
			comboboxTrainEncoding.Tag = null;
			labelTrainEncoding.Enabled = false;
			panelTrainEncoding.Enabled = false;
			labelTrainEncodingPreview.Enabled = false;
			textboxTrainEncodingPreview.Enabled = false;
			textboxTrainEncodingPreview.Text = "";
			groupboxTrainDetails.Visible = true;
		}

		// show score log
		private void ShowScoreLog(bool PenaltiesOnly) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			listviewScore.Items.Clear();
			int sum = 0;
			for (int i = 0; i < Game.ScoreLogCount; i++) {
				sum += Game.ScoreLogs[i].Value;
				if (!PenaltiesOnly | Game.ScoreLogs[i].Value < 0) {
					double x = Game.ScoreLogs[i].Time;
					int h = (int)Math.Floor(x / 3600.0);
					x -= 3600.0 * (double)h;
					int m = (int)Math.Floor(x / 60.0);
					x -= 60.0 * (double)m;
					int s = (int)Math.Floor(x);
					ListViewItem Item = listviewScore.Items.Add(h.ToString("00", Culture) + ":" + m.ToString("00", Culture) + ":" + s.ToString("00", Culture));
					Item.SubItems.Add(Game.ScoreLogs[i].Position.ToString("0", Culture));
					Item.SubItems.Add(Game.ScoreLogs[i].Value.ToString(Culture));
					Item.SubItems.Add(sum.ToString(Culture));
					Item.SubItems.Add(Interface.GetScoreText(Game.ScoreLogs[i].TextToken));
				}
			}
			listviewScore.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		// load image
		private Image LoadImage(string Folder, string Title) {
			string File = Interface.GetCombinedFileName(Folder, Title);
			if (System.IO.File.Exists(File)) {
				try {
					return Image.FromFile(File);
				} catch { }
			}
			return null;
		}

		// try load image
		private bool TryLoadImage(PictureBox Box, string Title) {
			string Folder = Interface.GetDataFolder("Menu");
			string File = Interface.GetCombinedFileName(Folder, Title);
			if (System.IO.File.Exists(File)) {
				try {
					Box.Image = Image.FromFile(File);
					return true;
				} catch {
					Box.Image = Box.ErrorImage;
					return false;
				}
			} else {
				Box.Image = Box.ErrorImage;
				return false;
			}
		}

	}
}