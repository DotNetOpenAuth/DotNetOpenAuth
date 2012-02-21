//-----------------------------------------------------------------------
// <copyright file="OAuth2TestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using Moq;

	public class OAuth2TestBase : TestBase {
		protected internal const string ClientId = "TestClientId";

		protected internal const string ClientSecret = "TestClientSecret";

		protected const string Username = "TestUser";

		protected static readonly Uri ClientCallback = new Uri("http://client/callback");

		protected static readonly AuthorizationServerDescription AuthorizationServerDescription = new AuthorizationServerDescription {
			AuthorizationEndpoint = new Uri("https://authserver/authorize"),
			TokenEndpoint = new Uri("https://authserver/token"),
		};

		protected static readonly IClientDescription ClientDescription = new ClientDescription(
			ClientSecret,
			ClientCallback,
			ClientType.Confidential);

		protected static readonly IAuthorizationServer AuthorizationServerMock = CreateAuthorizationServerMock().Object;

		protected static Mock<IAuthorizationServer> CreateAuthorizationServerMock() {
			var authHostMock = new Mock<IAuthorizationServer>();
			var cryptoStore = new MemoryCryptoKeyStore();
			authHostMock.Setup(m => m.GetClient(ClientId)).Returns(ClientDescription);
			authHostMock.SetupGet(m => m.CryptoKeyStore).Returns(cryptoStore);
			authHostMock.Setup(m => m.IsAuthorizationValid(It.Is<IAuthorizationDescription>(d => d.ClientIdentifier == ClientId && d.User == Username))).Returns(true);
			return authHostMock;
		}
	}
}
