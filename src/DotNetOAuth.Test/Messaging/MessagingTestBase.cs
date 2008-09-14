//-----------------------------------------------------------------------
// <copyright file="MessagingTestBase.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Xml;
	using DotNetOAuth.Messaging;
	using DotNetOAuth.Messaging.Bindings;
	using DotNetOAuth.Test.Mocks;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	/// <summary>
	/// The base class that all messaging test classes inherit from.
	/// </summary>
	public class MessagingTestBase : TestBase {
		internal Channel Channel { get; set; }

		[TestInitialize]
		public override void SetUp() {
			base.SetUp();

			this.Channel = new TestChannel();
		}

		internal static HttpRequestInfo CreateHttpRequestInfo(string method, IDictionary<string, string> fields) {
			string query = MessagingUtilities.CreateQueryString(fields);
			UriBuilder requestUri = new UriBuilder("http://localhost/path");
			WebHeaderCollection headers = new WebHeaderCollection();
			MemoryStream ms = new MemoryStream();
			if (method == "POST") {
				headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
				StreamWriter sw = new StreamWriter(ms);
				sw.Write(query);
				sw.Flush();
				ms.Position = 0;
			} else if (method == "GET") {
				requestUri.Query = query;
			} else {
				throw new ArgumentOutOfRangeException("method", method, "Expected POST or GET");
			}
			HttpRequestInfo request = new HttpRequestInfo {
				HttpMethod = method,
				Url = requestUri.Uri,
				Headers = headers,
				InputStream = ms,
			};

			return request;
		}

		internal static Channel CreateChannel(MessageProtection capabilityAndRecognition) {
			return CreateChannel(capabilityAndRecognition, capabilityAndRecognition);
		}

		internal static Channel CreateChannel(MessageProtection capability, MessageProtection recognition) {
			var bindingElements = new List<IChannelBindingElement>();
			if (capability >= MessageProtection.TamperProtection) {
				bindingElements.Add(new MockSigningBindingElement());
			}
			if (capability >= MessageProtection.Expiration) {
				bindingElements.Add(new StandardExpirationBindingElement());
			}
			if (capability >= MessageProtection.ReplayProtection) {
				bindingElements.Add(new MockReplayProtectionBindingElement());
			}

			bool signing = false, expiration = false, replay = false;
			if (recognition >= MessageProtection.TamperProtection) {
				signing = true;
			}
			if (recognition >= MessageProtection.Expiration) {
				expiration = true;
			}
			if (recognition >= MessageProtection.ReplayProtection) {
				replay = true;
			}

			var typeProvider = new TestMessageTypeProvider(signing, expiration, replay);
			return new TestChannel(typeProvider, bindingElements.ToArray());
		}

		internal void ParameterizedReceiveTest(string method) {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
			};
			IProtocolMessage requestMessage = this.Channel.ReadFromRequest(CreateHttpRequestInfo(method, fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestMessage));
			TestMessage testMessage = (TestMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
		}

		internal void ParameterizedReceiveProtectedTest(DateTime? utcCreatedDate, bool invalidSignature) {
			var fields = new Dictionary<string, string> {
				{ "age", "15" },
				{ "Name", "Andrew" },
				{ "Location", "http://hostb/pathB" },
				{ "Signature", invalidSignature ? "badsig" : MockSigningBindingElement.MessageSignature },
				{ "Nonce", "someNonce" },
			};
			if (utcCreatedDate.HasValue) {
				utcCreatedDate = DateTime.Parse(utcCreatedDate.Value.ToUniversalTime().ToString()); // round off the milliseconds so comparisons work later
				fields.Add("created_on", XmlConvert.ToString(utcCreatedDate.Value, XmlDateTimeSerializationMode.Utc));
			}
			IProtocolMessage requestMessage = this.Channel.ReadFromRequest(CreateHttpRequestInfo("GET", fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOfType(requestMessage, typeof(TestSignedDirectedMessage));
			TestSignedDirectedMessage testMessage = (TestSignedDirectedMessage)requestMessage;
			Assert.AreEqual(15, testMessage.Age);
			Assert.AreEqual("Andrew", testMessage.Name);
			Assert.AreEqual("http://hostb/pathB", testMessage.Location.AbsoluteUri);
			if (utcCreatedDate.HasValue) {
				IExpiringProtocolMessage expiringMessage = (IExpiringProtocolMessage)requestMessage;
				Assert.AreEqual(utcCreatedDate.Value, expiringMessage.UtcCreationDate);
			}
		}
	}
}
