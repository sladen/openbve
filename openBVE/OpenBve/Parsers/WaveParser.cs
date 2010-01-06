using System;
using System.IO;

namespace OpenBve {
	internal static class WaveParser {
		
		// --- structures and enumerations ---
		
		/// <summary>Represents the format of wave data.</summary>
		internal struct WaveFormat {
			// members
			/// <summary>The number of samples per second per channel.</summary>
			internal int SampleRate;
			/// <summary>The number of bits per sample.</summary>
			internal int BitsPerSample;
			/// <summary>The number of channels.</summary>
			internal int Channels;
			// constructors
			/// <summary>Creates a new instance of this structure.</summary>
			/// <param name="sampleRate">The number of samples per second per channel.</param>
			/// <param name="bitsPerSample">The number of bits per sample.</param>
			/// <param name="channels">The number of channels.</param>
			internal WaveFormat(int sampleRate, int bitsPerSample, int channels) {
				this.SampleRate = sampleRate;
				this.BitsPerSample = bitsPerSample;
				this.Channels = channels;
			}
			// operators
			public static bool operator ==(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return false;
				if (a.BitsPerSample != b.BitsPerSample) return false;
				if (a.Channels != b.Channels) return false;
				return true;
			}
			public static bool operator !=(WaveFormat a, WaveFormat b) {
				if (a.SampleRate != b.SampleRate) return true;
				if (a.BitsPerSample != b.BitsPerSample) return true;
				if (a.Channels != b.Channels) return true;
				return false;
			}
			public override bool Equals(object obj) {
				return base.Equals(obj);
			}
			public override int GetHashCode() {
				return base.GetHashCode();
			}
		}
		
		/// <summary>Represents wave data.</summary>
		internal class WaveData {
			// members
			/// <summary>The format of the wave data.</summary>
			internal WaveFormat Format;
			/// <summary>The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</summary>
			internal byte[] Bytes;
			// constructors
			/// <summary>Creates a new instance of this class.</summary>
			/// <param name="format">The format of the wave data.</param>
			/// <param name="bytes">The wave data in little endian byte order. If the bits per sample are not a multiple of 8, each sample is padded into a multiple-of-8 byte. For bytes per sample higher than 1, the values are stored as signed integers, otherwise as unsigned integers.</param>
			internal WaveData(WaveFormat format, byte[] bytes) {
				this.Format = format;
				this.Bytes = bytes;
			}
		}
		
		/// <summary>Represents the endianness of an integer.</summary>
		private enum Endianness {
			/// <summary>Represents little endian byte order, i.e. least-significant byte first.</summary>
			Little = 0,
			/// <summary>Represents big endian byte order, i.e. most-significant byte first.</summary>
			Big = 1
		}
		
		/// <summary>Represents the data format within the WAV's data chunk.</summary>
		private enum DataFormat {
			/// <summary>The format is invalid or has not yet been initialized.</summary>
			Invalid = 0,
			/// <summary>The format is PCM.</summary>
			Pcm = 1,
			/// <summary>The format is Microsoft ADPCM.</summary>
			MicrosoftAdPcm = 2
		}
		
		
		// --- members ---
		
		private static uint[] MicrosoftAdPcmAdaptionTable = new uint[] {
			230, 230, 230, 230, 307, 409, 512, 614,
			768, 614, 512, 409, 307, 230, 230, 230
		};
		
		
		// --- functions ---
		
