//-----------------------------------------------------------------------
// <copyright file="TestMessageFactory.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class TestMessageFactory : IMessageFactory {
		private bool signedMessages;
		private bool expiringMessages;
		private bool replayMessages;

		internal TestMessageFactory()
			: this(false, false, false) {
		}

		internal TestMessageFactory(bool signed, bool expiring, bool replay) {
			if ((!signed && expiring) || (!expiring && replay)) {
				throw new ArgumentException("Invalid combination of protection.");
			}
			this.signedMessages = signed;
			this.expiringMessages = expiring;
			this.replayMessages = replay;
		}

		#region IMessageFactory Members

		public IDirectedProtocolMessage GetNewRequestMessage(MessageReceivingEndpoint recipient, IDictionary<string, string> fields) {
			if (fields.ContainsKey("age")) {
				if (this.signedMessages) {
					if (this.expiringMessages) {
						if (this.replayMessages) {
							return new TestReplayProtectedMessage();
						}
						return new TestExpiringMessage();
					}
					return new TestSignedDirectedMessage();
				}
				var result = new TestDirectedMessage();
				if (fields.ContainsKey("GetOnly")) {
					result.HttpMethods = HttpDeliveryMethods.GetRequest;
				}
				return result;
			}
			return null;
		}

		public IDirectResponseProtocolMessage GetNewResponseMessage(IDirectedProtocolMessage request, IDictionary<string, string> fields) {
			TestMessage message = (TestMessage)this.GetNewRequestMessage(null, fields);
			message.OriginatingRequest = request;
			return message;
		}

		#endregion
	}
}
