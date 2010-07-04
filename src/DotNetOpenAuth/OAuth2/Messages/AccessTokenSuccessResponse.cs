//-----------------------------------------------------------------------
// <copyright file="AccessTokenSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A response from the Authorization Server to the Client containing a delegation code
	/// that the Client should use to obtain an access token.
	/// </summary>
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class AccessTokenSuccessResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenSuccessResponse(IAccessTokenRequest request)
			: base(request) {
		}

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		/// <value>Always HttpStatusCode.OK</value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.OK; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get {
				return new WebHeaderCollection
				{
					{ HttpResponseHeader.CacheControl, "no-store" },
				};
			}
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
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// OPTIONAL. The refresh token used to obtain new access tokens using the same end-user access grant as described in Section 6  (Refreshing an Access Token). 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = false, AllowEmpty = false)]
		public string RefreshToken { get; internal set; }

		/// <summary>
		/// Gets or sets the scope of access being requested.
		/// </summary>
		/// <value>The scope of the access request expressed as a list of space-delimited strings. The value of the scope parameter is defined by the authorization server. If the value contains multiple space-delimited strings, their order does not matter, and each string adds an additional access range to the requested scope.</value>
		[MessagePart(Protocol.scope, IsRequired = false, AllowEmpty = true)]
		public string Scope { get; set; }
	}
}
