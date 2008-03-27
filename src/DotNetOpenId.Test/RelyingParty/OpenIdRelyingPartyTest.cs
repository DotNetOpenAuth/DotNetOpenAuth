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
		public void CtorWithNullRequestUrl() {
			new OpenIdRelyingParty(store, null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException), UserMessage = "Until this is a supported scenario, an exception should be thrown right away.")]
		public void CtorWithNullStore() {
			var consumer = new OpenIdRelyingParty(null, new Uri("http://simple/req"));
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var consumer = new OpenIdRelyingParty(store, new Uri("http://simple/req"));
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var consumer = new OpenIdRelyingParty(store, new Uri("http://simple/req"));
			consumer.CreateRequest(simpleOpenId, simpleRealm);
		}
	}
}
