//-----------------------------------------------------------------------
// <copyright file="MessageFactoryTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	/// <summary>
	/// Verifies that the WRAP message types are recognized.
	/// </summary>
	public class MessageFactoryTests : OAuth2TestBase {
		private readonly MessageReceivingEndpoint recipient = new MessageReceivingEndpoint("http://who", HttpDeliveryMethods.PostRequest);
		private OAuth2AuthorizationServerChannel channel;
		private IMessageFactory messageFactory;

		public override void SetUp() {
			base.SetUp();

			this.channel = new OAuth2AuthorizationServerChannel(new Mock<IAuthorizationServer>().Object);
			this.messageFactory = this.channel.MessageFactoryTestHook;
		}

		#region End user authorization messages

		[TestCase]
		public void EndUserAuthorizationRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.response_type, "code" },
				{ Protocol.client_id, "abc" },
				{ Protocol.redirect_uri, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(EndUserAuthorizationRequest), request);
		}

		[TestCase]
		public void EndUserAuthorizationSuccessResponseWithCode() {
			var fields = new Dictionary<string, string> {
				{ Protocol.code, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(EndUserAuthorizationSuccessResponseBase), request);
		}

		[TestCase, Ignore("Not yet supported")]
		public void EndUserAuthorizationSuccessResponseWithAccessToken() {
			var fields = new Dictionary<string, string> {
				{ Protocol.access_token, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(EndUserAuthorizationSuccessResponseBase), request);
		}

		[TestCase]
		public void EndUserAuthorizationFailedResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.error, "access-denied" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(EndUserAuthorizationFailedResponse), request);
		}

		#endregion

		#region Access token request messages

		[TestCase]
		public void AccessTokenRefreshRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.refresh_token, "abc" },
				{ Protocol.grant_type, "refresh-token" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AccessTokenRefreshRequest), request);
		}

		[TestCase]
		public void AccessTokenAuthorizationCodeRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.code, "code" },
				{ Protocol.grant_type, "authorization-code" },
				{ Protocol.redirect_uri, "http://someUri" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AccessTokenAuthorizationCodeRequest), request);
		}

		[TestCase]
		public void AccessTokenBasicCredentialsRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.client_secret, "abc" },
				{ Protocol.grant_type, "basic-credentials" },
				{ Protocol.username, "abc" },
				{ Protocol.password , "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AccessTokenResourceOwnerPasswordCredentialsRequest), request);
		}

		[TestCase]
		public void AccessTokenClientCredentialsRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.client_secret, "abc" },
				{ Protocol.grant_type, "none" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AccessTokenClientCredentialsRequest), request);
		}

		[TestCase]
		public void AccessTokenAssertionRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.assertion_type, "abc" },
				{ Protocol.assertion, "abc" },
				{ Protocol.grant_type, "assertion" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AccessTokenAssertionRequest), request);
		}

		#endregion
	}
}
