using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using DotNetOpenAuth.Samples.OpenIDConnectRP.Models;

namespace DotNetOpenAuth.Samples.OpenIDConnectRP
{
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;
    using System.Web.Helpers;

    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });            
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions {
                ClientId = "DNOA",
                Authority = "https://localhost:44333/core",
                RedirectUri = "http://localhost:39743/",
                ResponseType = "id_token",
                Scope = "openid email",

                SignInAsAuthenticationType = "Cookies",

            });
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
        }
    }
}