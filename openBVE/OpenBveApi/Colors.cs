using System;

namespace OpenBveApi.Objects {
	
	/// <summary>Represents a solid color.</summary>
	public struct Color3 {
		// --- members ---
		/// <summary>The red component.</summary>
		public byte R;
		/// <summary>The green component.</summary>
		public byte G;
		/// <summary>The blue component.</summary>
		public byte B;
		// --- constructors ---
		/// <summary>Creates a new solid color.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		public Color3(byte r, byte g, byte b) {
			this.R = r;
			this.G = g;
			this.B = b;
		}
		// --- read-only fields ---
		/// <summary>Represents a black color.</summary>
		public static readonly Color3 Black = new Color3(0, 0, 0);
		/// <summary>Represents a red color.</summary>
		public static readonly Color3 Red = new Color3(1, 0, 0);
		/// <summary>Represents a green color.</summary>
		public static readonly Color3 Green = new Color3(0, 1, 0);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color3 Blue = new Color3(0, 0, 1);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color3 Cyan = new Color3(0, 1, 1);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color3 Mangeta = new Color3(1, 0, 1);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color3 Yellow = new Color3(1, 1, 0);
		/// <summary>Represents a white color.</summary>
		public static readonly Color3 White = new Color3(1, 1, 1);
	}
	
	/// <summary>Represents a color with alpha component.</summary>
	public struct Color4 {
		// --- members ---
		/// <summary>The red component.</summary>
		public byte R;
		/// <summary>The green component.</summary>
		public byte G;
		/// <summary>The blue component.</summary>
		public byte B;
		/// <summary>The alpha component.</summary>
		public byte A;
		// --- constructors ---
		/// <summary>Creates a new color with alpha component.</summary>
		/// <param name="r">The red component.</param>
		/// <param name="g">The green component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="a">The alpha component.</param>
		public Color4(byte r, byte g, byte b, byte a) {
			this.R = r;
			this.G = g;
			this.B = b;
			this.A = a;
		}
		/// <summary>Creates a new color with alpha component.</summary>
		/// <param name="color">The solid color.</param>
		/// <param name="a">The alpha component.</param>
		public Color4(Color3 color, byte a) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = a;
		}
		/// <summary>Creates a new color with alpha component.</summary>
		/// <param name="color">The solid color.</param>
		/// <remarks>The alpha component is set to full opacity.</remarks>
		public Color4(Color3 color) {
			this.R = color.R;
			this.G = color.G;
			this.B = color.B;
			this.A = 255;
		}
		// --- read-only fields ---
		/// <summary>Represents a black color.</summary>
		public static readonly Color4 Black = new Color4(0, 0, 0, 255);
		/// <summary>Represents a red color.</summary>
		public static readonly Color4 Red = new Color4(1, 0, 0, 255);
		/// <summary>Represents a green color.</summary>
		public static readonly Color4 Green = new Color4(0, 1, 0, 255);
		/// <summary>Represents a blue color.</summary>
		public static readonly Color4 Blue = new Color4(0, 0, 1, 255);
		/// <summary>Represents a cyan color.</summary>
		public static readonly Color4 Cyan = new Color4(0, 1, 1, 255);
		/// <summary>Represents a magenta color.</summary>
		public static readonly Color4 Mangeta = new Color4(1, 0, 1, 255);
		/// <summary>Represents a yellow color.</summary>
		public static readonly Color4 Yellow = new Color4(1, 1, 0, 255);
		/// <summary>Represents a white color.</summary>
		public static readonly Color4 White = new Color4(1, 1, 1, 255);
	}
	
}