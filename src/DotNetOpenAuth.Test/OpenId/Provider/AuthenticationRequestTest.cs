//-----------------------------------------------------------------------
// <copyright file="AuthenticationRequestTest.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Provider {
	using System;
	using System.IO;
	using System.Net.Http;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using System.Threading;
	using System.Threading.Tasks;

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
		[Test]
		public async Task UserSetupUrl() {
			// Construct a V1 immediate request
			Protocol protocol = Protocol.V11;
			OpenIdProvider provider = this.CreateProvider();
			var immediateRequest = new CheckIdRequest(protocol.Version, OPUri, DotNetOpenAuth.OpenId.AuthenticationRequestMode.Immediate);
			immediateRequest.Realm = RPRealmUri;
			immediateRequest.ReturnTo = RPUri;
			immediateRequest.LocalIdentifier = "http://somebody";
			var request = new AuthenticationRequest(provider, immediateRequest);

			// Now simulate the request being rejected and extract the user_setup_url
			request.IsAuthenticated = false;
			Uri userSetupUrl = ((NegativeAssertionResponse)await request.GetResponseAsync(CancellationToken.None)).UserSetupUrl;
			Assert.IsNotNull(userSetupUrl);

			// Now construct a new request as if it had just come in.
			var httpRequest = new HttpRequestMessage(HttpMethod.Get, userSetupUrl);
			var setupRequest = (AuthenticationRequest)await provider.GetRequestAsync(httpRequest);
			var setupRequestMessage = (CheckIdRequest)setupRequest.RequestMessage;

			// And make sure all the right properties are set.
			Assert.IsFalse(setupRequestMessage.Immediate);
			Assert.AreEqual(immediateRequest.Realm, setupRequestMessage.Realm);
			Assert.AreEqual(immediateRequest.ReturnTo, setupRequestMessage.ReturnTo);
			Assert.AreEqual(immediateRequest.LocalIdentifier, setupRequestMessage.LocalIdentifier);
			Assert.AreEqual(immediateRequest.Version, setupRequestMessage.Version);
		}

		/// <summary>
		/// Verifies that the AuthenticationRequest method is serializable.
		/// </summary>
		[Test]
		public void Serializable() {
			OpenIdProvider provider = this.CreateProvider();
			CheckIdRequest immediateRequest = new CheckIdRequest(Protocol.Default.Version, OPUri, DotNetOpenAuth.OpenId.AuthenticationRequestMode.Immediate);
			immediateRequest.Realm = RPRealmUri;
			immediateRequest.ReturnTo = RPUri;
			immediateRequest.LocalIdentifier = "http://somebody";
			AuthenticationRequest request = new AuthenticationRequest(provider, immediateRequest);

			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, request);

			ms.Position = 0;
			var req2 = (AuthenticationRequest)formatter.Deserialize(ms);
			Assert.That(req2, Is.Not.Null);
		}
	}
}
