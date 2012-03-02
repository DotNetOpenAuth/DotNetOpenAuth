//-----------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// Represents the result of OAuth & OpenId authentication 
	/// </summary>
	public class AuthenticationResult {
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

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="isSuccessful">if set to <c>true</c> [is successful].</param>
		public AuthenticationResult(bool isSuccessful) :
			this(isSuccessful,
				 provider: null,
				 providerUserId: null,
				 userName: null,
				 extraData: null) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public AuthenticationResult(Exception exception)
			: this(isSuccessful: false) {
			if (exception == null) {
				throw new ArgumentNullException("exception");
			}

			Error = exception;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
		/// </summary>
		/// <param name="isSuccessful">if set to <c>true</c> [is successful].</param>
		/// <param name="provider">The provider.</param>
		/// <param name="providerUserId">The provider user id.</param>
		/// <param name="userName">Name of the user.</param>
		/// <param name="extraData">The extra data.</param>
		public AuthenticationResult(
			bool isSuccessful,
			string provider,
			string providerUserId,
			string userName,
			IDictionary<string, string> extraData) {
			IsSuccessful = isSuccessful;
			Provider = provider;
			ProviderUserId = providerUserId;
			UserName = userName;
			if (extraData != null) {
				// wrap extraData in a read-only dictionary
				ExtraData = new ReadOnlyDictionary<string, string>(extraData);
			}
		}
	}
}