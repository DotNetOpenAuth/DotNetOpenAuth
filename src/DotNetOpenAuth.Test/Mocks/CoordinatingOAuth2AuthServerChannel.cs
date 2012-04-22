//-----------------------------------------------------------------------
// <copyright file="CoordinatingOAuth2AuthServerChannel.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;

	internal class CoordinatingOAuth2AuthServerChannel : CoordinatingChannel, IOAuth2ChannelWithAuthorizationServer {
		private OAuth2AuthorizationServerChannel wrappedChannel;

		internal CoordinatingOAuth2AuthServerChannel(Channel wrappedChannel, Action<IProtocolMessage> incomingMessageFilter, Action<IProtocolMessage> outgoingMessageFilter)
			: base(wrappedChannel, incomingMessageFilter, outgoingMessageFilter) {
			this.wrappedChannel = (OAuth2AuthorizationServerChannel)wrappedChannel;
		}

		public IAuthorizationServerHost AuthorizationServer {
			get { return this.wrappedChannel.AuthorizationServer; }
		}

		public IScopeSatisfiedCheck ScopeSatisfiedCheck {
			get { return this.wrappedChannel.ScopeSatisfiedCheck; }
			set { this.wrappedChannel.ScopeSatisfiedCheck = value; }
		}
	}
}
