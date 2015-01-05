//-----------------------------------------------------------------------
// <copyright file="AuthenticationTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Logging;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;
	using Validation;

	[TestFixture]
	public class AuthenticationTests : OpenIdTestBase {
		[SetUp]
		public override void SetUp() {
			base.SetUp();
		}

		[Test]
		public async Task SharedAssociationPositive() {
			await this.ParameterizedAuthenticationTestAsync(true, true, false);
		}

		/// <summary>
		/// Verifies that a shared association protects against tampering.
		/// </summary>
		[Test]
		public async Task SharedAssociationTampered() {
			await this.ParameterizedAuthenticationTestAsync(true, true, true);
		}

		[Test]
		public async Task SharedAssociationNegative() {
			await this.ParameterizedAuthenticationTestAsync(true, false, false);
		}

		[Test]
		public async Task PrivateAssociationPositive() {
			await this.ParameterizedAuthenticationTestAsync(false, true, false);
		}

		/// <summary>
		/// Verifies that a private association protects against tampering.
		/// </summary>
		[Test]
		public async Task PrivateAssociationTampered() {
			await this.ParameterizedAuthenticationTestAsync(false, true, true);
		}

		[Test]
		public async Task NoAssociationNegative() {
			await this.ParameterizedAuthenticationTestAsync(false, false, false);
		}

		[Test]
		public async Task UnsolicitedAssertion() {
			var opStore = new MemoryCryptoKeyAndNonceStore();
			Handle(RPUri).By(
				async req => {
					var rp = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
					IAuthenticationResponse response = await rp.GetResponseAsync(req);
					Assert.That(response, Is.Not.Null);
					Assert.AreEqual(AuthenticationStatus.Authenticated, response.Status);
					return new HttpResponseMessage();
				});
			Handle(OPUri).By(
				async (req, ct) => {
					var op = new OpenIdProvider(opStore, this.HostFactories);
					return await this.AutoProviderActionAsync(op, req, ct);
				});
			this.RegisterMockRPDiscovery(ssl: false);

			{
				var op = new OpenIdProvider(opStore, this.HostFactories);
				Identifier id = GetMockIdentifier(ProtocolVersion.V20);
				var assertion = await op.PrepareUnsolicitedAssertionAsync(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);

				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(assertion.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}

		[Test]
		public async Task UnsolicitedAssertionRejected() {
			var opStore = new MemoryCryptoKeyAndNonceStore();
			Handle(RPUri).By(
				async req => {
					var rp = new OpenIdRelyingParty(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
					rp.SecuritySettings.RejectUnsolicitedAssertions = true;
					IAuthenticationResponse response = await rp.GetResponseAsync(req);
					Assert.That(response, Is.Not.Null);
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
					return new HttpResponseMessage();
				});
			Handle(OPUri).By(
				async req => {
					var op = new OpenIdProvider(opStore, this.HostFactories);
					return await this.AutoProviderActionAsync(op, req, CancellationToken.None);
				});
			this.RegisterMockRPDiscovery(ssl: false);

			{
				var op = new OpenIdProvider(opStore, this.HostFactories);
				Identifier id = GetMockIdentifier(ProtocolVersion.V20);
				var assertion = await op.PrepareUnsolicitedAssertionAsync(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(assertion.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}

		/// <summary>
		/// Verifies that delegating identifiers are rejected in unsolicited assertions
		/// when the appropriate security setting is set.
		/// </summary>
		[Test]
		public async Task UnsolicitedDelegatingIdentifierRejection() {
			var opStore = new MemoryCryptoKeyAndNonceStore();
			Handle(RPUri).By(
				async req => {
					var rp = this.CreateRelyingParty();
					rp.SecuritySettings.RejectDelegatingIdentifiers = true;
					IAuthenticationResponse response = await rp.GetResponseAsync(req);
					Assert.That(response, Is.Not.Null);
					Assert.AreEqual(AuthenticationStatus.Failed, response.Status);
					return new HttpResponseMessage();
				});
			Handle(OPUri).By(
				async req => {
					var op = new OpenIdProvider(opStore, this.HostFactories);
					return await this.AutoProviderActionAsync(op, req, CancellationToken.None);
				});
			this.RegisterMockRPDiscovery(ssl: false);

			{
				var op = new OpenIdProvider(opStore, this.HostFactories);
				Identifier id = GetMockIdentifier(ProtocolVersion.V20, false, true);
				var assertion = await op.PrepareUnsolicitedAssertionAsync(OPUri, RPRealmUri, id, OPLocalIdentifiers[0]);
				using (var httpClient = this.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(assertion.Headers.Location)) {
						response.EnsureSuccessStatusCode();
					}
				}
			}
		}

		private async Task ParameterizedAuthenticationTestAsync(bool sharedAssociation, bool positive, bool tamper) {
			foreach (Protocol protocol in Protocol.AllPracticalVersions) {
				foreach (bool statelessRP in new[] { false, true }) {
					if (sharedAssociation && statelessRP) {
						// Skip the invalid combination scenario.
						continue;
					}

					foreach (bool immediate in new[] { false, true }) {
						TestLogger.InfoFormat("Beginning authentication test scenario.  OpenID: {0}, Shared: {1}, positive: {2}, tamper: {3}, stateless: {4}, immediate: {5}", protocol.Version, sharedAssociation, positive, tamper, statelessRP, immediate);
						await this.ParameterizedAuthenticationTestAsync(protocol, statelessRP, sharedAssociation, positive, immediate, tamper);
					}
				}
			}
		}

		private async Task ParameterizedAuthenticationTestAsync(Protocol protocol, bool statelessRP, bool sharedAssociation, bool positive, bool immediate, bool tamper) {
			Requires.That(!statelessRP || !sharedAssociation, null, "The RP cannot be stateless while sharing an association with the OP.");
			Requires.That(positive || !tamper, null, "Cannot tamper with a negative response.");
			var securitySettings = new ProviderSecuritySettings();
			var cryptoKeyStore = new MemoryCryptoKeyStore();
			var associationStore = new ProviderAssociationHandleEncoder(cryptoKeyStore);
			Association association = sharedAssociation ? HmacShaAssociationProvider.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart, associationStore, securitySettings) : null;
			int opStep = 0;
			HandleProvider(
				async (op, req) => {
					if (association != null) {
						var key = cryptoKeyStore.GetCurrentKey(
							ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, TimeSpan.FromSeconds(1));
						op.CryptoKeyStore.StoreKey(
							ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, key.Key, key.Value);
					}

					switch (++opStep) {
						case 1:
							var request = await op.Channel.ReadFromRequestAsync<CheckIdRequest>(req, CancellationToken.None);
							Assert.IsNotNull(request);
							IProtocolMessage response;
							if (positive) {
								response = new PositiveAssertionResponse(request);
							} else {
								response = await NegativeAssertionResponse.CreateAsync(request, CancellationToken.None, op.Channel);
							}

							return await op.Channel.PrepareResponseAsync(response);
						case 2:
							if (positive && (statelessRP || !sharedAssociation)) {
								var checkauthRequest =
									await op.Channel.ReadFromRequestAsync<CheckAuthenticationRequest>(req, CancellationToken.None);
								var checkauthResponse = new CheckAuthenticationResponse(checkauthRequest.Version, checkauthRequest);
								checkauthResponse.IsValid = checkauthRequest.IsValid;
								return await op.Channel.PrepareResponseAsync(checkauthResponse);
							}

							throw Assumes.NotReachable();
						case 3:
							if (positive && (statelessRP || !sharedAssociation)) {
								if (!tamper) {
									// Respond to the replay attack.
									var checkauthRequest =
										await op.Channel.ReadFromRequestAsync<CheckAuthenticationRequest>(req, CancellationToken.None);
									var checkauthResponse = new CheckAuthenticationResponse(checkauthRequest.Version, checkauthRequest);
									checkauthResponse.IsValid = checkauthRequest.IsValid;
									return await op.Channel.PrepareResponseAsync(checkauthResponse);
								}
							}

							throw Assumes.NotReachable();
						default:
							throw Assumes.NotReachable();
					}
				});

			{
				var rp = this.CreateRelyingParty(statelessRP);
				if (tamper) {
					rp.Channel.IncomingMessageFilter = message => {
						var assertion = message as PositiveAssertionResponse;
						if (assertion != null) {
							// Alter the Local Identifier between the Provider and the Relying Party.
							// If the signature binding element does its job, this should cause the RP
							// to throw.
							assertion.LocalIdentifier = "http://victim";
						}
					};
				}

				var request = new CheckIdRequest(
					protocol.Version, OPUri, immediate ? AuthenticationRequestMode.Immediate : AuthenticationRequestMode.Setup);

				if (association != null) {
					StoreAssociation(rp, OPUri, association);
					request.AssociationHandle = association.Handle;
				}

				request.ClaimedIdentifier = "http://claimedid";
				request.LocalIdentifier = "http://localid";
				request.ReturnTo = RPUri;
				request.Realm = RPUri;
				var redirectRequest = await rp.Channel.PrepareResponseAsync(request);
				Uri redirectResponse;
				this.HostFactories.AllowAutoRedirects = false;
				using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
					using (var response = await httpClient.GetAsync(redirectRequest.Headers.Location)) {
						Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
						redirectResponse = response.Headers.Location;
					}
				}

				var assertionMessage = new HttpRequestMessage(HttpMethod.Get, redirectResponse);
				if (positive) {
					if (tamper) {
						try {
							await rp.Channel.ReadFromRequestAsync<PositiveAssertionResponse>(assertionMessage, CancellationToken.None);
							Assert.Fail("Expected exception {0} not thrown.", typeof(InvalidSignatureException).Name);
						} catch (InvalidSignatureException) {
							TestLogger.InfoFormat(
								"Caught expected {0} exception after tampering with signed data.", typeof(InvalidSignatureException).Name);
						}
					} else {
						var response =
							await rp.Channel.ReadFromRequestAsync<PositiveAssertionResponse>(assertionMessage, CancellationToken.None);
						Assert.IsNotNull(response);
						Assert.AreEqual(request.ClaimedIdentifier, response.ClaimedIdentifier);
						Assert.AreEqual(request.LocalIdentifier, response.LocalIdentifier);
						Assert.AreEqual(request.ReturnTo, response.ReturnTo);

						// Attempt to replay the message and verify that it fails.
						// Because in various scenarios and protocol versions different components
						// notice the replay, we can get one of two exceptions thrown.
						// When the OP notices the replay we get a generic InvalidSignatureException.
						// When the RP notices the replay we get a specific ReplayMessageException.
						try {
							await rp.Channel.ReadFromRequestAsync<PositiveAssertionResponse>(assertionMessage, CancellationToken.None);
							Assert.Fail("Expected ProtocolException was not thrown.");
						} catch (ProtocolException ex) {
							Assert.IsTrue(
								ex is ReplayedMessageException || ex is InvalidSignatureException,
								"A {0} exception was thrown instead of the expected {1} or {2}.",
								ex.GetType(),
								typeof(ReplayedMessageException).Name,
								typeof(InvalidSignatureException).Name);
						}
					}
				} else {
					var response =
						await rp.Channel.ReadFromRequestAsync<NegativeAssertionResponse>(assertionMessage, CancellationToken.None);
					Assert.IsNotNull(response);
					if (immediate) {
						// Only 1.1 was required to include user_setup_url
						if (protocol.Version.Major < 2) {
							Assert.IsNotNull(response.UserSetupUrl);
						}
					} else {
						Assert.IsNull(response.UserSetupUrl);
					}
				}
			}
		}
	}
}
