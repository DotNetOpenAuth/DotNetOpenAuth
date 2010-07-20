//-----------------------------------------------------------------------
// <copyright file="HmacSha1SigningBindingElementTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.OAuth.Messages;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class HmacSha1SigningBindingElementTests : MessagingTestBase {
		[TestMethod]
		public void SignatureTest() {
			UnauthorizedTokenRequest message = SigningBindingElementBaseTests.CreateTestRequestTokenMessage(this.MessageDescriptions, null);

			HmacSha1SigningBindingElement_Accessor hmac = new HmacSha1SigningBindingElement_Accessor();
			hmac.Channel = new TestChannel(this.MessageDescriptions);
			Assert.AreEqual("kR0LhH8UqylaLfR/esXVVlP4sQI=", hmac.GetSignature(message));
		}
	}
}
