//-----------------------------------------------------------------------
// <copyright file="NonIdentityTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
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

			HandleProvider(
				async (op, req) => {
					var request = await op.Channel.ReadFromRequestAsync<SignedResponseRequest>(req, CancellationToken.None);
					Assert.IsNotInstanceOf<CheckIdRequest>(request);
					return new HttpResponseMessage();
				});

			{
				var rp = this.CreateRelyingParty();
				var request = new SignedResponseRequest(protocol.Version, OPUri, mode);
				var authRequest = await rp.Channel.PrepareResponseAsync(request);
				using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(authRequest.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}

		[Test]
		public async Task ExtensionOnlyFacadeLevel() {
			Protocol protocol = Protocol.V20;
			int opStep = 0;
			HandleProvider(
				async (op, req) => {
					switch (++opStep) {
						case 1:
							var assocRequest = await op.GetRequestAsync(req);
							return await op.PrepareResponseAsync(assocRequest);
						case 2:
							var request = (IAnonymousRequest)await op.GetRequestAsync(req);
							request.IsApproved = true;
							Assert.IsNotInstanceOf<CheckIdRequest>(request);
							return await op.PrepareResponseAsync(request);
						default:
							throw Assumes.NotReachable();
					}
				});

			{
				var rp = this.CreateRelyingParty();
				var request = await rp.CreateRequestAsync(GetMockIdentifier(protocol.ProtocolVersion), RPRealmUri, RPUri);

				request.IsExtensionOnly = true;
				var redirectRequest = await request.GetRedirectingResponseAsync();
				Uri redirectResponseUrl;
				this.HostFactories.AllowAutoRedirects = false;
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var redirectResponse = await httpClient.GetAsync(redirectRequest.Headers.Location)) {
						Assert.That(redirectResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
						redirectResponseUrl = redirectResponse.Headers.Location;
					}
				}

				IAuthenticationResponse response =
					await rp.GetResponseAsync(new HttpRequestMessage(HttpMethod.Get, redirectResponseUrl));
				Assert.AreEqual(AuthenticationStatus.ExtensionsOnly, response.Status);
			}
		}
	}
}
