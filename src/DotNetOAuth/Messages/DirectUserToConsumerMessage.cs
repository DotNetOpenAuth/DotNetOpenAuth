//-----------------------------------------------------------------------
// <copyright file="DirectUserToConsumerMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	internal class DirectUserToConsumerMessage : MessageBase, IDirectedProtocolMessage {
		private Uri consumer;

		internal DirectUserToConsumerMessage(Uri consumer) {
			this.consumer = consumer;
		}

		// TODO: graph in spec says this is optional, but in text is suggests it is required.
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string RequestToken { get; set; }

		protected override MessageTransport Transport {
			get { return MessageTransport.Indirect; }
		}

		protected override MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}

		#region IDirectedProtocolMessage Members

		Uri IDirectedProtocolMessage.Recipient {
			get { return this.consumer; }
		}

		#endregion
	}
}
