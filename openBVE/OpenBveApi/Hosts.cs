using System;

namespace OpenBveApi.Hosts {
	
	/// <summary>Represents the host application and functionality it exposes.</summary>
	public abstract class Host{
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="text">The textual message that describes the problem.</param>
		public virtual void Report(string text) { }
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(Path.PathReference path, out Textures.Texture texture) {
			texture = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Path.PathReference path, out Textures.TextureHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Textures.Texture texture, out Textures.TextureHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool LoadSound(Path.PathReference path, out Sounds.Sound sound) {
			sound = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Path.PathReference path, out Sounds.SoundHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Sounds.Sound sound, out Sounds.SoundHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Loads an object and returns the object data.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool LoadObject(Path.PathReference path, out Objects.AbstractObject obj) {
			obj = null;
			return false;
		}
		
		/// <summary>Registers an object and returns a handle to the object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="handle">Receives a handle to the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool RegisterObject(Path.PathReference path, out Objects.ObjectHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers an object and returns a handle to the object.</summary>
		/// <param name="obj">The object data.</param>
		/// <param name="handle">Receives a handle to the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool RegisterObject(Objects.AbstractObject obj, out Objects.ObjectHandle handle) {
			handle = null;
			return false;
		}
		
	}

}