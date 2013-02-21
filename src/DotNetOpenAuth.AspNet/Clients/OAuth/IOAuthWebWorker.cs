//-----------------------------------------------------------------------
// <copyright file="IOAuthWebWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The interface implemented by all OAuth web authentication modules in this assembly.
	/// </summary>
	public interface IOAuthWebWorker {
		/// <summary>
		/// Creates an HTTP message handler that authorizes outgoing web requests.
		/// </summary>
		/// <param name="accessToken">The access token.</param>
		HttpMessageHandler CreateMessageHandler(string accessToken);

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The response message.
		/// </returns>
		Task<AuthorizedTokenResponse> ProcessUserAuthorizationAsync(CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The response message</returns>
		Task<HttpResponseMessage> RequestAuthenticationAsync(Uri callback, CancellationToken cancellationToken = default(CancellationToken));
	}
}
