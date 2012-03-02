//-----------------------------------------------------------------------
// <copyright file="IAuthorizationCarryingRequest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System.Security.Cryptography;

	using Messaging;

	/// <summary>
	/// A message that carries some kind of token from the client to the authorization or resource server.
	/// </summary>
	internal interface IAuthorizationCarryingRequest : IDirectedProtocolMessage {
		/// <summary>
		/// Gets the authorization that the code or token describes.
		/// </summary>
		IAuthorizationDescription AuthorizationDescription { get; }
	}
}
