//-----------------------------------------------------------------------
// <copyright file="CoordinatingOutgoingWebResponse.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.Mocks {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics.Contracts;
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
			Requires.NotNull(message, "message");
			Requires.NotNull(receivingChannel, "receivingChannel");

			this.receivingChannel = receivingChannel;
			this.OriginalMessage = message;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public override void Send() {
			this.Respond();
		}

		public override void Respond() {
			this.receivingChannel.PostMessage(this.OriginalMessage);
		}
	}
}
