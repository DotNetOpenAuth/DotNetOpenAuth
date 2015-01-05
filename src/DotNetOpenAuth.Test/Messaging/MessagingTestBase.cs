//-----------------------------------------------------------------------
// <copyright file="MessagingTestBase.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test {
	using System;
	using System.Collections.Generic;
	using System.Collections.Specialized;
	using System.Globalization;
	using System.IO;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	/// <summary>
	/// The base class that all messaging test classes inherit from.
	/// </summary>
	public class MessagingTestBase : TestBase {
		protected internal const string DefaultUrlForHttpRequestInfo = "http://localhost/path";

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

			this.Channel = new TestChannel(this.HostFactories);
		}

		internal static HttpRequestMessage CreateHttpRequestInfo(HttpMethod method, IDictionary<string, string> fields) {
			var result = new HttpRequestMessage() { Method = method };
			var requestUri = new UriBuilder(DefaultUrlForHttpRequestInfo);
			if (method == HttpMethod.Post) {
				result.Content = new FormUrlEncodedContent(fields);
			} else if (method == HttpMethod.Get) {
				requestUri.AppendQueryArgs(fields);
			} else {
				throw new ArgumentOutOfRangeException("method", method, "Expected POST or GET");
			}

			result.RequestUri = requestUri.Uri;
			return result;
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
				message.Timestamp = DateTime.ParseExact("09/09/2008 08:00", "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture);
			}
			if (fill >= FieldFill.CompleteBeforeBindings) {
				message.Name = "Andrew";
				message.Location = new Uri("http://localtest/path");
			}
		}

		internal Channel CreateChannel(MessageProtections capabilityAndRecognition) {
			return this.CreateChannel(capabilityAndRecognition, capabilityAndRecognition);
		}

		internal Channel CreateChannel(MessageProtections capability, MessageProtections recognition) {
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
			return new TestChannel(typeProvider, bindingElements.ToArray(), this.HostFactories);
		}

		internal async Task ParameterizedReceiveTestAsync(HttpMethod method) {
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.CompleteBeforeBindings);

			IDirectedProtocolMessage requestMessage = await this.Channel.ReadFromRequestAsync(CreateHttpRequestInfo(method, fields), CancellationToken.None);
			Assert.IsNotNull(requestMessage);
			Assert.IsInstanceOf<TestMessage>(requestMessage);
			TestMessage actualMessage = (TestMessage)requestMessage;
			Assert.AreEqual(expectedMessage.Age, actualMessage.Age);
			Assert.AreEqual(expectedMessage.Name, actualMessage.Name);
			Assert.AreEqual(expectedMessage.Location, actualMessage.Location);
		}

		internal async Task ParameterizedReceiveProtectedTestAsync(DateTime? utcCreatedDate, bool invalidSignature) {
			TestMessage expectedMessage = GetStandardTestMessage(FieldFill.CompleteBeforeBindings);
			var fields = GetStandardTestFields(FieldFill.CompleteBeforeBindings);
			fields.Add("Signature", invalidSignature ? "badsig" : MockSigningBindingElement.MessageSignature);
			fields.Add("Nonce", "someNonce");
			if (utcCreatedDate.HasValue) {
				utcCreatedDate = DateTime.Parse(utcCreatedDate.Value.ToUniversalTime().ToString()); // round off the milliseconds so comparisons work later
				fields.Add("created_on", XmlConvert.ToString(utcCreatedDate.Value, XmlDateTimeSerializationMode.Utc));
			}
			IProtocolMessage requestMessage = await this.Channel.ReadFromRequestAsync(CreateHttpRequestInfo(HttpMethod.Get, fields), CancellationToken.None);
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
