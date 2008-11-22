//-----------------------------------------------------------------------
// <copyright file="CheckIdRequestTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.Messages {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OpenId.Messages;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Messaging;

	[TestClass]
	public class CheckIdRequestTests : OpenIdTestBase {
		private Uri ProviderEndpoint;
		private CheckIdRequest immediatev1;
		private CheckIdRequest setupv1;
		private CheckIdRequest immediatev2;
		private CheckIdRequest setupv2;


		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			ProviderEndpoint = new Uri("http://host");

			immediatev1 = new CheckIdRequest(Protocol.V11.Version, ProviderEndpoint, true);
			setupv1 = new CheckIdRequest(Protocol.V11.Version, ProviderEndpoint, false);

			immediatev2 = new CheckIdRequest(Protocol.V20.Version, ProviderEndpoint, true);
			setupv2 = new CheckIdRequest(Protocol.V20.Version, ProviderEndpoint, false);

			// Prepare all message versions so that they SHOULD be valid by default.
			// In particular, V1 messages require ReturnTo.
			immediatev1.ReturnTo = new Uri("http://returnto/");
			setupv1.ReturnTo = new Uri("http://returnto/");

			try {
				immediatev1.EnsureValidMessage();
				setupv1.EnsureValidMessage();
				immediatev2.EnsureValidMessage();
				setupv2.EnsureValidMessage();
			} catch (ProtocolException ex) {
				Assert.Inconclusive("All messages ought to be valid before tests run, but got: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Tests that having <see cref="CheckIdRequest.ClaimedIdentifier"/> set without
		/// <see cref="CheckIdRequest.LocalIdentifier"/> set is recognized as an error in OpenID 2.x.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void ClaimedIdentifierWithoutIdentity() {
			setupv2.ClaimedIdentifier = "http://andrew.arnott.myopenid.com/";
			setupv2.EnsureValidMessage();
		}

		/// <summary>
		/// Tests that having <see cref="CheckIdRequest.LocalIdentifier"/> set without
		/// <see cref="CheckIdRequest.ClaimedIdentifier"/> set is recognized as an error in OpenID 2.x.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void LocalIdentifierWithoutClaimedIdentifier() {
			setupv2.LocalIdentifier = "http://andrew.arnott.myopenid.com/";
			setupv2.EnsureValidMessage();
		}

		/// <summary>
		/// Tests that having <see cref="CheckIdRequest.LocalIdentifier"/> set without 
		/// <see cref="CheckIdRequest.ClaimedIdentifier"/> set is recognized as valid in OpenID 1.x.
		/// </summary>
		[TestMethod]
		public void LocalIdentifierWithoutClaimedIdentifierV1() {
			setupv1.LocalIdentifier = "http://andrew.arnott.myopenid.com/";
			setupv1.EnsureValidMessage();
		}

		/// <summary>
		/// Verifies that the validation check throws if the return_to and the realm
		/// values are not compatible.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void RealmReturnToMismatchV2() {
			setupv2.Realm = "http://somehost/";
			setupv2.ReturnTo = new Uri("http://someotherhost/");
			setupv2.EnsureValidMessage();
		}

		/// <summary>
		/// Verifies that the validation check throws if the return_to and the realm
		/// values are not compatible.
		/// </summary>
		[TestMethod, ExpectedException(typeof(ProtocolException))]
		public void RealmReturnToMismatchV1() {
			setupv1.Realm = "http://somehost/";
			setupv1.ReturnTo = new Uri("http://someotherhost/");
			setupv1.EnsureValidMessage();
		}
	}
}
