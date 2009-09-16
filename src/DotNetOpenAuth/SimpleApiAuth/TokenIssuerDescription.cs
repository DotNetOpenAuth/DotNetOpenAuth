//-----------------------------------------------------------------------
// <copyright file="TokenIssuerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleApiAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A description of a Simple API Auth Token Issuer.
	/// </summary>
	public class TokenIssuerDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="TokenIssuerDescription"/> class.
		/// </summary>
		public TokenIssuerDescription() {
			this.Version = Protocol.DefaultVersion;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenIssuerDescription"/> class.
		/// </summary>
		/// <param name="endpointUrl">The endpoint URL of the Token Issuer.</param>
		public TokenIssuerDescription(Uri endpointUrl)
			: this() {
			this.EndpointUrl = endpointUrl;
		}

		/// <summary>
		/// Gets or sets the endpoint URL of the Token Issuer.
		/// </summary>
		/// <value>The endpoint URL.</value>
		public Uri EndpointUrl { get; set; }

		/// <summary>
		/// Gets or sets the version of the Simple API Auth protocol to use with this Token Issuer.
		/// </summary>
		/// <value>The version.</value>
		public Version Version { get; set; }
	}
}
