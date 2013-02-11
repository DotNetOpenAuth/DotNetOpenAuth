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
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using Validation;

	/// <summary>
	/// The dot net open auth web consumer.
	/// </summary>
	public class DotNetOpenAuthWebConsumer : IOAuthWebWorker, IDisposable {
		#region Constants and Fields

		/// <summary>
		/// The _web consumer.
		/// </summary>
		private readonly WebConsumer webConsumer;

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
		public DotNetOpenAuthWebConsumer(ServiceProviderDescription serviceDescription, IConsumerTokenManager tokenManager) {
			Requires.NotNull(serviceDescription, "serviceDescription");
			Requires.NotNull(tokenManager, "tokenManager");

			this.webConsumer = new WebConsumer(serviceDescription, tokenManager);
		}

		#endregion

		#region Public Methods and Operators

		/// <summary>
		/// The prepare authorized request.
		/// </summary>
		/// <param name="profileEndpoint">The profile endpoint.</param>
		/// <param name="accessToken">The access token.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// An HTTP request.
		/// </returns>
		public Task<HttpRequestMessage> PrepareAuthorizedRequestAsync(MessageReceivingEndpoint profileEndpoint, string accessToken, CancellationToken cancellationToken = default(CancellationToken)) {
			return this.webConsumer.PrepareAuthorizedRequestAsync(profileEndpoint, accessToken, cancellationToken);
		}

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns>
		/// The response message.
		/// </returns>
		public Task<AuthorizedTokenResponse> ProcessUserAuthorizationAsync(CancellationToken cancellationToken = default(CancellationToken)) {
			return this.webConsumer.ProcessUserAuthorizationAsync(cancellationToken: cancellationToken);
		}

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response message.
		/// </returns>
		public async Task<HttpResponseMessage> RequestAuthenticationAsync(Uri callback, CancellationToken cancellationToken = default(CancellationToken)) {
			var redirectParameters = new Dictionary<string, string>();
			UserAuthorizationRequest request = await this.webConsumer.PrepareRequestUserAuthorizationAsync(
				callback, null, redirectParameters, cancellationToken);
			var response = await this.webConsumer.Channel.PrepareResponseAsync(request, cancellationToken);
			return response;
		}

		#endregion

		#region IDisposable members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.webConsumer.Dispose();
			}
		}
	}
}
