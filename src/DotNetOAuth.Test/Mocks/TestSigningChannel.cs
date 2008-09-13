//-----------------------------------------------------------------------
// <copyright file="TestSigningChannel.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	internal class TestSigningChannel : TestChannel {
		internal const string MessageSignature = "mocksignature";

		internal TestSigningChannel(bool expiring, bool replay)
			: base(new TestMessageTypeProvider(true, expiring, replay)) {
		}

		protected override void Sign(ISignedProtocolMessage message) {
			message.Signature = MessageSignature;
		}

		protected override bool IsSignatureValid(ISignedProtocolMessage message) {
			return message.Signature == MessageSignature;
		}
	}
}
