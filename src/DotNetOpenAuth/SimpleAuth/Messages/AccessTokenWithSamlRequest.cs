//-----------------------------------------------------------------------
// <copyright file="AccessTokenWithSamlRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request for an access token for a consumer application that can
	/// issue a SAML assertion to prove its identity.
	/// </summary>
	internal class AccessTokenWithSamlRequest : MessageBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenWithSamlRequest"/> class.
		/// </summary>
		/// <param name="version">The version.</param>
		internal AccessTokenWithSamlRequest(Version version)
			: base(version) {
		}

		/// <summary>
		/// Gets or sets the SAML token.
		/// </summary>
		/// <value>A SAML token serialized as an XML document.</value>
		[MessagePart(Protocol.sa_saml, IsRequired = true, AllowEmpty = false)]
		public string Saml { get; set; }

		/// <summary>
		/// Gets or sets the SWT.
		/// </summary>
		/// <value>The SWT (TODO: what is that?).</value>
		/// <remarks>
		/// The spec says that the SWT parameter is required for certain scenarios,
		/// so we mark it as optional here since the scenario may or may not apply.
		/// </remarks>
		[MessagePart(Protocol.sa_swt, IsRequired = false, AllowEmpty = false)]
		public string Swt { get; set; }
	}
}
