//-----------------------------------------------------------------------
// <copyright file="IOAuthWebWorker.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth.Messages;

	/// <summary>
	/// The io auth web worker.
	/// </summary>
	public interface IOAuthWebWorker {
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
		/// <returns>An HTTP request.</returns>
		HttpWebRequest PrepareAuthorizedRequest(MessageReceivingEndpoint profileEndpoint, string accessToken);

		/// <summary>
		/// The process user authorization.
		/// </summary>
		/// <returns>The response message.</returns>
		AuthorizedTokenResponse ProcessUserAuthorization();

		/// <summary>
		/// The request authentication.
		/// </summary>
		/// <param name="callback">
		/// The callback.
		/// </param>
		void RequestAuthentication(Uri callback);

		#endregion
	}
}
