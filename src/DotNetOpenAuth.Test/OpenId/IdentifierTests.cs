using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotNetOpenAuth.OpenId;

namespace DotNetOpenAuth.Test.OpenId {
	[TestClass]
	public class IdentifierTests {
		string uri = "http://www.yahoo.com/";
		string uriNoScheme = "www.yahoo.com";
		string uriHttps = "https://www.yahoo.com/";
		string xri = "=arnott*andrew";

		[TestMethod]
		public void Parse() {
			Assert.IsInstanceOfType(Identifier.Parse(uri), typeof(UriIdentifier));
			Assert.IsInstanceOfType(Identifier.Parse(xri), typeof(XriIdentifier));
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
			var id = Identifier.Parse(uri);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(uri, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify an HTTPS Uri
			id = Identifier.Parse(uriHttps);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(uriHttps, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify that if the scheme is missing it is added automatically
			id = Identifier.Parse(uriNoScheme);
			Assert.IsInstanceOfType(id, typeof(UriIdentifier));
			Assert.AreEqual(uri, ((UriIdentifier)id).Uri.AbsoluteUri);
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
