//-----------------------------------------------------------------------
// <copyright file="ScenarioTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ScenarioTests {
		private readonly Protocol Protocol = Protocol.V20;

		[TestMethod]
		public void AssociateDiffieHellmanMessages() {
			Association rpAssociation = null, opAssociation = null;
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var associateRequest = new AssociateDiffieHellmanRequest(new Uri("http://host"));
					associateRequest.AssociationType = Protocol.Args.SignatureAlgorithm.HMAC_SHA1;
					associateRequest.SessionType = Protocol.Args.SessionType.DH_SHA1;
					associateRequest.InitializeRequest();
					var associateResponse = rp.Channel.Request<AssociateDiffieHellmanResponse>(associateRequest);
					rpAssociation = associateResponse.CreateAssociation(associateRequest);
					Assert.IsNotNull(rpAssociation);
					Assert.IsFalse(MessagingUtilities.AreEquivalent(associateResponse.EncodedMacKey, rpAssociation.SecretKey), "Key should have been encrypted.");
				},
				op => {
					var associateRequest = op.Channel.ReadFromRequest<AssociateDiffieHellmanRequest>();
					var response = new AssociateDiffieHellmanResponse();
					response.AssociationType = associateRequest.AssociationType;
					opAssociation = response.CreateAssociation(associateRequest);
					op.Channel.Send(response);
				});
			coordinator.Run();
			Assert.AreEqual(opAssociation.Handle, rpAssociation.Handle);
			Assert.IsTrue(Math.Abs(opAssociation.SecondsTillExpiration - rpAssociation.SecondsTillExpiration) < 60);
			Assert.IsTrue(MessagingUtilities.AreEquivalent(opAssociation.SecretKey, rpAssociation.SecretKey));
		}
	}
}
