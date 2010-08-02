//-----------------------------------------------------------------------
// <copyright file="AccessTokenAssertionRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A request from a Client to an Authorization Server to exchange some assertion for an access token.
	/// </summary>
	internal class AccessTokenAssertionRequest : AccessTokenRequestBase {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenAssertionRequest"/> class.
		/// </summary>
		/// <param name="tokenEndpoint">The Authorization Server's access token endpoint URL.</param>
		/// <param name="version">The version.</param>
		internal AccessTokenAssertionRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		/// <summary>
		/// Gets or sets the format of the assertion as defined by the Authorization Server.
		/// </summary>
		/// <value>The assertion format.</value>
		[MessagePart(Protocol.assertion_type, IsRequired = true, AllowEmpty = false)]
		internal Uri AssertionType { get; set; }

		/// <summary>
		/// Gets or sets the assertion.
		/// </summary>
		/// <value>The assertion.</value>
		[MessagePart(Protocol.assertion, IsRequired = true, AllowEmpty = false)]
		internal string Assertion { get; set; }

		/// <summary>
		/// Gets the type of the grant.
		/// </summary>
		/// <value>The type of the grant.</value>
		internal override GrantType GrantType {
			get { return GrantType.Assertion; }
		}
	}
}
