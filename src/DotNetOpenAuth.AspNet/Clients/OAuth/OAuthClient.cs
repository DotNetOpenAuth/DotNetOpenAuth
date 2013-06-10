//-----------------------------------------------------------------------
// <copyright file="OAuthClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// Represents base class for OAuth 1.0 clients
	/// </summary>
	public abstract class OAuthClient : IAuthenticationClient {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient" /> class.
		/// </summary>
		/// <param name="providerName">Name of the provider.</param>
		/// <param name="serviceDescription">The service Description.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		protected OAuthClient(
			string providerName, ServiceProviderDescription serviceDescription, string consumerKey, string consumerSecret)
			: this(providerName, new DotNetOpenAuthWebConsumer(serviceDescription, consumerKey, consumerSecret)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient"/> class.
		/// </summary>
		/// <param name="providerName">
		/// The provider name.
		/// </param>
		/// <param name="webWorker">
		/// The web worker.
		/// </param>
		protected OAuthClient(string providerName, IOAuthWebWorker webWorker) {
			Requires.NotNull(providerName, "providerName");
			Requires.NotNull(webWorker, "webWorker");

			this.ProviderName = providerName;
			this.WebWorker = webWorker;
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Gets the name of the provider which provides authentication service.
		/// </summary>
		public string ProviderName { get; private set; }

		#endregion

		#region Properties

		/// <summary>
		/// Gets the OAuthWebConsumer instance which handles constructing requests to the OAuth providers.
		/// </summary>
		protected IOAuthWebWorker WebWorker { get; private set; }

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// Attempts to authenticate users by forwarding them to an external website, and upon succcess or failure, redirect users back to the specified url.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="returnUrl">The return url after users have completed authenticating against external website.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// A task that completes with the asynchronous operation.
		/// </returns>
		public virtual Task RequestAuthenticationAsync(HttpContextBase context, Uri returnUrl, CancellationToken cancellationToken = default(CancellationToken)) {
			Requires.NotNull(returnUrl, "returnUrl");
			Requires.NotNull(context, "context");

			Uri callback = returnUrl.StripQueryArgumentsWithPrefix("oauth_");
			return this.WebWorker.RequestAuthenticationAsync(callback, cancellationToken);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult" /> containing authentication result.
		/// </returns>
		public virtual async Task<AuthenticationResult> VerifyAuthenticationAsync(HttpContextBase context, CancellationToken cancellationToken = default(CancellationToken)) {
			AccessTokenResponse response = await this.WebWorker.ProcessUserAuthorizationAsync(context, cancellationToken);
			if (response == null) {
				return AuthenticationResult.Failed;
			}

			AuthenticationResult result = await this.VerifyAuthenticationCoreAsync(response, cancellationToken);
			if (result.IsSuccessful && result.ExtraData != null) {
				// add the access token to the user data dictionary just in case page developers want to use it
				var wrapExtraData = new NameValueCollection(result.ExtraData);
				wrapExtraData["accesstoken"] = response.AccessToken.Token;
				wrapExtraData["accesstokensecret"] = response.AccessToken.Secret;

				AuthenticationResult wrapResult = new AuthenticationResult(
					result.IsSuccessful,
					result.Provider,
					result.ProviderUserId,
					result.UserName,
					wrapExtraData);

				result = wrapResult;
			}

			return result;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Helper method to load an XDocument from an input stream.
		/// </summary>
		/// <param name="stream">The input stream from which to load the document.</param>
		/// <returns>The XML document.</returns>
		internal static XDocument LoadXDocumentFromStream(Stream stream) {
			const int MaxChars = 0x10000; // 64k

			var settings = MessagingUtilities.CreateUntrustedXmlReaderSettings();
			settings.MaxCharactersInDocument = MaxChars;
			return XDocument.Load(XmlReader.Create(stream, settings));
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="response">
		/// The access token returned from service provider 
		/// </param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// Authentication result 
		/// </returns>
		protected abstract Task<AuthenticationResult> VerifyAuthenticationCoreAsync(AccessTokenResponse response, CancellationToken cancellationToken);
		#endregion
	}
}
