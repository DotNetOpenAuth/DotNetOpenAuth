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

	/// <summary>
	/// Verifies that the WRAP message types are recognized.
	/// </summary>
	public class MessageFactoryTests : OAuthWrapTestBase {
		private readonly MessageReceivingEndpoint recipient = new MessageReceivingEndpoint("http://who", HttpDeliveryMethods.PostRequest);
		private OAuthWrapChannel channel;
		private IMessageFactory messageFactory;

		public override void SetUp() {
			base.SetUp();

			this.channel = new OAuthWrapChannel();
			this.messageFactory = OAuthWrapChannel_Accessor.AttachShadow(this.channel).MessageFactory;
		}

		#region Refresh Access Token messages

		[TestCase]
		public void RefreshAccessTokenRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.refresh_token, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RefreshAccessTokenRequest), request);
		}

		[TestCase]
		public void RefreshAccessTokenSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.access_token, "abc" },
			};
			var request = new RefreshAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(AccessTokenSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void RefreshAccessTokenFailedResponse() {
			var fields = new Dictionary<string, string> {
			};
			var request = new RefreshAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(AccessTokenFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		#endregion

		#region Web App profile messages

		[TestCase]
		public void WebAppRequestRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.redirect_uri, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(WebAppRequest), request);
		}

		[TestCase]
		public void WebAppFailedResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.error, "user_denied" },
			};
			var request = new WebAppRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(WebAppFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void WebAppSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.code, "abc" },
			};
			var request = new WebAppRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(WebAppSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void WebAppAccessTokenRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.client_secret, "abc" },
				{ Protocol.code, "abc" },
				{ Protocol.redirect_uri, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(WebAppAccessTokenRequest), request);
		}

		[TestCase, Ignore("Not implemented")]
		public void WebAppAccessTokenFailedResponse() {
			// HTTP 400 Bad Request
		}

		[TestCase, Ignore("Not implemented")]
		public void WebAppAccessTokenBadClientResponse() {
			// HTTP 401 Unauthorized
			// WWW-Authenticate: WRAP
		}

		#endregion

		#region Username and Password profile messages

		[TestCase]
		public void UserNamePasswordRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.wrap_username, "abc" },
				{ Protocol.wrap_password, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(UserNamePasswordRequest), request);
		}

		[TestCase]
		public void UserNamePasswordSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.access_token, "abc" },
			};
			var request = new UserNamePasswordRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(UserNamePasswordSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void UserNamePasswordVerificationResponse() {
			// HTTP 400 Bad Request
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_verification_url, "abc" },
			};
			var request = new UserNamePasswordRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(UserNamePasswordVerificationResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase, Ignore("Not implemented")]
		public void UserNamePasswordFailedResponse() {
			// HTTP 401 Unauthorized
			// WWW-Authenticate: WRAP
		}

		[TestCase]
		public void UsernamePasswordCaptchaResponse() {
			// HTTP 400 Bad Request
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_captcha_url, "abc" },
			};
			var request = new UserNamePasswordRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(UsernamePasswordCaptchaResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		#endregion

		#region Rich App profile messages

		[TestCase]
		public void RichAppRequest() {
			// include just required parameters
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RichAppRequest), request);

			// including optional parts
			fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.redirect_uri, "abc" },
				{ Protocol.state, "abc" },
				{ Protocol.scope, "abc" },
			};
			request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RichAppRequest), request);
		}

		[TestCase]
		public void RichAppResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.refresh_token, "abc" },
				{ Protocol.access_token, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RichAppResponse), request);
		}

		[TestCase]
		public void RichAppAccessTokenRequest() {
			// include just required parameters
			var fields = new Dictionary<string, string> {
				{ Protocol.client_id, "abc" },
				{ Protocol.code, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(RichAppAccessTokenRequest), request);
		}

		[TestCase]
		public void RichAppAccessTokenSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.refresh_token, "abc" },
				{ Protocol.access_token, "abc" },
			};
			var request = new RichAppAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(RichAppAccessTokenSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void RichAppAccessTokenFailedResponse() {
			// HTTP 401 Unauthorized
			// WWW-Authenticate: WRAP
			var fields = new Dictionary<string, string> {
			};
			var request = new RichAppAccessTokenRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(RichAppAccessTokenFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		#endregion

		#region Client Account and Password profile messages

		[TestCase]
		public void ClientAccountUsernamePasswordRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.wrap_name, "abc" },
				{ Protocol.wrap_password, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(ClientAccountUsernamePasswordRequest), request);
		}

		[TestCase]
		public void ClientAccountUsernamePasswordSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.refresh_token, "abc" },
				{ Protocol.access_token, "abc" },
			};
			var request = new ClientAccountUsernamePasswordRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(ClientAccountUsernamePasswordSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void ClientAccountUsernamePasswordFailedResponse() {
			// HTTP 401 Unauthorized
			// WWW-Authenticate: WRAP
			var fields = new Dictionary<string, string> {
			};
			var request = new ClientAccountUsernamePasswordRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(ClientAccountUsernamePasswordFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		#endregion

		#region Assertion profile messages

		[TestCase]
		public void AssertionRequest() {
			var fields = new Dictionary<string, string> {
				{ Protocol.format, "abc" },
				{ Protocol.assertion, "abc" },
			};
			IDirectedProtocolMessage request = this.messageFactory.GetNewRequestMessage(this.recipient, fields);
			Assert.IsInstanceOf(typeof(AssertionRequest), request);
		}

		[TestCase]
		public void AssertionSuccessResponse() {
			var fields = new Dictionary<string, string> {
				{ Protocol.access_token, "abc" },
			};
			var request = new AssertionRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(AssertionSuccessResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		[TestCase]
		public void AssertionFailedResponse() {
			// HTTP 401 Unauthorized
			// WWW-Authenticate: WRAP
			var fields = new Dictionary<string, string> {
			};
			var request = new AssertionRequest(this.recipient.Location, Protocol.Default.Version);
			Assert.IsInstanceOf(
				typeof(AssertionFailedResponse),
				this.messageFactory.GetNewResponseMessage(request, fields));
		}

		#endregion
	}
}
