//-----------------------------------------------------------------------
// <copyright file="OpenIdTestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Configuration;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Messaging;
	using DotNetOpenAuth.Test.Mocks;
	using DotNetOpenAuth.Test.OpenId.Extensions;

	using NUnit.Framework;

	using IAuthenticationRequest = DotNetOpenAuth.OpenId.Provider.IAuthenticationRequest;

	public class OpenIdTestBase : TestBase {
		protected internal const string IdentifierSelect = "http://specs.openid.net/auth/2.0/identifier_select";

		protected internal static readonly Uri BaseMockUri = new Uri("http://localhost/");
		protected internal static readonly Uri BaseMockUriSsl = new Uri("https://localhost/");

		protected internal static readonly Uri OPUri = new Uri(BaseMockUri, "/provider/endpoint");
		protected internal static readonly Uri OPUriSsl = new Uri(BaseMockUriSsl, "/provider/endpoint");
		protected internal static readonly Uri[] OPLocalIdentifiers = new[] { new Uri(OPUri, "/provider/someUser0"), new Uri(OPUri, "/provider/someUser1") };
		protected internal static readonly Uri[] OPLocalIdentifiersSsl = new[] { new Uri(OPUriSsl, "/provider/someUser0"), new Uri(OPUriSsl, "/provider/someUser1") };

		// Vanity URLs are Claimed Identifiers that delegate to some OP and its local identifier.
		protected internal static readonly Uri VanityUri = new Uri(BaseMockUri, "/userControlled/identity");
		protected internal static readonly Uri VanityUriSsl = new Uri(BaseMockUriSsl, "/userControlled/identity");

		protected internal static readonly Uri RPUri = new Uri(BaseMockUri, "/relyingparty/login");
		protected internal static readonly Uri RPUriSsl = new Uri(BaseMockUriSsl, "/relyingparty/login");
		protected internal static readonly Uri RPRealmUri = new Uri(BaseMockUri, "/relyingparty/");
		protected internal static readonly Uri RPRealmUriSsl = new Uri(BaseMockUriSsl, "/relyingparty/");

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdTestBase"/> class.
		/// </summary>
		internal OpenIdTestBase() {
			this.AutoProviderScenario = Scenarios.AutoApproval;
		}

		public enum Scenarios {
			AutoApproval,
			AutoApprovalAddFragment,
			ApproveOnSetup,
			AlwaysDeny,
		}

		internal Scenarios AutoProviderScenario { get; set; }

		protected RelyingPartySecuritySettings RelyingPartySecuritySettings { get; private set; }

		protected ProviderSecuritySettings ProviderSecuritySettings { get; private set; }

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.RelyingPartySecuritySettings = OpenIdElement.Configuration.RelyingParty.SecuritySettings.CreateSecuritySettings();
			this.ProviderSecuritySettings = OpenIdElement.Configuration.Provider.SecuritySettings.CreateSecuritySettings();

			this.AutoProviderScenario = Scenarios.AutoApproval;
			Identifier.EqualityOnStrings = true;
			this.HostFactories.InstallUntrustedWebReqestHandler = true;
		}

		[TearDown]
		public override void Cleanup() {
			base.Cleanup();

			Identifier.EqualityOnStrings = false;
		}

		/// <summary>
		/// Forces storage of an association in an RP's association store.
		/// </summary>
		/// <param name="relyingParty">The relying party.</param>
		/// <param name="providerEndpoint">The provider endpoint.</param>
		/// <param name="association">The association.</param>
		internal static void StoreAssociation(OpenIdRelyingParty relyingParty, Uri providerEndpoint, Association association) {
			// Only store the association if the RP is not in stateless mode.
			if (relyingParty.AssociationManager.AssociationStoreTestHook != null) {
				relyingParty.AssociationManager.AssociationStoreTestHook.StoreAssociation(providerEndpoint, association);
			}
		}

		/// <summary>
		/// Returns the content of a given embedded resource.
		/// </summary>
		/// <param name="path">The path of the file as it appears within the project,
		/// where the leading / marks the root directory of the project.</param>
		/// <returns>The content of the requested resource.</returns>
		internal static string LoadEmbeddedFile(string path) {
			if (!path.StartsWith("/")) {
				path = "/" + path;
			}
			path = "DotNetOpenAuth.Test.OpenId" + path.Replace('/', '.');
			Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			if (resource == null) {
				throw new ArgumentException();
			}
			using (StreamReader sr = new StreamReader(resource)) {
				return sr.ReadToEnd();
			}
		}

		internal static IdentifierDiscoveryResult GetServiceEndpoint(int user, ProtocolVersion providerVersion, int servicePriority, bool useSsl) {
			return GetServiceEndpoint(user, providerVersion, servicePriority, useSsl, false);
		}

		internal static IdentifierDiscoveryResult GetServiceEndpoint(int user, ProtocolVersion providerVersion, int servicePriority, bool useSsl, bool delegating) {
			var providerEndpoint = new ProviderEndpointDescription(
				useSsl ? OPUriSsl : OPUri,
				new string[] { Protocol.Lookup(providerVersion).ClaimedIdentifierServiceTypeURI });
			var local_id = useSsl ? OPLocalIdentifiersSsl[user] : OPLocalIdentifiers[user];
			var claimed_id = delegating ? (useSsl ? VanityUriSsl : VanityUri) : local_id;
			return IdentifierDiscoveryResult.CreateForClaimedIdentifier(
				claimed_id,
				claimed_id,
				local_id,
				providerEndpoint,
				servicePriority,
				10);
		}

		/// <summary>
		/// Gets a default implementation of a simple provider that responds to authentication requests
		/// per the scenario that is being simulated.
		/// </summary>
		/// <remarks>
		/// This is a very useful method to pass to the OpenIdCoordinator constructor for the Provider argument.
		/// </remarks>
		internal void RegisterAutoProvider() {
			this.Handle(OPUri).By(
				async (req, ct) => {
					var provider = new OpenIdProvider(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
					return await this.AutoProviderActionAsync(provider, req, ct);
				});
		}

		/// <summary>
		/// Gets a default implementation of a simple provider that responds to authentication requests
		/// per the scenario that is being simulated.
		/// </summary>
		/// <remarks>
		/// This is a very useful method to pass to the OpenIdCoordinator constructor for the Provider argument.
		/// </remarks>
		internal async Task<HttpResponseMessage> AutoProviderActionAsync(OpenIdProvider provider, HttpRequestMessage req, CancellationToken ct) {
			IRequest request = await provider.GetRequestAsync(req, ct);
			Assert.That(request, Is.Not.Null);

			if (!request.IsResponseReady) {
				var authRequest = (IAuthenticationRequest)request;
				switch (this.AutoProviderScenario) {
					case Scenarios.AutoApproval:
						authRequest.IsAuthenticated = true;
						break;
					case Scenarios.AutoApprovalAddFragment:
						authRequest.SetClaimedIdentifierFragment("frag");
						authRequest.IsAuthenticated = true;
						break;
					case Scenarios.ApproveOnSetup:
						authRequest.IsAuthenticated = !authRequest.Immediate;
						break;
					case Scenarios.AlwaysDeny:
						authRequest.IsAuthenticated = false;
						break;
					default:
						// All other scenarios are done programmatically only.
						throw new InvalidOperationException("Unrecognized scenario");
				}
			}

			return await provider.PrepareResponseAsync(request, ct);
		}

		internal Task<IEnumerable<IdentifierDiscoveryResult>> DiscoverAsync(Identifier identifier, CancellationToken cancellationToken = default(CancellationToken)) {
			var rp = this.CreateRelyingParty(true);
			return rp.DiscoverAsync(identifier, cancellationToken);
		}

		/// <summary>
		/// Simulates an extension request and response.
		/// </summary>
		/// <param name="protocol">The protocol to use in the roundtripping.</param>
		/// <param name="requests">The extensions to add to the request message.</param>
		/// <param name="responses">The extensions to add to the response message.</param>
		/// <remarks>
		/// This method relies on the extension objects' Equals methods to verify
		/// accurate transport.  The Equals methods should be verified by separate tests.
		/// </remarks>
		internal async Task RoundtripAsync(Protocol protocol, IEnumerable<IOpenIdMessageExtension> requests, IEnumerable<IOpenIdMessageExtension> responses) {
			var securitySettings = new ProviderSecuritySettings();
			var cryptoKeyStore = new MemoryCryptoKeyStore();
			var associationStore = new ProviderAssociationHandleEncoder(cryptoKeyStore);
			Association association = HmacShaAssociationProvider.Create(
				protocol,
				protocol.Args.SignatureAlgorithm.Best,
				AssociationRelyingPartyType.Smart,
				associationStore,
				securitySettings);

			this.HandleProvider(
				async (op, req) => {
					ExtensionTestUtilities.RegisterExtension(op.Channel, Mocks.MockOpenIdExtension.Factory);
					var key = cryptoKeyStore.GetCurrentKey(
						ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, TimeSpan.FromSeconds(1));
					op.CryptoKeyStore.StoreKey(
						ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, key.Key, key.Value);
					var request = await op.Channel.ReadFromRequestAsync<CheckIdRequest>(req, CancellationToken.None);
					var response = new PositiveAssertionResponse(request);
					var receivedRequests = request.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(requests.ToArray(), receivedRequests.ToArray());

					foreach (var extensionResponse in responses) {
						response.Extensions.Add(extensionResponse);
					}

					return await op.Channel.PrepareResponseAsync(response);
				});

			{
				var rp = this.CreateRelyingParty();
				ExtensionTestUtilities.RegisterExtension(rp.Channel, Mocks.MockOpenIdExtension.Factory);
				var requestBase = new CheckIdRequest(protocol.Version, OpenIdTestBase.OPUri, AuthenticationRequestMode.Immediate);
				OpenIdTestBase.StoreAssociation(rp, OpenIdTestBase.OPUri, association);
				requestBase.AssociationHandle = association.Handle;
				requestBase.ClaimedIdentifier = "http://claimedid";
				requestBase.LocalIdentifier = "http://localid";
				requestBase.ReturnTo = OpenIdTestBase.RPUri;

				foreach (IOpenIdMessageExtension extension in requests) {
					requestBase.Extensions.Add(extension);
				}

				var redirectingRequest = await rp.Channel.PrepareResponseAsync(requestBase);
				Uri redirectingResponseUri;
				this.HostFactories.AllowAutoRedirects = false;
				using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
					using (var redirectingResponse = await httpClient.GetAsync(redirectingRequest.Headers.Location)) {
						Assert.AreEqual(HttpStatusCode.Found, redirectingResponse.StatusCode);
						redirectingResponseUri = redirectingResponse.Headers.Location;
					}
				}

				var response =
					await
					rp.Channel.ReadFromRequestAsync<PositiveAssertionResponse>(
						new HttpRequestMessage(HttpMethod.Get, redirectingResponseUri), CancellationToken.None);
				var receivedResponses = response.Extensions.Cast<IOpenIdMessageExtension>();
				CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(responses.ToArray(), receivedResponses.ToArray());
			}
		}

		protected internal void HandleProvider(Func<OpenIdProvider, HttpRequestMessage, Task<HttpResponseMessage>> provider) {
			var op = this.CreateProvider();
			this.Handle(OPUri).By(async req => {
				return await provider(op, req);
			});
		}

		protected Realm GetMockRealm(bool useSsl) {
			var rpDescription = new RelyingPartyEndpointDescription(useSsl ? RPUriSsl : RPUri, new string[] { Protocol.V20.RPReturnToTypeURI });
			return new MockRealm(useSsl ? RPRealmUriSsl : RPRealmUri, rpDescription);
		}

		protected Identifier GetMockIdentifier(ProtocolVersion providerVersion) {
			return this.GetMockIdentifier(providerVersion, false);
		}

		protected Identifier GetMockIdentifier(ProtocolVersion providerVersion, bool useSsl) {
			return this.GetMockIdentifier(providerVersion, useSsl, false);
		}

		protected Identifier GetMockIdentifier(ProtocolVersion providerVersion, bool useSsl, bool delegating) {
			var se = GetServiceEndpoint(0, providerVersion, 10, useSsl, delegating);
			this.RegisterMockXrdsResponse(se);
			return se.ClaimedIdentifier;
		}

		protected Identifier GetMockDualIdentifier() {
			Protocol protocol = Protocol.Default;
			var opDesc = new ProviderEndpointDescription(OPUri, protocol.Version);
			var dualResults = new IdentifierDiscoveryResult[] {
				IdentifierDiscoveryResult.CreateForClaimedIdentifier(VanityUri.AbsoluteUri, OPLocalIdentifiers[0], opDesc, 10, 10),
				IdentifierDiscoveryResult.CreateForProviderIdentifier(protocol.ClaimedIdentifierForOPIdentifier, opDesc, 20, 20),
			};

			this.RegisterMockXrdsResponse(VanityUri, dualResults);
			return VanityUri;
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdRelyingParty"/> instance for general testing.
		/// </summary>
		/// <returns>The new instance.</returns>
		protected OpenIdRelyingParty CreateRelyingParty() {
			return this.CreateRelyingParty(false);
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdRelyingParty"/> instance for general testing.
		/// </summary>
		/// <param name="stateless">if set to <c>true</c> a stateless RP is created.</param>
		/// <returns>The new instance.</returns>
		protected OpenIdRelyingParty CreateRelyingParty(bool stateless) {
			var rp = new OpenIdRelyingParty(stateless ? null : new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
			return rp;
		}

		/// <summary>
		/// Creates a standard <see cref="OpenIdProvider"/> instance for general testing.
		/// </summary>
		/// <returns>The new instance.</returns>
		protected OpenIdProvider CreateProvider() {
			var op = new OpenIdProvider(new MemoryCryptoKeyAndNonceStore(), this.HostFactories);
			return op;
		}
	}
}
