//-----------------------------------------------------------------------
// <copyright file="UserAgentSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message from the authorization server to a user-agent client indicating that authorization has been granted.
	/// </summary>
	internal class UserAgentSuccessResponse : EndUserAuthorizationSuccessResponse, IHttpIndirectResponse, IAccessTokenSuccessResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The version.</param>
		internal UserAgentSuccessResponse(Uri clientCallback, Version version)
			: base(clientCallback, version) {
		}

		/// <summary>
		/// Gets a value indicating whether the payload for the message should be included
		/// in the redirect fragment instead of the query string or POST entity.
		/// </summary>
		bool IHttpIndirectResponse.Include301RedirectPayloadInFragment {
			get { return true; }
		}

		string IAccessTokenSuccessResponse.RefreshToken {
			get { return null; }
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>The access token.</value>
		[MessagePart(Protocol.access_token, IsRequired = true, AllowEmpty = false)]
		public string AccessToken { get; internal set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		public TimeSpan? Lifetime { get; internal set; }

		/// <summary>
		/// Gets or sets the state.
		/// </summary>
		/// <value>The state.</value>
		/// <remarks>
		/// REQUIRED if the state parameter was present in the client authorization request. Set to the exact value received from the client. 
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		internal string ClientState { get; set; }
	}
}
