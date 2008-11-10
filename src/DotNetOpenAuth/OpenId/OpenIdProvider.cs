//-----------------------------------------------------------------------
// <copyright file="OpenIdProvider.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OpenId.ChannelElements;

	/// <summary>
	/// Offers services for a web page that is acting as an OpenID identity server.
	/// </summary>
	public sealed class OpenIdProvider {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdProvider"/> class.
		/// </summary>
		public OpenIdProvider() {
			this.Channel = new OpenIdChannel();
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel { get; internal set; }
	}
}
