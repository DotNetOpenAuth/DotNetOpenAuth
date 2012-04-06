//-----------------------------------------------------------------------
// <copyright file="ResponseTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.IO;
	using System.Web;
	using DotNetOpenAuth.Messaging;
	using NUnit.Framework;

	[TestFixture]
	public class ResponseTests : TestBase {
		[Test, ExpectedException(typeof(InvalidOperationException))]
		public void RespondWithoutAspNetContext() {
			HttpContext.Current = null;
			new OutgoingWebResponse().Respond();
		}

		[Test]
		public void Respond() {
			StringWriter writer = new StringWriter();
			HttpRequest httpRequest = new HttpRequest("file", "http://server", string.Empty);
			HttpResponse httpResponse = new HttpResponse(writer);
			HttpContext context = new HttpContext(httpRequest, httpResponse);
			HttpContext.Current = context;

			OutgoingWebResponse response = new OutgoingWebResponse();
			response.Status = System.Net.HttpStatusCode.OK;
			response.Headers["someHeaderName"] = "someHeaderValue";
			response.Body = "some body";
			response.Respond();
			string results = writer.ToString();
			// For some reason the only output in test is the body... the headers require a web host
			Assert.AreEqual(response.Body, results);
		}
	}
}
