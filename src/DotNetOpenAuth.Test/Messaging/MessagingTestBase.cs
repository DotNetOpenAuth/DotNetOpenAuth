﻿//-----------------------------------------------------------------------
// <copyright file="MessagingTestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	/// <summary>
	/// The base class that all messaging test classes inherit from.
	/// </summary>
	public class MessagingTestBase : TestBase {
		internal enum FieldFill {
			/// <summary>
			/// An empty dictionary is returned.
			/// </summary>
			None,

			/// <summary>
			/// Only enough fields for the <see cref="TestMessageFactory"/>
			/// to identify the message are included.
			/// </summary>
			IdentifiableButNotAllRequired,

			/// <summary>
			/// All fields marked as required are included.
			/// </summary>
			AllRequired,

			/// <summary>
			/// All user-fillable fields in the message, leaving out those whose
			/// values are to be set by channel binding elements.
			/// </summary>
			CompleteBeforeBindings,
		}

		internal Channel Channel { get; set; }

		[SetUp]
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
				UrlBeforeRewriting = requestUri.Uri,
				Headers = headers,
				InputStream = ms,
			};

			return request;
		}

		internal static Channel CreateChannel(MessageProtections capabilityAndRecognition) {
			return CreateChannel(capabilityAndRecognition, capabilityAndRecognition);
		}

		internal static Channel CreateChannel(MessageProtections capability, MessageProtections recognition) {
			var bindingElements = new List<IChannelBindingElement>();
			if (capability >= MessageProtections.TamperProtection) {
				bindingElements.Add(new MockSigningBindingElement());
			}
			if (capability >= MessageProtections.Expiration) {
				bindingElements.Add(new StandardExpirationBindingElement());
			}
			if (capability >= MessageProtections.ReplayProtection) {
				bindingElements.Add(new MockReplayProtectionBindingElement());
			}

			bool signing = false, expiration = false, replay = false;
			if (recognition >= MessageProtections.TamperProtection) {
				signing = true;
			}
			if (recognition >= MessageProtections.Expiration) {
				expiration = true;
			}
			if (recognition >= MessageProtections.ReplayProtection) {
				replay = true;
			}

			var typeProvider = new TestMessageFactory(signing, expiration, replay);
			return new TestChannel(typeProvider, bindingElements.ToArray());
		}

		internal static IDictionary<string, string> GetStandardTestFields(FieldFill fill) {
			TestMessage expectedMessage = GetStandardTestMessage(fill);

			var fields = new Dictionary<string, string>();
			if (fill >= FieldFill.IdentifiableButNotAllRequired) {
				fields.Add("age", expectedMessage.Age.ToString());
			}
			if (fill >= FieldFill.AllRequired) {
				fields.Add("Timestamp", XmlConvert.ToString(expectedMessage.Timestamp, XmlDateTimeSerializationMode.Utc));
			}
			if (fill >= FieldFill.CompleteBeforeBindings) {
				fields.Add("Name", expectedMessage.Name);
				fields.Add("Location", expectedMessage.Location.AbsoluteUri);
			}

			return fields;
		}

		internal static TestMessage GetStandardTestMessage(FieldFill fill) {
			TestMessage message = new TestDirectedMessage();
			GetStandardTestMessage(fill, message);
			return message;
		}

		internal static void GetStandardTestMessage(FieldFill fill, TestMessage message) {
			if (message == null) {
				throw new ArgumentNullException("message");
			}

			if (fill >= FieldFill.IdentifiableButNotAllRequired) {
				message.Age = 15;
			}
			if (fill >= FieldFill.AllRequired) {
				message.Timestamp = DateTime.SpecifyKind(DateTime.Parse("9/19/2008 8 AM"), DateTimeKind.Utc);
			}
			if (fill >= FieldFill.CompleteBeforeBindings) {
				message.Name = "Andrew";
				message.Location = new Uri("http://localtest/path");
			}
		}

		internal void ParameterizedReceiveTest(string method) {
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.CompleteBeforeBindings);

			IDirectedProtocolMessage requestMessage = this.Channel.ReadFromRequest(CreateHttpRequestInfo(method, fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOf<TestMessage>(requestMessage);
			TestMessage actualMessage = (TestMessage)requestMessage;
			Assert.AreEqual(expectedMessage.Age, actualMessage.Age);
			Assert.AreEqual(expectedMessage.Name, actualMessage.Name);
			Assert.AreEqual(expectedMessage.Location, actualMessage.Location);
		}

		internal void ParameterizedReceiveProtectedTest(DateTime? utcCreatedDate, bool invalidSignature) {
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.CompleteBeforeBindings);
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			fields.Add("Signature", invalidSignature ? "badsig" : MockSigningBindingElement.MessageSignature);
			fields.Add("Nonce", "someNonce");
			if (utcCreatedDate.HasValue) {
				utcCreatedDate = DateTime.Parse(utcCreatedDate.Value.ToUniversalTime().ToString()); // round off the milliseconds so comparisons work later
				fields.Add("created_on", XmlConvert.ToString(utcCreatedDate.Value, XmlDateTimeSerializationMode.Utc));
			}
			IProtocolMessage requestMessage = this.Channel.ReadFromRequest(CreateHttpRequestInfo("GET", fields));
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOf<TestSignedDirectedMessage>(requestMessage);
			TestSignedDirectedMessage actualMessage = (TestSignedDirectedMessage)requestMessage;
			Assert.AreEqual(expectedMessage.Age, actualMessage.Age);
			Assert.AreEqual(expectedMessage.Name, actualMessage.Name);
			Assert.AreEqual(expectedMessage.Location, actualMessage.Location);
			if (utcCreatedDate.HasValue) {
				IExpiringProtocolMessage expiringMessage = (IExpiringProtocolMessage)requestMessage;
				Assert.AreEqual(utcCreatedDate.Value, expiringMessage.UtcCreationDate);
			}
		}
	}
}
