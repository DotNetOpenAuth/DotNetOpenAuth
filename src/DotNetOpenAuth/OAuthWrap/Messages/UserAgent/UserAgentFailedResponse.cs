//-----------------------------------------------------------------------
// <copyright file="UserAgentFailedResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class UserAgentFailedResponse : MessageBase {
		/// <summary>
		/// A constant parameter that indicates the user refused to grant the requested authorization.
		/// </summary>
		[MessagePart(Protocol.error, IsRequired = true)]
		private const string ErrorReason = Protocol.user_denied;

		internal UserAgentFailedResponse(Uri clientCallback, Version version)
			: base(version, MessageTransport.Indirect, clientCallback) {
		}

		/// <summary>
		/// Gets or sets the state of the client that was supplied to the Authorization Server.
		/// </summary>
		/// <value>
		/// An opaque value that Clients can use to maintain state associated with the authorization request.
		/// </value>
		/// <remarks>
		/// If this value is present, the Authorization Server MUST return it to the Client's callback URL.
		/// </remarks>
		[MessagePart(Protocol.state, IsRequired = false, AllowEmpty = true)]
		public string ClientState { get; set; }
	}
}
