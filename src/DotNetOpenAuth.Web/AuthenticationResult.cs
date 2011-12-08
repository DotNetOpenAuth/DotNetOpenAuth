using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DotNetOpenAuth.Web
{
    /// <summary>
    /// Represents the result of OAuth & OpenId authentication 
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Returns an instance which indicates failed authentication.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Security",
            "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "This type is immutable.")]
        public static readonly AuthenticationResult Failed = new AuthenticationResult(isSuccessful: false);

        /// <summary>
        /// Gets a value indicating whether the authentication step is successful.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if authentication is successful; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Gets the provider's name.
        /// </summary>
        public string Provider { get; private set; }

        /// <summary>
        /// Gets the unique user id that is returned from the provider.
        /// </summary>
        public string ProviderUserId { get; private set; }

        /// <summary>
        /// Gets the user name that is returned from the provider.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// Gets the optional extra data that may be returned from the provider
        /// </summary>
        public IDictionary<string, string> ExtraData { get; private set; }

        /// <summary>
        /// Gets the error that may have occured during the authentication process
        /// </summary>
        public Exception Error { get; private set; }

        public AuthenticationResult(bool isSuccessful) :
            this(isSuccessful,
                 provider: null,
                 providerUserId: null,
                 userName: null,
                 extraData: null)
        {
        }

        public AuthenticationResult(Exception exception) : this(isSuccessful: false)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            Error = exception;
        }

        public AuthenticationResult(
            bool isSuccessful,
            string provider,
            string providerUserId,
            string userName,
            IDictionary<string, string> extraData)
        {
            IsSuccessful = isSuccessful;
            Provider = provider;
            ProviderUserId = providerUserId;
            UserName = userName;
            ExtraData = extraData;
        }
    }
}
