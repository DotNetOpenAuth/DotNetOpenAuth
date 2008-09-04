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

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AppendQueryArgsNullUriBuilder() {
			UriUtil.AppendQueryArgs(null, new Dictionary<string, string>());
		}

		[Test]
		public void AppendQueryArgsNullDictionary() {
			UriUtil.AppendQueryArgs(new UriBuilder(), null);
		}

		[Test]
		public void UriBuilderToStringWithImpliedPorts() {
			Assert.AreEqual("http://localhost/p?q#f", 
				UriUtil.UriBuilderToStringWithImpliedPorts(new UriBuilder("http://localhost:80/p?q#f")));
			Assert.AreEqual("https://localhost/p?q#f",
				UriUtil.UriBuilderToStringWithImpliedPorts(new UriBuilder("https://localhost:443/p?q#f")));
			// Switch it up to make sure that ports are explicitly given where required.
			Assert.AreEqual("http://localhost:443/p?q#f",
				UriUtil.UriBuilderToStringWithImpliedPorts(new UriBuilder("http://localhost:443/p?q#f")));
			Assert.AreEqual("https://localhost:80/p?q#f",
				UriUtil.UriBuilderToStringWithImpliedPorts(new UriBuilder("https://localhost:80/p?q#f")));
			// and some random ports
			Assert.AreEqual("https://localhost:5000/p?q#f",
				UriUtil.UriBuilderToStringWithImpliedPorts(new UriBuilder("https://localhost:5000/p?q#f")));
		}
	}
}
