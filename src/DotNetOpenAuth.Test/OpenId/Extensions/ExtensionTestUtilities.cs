//-----------------------------------------------------------------------
// <copyright file="ExtensionTestUtilities.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Messaging;

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
		internal static void Roundtrip(
			Protocol protocol,
			IEnumerable<IOpenIdMessageExtension> requests,
			IEnumerable<IOpenIdMessageExtension> responses) {
			var securitySettings = new ProviderSecuritySettings();
			var cryptoKeyStore = new MemoryCryptoKeyStore();
			var associationStore = new ProviderAssociationHandleEncoder(cryptoKeyStore);
			Association association = HmacShaAssociationProvider.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart, associationStore, securitySettings);
			var coordinator = new OpenIdCoordinator(
				rp => {
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

					rp.Channel.Respond(requestBase);
					var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();

					var receivedResponses = response.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(responses.ToArray(), receivedResponses.ToArray());
				},
				op => {
					RegisterExtension(op.Channel, Mocks.MockOpenIdExtension.Factory);
					var key = cryptoKeyStore.GetCurrentKey(ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, TimeSpan.FromSeconds(1));
					op.CryptoKeyStore.StoreKey(ProviderAssociationHandleEncoder.AssociationHandleEncodingSecretBucket, key.Key, key.Value);
					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					var response = new PositiveAssertionResponse(request);
					var receivedRequests = request.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(requests.ToArray(), receivedRequests.ToArray());

					foreach (var extensionResponse in responses) {
						response.Extensions.Add(extensionResponse);
					}

					op.Channel.Respond(response);
				});
			coordinator.Run();
		}

		internal static void RegisterExtension(Channel channel, StandardOpenIdExtensionFactory.CreateDelegate extensionFactory) {
			Requires.NotNull(channel, "channel");

			var factory = (OpenIdExtensionFactoryAggregator)channel.BindingElements.OfType<ExtensionsBindingElement>().Single().ExtensionFactory;
			factory.Factories.OfType<StandardOpenIdExtensionFactory>().Single().RegisterExtension(extensionFactory);
		}
	}
}
