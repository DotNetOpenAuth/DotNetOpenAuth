//-----------------------------------------------------------------------
// <copyright file="MessageFactoryTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuthWrap {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuthWrap;
	using DotNetOpenAuth.OAuthWrap.ChannelElements;
	using DotNetOpenAuth.OAuthWrap.Messages;
	using NUnit.Framework;

	public class MessageFactoryTests {
		private OAuthWrapChannel channel;
		private IMessageFactory messageFactory;
		private MessageReceivingEndpoint recipient = new MessageReceivingEndpoint("http://who", HttpDeliveryMethods.PostRequest);

		public override void SetUp() {
			base.SetUp();

			this.channel = new OAuthWrapChannel();
			this.messageFactory = OAuthWrapChannel_Accessor.AttachShadow(this.channel).MessageFactory;
		}

		/// <summary>
		/// Verifies that the WRAP message types are initialized.
		/// </summary>
		[TestCase]
		public void RefreshAccessTokenRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_refresh_token, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RefreshAccessTokenRequest), request);
		}

		[TestCase]
		public void RefreshAccessTokenSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_access_token, "abc" },
			};
			var request = new RefreshAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(RefreshAccessTokenSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void RefreshAccessTokenFailedResponse() {
			var fields = new Dictionary<string, string> {
			};
			var request = new RefreshAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(RefreshAccessTokenFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}
	}
}
