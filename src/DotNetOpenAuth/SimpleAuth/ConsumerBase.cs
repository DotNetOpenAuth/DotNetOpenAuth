//-----------------------------------------------------------------------
// <copyright file="ConsumerBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A base class for common Consumer behaviors.
	/// </summary>
	public class ConsumerBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="ConsumerBase"/> class.
		/// </summary>
		protected ConsumerBase() {
		}

		/// <summary>
		/// Adds the necessary HTTP header to an HTTP request for protected resources
		/// so that the Service Provider will allow the request through.
		/// </summary>
		/// <param name="request">The request for protected resources.</param>
		/// <param name="accessToken">The access token previously obtained from the Token Issuer.</param>
		public static void AuthorizeRequest(HttpWebRequest request, string accessToken) {
			ErrorUtilities.VerifyArgumentNotNull(request, "request");
			request.Headers[HttpRequestHeader.Authorization] = Protocol.HttpAuthorizationScheme + " " + accessToken;
		}
	}
}
