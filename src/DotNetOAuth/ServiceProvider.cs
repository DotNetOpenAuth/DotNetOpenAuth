//-----------------------------------------------------------------------
// <copyright file="ServiceProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Web;

	/// <summary>
	/// A web application that allows access via OAuth.
	/// </summary>
	/// <remarks>
	/// <para>The Service Provider’s documentation should include:</para>
	/// <list>
	/// <item>The URLs (Request URLs) the Consumer will use when making OAuth requests, and the HTTP methods (i.e. GET, POST, etc.) used in the Request Token URL and Access Token URL.</item>
	/// <item>Signature methods supported by the Service Provider.</item>
	/// <item>Any additional request parameters that the Service Provider requires in order to obtain a Token. Service Provider specific parameters MUST NOT begin with oauth_.</item>
	/// </list>
	/// </remarks>
	internal class ServiceProvider {
		/// <summary>
		/// The field used to store the value of the <see cref="RequestTokenEndpoint"/> property.
		/// </summary>
		private ServiceProviderEndpoint requestTokenEndpoint;

		/// <summary>
		/// Gets or sets the URL used to obtain an unauthorized Request Token,
		/// described in Section 6.1 (Obtaining an Unauthorized Request Token).
		/// </summary>
		/// <remarks>
		/// The request URL query MUST NOT contain any OAuth Protocol Parameters.
		/// This is the URL that <see cref="Messages.RequestTokenMessage"/> messages are directed to.
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if this property is set to a URI with OAuth protocol parameters.</exception>
		public ServiceProviderEndpoint RequestTokenEndpoint {
			get {
				return this.requestTokenEndpoint;
			}

			set {
				if (value != null && UriUtil.QueryStringContainsOAuthParameters(value.Location)) {
					throw new ArgumentException(Strings.RequestUrlMustNotHaveOAuthParameters);
				}

				this.requestTokenEndpoint = value;
			}
		}

		/// <summary>
		/// Gets or sets the URL used to obtain User authorization for Consumer access, 
		/// described in Section 6.2 (Obtaining User Authorization).
		/// </summary>
		/// <remarks>
		/// This is the URL that <see cref="Messages.DirectUserToServiceProviderMessage"/> messages are
		/// indirectly (via the user agent) sent to.
		/// </remarks>
		public ServiceProviderEndpoint UserAuthorizationEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the URL used to exchange the User-authorized Request Token 
		/// for an Access Token, described in Section 6.3 (Obtaining an Access Token).
		/// </summary>
		/// <remarks>
		/// This is the URL that <see cref="Messages.RequestAccessTokenMessage"/> messages are directed to.
		/// </remarks>
		public ServiceProviderEndpoint AccessTokenEndpoint { get; set; }
	}
}
