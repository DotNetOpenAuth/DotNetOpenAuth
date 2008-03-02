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
		TrustRoot simpleTrustRoot = new TrustRoot("http://consumertest.openid.com");
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

		void parameterizedTest(Uri identityUrl, TrustRoot trustRoot, Uri returnTo, 
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {
			var consumer = new OpenIdConsumer(new NameValueCollection(), store);

			Assert.IsNull(consumer.Response);
			var request = consumer.CreateRequest(identityUrl, trustRoot, returnTo);

			// Test properties and defaults
			Assert.AreEqual(AuthenticationRequestMode.Setup, request.Mode);
			Assert.AreEqual(returnTo, request.ReturnToUrl);
			Assert.AreEqual(trustRoot.Url, request.TrustRootUrl.Url);

			request.Mode = requestMode;

			// Verify the redirect URL
			Assert.IsNotNull(request.RedirectToProviderUrl);
			var consumerToProviderQuery = HttpUtility.ParseQueryString(request.RedirectToProviderUrl.Query);
			Assert.IsTrue(consumerToProviderQuery[QueryStringArgs.openid.return_to].StartsWith(returnTo.AbsoluteUri, StringComparison.Ordinal));
			Assert.AreEqual(trustRoot.Url, consumerToProviderQuery[QueryStringArgs.openid.trust_root]);

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(request.RedirectToProviderUrl);
			providerRequest.AllowAutoRedirect = false;
			Uri redirectUrl;
			using (HttpWebResponse providerResponse = (HttpWebResponse)providerRequest.GetResponse()) {
				Assert.AreEqual(HttpStatusCode.Redirect, providerResponse.StatusCode);
				redirectUrl = new Uri(providerResponse.Headers[HttpResponseHeader.Location]);
			}
			var providerToConsumerQuery = HttpUtility.ParseQueryString(redirectUrl.Query);
			var consumer2 = new OpenIdConsumer(providerToConsumerQuery, store);
			Assert.AreEqual(expectedResult, consumer2.Response.Status);
			Assert.AreEqual(identityUrl, consumer2.Response.IdentityUrl);
		}

		[Test]
		public void SetupAuthentication() {
			parameterizedTest(
				TestSupport.IdentityUrl,
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Setup,
				AuthenticationStatus.Authenticated
			);
		}

		[Test]
		public void ImmediateAuthentication() {
			parameterizedTest(
				TestSupport.IdentityUrl,
				new TrustRoot(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri),
				TestSupport.GetFullUrl(TestSupport.ConsumerPage),
				AuthenticationRequestMode.Immediate,
				AuthenticationStatus.Authenticated
			);
		}
	}
}
