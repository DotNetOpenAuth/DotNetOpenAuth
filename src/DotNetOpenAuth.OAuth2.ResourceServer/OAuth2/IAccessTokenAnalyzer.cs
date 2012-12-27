//-----------------------------------------------------------------------
// <copyright file="IAccessTokenAnalyzer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// An interface that resource server hosts should implement if they accept access tokens
	/// issued by non-DotNetOpenAuth authorization servers.
	/// </summary>
	public interface IAccessTokenAnalyzer {
		/// <summary>
		/// Reads an access token to find out what data it authorizes access to.
		/// </summary>
		/// <param name="message">The message carrying the access token.</param>
		/// <param name="accessToken">The access token's serialized representation.</param>
		/// <returns>The deserialized, validated token.</returns>
		/// <exception cref="ProtocolException">Thrown if the access token is expired, invalid, or from an untrusted authorization server.</exception>
		AccessToken DeserializeAccessToken(IDirectedProtocolMessage message, string accessToken);
	}
}
