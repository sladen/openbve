using System;
using System.IO;
using System.Text;

namespace OpenBve {
	internal static class Tar {
		
		/// <summary>Extracts the content of a .tar, .tar.gz or .tgz file into a specified folder.</summary>
		/// <param name="file">The file to extract. If the file ends in .gz or .tgz, the gzip-compressed file is first decompressed.</param>
		/// <param name="folder">The folder to extract the content to.</param>
		public static void Unpack(string file, string folder) {
			if (file.EndsWith(".gz", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".tgz", StringComparison.OrdinalIgnoreCase)) {
				byte[] data = File.ReadAllBytes(file);
				byte[] uncompressed = Gzip.Decompress(data);
				Unpack(uncompressed, folder);
			} else {
				byte[] data = File.ReadAllBytes(file);
				Unpack(data, folder);
			}
		}
		
		/// <summary>Extracts the tar data into a specified folder.</summary>
		/// <param name="data">The tar data to extract.</param>
		/// <param name="folder">The folder to extract the content to.</param>
		private static void Unpack(byte[] data, string folder) {
			System.Text.ASCIIEncoding ascii = new ASCIIEncoding();
			System.Text.UTF8Encoding utf8 = new UTF8Encoding();
			int position = 0;
			while (position < data.Length) {
				string name = utf8.GetString(data, position, 100).TrimEnd('\0');
				if (name.Length == 0) {
					/*
					 * The name is empty. This marks the end of the file.
					 * */
					break;
				} else {
					/*
					 * Read the header and advance the position.
					 * */
					string sizeString = ascii.GetString(data, position + 124, 12).Trim('\0', ' ');
					int size = Convert.ToInt32(sizeString, 8);
					int mode;
					if (name[name.Length - 1] == '/') {
						mode = 53;
					} else {
						mode = (int)data[position + 156];
					}
					if (data[position + 257] == 0x75 && data[position + 258] == 0x73 && data[position + 259] == 0x74 && data[position + 260] == 0x61 && data[position + 261] == 0x72 && data[position + 262] == 0x00) {
						/*
						 * This is a POSIX ustar archive.
						 * */
						string namePrefix = utf8.GetString(data, position + 345, 155).TrimEnd(' ');
						if (namePrefix.Length != 0) {
							if (namePrefix[namePrefix.Length - 1] != '/' && name[0] != '/') {
								name = namePrefix + '/' + name;
							} else {
								name = namePrefix + name;
							}
						}
					} else if (data[position + 257] == 0x75 && data[position + 258] == 0x73 && data[position + 259] == 0x74 && data[position + 260] == 0x61 && data[position + 261] == 0x72 && data[position + 262] == 0x20) {
						/*
						 * This is a GNU tar archive.
						 * TODO: Implement support for GNU tar archives here.
						 * */
					}
					position += 512;
					/* 
					 * Process the data depending on the mode.
					 * */
					if (mode == 53) {
						/*
						 * This is a directory.
						 * */
						if (name[name.Length - 1] == '/') {
							name = name.Substring(0, name.Length - 1);
						}
						name = name.Replace('/', Path.DirectorySeparatorChar);
						try {
							Directory.CreateDirectory(Path.Combine(folder, name));
						} catch { }
					} else if (mode < 49 | mode > 54) {
						/*
						 * This is a normal file.
						 * */
						name = name.Replace('/', Path.DirectorySeparatorChar);
						int blocks = size + 511 >> 9;
						byte[] buffer = new byte[size];
						Array.Copy(data, position, buffer, 0, size);
						position += blocks << 9;
						string file = Path.Combine(folder, name);
						try {
							Directory.CreateDirectory(Path.GetDirectoryName(file));
						} catch { }
						if (File.Exists(file)) {
							try {
								File.SetAttributes(file, FileAttributes.Normal);
							} catch { }
						}
						File.WriteAllBytes(Path.Combine(folder, name), buffer);
					} else {
						/*
						 * Unsupported mode.
						 * */
					}
				}
			}
		}

	}
}