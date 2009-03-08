//-----------------------------------------------------------------------
// <copyright file="IndirectSignedResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
	using DotNetOpenAuth.OpenId.ChannelElements;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class IndirectSignedResponseTests : OpenIdTestBase {
		private const string CreationDateString = "2005-05-15T17:11:51Z";
		private readonly DateTime creationDate = DateTime.Parse(CreationDateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
		private CheckIdRequest request;
		private IndirectSignedResponse response;
		private IndirectSignedResponse unsolicited;
		private Protocol protocol;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.protocol = Protocol.V20;
			this.request = new CheckIdRequest(this.protocol.Version, OPUri, AuthenticationRequestMode.Setup);
			this.request.ReturnTo = RPUri;
			this.response = new IndirectSignedResponse(this.request);

			this.unsolicited = new IndirectSignedResponse(this.protocol.Version, RPUri);
		}

		[TestMethod]
		public void CtorFromRequest() {
			Assert.AreEqual(this.protocol.Args.Mode.id_res, this.response.Mode);
			Assert.AreEqual(this.request.Version, this.response.Version);
			Assert.AreEqual(this.request.ReturnTo, this.response.Recipient);
			Assert.AreEqual(OPUri, this.response.ProviderEndpoint);
			Assert.IsTrue(DateTime.UtcNow - ((ITamperResistantOpenIdMessage)this.response).UtcCreationDate < TimeSpan.FromSeconds(5));
		}

		[TestMethod]
		public void CtorUnsolicited() {
			Assert.AreEqual(this.protocol.Args.Mode.id_res, this.unsolicited.Mode);
			Assert.AreEqual(this.protocol.Version, this.unsolicited.Version);
			Assert.AreEqual(RPUri, this.unsolicited.Recipient);
			Assert.IsTrue(DateTime.UtcNow - ((ITamperResistantOpenIdMessage)this.unsolicited).UtcCreationDate < TimeSpan.FromSeconds(5));

			Assert.IsNull(this.unsolicited.ProviderEndpoint);
			this.unsolicited.ProviderEndpoint = OPUri;
			Assert.AreEqual(OPUri, this.unsolicited.ProviderEndpoint);
		}

		[TestMethod]
		public void ResponseNonceSetter() {
			const string HybridValue = CreationDateString + "UNIQUE";
			var responseAccessor = IndirectSignedResponse_Accessor.AttachShadow(this.response);
			IReplayProtectedProtocolMessage responseReplay = this.response;
			responseAccessor.ResponseNonce = HybridValue;
			Assert.AreEqual(HybridValue, responseAccessor.ResponseNonce);
			Assert.AreEqual(this.creationDate, responseReplay.UtcCreationDate);
			Assert.AreEqual("UNIQUE", responseReplay.Nonce);

			responseAccessor.ResponseNonce = null;
			Assert.IsNull(responseReplay.Nonce);
		}

		[TestMethod]
		public void ResponseNonceGetter() {
			var responseAccessor = IndirectSignedResponse_Accessor.AttachShadow(this.response);
			IReplayProtectedProtocolMessage responseReplay = this.response;
			responseReplay.Nonce = "UnIqUe";
			responseReplay.UtcCreationDate = this.creationDate;

			Assert.AreEqual(CreationDateString + "UnIqUe", responseAccessor.ResponseNonce);
			Assert.AreEqual("UnIqUe", responseReplay.Nonce);
			Assert.AreEqual(this.creationDate, responseReplay.UtcCreationDate);
		}

		[TestMethod]
		public void UtcCreationDateConvertsToUniversal() {
			IReplayProtectedProtocolMessage responseReplay = this.response;
			DateTime local = DateTime.Parse("1982-01-01", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);
			if (local.Kind != DateTimeKind.Local) {
				Assert.Inconclusive("Test cannot manage to create a local time.");
			}

			responseReplay.UtcCreationDate = local;
			DateTime utcCreationDate = responseReplay.UtcCreationDate;
			Assert.AreEqual(DateTimeKind.Utc, utcCreationDate.Kind, "Local time should have been converted to universal time.");
			Assert.AreNotEqual(local.Hour, utcCreationDate.Hour, "The hour was expected to change (unless local time _is_ UTC time for this PC!)");

			// Now try setting UTC time just to make sure it DOESN'T mangle the hour
			if (this.creationDate.Kind != DateTimeKind.Utc) {
				Assert.Inconclusive("We need a UTC datetime to set with.");
			}
			responseReplay.UtcCreationDate = this.creationDate;
			utcCreationDate = responseReplay.UtcCreationDate;
			Assert.AreEqual(DateTimeKind.Utc, utcCreationDate.Kind);
			Assert.AreEqual(this.creationDate.Hour, utcCreationDate.Hour, "The hour should match since both times are UTC time.");
		}

		[TestMethod]
		public void ReturnToDoesNotMatchRecipient() {
			// Make sure its valid first, so we know that when it's invalid
			// it is due to our tampering.
			this.response.EnsureValidMessage();

			UriBuilder returnToBuilder = new UriBuilder(this.response.ReturnTo);
			var extraArgs = new Dictionary<string, string>();
			extraArgs["a"] = "b";
			MessagingUtilities.AppendQueryArgs(returnToBuilder, extraArgs);
			this.response.ReturnTo = returnToBuilder.Uri;
			try {
				this.response.EnsureValidMessage();
				Assert.Fail("Expected ProtocolException not thrown.");
			} catch (ProtocolException) {
			}
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void GetReturnToArgumentNullKey() {
			this.response.GetReturnToArgument(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void GetReturnToArgumentEmptyKey() {
			this.response.GetReturnToArgument(string.Empty);
		}

		[TestMethod]
		public void GetReturnToArgumentDoesNotReturnExtraArgs() {
			this.response.ExtraData["a"] = "b";
			Assert.IsNull(this.response.GetReturnToArgument("a"));
			Assert.AreEqual(0, this.response.GetReturnToParameterNames().Count());
		}

		[TestMethod]
		public void GetReturnToArgumentAndNames() {
			UriBuilder returnToBuilder = new UriBuilder(this.response.ReturnTo);
			returnToBuilder.AppendQueryArgs(new Dictionary<string, string> { { "a", "b" } });
			this.request.ReturnTo = returnToBuilder.Uri;

			// First pretend that the return_to args were signed.
			this.response = new IndirectSignedResponse(this.request);
			this.response.ReturnToParametersSignatureValidated = true;
			Assert.AreEqual(1, this.response.GetReturnToParameterNames().Count());
			Assert.IsTrue(this.response.GetReturnToParameterNames().Contains("a"));
			Assert.AreEqual("b", this.response.GetReturnToArgument("a"));

			// Now simulate them NOT being signed.  They should still be visible at this level.
			this.response = new IndirectSignedResponse(this.request);
			this.response.ReturnToParametersSignatureValidated = false;
			Assert.AreEqual(1, this.response.GetReturnToParameterNames().Count());
			Assert.IsTrue(this.response.GetReturnToParameterNames().Contains("a"));
			Assert.AreEqual("b", this.response.GetReturnToArgument("a"));
		}
	}
}
