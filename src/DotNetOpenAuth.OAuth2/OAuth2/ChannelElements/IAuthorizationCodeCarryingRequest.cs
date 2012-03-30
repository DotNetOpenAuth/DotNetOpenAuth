//-----------------------------------------------------------------------
// <copyright file="IAuthorizationCodeCarryingRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	/// <summary>
	/// A message that carries an authorization code between client and authorization server.
	/// </summary>
	internal interface IAuthorizationCodeCarryingRequest : IAuthorizationCarryingRequest {
		/// <summary>
		/// Gets or sets the authorization code.
		/// </summary>
		string Code { get; set; }

		/// <summary>
		/// Gets or sets the authorization that the code describes.
		/// </summary>
		new AuthorizationCode AuthorizationDescription { get; set; }
	}
}
