#pragma warning disable 0660, 0661

using System;

namespace OpenBveApi.Geometry {
	/// <summary>Represents a two-dimensional vector.</summary>
	public struct Vector2 {
		
		// --- members ---
		
		/// <summary>The x-coordinate.</summary>
		public double X;
		
		/// <summary>The y-coordinate.</summary>
		public double Y;
		
		
		// --- constructors ---
		
		/// <summary>Creates a new two-dimensional vector.</summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		public Vector2(double x, double y) {
			this.X = x;
			this.Y = y;
		}
		
		
		// --- operators ---
		
		/// <summary>Checks whether the two specified vectors are equal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are equal.</returns>
		public static bool operator ==(Vector2 a, Vector2 b) {
			if (a.X != b.X) return false;
			if (a.Y != b.Y) return false;
			return true;
		}
		
		/// <summary>Checks whether the two specified vectors are unequal.</summary>
		/// <param name="a">The first vector.</param>
		/// <param name="b">The second vector.</param>
		/// <returns>Whether the two vectors are unequal.</returns>
		public static bool operator !=(Vector2 a, Vector2 b) {
			if (a.X != b.X) return true;
			if (a.Y != b.Y) return true;
			return false;
		}
		
		
		// --- read-only fields ---
		
		/// <summary>Represents a null vector.</summary>
		public static readonly Vector2 Null = new Vector2(0.0, 0.0);
		
		/// <summary>Represents vector pointing left.</summary>
		public static readonly Vector2 Left = new Vector2(-1.0, 0.0);
		
		/// <summary>Represents vector pointing right.</summary>
		public static readonly Vector2 Right = new Vector2(1.0, 0.0);
		
		/// <summary>Represents vector pointing up.</summary>
		public static readonly Vector2 Up = new Vector2(0.0, -1.0);
		
		/// <summary>Represents vector pointing down.</summary>
		public static readonly Vector2 Down = new Vector2(0.0, 1.0);
		
	}
}