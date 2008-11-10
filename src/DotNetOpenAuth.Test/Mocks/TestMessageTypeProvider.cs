//-----------------------------------------------------------------------
// <copyright file="TestMessageTypeProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class TestMessageTypeProvider : IMessageTypeProvider {
		private bool signedMessages;
		private bool expiringMessages;
		private bool replayMessages;

		internal TestMessageTypeProvider()
			: this(false, false, false) {
		}

		internal TestMessageTypeProvider(bool signed, bool expiring, bool replay) {
			if ((!signed && expiring) || (!expiring && replay)) {
				throw new ArgumentException("Invalid combination of protection.");
			}
			this.signedMessages = signed;
			this.expiringMessages = expiring;
			this.replayMessages = replay;
		}

		#region IMessageTypeProvider Members

		public Type GetRequestMessageType(IDictionary<string, string> fields) {
			if (fields.ContainsKey("age")) {
				if (this.signedMessages) {
					if (this.expiringMessages) {
						if (this.replayMessages) {
							return typeof(TestReplayProtectedMessage);
						}
						return typeof(TestExpiringMessage);
					}
					return typeof(TestSignedDirectedMessage);
				}
				return typeof(TestDirectedMessage);
			}
			return null;
		}

		public Type GetResponseMessageType(IProtocolMessage request, IDictionary<string, string> fields) {
			return this.GetRequestMessageType(fields);
		}

		#endregion
	}
}
