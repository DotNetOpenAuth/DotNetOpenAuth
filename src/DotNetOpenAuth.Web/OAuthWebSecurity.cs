using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.Web.Clients;
using DotNetOpenAuth.Web.Resources;

namespace DotNetOpenAuth.Web
{
    /// <summary>
    /// Contains APIs to manage authentication against OAuth & OpenID service providers
    /// </summary>
    public static class OAuthWebSecurity
    {
        private const string ProviderQueryStringName = "__provider__";

        private static IOAuthDataProvider _oAuthDataProvider;
        private static IOAuthDataProvider OAuthDataProvider
        {
            get
            {
                return _oAuthDataProvider;
            }
        }

        // contains all registered authentication clients
        private static readonly AuthenticationClientCollection _authenticationClients = new AuthenticationClientCollection();

        public static void RegisterDataProvider(IOAuthDataProvider dataProvider)
        {
            if (dataProvider == null)
            {
                throw new ArgumentNullException("dataProvider");
            }

            var originalValue = Interlocked.CompareExchange(ref _oAuthDataProvider, dataProvider, null);
            if (originalValue != null)
            {
                throw new InvalidOperationException(WebResources.OAuthDataProviderRegistered);
            }
        }

        public static bool IsOAuthDataProviderRegistered
        {
            get
            {
                return OAuthDataProvider != null;
            }
        }

        private static void EnsureDataProvider()
        {
            if (!IsOAuthDataProviderRegistered)
            {
                throw new InvalidOperationException(WebResources.OAuthDataProviderNotRegistered);
            }
        }

        /// <summary>
        /// Registers a supported OAuth client with the specified consumer key and consumer secret.
        /// </summary>
        /// <param name="client">One of the supported OAuth clients.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        public static void RegisterOAuthClient(BuiltInOAuthClient client, string consumerKey, string consumerSecret)
        {
            IAuthenticationClient authenticationClient;
            switch (client)
            {
                case BuiltInOAuthClient.LinkedIn:
                    authenticationClient = new LinkedInClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.Twitter:
                    authenticationClient = new TwitterClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.Facebook:
                    authenticationClient = new FacebookClient(consumerKey, consumerSecret);
                    break;

                case BuiltInOAuthClient.WindowsLive:
                    authenticationClient = new WindowsLiveClient(consumerKey, consumerSecret);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("client");
            }
            RegisterClient(authenticationClient);
        }

        /// <summary>
        /// Registers a supported OpenID client
        /// </summary>
        public static void RegisterOpenIDClient(BuiltInOpenIDClient openIDClient)
        {
            IAuthenticationClient client;
            switch (openIDClient)
            {
                case BuiltInOpenIDClient.Google:
                    client = new GoogleOpenIdClient();
                    break;

                case BuiltInOpenIDClient.Yahoo:
                    client = new YahooOpenIdClient();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("openIDClient");
            }

            RegisterClient(client);
        }

        /// <summary>
        /// Registers an authentication client.
        /// </summary>
        public static void RegisterClient(IAuthenticationClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }

            if (String.IsNullOrEmpty(client.ProviderName))
            {
                throw new ArgumentException(WebResources.InvalidServiceProviderName, "client");
            }

            if (_authenticationClients.Contains(client))
            {
                throw new ArgumentException(WebResources.ServiceProviderNameExists, "client");
            }

            _authenticationClients.Add(client);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        public static void RequestAuthentication(string provider)
        {
            RequestAuthentication(provider, returnUrl: null);
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="returnUrl">The return url after user is authenticated.</param>
        [SuppressMessage(
            "Microsoft.Design",
            "CA1054:UriParametersShouldNotBeStrings",
            MessageId = "1#",
            Justification = "We want to allow relative app path, and support ~/")]
        public static void RequestAuthentication(string provider, string returnUrl)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            RequestAuthenticationCore(new HttpContextWrapper(HttpContext.Current), provider, returnUrl);
        }

