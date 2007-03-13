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
		}

		public override FetchResponse Get(Uri uri, uint maxRead)
		{
			HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
			request.KeepAlive = false;
			request.Method = "GET";
			request.MaximumAutomaticRedirections = 10;

			FetchResponse fresp = null;
			HttpWebResponse response;
			string message = null;

			try
			{
				response = request.GetResponse() as HttpWebResponse;
				try
				{
					fresp = GetResponse(response, maxRead);

					if (response.StatusCode == HttpStatusCode.OK)
						return fresp;

					message = response.StatusCode.ToString();
				}
				finally
				{
					response.Close();
				}
			}
			catch (WebException e)
			{
				response = e.Response as HttpWebResponse;

				if (response != null)
				{
					try
					{
						fresp = GetResponse(response, maxRead);
					}
					finally
					{
						response.Close();
					}

					message = e.Message;
				}
			}

			throw new FetchException(fresp, message);
		}

		public override FetchResponse Post(Uri uri, byte[] body, uint maxRead)
		{
			HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
			request.ReadWriteTimeout = 20;
			request.KeepAlive = false;
			request.Method = "POST";
			request.MaximumAutomaticRedirections = 10;
			request.ContentLength = body.Length;
			request.ContentType = "application/x-www-form-urlencoded";

			FetchResponse fresp = null;

			HttpWebResponse response;
			string message = null;

			try
			{
				Stream outStream = request.GetRequestStream();
				outStream.Write(body, 0, body.Length);
				outStream.Close();

				response = request.GetResponse() as HttpWebResponse;
				try
				{
					fresp = GetResponse(response, maxRead);
					if (response.StatusCode == HttpStatusCode.OK)
						return fresp;

					message = response.StatusCode.ToString();
				}
				finally
				{
					response.Close();
				}
			}
			catch (WebException e)
			{
				response = e.Response as HttpWebResponse;

				if (response != null)
				{
					try
					{
						fresp = GetResponse(response, maxRead);
					}
					finally
					{
						response.Close();
					}
					message = e.Message;
				}
			}

			throw new FetchException(fresp, message);
		}
	}
}
