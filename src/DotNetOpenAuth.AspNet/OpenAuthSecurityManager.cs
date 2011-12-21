using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web;
using DotNetOpenAuth.Messaging;

namespace DotNetOpenAuth.AspNet {
    /// <summary>
    /// Manage authenticating with an external OAuth or OpenID provider
    /// </summary>
    public class OpenAuthSecurityManager {
        private const string ProviderQueryStringName = "__provider__";

        private readonly HttpContextBase _requestContext;
        private readonly IOpenAuthDataProvider _dataProvider;
        private readonly IAuthenticationClient _authenticationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAuthSecurityManager"/> class.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        public OpenAuthSecurityManager(HttpContextBase requestContext) :
            this(requestContext, provider: null, dataProvider: null) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAuthSecurityManager"/> class.
        /// </summary>
        /// <param name="requestContext">The request context.</param>
        /// <param name="provider">The provider.</param>
        /// <param name="dataProvider">The data provider.</param>
        public OpenAuthSecurityManager(HttpContextBase requestContext, IAuthenticationClient provider, IOpenAuthDataProvider dataProvider) {
            if (requestContext == null)
            {
                throw new ArgumentNullException("requestContext");
            }

            _requestContext = requestContext;
            _dataProvider = dataProvider;
            _authenticationProvider = provider;
        }

        /// <summary>
        /// Requests the specified provider to start the authentication by directing users to an external website
        /// </summary>
        /// <param name="returnUrl">The return url after user is authenticated.</param>
        public void RequestAuthentication(string returnUrl) {
            // convert returnUrl to an absolute path
            Uri uri;
            if (!String.IsNullOrEmpty(returnUrl)) {
                uri = UriHelper.ConvertToAbsoluteUri(returnUrl, _requestContext);
            }
            else {
                uri = HttpRequestInfo.GetPublicFacingUrl(_requestContext.Request, _requestContext.Request.ServerVariables);
            }
            // attach the provider parameter so that we know which provider initiated 
            // the login when user is redirected back to this page
            uri = uri.AttachQueryStringParameter(ProviderQueryStringName, _authenticationProvider.ProviderName);
            _authenticationProvider.RequestAuthentication(_requestContext, uri);
        }

        public static string GetProviderName(HttpContextBase context) {
            return context.Request.QueryString[ProviderQueryStringName];
        }

        /// <summary>
        /// Checks if user is successfully authenticated when user is redirected back to this user.
        /// </summary>
        /// <returns></returns>
        public AuthenticationResult VerifyAuthentication() {
            AuthenticationResult result = _authenticationProvider.VerifyAuthentication(_requestContext);
            if (!result.IsSuccessful) {
                // if the result is a Failed result, creates a new Failed response which has providerName info.
                result = new AuthenticationResult(isSuccessful: false, 
                                                  provider: _authenticationProvider.ProviderName, 
                                                  providerUserId: null,
                                                  userName: null, 
                                                  extraData: null);
            }

            return result;
        }

        /// <summary>
        /// Checks if the specified provider user id represents a valid account.
        /// If it does, log user in.
        /// </summary>
        /// <param name="providerUserId">The provider user id.</param>
        /// <param name="createPersistentCookie">if set to <c>true</c> create persistent cookie.</param>
        /// <returns>
        ///   <c>true</c> if the login is successful.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Login", Justification = "Login is used more consistently in ASP.Net")]
        public bool Login(string providerUserId, bool createPersistentCookie) {
            string userName = _dataProvider.GetUserNameFromOpenAuth(_authenticationProvider.ProviderName, providerUserId);
            if (String.IsNullOrEmpty(userName)) {
                return false;
            }

            OpenAuthAuthenticationTicketHelper.SetAuthenticationTicket(_requestContext, userName, createPersistentCookie);
            return true;
        }

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated by an OAuth & OpenID provider.
        /// </summary>
        public bool IsAuthenticatedWithOpenAuth {
            get {
                return _requestContext.Request.IsAuthenticated &&
                       OpenAuthAuthenticationTicketHelper.IsValidAuthenticationTicket(_requestContext);
            }
        }
    }
}