        internal static void RequestAuthenticationCore(HttpContextBase context, string provider, string returnUrl)
        {
            if (String.IsNullOrEmpty(provider))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "provider"),
                    "provider");
            }

            IAuthenticationClient client = GetOAuthClient(provider);

            // convert returnUrl to an absolute path
            Uri uri;
            if (!String.IsNullOrEmpty(returnUrl))
            {
                uri = UriHelper.ConvertToAbsoluteUri(returnUrl);
            }
            else
            {
                uri = UriHelper.GetPublicFacingUrl(context.Request);
            }
            // attach the provider parameter so that we know which provider initiated 
            // the login when user is redirected back to this page
            uri = uri.AttachQueryStringParameter(ProviderQueryStringName, provider);
            client.RequestAuthentication(context, uri);
        }

        /// <summary>
        /// Checks if user is successfully authenticated when user is redirected back to this user.
        /// </summary>
        /// <returns></returns>
        public static AuthenticationResult VerifyAuthentication()
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return VerifyAuthenticationCore(new HttpContextWrapper(HttpContext.Current));
        }

        internal static AuthenticationResult VerifyAuthenticationCore(HttpContextBase context)
        {
            string providerName = context.Request.QueryString[ProviderQueryStringName];
            if (String.IsNullOrEmpty(providerName))
            {
                return AuthenticationResult.Failed;
            }

            IAuthenticationClient client;
            if (TryGetOAuthClient(providerName, out client))
            {
                AuthenticationResult result = client.VerifyAuthentication(context);
                if (!result.IsSuccessful)
                {
                    // if the result is a Failed result, creates a new Failed response which has providerName info.
                    result = new AuthenticationResult(isSuccessful: false, provider: providerName, providerUserId: null,
                                                      userName: null, extraData: null);
                }

                return result;
            }
            else
            {
                throw new InvalidOperationException(WebResources.InvalidServiceProviderName);
            }
        }

        /// <summary>
        /// Checks if the specified provider user id represents a valid account.
        /// If it does, log user in.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <returns><c>true</c> if the login is successful.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
        public static bool Login(string providerName, string providerUserId, bool createPersistentCookie)
        {
            if (HttpContext.Current == null)
            {
                throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
            }

            return LoginCore(new HttpContextWrapper(HttpContext.Current), providerName, providerUserId, createPersistentCookie);
        }

        internal static bool LoginCore(HttpContextBase context, string providerName, string providerUserId, bool createPersistentCookie)
        {
            EnsureDataProvider();

            string userName = OAuthDataProvider.GetUserNameFromOAuth(providerName, providerUserId);
            if (String.IsNullOrEmpty(userName))
            {
                return false;
            }

            OAuthAuthenticationTicketHelper.SetAuthenticationTicket(
                   context,
                   userName,
                   createPersistentCookie);
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated by an OAuth provider.
        /// </summary>
        public static bool IsAuthenticatedViaOAuth
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    throw new InvalidOperationException(WebResources.HttpContextNotAvailable);
                }

                return GetIsAuthenticatedViaOAuthCore(new HttpContextWrapper(HttpContext.Current));
            }
        }

        internal static bool GetIsAuthenticatedViaOAuthCore(HttpContextBase context)
        {
            if (!context.Request.IsAuthenticated)
            {
                return false;
            }
            return OAuthAuthenticationTicketHelper.IsOAuthAuthenticationTicket(context);
        }

        /// <summary>
        /// Creates or update the account with the specified provider, provider user id and associate it with the specified user name.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="userName">The user name.</param>
        public static void CreateOrUpdateAccount(string providerName, string providerUserId, string userName)
        {
            EnsureDataProvider();
            OAuthDataProvider.CreateOrUpdateOAuthAccount(providerName, providerUserId, userName);
        }

        public static string GetUsername(string providerName, string providerUserId)
        {
            EnsureDataProvider();
            return OAuthDataProvider.GetUserNameFromOAuth(providerName, providerUserId);
        }

        /// <summary>
        /// Gets all OAuth & OpenID accounts which are associted with the specified user name.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public static ICollection<OAuthAccount> GetAccountsFromUserName(string userName)
        {
            if (String.IsNullOrEmpty(userName))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "userName"),
                    "userName");
            }

            EnsureDataProvider();

            return OAuthDataProvider.GetOAuthAccountsFromUserName(userName);
        }

        /// <summary>
        /// Delete the specified OAuth & OpenID account
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public static void DeleteAccount(string providerName, string providerUserId)
        {
            EnsureDataProvider();

            OAuthDataProvider.DeleteOAuthAccount(providerName, providerUserId);
        }

        internal static IAuthenticationClient GetOAuthClient(string providerName)
        {
            if (!_authenticationClients.Contains(providerName))
            {
                throw new ArgumentException(WebResources.ServiceProviderNotFound, "providerName");
            }

            return _authenticationClients[providerName];
        }

        internal static bool TryGetOAuthClient(string provider, out IAuthenticationClient client)
        {
            if (_authenticationClients.Contains(provider))
            {
                client = _authenticationClients[provider];
                return true;
            }
            else
            {
                client = null;
                return false;
            }
        }

        /// <summary>
        /// for unit tests
        /// </summary>
        internal static void ClearProviders()
        {
            _authenticationClients.Clear();
        }

        /// <summary>
        /// for unit tests
        /// </summary>
        internal static void ClearDataProvider()
        {
            _oAuthDataProvider = null;
        }
    }
}