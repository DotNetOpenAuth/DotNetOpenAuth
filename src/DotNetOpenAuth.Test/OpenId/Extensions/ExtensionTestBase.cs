//-----------------------------------------------------------------------
// <copyright file="ExtensionTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.Test.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Extensions;

	public static class ExtensionTestUtilities {
		internal static void Roundtrip(
			Protocol protocol,
			IEnumerable<IOpenIdMessageExtension> requests,
			IEnumerable<IOpenIdMessageExtension> responses) {
			Association association = HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart);
			var coordinator = new OpenIdCoordinator(
				rp => {
					RegisterExtension(rp.Channel, Mocks.MockOpenIdExtension.Factory);
					var requestBase = new CheckIdRequest(protocol.Version, OpenIdTestBase.ProviderUri, true);
					rp.AssociationStore.StoreAssociation(OpenIdTestBase.ProviderUri, association);
					requestBase.AssociationHandle = association.Handle;
					requestBase.ClaimedIdentifier = "http://claimedid";
					requestBase.LocalIdentifier = "http://localid";
					requestBase.ReturnTo = OpenIdTestBase.RPUri;

					foreach (IOpenIdMessageExtension extension in requests) {
						requestBase.Extensions.Add(extension);
					}

					rp.Channel.Send(requestBase);
					var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();

					var receivedResponses = response.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(responses.ToArray(), receivedResponses.ToArray());
				},
				op => {
					RegisterExtension(op.Channel, Mocks.MockOpenIdExtension.Factory);
					op.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					var response = new PositiveAssertionResponse(request);
					var receivedRequests = request.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(requests.ToArray(), receivedRequests.ToArray());

					foreach (var extensionResponse in responses) {
						response.Extensions.Add(extensionResponse);
					}

					op.Channel.Send(response);
				});
			coordinator.Run();
		}

		internal static void RegisterExtension(Channel channel, OpenIdExtensionFactory.CreateDelegate extensionFactory) {
			ErrorUtilities.VerifyArgumentNotNull(channel, "channel");

			OpenIdExtensionFactory factory = (OpenIdExtensionFactory)channel.BindingElements.OfType<ExtensionsBindingElement>().Single().ExtensionFactory;
			factory.RegisterExtension(extensionFactory);
		}
	}
}
