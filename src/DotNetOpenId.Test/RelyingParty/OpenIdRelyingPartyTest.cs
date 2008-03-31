using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace DotNetOpenId.Test.RelyingParty {
	[TestFixture]
	public class OpenIdRelyingPartyTest {
		IRelyingPartyApplicationStore store;
		UriIdentifier simpleOpenId = new UriIdentifier("http://nonexistant.openid.com");
		Realm simpleRealm = new Realm("http://consumertest.openid.com");
		Uri simpleReturnTo = new Uri("http://consumertest.openid.com/consumertests");
		Uri simpleNonOpenIdRequest = new Uri("http://localhost/hi");

		[SetUp]
		public void Setup() {
			store = new ConsumerApplicationMemoryStore();
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void DefaultCtorWithoutContext() {
			new OpenIdRelyingParty();
		}

		[Test]
		public void CtorWithNullRequestUri() {
			new OpenIdRelyingParty(store, null);
		}

		[Test]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(null, simpleNonOpenIdRequest);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest);
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var consumer = new OpenIdRelyingParty(store, simpleNonOpenIdRequest);
			consumer.CreateRequest(simpleOpenId, simpleRealm);
		}
	}
}
