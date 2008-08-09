using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using DotNetOpenId.Test.Hosting;
using System.Text.RegularExpressions;
using DotNetOpenId.Provider;
using System.Collections.Specialized;
using ProviderMemoryStore = DotNetOpenId.AssociationMemoryStore<DotNetOpenId.AssociationRelyingPartyType>;

namespace DotNetOpenId.Test.Provider {
	[TestFixture]
	public class OpenIdProviderTest {
		readonly Uri providerEndpoint = new Uri("http://someendpoint");
		readonly Uri emptyRequestUrl = new Uri("http://someendpoint/request");

		/// <summary>
		/// Verifies that without an ASP.NET context, the default constructor fails.
		/// </summary>
		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void CtorDefault() {
			OpenIdProvider op = new OpenIdProvider();
		}

		[Test]
		public void CtorNonDefault() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), 
				providerEndpoint, emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			OpenIdProvider op = new OpenIdProvider(null, providerEndpoint, 
				emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullEndpoint() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), 
				null, emptyRequestUrl, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullRequestUrl() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), 
				providerEndpoint, null, new NameValueCollection());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullQuery() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(), 
				providerEndpoint, emptyRequestUrl, null);
		}

		[Test]
		public void RequestNullOnEmptyRequest() {
			OpenIdProvider op = new OpenIdProvider(new ProviderMemoryStore(),
				providerEndpoint, emptyRequestUrl, new NameValueCollection());
			Assert.IsNull(op.Request);
		}

		//[Test, Ignore("Not implemented")]
		public void PrepareUnsolicitedAssertion() {
			// TODO: code here
		}
	}
}
