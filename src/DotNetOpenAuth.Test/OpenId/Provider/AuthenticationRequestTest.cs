//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using NUnit.Framework;

	[TestFixture]
	public class AuthenticationRequestTest : OpenIdTestBase {
		/// <summary>
		/// Verifies the user_setup_url is set properly for immediate negative responses.
		/// </summary>
		[TestCase]
		public void UserSetupUrl() {
			// Construct a V1 immediate request
			Protocol protocol = Protocol.V11;
			OpenIdProvider provider = this.CreateProvider();
			CheckIdRequest immediateRequest = new CheckIdRequest(protocol.Version, OPUri, DotNetOpenAuth.OpenId.AuthenticationRequestMode.Immediate);
			immediateRequest.Realm = RPRealmUri;
			immediateRequest.ReturnTo = RPUri;
			immediateRequest.LocalIdentifier = "http://somebody";
			AuthenticationRequest request = new AuthenticationRequest(provider, immediateRequest);

			// Now simulate the request being rejected and extract the user_setup_url
			request.IsAuthenticated = false;
			Uri userSetupUrl = ((NegativeAssertionResponse)request.Response).UserSetupUrl;
			Assert.IsNotNull(userSetupUrl);

			// Now construct a new request as if it had just come in.
			HttpRequestInfo httpRequest = new HttpRequestInfo { UrlBeforeRewriting = userSetupUrl };
			var setupRequest = (AuthenticationRequest)provider.GetRequest(httpRequest);
			var setupRequestMessage = (CheckIdRequest)setupRequest.RequestMessage;

			// And make sure all the right properties are set.
			Assert.IsFalse(setupRequestMessage.Immediate);
			Assert.AreEqual(immediateRequest.Realm, setupRequestMessage.Realm);
			Assert.AreEqual(immediateRequest.ReturnTo, setupRequestMessage.ReturnTo);
			Assert.AreEqual(immediateRequest.LocalIdentifier, setupRequestMessage.LocalIdentifier);
			Assert.AreEqual(immediateRequest.Version, setupRequestMessage.Version);
		}
	}
}
