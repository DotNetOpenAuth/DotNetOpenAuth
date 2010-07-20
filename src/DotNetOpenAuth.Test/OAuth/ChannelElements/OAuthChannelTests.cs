//-----------------------------------------------------------------------
// <copyright file="OAuthChannelTests.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Text;
	using System.Web;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class OAuthChannelTests : TestBase {
		private OAuthChannel channel;
		private OAuthChannel_Accessor accessor;
		private TestWebRequestHandler webRequestHandler;
		private SigningBindingElementBase signingElement;
		private INonceStore nonceStore;

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.webRequestHandler = new TestWebRequestHandler();
			this.signingElement = new RsaSha1SigningBindingElement(new InMemoryTokenManager());
			this.nonceStore = new NonceMemoryStore(StandardExpirationBindingElement.DefaultMaximumMessageAge);
			this.channel = new OAuthChannel(this.signingElement, this.nonceStore, new InMemoryTokenManager(), new TestMessageFactory());
			this.accessor = OAuthChannel_Accessor.AttachShadow(this.channel);
			this.channel.WebRequestHandler = this.webRequestHandler;
		}

		[TestMethod, ExpectedException(typeof(ArgumentException))]
		public void CtorNullSigner() {
			new OAuthChannel(null, this.nonceStore, new InMemoryTokenManager(), new TestMessageFactory());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			new OAuthChannel(new RsaSha1SigningBindingElement(new InMemoryTokenManager()), null, new InMemoryTokenManager(), new TestMessageFactory());
		}

		[TestMethod, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullTokenManager() {
			new OAuthChannel(new RsaSha1SigningBindingElement(new InMemoryTokenManager()), this.nonceStore, null, new TestMessageFactory());
		}

		[TestMethod]
		public void CtorSimpleConsumer() {
			new OAuthChannel(new RsaSha1SigningBindingElement(new InMemoryTokenManager()), this.nonceStore, (IConsumerTokenManager)new InMemoryTokenManager());
		}

		[TestMethod]
		public void CtorSimpleServiceProvider() {
			new OAuthChannel(new RsaSha1SigningBindingElement(new InMemoryTokenManager()), this.nonceStore, (IServiceProviderTokenManager)new InMemoryTokenManager());
		}

		[TestMethod]
		public void ReadFromRequestAuthorization() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that the OAuth ReadFromRequest method gathers parameters
		/// from the Authorization header, the query string and the entity form data.
		/// </summary>
		[TestMethod]
		public void ReadFromRequestAuthorizationScattered() {
			// Start by creating a standard POST HTTP request.
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
			};
			HttpRequestInfo requestInfo = CreateHttpRequestInfo(HttpDeliveryMethods.PostRequest, fields);

			// Now add another field to the request URL
			UriBuilder builder = new UriBuilder(requestInfo.UrlBeforeRewriting);
			builder.Query = "Name=Andrew";
			requestInfo.UrlBeforeRewriting = builder.Uri;
			requestInfo.RawUrl = builder.Path + builder.Query + builder.Fragment;

			// Finally, add an Authorization header
			fields = new Dictionary<string, string> {
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
			};
			requestInfo.Headers.Add(HttpRequestHeader.Authorization, CreateAuthorizationHeader(fields));

			IDirectedProtocolMessage requestMessage = this.channel.ReadFromRequest(requestInfo);

			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		[TestMethod]
		public void ReadFromRequestForm() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.PostRequest);
		}

		[TestMethod]
		public void ReadFromRequestQueryString() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.GetRequest);
		}

		[TestMethod]
		public void SendDirectMessageResponse() {
			IProtocolMessage message = new TestDirectedMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
			};

			OutgoingWebResponse response = this.channel.PrepareResponse(message);
			Assert.AreSame(message, response.OriginalMessage);
			Assert.AreEqual(HttpStatusCode.OK, response.Status);
			Assert.AreEqual(0, response.Headers.Count);

			NameValueCollection body = HttpUtility.ParseQueryString(response.Body);
			Assert.AreEqual("15", body["age"]);
			Assert.AreEqual("Andrew", body["Name"]);
			Assert.AreEqual("http://hostb/pathB", body["Location"]);
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
			Channel_Accessor channelAccessor = Channel_Accessor.AttachShadow(this.channel);
			IDictionary<string, string> deserializedFields = channelAccessor.ReadFromResponseCore(new CachedDirectWebResponse { CachedResponseStream = ms });
			Assert.AreEqual(fields.Count, deserializedFields.Count);
			foreach (string key in fields.Keys) {
				Assert.AreEqual(fields[key], deserializedFields[key]);
			}
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
			message.HttpMethods = HttpDeliveryMethods.None;
			this.channel.Request(message);
		}

		[TestMethod]
		public void RequestUsingAuthorizationHeader() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that message parts can be distributed to the query, form, and Authorization header.
		/// </summary>
		[TestMethod]
		public void RequestUsingAuthorizationHeaderScattered() {
			TestDirectedMessage request = new TestDirectedMessage(MessageTransport.Direct) {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
				Recipient = new Uri("http://localtest"),
				Timestamp = DateTime.UtcNow,
				HttpMethods = HttpDeliveryMethods.AuthorizationHeaderRequest,
			};

			// ExtraData should appear in the form since this is a POST request,
			// and only official message parts get a place in the Authorization header.
			((IProtocolMessage)request).ExtraData["appearinform"] = "formish";
			request.Recipient = new Uri("http://localhost/?appearinquery=queryish");
			request.HttpMethods = HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.PostRequest;

			HttpWebRequest webRequest = this.channel.InitializeRequest(request);
			Assert.IsNotNull(webRequest);
			Assert.AreEqual("POST", webRequest.Method);
			Assert.AreEqual(request.Recipient, webRequest.RequestUri);

			var declaredParts = new Dictionary<string, string> {
					{ "age", request.Age.ToString() },
					{ "Name", request.Name },
					{ "Location", request.Location.AbsoluteUri },
					{ "Timestamp", XmlConvert.ToString(request.Timestamp, XmlDateTimeSerializationMode.Utc) },
				};

			Assert.AreEqual(CreateAuthorizationHeader(declaredParts), webRequest.Headers[HttpRequestHeader.Authorization]);
			Assert.AreEqual("appearinform=formish", this.webRequestHandler.RequestEntityAsString);
		}

		[TestMethod]
		public void RequestUsingGet() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.GetRequest);
		}

		[TestMethod]
		public void RequestUsingPost() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.PostRequest);
		}

		/// <summary>
		/// Verifies that messages asking for special HTTP status codes get them.
		/// </summary>
		[TestMethod]
		public void SendDirectMessageResponseHonorsHttpStatusCodes() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			OutgoingWebResponse directResponse = this.accessor.PrepareDirectResponse(message);
			Assert.AreEqual(HttpStatusCode.OK, directResponse.Status);

			var httpMessage = new TestDirectResponseMessageWithHttpStatus();
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, httpMessage);
			httpMessage.HttpStatusCode = HttpStatusCode.NotAcceptable;
			directResponse = this.accessor.PrepareDirectResponse(httpMessage);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, directResponse.Status);
		}

		private static string CreateAuthorizationHeader(IDictionary<string, string> fields) {
			ErrorUtilities.VerifyArgumentNotNull(fields, "fields");

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

		private static HttpRequestInfo CreateHttpRequestInfo(HttpDeliveryMethods scheme, IDictionary<string, string> fields) {
			string query = MessagingUtilities.CreateQueryString(fields);
			UriBuilder requestUri = new UriBuilder("http://localhost/path");
			WebHeaderCollection headers = new WebHeaderCollection();
			MemoryStream ms = new MemoryStream();
			string method;
			switch (scheme) {
				case HttpDeliveryMethods.PostRequest:
					method = "POST";
					headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
					StreamWriter sw = new StreamWriter(ms);
					sw.Write(query);
					sw.Flush();
					ms.Position = 0;
					break;
				case HttpDeliveryMethods.GetRequest:
					method = "GET";
					requestUri.Query = query;
					break;
				case HttpDeliveryMethods.AuthorizationHeaderRequest:
					method = "GET";
					headers.Add(HttpRequestHeader.Authorization, CreateAuthorizationHeader(fields));
					break;
				default:
					throw new ArgumentOutOfRangeException("scheme", scheme, "Unexpected value");
			}
			HttpRequestInfo request = new HttpRequestInfo {
				HttpMethod = method,
				UrlBeforeRewriting = requestUri.Uri,
				RawUrl = requestUri.Path + requestUri.Query + requestUri.Fragment,
				Headers = headers,
				InputStream = ms,
			};

			return request;
		}

		private static HttpRequestInfo ConvertToRequestInfo(HttpWebRequest request, Stream postEntity) {
			HttpRequestInfo info = new HttpRequestInfo {
				HttpMethod = request.Method,
				UrlBeforeRewriting = request.RequestUri,
				RawUrl = request.RequestUri.AbsolutePath + request.RequestUri.Query + request.RequestUri.Fragment,
				Headers = request.Headers,
				InputStream = postEntity,
			};
			return info;
		}

		private void ParameterizedRequestTest(HttpDeliveryMethods scheme) {
			TestDirectedMessage request = new TestDirectedMessage(MessageTransport.Direct) {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
				Recipient = new Uri("http://localtest"),
				Timestamp = DateTime.UtcNow,
				HttpMethods = scheme,
			};

			CachedDirectWebResponse rawResponse = null;
			this.webRequestHandler.Callback = (req) => {
				Assert.IsNotNull(req);
				HttpRequestInfo reqInfo = ConvertToRequestInfo(req, this.webRequestHandler.RequestEntityStream);
				Assert.AreEqual(scheme == HttpDeliveryMethods.PostRequest ? "POST" : "GET", reqInfo.HttpMethod);
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
				rawResponse = new CachedDirectWebResponse();
				rawResponse.SetResponse(MessagingUtilities.CreateQueryString(responseFields));
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

		private void ParameterizedReceiveTest(HttpDeliveryMethods scheme) {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
				{ "realm" , "someValue" },
			};
			IProtocolMessage requestMessage = this.channel.ReadFromRequest(CreateHttpRequestInfo(scheme, fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
			if (scheme == HttpDeliveryMethods.AuthorizationHeaderRequest) {
				// The realm value should be ignored in the authorization header
				Assert.IsFalse(((IMessage)testMessage).ExtraData.ContainsKey("realm"));
			} else {
				Assert.AreEqual("someValue", ((IMessage)testMessage).ExtraData["realm"]);
			}
		}
	}
}
