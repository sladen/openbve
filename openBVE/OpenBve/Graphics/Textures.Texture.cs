#pragma warning disable 0659, 0661

using System;
using System.Drawing;

namespace OpenBve {
	internal static partial class Textures {
		
		// --- texture ---

		/// <summary>Represents a texture.</summary>
		internal class Texture : OpenBveApi.Textures.TextureHandle {
			// --- members ---
			/// <summary>The origin where the texture can be loaded from.</summary>
			internal TextureOrigin Origin;
			/// <summary>Whether the texture is loaded and the OpenGL texture name is valid.</summary>
			internal bool Loaded;
			/// <summary>The OpenGL texture name. Only valid if the texture is loaded.</summary>
			internal int OpenGlTextureName;
			/// <summary>The width of the texture. Only valid if the texture is loaded.</summary>
			internal int Width;
			/// <summary>The height of the texture. Only valid if the texture is loaded.</summary>
			internal int Height;
			/// <summary>The type of transparency encountered in the texture. Only valid if the texture is loaded.</summary>
			internal OpenBveApi.Textures.TextureTransparencyType Transparency;
			/// <summary>Whether to ignore further attemps to load the texture after previous attempts have failed.</summary>
			internal bool Ignore;
			// --- constructors ---
			/// <summary>Creates a new texture.</summary>
			/// <param name="path">The path to the texture.</param>
			/// <param name="parameters">The parameters that specify how to process the texture.</param>
			internal Texture(OpenBveApi.Path.PathReference path, OpenBveApi.Textures.TextureParameters parameters) {
				this.Origin = new PathOrigin(path, parameters);
				this.Loaded = false;
			}
			/// <summary>Creates a new texture.</summary>
			/// <param name="bitmap">The System.Drawing.Bitmap that contains the texture.</param>
			internal Texture(Bitmap bitmap) {
				this.Origin = new BitmapOrigin(bitmap);
				this.Loaded = false;
			}
			/// <summary>Creates a new texture.</summary>
			/// <param name="texture">The texture raw data.</param>
			internal Texture(OpenBveApi.Textures.Texture texture) {
				this.Origin = new RawOrigin(texture);
				this.Loaded = false;
			}
			// --- operators ---
			/// <summary>Checks whether two textures are equal.</summary>
			/// <param name="a">The first texture.</param>
			/// <param name="b">The second texture.</param>
			/// <returns>Whether the two textures are equal.</returns>
			public static bool operator ==(Texture a, Texture b) {
				if (object.ReferenceEquals(a, b)) return true;
				if (object.ReferenceEquals(a, null)) return false;
				if (object.ReferenceEquals(b, null)) return false;
				return a.Origin == b.Origin;
			}
			/// <summary>Checks whether two textures are unequal.</summary>
			/// <param name="a">The first texture.</param>
			/// <param name="b">The second texture.</param>
			/// <returns>Whether the two textures are unequal.</returns>
			public static bool operator !=(Texture a, Texture b) {
				if (object.ReferenceEquals(a, b)) return false;
				if (object.ReferenceEquals(a, null)) return true;
				if (object.ReferenceEquals(b, null)) return true;
				return a.Origin != b.Origin;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (object.ReferenceEquals(this, obj)) return true;
				if (object.ReferenceEquals(this, null)) return false;
				if (object.ReferenceEquals(obj, null)) return false;
				if (!(obj is Texture)) return false;
				return this.Origin == ((Texture)obj).Origin;
			}
		}

	}
}