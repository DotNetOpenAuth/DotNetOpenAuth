//-----------------------------------------------------------------------
// <copyright file="HmacSha1SigningBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.ChannelElements {
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messages;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class HmacSha1SigningBindingElementTests : MessagingTestBase {
		[TestMethod]
		public void SignatureTest() {
			GetRequestTokenMessage message = SigningBindingElementBaseTests.CreateTestRequestTokenMessage();

			HmacSha1SigningBindingElement_Accessor hmac = new HmacSha1SigningBindingElement_Accessor();
			Assert.AreEqual("kR0LhH8UqylaLfR/esXVVlP4sQI=", hmac.GetSignature(message));
		}
	}
}
