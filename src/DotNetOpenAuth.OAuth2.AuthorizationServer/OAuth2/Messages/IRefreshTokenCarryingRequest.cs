//-----------------------------------------------------------------------
// <copyright file="IRefreshTokenCarryingRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.AuthServer.ChannelElements {
	using DotNetOpenAuth.OAuth2.ChannelElements;

	/// <summary>
	/// A message that carries a refresh token between client and authorization server.
	/// </summary>
	internal interface IRefreshTokenCarryingRequest : IAuthorizationCarryingRequest {
		/// <summary>
		/// Gets or sets the refresh token.
		/// </summary>
		string RefreshToken { get; set; }

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		new RefreshToken AuthorizationDescription { get; set; }
	}
}
