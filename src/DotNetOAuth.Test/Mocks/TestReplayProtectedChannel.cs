//-----------------------------------------------------------------------
// <copyright file="TestReplayProtectedChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	internal class TestReplayProtectedChannel : TestSigningChannel {
		private bool messageReceived;

		internal TestReplayProtectedChannel()
			: base(true, true) {
		}

		protected override bool IsMessageReplayed(DotNetOAuth.Messaging.IReplayProtectedProtocolMessage message) {
			Assert.AreEqual("someNonce", message.Nonce, "The nonce didn't serialize correctly, or something");
			// this mock implementation passes the first time and fails subsequent times.
			bool replay = this.messageReceived;
			this.messageReceived = true;
			return replay;
		}

		protected override void ApplyReplayProtection(IReplayProtectedProtocolMessage message) {
			message.Nonce = "someNonce";
			// no-op
		}
	}
}
