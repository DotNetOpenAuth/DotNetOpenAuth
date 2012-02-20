//-----------------------------------------------------------------------
// <copyright file="AssociateUnsuccessfulResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.Messages {
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// The Provider's response to a Relying Party that requested an association that the Provider does not support.
	/// </summary>
	/// <remarks>
	/// This message type described in OpenID 2.0 section 8.2.4.
	/// </remarks>
	[DebuggerDisplay("OpenID {Version} associate (failed) response")]
	internal class AssociateUnsuccessfulResponse : DirectErrorResponse {
		/// <summary>
		/// A hard-coded string indicating an error occurred.
		/// </summary>
		/// <value>"unsupported-type" </value>
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "Read by reflection")]
		[MessagePart("error_code", IsRequired = true, AllowEmpty = false)]
#pragma warning disable 0414 // read by reflection
		private readonly string Error = "unsupported-type";
#pragma warning restore 0414

		/// <summary>
		/// Initializes a new instance of the <see cref="AssociateUnsuccessfulResponse"/> class.
		/// </summary>
		/// <param name="responseVersion">The OpenID version of the response message.</param>
		/// <param name="originatingRequest">The originating request.</param>
		internal AssociateUnsuccessfulResponse(Version responseVersion, AssociateRequest originatingRequest)
			: base(responseVersion, originatingRequest) {
			this.ErrorMessage = string.Format(CultureInfo.CurrentCulture, OpenIdStrings.AssociationOrSessionTypeUnrecognizedOrNotSupported, originatingRequest.AssociationType, originatingRequest.SessionType);
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
