//-----------------------------------------------------------------------
// <copyright file="DirectErrorResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message sent from a Provider to a Relying Party in response to a direct message request that resulted in an error.
	/// </summary>
	/// <remarks>
	/// This message must be sent with an HTTP status code of 400.
	/// This class satisfies OpenID 2.0 section 5.1.2.2.
	/// </remarks>
	internal class DirectErrorResponse : ResponseBase {
		/// <summary>
		/// Gets or sets a human-readable message indicating why the request failed. 
		/// </summary>
		[MessagePart("error", IsRequired = true, AllowEmpty = true)]
		internal string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the contact address for the administrator of the server. 
		/// </summary>
		/// <value>The contact address may take any form, as it is intended to be displayed to a person. </value>
		[MessagePart("contact", IsRequired = false, AllowEmpty = true)]
		internal string Contact { get; set; }

		/// <summary>
		/// Gets or sets a reference token, such as a support ticket number or a URL to a news blog, etc. 
		/// </summary>
		[MessagePart("reference", IsRequired = false, AllowEmpty = true)]
		internal string Reference { get; set; }
	}
}
