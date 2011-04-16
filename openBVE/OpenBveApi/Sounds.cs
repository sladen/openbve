using System;

namespace OpenBveApi.Sounds {
	
	// --- structures ---
	
	/// <summary>Represents a sound.</summary>
	public class Sound {
		// --- members ---
		/// <summary>The number of samples per second.</summary>
		private int MySampleRate;
		/// <summary>The number of bits per sample. Allowed values are 8 or 16.</summary>
		private int MyBitsPerSample;
		/// <summary>The PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</summary>
		private byte[][] MyBytes;
		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="sampleRate">The number of samples per second.</param>
		/// <param name="bitsPerSample">The number of bits per sample. Allowed values are 8 or 16.</param>
		/// <param name="bytes">The PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</param>
		/// <exception cref="System.ArgumentNullException">Raised when the bytes array or any of its subarrays is a null reference.</exception>
		/// <exception cref="System.ArgumentException">Raised when the bytes' subarrays are of unequal length.</exception>
		/// <exception cref="System.ArgumentException">Raised when the number of bits per samples is neither 8 nor 16.</exception>
		public Sound(int sampleRate, int bitsPerSample, byte[][] bytes) {
			if (bytes == null) {
				throw new ArgumentNullException("The data bytes are a null reference.");
			}
			for (int i = 0; i < bytes.Length; i++) {
				if (bytes[i] == null) {
					throw new ArgumentNullException("The data bytes of a particular channel is a null reference.");
				}
			}
			for (int i = 1; i < bytes.Length; i++) {
				if (bytes[i].Length != bytes[0].Length) {
					throw new ArgumentException("The data bytes of the channels are of unequal length.");
				}
			}
			if (bitsPerSample != 8 & bitsPerSample != 16) {
				throw new ArgumentException("The number of bits per sample is neither 8 nor 16.");
			} else {
				this.MySampleRate = sampleRate;
				this.MyBitsPerSample = bitsPerSample;
				this.MyBytes = bytes;
			}
		}
		// --- properties ---
		/// <summary>Gets the number of samples per second.</summary>
		public int SampleRate {
			get {
				return this.MySampleRate;
			}
		}
		/// <summary>Gets the number of bits per sample. Allowed values are 8 or 16.</summary>
		public int BitsPerSample {
			get {
				return this.MyBitsPerSample;
			}
		}
		/// <summary>Gets the PCM sound data per channel. For 8 bits per sample, samples are unsigned from 0 to 255. For 16 bits per sample, samples are signed from -32768 to 32767 and in little endian byte order.</summary>
		public byte[][] Bytes {
			get {
				return this.MyBytes;
			}
		}
	}
	
	
	// --- handles ---
	
	/// <summary>Represents a handle to a sound.</summary>
	public abstract class SoundHandle { }
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading sounds. Plugins must implement this interface if they wish to expose sounds.</summary>
	public interface ISound {
		
		/// <summary>Checks whether the plugin can load the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		bool CanLoadTexture(Path.PathReference path);
		
		/// <summary>Loads the specified sound.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		bool LoadTexture(Path.PathReference path, out Sound sound);
		
	}
	
}