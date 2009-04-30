//-----------------------------------------------------------------------
// <copyright file="IdentifierTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DotNetOpenAuth.OpenId;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class IdentifierTests {
		private string uri = "http://www.yahoo.com/";
		private string uriNoScheme = "www.yahoo.com";
		private string uriHttps = "https://www.yahoo.com/";
		private string xri = "=arnott*andrew";

		[TestMethod]
		public void TryParseNoThrow() {
			Identifier id;
			Assert.IsFalse(Identifier.TryParse(null, out id));
			Assert.IsFalse(Identifier.TryParse(string.Empty, out id));
		}

		[TestMethod]
		public void TryParse() {
			Identifier id;
			Assert.IsTrue(Identifier.TryParse("http://host/path", out id));
			Assert.AreEqual("http://host/path", id.ToString());
			Assert.IsTrue(Identifier.TryParse("=arnott", out id));
			Assert.AreEqual("=arnott", id.ToString());
		}

		[TestMethod]
		public void Parse() {
			Assert.IsInstanceOfType(Identifier.Parse(this.uri), typeof(UriIdentifier));
			Assert.IsInstanceOfType(Identifier.Parse(this.xri), typeof(XriIdentifier));
		}

		/// <summary>
		/// Tests conformance with 2.0 spec section 7.2#2
		/// </summary>
		[TestMethod]
		public void ParseEndUserSuppliedXriIdentifer() {
			List<char> symbols = new List<char>(XriIdentifier.GlobalContextSymbols);
			symbols.Add('(');
			List<string> prefixes = new List<string>();
			prefixes.AddRange(symbols.Select(s => s.ToString()));
			prefixes.AddRange(symbols.Select(s => "xri://" + s.ToString()));
			foreach (string prefix in prefixes) {
				var id = Identifier.Parse(prefix + "andrew");
				Assert.IsInstanceOfType(id, typeof(XriIdentifier));
			}
		}

		/// <summary>
		/// Verifies conformance with 2.0 spec section 7.2#3
		/// </summary>
		[TestMethod]
		public void ParseEndUserSuppliedUriIdentifier() {
			// verify a fully-qualified Uri
			var id = Identifier.Parse(this.uri);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(this.uri, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify an HTTPS Uri
			id = Identifier.Parse(this.uriHttps);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(this.uriHttps, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify that if the scheme is missing it is added automatically
			id = Identifier.Parse(this.uriNoScheme);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(this.uri, ((UriIdentifier)id).Uri.AbsoluteUri);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ParseNull() {
			Identifier.Parse(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void ParseEmpty() {
			Identifier.Parse(string.Empty);
		}
	}
}
