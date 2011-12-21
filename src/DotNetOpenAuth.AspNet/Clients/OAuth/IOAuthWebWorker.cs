using System;
using System.Net;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth.Messages;

namespace DotNetOpenAuth.AspNet.Clients
{
    public interface IOAuthWebWorker
    {
        void RequestAuthentication(Uri callback);
        AuthorizedTokenResponse ProcessUserAuthorization();
        HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken);
    }
}