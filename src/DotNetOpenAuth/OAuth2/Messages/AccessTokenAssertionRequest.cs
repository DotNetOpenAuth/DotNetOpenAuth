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

	internal class AccessTokenAssertionRequest : AccessTokenRequestBase {
		internal AccessTokenAssertionRequest(Uri tokenEndpoint, Version version)
			: base(tokenEndpoint, version) {
		}

		/// <summary>
		/// Gets or sets the format of the assertion as defined by the Authorization Server.
		/// </summary>
		/// <value>The assertion format.</value>
		[MessagePart(Protocol.assertion_type, IsRequired = true, AllowEmpty = false)]
		internal string AssertionType { get; set; }

		/// <summary>
		/// Gets or sets the assertion.
		/// </summary>
		/// <value>The assertion.</value>
		[MessagePart(Protocol.assertion, IsRequired = true, AllowEmpty = false)]
		internal string Assertion { get; set; }

		internal override GrantType GrantType {
			get { return GrantType.Assertion; }
		}
	}
}
