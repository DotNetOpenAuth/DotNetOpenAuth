//-----------------------------------------------------------------------
// <copyright file="AuthorizeTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class AuthorizeTests : OAuth2TestBase {
		[TestCase]
		public void Test1() {
			var authHostMock = new Mock<IAuthorizationServer>();
			var cryptoStore = new MemoryCryptoKeyStore();
			authHostMock.Setup(m => m.GetClient(ClientId)).Returns(ClientDescription);
			authHostMock.SetupGet(m => m.CryptoKeyStore).Returns(cryptoStore);
			authHostMock.Setup(m => m.IsAuthorizationValid(It.Is<IAuthorizationDescription>(d => d.ClientIdentifier == ClientId && d.User == Username))).Returns(true);
			var coordinator = new OAuth2Coordinator(
				AuthorizationServerDescription,
				authHostMock.Object,
				client => {
					var authState = new AuthorizationState {
						Callback = ClientCallback,
					};
					client.PrepareRequestUserAuthorization(authState).Respond();
					var result = client.ProcessUserAuthorization();
					Assert.IsNotNull(result.AccessToken);
				},
				server => {
					var request = server.ReadAuthorizationRequest();
					server.ApproveAuthorizationRequest(request, Username);
					var tokenRequest = server.ReadAccessTokenRequest();
					var tokenResponse = server.PrepareAccessTokenResponse(tokenRequest);
					server.Channel.Respond(tokenResponse);
				});
			coordinator.Run();
		}
	}
}
