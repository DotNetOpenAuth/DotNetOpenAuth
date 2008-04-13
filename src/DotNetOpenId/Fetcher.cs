#if DEBUG
#define LONGTIMEOUT
#endif
namespace DotNetOpenId {
	using System;
	using System.Net;
	using System.IO;

	/// <summary>
	/// A paranoid HTTP get/post request engine.  It helps to protect against attacks from remote
	/// server leaving dangling connections, sending too much data, etc.
	/// </summary>
	internal static class Fetcher {
		/// <summary>
		/// The default maximum bytes to read in any given HTTP request.
		/// Default is 1MB.
		/// </summary>
		public static int MaximumBytesToRead = (1024 * 1024);
		/// <summary>
		/// The total number of redirections to allow on any one request.
		/// </summary>
		public static int MaximumRedirections = 10;
		/// <summary>
		/// Gets the time allowed to wait for single read or write operation to complete.
		/// </summary>
		public static TimeSpan ReadWriteTimeout = TimeSpan.FromMilliseconds(500);
		/// <summary>
		/// Gets the time allowed for an entire request.
		/// </summary>
		public static TimeSpan Timeout = TimeSpan.FromSeconds(5);

#if LONGTIMEOUT
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static Fetcher() {
			ReadWriteTimeout = TimeSpan.FromHours(1);
			Timeout = TimeSpan.FromHours(1);
		}
#endif

		/// <summary>
		/// Reads a maximum number of bytes from a response stream.
		/// </summary>
		/// <returns>
		/// The number of bytes actually read.  
		/// WARNING: This can be fewer than the size of the returned buffer.
		/// </returns>
		static void readData(HttpWebResponse resp, out byte[] buffer, out int length) {
			int bufferSize = resp.ContentLength >= 0 && resp.ContentLength < int.MaxValue ?
				Math.Min(MaximumBytesToRead, (int)resp.ContentLength) : MaximumBytesToRead;
			buffer = new byte[bufferSize];
			using (Stream stream = resp.GetResponseStream()) {
				int dataLength = 0;
				int chunkSize;
				while (dataLength < bufferSize && (chunkSize = stream.Read(buffer, dataLength, bufferSize - dataLength)) > 0)
					dataLength += chunkSize;
				length = dataLength;
			}
		}

		static FetchResponse getResponse(Uri requestUri, HttpWebResponse resp) {
			byte[] data;
			int length;
			readData(resp, out data, out length);
			return new FetchResponse(requestUri, resp, new MemoryStream(data, 0, length));
		}

		public static FetchResponse Request(Uri uri) {
			return Request(uri, null);
		}

		public static FetchResponse Request(Uri uri, byte[] body) {
			return Request(uri, body, null);
		}

		public static FetchResponse Request(Uri uri, byte[] body, string[] acceptTypes) {
			if (uri == null) throw new ArgumentNullException("uri");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.ReadWriteTimeout = (int)ReadWriteTimeout.TotalMilliseconds;
			request.Timeout = (int)Timeout.TotalMilliseconds;
			request.KeepAlive = false;
			request.MaximumAutomaticRedirections = MaximumRedirections;
			if (acceptTypes != null)
				request.Accept = string.Join(",", acceptTypes);
			if (body != null) {
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = body.Length;
				request.Method = "POST";
			}

			try {
				if (body != null) {
					using (Stream outStream = request.GetRequestStream()) {
						outStream.Write(body, 0, body.Length);
					}
				}

				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					return getResponse(uri, response);
				}
			} catch (WebException e) {
				using (HttpWebResponse response = (HttpWebResponse)e.Response) {
					if (response != null) {
						return getResponse(uri, response);
					} else {
						throw;
					}
				}
			}
		}
	}
}
