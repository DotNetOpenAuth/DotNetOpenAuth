//-----------------------------------------------------------------------
// <copyright file="OAuthChannelTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OAuth.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Web;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.OAuth.ChannelElements;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;
	using Validation;

	[TestFixture]
	public class OAuthChannelTests : TestBase {
		private OAuthChannel channel;
		private SigningBindingElementBase signingElement;
		private INonceStore nonceStore;
		private DotNetOpenAuth.OAuth.ServiceProviderSecuritySettings serviceProviderSecuritySettings = DotNetOpenAuth.Configuration.OAuthElement.Configuration.ServiceProvider.SecuritySettings.CreateSecuritySettings();

		[SetUp]
		public override void SetUp() {
			base.SetUp();

			this.signingElement = new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager());
			this.nonceStore = new MemoryNonceStore(StandardExpirationBindingElement.MaximumMessageAge);
			this.channel = new OAuthServiceProviderChannel(this.signingElement, this.nonceStore, new InMemoryTokenManager(), this.serviceProviderSecuritySettings, new TestMessageFactory(), this.HostFactories);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public void CtorNullSigner() {
			new OAuthServiceProviderChannel(null, this.nonceStore, new InMemoryTokenManager(), this.serviceProviderSecuritySettings, new TestMessageFactory());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullStore() {
			new OAuthServiceProviderChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), null, new InMemoryTokenManager(), this.serviceProviderSecuritySettings, new TestMessageFactory());
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void CtorNullTokenManager() {
			new OAuthServiceProviderChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), this.nonceStore, null, this.serviceProviderSecuritySettings, new TestMessageFactory());
		}

		[Test]
		public void CtorSimpleServiceProvider() {
			new OAuthServiceProviderChannel(new RsaSha1ServiceProviderSigningBindingElement(new InMemoryTokenManager()), this.nonceStore, (IServiceProviderTokenManager)new InMemoryTokenManager(), this.serviceProviderSecuritySettings);
		}

		[Test]
		public async Task ReadFromRequestAuthorization() {
			await this.ParameterizedReceiveTestAsync(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that the OAuth ReadFromRequest method gathers parameters
		/// from the Authorization header, the query string and the entity form data.
		/// </summary>
		[Test]
		public async Task ReadFromRequestAuthorizationScattered() {
			// Start by creating a standard POST HTTP request.
			var postedFields = new Dictionary<string, string> {
				{ "age", "15" },
			};

			// Now add another field to the request URL
			var builder = new UriBuilder(MessagingTestBase.DefaultUrlForHttpRequestInfo);
			builder.Query = "Name=Andrew";

			// Finally, add an Authorization header
			var authHeaderFields = new Dictionary<string, string> {
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
			};
			var headers = new NameValueCollection();
			headers.Add(HttpRequestHeaders.Authorization, CreateAuthorizationHeader(authHeaderFields));
			headers.Add(HttpRequestHeaders.ContentType, Channel.HttpFormUrlEncoded);

			var requestInfo = new HttpRequestInfo("POST", builder.Uri, form: postedFields.ToNameValueCollection(), headers: headers);

			IDirectedProtocolMessage requestMessage = await this.channel.ReadFromRequestAsync(requestInfo.AsHttpRequestMessage(), CancellationToken.None);

			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOf<TestMessage>(requestMessage);
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		[Test]
		public async Task ReadFromRequestForm() {
			await this.ParameterizedReceiveTestAsync(HttpDeliveryMethods.PostRequest);
		}

		[Test]
		public async Task ReadFromRequestQueryString() {
			await this.ParameterizedReceiveTestAsync(HttpDeliveryMethods.GetRequest);
		}

		[Test]
		public async Task SendDirectMessageResponse() {
			IProtocolMessage message = new TestDirectedMessage {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
			};

			var response = await this.channel.PrepareResponseAsync(message);
			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			Assert.AreEqual(Channel.HttpFormUrlEncodedContentType.MediaType, response.Content.Headers.ContentType.MediaType);

			NameValueCollection body = HttpUtility.ParseQueryString(await response.Content.ReadAsStringAsync());
			Assert.AreEqual("15", body["age"]);
			Assert.AreEqual("Andrew", body["Name"]);
			Assert.AreEqual("http://hostb/pathB", body["Location"]);
		}

		[Test]
		public async Task ReadFromResponse() {
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
			IDictionary<string, string> deserializedFields = await this.channel.ReadFromResponseCoreAsyncTestHook(
				new HttpResponseMessage { Content = new StreamContent(ms) },
				CancellationToken.None);
			Assert.AreEqual(fields.Count, deserializedFields.Count);
			foreach (string key in fields.Keys) {
				Assert.AreEqual(fields[key], deserializedFields[key]);
			}
		}

		[Test, ExpectedException(typeof(ArgumentNullException))]
		public async Task RequestNull() {
			await this.channel.RequestAsync(null, CancellationToken.None);
		}

		[Test, ExpectedException(typeof(ArgumentException))]
		public async Task RequestNullRecipient() {
			IDirectedProtocolMessage message = new TestDirectedMessage(MessageTransport.Direct);
			await this.channel.RequestAsync(message, CancellationToken.None);
		}

		[Test, ExpectedException(typeof(NotSupportedException))]
		public async Task RequestBadPreferredScheme() {
			TestDirectedMessage message = new TestDirectedMessage(MessageTransport.Direct);
			message.Recipient = new Uri("http://localtest");
			message.HttpMethods = HttpDeliveryMethods.None;
			await this.channel.RequestAsync(message, CancellationToken.None);
		}

		[Test]
		public async Task RequestUsingAuthorizationHeader() {
			await this.ParameterizedRequestTestAsync(HttpDeliveryMethods.AuthorizationHeaderRequest);
		}

		/// <summary>
		/// Verifies that message parts can be distributed to the query, form, and Authorization header.
		/// </summary>
		[Test]
		public async Task RequestUsingAuthorizationHeaderScattered() {
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

			var webRequest = await this.channel.InitializeRequestAsync(request, CancellationToken.None);
			Assert.IsNotNull(webRequest);
			Assert.AreEqual(HttpMethod.Post, webRequest.Method);
			Assert.AreEqual(request.Recipient, webRequest.RequestUri);

			var declaredParts = new Dictionary<string, string> {
					{ "age", request.Age.ToString() },
					{ "Name", request.Name },
					{ "Location", request.Location.AbsoluteUri },
					{ "Timestamp", XmlConvert.ToString(request.Timestamp, XmlDateTimeSerializationMode.Utc) },
				};

			Assert.AreEqual(CreateAuthorizationHeader(declaredParts), webRequest.Headers.Authorization.ToString());
			Assert.AreEqual("appearinform=formish", await webRequest.Content.ReadAsStringAsync());
		}

		[Test]
		public async Task RequestUsingGet() {
			await this.ParameterizedRequestTestAsync(HttpDeliveryMethods.GetRequest);
		}

		[Test]
		public async Task RequestUsingPost() {
			await this.ParameterizedRequestTestAsync(HttpDeliveryMethods.PostRequest);
		}

		[Test]
		public async Task RequestUsingHead() {
			await this.ParameterizedRequestTestAsync(HttpDeliveryMethods.HeadRequest);
		}

		/// <summary>
		/// Verifies that messages asking for special HTTP status codes get them.
		/// </summary>
		[Test]
		public void SendDirectMessageResponseHonorsHttpStatusCodes() {
			IProtocolMessage message = MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired);
			var directResponse = this.channel.PrepareDirectResponseTestHook(message);
			Assert.AreEqual(HttpStatusCode.OK, directResponse.StatusCode);

			var httpMessage = new TestDirectResponseMessageWithHttpStatus();
			MessagingTestBase.GetStandardTestMessage(MessagingTestBase.FieldFill.AllRequired, httpMessage);
			httpMessage.HttpStatusCode = HttpStatusCode.NotAcceptable;
			directResponse = this.channel.PrepareDirectResponseTestHook(httpMessage);
			Assert.AreEqual(HttpStatusCode.NotAcceptable, directResponse.StatusCode);
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
			var requestUri = new UriBuilder(MessagingTestBase.DefaultUrlForHttpRequestInfo);
			var headers = new NameValueCollection();
			NameValueCollection form = null;
			string method;
			switch (scheme) {
				case HttpDeliveryMethods.PostRequest:
					method = "POST";
					form = fields.ToNameValueCollection();
					headers.Add(HttpRequestHeaders.ContentType, Channel.HttpFormUrlEncoded);
					break;
				case HttpDeliveryMethods.GetRequest:
					method = "GET";
					requestUri.Query = MessagingUtilities.CreateQueryString(fields);
					break;
				case HttpDeliveryMethods.AuthorizationHeaderRequest:
					method = "GET";
					headers.Add(HttpRequestHeaders.Authorization, CreateAuthorizationHeader(fields));
					break;
				default:
					throw new ArgumentOutOfRangeException("scheme", scheme, "Unexpected value");
			}

			return new HttpRequestInfo(method, requestUri.Uri, form: form, headers: headers);
		}

		private static HttpRequestInfo ConvertToRequestInfo(HttpWebRequest request, Stream postEntity) {
			return new HttpRequestInfo(request.Method, request.RequestUri, request.Headers, postEntity);
		}

		private async Task ParameterizedRequestTestAsync(HttpDeliveryMethods scheme) {
			var request = new TestDirectedMessage(MessageTransport.Direct) {
				Age = 15,
				Name = "Andrew",
				Location = new Uri("http://hostb/pathB"),
				Recipient = new Uri("http://localtest"),
				Timestamp = DateTime.UtcNow,
				HttpMethods = scheme,
			};

			Handle(request.Recipient).By(
				async (req, ct) => {
					Assert.IsNotNull(req);
					Assert.AreEqual(MessagingUtilities.GetHttpVerb(scheme), req.Method);
					var incomingMessage = (await this.channel.ReadFromRequestAsync(req, CancellationToken.None)) as TestMessage;
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
					var rawResponse = new HttpResponseMessage();
					rawResponse.Content = new StringContent(MessagingUtilities.CreateQueryString(responseFields));
					return rawResponse;
				});

			IProtocolMessage response = await this.channel.RequestAsync(request, CancellationToken.None);
			Assert.IsNotNull(response);
			Assert.IsInstanceOf<TestMessage>(response);
			TestMessage responseMessage = (TestMessage)response;
			Assert.AreEqual(request.Age, responseMessage.Age);
			Assert.AreEqual(request.Name, responseMessage.Name);
			Assert.AreEqual(request.Location, responseMessage.Location);
		}

		private async Task ParameterizedReceiveTestAsync(HttpDeliveryMethods scheme) {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
				{ "Timestamp", XmlConvert.ToString(DateTime.UtcNow, XmlDateTimeSerializationMode.Utc) },
				{ "realm", "someValue" },
			};
			IProtocolMessage requestMessage = await this.channel.ReadFromRequestAsync(CreateHttpRequestInfo(scheme, fields).AsHttpRequestMessage(), CancellationToken.None);
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
