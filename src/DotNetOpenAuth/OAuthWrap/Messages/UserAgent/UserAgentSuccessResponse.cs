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
	internal class UserAgentSuccessResponse : MessageBase, IHttpIndirectResponse, IAccessTokenSuccessResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentSuccessResponse"/> class.
		/// </summary>
		/// <param name="clientCallback">The client callback.</param>
		/// <param name="version">The version.</param>
		internal UserAgentSuccessResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback)
		{
		}

		/// <summary>
		/// Gets a value indicating whether the payload for the message should be included
		/// in the redirect fragment instead of the query string or POST entity.
		/// </summary>
		bool IHttpIndirectResponse.Include301RedirectPayloadInFragment {
			get { return true; }
		}

		/// <summary>
		/// Gets the access token.
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
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// OPTIONAL. The refresh token used to obtain new access tokens using the same end-user access grant as described in Section 6  (Refreshing an Access Token). 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = false, AllowEmpty = false)]
		public string RefreshToken { get; internal set; }

		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		/// <value>The access token secret.</value>
		/// <remarks>
		/// REQUIRED if requested by the client. The corresponding access token secret as requested by the client. 
		/// </remarks>
		[MessagePart(Protocol.access_token_secret, IsRequired = false, AllowEmpty = false)]
		public string AccessTokenSecret { get; internal set; }

		/// <summary>
		/// Gets the scope.
		/// </summary>
		/// <value>The scope.</value>
		string IAccessTokenSuccessResponse.Scope {
			get { return null; }
		}

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
