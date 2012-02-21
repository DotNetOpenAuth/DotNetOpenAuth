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
		public void AuthCodeGrantAuthorization() {
			var coordinator = new OAuth2Coordinator(
				AuthorizationServerDescription,
				AuthorizationServerMock,
				client => {
					var authState = new AuthorizationState {
						Callback = ClientCallback,
					};
					client.PrepareRequestUserAuthorization(authState).Respond();
					var result = client.ProcessUserAuthorization();
					Assert.IsNotNullOrEmpty(result.AccessToken);
					Assert.IsNotNullOrEmpty(result.RefreshToken);
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
