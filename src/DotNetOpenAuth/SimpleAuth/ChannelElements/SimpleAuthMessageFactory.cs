//-----------------------------------------------------------------------
// <copyright file="SimpleAuthMessageFactory.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.SimpleAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.SimpleAuth.Messages;

	/// <summary>
	/// The message factory for Simple Auth messages.
	/// </summary>
	internal class SimpleAuthMessageFactory : IMessageFactory {
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleAuthMessageFactory"/> class.
		/// </summary>
		internal SimpleAuthMessageFactory() {
		}

		#region IMessageFactory Members

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="recipient">The intended or actual recipient of the request message.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			Version version = Protocol.DefaultVersion;

			if (fields.ContainsKey("sa_consumer_key") && fields.ContainsKey("sa_callback")) {
				return new UserAuthorizationInUserAgentRequest(recipient, version);
			}

			if (fields.ContainsKey("sa_consumer_key") && fields.ContainsKey("sa_verifier")) {
				return new RequestAccessTokenWithVerifier(recipient.Location, version);
			}

			if (fields.ContainsKey("sa_verifier")) {
				return new UserAuthorizationInUserAgentResponse(recipient.Location, version);
			}

			return null;
		}

		/// <summary>
		/// Analyzes an incoming request message payload to discover what kind of
		/// message is embedded in it and returns the type, or null if no match is found.
		/// </summary>
		/// <param name="request">The message that was sent as a request that resulted in the response.</param>
		/// <param name="fields">The name/value pairs that make up the message payload.</param>
		/// <returns>
		/// A newly instantiated <see cref="IProtocolMessage"/>-derived object that this message can
		/// deserialize to.  Null if the request isn't recognized as a valid protocol message.
		/// </returns>
		public IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			return null;
		}

		#endregion
	}
}
