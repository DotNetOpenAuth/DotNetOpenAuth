namespace DotNetOpenId.Consumer
{
	using System;
	using System.Net;
	using System.IO;

	[Serializable]
	internal abstract class Fetcher
	{
		/// <summary>
		/// The default maximum bytes to read in any given HTTP request.
		/// Default is 1MB.
		/// </summary>
		public static int MaximumBytesToRead = (1024 * 1024);

		/// <summary>
		/// Reads a maximum number of bytes from a response stream.
		/// </summary>
		/// <returns>
		/// The number of bytes actually read.  
		/// WARNING: This can be fewer than the size of the returned buffer.
		/// </returns>
		protected static int ReadData(HttpWebResponse resp, int maximumBytesToRead, out byte[] buffer)
		{
			int bufferSize = resp.ContentLength >= 0 && resp.ContentLength < int.MaxValue ?
				Math.Min(maximumBytesToRead, (int)resp.ContentLength) : maximumBytesToRead;
			buffer = new byte[bufferSize];
			using (Stream stream = resp.GetResponseStream())
			{
				int dataLength = 0;
				int chunkSize;
				while (dataLength < bufferSize && (chunkSize = stream.Read(buffer, dataLength, bufferSize - dataLength)) > 0)
					dataLength += chunkSize;
				return dataLength;
			}
		}
		
		protected static FetchResponse GetResponse(HttpWebResponse resp, int maximumBytesToRead)
		{
			byte[] data;
			int length = ReadData(resp, maximumBytesToRead, out data);
			return new FetchResponse(resp.StatusCode, resp.ResponseUri,
					resp.CharacterSet, data, length);
		}

		public abstract FetchResponse Get(Uri uri, int maximumBytesToRead);
		
		public virtual FetchResponse Get(Uri uri)
		{
			return Get(uri, MaximumBytesToRead);
		}

		public abstract FetchResponse Post(Uri uri, byte[] body, int maximumBytesToRead);

		public virtual FetchResponse Post(Uri uri, byte[] body)
		{
			return Post(uri, body, MaximumBytesToRead);
		}
	}
}
