//-----------------------------------------------------------------------
// <copyright file="CoordinatingOutgoingWebResponse.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	internal class CoordinatingOutgoingWebResponse : OutgoingWebResponse {
		private CoordinatingChannel receivingChannel;

		/// <summary>
		/// Initializes a new instance of the <see cref="CoordinatingOutgoingWebResponse"/> class.
		/// </summary>
		/// <param name="message">The direct response message to send to the remote channel.  This message will be cloned.</param>
		/// <param name="receivingChannel">The receiving channel.</param>
		internal CoordinatingOutgoingWebResponse(IProtocolMessage message, CoordinatingChannel receivingChannel) {
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
