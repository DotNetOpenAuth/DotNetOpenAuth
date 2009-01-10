//-----------------------------------------------------------------------
// <copyright file="FailedAuthenticationResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class FailedAuthenticationResponseTests : OpenIdTestBase {
		private FailedAuthenticationResponse response;
		private ProtocolException exception;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.exception = new ProtocolException("Some failure");
			this.response = new FailedAuthenticationResponse(this.exception);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNull() {
			new FailedAuthenticationResponse(null);
		}

		[TestMethod]
		public void CommonProperties() {
			Assert.AreEqual(AuthenticationStatus.Failed, this.response.Status);
			Assert.AreSame(this.exception, this.response.Exception);
			Assert.IsNull(this.response.ClaimedIdentifier);
			Assert.IsNull(this.response.FriendlyIdentifierForDisplay);
		}

		[TestMethod]
		public void CommonMethods() {
			Assert.IsNull(this.response.GetExtension<ClaimsRequest>());
			Assert.IsNull(this.response.GetExtension(typeof(ClaimsRequest)));
			Assert.IsNull(this.response.GetCallbackArgument("somearg"));
			Assert.AreEqual(0, this.response.GetCallbackArguments().Count);
		}
	}
}
