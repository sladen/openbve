using System;

namespace OpenBveApi.Sound {
	
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
		/// <exception cref="System.ArgumentException">Raised when the bits per samples are neither 8 nor 16.</exception>
		public Sound(int sampleRate, int bitsPerSample, byte[][] bytes) {
			if (bitsPerSample != 8 & bitsPerSample != 16) {
				throw new ArgumentException();
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
	
	
	// --- interfaces ---
	
	/// <summary>Represents the interface for loading sounds.</summary>
	public interface ISound {
		
		/// <summary>Is called to check whether the plugin can load the specified sound.</summary>
		/// <param name="file">The file to the sound.</param>
		/// <param name="optional">Additional information that describes how to process the file, or a null reference.</param>
		/// <returns>Whether the plugin can load the specified sound.</returns>
		/// <remarks>The plugin should only inspect file extensions, identifiers or headers. It should not perform a full file validation.</remarks>
		bool CanLoadSound(string file, string optional);
		
		/// <summary>Is called to let the plugin load the specified sound.</summary>
		/// <param name="file">The file to the sound.</param>
		/// <param name="optional">Additional information that describes how to process the file, or a null reference.</param>
		/// <param name="sound">Receives the sound on success.</param>
		/// <returns>Whether the plugin succeeded in loading the sound.</returns>
		bool LoadSound(string file, string optional, out Sound sound);
		
	}
	
}