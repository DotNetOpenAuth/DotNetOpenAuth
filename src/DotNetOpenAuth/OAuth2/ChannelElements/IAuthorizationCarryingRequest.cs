//-----------------------------------------------------------------------
// <copyright file="IAuthorizationCarryingRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System.Security.Cryptography;

	using Messaging;

	/// <summary>
	/// The various types of tokens created by the authorization server.
	/// </summary>
	internal enum CodeOrTokenType {
		/// <summary>
		/// The code issued to the client after the user has approved authorization.
		/// </summary>
		AuthorizationCode,

		/// <summary>
		/// The long-lived token issued to the client that enables it to obtain
		/// short-lived access tokens later.
		/// </summary>
		RefreshToken,

		/// <summary>
		/// A (typically) short-lived token.
		/// </summary>
		AccessToken,
	}

	/// <summary>
	/// A message that carries some kind of token from the client to the authorization or resource server.
	/// </summary>
	internal interface IAuthorizationCarryingRequest : IDirectedProtocolMessage {
		/// <summary>
		/// Gets or sets the verification code or refresh/access token.
		/// </summary>
		/// <value>The code or token.</value>
		string CodeOrToken { get; set; }

		/// <summary>
		/// Gets the type of the code or token.
		/// </summary>
		/// <value>The type of the code or token.</value>
		CodeOrTokenType CodeOrTokenType { get; }

		/// <summary>
		/// Gets or sets the authorization that the token describes.
		/// </summary>
		IAuthorizationDescription AuthorizationDescription { get; set; }
	}
}
