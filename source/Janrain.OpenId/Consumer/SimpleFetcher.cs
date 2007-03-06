namespace Janrain.OpenId.Consumer
{
	using System;
	using System.IO;
	using System.Net;

	[Serializable]
	public class SimpleFetcher : Fetcher
	{
		public SimpleFetcher()
		{
			throw new System.NotImplementedException();
		}

		public override FetchResponse Get(Uri uri, uint maxRead)
		{
			throw new System.NotImplementedException();
		}

		public override FetchResponse Post(Uri uri, byte[] body, uint maxRead)
		{
			throw new System.NotImplementedException();
		}
	}
}
