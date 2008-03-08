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
	public class OpenIdConsumerTest {
		IRelyingPartyApplicationStore store;
		Uri simpleOpenId = new Uri("http://nonexistant.openid.com");
		Realm simpleRealm = new Realm("http://consumertest.openid.com");
		Uri simpleReturnTo = new Uri("http://consumertest.openid.com/consumertests");

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
		[ExpectedException(typeof(ArgumentNullException))]
		public void CtorWithNullQuery() {
			new OpenIdRelyingParty(null, store);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException), UserMessage = "Until this is a supported scenario, an exception should be thrown right away.")]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(new NameValueCollection(), null);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var query = new NameValueCollection();
			var consumer = new OpenIdRelyingParty(query, store);
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var query = new NameValueCollection();
			var consumer = new OpenIdRelyingParty(query, store);
			consumer.CreateRequest(simpleOpenId, simpleRealm);
		}
	}
}
