//-----------------------------------------------------------------------
// <copyright file="AuthenticationTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class AuthenticationTests : OpenIdTestBase {
		[TestInitialize]
		public override void SetUp() {
			base.SetUp();
		}

		[TestMethod]
		public void Simple() {
			Protocol protocol = Protocol.Default;
			Association association = HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.HMAC_SHA256, AssociationRelyingPartyType.Smart);
			var coordinator = new OpenIdCoordinator(
				rp => {
					rp.AssociationStore.StoreAssociation(ProviderUri, association);

					var request = new CheckIdRequest(protocol.Version, ProviderUri, false);
					request.AssociationHandle = association.Handle;
					request.ClaimedIdentifier = "http://claimedid";
					request.LocalIdentifier = "http://localid";
					request.ReturnTo = RPUri;
					rp.Channel.Send(request);
					var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
					Assert.IsNotNull(response);
				},
				op => {
					op.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					Assert.IsNotNull(request);
					var response = new PositiveAssertionResponse(request);
					op.Channel.Send(response);
				});
			coordinator.Run();
		}
	}
}
