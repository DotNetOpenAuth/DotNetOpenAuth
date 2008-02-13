namespace DotNetOpenId.Consumer
{
	using System;
	using System.IO;
	using System.Net;

	[Serializable]
	internal class SimpleFetcher : Fetcher
	{
		public SimpleFetcher()
		{
		}

		public override FetchResponse Get(Uri uri, int maxBytesToRead)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
			request.KeepAlive = false;
			request.Method = "GET";
			request.MaximumAutomaticRedirections = 10;

			FetchResponse fresp = null;
			HttpWebResponse response;
			string message = null;

			try
			{
				response = (HttpWebResponse)request.GetResponse();
				try
				{
					fresp = GetResponse(response, maxBytesToRead);

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
						fresp = GetResponse(response, maxBytesToRead);
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

		public override FetchResponse Post(Uri uri, byte[] body, int maxBytesToRead)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
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

				response = (HttpWebResponse)request.GetResponse();
				try
				{
					fresp = GetResponse(response, maxBytesToRead);
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
						fresp = GetResponse(response, maxBytesToRead);
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
