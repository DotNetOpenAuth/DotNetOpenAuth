//-----------------------------------------------------------------------
// <copyright file="OAuthChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;
	using System.Xml;
	using DotNetOAuth.ChannelElements;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OAuthChannelTests : TestBase {
		private OAuthChannel channel;
		private TestWebRequestHandler webRequestHandler;
		private SigningBindingElementBase signingElement;
		private INonceStore nonceStore;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.webRequestHandler = new TestWebRequestHandler();
			this.signingElement = new RsaSha1SigningBindingElement();
			this.nonceStore = new NonceMemoryStore();
			this.channel = new OAuthChannel(this.signingElement, this.nonceStore, new TestMessageTypeProvider(), this.webRequestHandler);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullHandler() {
			new OAuthChannel(this.signingElement, this.nonceStore, new TestMessageTypeProvider(), null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorNullSigner() {
			new OAuthChannel(null, this.nonceStore, new TestMessageTypeProvider(), this.webRequestHandler);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			new OAuthChannel(this.signingElement, null, new TestMessageTypeProvider(), this.webRequestHandler);
		}

		[TestMethod]
		public void CtorSimple() {
			new OAuthChannel(this.signingElement, this.nonceStore);
		}

		[TestMethod]
		public void ReadFromRequestAuthorization() {
			this.ParameterizedReceiveTest(HttpDeliveryMethod.AuthorizationHeaderRequest);
		}

		[TestMethod]
		public void ReadFromRequestForm() {
			this.ParameterizedReceiveTest(HttpDeliveryMethod.PostRequest);
		}

		[TestMethod]
		public void ReadFromRequestQueryString() {
			this.ParameterizedReceiveTest(HttpDeliveryMethod.GetRequest);
		}

		[TestMethod]
		public void SendDirectMessageResponse() {
			IProtocolMessage message = new TestMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
			};
			this.channel.Send(message);

			Response response = this.channel.DequeueIndirectOrResponseMessage();
			Assert.AreSame(message, response.OriginalMessage);
			Assert.AreEqual(HttpStatusCode.OK, response.Status);
			Assert.AreEqual(0, response.Headers.Count);

			NameValueCollection body = HttpUtility.ParseQueryString(response.Body);
			Assert.AreEqual("15", body["age"]);
			Assert.AreEqual("Andrew", body["Name"]);
			Assert.AreEqual("http://hostb/pathB", body["Location"]);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void ReadFromResponseNull() {
			this.channel.ReadFromResponse(null);
		}

		[TestMethod]
		public void ReadFromResponse() {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
			};

			MemoryStream ms = new MemoryStream();
			StreamWriter writer = new StreamWriter(ms);
			writer.Write(MessagingUtilities.CreateQueryString(fields));
			writer.Flush();
			ms.Seek(0, SeekOrigin.Begin);
			IProtocolMessage message = this.channel.ReadFromResponse(ms);
			Assert.IsNotNull(message);
			Assert.IsInstanceOfType(message, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)message;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
			Assert.IsNull(testMessage.EmptyMember);
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void RequestNull() {
			this.channel.Request(null);
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void RequestNullRecipient() {
			IDirectedProtocolMessage message = new TestDirectedMessage(MessageTransport.Direct);
			this.channel.Request(message);
		}

		[TestMethod, ExpectedException(typeof(NotSupportedException))]
		public void RequestBadPreferredScheme() {
			TestDirectedMessage message = new TestDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			message.HttpMethods = HttpDeliveryMethod.None;
			this.channel.Request(message);
		}

		[TestMethod]
		public void RequestUsingAuthorizationHeader() {
			this.ParameterizedRequestTest(HttpDeliveryMethod.AuthorizationHeaderRequest);
		}

		[TestMethod]
		public void RequestUsingGet() {
			this.ParameterizedRequestTest(HttpDeliveryMethod.GetRequest);
		}

		[TestMethod]
		public void RequestUsingPost() {
			this.ParameterizedRequestTest(HttpDeliveryMethod.PostRequest);
		}

		private static string CreateAuthorizationHeader(IDictionary<string, string> fields) {
			if (fields == null) {
				throw new ArgumentNullException("fields");
			}

			StringBuilder authorization = new StringBuilder();
			authorization.Append("OAuth ");
			foreach (var pair in fields) {
				string key = Uri.EscapeDataString(pair.Key);
				string value = Uri.EscapeDataString(pair.Value);
				authorization.Append(key);
				authorization.Append("=\"");
				authorization.Append(value);
				authorization.Append("\",");
			}
			authorization.Length--; // remove trailing comma

			return authorization.ToString();
		}

		private static HttpRequestInfo CreateHttpRequestInfo(HttpDeliveryMethod scheme, IDictionary<string, string> fields) {
			string query = MessagingUtilities.CreateQueryString(fields);
			UriBuilder requestUri = new UriBuilder("http://localhost/path");
			WebHeaderCollection headers = new WebHeaderCollection();
			MemoryStream ms = new MemoryStream();
			string method;
			switch (scheme) {
				case HttpDeliveryMethod.PostRequest:
					method = "POST";
					headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
					StreamWriter sw = new StreamWriter(ms);
					sw.Write(query);
					sw.Flush();
					ms.Position = 0;
					break;
				case HttpDeliveryMethod.GetRequest:
					method = "GET";
					requestUri.Query = query;
					break;
				case HttpDeliveryMethod.AuthorizationHeaderRequest:
					method = "GET";
					headers.Add(HttpRequestHeader.Authorization, CreateAuthorizationHeader(fields));
					break;
				default:
					throw new ArgumentOutOfRangeException("scheme", scheme, "Unexpected value");
			}
			HttpRequestInfo request = new HttpRequestInfo {
				HttpMethod = method,
				Url = requestUri.Uri,
				Headers = headers,
				InputStream = ms,
			};

			return request;
		}

		private static HttpRequestInfo ConvertToRequestInfo(HttpWebRequest request, Stream postEntity) {
			HttpRequestInfo info = new HttpRequestInfo {
				HttpMethod = request.Method,
				Url = request.RequestUri,
				Headers = request.Headers,
				InputStream = postEntity,
			};
			return info;
		}

		private void ParameterizedRequestTest(HttpDeliveryMethod scheme) {
			TestDirectedMessage request = new TestDirectedMessage(MessageTransport.Direct) {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
				Recipient = new Uri("http://localtest"),
				Timestamp = DateTime.UtcNow,
				HttpMethods = scheme,
			};

			Response rawResponse = null;
			this.webRequestHandler.Callback = (req) => {
				Assert.IsNotNull(req);
				HttpRequestInfo reqInfo = ConvertToRequestInfo(req, this.webRequestHandler.RequestEntityStream);
				Assert.AreEqual(scheme == HttpDeliveryMethod.PostRequest ? "POST" : "GET", reqInfo.HttpMethod);
				var incomingMessage = this.channel.ReadFromRequest(reqInfo) as TestMessage;
				Assert.IsNotNull(incomingMessage);
				Assert.AreEqual(request.Age, incomingMessage.Age);
				Assert.AreEqual(request.Name, incomingMessage.Name);
				Assert.AreEqual(request.Location, incomingMessage.Location);
				Assert.AreEqual(request.Timestamp, incomingMessage.Timestamp);

				var responseFields = new Dictionary<string, string> {
					{ "age", request.Age.ToString() },
					{ "Name", request.Name },
					{ "Location", request.Location.AbsoluteUri },
					{ "Timestamp", XmlConvert.ToString(request.Timestamp, XmlDateTimeSerializationMode.Utc) },
				};
				rawResponse = new Response {
					Body = MessagingUtilities.CreateQueryString(responseFields),
				};
				return rawResponse;
			};

			IProtocolMessage response = this.channel.Request(request);
			Assert.IsNotNull(response);
			Assert.IsInstanceOfType(response, typeof(TestMessage));
			TestMessage responseMessage = (TestMessage)response;
			Assert.AreEqual(request.Age, responseMessage.Age);
			Assert.AreEqual(request.Name, responseMessage.Name);
			Assert.AreEqual(request.Location, responseMessage.Location);
		}

		private void ParameterizedReceiveTest(HttpDeliveryMethod scheme) {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
			};
			IProtocolMessage requestMessage = this.channel.ReadFromRequest(CreateHttpRequestInfo(scheme, fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}
	}
}
