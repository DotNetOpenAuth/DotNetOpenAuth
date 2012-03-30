//-----------------------------------------------------------------------
// <copyright file="PositiveAssertionResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class PositiveAssertionResponseTests : OpenIdTestBase {
		private const string CreationDateString = "2005-05-15T17:11:51Z";
		private readonly DateTime creationDate = DateTime.Parse(CreationDateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
		private CheckIdRequest request;
		private PositiveAssertionResponse response;
		private PositiveAssertionResponse unsolicited;
		private Protocol protocol;

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.V20;
			this.request = new CheckIdRequest(this.protocol.Version, OPUri, AuthenticationRequestMode.Setup);
			this.request.ReturnTo = RPUri;
			this.response = new PositiveAssertionResponse(this.request);

			this.unsolicited = new PositiveAssertionResponse(this.protocol.Version, RPUri);
		}

		[Test]
		public void CtorFromRequest() {
			Assert.AreEqual(this.protocol.Args.Mode.id_res, this.response.Mode);
			Assert.AreEqual(this.request.Version, this.response.Version);
			Assert.AreEqual(this.request.ReturnTo, this.response.Recipient);
			Assert.AreEqual(OPUri, this.response.ProviderEndpoint);
		}

		[Test]
		public void CtorUnsolicited() {
			Assert.AreEqual(this.protocol.Args.Mode.id_res, this.unsolicited.Mode);
			Assert.AreEqual(this.protocol.Version, this.unsolicited.Version);
			Assert.AreEqual(RPUri, this.unsolicited.Recipient);

			Assert.IsNull(this.unsolicited.ProviderEndpoint);
			this.unsolicited.ProviderEndpoint = OPUri;
			Assert.AreEqual(OPUri, this.unsolicited.ProviderEndpoint);
		}

		/// <summary>
		/// Verifies that local_id and claimed_id can either be null or specified.
		/// </summary>
		[Test]
		public void ClaimedIdAndLocalIdSpecifiedIsValid() {
			this.response.LocalIdentifier = "http://local";
			this.response.ClaimedIdentifier = "http://claimedid";
			this.response.EnsureValidMessage();

			this.response.LocalIdentifier = null;
			this.response.ClaimedIdentifier = null;
			this.response.EnsureValidMessage();
		}
	}
}
