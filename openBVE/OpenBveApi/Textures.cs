using System;

namespace OpenBveApi.Textures {
	
	// --- structures ---

	/// <summary>Represents a texture.</summary>
	public class Texture {
		// --- members ---
		/// <summary>The width of the texture in pixels.</summary>
		private int MyWidth;
		/// <summary>The height of the texture in pixels.</summary>
		private int MyHeight;
		/// <summary>The number of bits per pixel. Must be 32.</summary>
		private int MyBitsPerPixel;
		/// <summary>The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue, alpha.</summary>
		private byte[] MyBytes;
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="width">The width of the texture in pixels.</param>
		/// <param name="height">The height of the texture in pixels.</param>
		/// <param name="bitsPerPixel">The number of bits per pixel. Must be 32.</param>
		/// <param name="bytes">The texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue, alpha.</param>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per pixel is not 32.</exception>
		/// <exception cref="System.ArgumentNullException">Raised when the byte array is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the byte array is of unexpected length.</exception>
		public Texture(int width, int height, int bitsPerPixel, byte[] bytes) {
			if (bitsPerPixel != 32) {
				throw new ArgumentException("The number of bits per pixel is not 32.");
			} else if (bytes != null) {
				throw new ArgumentNullException("The data bytes are a null reference.");
			} else if (4 * width * height != bytes.Length) {
				throw new ArgumentException("The data bytes are not of the expected length.");
			} else {
				this.MyWidth = width;
				this.MyHeight = height;
				this.MyBitsPerPixel = bitsPerPixel;
				this.MyBytes = bytes;
			}
		}
		// --- properties ---
		/// <summary>Gets the width of the texture in pixels.</summary>
		public int Width {
			get {
				return this.MyWidth;
			}
		}
		/// <summary>Gets the height of the texture in pixels.</summary>
		public int Height {
			get {
				return this.MyHeight;
			}
		}
		/// <summary>Gets the number of bits per pixel.</summary>
		public int BitsPerPixel {
			get {
				return this.MyBitsPerPixel;
			}
		}
		/// <summary>Gets the texture data. Pixels are stored row-based from top to bottom, and within a row from left to right. For 32 bits per pixel, four bytes are used in the order red, green, blue, alpha.</summary>
		public byte[] Bytes {
			get {
				return this.MyBytes;
			}
		}
	}
	
	
	// --- handles ---
	
	/// <summary>Represents a handle to a texture.</summary>
	public abstract class TextureHandle { }
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading textures. Plugins must implement this interface if they wish to expose textures.</summary>
	public interface ITexture {
		
		/// <summary>Checks whether the plugin can load the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified texture.</returns>
		bool CanLoadTexture(Path.PathReference path);
		
		/// <summary>Loads the specified texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		bool LoadTexture(Path.PathReference path, out Texture texture);
		
	}
	
}