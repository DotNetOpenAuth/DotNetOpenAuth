//-----------------------------------------------------------------------
// <copyright file="UserAgentRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A message requesting user authorization to access protected data on behalf
	/// of an installed application or browser-hosted Javascript.
	/// </summary>
	[Serializable]
	internal class UserAgentRequest : EndUserAuthorizationRequest {
		/// <summary>
		/// The type of message.
		/// </summary>
		[MessagePart(Protocol.type, IsRequired = true)]
		private const string Type = "user_agent";

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentRequest"/> class.
		/// </summary>
		/// <param name="authorizationEndpoint">The authorization endpoint.</param>
		/// <param name="version">The version.</param>
		internal UserAgentRequest(Uri authorizationEndpoint, Version version)
			: base(authorizationEndpoint, version) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UserAgentRequest"/> class.
		/// </summary>
		/// <param name="authorizationServer">The authorization server.</param>
		internal UserAgentRequest(AuthorizationServerDescription authorizationServer)
			: base(authorizationServer) {
		}
	}
}
