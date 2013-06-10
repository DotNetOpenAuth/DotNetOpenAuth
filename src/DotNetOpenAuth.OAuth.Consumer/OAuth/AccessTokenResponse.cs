//-----------------------------------------------------------------------
// <copyright file="AccessTokenResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	/// <summary>
	/// Captures the data that is returned from a request for an access token.
	/// </summary>
	public class AccessTokenResponse {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessTokenResponse"/> class.
		/// </summary>
		/// <param name="accessToken">The access token.</param>
		/// <param name="tokenSecret">The token secret.</param>
		/// <param name="extraData">Any extra data that came with the response.</param>
		public AccessTokenResponse(string accessToken, string tokenSecret, NameValueCollection extraData) {
			this.AccessToken = new AccessToken(accessToken, tokenSecret);
			this.ExtraData = extraData;
		}

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value>
		/// The access token.
		/// </value>
		public AccessToken AccessToken { get; set; }

		/// <summary>
		/// Gets or sets any extra data that came with the response..
		/// </summary>
		public NameValueCollection ExtraData { get; set; }
	}
}
