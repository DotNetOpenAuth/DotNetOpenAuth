namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	public interface IOAuthWebWorker {
		void RequestAuthentication(Uri callback);
		AuthorizedTokenResponse ProcessUserAuthorization();
		HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken);
	}
}