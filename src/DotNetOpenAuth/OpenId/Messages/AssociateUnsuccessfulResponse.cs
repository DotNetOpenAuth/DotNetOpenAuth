//-----------------------------------------------------------------------
// <copyright file="AssociateUnsuccessfulResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The Provider's response to a Relying Party that requested an association that the Provider does not support.
	/// </summary>
	/// <remarks>
	/// This message type described in OpenID 2.0 section 8.2.4.
	/// </remarks>
	internal class AssociateUnsuccessfulResponse : DirectErrorResponse {
		/// <summary>
		/// A hard-coded string indicating an error occurred.
		/// </summary>
		/// <value>"unsupported-type" </value>
		[MessagePart("error_code", IsRequired = true, AllowEmpty = false)]
#pragma warning disable 0414 // read by reflection
		private readonly string Error = "unsupported-type";
#pragma warning restore 0414

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnsuccessfulResponse"/> class.
		/// </summary>
		internal AssociateUnsuccessfulResponse() {
		}

		/// <summary>
		/// Gets or sets an association type supported by the OP from Section 8.3 (Association Types). 
		/// </summary>
		[MessagePart("assoc_type", IsRequired = false, AllowEmpty = false)]
		internal string AssociationType { get; set; }

		/// <summary>
		/// Gets or sets a valid association session type from Section 8.4 (Association Session Types) that the OP supports. 
		/// </summary>
		[MessagePart("session_type", IsRequired = false, AllowEmpty = false)]
		internal string SessionType { get; set; }
	}
}
