using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.Consumer;
using System.Collections.Specialized;

namespace DotNetOpenId.Test.Consumer {
	[TestFixture]
	public class OpenIdConsumerTest {
		IConsumerApplicationStore store;
		Uri simpleOpenId = new Uri("http://nonexistant.openid.com");
		TrustRoot simpleTrustRoot = new TrustRoot("http://consumertest.openid.com");
		Uri simpleReturnTo = new Uri("http://consumertest.openid.com/consumertests");

		[SetUp]
		public void Setup() {
			store = new DotNetOpenId.Consumer.ConsumerApplicationMemoryStore();
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
			consumer.CreateRequest(simpleOpenId, simpleTrustRoot);
		}

		[Test]
		[Ignore("Not finished")]
		public void BasicSimulation() {
			var query = new NameValueCollection();
			var consumer = new OpenIdConsumer(query, store);
			
			Assert.IsNull(consumer.Response);
			var request = consumer.CreateRequest(simpleOpenId, simpleTrustRoot, simpleReturnTo);

			// Test defaults
			Assert.AreEqual(AuthenticationRequestMode.Setup, request.Mode);
			Assert.AreEqual(simpleReturnTo, request.ReturnToUrl);
			Assert.AreEqual(simpleTrustRoot, request.TrustRootUrl);
			Assert.IsNotNull(request.RedirectToProviderUrl);
		}
	}
}
