//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthWebConsumer.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The dot net open auth web consumer.
	/// </summary>
	public class DotNetOpenAuthWebConsumer : IOAuthWebWorker {
		#region Constants and Fields

		/// <summary>
		/// The _web consumer.
		/// </summary>
		private readonly WebConsumer _webConsumer;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthWebConsumer"/> class.
		/// </summary>
		/// <param name="serviceDescription">
		/// The service description.
		/// </param>
		/// <param name="tokenManager">
		/// The token manager.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// </exception>
		public DotNetOpenAuthWebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager) {
			Requires.NotNull(serviceDescription, "serviceDescription");
			Requires.NotNull(tokenManager, "tokenManager");

			this._webConsumer = new WebConsumer(serviceDescription, tokenManager);
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// The prepare authorized request.
		/// </summary>
		/// <param name="profileEndpoint">
		/// The profile endpoint.
		/// </param>
		/// <param name="accessToken">
		/// The access token.
		/// </param>
		/// <returns>
		/// </returns>
		public HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken) {
			return this._webConsumer.PrepareAuthorizedRequest(profileEndpoint, accessToken);
		}

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <returns>
		/// </returns>
		public AuthorizedTokenResponse ProcessUserAuthorization() {
			return this._webConsumer.ProcessUserAuthorization();
		}

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">
		/// The callback.
		/// </param>
		public void RequestAuthentication(Uri callback) {
			var redirectParameters = new Dictionary<string, string> { { "force_login", "false" } };
			UserAuthorizationRequest request = this._webConsumer.PrepareRequestUserAuthorization(
				callback, null, redirectParameters);
			this._webConsumer.Channel.PrepareResponse(request).Send();
		}

		#endregion
	}
}
