#if DEBUG
#define LONGTIMEOUT
#endif
namespace DotNetOpenId.Consumer
{
	using System;
	using System.Net;
	using System.IO;

	/// <summary>
	/// A paranoid HTTP get/post request engine.  It helps to protect against attacks from remote
	/// server leaving dangling connections, sending too much data, etc.
	/// </summary>
	internal static class Fetcher
	{
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
		static void readData(HttpWebResponse resp, out byte[] buffer, out int length)
		{
			int bufferSize = resp.ContentLength >= 0 && resp.ContentLength < int.MaxValue ?
				Math.Min(MaximumBytesToRead, (int)resp.ContentLength) : MaximumBytesToRead;
			buffer = new byte[bufferSize];
			using (Stream stream = resp.GetResponseStream())
			{
				int dataLength = 0;
				int chunkSize;
				while (dataLength < bufferSize && (chunkSize = stream.Read(buffer, dataLength, bufferSize - dataLength)) > 0)
					dataLength += chunkSize;
				length = dataLength;
			}
		}
		
		static FetchResponse getResponse(HttpWebResponse resp)
		{
			byte[] data;
			int length;
			readData(resp, out data, out length);
			return new FetchResponse(resp.StatusCode, resp.ResponseUri, resp.CharacterSet, data, length);
		}

		public static FetchResponse Request(Uri uri) {
			return Request(uri, null);
		}

		public static FetchResponse Request(Uri uri, byte[] body)
		{
			if (uri == null) throw new ArgumentNullException("uri");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.ReadWriteTimeout = (int)ReadWriteTimeout.TotalMilliseconds;
			request.Timeout = (int)Timeout.TotalMilliseconds;
			request.KeepAlive = false;
			request.MaximumAutomaticRedirections = MaximumRedirections;
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

				using(HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					return getResponse(response);
				}
			} catch (WebException e) {
				using (HttpWebResponse response = (HttpWebResponse)e.Response) {
					if (response != null) {
						return getResponse(response);
					} else {
						throw;
					}
				}
			}
		}

	}
}
