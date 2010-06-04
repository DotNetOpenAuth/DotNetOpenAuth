//-----------------------------------------------------------------------
// <copyright file="IAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Messages;
	using Messaging;

	internal interface ITokenCarryingRequest : IDirectedProtocolMessage {
		string CodeOrToken { get; set;  }

		CodeOrTokenType CodeOrTokenType { get; }

		IAuthorizationDescription AuthorizationDescription { get; set; }
	}

	public interface IAccessTokenRequest : IDirectedProtocolMessage {
		string ClientIdentifier { get; }

		string ClientSecret { get; }

		string SecretType { get; }
	}

	internal enum CodeOrTokenType {
		VerificationCode,

		RefreshToken,

		AccessToken,
	}
}
