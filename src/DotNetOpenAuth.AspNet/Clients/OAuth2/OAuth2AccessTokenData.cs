//-----------------------------------------------------------------------
// <copyright file="OAuth2AccessTokenData.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.AspNet.Clients {
	using System.Runtime.Serialization;

	[DataContract]
	public class OAuth2AccessTokenData {
		[DataMember(Name = "access_token")]
		public string AccessToken { get; set; }

		[DataMember(Name = "refresh_token")]
		public string RefreshToken { get; set; }

		[DataMember(Name = "scope")]
		public string Scope { get; set; }

		[DataMember(Name = "token_type")]
		public string TokenType { get; set; }
	}
}
