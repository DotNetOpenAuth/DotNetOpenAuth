//-----------------------------------------------------------------------
// <copyright file="DirectUserToServiceProviderMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	internal class DirectUserToServiceProviderMessage : MessageBase, IDirectedProtocolMessage {
		private Uri serviceProvider;

		internal DirectUserToServiceProviderMessage(Uri serviceProvider) {
			if (serviceProvider == null) {
				throw new ArgumentNullException("serviceProvider");
			}

			this.serviceProvider = serviceProvider;
		}

		[MessagePart(Name = "oauth_token", IsRequired = false)]
		public string RequestToken { get; set; }
		[MessagePart(Name = "oauth_callback", IsRequired = false)]
		public string Callback { get; set; }

		protected override MessageTransport Transport {
			get { return MessageTransport.Indirect; }
		}

		protected override MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}

		#region IDirectedProtocolMessage Members

		Uri IDirectedProtocolMessage.Recipient {
			get { return this.serviceProvider; }
		}

		#endregion
	}
}
