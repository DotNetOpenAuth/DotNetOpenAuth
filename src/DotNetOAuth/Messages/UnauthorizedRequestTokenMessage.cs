//-----------------------------------------------------------------------
// <copyright file="UnauthorizedRequestTokenMessage.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth.Messages {
	using System;
	using System.Runtime.Serialization;
	using DotNetOAuth.Messaging;

	internal class UnauthorizedRequestTokenMessage : MessageBase {
		internal UnauthorizedRequestTokenMessage() {
		}

		[MessagePart(Name = "oauth_token", IsRequired = true)]
		public string RequestToken { get; set; }
		[MessagePart(Name = "oauth_token_secret", IsRequired = true)]
		public string TokenSecret { get; set; }

		protected override MessageTransport Transport {
			get { return MessageTransport.Direct; }
		}

		protected override MessageProtection RequiredProtection {
			get { return MessageProtection.None; }
		}
	}
}
