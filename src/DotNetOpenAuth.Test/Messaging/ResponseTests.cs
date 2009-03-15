//-----------------------------------------------------------------------
// <copyright file="ResponseTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.IO;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ResponseTests : TestBase {
		[TestMethod, ExpectedException(typeof(InvalidOperationException))]
		public void SendWithoutAspNetContext() {
			new OutgoingWebResponse().Send();
		}

		[TestMethod]
		public void Send() {
			StringWriter writer = new StringWriter();
			HttpRequest httpRequest = new HttpRequest("file", "http://server", string.Empty);
			HttpResponse httpResponse = new HttpResponse(writer);
			HttpContext context = new HttpContext(httpRequest, httpResponse);
			HttpContext.Current = context;

			OutgoingWebResponse response = new OutgoingWebResponse();
			response.Status = System.Net.HttpStatusCode.OK;
			response.Headers["someHeaderName"] = "someHeaderValue";
			response.Body = "some body";
			response.Send();
			string results = writer.ToString();
			// For some reason the only output in test is the body... the headers require a web host
			Assert.AreEqual(response.Body, results);
		}
	}
}
