using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace DotNetOAuth {
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
		private Uri requestTokenUri;

		/// <summary>
		/// The URL used to obtain an unauthorized Request Token, described in Section 6.1 (Obtaining an Unauthorized Request Token).
		/// </summary>
		/// <remarks>
		/// The request URL query MUST NOT contain any OAuth Protocol Parameters.
		/// </remarks>
		/// <exception cref="ArgumentException">Thrown if this property is set to a URI with OAuth protocol parameters.</exception>
		public Uri RequestTokenUri {
			get { return requestTokenUri; }
			set {
				if (UriUtil.QueryStringContainsOAuthParameters(value)) {
					throw new ArgumentException(Strings.RequestUrlMustNotHaveOAuthParameters);
				}
				requestTokenUri = value;
			}
		}

		/// <summary>
		/// The URL used to obtain User authorization for Consumer access, described in Section 6.2 (Obtaining User Authorization).
		/// </summary>
		public Uri UserAuthorizationUri { get; set;  }

		/// <summary>
		/// The URL used to exchange the User-authorized Request Token for an Access Token, described in Section 6.3 (Obtaining an Access Token).
		/// </summary>
		public Uri AccessTokenUri { get; set; }
	}
}
