using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using DotNetOpenAuth;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace oAuthMVC
{
    public class OAuthAuthorizationManager
    {
        public OAuthAuthorizationManager()
        {
        }

        public static bool CheckAccess()
        {

            ServiceProvider sp = Constants.CreateServiceProvider();
            //DotNetOpenAuth.Messaging.IDirectedProtocolMessage message = sp.ReadRequest();
            var auth = sp.ReadProtectedResourceAuthorization();

            DotNetOpenAuth.Messaging.IDirectedProtocolMessage message = ((DotNetOpenAuth.OAuth.Messages.MessageBase)(auth));

            if (auth != null)
            {
                var accessToken = Global.DataContext.OAuthTokens.Single(token => token.Token == auth.AccessToken);

                OAuthPrincipal p = sp.CreatePrincipal(auth);
                System.Web.HttpContext.Current.User = p;
                var policy = new OAuthPrincipalAuthorizationPolicy(p);
                var policies = new List<IAuthorizationPolicy> {
				policy,
			};

                // Only allow this method call if the access token scope permits it.
                string[] scopes = accessToken.Scope.Split('|');

                // TODO: Need to check the scopes here. Rather than a string this will likely be a scope that is tied to the
                // oAuth token and endpoint so you can't just change the endpoint to get the value back. This token is stored
                // in the database but not sure how this relates to the method name & endpoint. At the moment you can just change
                // the method name and it works.
                if (scopes.Contains("GetName") || scopes.Contains("GetAge") || scopes.Contains("GetFavoriteSites"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
