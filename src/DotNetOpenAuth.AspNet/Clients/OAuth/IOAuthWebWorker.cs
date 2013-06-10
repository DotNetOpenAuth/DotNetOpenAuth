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
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The interface implemented by all OAuth web authentication modules in this assembly.
	/// </summary>
	public interface IOAuthWebWorker {
		/// <summary>
		/// Creates an HTTP message handler that authorizes outgoing web requests.
		/// </summary>
		/// <param name="accessToken">The access token.</param>
		/// <returns>An <see cref="HttpMessageHandler"/> that applies the access token to all outgoing requests.</returns>
		HttpMessageHandler CreateMessageHandler(AccessToken accessToken);

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <param name="context">The HTTP context.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>
		/// The access token, if obtained; otherwise <c>null</c>.
		/// </returns>
		Task<AccessTokenResponse> ProcessUserAuthorizationAsync(HttpContextBase context = null, CancellationToken cancellationToken = default(CancellationToken));

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">The callback.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The URL to redirect the user agent to.</returns>
		Task<Uri> RequestAuthenticationAsync(Uri callback, CancellationToken cancellationToken = default(CancellationToken));
	}
}
