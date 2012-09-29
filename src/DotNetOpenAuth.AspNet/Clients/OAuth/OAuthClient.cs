//-----------------------------------------------------------------------
// <copyright file="OAuthClient.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.IO;
	using System.Web;
	using System.Xml;
	using System.Xml.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Represents base class for OAuth 1.0 clients
	/// </summary>
	public abstract class OAuthClient : IAuthenticationClient {
		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient"/> class.
		/// </summary>
		/// <param name="providerName">
		/// Name of the provider. 
		/// </param>
		/// <param name="serviceDescription">
		/// The service description. 
		/// </param>
		/// <param name="consumerKey">
		/// The consumer key. 
		/// </param>
		/// <param name="consumerSecret">
		/// The consumer secret. 
		/// </param>
		protected OAuthClient(
			string providerName, ServiceProviderDescription serviceDescription, string consumerKey, string consumerSecret)
			: this(providerName, serviceDescription, new InMemoryOAuthTokenManager(consumerKey, consumerSecret)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient"/> class.
		/// </summary>
		/// <param name="providerName">
		/// Name of the provider. 
		/// </param>
		/// <param name="serviceDescription">
		/// The service Description.
		/// </param>
		/// <param name="tokenManager">
		/// The token Manager.
		/// </param>
		[SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "I don't know how to ensure this rule is followed given this API")]
		protected OAuthClient(
			string providerName, ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager)
			: this(providerName, new DotNetOpenAuthWebConsumer(serviceDescription, tokenManager)) {
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
		/// <param name="context">
		/// The context.
		/// </param>
		/// <param name="returnUrl">
		/// The return url after users have completed authenticating against external website. 
		/// </param>
		public virtual void RequestAuthentication(HttpContextBase context, Uri returnUrl) {
			Requires.NotNull(returnUrl, "returnUrl");
			Requires.NotNull(context, "context");

			Uri callback = returnUrl.StripQueryArgumentsWithPrefix("oauth_");
			this.WebWorker.RequestAuthentication(callback);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="context">
		/// The context.
		/// </param>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult"/> containing authentication result. 
		/// </returns>
		public virtual AuthenticationResult VerifyAuthentication(HttpContextBase context) {
			AuthorizedTokenResponse response = this.WebWorker.ProcessUserAuthorization();
			if (response == null) {
				return AuthenticationResult.Failed;
			}

			AuthenticationResult result = this.VerifyAuthenticationCore(response);
			if (result.IsSuccessful && result.ExtraData != null) {
				// add the access token to the user data dictionary just in case page developers want to use it
				var wrapExtraData = result.ExtraData.IsReadOnly
					? new Dictionary<string, string>(result.ExtraData)
					: result.ExtraData;
				wrapExtraData["accesstoken"] = response.AccessToken;

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
		/// The response token returned from service provider 
		/// </param>
		/// <returns>
		/// Authentication result 
		/// </returns>
		protected abstract AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response);
		#endregion
	}
}
