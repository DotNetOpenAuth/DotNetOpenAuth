using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Consumer;
using System.Collections.Specialized;
using System.Web;
using System.Net;

namespace DotNetOpenId.Test.Consumer {
	[TestFixture]
	public class OpenIdConsumerTest {
		IConsumerApplicationStore store;
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
			new OpenIdConsumer();
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CtorWithNullQuery() {
			new OpenIdConsumer(null, store);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException), UserMessage = "Until this is a supported scenario, an exception should be thrown right away.")]
		public void CtorWithNullStore() {
			var consumer = new OpenIdConsumer(new NameValueCollection(), null);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext1() {
			var query = new NameValueCollection();
			var consumer = new OpenIdConsumer(query, store);
			consumer.CreateRequest(simpleOpenId);
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void CreateRequestWithoutContext2() {
			var query = new NameValueCollection();
			var consumer = new OpenIdConsumer(query, store);
			consumer.CreateRequest(simpleOpenId, simpleRealm);
		}
	}
}
