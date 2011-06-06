namespace OAuthAuthorizationServer.Code {
	using System;
	using System.Collections.Generic;

	using DotNetOpenAuth.OAuth2;

	/// <summary>
	/// An OAuth 2.0 Client that has registered with this Authorization Server.
	/// </summary>
	public partial class Client : IConsumerDescription {
		#region IConsumerDescription Members

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string IConsumerDescription.Secret {
			get { return this.ClientSecret; }
		}

		/// <summary>
		/// Gets the callback to use when an individual authorization request
		/// does not include an explicit callback URI.
		/// </summary>
		/// <value>
		/// An absolute URL; or <c>null</c> if none is registered.
		/// </value>
		Uri IConsumerDescription.DefaultCallback {
			get { return string.IsNullOrEmpty(this.Callback) ? null : new Uri(this.Callback); }
		}

		/// <summary>
		/// Determines whether a callback URI included in a client's authorization request
		/// is among those allowed callbacks for the registered client.
		/// </summary>
		/// <param name="callback">The absolute URI the client has requested the authorization result be received at.</param>
		/// <returns>
		///   <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
		/// </returns>
		bool IConsumerDescription.IsCallbackAllowed(Uri callback) {
			return string.IsNullOrEmpty(this.Callback) || callback == new Uri(this.Callback);
		}

		#endregion
	}
}