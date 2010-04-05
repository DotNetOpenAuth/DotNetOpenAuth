//-----------------------------------------------------------------------
// <copyright file="OAuthWrapChannelTests.cs" company="Andrew Arnott">
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

	[TestFixture]
	public class OAuthWrapChannelTests : OAuthWrapTestBase {
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
		public void MessageFactory() {
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_refresh_token, "abc" },
			};
			IDirectedProtocolMessage request = messageFactory.GetNewRequestMessage(recipient, fields);
			Assert.IsInstanceOf(typeof(RefreshAccessTokenRequest), request);

			fields.Clear();
			fields[Protocol.wrap_access_token] = "abc";
			Assert.IsInstanceOf(typeof(RefreshAccessTokenSuccessResponse), messageFactory.GetNewResponseMessage(request, fields));

			fields.Clear();
			Assert.IsInstanceOf(typeof(RefreshAccessTokenFailedResponse), messageFactory.GetNewResponseMessage(request, fields));
		}
	}
}
