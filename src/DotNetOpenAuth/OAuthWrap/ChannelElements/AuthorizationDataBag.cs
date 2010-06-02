//-----------------------------------------------------------------------
// <copyright file="AuthorizationDataBag.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.Messaging.Bindings;

	internal abstract class AuthorizationDataBag : DataBag, IAuthorizationDescription {
		protected AuthorizationDataBag(byte[] secret, bool signed = false, bool encrypted = false, bool compressed = false, TimeSpan? maximumAge = null, INonceStore decodeOnceOnly = null)
			: base(secret, signed, encrypted, compressed, maximumAge, decodeOnceOnly) {
		}

		[MessagePart]
		public string ClientIdentifier { get; set; }

		public DateTime UtcIssued {
			get { return this.UtcCreationDate; }
		}

		[MessagePart]
		public string User { get; set; }

		[MessagePart]
		public string Scope { get; set; }
	}
}
