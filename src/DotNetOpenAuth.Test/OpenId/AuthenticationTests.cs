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
		public void SharedAssociationPositive() {
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
					Assert.AreEqual(request.ClaimedIdentifier, response.ClaimedIdentifier);
					Assert.AreEqual(request.LocalIdentifier, response.LocalIdentifier);
					Assert.AreEqual(request.ReturnTo, response.ReturnTo);
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

		[TestMethod]
		public void SharedAssociationNegative() {
			Protocol protocol = Protocol.V11;
			Uri userSetupUrl = new Uri("http://usersetupurl");
			Association association = HmacShaAssociation.Create(protocol, protocol.Args.SignatureAlgorithm.HMAC_SHA1, AssociationRelyingPartyType.Smart);
			var coordinator = new OpenIdCoordinator(
				rp => {
					rp.AssociationStore.StoreAssociation(ProviderUri, association);

					var request = new CheckIdRequest(protocol.Version, ProviderUri, true);
					request.AssociationHandle = association.Handle;
					request.ClaimedIdentifier = "http://claimedid";
					request.LocalIdentifier = "http://localid";
					request.ReturnTo = RPUri;
					rp.Channel.Send(request);
					var response = rp.Channel.ReadFromRequest<NegativeAssertionResponse>();
					Assert.IsNotNull(response);
					Assert.AreEqual(userSetupUrl, response.UserSetupUrl);
				},
				op => {
					op.AssociationStore.StoreAssociation(AssociationRelyingPartyType.Smart, association);
					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					Assert.IsNotNull(request);
					var response = new NegativeAssertionResponse(request);
					response.UserSetupUrl = userSetupUrl;
					op.Channel.Send(response);
				});
			coordinator.Run();
		}

		[TestMethod]
		public void PrivateAssociationPositive() {
			Protocol protocol = Protocol.Default;
			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = new CheckIdRequest(protocol.Version, ProviderUri, false);
					request.ClaimedIdentifier = "http://claimedid";
					request.LocalIdentifier = "http://localid";
					request.ReturnTo = RPUri;
					rp.Channel.Send(request);
					var response = rp.Channel.ReadFromRequest<PositiveAssertionResponse>();
					Assert.IsNotNull(response);
					Assert.AreEqual(request.ClaimedIdentifier, response.ClaimedIdentifier);
					Assert.AreEqual(request.LocalIdentifier, response.LocalIdentifier);
					Assert.AreEqual(request.ReturnTo, response.ReturnTo);
				},
				op => {
					var request = op.Channel.ReadFromRequest<CheckIdRequest>();
					Assert.IsNotNull(request);
					var response = new PositiveAssertionResponse(request);
					op.Channel.Send(response);
					var checkauth = op.Channel.ReadFromRequest<CheckAuthenticationRequest>();
					var checkauthResponse = new CheckAuthenticationResponse(checkauth);
					checkauthResponse.IsValid = true; // TODO: how do we establish that the signature is good?
					op.Channel.Send(checkauthResponse);
				});
			coordinator.Run();
		}
	}
}
