//-----------------------------------------------------------------------
// <copyright file="StandardMessageFactoryTests.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Messaging {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Reflection;
	using DotNetOpenAuth.Test.Mocks;
	using NUnit.Framework;

	[TestFixture]
	public class StandardMessageFactoryTests : MessagingTestBase {
		private static readonly Version V1 = new Version(1, 0);
		private static readonly MessageReceivingEndpoint receiver = new MessageReceivingEndpoint("http://receiver", HttpDeliveryMethods.PostRequest);

		private StandardMessageFactory factory;

		public override void SetUp() {
			base.SetUp();

			this.factory = new StandardMessageFactory();
		}

		/// <summary>
		/// Verifies that AddMessageTypes throws the appropriate exception on null input.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentNullException))]
		public void AddMessageTypesNull() {
			this.factory.AddMessageTypes(null);
		}

		/// <summary>
		/// Verifies that AddMessageTypes throws the appropriate exception on null input.
		/// </summary>
		[Test, ExpectedException(typeof(ArgumentException))]
		public void AddMessageTypesNullMessageDescription() {
			this.factory.AddMessageTypes(new MessageDescription[] { null });
		}

		/// <summary>
		/// Verifies very simple recognition of a single message type
		/// </summary>
		[Test]
		public void SingleRequestMessageType() {
			this.factory.AddMessageTypes(new MessageDescription[] { MessageDescriptions.Get(typeof(RequestMessageMock), V1) });
			var fields = new Dictionary<string, string> {
				{ "random", "bits" },
			};
			Assert.IsNull(this.factory.GetNewRequestMessage(receiver, fields));
			fields["Age"] = "18";
			Assert.IsInstanceOf(typeof(RequestMessageMock), this.factory.GetNewRequestMessage(receiver, fields));
		}

		/// <summary>
		/// Verifies very simple recognition of a single message type
		/// </summary>
		[Test]
		public void SingleResponseMessageType() {
			this.factory.AddMessageTypes(new MessageDescription[] { MessageDescriptions.Get(typeof(DirectResponseMessageMock), V1) });
			var fields = new Dictionary<string, string> {
				{ "random", "bits" },
			};
			IDirectedProtocolMessage request = new RequestMessageMock(receiver.Location, V1);
			Assert.IsNull(this.factory.GetNewResponseMessage(request, fields));
			fields["Age"] = "18";
			IDirectResponseProtocolMessage response = this.factory.GetNewResponseMessage(request, fields);
			Assert.IsInstanceOf<DirectResponseMessageMock>(response);
			Assert.AreSame(request, response.OriginatingRequest);

			// Verify that we can instantiate a response with a derived-type of an expected request message.
			request = new TestSignedDirectedMessage();
			response = this.factory.GetNewResponseMessage(request, fields);
			Assert.IsInstanceOf<DirectResponseMessageMock>(response);
			Assert.AreSame(request, response.OriginatingRequest);
		}

		private class DirectResponseMessageMock : IDirectResponseProtocolMessage {
			internal DirectResponseMessageMock(RequestMessageMock request) {
				this.OriginatingRequest = request;
			}

			internal DirectResponseMessageMock(TestDirectedMessage request) {
				this.OriginatingRequest = request;
			}

			[MessagePart(IsRequired = true)]
			public int Age { get; set; }

			#region IDirectResponseProtocolMessage Members

			public IDirectedProtocolMessage OriginatingRequest { get; private set; }

			#endregion

			#region IProtocolMessage Members

			public MessageProtections RequiredProtection {
				get { throw new NotImplementedException(); }
			}

			public MessageTransport Transport {
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IMessage Members

			public Version Version {
				get { throw new NotImplementedException(); }
			}

			public System.Collections.Generic.IDictionary<string, string> ExtraData {
				get { throw new NotImplementedException(); }
			}

			public void EnsureValidMessage() {
				throw new NotImplementedException();
			}

			#endregion
		}

		private class RequestMessageMock : IDirectedProtocolMessage {
			internal RequestMessageMock(Uri recipient, Version version) {
			}

			[MessagePart(IsRequired = true)]
			public int Age { get; set; }

			#region IDirectedProtocolMessage Members

			public HttpDeliveryMethods HttpMethods {
				get { throw new NotImplementedException(); }
			}

			public Uri Recipient {
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IProtocolMessage Members

			public MessageProtections RequiredProtection {
				get { throw new NotImplementedException(); }
			}

			public MessageTransport Transport {
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IMessage Members

			public Version Version {
				get { throw new NotImplementedException(); }
			}

			public System.Collections.Generic.IDictionary<string, string> ExtraData {
				get { throw new NotImplementedException(); }
			}

			public void EnsureValidMessage() {
				throw new NotImplementedException();
			}

			#endregion
		}
	}
}
