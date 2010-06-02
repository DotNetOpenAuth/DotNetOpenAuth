//-----------------------------------------------------------------------
// <copyright file="AccessToken.cs" company="Andrew Arnott">
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

	internal class AccessToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="AccessToken"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private AccessToken(OAuthWrapAuthorizationServerChannel channel, TimeSpan? lifetime = null)
			: base(channel, true, true, true, lifetime) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
		}

		internal AccessToken(OAuthWrapAuthorizationServerChannel channel, IAuthorizationDescription authorization)
			: this(channel) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
		}

		internal static AccessToken Decode(OAuthWrapAuthorizationServerChannel channel, string value, TimeSpan lifetime, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<AccessToken>() != null);

			var self = new AccessToken(channel, lifetime);
			self.Decode(value, containingMessage);
			return self;
		}
	}
}
