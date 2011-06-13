using System;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;

namespace OpenBveApi.Hosts {

	/// <summary>Represents the type of problem that is reported to the host.</summary>
	public enum ProblemType {
		/// <summary>Indicates that a file could not be found.</summary>
		FileNotFound = 1,
		/// <summary>Indicates that a directory could not be found.</summary>
		DirectoryNotFound = 2,
		/// <summary>Indicates that a file or directory could not be found.</summary>
		PathNotFound = 3,
		/// <summary>Indicates invalid data in a file or directory.</summary>
		InvalidData = 4,
		/// <summary>Indicates an invalid operation.</summary>
		InvalidOperation = 5,
		/// <summary>Indicates an unexpected exception.</summary>
		UnexpectedException = 6
	}
	
	/// <summary>Represents the host application and functionality it exposes.</summary>
	public abstract class HostInterface {

		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public virtual void ReportProblem(ProblemType type, string text) { }
		
		/// <summary>Gets the fully-qualified path of the specified package.</summary>
		/// <param name="name">The name of the package.</param>
		/// <param name="path">Receives the fully-qualified path to the package.</param>
		/// <returns>Whether the package exists and the path was returned successfully.</returns>
		public virtual bool GetPackageDirectory(string name, out string path) {
			path = null;
			return false;
		}
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public virtual bool QueryTextureDimensions(Path.PathReference path, out int width, out int height) {
			width = 0;
			height = 0;
			return false;
		}
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool LoadTexture(Path.PathReference path, TextureParameters parameters, out Texture texture) {
			texture = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Path.PathReference path, TextureParameters parameters, out TextureHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public virtual bool RegisterTexture(Textures.Texture texture, TextureParameters parameters, out TextureHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool LoadSound(Path.PathReference path, out Sound sound) {
			sound = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Path.PathReference path, out SoundHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public virtual bool RegisterSound(Sounds.Sound sound, out SoundHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Loads an object and returns the object data.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool LoadObject(Path.PathReference path, out AbstractObject obj) {
			obj = null;
			return false;
		}
		
		/// <summary>Registers an object and returns a handle to the object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="handle">Receives a handle to the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool RegisterObject(Path.PathReference path, out ObjectHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Registers an object and returns a handle to the object.</summary>
		/// <param name="obj">The object data.</param>
		/// <param name="handle">Receives a handle to the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public virtual bool RegisterObject(Objects.AbstractObject obj, out ObjectHandle handle) {
			handle = null;
			return false;
		}
		
		/// <summary>Places an object in the world.</summary>
		/// <param name="handle">The handle to the object.</param>
		/// <param name="position">The position.</param>
		/// <param name="orientation">The orientation.</param>
		/// <returns>Whether the object was placed successfully.</returns>
		public virtual bool PlaceObject(ObjectHandle handle, Geometry.Vector3 position, Geometry.Orientation3 orientation) {
			return false;
		}

		/// <summary>Places an object in the world.</summary>
		/// <param name="obj">The object data.</param>
		/// <param name="position">The position.</param>
		/// <param name="orientation">The orientation.</param>
		/// <returns>Whether the object was placed successfully.</returns>
		public virtual bool PlaceObject(Objects.AbstractObject obj, Geometry.Vector3 position, Geometry.Orientation3 orientation) {
			return false;
		}

	}
}