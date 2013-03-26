//-----------------------------------------------------------------------
// <copyright file="OAuth2TestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.ChannelElements;
	using DotNetOpenAuth.OAuth2.Messages;
	using Moq;

	public class OAuth2TestBase : TestBase {
		protected internal const string ClientId = "TestClientId";

		protected internal const string ClientSecret = "TestClientSecret";

		protected const string ResourceOwnerUsername = "TestUser";

		protected const string ResourceOwnerPassword = "TestUserPassword";

		protected static readonly string[] TestScopes = new[] { "Scope1", "Scope2" };

		protected static readonly Uri ClientCallback = new Uri("http://client/callback");

		protected static readonly RSACryptoServiceProvider AsymmetricKey = new RSACryptoServiceProvider(512);

		protected static readonly AuthorizationServerDescription AuthorizationServerDescription = new AuthorizationServerDescription {
			AuthorizationEndpoint = new Uri("https://authserver/authorize"),
			TokenEndpoint = new Uri("https://authserver/token"),
		};

		protected static readonly IClientDescription ClientDescription = new ClientDescription(ClientSecret, ClientCallback);

		protected static readonly IAuthorizationServerHost AuthorizationServerMock = CreateAuthorizationServerMock().Object;

		protected static Mock<IAuthorizationServerHost> CreateAuthorizationServerMock() {
			var authHostMock = new Mock<IAuthorizationServerHost>();
			var cryptoStore = new MemoryCryptoKeyStore();
			authHostMock.Setup(m => m.GetClient(ClientId)).Returns(ClientDescription);
			authHostMock.SetupGet(m => m.CryptoKeyStore).Returns(cryptoStore);
			authHostMock.Setup(
				m =>
				m.IsAuthorizationValid(
					It.Is<IAuthorizationDescription>(
						d =>
						d.ClientIdentifier == ClientId && d.User == ResourceOwnerUsername &&
						MessagingUtilities.AreEquivalent(d.Scope, TestScopes)))).Returns(true);
			authHostMock
				.Setup(m => m.CheckAuthorizeResourceOwnerCredentialGrant(ResourceOwnerUsername, ResourceOwnerPassword, It.IsAny<IAccessTokenRequest>()))
				.Returns<string, string, IAccessTokenRequest>((p1, p2, p3) => new AutomatedUserAuthorizationCheckResponse(p3, true, ResourceOwnerUsername));
			authHostMock.Setup(m => m.CreateAccessToken(It.IsAny<IAccessTokenRequest>())).Returns(new AccessTokenResult(new AuthorizationServerAccessToken() { AccessTokenSigningKey = AsymmetricKey }));
			return authHostMock;
		}
	}
}
