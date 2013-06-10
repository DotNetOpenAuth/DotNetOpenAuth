//-----------------------------------------------------------------------
// <copyright file="DotNetOpenAuthWebConsumer.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// The dot net open auth web consumer.
	/// </summary>
	public class DotNetOpenAuthWebConsumer : IOAuthWebWorker {
		#region Constants and Fields

		/// <summary>
		/// The _web consumer.
		/// </summary>
		private readonly Consumer webConsumer;

		#endregion

		#region Constructors and Destructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DotNetOpenAuthWebConsumer" /> class.
		/// </summary>
		/// <param name="serviceDescription">The service description.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer secret.</param>
		public DotNetOpenAuthWebConsumer(ServiceProviderDescription serviceDescription, string consumerKey, string consumerSecret) {
			Requires.NotNull(serviceDescription, "serviceDescription");

			this.webConsumer = new Consumer {
				ServiceProvider = serviceDescription,
				ConsumerKey = consumerKey,
				ConsumerSecret = consumerSecret,
				TemporaryCredentialStorage = new CookieTemporaryCredentialStorage(),
			};
		}

		#endregion

		/// <summary>
		/// Gets the DotNetOpenAuth <see cref="Consumer"/> instance that can be used to make OAuth 1.0 authorized HTTP requests.
		/// </summary>
		public Consumer Consumer {
			get { return this.webConsumer; }
		}

		#region Public Methods and Operators

		/// <summary>
		/// Creates an HTTP message handler that authorizes outgoing web requests.
		/// </summary>
		/// <param name="accessToken">The access token.</param>
		/// <returns>An <see cref="HttpMessageHandler"/> that applies the access token to all outgoing requests.</returns>
		public HttpMessageHandler CreateMessageHandler(AccessToken accessToken) {
			Requires.NotNullOrEmpty(accessToken.Token, "accessToken");

			return this.Consumer.CreateMessageHandler(accessToken);
		}

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <param name="context">The HTTP context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response message.
		/// </returns>
		public Task<AccessTokenResponse> ProcessUserAuthorizationAsync(HttpContextBase context = null, CancellationToken cancellationToken = default(CancellationToken)) {
			if (context == null) {
				context = new HttpContextWrapper(HttpContext.Current);
			}

			return this.webConsumer.ProcessUserAuthorizationAsync(context.Request.Url, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response message.
		/// </returns>
		public Task<Uri> RequestAuthenticationAsync(Uri callback, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.webConsumer.RequestUserAuthorizationAsync(callback, cancellationToken: cancellationToken);
		}

		#endregion
	}
}
