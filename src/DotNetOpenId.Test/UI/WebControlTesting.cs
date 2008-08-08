using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DotNetOpenId.RelyingParty;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DotNetOpenId.Test.UI {
	[TestFixture]
	public class WebControlTesting {
		void parameterizedWebClientTest(Identifier identityUrl,
			AuthenticationRequestMode requestMode, AuthenticationStatus expectedResult) {

			Uri redirectToProviderUrl;
			HttpWebRequest rpRequest = (HttpWebRequest)WebRequest.Create(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			NameValueCollection query = new NameValueCollection();
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
					Regex regex = new Regex(@"\<input\b.*\bname=""(\w+)"".*\bvalue=""([^""]+)""", RegexOptions.IgnoreCase);
					while (!sr.EndOfStream) {
						string line = sr.ReadLine();
						Match m = regex.Match(line);
						if (m.Success) {
							query[m.Groups[1].Value] = m.Groups[2].Value;
						}
					}
				}
			}
			query["OpenIdTextBox1$wrappedTextBox"] = identityUrl;
			rpRequest = (HttpWebRequest)WebRequest.Create(TestSupport.GetFullUrl(TestSupport.ConsumerPage));
			rpRequest.Method = "POST";
			rpRequest.AllowAutoRedirect = false;
			string queryString = UriUtil.CreateQueryString(query);
			rpRequest.ContentLength = queryString.Length;
			rpRequest.ContentType = "application/x-www-form-urlencoded";
			using (StreamWriter sw = new StreamWriter(rpRequest.GetRequestStream())) {
				sw.Write(queryString);
			}
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
					string doc = sr.ReadToEnd();
					Debug.WriteLine(doc);
				}
				redirectToProviderUrl = new Uri(response.Headers[HttpResponseHeader.Location]);
			}

			HttpWebRequest providerRequest = (HttpWebRequest)WebRequest.Create(redirectToProviderUrl);
			providerRequest.AllowAutoRedirect = false;
			Uri redirectUrl;
			try {
				using (HttpWebResponse providerResponse = (HttpWebResponse)providerRequest.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.Redirect, providerResponse.StatusCode);
					redirectUrl = new Uri(providerResponse.Headers[HttpResponseHeader.Location]);
				}
			} catch (WebException ex) {
				TestSupport.Logger.Error("WebException", ex);
				if (ex.Response != null) {
					using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream())) {
						TestSupport.Logger.ErrorFormat("Response stream follows: {0}", sr.ReadToEnd());
					}
				}
				throw;
			}
			rpRequest = (HttpWebRequest)WebRequest.Create(redirectUrl);
			rpRequest.AllowAutoRedirect = false;
			using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
				Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode); // redirect on login
			}

			// Try replay attack
			if (expectedResult == AuthenticationStatus.Authenticated) {
				// This simulates a network sniffing user who caught the 
				// authenticating query en route to either the user agent or
				// the consumer, and tries the same query to the consumer in an
				// attempt to spoof the identity of the authenticating user.
				rpRequest = (HttpWebRequest)WebRequest.Create(redirectUrl);
				rpRequest.AllowAutoRedirect = false;
				using (HttpWebResponse response = (HttpWebResponse)rpRequest.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode); // error message
				}
			}
		}

		[Test]
		public void Pass_Setup_AutoApproval_20() {
			Identifier userSuppliedIdentifier = TestSupport.GetMockIdentifier(TestSupport.Scenarios.AutoApproval, ProtocolVersion.V20);
			Identifier claimedId = userSuppliedIdentifier;
			parameterizedWebClientTest(userSuppliedIdentifier, AuthenticationRequestMode.Setup, AuthenticationStatus.Authenticated);
		}

		[Test]
		public void Fail_Immediate_ApproveOnSetup_20() {
			Identifier userSuppliedIdentifier = TestSupport.GetMockIdentifier(TestSupport.Scenarios.ApproveOnSetup, ProtocolVersion.V20);
			Identifier claimedId = userSuppliedIdentifier;
			parameterizedWebClientTest(userSuppliedIdentifier, AuthenticationRequestMode.Immediate, AuthenticationStatus.Authenticated);
		}
	}
}
