using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Collections.Specialized;

namespace DotNetOpenId.Test {
	[TestFixture]
	public class UriUtilTest {
		[Test]
		public void CreateQueryString() {
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			Assert.AreEqual("a=b&c%2fd=e%2ff", UriUtil.CreateQueryString(args));
		}

		[Test]
		public void CreateQueryStringEmptyCollection() {
			Assert.AreEqual(0, UriUtil.CreateQueryString(new Dictionary<string, string>()).Length);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateQueryStringNullNvc() {
			UriUtil.CreateQueryString((NameValueCollection)null);
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CreateQueryStringNullDictionary() {
			UriUtil.CreateQueryString((IDictionary<string, string>)null);
		}

		[Test]
		public void AppendQueryArgs() {
			UriBuilder uri = new UriBuilder("http://baseline.org/page");
			var args = new Dictionary<string, string>();
			args.Add("a", "b");
			args.Add("c/d", "e/f");
			UriUtil.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff", uri.Uri.AbsoluteUri);
			args.Clear();
			args.Add("g", "h");
			UriUtil.AppendQueryArgs(uri, args);
			Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff&g=h", uri.Uri.AbsoluteUri);
		}
	}
}
