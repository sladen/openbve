using System;
using System.Text;

namespace OpenBveApi {
	/// <summary>Provides path-related functions for accessing files and directories in a cross-platform manner.</summary>
	public static class Path {
		
		
		// --- path references ---
		
		/// <summary>Represents a reference to a file or folder.</summary>
		public abstract class PathReference { }
		
		/// <summary>Represents a reference to a file.</summary>
		public class FileReference : PathReference {
			// --- members ---
			/// <summary>The absolute path to the file.</summary>
			public string File;
			/// <summary>Additional data that describes how to process the file, or a null reference.</summary>
			public string Data;
			/// <summary>The encoding that should be used to process textual data if no specific encoding is mandated.</summary>
			public Encoding Encoding;
			// --- constructors ---
			/// <summary>Creates a new file reference.</summary>
			/// <param name="file">The absolute path to the file.</param>
			/// <param name="data">Additional data that describes how to process the file, or a null reference.</param>
			/// <param name="encoding">The encoding that should be used to process textual data if no specific encoding is mandated.</param>
			public FileReference(string file, string data, Encoding encoding) {
				this.File = file;
				this.Data = data;
				this.Encoding = encoding;
			}
		}
		
		/// <summary>Represents a reference to a folder.</summary>
		public class FolderReference : PathReference {
			// --- members ---
			/// <summary>The absolute path to the folder.</summary>
			public string File;
			/// <summary>Additional data that describes how to process the folder, or a null reference.</summary>
			public string Data;
			/// <summary>The encoding that should be used to process textual data if no specific encoding is mandated.</summary>
			public Encoding Encoding;
			// --- constructors ---
			/// <summary>Creates a new folder reference.</summary>
			/// <param name="file">The absolute path to the folder.</param>
			/// <param name="data">Additional data that describes how to process the folder, or a null reference.</param>
			/// <param name="encoding">The encoding that should be used to process textual data if no specific encoding is mandated.</param>
			public FolderReference(string file, string data, Encoding encoding) {
				this.File = file;
				this.Data = data;
				this.Encoding = encoding;
			}
		}
		
		
		// --- read-only fields ---
		
		/// <summary>The list of characters that are invalid in platform-independent relative paths.</summary>
		private static readonly char[] InvalidPathChars = new char[] { ':', '*', '?', '"', '<', '>', '|' };
		
		
		// --- public functions ---
		
		/// <summary>Combines a platform-specific absolute path with a platform-independent relative path that points to a directory.</summary>
		/// <param name="absolute">The platform-specific absolute path.</param>
		/// <param name="relative">The platform-independent relative path.</param>
		/// <returns>A platform-specific absolute path to the specified directory.</returns>
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths, due to unauthorized access, or when the specified directory could not be found.</exception>
		public static string CombineDirectory(string absolute, string relative) {
			if (relative.IndexOfAny(InvalidPathChars) >= 0) {
				throw new ArgumentException("The relative path contains invalid characters.");
			}
			string[] parts = relative.Split('/', '\\');
			for (int i = 0; i < parts.Length; i++) {
				if (parts[i].Length != 0) {
					/*
					 * Consider only non-empty parts.
					 * */
					if (IsAllPeriods(parts[i])) {
						/*
						 * A string of periods is a reference to an
						 * upper directory. A single period is the
						 * current directory. For each additional
						 * period, jump one directory up.
						 * */
						for (int j = 1; j < parts[i].Length; j++) {
							absolute = System.IO.Path.GetDirectoryName(absolute);
						}
					} else {
						/*
						 * This part references a directory.
						 * */
						string directory = System.IO.Path.Combine(absolute, parts[i]);
						if (System.IO.Directory.Exists(directory)) {
							absolute = directory;
						} else {
							/*
							 * Try to find the directory case-insensitively.
							 * */
							bool found = false;
							string[] directories = System.IO.Directory.GetDirectories(absolute);
							for (int j = 0; j < directories.Length; j++) {
								string name = System.IO.Path.GetFileName(directories[j]);
								if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
									absolute = directories[j];
									found = true;
									break;
								}
							}
							if (!found) {
								throw new System.IO.DirectoryNotFoundException("The specified directory could not be found.");
							}
						}
					}
				}
			}
			return absolute;
		}

		/// <summary>Combines a platform-specific absolute path with a platform-independent relative path that points to a file.</summary>
		/// <param name="absolute">The platform-specific absolute path.</param>
		/// <param name="relative">The platform-independent relative path.</param>
		/// <returns>Whether the operation succeeded and the specified file was found.</returns>
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths, due to unauthorized access, or when the specified file could not be found.</exception>
		public static string CombineFile(string absolute, string relative) {
			if (relative.IndexOfAny(InvalidPathChars) >= 0) {
				throw new ArgumentException("The relative path contains invalid characters.");
			}
			string[] parts = relative.Split('/', '\\');
			for (int i = 0; i < parts.Length; i++) {
				if (parts[i].Length != 0) {
					/*
					 * Consider only non-empty parts.
					 * */
					if (IsAllPeriods(parts[i])) {
						if (i == parts.Length - 1) {
							/*
							 * The last part must not be all periods because
							 * it would reference a directory then, not a file.
							 * */
							throw new ArgumentException("The relative path is malformed.");
						} else {
							/*
							 * A string of periods is a reference to an
							 * upper directory. A single period is the
							 * current directory. For each additional
							 * period, jump one directory up.
							 * */
							for (int j = 1; j < parts[i].Length; j++) {
								absolute = System.IO.Path.GetDirectoryName(absolute);
							}
						}
					} else if (i == parts.Length - 1) {
						/*
						 * The last part references a file.
						 * */
						string file = System.IO.Path.Combine(absolute, parts[i]);
						if (System.IO.File.Exists(file)) {
							return file;
						} else {
							/*
							 * Try to find the file case-insensitively.
							 * */
							string[] files = System.IO.Directory.GetFiles(absolute);
							for (int j = 0; j < files.Length; j++) {
								string name = System.IO.Path.GetFileName(files[j]);
								if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
									return files[j];
								}
							}
							throw new System.IO.FileNotFoundException("The specified file could not be found.");
						}
					} else {
						/*
						 * This part references a directory.
						 * */
						string directory = System.IO.Path.Combine(absolute, parts[i]);
						if (System.IO.Directory.Exists(directory)) {
							absolute = directory;
						} else {
							/*
							 * Try to find the directory case-insensitively.
							 * */
							bool found = false;
							string[] directories = System.IO.Directory.GetDirectories(absolute);
							for (int j = 0; j < directories.Length; j++) {
								string name = System.IO.Path.GetFileName(directories[j]);
								if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
									absolute = directories[j];
									found = true;
									break;
								}
							}
							if (!found) {
								throw new System.IO.DirectoryNotFoundException("The specified directory could not be found.");
							}
						}
					}
				}
			}
			throw new ArgumentException("The reference to the file is malformed.");
		}
		
		
		// --- private functions ---
		
		/// <summary>Checks whether the specified string consists only of periods.</summary>
		/// <param name="text">The string to check.</param>
		/// <returns>Whether the string consists only of periods.</returns>
		private static bool IsAllPeriods(string text) {
			for (int i = 0; i < text.Length; i++) {
				if (text[i] != '.') {
					return false;
				}
			}
			return true;
		}
		
	}
}