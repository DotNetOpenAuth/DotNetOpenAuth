using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenId.RelyingParty;
using NUnit.Framework;
using System.Net;
using DotNetOpenId.Extensions;
using System.IO;
using System.Diagnostics;

namespace DotNetOpenId.Test.Extensions {
	public class ExtensionTestBase {
		protected IRelyingPartyApplicationStore Store;
		protected const ProtocolVersion Version = ProtocolVersion.V20;

		[SetUp]
		public virtual void Setup() {
			Store = new ApplicationMemoryStore();
		}

		protected T ParameterizedTest<T>(Identifier identityUrl, IExtensionRequest extensionArgs)
			where T : IExtensionResponse, new() {
			Debug.Assert(identityUrl != null);
			var returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
			var consumer = new OpenIdRelyingParty(Store, null);
			var request = consumer.CreateRequest(identityUrl, realm, returnTo);
			if (extensionArgs != null)
				extensionArgs.AddToRequest(request);

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(request.RedirectToProviderUrl);
			providerRequest.AllowAutoRedirect = false;
			Uri redirectUrl;
			try {
				using (HttpWebResponse providerResponse = (HttpWebResponse)providerRequest.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.Redirect, providerResponse.StatusCode);
					redirectUrl = new Uri(providerResponse.Headers[HttpResponseHeader.Location]);
				}
			} catch (WebException ex) {
				Trace.WriteLine(ex);
				if (ex.Response != null) {
					using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
						Trace.WriteLine(sr.ReadToEnd());
					}
				}
				throw;
			}
			consumer = new OpenIdRelyingParty(Store, redirectUrl);
			Assert.AreEqual(AuthenticationStatus.Authenticated, consumer.Response.Status);
			Assert.AreEqual(identityUrl, consumer.Response.ClaimedIdentifier);
			T r = new T();
			return r.ReadFromResponse(consumer.Response) ? r : default(T);
		}
	}
}
