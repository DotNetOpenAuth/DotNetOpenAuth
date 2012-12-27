//-----------------------------------------------------------------------
// <copyright file="UnauthorizedResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Validation;

	/// <summary>
	/// A direct response sent in response to a rejected Bearer access token.
	/// </summary>
	/// <remarks>
	/// This satisfies the spec in: http://self-issued.info/docs/draft-ietf-oauth-v2-bearer.html#authn-header
	/// </remarks>
	public class UnauthorizedResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// The headers in the response message.
		/// </summary>
		private readonly WebHeaderCollection headers = new WebHeaderCollection();

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class.
		/// </summary>
		/// <param name="version">The protocol version.</param>
		protected UnauthorizedResponse(Version version = null)
			: base(version ?? Protocol.Default.Version) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		protected UnauthorizedResponse(IDirectedProtocolMessage request)
			: base(request) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets or sets the HTTP status code that the direct response should be sent with.
		/// </summary>
		public HttpStatusCode HttpStatusCode { get; set; }

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		public WebHeaderCollection Headers {
			get { return this.headers; }
		}

		#endregion

		/// <summary>
		/// Gets or sets the well known error code.
		/// </summary>
		/// <value>One of the values from <see cref="Protocol.BearerTokenErrorCodes"/>.</value>
		[MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorCode)]
		public string ErrorCode { get; set; }

		/// <summary>
		/// Gets or sets a human-readable explanation for developers that is not meant to be displayed to end users.
		/// </summary>
		[MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorDescription)]
		public string ErrorDescription { get; set; }

		/// <summary>
		/// Gets or sets an absolute URI identifying a human-readable web page explaining the error.
		/// </summary>
		[MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.ErrorUri)]
		public Uri ErrorUri { get; set; }

		/// <summary>
		/// Gets or sets the realm.
		/// </summary>
		/// <value>The realm.</value>
		[MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.Realm)]
		public string Realm { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart(Protocol.BearerTokenUnauthorizedResponseParameters.Scope, Encoder = typeof(ScopeEncoder))]
		public HashSet<string> Scope { get; set; }

		/// <summary>
		/// Gets the scheme to use in the WWW-Authenticate header.
		/// </summary>
		internal virtual string Scheme {
			get { return Protocol.BearerHttpAuthorizationScheme; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class
		/// to inform the client that the request was invalid.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="version">The version of OAuth 2 that is in use.</param>
		/// <returns>The error message.</returns>
		internal static UnauthorizedResponse InvalidRequest(ProtocolException exception, Version version = null) {
			Requires.NotNull(exception, "exception");
			var message = new UnauthorizedResponse(version) {
				ErrorCode = Protocol.BearerTokenErrorCodes.InvalidRequest,
				ErrorDescription = exception.Message,
				HttpStatusCode = System.Net.HttpStatusCode.BadRequest,
			};

			return message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class
		/// to inform the client that the bearer token included in the request was rejected.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="exception">The exception.</param>
		/// <returns>The error message.</returns>
		internal static UnauthorizedResponse InvalidToken(IDirectedProtocolMessage request, ProtocolException exception) {
			Requires.NotNull(request, "request");
			Requires.NotNull(exception, "exception");
			var message = new UnauthorizedResponse(request) {
				ErrorCode = Protocol.BearerTokenErrorCodes.InvalidToken,
				ErrorDescription = exception.Message,
				HttpStatusCode = System.Net.HttpStatusCode.Unauthorized,
			};

			return message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class
		/// to inform the client of the required set of scopes required to perform this operation.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="requiredScopes">The set of scopes required to perform this operation.</param>
		/// <returns>The error message.</returns>
		internal static UnauthorizedResponse InsufficientScope(IDirectedProtocolMessage request, HashSet<string> requiredScopes) {
			Requires.NotNull(request, "request");
			Requires.NotNull(requiredScopes, "requiredScopes");
			var message = new UnauthorizedResponse(request) {
				HttpStatusCode = System.Net.HttpStatusCode.Forbidden,
				ErrorCode = Protocol.BearerTokenErrorCodes.InsufficientScope,
				Scope = requiredScopes,
			};
			return message;
		}

		/// <summary>
		/// Ensures the message is valid.
		/// </summary>
		protected override void EnsureValidMessage() {
			base.EnsureValidMessage();

			// Make sure the characters used in the supplied parameters satisfy requirements.
			VerifyErrorCodeOrDescription(this.ErrorCode, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorCode);
			VerifyErrorCodeOrDescription(this.ErrorDescription, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorDescription);
			VerifyErrorUri(this.ErrorUri);

			// Ensure that at least one parameter is specified, as required in the spec.
			ErrorUtilities.VerifyProtocol(
				this.ErrorCode != null || this.ErrorDescription != null || this.ErrorUri != null || this.Realm != null || this.Scope != null,
				OAuthStrings.BearerTokenUnauthorizedAtLeastOneParameterRequired);
		}

		/// <summary>
		/// Ensures the error or error_description parameters contain only allowed characters.
		/// </summary>
		/// <param name="value">The argument.</param>
		/// <param name="parameterName">The name of the parameter being validated.  Used when errors are reported.</param>
		private static void VerifyErrorCodeOrDescription(string value, string parameterName) {
			if (value != null) {
				for (int i = 0; i < value.Length; i++) {
					// The allowed set of characters comes from http://self-issued.info/docs/draft-ietf-oauth-v2-bearer.html#authn-header
					char ch = value[i];
					if (!((ch >= '\x20' && ch <= '\x21') || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E'))) {
						ErrorUtilities.ThrowProtocol(OAuthStrings.ParameterContainsIllegalCharacters, parameterName, ch);
					}
				}
			}
		}

		/// <summary>
		/// Ensures the error_uri parameter contains only allowed characters and is an absolute URI.
		/// </summary>
		/// <param name="valueUri">The absolute URI.</param>
		private static void VerifyErrorUri(Uri valueUri) {
			if (valueUri != null) {
				ErrorUtilities.VerifyProtocol(valueUri.IsAbsoluteUri, OAuthStrings.AbsoluteUriRequired);
				string value = valueUri.AbsoluteUri;
				for (int i = 0; i < value.Length; i++) {
					// The allowed set of characters comes from http://self-issued.info/docs/draft-ietf-oauth-v2-bearer.html#authn-header
					char ch = value[i];
					if (!(ch == '\x21' || (ch >= '\x23' && ch <= '\x5B') || (ch >= '\x5D' && ch <= '\x7E'))) {
						ErrorUtilities.ThrowProtocol(OAuthStrings.ParameterContainsIllegalCharacters, Protocol.BearerTokenUnauthorizedResponseParameters.ErrorUri, ch);
					}
				}
			}
		}
	}
}
