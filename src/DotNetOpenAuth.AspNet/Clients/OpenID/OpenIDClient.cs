using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.AspNet.Resources;

namespace DotNetOpenAuth.AspNet.Clients
{
    /// <summary>
    /// Base classes for OpenID clients.
    /// </summary>
    public class OpenIDClient : IAuthenticationClient
    {
        private readonly Identifier _providerIdentifier;
        private readonly string _providerName;

        private static OpenIdRelyingParty _openidRelayingParty =
            new OpenIdRelyingParty(new StandardRelyingPartyApplicationStore());

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenIDClient"/> class.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="providerIdentifier">The provider identifier, which is the usually the login url of the specified provider.</param>
        public OpenIDClient(string providerName, string providerIdentifier)
        {
            if (String.IsNullOrEmpty(providerIdentifier))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "providerIdentifier"),
                    "providerIdentifier");
            }

            if (String.IsNullOrEmpty(providerName))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "providerName"),
                    "providerName");
            }

            _providerName = providerName;
            if (!Identifier.TryParse(providerIdentifier, out _providerIdentifier) || _providerIdentifier == null)
            {
                throw new ArgumentException(WebResources.OpenIDInvalidIdentifier, "providerIdentifier");
            }
        }

        /// <summary>
        /// Gets the name of the provider which provides authentication service.
        /// </summary>
        public string ProviderName
        {
            get
            {
                return _providerName;
            }
        }

        /// <summary>
        /// Attempts to authenticate users by forwarding them to an external website, and
        /// upon succcess or failure, redirect users back to the specified url.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <param name="returnUrl">The return url after users have completed authenticating against external website.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Usage",
            "CA2234:PassSystemUriObjectsInsteadOfStrings",
            Justification = "We don't have a Uri object handy.")]
        public virtual void RequestAuthentication(HttpContextBase context, Uri returnUrl)
        {
            if (returnUrl == null)
            {
                throw new ArgumentNullException("returnUrl");
            }

            var realm = new Realm(returnUrl.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));
            IAuthenticationRequest request = _openidRelayingParty.CreateRequest(_providerIdentifier, realm, returnUrl);

            // give subclasses a chance to modify request message, e.g. add extension attributes, etc.
            OnBeforeSendingAuthenticationRequest(request);

            request.RedirectToProvider();
        }

        /// <summary>
        /// Called just before the authentication request is sent to service provider.
        /// </summary>
        /// <param name="request">The request.</param>
        protected virtual void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request)
        {
        }

        /// <summary>
        /// Check if authentication succeeded after user is redirected back from the service provider.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <returns>
        /// An instance of <see cref="AuthenticationResult"/> containing authentication result.
        /// </returns>
        public virtual AuthenticationResult VerifyAuthentication(HttpContextBase context)
        {
            IAuthenticationResponse response = _openidRelayingParty.GetResponse();
            if (response == null)
            {
                throw new InvalidOperationException(WebResources.OpenIDFailedToGetResponse);
            }

            if (response.Status == AuthenticationStatus.Authenticated)
            {
                string id = response.ClaimedIdentifier;
                string username;

                Dictionary<string, string> extraData = GetExtraData(response) ?? new Dictionary<string, string>();
                // try to look up username from the 'username' or 'email' property. If not found, fall back to 'friendly id'
                if (!extraData.TryGetValue("username", out username) && !extraData.TryGetValue("email", out username))
                {
                    username = response.FriendlyIdentifierForDisplay;
                }

                return new AuthenticationResult(
                    true,
                    ProviderName,
                    id,
                    username,
                    extraData);
            }

            return AuthenticationResult.Failed;
        }

        /// <summary>
        /// Gets the extra data obtained from the response message when authentication is successful.
        /// </summary>
        /// <param name="response">The response message.</param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> GetExtraData(IAuthenticationResponse response)
        {
            return null;
        }
    }
}