//-----------------------------------------------------------------------
// <copyright file="AccessTokenSuccessResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A response from the Authorization Server to the Consumer containing a delegation code
	/// that the Consumer should use to obtain an access token.
	/// </summary>
	/// <remarks>
	/// This message type is shared by the Web App, Rich App, and Username/Password profiles.
	/// </remarks>
	internal class AccessTokenSuccessResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenSuccessResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal AccessTokenSuccessResponse(IDirectedProtocolMessage request)
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
		internal string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the lifetime of the access token.
		/// </summary>
		/// <value>The lifetime.</value>
		[MessagePart(Protocol.expires_in, IsRequired = false, Encoder = typeof(TimespanSecondsEncoder))]
		internal TimeSpan? Lifetime { get; set; }

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value>The refresh token.</value>
		/// <remarks>
		/// OPTIONAL. The refresh token used to obtain new access tokens using the same end-user access grant as described in Section 6  (Refreshing an Access Token). 
		/// </remarks>
		[MessagePart(Protocol.refresh_token, IsRequired = false, AllowEmpty = false)]
		internal string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		/// <value>The access token secret.</value>
		/// <remarks>
		/// REQUIRED if requested by the client. The corresponding access token secret as requested by the client. 
		/// </remarks>
		[MessagePart(Protocol.access_token_secret, IsRequired = false, AllowEmpty = false)]
		internal string AccessTokenSecret { get; set; }
	}
}
