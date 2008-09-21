//-----------------------------------------------------------------------
// <copyright file="AccessProtectedResourcesMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	internal class AccessProtectedResourcesMessage : MessageBase, IDirectedProtocolMessage {
		private Uri serviceProvider;

		internal AccessProtectedResourcesMessage(Uri serviceProvider) {
			if (serviceProvider == null) {
				throw new ArgumentNullException("serviceProvider");
			}

			this.serviceProvider = serviceProvider;
		}

		[MessagePart(Name = "oauth_consumer_key", IsRequired = true)]
		public string ConsumerKey { get; set; }
		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string AccessToken { get; set; }
		[MessagePart(Name = "oauth_signature_method", IsRequired = true)]
		public string SignatureMethod { get; set; }
		[MessagePart(Name = "oauth_signature", IsRequired = true)]
		public string Signature { get; set; }
		[MessagePart(Name = "oauth_timestamp", IsRequired = true)]
		public Uri Timestamp { get; set; }
		[MessagePart(Name = "oauth_nonce", IsRequired = true)]
		public Uri Nonce { get; set; }
		[MessagePart(Name = "oauth_version", IsRequired = false)]
		public Uri Version { get; set; }

		protected override MessageTransport Transport {
			get { return MessageTransport.Direct; }
		}

		protected override MessageProtection RequiredProtection {
			get { return MessageProtection.All; }
		}
	
		#region IDirectedProtocolMessage Members

		Uri IDirectedProtocolMessage.Recipient {
			get { return this.serviceProvider; }
		}

		#endregion
	}
}
