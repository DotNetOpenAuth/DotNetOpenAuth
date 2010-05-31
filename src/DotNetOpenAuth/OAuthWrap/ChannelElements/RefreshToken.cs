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

	internal class RefreshToken : DataBag {
		/// <summary>
		/// Initializes a new instance of the <see cref="RefreshToken"/> class.
		/// </summary>
		/// <param name="channel">The channel.</param>
		internal RefreshToken(OAuthWrapAuthorizationServerChannel channel)
			: base(channel, true, true, true) {
			Contract.Requires<ArgumentNullException>(channel != null, "channel");
		}
	}
}
