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
	using DotNetOpenAuth.OpenId.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ScenarioTests {
		[TestMethod]
		public void Associate() {
			// TODO: This is a VERY trivial association scenario that doesn't actually do anything significant.  It needs to get beefed up.
			OpenIdCoordinator coordinator = new OpenIdCoordinator(
				rp => {
					var associateRequest = new AssociateRequest(new Uri("http://host"));
					associateRequest.AssociationType = "HMAC-SHA1";
					associateRequest.SessionType = "DH-SHA1";
					IProtocolMessage responseMessage = rp.Channel.Request(associateRequest);
				},
				op => {
					var associateRequest = op.Channel.ReadFromRequest<AssociateRequest>();
					var response = new AssociateUnencryptedResponse();
					response.AssociationType = associateRequest.AssociationType;
					response.SessionType = associateRequest.SessionType;
					response.AssociationHandle = "{somehandle}";
					response.MacKey = new byte[] { 0x22, 0x33, 0x44 };
					op.Channel.Send(response);
				});
			coordinator.Run();
		}
	}
}
