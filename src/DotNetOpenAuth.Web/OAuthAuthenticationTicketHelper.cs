using System;
using System.Diagnostics;
using System.Web;
using System.Web.Security;
using DotNetOpenAuth.Web.Resources;

namespace DotNetOpenAuth.Web
{
    internal static class OAuthAuthenticationTicketHelper
    {
        private const string OAuthCookieToken = "OAuth";

        public static void SetAuthenticationTicket(HttpContextBase context, string userName, bool createPersistentCookie)
        {
            if (!context.Request.IsSecureConnection && FormsAuthentication.RequireSSL)
            {
                throw new HttpException(WebResources.ConnectionNotSecure);
            }

            HttpCookie cookie = GetAuthCookie(userName, createPersistentCookie);
            context.Response.Cookies.Add(cookie);
        }

        public static bool IsOAuthAuthenticationTicket(HttpContextBase context)
        {
            HttpCookie cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null)
            {
                return false;
            }

            string encryptedCookieData = cookie.Value;
            if (String.IsNullOrEmpty(encryptedCookieData))
            {
                return false;
            }

            try
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(encryptedCookieData);
                return authTicket != null && !authTicket.Expired && authTicket.UserData == OAuthCookieToken;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static HttpCookie GetAuthCookie(string userName, bool createPersistentCookie)
        {
            Debug.Assert(!String.IsNullOrEmpty(userName));

            var ticket = new FormsAuthenticationTicket(
                /* version */ 2,
                              userName,
                              DateTime.Now,
                              DateTime.Now.Add(FormsAuthentication.Timeout),
                              createPersistentCookie,
                              OAuthCookieToken,
                              FormsAuthentication.FormsCookiePath);

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            if (encryptedTicket == null || encryptedTicket.Length < 1)
            {
                throw new HttpException(WebResources.FailedToEncryptTicket);
            }

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath,
                Secure = FormsAuthentication.RequireSSL
            };

            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }

            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            return cookie;
        }
    }
}