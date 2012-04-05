//-----------------------------------------------------------------------
// <copyright file="IdentifierTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class IdentifierTests {
		private string uri = "http://www.yahoo.com/";
		private string uriNoScheme = "www.yahoo.com";
		private string uriHttps = "https://www.yahoo.com/";
		private string xri = "=arnott*andrew";

		[Test]
		public void TryParseNoThrow() {
			Identifier id;
			Assert.IsFalse(Identifier.TryParse(null, out id));
			Assert.IsFalse(Identifier.TryParse(string.Empty, out id));
		}

		[Test]
		public void TryParse() {
			Identifier id;
			Assert.IsTrue(Identifier.TryParse("http://host/path", out id));
			Assert.AreEqual("http://host/path", id.ToString());
			Assert.IsTrue(Identifier.TryParse("=arnott", out id));
			Assert.AreEqual("=arnott", id.ToString());
		}

		[Test]
		public void Parse() {
			Assert.IsInstanceOf<UriIdentifier>(Identifier.Parse(this.uri));
			Assert.IsInstanceOf<XriIdentifier>(Identifier.Parse(this.xri));
		}

		/// <summary>
		/// Tests conformance with 2.0 spec section 7.2#2
		/// </summary>
		[Test]
		public void ParseEndUserSuppliedXriIdentifer() {
			List<char> symbols = new List<char>(XriIdentifier.GlobalContextSymbols);
			symbols.Add('(');
			List<string> prefixes = new List<string>();
			prefixes.AddRange(symbols.Select(s => s.ToString()));
			prefixes.AddRange(symbols.Select(s => "xri://" + s.ToString()));
			foreach (string prefix in prefixes) {
				var id = Identifier.Parse(prefix + "andrew");
				Assert.IsInstanceOf<XriIdentifier>(id);
			}
		}

		/// <summary>
		/// Verifies conformance with 2.0 spec section 7.2#3
		/// </summary>
		[Test]
		public void ParseEndUserSuppliedUriIdentifier() {
			// verify a fully-qualified Uri
			var id = Identifier.Parse(this.uri);
			Assert.IsInstanceOf<UriIdentifier>(id);
			Assert.AreEqual(this.uri, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify an HTTPS Uri
			id = Identifier.Parse(this.uriHttps);
			Assert.IsInstanceOf<UriIdentifier>(id);
			Assert.AreEqual(this.uriHttps, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify that if the scheme is missing it is added automatically
			id = Identifier.Parse(this.uriNoScheme);
			Assert.IsInstanceOf<UriIdentifier>(id);
			Assert.AreEqual(this.uri, ((UriIdentifier)id).Uri.AbsoluteUri);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ParseNull() {
			Identifier.Parse(null);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void ParseEmpty() {
			Identifier.Parse(string.Empty);
		}

		[Test]
		public void MessagePartConvertibility() {
			var message = new MessageWithIdentifier();
			var messageDescription = new MessageDescription(message.GetType(), new Version(1, 0));
			var messageDictionary = new MessageDictionary(message, messageDescription, false);
			messageDictionary["Identifier"] = OpenId.OpenIdTestBase.IdentifierSelect;
			Assert.That(messageDictionary["Identifier"], Is.EqualTo(OpenId.OpenIdTestBase.IdentifierSelect));
		}

		private class MessageWithIdentifier : TestMessage {
			[MessagePart]
			internal Identifier Identifier { get; set; }
		}
	}
}
