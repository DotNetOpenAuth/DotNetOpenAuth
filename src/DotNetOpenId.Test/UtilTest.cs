using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Web;
using System.Collections.Specialized;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UtilTest {
		[Test]
		public void NameValueCollectionToDictionary() {
			NameValueCollection nvc = HttpUtility.ParseQueryString("?a=b");
			IDictionary<string, string> dict = Util.NameValueCollectionToDictionary(nvc);
			Assert.AreEqual(1, dict.Count);
			Assert.IsTrue(dict["a"] == "b");
		}

		[Test]
		public void NameValueCollectionToDictionaryWithEmptyMemberTest() {
			// Google Code Issue 81.
			NameValueCollection nvc = HttpUtility.ParseQueryString("?&a=b");
			IDictionary<string, string> dict = Util.NameValueCollectionToDictionary(nvc);
			Assert.IsTrue(dict["a"] == "b");
		}

		[Test]
		public void NameValueCollectionToDictionaryNull() {
			Assert.IsNull(Util.NameValueCollectionToDictionary(null));
		}

		[Test]
		public void DictionaryToNameValueCollectionNull() {
			Assert.IsNull(Util.DictionaryToNameValueCollection(null));
		}
	}
}
