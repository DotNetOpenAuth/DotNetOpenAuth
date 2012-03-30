//-----------------------------------------------------------------------
// <copyright file="OAuth2AccessTokenData.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System.Runtime.Serialization;

	/// <summary>
	/// Captures the result of an access token request, including an optional refresh token.
	/// </summary>
	[DataContract]
	public class OAuth2AccessTokenData {
		#region Public Properties

		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		/// <value> The access token. </value>
		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		/// <value> The refresh token. </value>
		[DataMember(Name = "refresh_token")]
		public string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the scope.
		/// </summary>
		/// <value> The scope. </value>
		[DataMember(Name = "scope")]
		public string Scope { get; set; }

		/// <summary>
		/// Gets or sets the type of the token.
		/// </summary>
		/// <value> The type of the token. </value>
		[DataMember(Name = "token_type")]
		public string TokenType { get; set; }
		#endregion
	}
}
