using System;

namespace OpenBve {
	/// <summary>Represents the host application.</summary>
	internal class Host : OpenBveApi.Hosts.HostInterface {
		
		// --- constructors ---
		
		/// <summary>Creates a new instance of this class.</summary>
		internal Host() {
		}
		
		
		// --- functions ---
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public override void ReportProblem(OpenBveApi.Hosts.ProblemType type, string text) {
			//string file = @"C:\log.txt";
			//System.IO.File.AppendAllText(file, type.ToString() + ": " + text + "\n");
		}
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(OpenBveApi.Path.PathReference path, out int width, out int height) {
			if (path.Exists()) {
				for (int i = 0; i < Plugins.LoadedPlugins.Length; i++) {
					if (Plugins.LoadedPlugins[i].Texture != null) {
						if (Plugins.LoadedPlugins[i].Texture.CanLoadTexture(path)) {
							if (Plugins.LoadedPlugins[i].Texture.QueryTextureDimensions(path, out width, out height)) {
								return true;
							} else {
								width = 0;
								height = 0;
								return false;
							}
						}
					}
				}
			}
			width = 0;
			height = 0;
			return false;
		}
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(OpenBveApi.Path.PathReference path, OpenBveApi.Textures.TextureParameters parameters, out OpenBveApi.Textures.Texture texture) {
			if (path.Exists()) {
				for (int i = 0; i < Plugins.LoadedPlugins.Length; i++) {
					if (Plugins.LoadedPlugins[i].Texture != null) {
						if (Plugins.LoadedPlugins[i].Texture.CanLoadTexture(path)) {
							if (!Plugins.LoadedPlugins[i].Texture.LoadTexture(path, out texture)) {
								texture = null;
								return false;
							}
							try {
								texture = OpenBveApi.Textures.Texture.ApplyParameters(texture, parameters);
								return true;
							} catch {
								texture = null;
								return false;
							}
						}
					}
				}
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path.ToString());
			}
			texture = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool RegisterTexture(OpenBveApi.Path.PathReference path, OpenBveApi.Textures.TextureParameters parameters, out OpenBveApi.Textures.TextureHandle handle) {
			if (path.Exists()) {
				Textures.Texture data;
				if (Textures.RegisterTexture(path, parameters, out data)) {
					handle = data;
					return true;
				}
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path.ToString());
			}
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool RegisterTexture(OpenBveApi.Textures.Texture texture, OpenBveApi.Textures.TextureParameters parameters, out OpenBveApi.Textures.TextureHandle handle) {
			texture = OpenBveApi.Textures.Texture.ApplyParameters(texture, parameters);
			Textures.Texture data;
			if (Textures.RegisterTexture(texture, out data)) {
				handle = data;
				return true;
			} else {
				handle = null;
				return false;
			}
		}
		
	}
}