//-----------------------------------------------------------------------
// <copyright file="HostingTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Hosting {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using System.Net;
	using System.IO;
	using DotNetOpenAuth.Test.OpenId;

	[TestClass]
	public class HostingTests {
		[TestMethod]
		public void AspHostBasicTest() {
			using (AspNetHost host = AspNetHost.CreateHost(TestSupport.TestWebDirectory)) {
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(host.BaseUri);
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
					using (StreamReader sr = new StreamReader(response.GetResponseStream())) {
						string content = sr.ReadToEnd();
						StringAssert.Contains(content, "Test home page");
					}
				}
			}
		}
	}
}
