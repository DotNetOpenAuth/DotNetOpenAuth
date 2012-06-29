//-----------------------------------------------------------------------
// <copyright file="AuthorizationServerTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth2;
	using DotNetOpenAuth.OAuth2.Messages;
	using NUnit.Framework;

	/// <summary>
	/// Verifies authorization server functionality.
	/// </summary>
	[TestFixture]
	public class AuthorizationServerTests : OAuth2TestBase {
		/// <summary>
		/// Verifies that authorization server responds with an appropriate error response.
		/// </summary>
		[Test]
		public void ErrorResponseTest() {
			var coordinator = new OAuth2Coordinator<UserAgentClient>(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				new UserAgentClient(AuthorizationServerDescription),
				client => {
					var request = new AccessTokenAuthorizationCodeRequestC(AuthorizationServerDescription)
					{ ClientIdentifier = ClientId, ClientSecret = ClientSecret, AuthorizationCode = "foo" };

					var response = client.Channel.Request<AccessTokenFailedResponse>(request);
					Assert.That(response.Error, Is.Not.Null.And.Not.Empty);
					Assert.That(response.Error, Is.EqualTo(Protocol.AccessTokenRequestErrorCodes.InvalidRequest));
				},
				server => {
					server.HandleTokenRequest().Respond();
				});
			coordinator.Run();
		}
	}
}
