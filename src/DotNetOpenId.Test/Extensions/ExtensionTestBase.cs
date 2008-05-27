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
using System.Web;

namespace DotNetOpenId.Test.Extensions {
	public class ExtensionTestBase {
		protected IRelyingPartyApplicationStore AppStore;
		protected const ProtocolVersion Version = ProtocolVersion.V20;

		[SetUp]
		public virtual void Setup() {
			AppStore = new ApplicationMemoryStore();
		}

		protected T ParameterizedTest<T>(Identifier identityUrl, IExtensionRequest extension)
			where T : IExtensionResponse, new() {
			Debug.Assert(identityUrl != null);
			var returnTo = TestSupport.GetFullUrl(TestSupport.ConsumerPage);
			var realm = new Realm(TestSupport.GetFullUrl(TestSupport.ConsumerPage).AbsoluteUri);
			var consumer = new OpenIdRelyingParty(AppStore, null, null);
			var request = consumer.CreateRequest(identityUrl, realm, returnTo);
			if (extension != null)
				request.AddExtension(extension);

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(request.RedirectingResponse.ExtractUrl());
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
			consumer = new OpenIdRelyingParty(AppStore, redirectUrl, HttpUtility.ParseQueryString(redirectUrl.Query));
			Assert.AreEqual(AuthenticationStatus.Authenticated, consumer.Response.Status);
			Assert.AreEqual(identityUrl, consumer.Response.ClaimedIdentifier);
			return consumer.Response.GetExtension<T>();
		}
	}
}
