//-----------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth {
	/// <summary>
	/// An OAuth 1.0 access token and secret.
	/// </summary>
	public struct AccessToken {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> struct.
		/// </summary>
		/// <param name="token">The token.</param>
		/// <param name="secret">The secret.</param>
		public AccessToken(string token, string secret)
			: this() {
			this.Token = token;
			this.Secret = secret;
		}

		/// <summary>
		/// Gets or sets the token.
		/// </summary>
		/// <value>
		/// The token.
		/// </value>
		public string Token { get; set; }

		/// <summary>
		/// Gets or sets the token secret.
		/// </summary>
		/// <value>
		/// The secret.
		/// </value>
		public string Secret { get; set; }

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString() {
			return this.Token;
		}
	}
}