		/// <summary>Reads wave data from a RIFF/WAVE/PCM file.</summary>
		/// <param name="fileName">The file name of the RIFF/WAVE/PCM file.</param>
		/// <returns>The wave data.</returns>
		/// <remarks>Both RIFF and RIFX container formats are supported by this function.</remarks>
		internal static WaveData LoadFromFile(string fileName) {
			string fileTitle = Path.GetFileName(fileName);
			byte[] fileBytes = File.ReadAllBytes(fileName);
			using (MemoryStream stream = new MemoryStream(fileBytes)) {
				using (BinaryReader reader = new BinaryReader(stream)) {
					// RIFF/RIFX chunk
					Endianness endianness;
					uint headerCkID = reader.ReadUInt32(); /* Chunk ID is character-based */
					if (headerCkID == 0x46464952) {
						endianness = Endianness.Little;
					} else if (headerCkID == 0x58464952) {
						endianness = Endianness.Big;
					} else {
						throw new InvalidDataException("Invalid chunk ID in " + fileTitle);
					}
					uint headerCkSize = ReadUInt32(reader, endianness);
					uint formType = ReadUInt32(reader, endianness);
					if (formType != 0x45564157) {
						throw new InvalidDataException("Unsupported format in " + fileTitle);
					}
					// data chunks
					WaveFormat format = new WaveFormat();
					DataFormat dataFormat = DataFormat.Invalid;
					byte[] dataBytes = null;
					long[][] microsoftAdPcmCoefficients = null;
					ushort microsoftAdPcmSamplesPerBlock = 0;
					int blockSize = 0;
					while (stream.Position + 8 <= stream.Length) {
						uint ckID = reader.ReadUInt32(); /* Chunk ID is character-based */
						uint ckSize = ReadUInt32(reader, endianness);
						if (ckID == 0x20746d66) {
							// "fmt " chunk
							if (ckSize < 14) {
								throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
							}
							ushort wFormatTag = ReadUInt16(reader, endianness);
							ushort wChannels = ReadUInt16(reader, endianness);
							uint dwSamplesPerSec = ReadUInt32(reader, endianness);
							if (dwSamplesPerSec >= 0x80000000) {
								throw new InvalidDataException("Unsupported dwSamplesPerSec in " + fileTitle);
							}
							uint dwAvgBytesPerSec = ReadUInt32(reader, endianness);
							ushort wBlockAlign = ReadUInt16(reader, endianness);
							blockSize = (int)wBlockAlign;
							if (wFormatTag == 1) {
								// PCM
								if (ckSize < 16) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								stream.Position += ckSize - 16;
								if (wBitsPerSample < 1) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								if (wBlockAlign != ((wBitsPerSample + 7) / 8) * wChannels) {
									throw new InvalidDataException("Unexpected wBlockAlign in " + fileTitle);
								}
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = (int)wBitsPerSample;
								format.Channels = (int)wChannels;
								dataFormat = DataFormat.Pcm;
							} else if (wFormatTag == 2) {
								// Microsoft ADPCM
								if (ckSize < 22) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								ushort wBitsPerSample = ReadUInt16(reader, endianness);
								if (wBitsPerSample != 4) {
									throw new InvalidDataException("Unsupported wBitsPerSample in " + fileTitle);
								}
								ushort cbSize = ReadUInt16(reader, endianness);
								microsoftAdPcmSamplesPerBlock = ReadUInt16(reader, endianness);
								if (microsoftAdPcmSamplesPerBlock == 0 | microsoftAdPcmSamplesPerBlock > 2 * ((int)wBlockAlign - 6)) {
									throw new InvalidDataException("Unexpected nSamplesPerBlock in " + fileTitle);
								}
								ushort wNumCoef = ReadUInt16(reader, endianness);
								if (ckSize < 22 + 4 * wNumCoef) {
									throw new InvalidDataException("Unsupported fmt chunk size in " + fileTitle);
								}
								microsoftAdPcmCoefficients = new long[wNumCoef][];
								for (int i = 0; i < wNumCoef; i++) {
									unchecked {
										microsoftAdPcmCoefficients[i] = new long[] {
											(long)ReadUInt16(reader, endianness),
											(long)ReadUInt16(reader, endianness)
										};
									}
								}
								stream.Position += ckSize - (22 + 4 * wNumCoef);
								format.SampleRate = (int)dwSamplesPerSec;
								format.BitsPerSample = 16;
								format.Channels = (int)wChannels;
								dataFormat = DataFormat.MicrosoftAdPcm;
							} else {
								// unsupported format
								throw new InvalidDataException("Unsupported wFormatTag in " + fileTitle);
							}
						} else if (ckID == 0x61746164) {
							// "data" chunk
							if (ckSize >= 0x80000000) {
								throw new InvalidDataException("Unsupported data chunk size in " + fileTitle);
							}
							if (dataFormat == DataFormat.Pcm) {
								// PCM
								int bytesPerSample = (format.BitsPerSample + 7) / 8;
								int samples = (int)ckSize / (format.Channels * bytesPerSample);
								int dataSize = samples * format.Channels * bytesPerSample;
								dataBytes = reader.ReadBytes(dataSize);
								stream.Position += ckSize - dataSize;
							} else if (dataFormat == DataFormat.MicrosoftAdPcm) {
								// Microsoft ADPCM
								if (format.Channels != 1) {
									throw new NotImplementedException("Unsupported wChannels for ADPCM in " + fileTitle);
								}
								int blocks = (int)ckSize / blockSize;
								dataBytes = new byte[2 * blocks * microsoftAdPcmSamplesPerBlock];
								int position = 0;
								for (int i = 0; i < blocks; i++) {
									unchecked {
										int bPredictor = (int)reader.ReadByte();
										if (bPredictor >= microsoftAdPcmCoefficients.Length) {
											throw new InvalidDataException("Invalid bPredictor in " + fileTitle);
										}
										ushort iDelta = ReadUInt16(reader, endianness);
										short iSamp1 = (short)ReadUInt16(reader, endianness);
										short iSamp2 = (short)ReadUInt16(reader, endianness);
										dataBytes[position] = (byte)(ushort)iSamp2;
										dataBytes[position + 1] = (byte)((ushort)iSamp2 >> 8);
										dataBytes[position + 2] = (byte)(ushort)iSamp1;
										dataBytes[position + 3] = (byte)((ushort)iSamp1 >> 8);
										position += 4;
										long iCoef1 = microsoftAdPcmCoefficients[bPredictor][0];
										long iCoef2 = microsoftAdPcmCoefficients[bPredictor][1];
										uint nibbleByte = 0;
										bool nibbleFirst = true;
										for (int j = 0; j < microsoftAdPcmSamplesPerBlock - 2; j++) {
											int lPredSample = (int)(((long)iSamp1 * iCoef1 + (long)iSamp2 * iCoef2) >> 8);
											int iErrorDeltaUnsigned;
											if (nibbleFirst) {
												nibbleByte = (uint)reader.ReadByte();
												iErrorDeltaUnsigned = (int)(nibbleByte >> 4);
												nibbleFirst = false;
											} else {
												iErrorDeltaUnsigned = (int)(nibbleByte & 15);
												nibbleFirst = true;
											}
											int iErrorDeltaSigned =
												iErrorDeltaUnsigned >= 8 ? iErrorDeltaUnsigned - 16 : iErrorDeltaUnsigned;
											int lNewSampInt = 
												lPredSample + (int)iDelta * iErrorDeltaSigned;
											short lNewSamp =
												lNewSampInt <= -32768 ? (short)-32768 :
												lNewSampInt >= 32767 ? (short)32767 :
												(short)lNewSampInt;
											iDelta =
												(ushort)((uint)iDelta * MicrosoftAdPcmAdaptionTable[iErrorDeltaUnsigned] >> 8);
											if (iDelta < 16) {
												iDelta = 16;
											}
											iSamp2 = iSamp1;
											iSamp1 = lNewSamp;
											dataBytes[position] = (byte)(ushort)lNewSamp;
											dataBytes[position + 1] = (byte)((ushort)lNewSamp >> 8);
											position += 2;
										}
									}
									stream.Position += blockSize - ((microsoftAdPcmSamplesPerBlock - 1 >> 1) + 7);
									System.IO.File.WriteAllBytes(@"C:\debug.dat", dataBytes);
								}
								stream.Position += (int)ckSize - blocks * blockSize;
							} else {
								// invalid
								throw new InvalidDataException("No fmt chunk before the data chunk in " + fileTitle);
							}
						} else {
							// unsupported chunk
							stream.Position += (long)ckSize;
						}
						// pad byte
						if ((ckSize & 1) == 1) {
							stream.Position++;
						}
					}
					// finalize
					if (dataBytes == null) {
						throw new InvalidDataException("No data chunk before the end of the file in " + fileTitle);
					} else {
						return new WaveData(format, dataBytes);
					}
				}
			}
		}
		
