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
		internal AccessToken(OAuthWrapAuthorizationServerChannel channel, TimeSpan lifetime)
			: base(channel, true, true, true, lifetime) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
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
