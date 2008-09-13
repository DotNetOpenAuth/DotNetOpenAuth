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

		internal static Channel CreateChannel(ChannelProtection capabilityAndRecognition) {
			return CreateChannel(capabilityAndRecognition, capabilityAndRecognition);
		}

		internal static Channel CreateChannel(ChannelProtection capability, ChannelProtection recognition) {
			bool signing = false, expiration = false, replay = false;
			var bindingElements = new List<IChannelBindingElement>();
			if (capability >= ChannelProtection.TamperProtection) {
				bindingElements.Add(new MockSigningBindingElement());
				signing = true;
			}
			if (capability >= ChannelProtection.Expiration) {
				bindingElements.Add(new StandardMessageExpirationBindingElement());
				expiration = true;
			}
			if (capability >= ChannelProtection.ReplayProtection) {
				bindingElements.Add(new MockReplayProtectionBindingElement());
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