		/// <summary>Reads a System.UInt32 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt32 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static uint ReadUInt32(BinaryReader reader, Endianness endianness) {
			uint value = reader.ReadUInt32();
			if (endianness == Endianness.Big) {
				unchecked {
					return (value << 24) | (value & ((uint)0xFF00 << 8)) | ((value & (uint)0xFF0000) >> 8) | (value >> 24);
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Reads a System.UInt16 from a binary reader with the specified endianness.</summary>
		/// <param name="reader">The binary reader.</param>
		/// <param name="endianness">The endianness.</param>
		/// <returns>The System.UInt16 read from the reader.</returns>
		/// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
		/// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
		/// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
		private static ushort ReadUInt16(BinaryReader reader, Endianness endianness) {
			ushort value = reader.ReadUInt16();
			if (endianness == Endianness.Big) {
				unchecked {
					return (ushort)(((uint)value << 8) | ((uint)value >> 8));
				}
			} else {
				return value;
			}
		}
		
		/// <summary>Converts the specified wave data to 8-bit or 16-bit mono.</summary>
		/// <param name="data">The original wave data.</param>
		/// <returns>The wave data converted to 8-bit or 16-bit mono.</returns>
		/// <remarks>If the bits per sample per channel are less than or equal to 8, the result will be 8-bit mono, otherwise 16-bit mono.</remarks>
		internal static WaveData ConvertToMono8Or16(WaveData data) {
			if ((data.Format.BitsPerSample == 8 | data.Format.BitsPerSample == 16) & data.Format.Channels == 1) {
				// already in target format
				return data;
			} else if (data.Format.Channels != 1) {
				// convert to mono first
				int bytesPerSample = (data.Format.BitsPerSample + 7) / 8;
				int samples = data.Bytes.Length / (data.Format.Channels * bytesPerSample);
				byte[] bytes = new byte[samples * bytesPerSample];
				const int chosenChannel = 0;
				int to = 0;
				for (int i = 0; i < samples; i++) {
					int from = (i * data.Format.Channels + chosenChannel) * bytesPerSample;
					for (int j = 0; j < bytesPerSample; j++) {
						bytes[to] = data.Bytes[from + j];
						to++;
					}
				}
				WaveFormat format = new WaveFormat(data.Format.SampleRate, data.Format.BitsPerSample, 1);
				return ConvertToMono8Or16(new WaveData(format, bytes));
			} else if (data.Format.BitsPerSample < 8) {
				// less than 8 bits per sample
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 8, 1);
				return new WaveData(format, data.Bytes);
			} else if (data.Format.BitsPerSample < 16) {
				// between 9 and 15 bits per sample
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 16, 1);
				return new WaveData(format, data.Bytes);
			} else {
				// more than 16 bits per sample
				int bytesPerSample = (data.Format.BitsPerSample + 7) / 8;
				int samples = data.Bytes.Length / bytesPerSample;
				byte[] bytes = new byte[2 * samples];
				for (int i = 0; i < samples; i++) {
					int j = (i + 1) * bytesPerSample;
					bytes[2 * i] = data.Bytes[j - 2];
					bytes[2 * i + 1] = data.Bytes[j - 1];
				}
				WaveFormat format = new WaveFormat(data.Format.SampleRate, 16, 1);
				return new WaveData(format, bytes);
			}
		}
		
	}
}