//-----------------------------------------------------------------------
// <copyright file="ExtensionTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Extensions {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenId.Extensions;
	using DotNetOpenAuth.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Messages;
	using System.Collections.ObjectModel;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Test.Messaging;

	public class ExtensionTestBase : OpenIdTestBase {
		protected const ProtocolVersion Version = ProtocolVersion.V20;

		[TestInitialize]
		public virtual void Setup() {
			base.SetUp();
		}

		internal void Roundtrip(
			Protocol protocol,
			IEnumerable<IOpenIdMessageExtension> requests,
			IEnumerable<IOpenIdMessageExtension> responses) {
			Association association = HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.Best, AssociationRelyingPartyType.Smart);
			var coordinator = new OpenIdCoordinator(
				rp => {
					var requestBase = new CheckIdRequest(protocol.Version, ProviderUri, true);
					rp.AssociationStore.StoreAssociation(ProviderUri, association);
					requestBase.AssociationHandle = association.Handle;
					requestBase.ClaimedIdentifier = "http://claimedid";
					requestBase.LocalIdentifier = "http://localid";
					requestBase.ReturnTo = RPUri;

					foreach (IOpenIdMessageExtension extension in requests) {
						requestBase.Extensions.Add(extension);
					}

					rp.Channel.Send(requestBase);
					var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();

					var receivedResponses = response.Extensions.Cast<IOpenIdMessageExtension>();
					CollectionAssert<IOpenIdMessageExtension>.AreEquivalentByEquality(responses.ToArray(), receivedResponses.ToArray());
				},
				op => {
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
	}
}
