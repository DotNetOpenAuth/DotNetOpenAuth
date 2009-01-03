//-----------------------------------------------------------------------
// <copyright file="CoordinatingUserAgentResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class CoordinatingUserAgentResponse : UserAgentResponse {
		private CoordinatingChannel receivingChannel;

		internal CoordinatingUserAgentResponse(IProtocolMessage message, CoordinatingChannel receivingChannel) {
			ErrorUtilities.VerifyArgumentNotNull(message, "message");
			ErrorUtilities.VerifyArgumentNotNull(receivingChannel, "receivingChannel");

			this.receivingChannel = receivingChannel;
			this.OriginalMessage = message;
		}

		public override void Send() {
			this.receivingChannel.PostMessage(this.OriginalMessage);
		}
	}
}
