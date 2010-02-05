//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// A description of an OAuth WRAP Authorization Server.
	/// </summary>
	public class AuthorizationServerDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerDescription"/> class.
		/// </summary>
		public AuthorizationServerDescription() {
			this.Version = Protocol.DefaultVersion;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizationServerDescription"/> class.
		/// </summary>
		/// <param name="endpointUrl">The endpoint URL of the Authorization Server.</param>
		public AuthorizationServerDescription(Uri endpointUrl)
			: this() {
			this.EndpointUrl = endpointUrl;
		}

		/// <summary>
		/// Gets or sets the endpoint URL of the Authorization Server.
		/// </summary>
		/// <value>The endpoint URL.</value>
		public Uri EndpointUrl { get; set; }

		/// <summary>
		/// Gets or sets the version of the OAuth WRAP protocol to use with this Authorization Server.
		/// </summary>
		/// <value>The version.</value>
		public Version Version { get; set; }
	}
}
