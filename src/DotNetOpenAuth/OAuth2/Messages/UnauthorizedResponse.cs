//-----------------------------------------------------------------------
// <copyright file="UnauthorizedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Diagnostics.Contracts;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A direct response that is simply a 401 Unauthorized with an 
	/// WWW-Authenticate: OAuth header.
	/// </summary>
	internal class UnauthorizedResponse : MessageBase, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="version">The protocol version.</param>
		internal UnauthorizedResponse(ProtocolException exception, Version version = null)
			: base(version ?? Protocol.Default.Version) {
			Contract.Requires<ArgumentNullException>(exception != null);
			this.ErrorMessage = exception.Message;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		internal UnauthorizedResponse(IDirectedProtocolMessage request)
			: base(request) {
			this.Realm = "Service";
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UnauthorizedResponse"/> class.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="exception">The exception.</param>
		internal UnauthorizedResponse(IDirectedProtocolMessage request, ProtocolException exception)
			: this(request) {
			Contract.Requires<ArgumentNullException>(exception != null);
			this.ErrorMessage = exception.Message;
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct response should be sent with.
		/// </summary>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.Unauthorized; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get {
				return new WebHeaderCollection() {
					{ HttpResponseHeader.WwwAuthenticate, Protocol.HttpAuthorizationScheme },
				};
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>The error message.</value>
		[MessagePart("error")]
		internal string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the realm.
		/// </summary>
		/// <value>The realm.</value>
		[MessagePart("realm")]
		internal string Realm { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value>The scope.</value>
		[MessagePart("scope")]
		internal string Scope { get; set; }

		/// <summary>
		/// Gets or sets the algorithms.
		/// </summary>
		/// <value>The algorithms.</value>
		[MessagePart("algorithms")]
		internal string Algorithms { get; set; }

		/// <summary>
		/// Gets or sets the user endpoint.
		/// </summary>
		/// <value>The user endpoint.</value>
		[MessagePart("user-uri")]
		internal Uri UserEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the token endpoint.
		/// </summary>
		/// <value>The token endpoint.</value>
		[MessagePart("token-uri")]
		internal Uri TokenEndpoint { get; set; }
	}
}
