using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenBve {
	/// <summary>Represents plugins loaded by the program.</summary>
	internal static class Plugins {
		
		// --- classes ---
		
		/// <summary>Represents a plugin.</summary>
		internal class Plugin {
			// --- members ---
			/// <summary>The plugin file.</summary>
			internal string File;
			/// <summary>The plugin title.</summary>
			internal string Title;
			/// <summary>The ITexture interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Textures.TextureInterface Texture;
			/// <summary>The ISound interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Sounds.SoundInterface Sound;
			/// <summary>The IObject interface exposed by the plugin, or a null reference.</summary>
			internal OpenBveApi.Objects.ObjectInterface Object;
			// --- constructors ---
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="file">The plugin file.</param>
			internal Plugin(string file) {
				this.File = file;
				this.Title = Path.GetFileName(file);
				this.Texture = null;
				this.Sound = null;
				this.Object = null;
			}
			// --- functions ---
			/// <summary>Loads all interface this plugin supports.</summary>
			internal void Load() {
				if (this.Texture != null) {
					this.Texture.Load(Program.CurrentHost);
				}
				if (this.Sound != null) {
					this.Sound.Load(Program.CurrentHost);
				}
				if (this.Object != null) {
					this.Object.Load(Program.CurrentHost);
				}
			}
			/// <summary>Unloads all interface this plugin supports.</summary>
			internal void Unload() {
				if (this.Texture != null) {
					this.Texture.Unload();
				}
				if (this.Sound != null) {
					this.Sound.Unload();
				}
				if (this.Object != null) {
					this.Object.Unload();
				}
			}
		}
		
		
		// --- members ---
		
		/// <summary>A list of all non-runtime plugins that are currently loaded, or a null reference.</summary>
		internal static Plugin[] LoadedPlugins = null;
		
		
		// --- functions ---

		/// <summary>Loads all non-runtime plugins.</summary>
		internal static void LoadPlugins() {
			UnloadPlugins();
			string folder = Program.FileSystem.GetDataFolder("Plugins");
			string[] files = Directory.GetFiles(folder);
			List<Plugin> list = new List<Plugin>();
			foreach (string file in files) {
				if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) {
					try {
						Plugin plugin = new Plugin(file);
						Assembly assembly = Assembly.LoadFile(file);
						Type[] types = assembly.GetTypes();
						foreach (Type type in types) {
							if (type.IsSubclassOf(typeof(OpenBveApi.Textures.TextureInterface))) {
								plugin.Texture = (OpenBveApi.Textures.TextureInterface)assembly.CreateInstance(type.FullName);
							}
							if (type.IsSubclassOf(typeof(OpenBveApi.Sounds.SoundInterface))) {
								plugin.Sound = (OpenBveApi.Sounds.SoundInterface)assembly.CreateInstance(type.FullName);
							}
							if (type.IsSubclassOf(typeof(OpenBveApi.Objects.ObjectInterface))) {
								plugin.Object = (OpenBveApi.Objects.ObjectInterface)assembly.CreateInstance(type.FullName);
							}
						}
						if (plugin.Texture != null | plugin.Sound != null | plugin.Object != null) {
							plugin.Load();
							list.Add(plugin);
						}
					} catch {
						// TODO //
					}
				}
			}
			LoadedPlugins = list.ToArray();
		}
		
		/// <summary>Unloads all non-runtime plugins.</summary>
		internal static void UnloadPlugins() {
			if (LoadedPlugins != null) {
				foreach (Plugin plugin in LoadedPlugins) {
					plugin.Unload();
				}
				LoadedPlugins = null;
			}
		}
		
	}
}