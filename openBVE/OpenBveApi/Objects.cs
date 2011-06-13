using System;
using OpenBveApi.Geometry;

namespace OpenBveApi.Objects {
	
	// --- objects ---
	
	/// <summary>Represents an abstract object. This is the base class from which all objects must inherit.</summary>
	public abstract class AbstractObject {
		/// <summary>Translates the object by the specified offset.</summary>
		/// <param name="offset">The offset by which to translate.</param>
		public abstract void Translate(Vector3 offset);
		/// <summary>Translates the object by the specified offset that is measured in the specified orientation.</summary>
		/// <param name="orientation">The orientation along which to translate.</param>
		/// <param name="offset">The offset measured in the specified orientation.</param>
		public abstract void Translate(Orientation3 orientation, Vector3 offset);
		/// <summary>Rotates the object around the specified axis.</summary>
		/// <param name="direction">The axis along which to rotate.</param>
		/// <param name="cosineOfAngle">The cosine of the angle by which to rotate.</param>
		/// <param name="sineOfAngle">The sine of the angle by which to rotate.</param>
		public abstract void Rotate(Vector3 direction, double cosineOfAngle, double sineOfAngle);
		/// <summary>Rotates the object from the default orientation into the specified orientation.</summary>
		/// <param name="orientation">The target orientation.</param>
		/// <remarks>The default orientation is X = {1, 0, 0), Y = {0, 1, 0} and Z = {0, 0, 1}.</remarks>
		public abstract void Rotate(Orientation3 orientation);
		/// <summary>Scales the object by the specified factor.</summary>
		/// <param name="factor">The factor by which to scale.</param>
		public abstract void Scale(Vector3 factor);
	}
	
	/// <summary>Represents an abstract static object. This is the base class from which all static objects must inherit.</summary>
	public abstract class StaticObject : AbstractObject { }
	
	/// <summary>Represents an abstract animated object. This is the base class from which all animated objects must inherit.</summary>
	public abstract class AnimatedObject : AbstractObject { }
	
	
	// --- materials ---
	
	/// <summary>Represents an abstract material. This is the base class from which all materials must inherit.</summary>
	public abstract class AbstractMaterial { }
	
	
	// --- glow ---
	
	/// <summary>Represents an abstract glow. This is the base class from which all glows must inherit.</summary>
	public abstract class AbstractGlow { }
	
	/// <summary>Represents an abstract orientational glow. This is the base class from which all orientational glows must inherit.</summary>
	/// <remarks>This type of glow computes the intensity as a function of the camera and object's position and orientation.</remarks>
	public abstract class OrientationalGlow : AbstractGlow {
		/// <summary>Gets the intensity of the glow.</summary>
		/// <param name="cameraPosition">The position of the camera.</param>
		/// <param name="cameraOrientation">The orientation of the camera.</param>
		/// <param name="objectPosition">The position of the object.</param>
		/// <param name="objectOrientation">The orientation of the object.</param>
		/// <returns>The intensity of the glow expressed as a value between 0 and 1.</returns>
		public abstract double GetIntensity(Vector3 cameraPosition, Orientation3 cameraOrientation, Vector3 objectPosition, Vector3 objectOrientation);
	}
	
	
	// --- handles ---
	
	/// <summary>Represents a handle to an object.</summary>
	public abstract class ObjectHandle { }
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading objects. Plugins must implement this interface if they wish to expose objects.</summary>
	public abstract class ObjectInterface {
		
		/// <summary>Called when the plugin is loaded.</summary>
		/// <param name="host">The host that loaded the plugin.</param>
		public virtual void Load(Hosts.HostInterface host) { }
		
		/// <summary>Called when the plugin is unloaded.</summary>
		public virtual void Unload() { }
		
		/// <summary>Checks whether the plugin can load the specified object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <returns>Whether the plugin can load the specified object.</returns>
		public abstract bool CanLoadObject(Path.PathReference path);
		
		/// <summary>Loads the specified object.</summary>
		/// <param name="path">The path to the file or folder that contains the object.</param>
		/// <param name="obj">Receives the object.</param>
		/// <returns>Whether loading the object was successful.</returns>
		public abstract bool LoadObject(Path.PathReference path, out Object obj);
		
	}
	
}