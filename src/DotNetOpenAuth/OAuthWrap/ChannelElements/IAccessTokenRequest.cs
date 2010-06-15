//-----------------------------------------------------------------------
// <copyright file="IAccessTokenRequest.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuthWrap.ChannelElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Messages;
	using Messaging;

	/// <summary>
	/// A message from the client to the authorization server requesting an access token.
	/// </summary>
	public interface IAccessTokenRequest : IDirectedProtocolMessage {
		/// <summary>
		/// Gets the client identifier.
		/// </summary>
		/// <value>The client identifier.</value>
		string ClientIdentifier { get; }

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		/// <value>The client secret.</value>
		string ClientSecret { get; }
	}
}
