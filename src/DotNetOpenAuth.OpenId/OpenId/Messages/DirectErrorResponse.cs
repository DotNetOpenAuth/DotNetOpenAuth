//-----------------------------------------------------------------------
// <copyright file="DirectErrorResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Net;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message sent from a Provider to a Relying Party in response to a direct message request that resulted in an error.
	/// </summary>
	/// <remarks>
	/// This message must be sent with an HTTP status code of 400.
	/// This class satisfies OpenID 2.0 section 5.1.2.2.
	/// </remarks>
	internal class DirectErrorResponse : DirectResponseBase, IErrorMessage, IHttpDirectResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectErrorResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal DirectErrorResponse(Version responseVersion, IDirectedProtocolMessage originatingRequest)
			: base(responseVersion, originatingRequest) {
		}

		#region IHttpDirectResponse Members

		/// <summary>
		/// Gets the HTTP status code that the direct respones should be sent with.
		/// </summary>
		/// <value><see cref="HttpStatusCode.BadRequest"/></value>
		HttpStatusCode IHttpDirectResponse.HttpStatusCode {
			get { return HttpStatusCode.BadRequest; }
		}

		/// <summary>
		/// Gets the HTTP headers to add to the response.
		/// </summary>
		/// <value>May be an empty collection, but must not be <c>null</c>.</value>
		WebHeaderCollection IHttpDirectResponse.Headers {
			get { return new WebHeaderCollection(); }
		}

		#endregion

		/// <summary>
		/// Gets or sets a human-readable message indicating why the request failed. 
		/// </summary>
		[MessagePart("error", IsRequired = true, AllowEmpty = true)]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the contact address for the administrator of the server. 
		/// </summary>
		/// <value>The contact address may take any form, as it is intended to be displayed to a person. </value>
		[MessagePart("contact", IsRequired = false, AllowEmpty = true)]
		public string Contact { get; set; }

		/// <summary>
		/// Gets or sets a reference token, such as a support ticket number or a URL to a news blog, etc. 
		/// </summary>
		[MessagePart("reference", IsRequired = false, AllowEmpty = true)]
		public string Reference { get; set; }
	}
}
