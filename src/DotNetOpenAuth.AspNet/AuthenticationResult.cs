//-----------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents the result of OAuth or OpenID authentication.
	/// </summary>
	public class AuthenticationResult {
		/// <summary>
		/// Returns an instance which indicates failed authentication.
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
			Justification = "This type is immutable.")]
		public static readonly AuthenticationResult Failed = new AuthenticationResult(isSuccessful: false);

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="isSuccessful">
		/// if set to <c>true</c> [is successful]. 
		/// </param>
		public AuthenticationResult(bool isSuccessful)
			: this(isSuccessful, provider: null, providerUserId: null, userName: null, extraData: null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="exception">
		/// The exception. 
		/// </param>
		public AuthenticationResult(Exception exception)
			: this(exception, provider: null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="provider">The provider name.</param>
		public AuthenticationResult(Exception exception, string provider)
			: this(isSuccessful: false) {
			if (exception == null) {
				throw new ArgumentNullException("exception");
			}

			this.Error = exception;
			this.Provider = provider;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="isSuccessful">
		/// if set to <c>true</c> [is successful]. 
		/// </param>
		/// <param name="provider">
		/// The provider. 
		/// </param>
		/// <param name="providerUserId">
		/// The provider user id. 
		/// </param>
		/// <param name="userName">
		/// Name of the user. 
		/// </param>
		/// <param name="extraData">
		/// The extra data. 
		/// </param>
		public AuthenticationResult(
			bool isSuccessful, string provider, string providerUserId, string userName, NameValueCollection extraData) {
			this.IsSuccessful = isSuccessful;
			this.Provider = provider;
			this.ProviderUserId = providerUserId;
			this.UserName = userName;
			this.ExtraData = extraData ?? new NameValueCollection();
		}

		/// <summary>
		/// Gets the error that may have occured during the authentication process
		/// </summary>
		public Exception Error { get; private set; }

		/// <summary>
		/// Gets the optional extra data that may be returned from the provider
		/// </summary>
		public NameValueCollection ExtraData { get; private set; }

		/// <summary>
		/// Gets a value indicating whether the authentication step is successful.
		/// </summary>
		/// <value> <c>true</c> if authentication is successful; otherwise, <c>false</c> . </value>
		public bool IsSuccessful { get; private set; }

		/// <summary>
		/// Gets the provider's name.
		/// </summary>
		public string Provider { get; private set; }

		/// <summary>
		/// Gets the user id that is returned from the provider.  It is unique only within the Provider's namespace.
		/// </summary>
		public string ProviderUserId { get; private set; }

		/// <summary>
		/// Gets an (insecure, non-unique) alias for the user that the user should recognize as himself/herself.
		/// </summary>
		/// <value>This may take the form of an email address, a URL, or any other value that the user may recognize.</value>
		/// <remarks>
		/// This alias may come from the Provider or may be derived by the relying party if the Provider does not supply one.
		/// It is not guaranteed to be unique and certainly does not merit any trust in any suggested authenticity.
		/// </remarks>
		public string UserName { get; private set; }
	}
}
