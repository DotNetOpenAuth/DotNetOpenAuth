using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace DotNetOpenAuth.Web.Clients
{
    internal sealed class WindowsLiveClient : OAuth2Client
    {
        private const string TokenEndpoint = "https://oauth.live.com/token";
        private const string AuthorizationEndpoint = "https://oauth.live.com/authorize";
        private readonly string _appId;
        private readonly string _appSecret;

        public WindowsLiveClient(string appId, string appSecret)
            : base("windowslive")
        {
            if (String.IsNullOrEmpty(appId))
            {
                throw new ArgumentNullException("appId");
            }

            if (String.IsNullOrEmpty("appSecret"))
            {
                throw new ArgumentNullException("appSecret");
            }

            _appId = appId;
            _appSecret = appSecret;
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            var builder = new UriBuilder(AuthorizationEndpoint);
            builder.AppendQueryArguments(new Dictionary<string, string>
            {
                { "client_id", _appId },
                { "scope", "wl.basic" },
                { "response_type", "code" },
                { "redirect_uri", returnUrl.ToString() }
            });

            return builder.Uri;
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("client_id={0}", _appId);
            builder.AppendFormat("&redirect_uri={0}", Uri.EscapeDataString(returnUrl.ToString()));
            builder.AppendFormat("&client_secret={0}", _appSecret);
            builder.AppendFormat("&code={0}", authorizationCode);
            builder.Append("&grant_type=authorization_code");

            WebRequest tokenRequest = WebRequest.Create(TokenEndpoint);
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.ContentLength = builder.Length;
            tokenRequest.Method = "POST";

            using (Stream requestStream = tokenRequest.GetRequestStream())
            {
                var writer = new StreamWriter(requestStream);
                writer.Write(builder.ToString());
                writer.Flush();
            }

            HttpWebResponse tokenResponse = (HttpWebResponse)tokenRequest.GetResponse();
            if (tokenResponse.StatusCode == HttpStatusCode.OK)
            {
                using (Stream responseStream = tokenResponse.GetResponseStream())
                {
                    var tokenData = JsonHelper.Deserialize<OAuth2AccessTokenData>(responseStream);
                    if (tokenData != null)
                    {
                        return tokenData.AccessToken;
                    }
                }
            }

            return null;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            WindowsLiveUserData graph;
            var request = WebRequest.Create("https://apis.live.net/v5.0/me?access_token=" + Uri.EscapeDataString(accessToken));
            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    graph = JsonHelper.Deserialize<WindowsLiveUserData>(responseStream);
                }
            }

            var userData = new Dictionary<string, string>();
            userData.AddItemIfNotEmpty("id", graph.Id);
            userData.AddItemIfNotEmpty("username", graph.Name);
            userData.AddItemIfNotEmpty("name", graph.Name);
            userData.AddItemIfNotEmpty("link", graph.Link == null ? null : graph.Link.ToString());
            userData.AddItemIfNotEmpty("gender", graph.Gender);
            userData.AddItemIfNotEmpty("firstname", graph.FirstName);
            userData.AddItemIfNotEmpty("lastname", graph.LastName);
            return userData;
        }
    }
}
