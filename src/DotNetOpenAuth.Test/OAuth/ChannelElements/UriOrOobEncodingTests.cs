//-----------------------------------------------------------------------
// <copyright file="UriOrOobEncodingTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using NUnit.Framework;

	[TestFixture]
	public class UriOrOobEncodingTests : TestBase {
		private UriOrOobEncoding encoding;

		[SetUp]
		public void Setup() {
			this.encoding = new UriOrOobEncoding();
		}

		/// <summary>
		/// Verifies null value encoding
		/// </summary>
		[Test]
		public void NullValueEncoding() {
			Assert.AreEqual("oob", this.encoding.EncodedNullValue);
		}

		/// <summary>
		/// Verifies decoding "oob" results in a null uri.
		/// </summary>
		[Test]
		public void DecodeOobToNullUri() {
			Assert.IsNull(this.encoding.Decode("oob"));
		}

		/// <summary>
		/// Verifies that decoding an empty string generates an exception.
		/// </summary>
		[Test, ExpectedException(typeof(UriFormatException))]
		public void DecodeEmptyStringFails() {
			this.encoding.Decode(string.Empty);
		}

		/// <summary>
		/// Verifies proper decoding/encoding of a Uri
		/// </summary>
		[Test]
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
		[Test, ExpectedException(typeof(UriFormatException))]
		public void RelativeUriDecodeFails() {
			this.encoding.Decode("../a/b");
		}
	}
}
