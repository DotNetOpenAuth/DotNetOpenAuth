namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// Represents base class for OAuth 1.0 clients
	/// </summary>
	public abstract class OAuthClient : IAuthenticationClient {
		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient"/> class.
		/// </summary>
		/// <param name="providerName">Name of the provider.</param>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		protected OAuthClient(string providerName, ServiceProviderDescription serviceDescription, string consumerKey, string consumerSecret) :
			this(providerName, serviceDescription, new InMemoryOAuthTokenManager(consumerKey, consumerSecret)) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuthClient"/> class.
		/// </summary>
		/// <param name="providerName">Name of the provider.</param>
		protected OAuthClient(string providerName, ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager) :
			this(providerName, new DotNetOpenAuthWebConsumer(serviceDescription, tokenManager)) {
		}

		protected OAuthClient(string providerName, IOAuthWebWorker webWorker) {
			if (providerName == null) {
				throw new ArgumentNullException("providerName");
			}

			if (webWorker == null) {
				throw new ArgumentNullException("webWorker");
			}

			ProviderName = providerName;
			WebWorker = webWorker;
		}

		/// <summary>
		/// Gets the name of the provider which provides authentication service.
		/// </summary>
		public string ProviderName {
			get;
			private set;
		}

		/// <summary>
		/// Gets the <see cref="OAuthWebConsumer"/> instance which handles constructing requests
		/// to the OAuth providers.
		/// </summary>
		protected IOAuthWebWorker WebWorker {
			get;
			private set;
		}

		/// <summary>
		/// Attempts to authenticate users by forwarding them to an external website, and
		/// upon succcess or failure, redirect users back to the specified url.
		/// </summary>
		/// <param name="returnUrl">The return url after users have completed authenticating against external website.</param>
		public virtual void RequestAuthentication(HttpContextBase context, Uri returnUrl) {
			if (returnUrl == null) {
				throw new ArgumentNullException("returnUrl");
			}

			if (context == null) {
				throw new ArgumentNullException("context");
			}

			Uri callback = returnUrl.StripQueryArgumentsWithPrefix("oauth_");
			WebWorker.RequestAuthentication(callback);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <returns>
		/// An instance of <see cref="AuthenticationResult"/> containing authentication result.
		/// </returns>
		public virtual AuthenticationResult VerifyAuthentication(HttpContextBase context) {
			AuthorizedTokenResponse response = WebWorker.ProcessUserAuthorization();
			if (response == null) {
				return AuthenticationResult.Failed;
			}

			return VerifyAuthenticationCore(response);
		}

		/// <summary>
		/// Check if authentication succeeded after user is redirected back from the service provider.
		/// </summary>
		/// <param name="response">The response token returned from service provider</param>
		/// <returns>Authentication result</returns>
		protected abstract AuthenticationResult VerifyAuthenticationCore(AuthorizedTokenResponse response);
	}
}