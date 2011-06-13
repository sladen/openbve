using System;
using OpenBveApi.Hosts;
using OpenBveApi.Textures;

namespace Plugin {
	/// <summary>Implements the texture interface.</summary>
	public partial class Plugin : TextureInterface {
		
		// --- members ---
		
		/// <summary>The host that loaded the plugin.</summary>
		private HostInterface CurrentHost = null;
		
		
		// --- functions ---
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public override void Load(HostInterface host) {
			CurrentHost = host;
		}
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(OpenBveApi.Path.PathReference path, out int width, out int height) {
			QueryDimensionsFromFile(((OpenBveApi.Path.FileReference)path).File, out width, out height);
			return true;
		}
		
		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		public override bool CanLoadTexture(OpenBveApi.Path.PathReference path) {
			if (path is OpenBveApi.Path.FileReference) {
				return CanLoadFile(((OpenBveApi.Path.FileReference)path).File);
			} else {
				return false;
			}
		}
		
		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(OpenBveApi.Path.PathReference path, out Texture texture) {
			texture = LoadFromFile(((OpenBveApi.Path.FileReference)path).File);
			return true;
		}
		
	}
}