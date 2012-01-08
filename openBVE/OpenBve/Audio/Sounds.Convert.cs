using System;
using System.Collections.Generic;
using OpenBveApi.Sounds;

namespace OpenBve {
	internal static partial class Sounds {
		
		/// <summary>Mixes all channels in the specified sound to get a mono mix.</summary>
		/// <param name="sound">The sound.</param>
		/// <returns>The mono mix in the same format as the original.</returns>
		/// <exception cref="System.NotSupportedException">Raised when the bits per sample are not supported.</exception>
		private static byte[] GetMonoMix(Sound sound) {
			if (sound.Bytes.Length == 1) {
				// --- already mono ---
				return sound.Bytes[0];
			} else if (sound.BitsPerSample != 8 & sound.BitsPerSample != 16) {
				// --- format not supported ---
				throw new NotSupportedException();
			} else if (sound.BitsPerSample == 8) {
				// --- 8 bits per sample ---
				byte[] bytes = new byte[sound.Bytes[0].Length];
				for (int i = 0; i < sound.Bytes[0].Length; i++) {
					float mix = 0.0f;
					for (int j = 0; j < sound.Bytes.Length; j++) {
						float value = ((float)sound.Bytes[j][i] - 128.0f) / 128.0f;
						mix = Mix(mix, value);
					}
					int sample = (byte)(mix * 127.0f + 128.0f);
					bytes[i] = (byte)(sample & 0xFF);
				}
				return bytes;
			} else {
				// --- 16 bits per sample ---
				byte[] bytes = new byte[sound.Bytes[0].Length];
				for (int i = 0; i < sound.Bytes[0].Length; i += 2) {
					float mix = 0.0f;
					for (int j = 0; j < sound.Bytes.Length; j++) {
						float value = (float)(short)(ushort)(sound.Bytes[j][i] | (sound.Bytes[j][i + 1] << 8)) / 32768.0f;
						mix = Mix(mix, value);
					}
					int sample = (int)(ushort)(short)(32767.0f * mix);
					bytes[i] = (byte)(sample & 0xFF);
					bytes[i + 1] = (byte)(sample >> 8);
				}
				return bytes;
			}
		}
		
		/// <summary>Mixes two samples.</summary>
		/// <param name="a">The first sample in the range from -1.0 to 1.0.</param>
		/// <param name="b">The second sample in the range from -1.0 to 1.0.</param>
		/// <returns>The mixed sample in the range from -1.0 to 1.0.</returns>
		private static float Mix(float a, float b) {
			if (a < 0.0f & b < 0.0f) {
				return a + b + a * b;
			} else if (a > 0.0f & b > 0.0f) {
				return a + b - a * b;
			} else {
				return a + b;
			}
		}
		
	}
}