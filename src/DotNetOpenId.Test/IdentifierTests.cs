using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class IdentifierTests {
		string uri = "http://www.yahoo.com/";
		string uriNoScheme = "www.yahoo.com";
		string uriHttps = "https://www.yahoo.com/";
		string xri = "=arnott*andrew";

		[Test]
		public void Parse() {
			Assert.IsInstanceOfType(typeof(UriIdentifier), Identifier.Parse(uri));
			Assert.IsInstanceOfType(typeof(XriIdentifier), Identifier.Parse(xri));
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
				Assert.IsInstanceOfType(typeof(XriIdentifier), id);
			}
		}

		/// <summary>
		/// Verifies conformance with 2.0 spec section 7.2#3
		/// </summary>
		[Test]
		public void ParseEndUserSuppliedUriIdentifier() {
			// verify a fully-qualified Uri
			var id = Identifier.Parse(uri);
			Assert.IsInstanceOfType(typeof(UriIdentifier), id);
			Assert.AreEqual(uri, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify an HTTPS Uri
			id = Identifier.Parse(uriHttps);
			Assert.IsInstanceOfType(typeof(UriIdentifier), id);
			Assert.AreEqual(uriHttps, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify that if the scheme is missing it is added automatically
			id = Identifier.Parse(uriNoScheme);
			Assert.IsInstanceOfType(typeof(UriIdentifier), id);
			Assert.AreEqual(uri, ((UriIdentifier)id).Uri.AbsoluteUri);
			// verify that fragments are stripped
			id = Identifier.Parse(uri + "#fragment");
			Assert.AreEqual(uri, ((UriIdentifier)id).Uri.AbsoluteUri);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ParseNull() {
			Identifier.Parse(null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void ParseEmpty() {
			Identifier.Parse(string.Empty);
		}
	}
}
