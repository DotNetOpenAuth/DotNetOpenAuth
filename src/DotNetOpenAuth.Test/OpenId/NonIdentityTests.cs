//-----------------------------------------------------------------------
// <copyright file="NonIdentityTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Net.Http;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class NonIdentityTests : OpenIdTestBase {
		[Test]
		public async Task ExtensionOnlyChannelLevel() {
			Protocol protocol = Protocol.V20;
			var mode = AuthenticationRequestMode.Setup;

			await RunAsync(
				RelyingPartyDriver(async (rp, ct) => {
					var request = new SignedResponseRequest(protocol.Version, OPUri, mode);
					var authRequest = await rp.Channel.PrepareResponseAsync(request);
					using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
						using (var response = await httpClient.GetAsync(authRequest.Headers.Location, ct)) {
							response.EnsureSuccessStatusCode();
						}
					}
				}),
				HandleProvider(async (op, req, ct) => {
					var request = await op.Channel.ReadFromRequestAsync<SignedResponseRequest>(req, ct);
					Assert.IsNotInstanceOf<CheckIdRequest>(request);
					return new HttpResponseMessage();
				}));
		}

		[Test]
		public async Task ExtensionOnlyFacadeLevel() {
			Protocol protocol = Protocol.V20;
			int opStep = 0;
			await RunAsync(
				RelyingPartyDriver(async (rp, ct) => {
					var request = await rp.CreateRequestAsync(GetMockIdentifier(protocol.ProtocolVersion), RPRealmUri, RPUri, ct);

					request.IsExtensionOnly = true;
					var redirectRequest = await request.GetRedirectingResponseAsync(ct);
					Uri redirectResponseUrl;
					using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
						using (var redirectResponse = await httpClient.GetAsync(redirectRequest.Headers.Location, ct)) {
							redirectResponse.EnsureSuccessStatusCode();
							redirectResponseUrl = redirectRequest.Headers.Location;
						}
					}

					IAuthenticationResponse response = await rp.GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, redirectResponseUrl));
					Assert.AreEqual(AuthenticationStatus.ExtensionsOnly, response.Status);
				}),
				HandleProvider(async (op, req, ct) => {
					switch (++opStep) {
						case 1:
							var assocRequest = await op.GetRequestAsync(req, ct);
							return await op.PrepareResponseAsync(assocRequest, ct);
						case 2:
							var request = (IAnonymousRequest)await op.GetRequestAsync(req, ct);
							request.IsApproved = true;
							Assert.IsNotInstanceOf<CheckIdRequest>(request);
							return await op.PrepareResponseAsync(request, ct);
						default:
							throw Assumes.NotReachable();
					}
				}));
		}
	}
}
