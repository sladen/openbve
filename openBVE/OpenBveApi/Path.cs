#pragma warning disable 0659, 0661

using System;
using System.Text;

namespace OpenBveApi {
	/// <summary>Provides path-related functions for accessing files and directories in a cross-platform manner.</summary>
	public static class Path {
		
		// --- path references ---
		
		/// <summary>Represents an abstract reference to a file or directory. Use FileReference or DirectoryReference to create instances.</summary>
		public abstract class PathReference {
			// --- operators ---
			/// <summary>Checks whether two path references are equal.</summary>
			/// <param name="a">The first path reference.</param>
			/// <param name="b">The second path reference.</param>
			/// <returns>Whether the two path references are equal.</returns>
			public static bool operator ==(PathReference a, PathReference b) {
				if (a is FileReference & b is FileReference) {
					return (FileReference)a == (FileReference)b;
				} else if (a is DirectoryReference & b is DirectoryReference) {
					return (DirectoryReference)a == (DirectoryReference)b;
				} else {
					return object.ReferenceEquals(a, b);
				}
			}
			/// <summary>Checks whether two path references are unequal.</summary>
			/// <param name="a">The first path reference.</param>
			/// <param name="b">The second path reference.</param>
			/// <returns>Whether the two path references are unequal.</returns>
			public static bool operator !=(PathReference a, PathReference b) {
				if (a is FileReference & b is FileReference) {
					return (FileReference)a != (FileReference)b;
				} else if (a is DirectoryReference & b is DirectoryReference) {
					return (DirectoryReference)a != (DirectoryReference)b;
				} else {
					return !object.ReferenceEquals(a, b);
				}
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (this is FileReference & obj is FileReference) {
					return (FileReference)this == (FileReference)obj;
				} else if (this is DirectoryReference & obj is DirectoryReference) {
					return (DirectoryReference)this == (DirectoryReference)obj;
				} else {
					return object.ReferenceEquals(this, obj);
				}
			}
			// --- functions ---
			/// <summary>Checks whether this path exists.</summary>
			/// <returns>Whether this path exists.</returns>
			public abstract bool Exists();
		}
		
		
		// --- file reference ---
		
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
			/// <remarks>No optional data is used and the default encoding is UTF-8.</remarks>
			public FileReference(string file) {
				this.File = file;
				this.Data = null;
				this.Encoding = Encoding.UTF8;
			}
			/// <summary>Creates a new file reference.</summary>
			/// <param name="file">The absolute path to the file.</param>
			/// <param name="data">Additional data that describes how to process the file, or a null reference.</param>
			/// <param name="encoding">The encoding that should be used to process textual data if no specific encoding is mandated.</param>
			public FileReference(string file, string data, Encoding encoding) {
				this.File = file;
				this.Data = data;
				this.Encoding = encoding;
			}
			// --- operators ---
			/// <summary>Checks whether two file references are equal.</summary>
			/// <param name="a">The first file reference.</param>
			/// <param name="b">The second file reference.</param>
			/// <returns>Whether the two file references are equal.</returns>
			public static bool operator ==(FileReference a, FileReference b) {
				if (object.ReferenceEquals(a, b)) return true;
				if (object.ReferenceEquals(a, null)) return false;
				if (object.ReferenceEquals(b, null)) return false;
				if (a.File != b.File) return false;
				if (a.Data != b.Data) return false;
				if (a.Encoding != b.Encoding) return false;
				return true;
			}
			/// <summary>Checks whether two file references are unequal.</summary>
			/// <param name="a">The first file reference.</param>
			/// <param name="b">The second file reference.</param>
			/// <returns>Whether the two file references are unequal.</returns>
			public static bool operator !=(FileReference a, FileReference b) {
				if (object.ReferenceEquals(a, b)) return false;
				if (object.ReferenceEquals(a, null)) return true;
				if (object.ReferenceEquals(b, null)) return true;
				if (a.File != b.File) return true;
				if (a.Data != b.Data) return true;
				if (a.Encoding != b.Encoding) return true;
				return false;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (object.ReferenceEquals(this, obj)) return true;
				if (object.ReferenceEquals(this, null)) return false;
				if (object.ReferenceEquals(obj, null)) return false;
				if (!(obj is FileReference)) return false;
				FileReference x = (FileReference)obj;
				if (this.File != x.File) return true;
				if (this.Data != x.Data) return true;
				if (this.Encoding != x.Encoding) return true;
				return false;
			}
			// --- functions ---
			/// <summary>Checks whether the file represented by this reference exists.</summary>
			/// <returns>Whether the file represented by this reference exists.</returns>
			public override bool Exists() {
				return System.IO.File.Exists(this.File);
			}
			/// <summary>Gets a string representation for this path.</summary>
			/// <returns>The string representation for this path.</returns>
			public override string ToString() {
				return this.File;
			}
		}
		
		
		// --- directory reference ---
		
