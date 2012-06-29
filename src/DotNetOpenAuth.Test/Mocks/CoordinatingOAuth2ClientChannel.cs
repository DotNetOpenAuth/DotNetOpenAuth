//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuth2ClientChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	internal class CoordinatingOAuth2ClientChannel : CoordinatingChannel, IOAuth2ChannelWithClient {
		private OAuth2ClientChannel wrappedChannel;

		internal CoordinatingOAuth2ClientChannel(Channel wrappedChannel, Action<IProtocolMessage> incomingMessageFilter, Action<IProtocolMessage> outgoingMessageFilter)
			: base(wrappedChannel, incomingMessageFilter, outgoingMessageFilter) {
			this.wrappedChannel = (OAuth2ClientChannel)wrappedChannel;
		}

		public string ClientIdentifier {
			get { return this.wrappedChannel.ClientIdentifier; }
			set { this.wrappedChannel.ClientIdentifier = value; }
		}

		public DotNetOpenAuth.OAuth2.ClientCredentialApplicator ClientCredentialApplicator {
			get { return this.wrappedChannel.ClientCredentialApplicator; }
			set { this.wrappedChannel.ClientCredentialApplicator = value; }
		}
	}
}