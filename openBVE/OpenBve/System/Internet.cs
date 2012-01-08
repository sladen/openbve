using System;
using System.IO;
using System.Net;
using System.Threading;

namespace OpenBve {
	/// <summary>Provides methods for accessing the internet.</summary>
	internal static class Internet {
		
		/// <summary>Adds some user agent and referer to the web client headers.</summary>
		/// <param name="client">The web client.</param>
		/// <param name="url">The URL to be accessed.</param>
		private static void AddWebClientHeaders(WebClient client, string url) {
			const string agent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.6; rv:9.0) Gecko/20100101 Firefox/9.0";
			try {
				client.Headers.Add(HttpRequestHeader.UserAgent, agent);
			} catch { }
			if (url.StartsWith("http://")) {
				int index = url.IndexOf('/', 7);
				if (index >= 7) {
					string referer = url.Substring(0, index + 1);
					try {
						client.Headers.Add(HttpRequestHeader.Referer, referer);
					} catch { }
				}
			}
		}
		
		/// <summary>Downloads data from the specified URL.</summary>
		/// <param name="url">The URL.</param>
		/// <returns>The data.</returns>
		internal static byte[] DownloadBytesFromUrl(string url) {
			byte[] bytes;
			using (WebClient client = new WebClient()) {
				AddWebClientHeaders(client, url);
				bytes = client.DownloadData(url);
			}
			return bytes;
		}
		
		/// <summary>Downloads data from the specified URL.</summary>
		/// <param name="url">The URL.</param>
		/// <param name="bytes">Receives the data.</param>
		/// <param name="size">Accumulates the size of the downloaded data in an interlocked operation. If the operation fails, all accumulated size is subtracted again.</param>
		/// <returns>Whether the operation was successful.</returns>
		internal static bool TryDownloadBytesFromUrl(string url, out byte[] bytes, ref int size) {
			int count = 0;
			try {
				using (WebClient client = new WebClient()) {
					AddWebClientHeaders(client, url);
					using (Stream stream = client.OpenRead(url)) {
						const int chunkSize = 65536;
						bytes = new byte[chunkSize];
						while (true) {
							if (count + chunkSize >= bytes.Length) {
								Array.Resize<byte>(ref bytes, bytes.Length << 1);
							}
							int read = stream.Read(bytes, count, chunkSize);
							if (read != 0) {
								count += read;
								Interlocked.Add(ref size, read);
							} else {
								break;
							}
						}
					}
				}
				Array.Resize<byte>(ref bytes, count);
				return true;
			} catch {
				Interlocked.Add(ref size, -count);
				bytes = null;
				return false;
			}
		}
		
		/// <summary>Downloads bytes from the specified URL and saves them to a file.</summary>
		/// <param name="url">The URL.</param>
		/// <param name="file">The file name.</param>
		/// <param name="days">If the file already exists and was modified during the last so and so days, the download will be bypassed.</param>
		/// <param name="callback">The function to execute once the data has been saved to the file, or a null reference. The argument in the callback function is of type System.String and contains the file name.</param>
		internal static void DownloadAndSaveAsynchronous(string url, string file, double days, ParameterizedThreadStart callback) {
			bool download;
			if (File.Exists(file)) {
				try {
					DateTime lastWrite = File.GetLastWriteTime(file);
					TimeSpan span = DateTime.Now - lastWrite;
					download = span.TotalDays > days;
				} catch {
					download = true;
				}
			} else {
				download = true;
			}
			if (download) {
				ThreadStart start = new ThreadStart(
					() => {
						try {
							byte[] bytes = DownloadBytesFromUrl(url);
							string directory = Path.GetDirectoryName(file);
							try {
								Directory.CreateDirectory(directory);
								File.WriteAllBytes(file, bytes);
							} catch { }
							if (callback != null) {
								callback.Invoke(file);
							}
						} catch { }
					}
				);
				Thread thread = new Thread(start);
				thread.IsBackground = true;
				thread.Start();
			} else if (callback != null) {
				callback.Invoke(file);
			}
		}
		
	}
}