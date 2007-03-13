namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Net;
	using System.IO;

	[Serializable]
	public abstract class Fetcher
	{
		public static uint MAX_BYTES = (1024 * 1024);

		// 1MB
		protected static int ReadData(HttpWebResponse resp, uint max_bytes, ref byte[] buffer)
		{
			MemoryStream ms = null;
			Stream stream = resp.GetResponseStream();
			int length = (int)resp.ContentLength;
			bool nolength = (length == (-1));
			int size = (nolength ? 8192 : length);
			if (nolength)
				ms = new MemoryStream();

			size = Math.Min(size, (int)max_bytes);
			int nread = 0;
			int offset = 0;
			buffer = new byte[size];
			while ((nread = stream.Read(buffer, offset, size)) != 0)
			{
				if (nolength)
					ms.Write(buffer, 0, nread);
				else
				{
					size -= nread;
					offset += nread;
				}
			}

			if (nolength)
			{
				buffer = ms.ToArray();
				offset = buffer.Length;
			}
			return offset;
		}
		
		protected static FetchResponse GetResponse(HttpWebResponse resp, uint maxRead)
		{
			byte[] data = null;
			int length = ReadData(resp, maxRead, ref data);
			return new FetchResponse(resp.StatusCode, resp.ResponseUri,
					resp.CharacterSet, data, length);
		}
		
		public abstract FetchResponse Get(Uri uri, uint maxRead);
		
		public virtual FetchResponse Get(Uri uri)
		{
			return Get(uri, MAX_BYTES);
		}

		public abstract FetchResponse Post(Uri uri, byte[] body, uint maxRead);

		public virtual FetchResponse Post(Uri uri, byte[] body)
		{
			return Post(uri, body, MAX_BYTES);
		}
	}
}
