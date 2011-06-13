using System;
using System.Drawing;
using Tao.OpenGl;

namespace OpenBve {
	internal static partial class Renderer {
		
		/* --------------------------------------------------------------
		 * This file contains the drawing routines for the loading screen
		 * -------------------------------------------------------------- */
		
		internal static void DrawLoadingScreen() {
			
			// begin HACK //
			Gl.glEnable(Gl.GL_BLEND);
			int size = Math.Min(Screen.Width, Screen.Height);
			if (Textures.LoadTexture(TextureLogo)) {
				DrawRectangle(TextureLogo, new Point((Screen.Width - size) / 2, (Screen.Height - size) / 2), new Size(size, size), OpenBveApi.Objects.Color128.White);
			}
			DrawRectangle(null, new Point((Screen.Width - size) / 2, Screen.Height - (int)Fonts.NormalFont.FontSize - 10), new Size(Screen.Width, (int)Fonts.NormalFont.FontSize + 10), new OpenBveApi.Objects.Color128(0.0f, 0.0f, 0.0f, 0.5f));
			string text;
			if (Loading.RouteProgress < 1.0) {
				text = "Loading route... " + (100.0 * Loading.RouteProgress).ToString("0") + "%";
			} else if (Loading.TrainProgress < 1.0) {
				text = "Loading train... " + (100.0 * Loading.RouteProgress).ToString("0") + "%";
			} else {
				text = "Loading textures...";
			}
			DrawString(Fonts.NormalFont, text, new Point((Screen.Width - size) / 2 + 5, Screen.Height - (int)(Fonts.NormalFont.FontSize / 2) - 5), TextAlignment.CenterLeft, OpenBveApi.Objects.Color128.White);
			// end HACK //

		}
		
	}
}