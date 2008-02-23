namespace DotNetOpenId.Consumer
{
	using System;

	internal class FetchException : Exception
	{
		public readonly FetchResponse response;

		public FetchException(FetchResponse response, string message)
			: base(message)
		{
			this.response = response;
		}
	}
}
