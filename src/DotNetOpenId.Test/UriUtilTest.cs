using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Collections.Specialized;

namespace DotNetOpenId.Test {
    [TestFixture]
    public class UriUtilTest {
        [Test]
        public void NormalizeUri() {
            Assert.AreEqual("http://www.yahoo.com/", UriUtil.NormalizeUri("www.YAHOO.com").AbsoluteUri);
            Assert.AreEqual("http://www.yahoo.com/boo", UriUtil.NormalizeUri("www.YAHOO.com/boo").AbsoluteUri);
            Assert.AreEqual("http://www.yahoo.com/", UriUtil.NormalizeUri("http://www.YAHOO.com").AbsoluteUri);
            Assert.AreEqual("https://www.yahoo.com/", UriUtil.NormalizeUri("https://www.YAHOO.com").AbsoluteUri);
            Assert.AreEqual("xri://www.yahoo.com/", UriUtil.NormalizeUri("xri://www.YAHOO.com").AbsoluteUri);
        }

        [Test]
        public void CreateQueryString() {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("a", "b");
            nvc.Add("c/d", "e/f");
            Assert.AreEqual("a=b&c%2fd=e%2ff", UriUtil.CreateQueryString(nvc));
        }

        [Test]
        public void AppendQueryArgs() {
            UriBuilder uri = new UriBuilder("http://baseline.org/page");
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("a", "b");
            nvc.Add("c/d", "e/f");
            UriUtil.AppendQueryArgs(uri, nvc);
            Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff", uri.Uri.AbsoluteUri);
            nvc.Clear();
            nvc.Add("g", "h");
            UriUtil.AppendQueryArgs(uri, nvc);
            Assert.AreEqual("http://baseline.org/page?a=b&c%2fd=e%2ff&g=h", uri.Uri.AbsoluteUri);
        }
    }
}
