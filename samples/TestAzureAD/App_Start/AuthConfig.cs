using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Membership.OpenAuth;

namespace TestAzureAD
{
	internal static class AuthConfig
	{
		public static void RegisterOpenAuth()
		{
			// See http://go.microsoft.com/fwlink/?LinkId=252803 for details on setting up this ASP.NET
			// application to support logging in via external services.

			//OpenAuth.AuthenticationClients.AddTwitter(
			//    consumerKey: "your Twitter consumer key",
			//    consumerSecret: "your Twitter consumer secret");

			OpenAuth.AuthenticationClients.AddFacebook(
				appId: "XX",
				appSecret: "YY");

			//OpenAuth.AuthenticationClients.AddMicrosoft(
			//    clientId: "your Microsoft account client id",
			//    clientSecret: "your Microsoft account client secret");

			//OpenAuth.AuthenticationClients.AddGoogle();

			OpenAuth.AuthenticationClients.Add("Azure Active Directory", () => new DotNetOpenAuth.AspNet.Clients.AzureADClient("64e0b14b-43ae-497c-b1a8-e8a841a341fd", "MySecretPassword"));
					
			
			//AddFacebook(
			    //appId: "your Facebook app id",
			    //appSecret: "your Facebook app secret");
			

		}
	}
}