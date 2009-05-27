//-----------------------------------------------------------------------
// <copyright file="NonIdentityTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class NonIdentityTests : OpenIdTestBase {
		[TestMethod]
		public void ExtensionOnlyChannelLevel() {
			Protocol protocol = Protocol.V20;
			AuthenticationRequestMode mode = AuthenticationRequestMode.Setup;

			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = new SignedResponseRequest(protocol.Version, OPUri, mode);
					rp.Channel.Send(request);
				},
				op => {
					var request = op.Channel.ReadFromRequest<SignedResponseRequest>();
					Assert.IsNotInstanceOfType(request, typeof(CheckIdRequest));
				});
			coordinator.Run();
		}

		[TestMethod]
		public void ExtensionOnlyFacadeLevel() {
			Protocol protocol = Protocol.V20;
			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = rp.CreateRequest(GetMockIdentifier(protocol.ProtocolVersion), RPRealmUri, RPUri);

					request.IsExtensionOnly = true;
					rp.Channel.Send(request.RedirectingResponse.OriginalMessage);
					IAuthenticationResponse response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.ExtensionsOnly, response.Status);
				},
				op => {
					var assocRequest = op.GetRequest();
					op.SendResponse(assocRequest);

					var request = (IAnonymousRequest)op.GetRequest();
					request.IsApproved = true;
					Assert.IsNotInstanceOfType(request, typeof(CheckIdRequest));
					op.SendResponse(request);
				});
			coordinator.Run();
		}
	}
}
