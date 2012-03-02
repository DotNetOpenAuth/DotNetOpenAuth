//-----------------------------------------------------------------------
// <copyright file="OAuthChannelTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Diagnostics.Contracts;
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
	using NUnit.Framework;

	[TestFixture]
	public class OAuthChannelTests : TestBase {
		private OAuthChannel channel;
		private TestWebRequestHandler webRequestHandler;
		private SigningBindingElementBase signingElement;
		private INonceStore nonceStore;
		private DotNetOpenAuth.OAuth.ServiceProviderSecuritySettings serviceProviderSecuritySettings = DotNetOpenAuth.Configuration.OAuthElement.Configuration.ServiceProvider.SecuritySettings.CreateSecuritySettings();
		private DotNetOpenAuth.OAuth.ConsumerSecuritySettings consumerSecuritySettings = DotNetOpenAuth.Configuration.OAuthElement.Configuration.Consumer.SecuritySettings.CreateSecuritySettings();

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.webRequestHandler = new TestWebRequestHandler();
			this.signingElement = new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager());
			this.nonceStore = new NonceMemoryStore(StandardExpirationBindingElement.MaximumMessageAge);
			this.channel = new OAuthServiceProviderChannel(this.signingElement, this.nonceStore, new InMemoryTokenManager(), this.serviceProviderSecuritySettings, new TestMessageFactory());
			this.channel.WebRequestHandler = this.webRequestHandler;
		}

		[TestCase, ExpectedException(typeof(ArgumentException))]
		public void CtorNullSigner() {
			new OAuthConsumerChannel(null, this.nonceStore, new InMemoryTokenManager(), this.consumerSecuritySettings, new TestMessageFactory());
		}

		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			new OAuthConsumerChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), null, new InMemoryTokenManager(), this.consumerSecuritySettings, new TestMessageFactory());
		}

		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullTokenManager() {
			new OAuthConsumerChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), this.nonceStore, null, this.consumerSecuritySettings, new TestMessageFactory());
		}

		[Test]
		public void CtorSimpleConsumer() {
			new OAuthConsumerChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), this.nonceStore, (IConsumerTokenManager)new InMemoryTokenManager(), this.consumerSecuritySettings);
		}

		[Test]
		public void CtorSimpleServiceProvider() {
			new OAuthServiceProviderChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), this.nonceStore, (IServiceProviderTokenManager)new InMemoryTokenManager(), this.serviceProviderSecuritySettings);
		}

		[Test]
		public void ReadFromRequestAuthorization() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that the OAuth ReadFromRequest method gathers parameters
		/// from the Authorization header, the query string and the entity form data.
		/// </summary>
		[Test]
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
			Assert.IsInstanceOf<TestMessage>(requestMessage);
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		[Test]
		public void ReadFromRequestForm() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.PostRequest);
		}

		[Test]
		public void ReadFromRequestQueryString() {
			this.ParameterizedReceiveTest(HttpDeliveryMethods.GetRequest);
		}

		[Test]
		public void SendDirectMessageResponse() {
			IProtocolMessage message = new TestDirectedMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
			};

			OutgoingWebResponse response = this.channel.PrepareResponse(message);
			Assert.AreSame(message, response.OriginalMessage);
			Assert.AreEqual(HttpStatusCode.OK, response.Status);
			Assert.AreEqual(2, response.Headers.Count);

			NameValueCollection body = HttpUtility.ParseQueryString(response.Body);
			Assert.AreEqual("15", body["age"]);
			Assert.AreEqual("Andrew", body["Name"]);
			Assert.AreEqual("http://hostb/pathB", body["Location"]);
		}

		[Test]
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
			IDictionary<string, string> deserializedFields = this.channel.ReadFromResponseCoreTestHook(new CachedDirectWebResponse { CachedResponseStream = ms });
			Assert.AreEqual(fields.Count, deserializedFields.Count);
			foreach (string key in fields.Keys) {
				Assert.AreEqual(fields[key], deserializedFields[key]);
			}
		}

		[TestCase, ExpectedException(typeof(ArgumentNullException))]
		public void RequestNull() {
			this.channel.Request(null);
		}

		[TestCase, ExpectedException(typeof(ArgumentException))]
		public void RequestNullRecipient() {
			IDirectedProtocolMessage message = new TestDirectedMessage(MessageTransport.Direct);
			this.channel.Request(message);
		}

		[TestCase, ExpectedException(typeof(NotSupportedException))]
		public void RequestBadPreferredScheme() {
			TestDirectedMessage message = new TestDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			message.HttpMethods = HttpDeliveryMethods.None;
			this.channel.Request(message);
		}

		[Test]
		public void RequestUsingAuthorizationHeader() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that message parts can be distributed to the query, form, and Authorization header.
		/// </summary>
		[Test]
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

		[Test]
		public void RequestUsingGet() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.GetRequest);
		}

		[Test]
		public void RequestUsingPost() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.PostRequest);
		}

		[Test]
		public void RequestUsingHead() {
			this.ParameterizedRequestTest(HttpDeliveryMethods.HeadRequest);
		}

		/// <summary>
		/// Verifies that messages asking for special HTTP status codes get them.
		/// </summary>
		[Test]
		public void SendDirectMessageResponseHonorsHttpStatusCodes() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			OutgoingWebResponse directResponse = this.channel.PrepareDirectResponseTestHook(message);
			Assert.AreEqual(HttpStatusCode.OK, directResponse.Status);

			var httpMessage = new TestDirectResponseMessageWithHttpStatus();
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, httpMessage);
			httpMessage.HttpStatusCode = HttpStatusCode.NotAcceptable;
			directResponse = this.channel.PrepareDirectResponseTestHook(httpMessage);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, directResponse.Status);
		}

		private static string CreateAuthorizationHeader(IDictionary<string, string> fields) {
			Requires.NotNull(fields, "fields");

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
				Assert.AreEqual(MessagingUtilities.GetHttpVerb(scheme), reqInfo.HttpMethod);
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
			Assert.IsInstanceOf<TestMessage>(response);
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
				{ "realm", "someValue" },
			};
			IProtocolMessage requestMessage = this.channel.ReadFromRequest(CreateHttpRequestInfo(scheme, fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOf<TestMessage>(requestMessage);
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
