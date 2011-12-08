using System;
using System.Globalization;
using DotNetOpenAuth.Web.Resources;

namespace DotNetOpenAuth.Web
{
    /// <summary>
    /// Represents an OpenAuth & OpenID account.
    /// </summary>
    public class OAuthAccount
    {
        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string Provider { get; private set; }

        /// <summary>
        /// Gets the provider user id.
        /// </summary>
        public string ProviderUserId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OAuthAccountData"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="providerUserId">The provider user id.</param>
        public OAuthAccount(string provider, string providerUserId)
        {
            if (string.IsNullOrEmpty(provider))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "provider"),
                    "provider");
            }

            if (string.IsNullOrEmpty(providerUserId))
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.CurrentCulture, WebResources.Argument_Cannot_Be_Null_Or_Empty, "providerUserId"),
                    "providerUserId");
            }

            Provider = provider;
            ProviderUserId = providerUserId;
        }
    }
}
