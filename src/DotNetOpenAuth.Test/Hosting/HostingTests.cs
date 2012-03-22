//-----------------------------------------------------------------------
// <copyright file="HostingTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Text;
	using DotNetOpenAuth.Test.OpenId;
	using NUnit.Framework;

	[TestFixture, Category("HostASPNET")]
	public class HostingTests : TestBase {
		[Test]
		public void AspHostBasicTest() {
			try {
				using (AspNetHost host = AspNetHost.CreateHost(TestWebDirectory)) {
					HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host.BaseUri);
					using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
						Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
						using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
							string content = sr.ReadToEnd();
							StringAssert.Contains("Test home page", content);
						}
					}
				}
			} catch (FileNotFoundException ex) {
				Assert.Inconclusive(
					"Unable to execute hosted ASP.NET tests because {0} could not be found.  {1}", ex.FileName, ex.FusionLog);
			} catch (WebException ex) {
				if (ex.Response != null) {
					using (var responseStream = new StreamReader(ex.Response.GetResponseStream())) {
						Console.WriteLine(responseStream.ReadToEnd());
					}
				}

				throw;
			}
		}
	}
}
