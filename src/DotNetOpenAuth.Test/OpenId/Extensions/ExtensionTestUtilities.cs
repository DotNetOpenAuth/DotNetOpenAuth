//-----------------------------------------------------------------------
// <copyright file="ExtensionTestUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Threading.Tasks;

	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Messaging;
	using Validation;

	public static class ExtensionTestUtilities {
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
		internal static async Task RoundtripAsync(
			Protocol protocol,
			IEnumerable<IOpenIdMessageExtension> requests,
			IEnumerable<IOpenIdMessageExtension> responses) {
			var securitySettings = new ProviderSecuritySettings();
			var cryptoKeyStore = new MemoryCryptoKeyStore();
			var associationStore = new ProviderAssociationHandleEncoder(cryptoKeyStore);
			Association association = HmacShaAssociationProvider.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart, associationStore, securitySettings);
			await CoordinatorBase.RunAsync(
				OpenIdTestBase.RelyingPartyDriver(async (rp, ct) => {
					RegisterExtension(rp.Channel, Mocks.MockOpenIdExtension.Factory);
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
					using (var httpClient = rp.Channel.HostFactories.CreateHttpClient()) {
						using (var redirectingResponse = await httpClient.GetAsync(redirectingRequest.Headers.Location, ct)) {
							redirectingResponse.EnsureSuccessStatusCode();
							redirectingResponseUri = redirectingResponse.Headers.Location;
						}
					}

					var response = await rp.Channel.ReadFromRequestAsync<PositiveAssertionResponse>(new HttpRequestMessage(HttpMethod.Get, redirectingResponseUri), ct);
					var receivedResponses = response.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(responses.ToArray(), receivedResponses.ToArray());
				}),
				OpenIdTestBase.HandleProvider(async (op, req, ct) => {
					RegisterExtension(op.Channel, Mocks.MockOpenIdExtension.Factory);
					var key = cryptoKeyStore.GetCurrentKey(ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, TimeSpan.FromSeconds(1));
					op.CryptoKeyStore.StoreKey(ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, key.Key, key.Value);
					var request = await op.Channel.ReadFromRequestAsync<CheckIdRequest>(req, ct);
					var response = new PositiveAssertionResponse(request);
					var receivedRequests = request.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(requests.ToArray(), receivedRequests.ToArray());

					foreach (var extensionResponse in responses) {
						response.Extensions.Add(extensionResponse);
					}

					return await op.Channel.PrepareResponseAsync(response, ct);
				}));
		}

		internal static void RegisterExtension(Channel channel, StandardOpenIdExtensionFactory.CreateDelegate extensionFactory) {
			Requires.NotNull(channel, "channel");

			var factory = (OpenIdExtensionFactoryAggregator)channel.BindingElements.OfType<ExtensionsBindingElement>().Single().ExtensionFactory;
			factory.Factories.OfType<StandardOpenIdExtensionFactory>().Single().RegisterExtension(extensionFactory);
		}
	}
}
