//-----------------------------------------------------------------------
// <copyright file="OpenIdRelyingParty.cs" company="Andrew Arnott">
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
	/// Provides the programmatic facilities to act as an OpenId consumer.
	/// </summary>
	public sealed class OpenIdRelyingParty {
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenIdRelyingParty"/> class.
		/// </summary>
		public OpenIdRelyingParty() {
			this.OpenIdChannel = new OpenIdChannel();
		}

		/// <summary>
		/// Gets the channel to use for sending/receiving messages.
		/// </summary>
		public Channel Channel {
			get { return this.OpenIdChannel; }
		}

		/// <summary>
		/// Gets or sets the channel to use for sending/receiving messages.
		/// </summary>
		internal OpenIdChannel OpenIdChannel { get; set; }
	}
}
