//-----------------------------------------------------------------------
// <copyright file="IAccessTokenCarryingRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	/// <summary>
	/// A message that carries an access token between client and authorization server.
	/// </summary>
	internal interface IAccessTokenCarryingRequest : IAuthorizationCarryingRequest {
		/// <summary>
		/// Gets or sets the access token.
		/// </summary>
		string AccessToken { get; set; }

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		new AccessToken AuthorizationDescription { get; set; }
	}
}
