using System;
using System.IO;
using System.Reflection;

namespace OpenBve {
	/// <summary>Represents plugins loaded by the program.</summary>
	internal static class Plugins {
		
		internal class Plugin {
			// --- members ---
			/// <summary>The path to the plugin file.</summary>
			internal string File;
			/// <summary>The ITexture interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Textures.ITexture Texture;
			/// <summary>The ISound interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Sounds.ISound Sound;
			/// <summary>The IObject interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Objects.IObject Object;
			// --- constructors ---
			internal Plugin(string file) {
				this.File = file;
				this.Texture = null;
				this.Sound = null;
				this.Object = null;
			}
		}

		/// <summary>Loads all available plugins.</summary>
		internal static void LoadPlugins() {
			string folder = Program.FileSystem.GetDataFolder("Plugins");
			string[] files = Directory.GetFiles(folder);
			foreach (string file in files) {
				if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
					try {
						Plugin plugin = new Plugin(file);
						Assembly assembly = Assembly.LoadFile(file);
						Type[] types = assembly.GetTypes();
						foreach (Type type in types) {
							if (type.IsPublic && (type.Attributes & TypeAttributes.Abstract) == 0) {
								object instance = assembly.CreateInstance(type.FullName);
								if (instance is OpenBveApi.Textures.ITexture) {
									plugin.Texture = (OpenBveApi.Textures.ITexture)instance;
								} else if (instance is OpenBveApi.Sounds.ISound) {
									plugin.Sound = (OpenBveApi.Sounds.ISound)instance;
								} else if (instance is OpenBveApi.Objects.IObject) {
									plugin.Object = (OpenBveApi.Objects.IObject)instance;
								}
							}
						}
					} catch (Exception ex) {
						// TODO
					}
				}
			}
		}
		
	}
}