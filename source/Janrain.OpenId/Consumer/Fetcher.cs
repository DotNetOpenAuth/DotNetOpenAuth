namespace Janrain.OpenId.Consumer
{
	using System;
	using System.Net;
	using System.IO;

	[Serializable]
	public abstract class Fetcher
	{
		// Fields
		public static uint MAX_BYTES;

		// Methods
		static Fetcher() { throw new NotImplementedException(); }
		public Fetcher() { throw new NotImplementedException(); }
		public virtual FetchResponse Get(Uri uri) { throw new NotImplementedException(); }
		public abstract FetchResponse Get(Uri uri, uint maxRead);
		protected static FetchResponse GetResponse(HttpWebResponse resp, uint maxRead) { throw new NotImplementedException(); }
		public virtual FetchResponse Post(Uri uri, byte[] body) { throw new NotImplementedException(); }
		public abstract FetchResponse Post(Uri uri, byte[] body, uint maxRead);
		protected static int ReadData(HttpWebResponse resp, uint max_bytes, ref byte[] buffer) { throw new NotImplementedException(); }
	}
}
