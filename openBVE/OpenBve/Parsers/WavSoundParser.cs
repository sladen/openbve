using System;
using System.IO;

namespace OpenBve {
	internal static class WaveParser {
		
		// wave format
		internal struct WaveFormat {
			// members
			internal uint SampleRate;
			internal uint BitsPerSample;
			internal ushort Channels;
			// constructors
			internal WaveFormat(uint SampleRate, uint BitsPerSample, ushort Channels) {
				this.SampleRate = SampleRate;
				this.BitsPerSample = BitsPerSample;
				this.Channels = Channels;
			}
			// operators
			public static bool operator ==(WaveFormat A, WaveFormat B) {
				if (A.SampleRate != B.SampleRate) return false;
				if (A.BitsPerSample != B.BitsPerSample) return false;
				if (A.Channels != B.Channels) return false;
				return true;
			}
			public static bool operator !=(WaveFormat A, WaveFormat B) {
				if (A.SampleRate != B.SampleRate) return true;
				if (A.BitsPerSample != B.BitsPerSample) return true;
				if (A.Channels != B.Channels) return true;
				return false;
			}
			public override bool Equals(object obj) {
				return base.Equals(obj);
			}
			public override int GetHashCode() {
				return base.GetHashCode();
			}
		}
		
		// wave data
		internal class WaveData {
			// members
			internal WaveFormat Format;
			internal byte[] Bytes;
			// constructors
			internal WaveData(WaveFormat Format, byte[] Bytes) {
				this.Format = Format;
				this.Bytes = Bytes;
			}
		}
		
		// load from file
		/// <summary>Reads wave data from a file.</summary>
		/// <param name="FileName">The file name of the WAVE file.</param>
		/// <returns>The wave data.</returns>
		internal static WaveData LoadFromFile(string FileName) {
			string fileTitle = Path.GetFileName(FileName);
			using (FileStream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// chunk (RIFF)
					uint chunkID = reader.ReadUInt32();
					if (chunkID != 0x46464952) {
						throw new InvalidDataException("Invalid chunk ID in " + fileTitle);
					}
					uint chunkSize = reader.ReadUInt32();
					uint riffFormat = reader.ReadUInt32();
					if (riffFormat != 0x45564157) {
						throw new InvalidDataException("Unsupported format in " + fileTitle);
					}
					// sub chunks
					WaveFormat format = new WaveFormat();
					byte[] bytes = null;
					while (stream.Position < stream.Length) {
						uint subChunkID = reader.ReadUInt32();
						uint subChunkSize = reader.ReadUInt32();
						if (subChunkID == 0x20746d66) {
							// "fmt " chunk
							if (subChunkSize != 16 & subChunkSize < 18) {
								throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
							}
							ushort audioFormat = reader.ReadUInt16();
							if (audioFormat != 1) {
								throw new InvalidDataException("Unsupported audioFormat in " + fileTitle);
							}
							ushort numChannels = reader.ReadUInt16();
							uint sampleRate = reader.ReadUInt32();
							uint byteRate = reader.ReadUInt32();
							ushort blockAlign = reader.ReadUInt16();
							ushort bitsPerSample = reader.ReadUInt16();
							if (bitsPerSample != 8 & bitsPerSample != 16) {
								throw new InvalidDataException("Unsupported bitsPerSample in " + fileTitle);
							}
							if (blockAlign != numChannels * bitsPerSample / 8) {
								throw new InvalidDataException("Unsupported blockAligm in " + fileTitle);
							}
							if (byteRate != sampleRate * (uint)numChannels * (uint)bitsPerSample / 8) {
								throw new InvalidDataException("Unsupported byteRate in " + fileTitle);
							}
							if (subChunkSize >= 18) {
								uint extraParamSize = reader.ReadUInt16();
								if (extraParamSize != subChunkSize - 18) {
									throw new InvalidDataException("Invalid extraParamSize in " + fileTitle);
								}
								byte[] extraParams = reader.ReadBytes((int)extraParamSize);
							}
							format.SampleRate = sampleRate;
							format.BitsPerSample = bitsPerSample;
							format.Channels = numChannels;
						} else if (subChunkID == 0x61746164) {
							// "data" chunk
							if (format.SampleRate == 0 | format.BitsPerSample == 0 | format.Channels == 0) {
								throw new InvalidDataException("No fmt chunk before data chunk in " + fileTitle);
							}
							if (subChunkSize >= 0x80000000) {
								throw new InvalidDataException("Unsupported data chunk size in " + fileTitle);
							}
							uint numSamples = 8 * subChunkSize / ((uint)format.Channels * (uint)format.BitsPerSample);
							bytes = reader.ReadBytes((int)subChunkSize);
							if ((subChunkSize & 1) == 1) {
								stream.Position++;
							}
						} else {
							// unsupported chunk
							stream.Position += (long)subChunkSize;
						}
					}
					// finalize
					if (bytes == null) {
						throw new InvalidDataException("No data chunk before the end of the file in " + fileTitle);
					}
					return new WaveData(format, bytes);
				}
			}
		}
		
		// convert to mono
		internal static WaveData ConvertToMono(WaveData input) {
			if (input.Format.Channels == 1) {
				return input;
			} else {
				int bytesPerSample = (int)(input.Format.BitsPerSample / 8);
				int samples = input.Bytes.Length / ((int)input.Format.Channels * bytesPerSample);
				byte[] bytes = new byte[samples * bytesPerSample];
				const int chosenChannel = 0;
				int to = 0;
				for (int i = 0; i < samples; i++) {
					int from = i * bytesPerSample * input.Format.Channels + chosenChannel * bytesPerSample;
					for (int j = 0; j < bytesPerSample; j++) {
						bytes[to] = input.Bytes[from + j];
						to++;
					}
				}
				WaveFormat format = new WaveFormat(input.Format.SampleRate, input.Format.BitsPerSample, 1);
				return new WaveData(format, bytes);
			}
		}
		
	}
}