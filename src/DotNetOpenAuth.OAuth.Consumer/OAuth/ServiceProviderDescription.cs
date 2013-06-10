//-----------------------------------------------------------------------
// <copyright file="ServiceProviderDescription.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text;
	using System.Threading.Tasks;
	using Validation;

	/// <summary>
	/// Describes an OAuth 1.0 service provider.
	/// </summary>
	public class ServiceProviderDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderDescription" /> class.
		/// </summary>
		public ServiceProviderDescription() {
			this.TemporaryCredentialsRequestEndpointMethod = HttpMethod.Post;
			this.TokenRequestEndpointMethod = HttpMethod.Post;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderDescription"/> class.
		/// </summary>
		/// <param name="temporaryCredentialsRequestEndpoint">The temporary credentials request endpoint.</param>
		/// <param name="resourceOwnerAuthorizationEndpoint">The resource owner authorization endpoint.</param>
		/// <param name="tokenRequestEndpoint">The token request endpoint.</param>
		public ServiceProviderDescription(
			string temporaryCredentialsRequestEndpoint, string resourceOwnerAuthorizationEndpoint, string tokenRequestEndpoint)
			: this() {
			if (temporaryCredentialsRequestEndpoint != null) {
				this.TemporaryCredentialsRequestEndpoint = new Uri(temporaryCredentialsRequestEndpoint, UriKind.Absolute);
			}

			if (resourceOwnerAuthorizationEndpoint != null) {
				this.ResourceOwnerAuthorizationEndpoint = new Uri(resourceOwnerAuthorizationEndpoint, UriKind.Absolute);
			}

			if (tokenRequestEndpoint != null) {
				this.TokenRequestEndpoint = new Uri(tokenRequestEndpoint, UriKind.Absolute);
			}
		}

		/// <summary>
		/// Gets or sets the temporary credentials request endpoint.
		/// </summary>
		/// <value>
		/// The temporary credentials request endpoint.
		/// </value>
		public Uri TemporaryCredentialsRequestEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method to use with the temporary credentials request endpoint.
		/// </summary>
		public HttpMethod TemporaryCredentialsRequestEndpointMethod { get; set; }

		/// <summary>
		/// Gets or sets the resource owner authorization endpoint.
		/// </summary>
		/// <value>
		/// The resource owner authorization endpoint.
		/// May be <c>null</c> for 2-legged OAuth.
		/// </value>
		public Uri ResourceOwnerAuthorizationEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the token request endpoint.
		/// </summary>
		/// <value>
		/// The token request endpoint.
		/// </value>
		public Uri TokenRequestEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method to use with the token request endpoint.
		/// </summary>
		public HttpMethod TokenRequestEndpointMethod { get; set; }
	}
}
