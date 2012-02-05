//-----------------------------------------------------------------------
// <copyright file="PositiveAuthenticationResponseSnapshotTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System.Collections.Generic;
	using System.IO;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Formatters.Binary;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Moq;
	using NUnit.Framework;

	[TestFixture]
	public class PositiveAuthenticationResponseSnapshotTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the PositiveAuthenticationResponseSnapshot is serializable,
		/// as required by the <see cref="OpenIdRelyingPartyAjaxControlBase"/> class.
		/// </summary>
		[Test]
		public void Serializable() {
			var response = new Mock<IAuthenticationResponse>(MockBehavior.Strict);
			response.Setup(o => o.ClaimedIdentifier).Returns(VanityUri);
			response.Setup(o => o.FriendlyIdentifierForDisplay).Returns(VanityUri.AbsoluteUri);
			response.Setup(o => o.Status).Returns(AuthenticationStatus.Authenticated);
			response.Setup(o => o.Provider).Returns(new ProviderEndpointDescription(OPUri, Protocol.Default.Version));
			response.Setup(o => o.GetUntrustedCallbackArguments()).Returns(new Dictionary<string, string>());
			response.Setup(o => o.GetCallbackArguments()).Returns(new Dictionary<string, string>());
			var snapshot = new PositiveAuthenticationResponseSnapshot(response.Object);
			MemoryStream ms = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(ms, snapshot);
		}
	}
}
