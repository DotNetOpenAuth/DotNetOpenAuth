//-----------------------------------------------------------------------
// <copyright file="OpenIdTextBoxTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId.RelyingParty {
	using DotNetOpenAuth.OpenId.RelyingParty;
	using NUnit.Framework;

	[TestFixture]
	public class OpenIdTextBoxTests : OpenIdTestBase {
		/// <summary>
		/// Verifies that the Text and Identifier properties interact correctly.
		/// </summary>
		[Test]
		public void IdentifierTextInteraction() {
			var box = new OpenIdTextBox();
			Assert.AreEqual(string.Empty, box.Text);
			Assert.IsNull(box.Identifier);

			box.Text = "=arnott";
			Assert.AreEqual("=arnott", box.Text);
			Assert.AreEqual("=arnott", box.Identifier.ToString());

			box.Identifier = "=bob";
			Assert.AreEqual("=bob", box.Text);
			Assert.AreEqual("=bob", box.Identifier.ToString());

			box.Text = string.Empty;
			Assert.AreEqual(string.Empty, box.Text);
			Assert.IsNull(box.Identifier);

			box.Text = null;
			Assert.AreEqual(string.Empty, box.Text);
			Assert.IsNull(box.Identifier);

			// Invalid identifier case
			box.Text = "/";
			Assert.AreEqual("/", box.Text);
			Assert.IsNull(box.Identifier);

			// blank out the invalid case
			box.Identifier = null;
			Assert.AreEqual(string.Empty, box.Text);
			Assert.IsNull(box.Identifier);
		}
	}
}