		/// <summary>Represents a reference to a directory.</summary>
		public class DirectoryReference : PathReference {
			// --- members ---
			/// <summary>The absolute path to the directory.</summary>
			public string Directory;
			/// <summary>Additional data that describes how to process the directory, or a null reference.</summary>
			public string Data;
			/// <summary>The encoding that should be used to process textual data if no specific encoding is mandated.</summary>
			public Encoding Encoding;
			// --- constructors ---
			/// <summary>Creates a new directory reference.</summary>
			/// <param name="directory">The absolute path to the directory.</param>
			/// <remarks>No optional data is used and the default encoding is UTF-8.</remarks>
			public DirectoryReference(string directory) {
				this.Directory = directory;
				this.Data = null;
				this.Encoding = Encoding.UTF8;
			}
			/// <summary>Creates a new directory reference.</summary>
			/// <param name="directory">The absolute path to the directory.</param>
			/// <param name="data">Additional data that describes how to process the directory, or a null reference.</param>
			/// <param name="encoding">The encoding that should be used to process textual data if no specific encoding is mandated.</param>
			public DirectoryReference(string directory, string data, Encoding encoding) {
				this.Directory = directory;
				this.Data = data;
				this.Encoding = encoding;
			}
			// --- operators ---
			/// <summary>Checks whether two directory references are equal.</summary>
			/// <param name="a">The first directory reference.</param>
			/// <param name="b">The second directory reference.</param>
			/// <returns>Whether the two directory references are equal.</returns>
			public static bool operator ==(DirectoryReference a, DirectoryReference b) {
				if (object.ReferenceEquals(a, b)) return true;
				if (object.ReferenceEquals(a, null)) return false;
				if (object.ReferenceEquals(b, null)) return false;
				if (a.Directory != b.Directory) return false;
				if (a.Data != b.Data) return false;
				if (a.Encoding != b.Encoding) return false;
				return true;
			}
			/// <summary>Checks whether two directory references are unequal.</summary>
			/// <param name="a">The first directory reference.</param>
			/// <param name="b">The second directory reference.</param>
			/// <returns>Whether the two directory references are unequal.</returns>
			public static bool operator !=(DirectoryReference a, DirectoryReference b) {
				if (object.ReferenceEquals(a, b)) return false;
				if (object.ReferenceEquals(a, null)) return true;
				if (object.ReferenceEquals(b, null)) return true;
				if (a.Directory != b.Directory) return true;
				if (a.Data != b.Data) return true;
				if (a.Encoding != b.Encoding) return true;
				return false;
			}
			/// <summary>Checks whether this instance is equal to the specified object.</summary>
			/// <param name="obj">The object.</param>
			/// <returns>Whether this instance is equal to the specified object.</returns>
			public override bool Equals(object obj) {
				if (object.ReferenceEquals(this, obj)) return true;
				if (object.ReferenceEquals(this, null)) return false;
				if (object.ReferenceEquals(obj, null)) return false;
				if (!(obj is DirectoryReference)) return false;
				DirectoryReference x = (DirectoryReference)obj;
				if (this.Directory != x.Directory) return true;
				if (this.Data != x.Data) return true;
				if (this.Encoding != x.Encoding) return true;
				return false;
			}
			// --- functions ---
			/// <summary>Checks whether the directory represented by this reference exists.</summary>
			/// <returns>Whether the directory represented by this reference exists.</returns>
			public override bool Exists() {
				return System.IO.Directory.Exists(this.Directory);
			}
			/// <summary>Gets a string representation for this path.</summary>
			/// <returns>The string representation for this path.</returns>
			public override string ToString() {
				return this.Directory;
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
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths or due to unauthorized access.</exception>
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
							if (System.IO.Directory.Exists(absolute)) {
								string[] directories = System.IO.Directory.GetDirectories(absolute);
								for (int j = 0; j < directories.Length; j++) {
									string name = System.IO.Path.GetFileName(directories[j]);
									if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
										absolute = directories[j];
										found = true;
										break;
									}
								}
							}
							if (!found) {
								absolute = directory;
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
		/// <exception cref="System.Exception">Raised when combining the paths failed, for example due to malformed paths or due to unauthorized access.</exception>
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
							if (System.IO.Directory.Exists(absolute)) {
								string[] files = System.IO.Directory.GetFiles(absolute);
								for (int j = 0; j < files.Length; j++) {
									string name = System.IO.Path.GetFileName(files[j]);
									if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
										return files[j];
									}
								}
							}
							return file;
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
							if (System.IO.Directory.Exists(absolute)) {
								string[] directories = System.IO.Directory.GetDirectories(absolute);
								for (int j = 0; j < directories.Length; j++) {
									string name = System.IO.Path.GetFileName(directories[j]);
									if (name.Equals(parts[i], StringComparison.OrdinalIgnoreCase)) {
										absolute = directories[j];
										found = true;
										break;
									}
								}
							}
							if (!found) {
								absolute = directory;
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