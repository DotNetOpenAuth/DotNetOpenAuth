//-----------------------------------------------------------------------
// <copyright file="UriOrOobEncoderTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using DotNetOpenAuth.OAuth.ChannelElements;

	[TestClass]
	public class UriOrOobEncodingTests : TestBase {
		private UriOrOobEncoding encoding;

		[TestInitialize]
		public void Setup() {
			this.encoding = new UriOrOobEncoding();
		}

		/// <summary>
		/// Verifies encoding null URIs results in the value "oob".
		/// </summary>
		[TestMethod]
		public void EncodeNullValueToOob() {
			Assert.AreEqual("oob", this.encoding.Encode(null));
		}

		/// <summary>
		/// Verifies decoding "oob" results in a null uri.
		/// </summary>
		[TestMethod]
		public void DecodeOobToNullUri() {
			Assert.IsNull(this.encoding.Decode("oob"));
		}

		/// <summary>
		/// Verifies that decoding an empty string generates an exception.
		/// </summary>
		[TestMethod, ExpectedException(typeof(UriFormatException))]
		public void DecodeEmptyStringFails() {
			this.encoding.Decode(string.Empty);
		}

		/// <summary>
		/// Verifies proper decoding/encoding of a Uri
		/// </summary>
		[TestMethod]
		public void UriEncodeDecode() {
			Uri original = new Uri("http://somehost/p?q=a#frag");
			string encodedValue = this.encoding.Encode(original);
			Assert.AreEqual(original.AbsoluteUri, encodedValue);
			Uri decoded = (Uri)this.encoding.Decode(encodedValue);
			Assert.AreEqual(original, decoded);
		}

		/// <summary>
		/// Verifies failure to decode a relative Uri
		/// </summary>
		[TestMethod, ExpectedException(typeof(UriFormatException))]
		public void RelativeUriDecodeFails() {
			this.encoding.Decode("../a/b");
		}
	}
}
