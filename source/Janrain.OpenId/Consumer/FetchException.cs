namespace Janrain.OpenId.Consumer
{
	using System;

	public class FetchException : ApplicationException
	{
		public readonly FetchResponse response;

		public FetchException(FetchResponse response, string message)
			: base(message)
		{
			this.response = response;
		}
	}
}
