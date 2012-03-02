//-----------------------------------------------------------------------
// <copyright file="IndirectErrorResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message sent from a Provider to a Relying Party in response to an indirect message request that resulted in an error.
	/// </summary>
	/// <remarks>
	/// This class satisfies OpenID 2.0 section 5.2.3.
	/// </remarks>
	internal class IndirectErrorResponse : IndirectResponseBase, IErrorMessage {
		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectErrorResponse"/> class.
		/// </summary>
		/// <param name="request">The request that resulted in this error on the Provider.</param>
		internal IndirectErrorResponse(SignedResponseRequest request)
			: base(request, Protocol.Lookup(GetVersion(request)).openidnp.error) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IndirectErrorResponse"/> class.
		/// </summary>
		/// <param name="version">The OpenID version this message should comply with.</param>
		/// <param name="recipient">The recipient of this message.</param>
		internal IndirectErrorResponse(Version version, Uri recipient)
			: base(version, recipient, Protocol.Lookup(version).openidnp.error) {
		}

		/// <summary>
		/// Gets or sets a human-readable message indicating why the request failed. 
		/// </summary>
		[MessagePart("openid.error", IsRequired = true, AllowEmpty = true)]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the contact address for the administrator of the server. 
		/// </summary>
		/// <value>The contact address may take any form, as it is intended to be displayed to a person. </value>
		[MessagePart("openid.contact", IsRequired = false, AllowEmpty = true)]
		public string Contact { get; set; }

		/// <summary>
		/// Gets or sets a reference token, such as a support ticket number or a URL to a news blog, etc. 
		/// </summary>
		[MessagePart("openid.reference", IsRequired = false, AllowEmpty = true)]
		public string Reference { get; set; }
	}
}
