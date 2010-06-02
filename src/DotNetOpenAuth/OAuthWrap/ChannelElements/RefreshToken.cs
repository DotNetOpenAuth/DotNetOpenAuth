//-----------------------------------------------------------------------
// <copyright file="RefreshToken.cs" company="Andrew Arnott">
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

	internal class RefreshToken : AuthorizationDataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		private RefreshToken(OAuthWrapAuthorizationServerChannel channel)
			: base(channel, true, true, true) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
		}

		internal RefreshToken(OAuthWrapAuthorizationServerChannel channel, IAuthorizationDescription authorization)
			: this(channel) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentNullException>(authorization != null, "authorization");

			this.ClientIdentifier = authorization.ClientIdentifier;
			this.UtcCreationDate = authorization.UtcIssued;
			this.User = authorization.User;
			this.Scope = authorization.Scope;
		}

		internal static RefreshToken Decode(OAuthWrapAuthorizationServerChannel channel, string value, IProtocolMessage containingMessage) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(value));
			Contract.Requires<ArgumentNullException>(containingMessage != null, "containingMessage");
			Contract.Ensures(Contract.Result<RefreshToken>() != null);

			var self = new RefreshToken(channel);
			self.Decode(value, containingMessage);
			return self;
		}
	}
}
