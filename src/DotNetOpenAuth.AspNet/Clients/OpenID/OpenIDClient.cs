//-----------------------------------------------------------------------
// <copyright file="OpenIdClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Validation;

	/// <summary>
	/// Base classes for OpenID clients.
	/// </summary>
	public class OpenIdClient : IAuthenticationClient {
		#region Constants and Fields

		/// <summary>
		/// The openid relying party.
		/// </summary>
		/// <remarks>
		/// Pass null as applicationStore to specify dumb mode
		/// </remarks>
		private static readonly OpenIdRelyingParty RelyingParty = new OpenIdRelyingParty(applicationStore: null);

		/// <summary>
		/// The provider identifier.
		/// </summary>
		private readonly Identifier providerIdentifier;

		/// <summary>
		/// The provider name.
		/// </summary>
		private readonly string providerName;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdClient"/> class.
		/// </summary>
		/// <param name="providerName">
		/// Name of the provider. 
		/// </param>
		/// <param name="providerIdentifier">
		/// The provider identifier, which is the usually the login url of the specified provider. 
		/// </param>
		public OpenIdClient(string providerName, Identifier providerIdentifier) {
			Requires.NotNullOrEmpty(providerName, "providerName");
			Requires.NotNull(providerIdentifier, "providerIdentifier");

			this.providerName = providerName;
			this.providerIdentifier = providerIdentifier;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the provider which provides authentication service.
		/// </summary>
		public string ProviderName {
			get {
				return this.providerName;
			}
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Attempts to authenticate users by forwarding them to an external website, and upon succcess or failure, redirect users back to the specified url.
		/// </summary>
		/// <param name="context">The context of the current request.</param>
		/// <param name="returnUrl">The return url after users have completed authenticating against external website.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings",
			Justification = "We don't have a Uri object handy.")]
		public virtual async Task RequestAuthenticationAsync(HttpContextBase context, Uri returnUrl, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(returnUrl, "returnUrl");

			var realm = new Realm(returnUrl.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped));
			IAuthenticationRequest request = await RelyingParty.CreateRequestAsync(this.providerIdentifier, realm, returnUrl, cancellationToken);

			// give subclasses a chance to modify request message, e.g. add extension attributes, etc.
			this.OnBeforeSendingAuthenticationRequest(request);

			await request.RedirectToProviderAsync(context);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">The context of the current request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult" /> containing authentication result.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">Thrown if no OpenID response was found in the incoming HTTP request.</exception>
		public virtual async Task<AuthenticationResult> VerifyAuthenticationAsync(HttpContextBase context, CancellationToken cancellationToken = default(CancellationToken)) {
			IAuthenticationResponse response = await RelyingParty.GetResponseAsync(context.Request, cancellationToken);
			if (response == null) {
				throw new InvalidOperationException(WebResources.OpenIDFailedToGetResponse);
			}

			if (response.Status == AuthenticationStatus.Authenticated) {
				string id = response.ClaimedIdentifier;
				var extraData = this.GetExtraData(response) ?? new NameValueCollection();

				// try to look up username from the 'username' or 'email' property. If not found, fall back to 'friendly id'
				string username = extraData["username"] ?? extraData["email"] ?? response.FriendlyIdentifierForDisplay;

				return new AuthenticationResult(true, this.ProviderName, id, username, extraData);
			}

			return AuthenticationResult.Failed;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the extra data obtained from the response message when authentication is successful.
		/// </summary>
		/// <param name="response">
		/// The response message. 
		/// </param>
		/// <returns>Always null.</returns>
		protected virtual NameValueCollection GetExtraData(IAuthenticationResponse response) {
			return null;
		}

		/// <summary>
		/// Called just before the authentication request is sent to service provider.
		/// </summary>
		/// <param name="request">
		/// The request. 
		/// </param>
		protected virtual void OnBeforeSendingAuthenticationRequest(IAuthenticationRequest request) { }

		#endregion
	}
}
