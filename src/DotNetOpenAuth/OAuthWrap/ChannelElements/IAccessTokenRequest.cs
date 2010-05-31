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

	internal interface IAccessTokenRequest : IDirectedProtocolMessage {
		string ClientIdentifier { get; }
		
		string Scope { get; }

		string SecretType { get; set; }
	}
}
