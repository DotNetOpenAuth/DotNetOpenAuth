//-----------------------------------------------------------------------
// <copyright file="NonIdentityTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class NonIdentityTests : OpenIdTestBase {
		[Test]
		public void ExtensionOnlyChannelLevel() {
			Protocol protocol = Protocol.V20;
			AuthenticationRequestMode mode = AuthenticationRequestMode.Setup;

			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = new SignedResponseRequest(protocol.Version, OPUri, mode);
					rp.Channel.Respond(request);
				},
				op => {
					var request = op.Channel.ReadFromRequest<SignedResponseRequest>();
					Assert.IsNotInstanceOf<CheckIdRequest>(request);
				});
			coordinator.Run();
		}

		[Test]
		public void ExtensionOnlyFacadeLevel() {
			Protocol protocol = Protocol.V20;
			var coordinator = new OpenIdCoordinator(
				rp => {
					var request = rp.CreateRequest(GetMockIdentifier(protocol.ProtocolVersion), RPRealmUri, RPUri);

					request.IsExtensionOnly = true;
					rp.Channel.Respond(request.RedirectingResponse.OriginalMessage);
					IAuthenticationResponse response = rp.GetResponse();
					Assert.AreEqual(AuthenticationStatus.ExtensionsOnly, response.Status);
				},
				op => {
					var assocRequest = op.GetRequest();
					op.Respond(assocRequest);

					var request = (IAnonymousRequest)op.GetRequest();
					request.IsApproved = true;
					Assert.IsNotInstanceOf<CheckIdRequest>(request);
					op.Respond(request);
				});
			coordinator.Run();
		}
	}
}
