//-----------------------------------------------------------------------
// <copyright file="OAuth2ChannelBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.Messages;

	/// <summary>
	/// The base messaging channel used by OAuth 2.0 parties.
	/// </summary>
	internal abstract class OAuth2ChannelBase : StandardMessageFactoryChannel {
		/// <summary>
		/// The messages receivable by this channel.
		/// </summary>
		private static readonly Type[] MessageTypes = new Type[] {
			typeof(AccessTokenRefreshRequest),
			typeof(AccessTokenAuthorizationCodeRequest),
			typeof(AccessTokenResourceOwnerPasswordCredentialsRequest),
			typeof(AccessTokenAssertionRequest),
			typeof(AccessTokenClientCredentialsRequest),
			typeof(AccessTokenSuccessResponse),
			typeof(AccessTokenFailedResponse),
			typeof(EndUserAuthorizationRequest),
			typeof(EndUserAuthorizationSuccessAuthCodeResponse),
			typeof(EndUserAuthorizationSuccessAccessTokenResponse),
			typeof(EndUserAuthorizationFailedResponse),
			typeof(UnauthorizedResponse),
		};

		/// <summary>
		/// The protocol versions supported by this channel.
		/// </summary>
		private static readonly Version[] Versions = Protocol.AllVersions.Select(v => v.Version).ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="OAuth2ChannelBase"/> class.
		/// </summary>
		/// <param name="channelBindingElements">The channel binding elements.</param>
		internal OAuth2ChannelBase(params IChannelBindingElement[] channelBindingElements)
			: base(MessageTypes, Versions, channelBindingElements) {
		}
	}
}
