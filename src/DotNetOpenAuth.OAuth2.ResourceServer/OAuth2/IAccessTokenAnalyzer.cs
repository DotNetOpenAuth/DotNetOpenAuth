//-----------------------------------------------------------------------
// <copyright file="IAccessTokenAnalyzer.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Diagnostics.Contracts;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// An interface that resource server hosts should implement if they accept access tokens
	/// issued by non-DotNetOpenAuth authorization servers.
	/// </summary>
	[ContractClass((typeof(IAccessTokenAnalyzerContract)))]
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

	/// <summary>
	/// Code contract for the <see cref="IAccessTokenAnalyzer"/> interface.
	/// </summary>
	[ContractClassFor(typeof(IAccessTokenAnalyzer))]
	internal abstract class IAccessTokenAnalyzerContract : IAccessTokenAnalyzer {
		/// <summary>
		/// Prevents a default instance of the <see cref="IAccessTokenAnalyzerContract"/> class from being created.
		/// </summary>
		private IAccessTokenAnalyzerContract() {
		}

		/// <summary>
		/// Reads an access token to find out what data it authorizes access to.
		/// </summary>
		/// <param name="message">The message carrying the access token.</param>
		/// <param name="accessToken">The access token's serialized representation.</param>
		/// <returns>The deserialized, validated token.</returns>
		/// <exception cref="ProtocolException">Thrown if the access token is expired, invalid, or from an untrusted authorization server.</exception>
		AccessToken IAccessTokenAnalyzer.DeserializeAccessToken(IDirectedProtocolMessage message, string accessToken) {
			Requires.NotNull(message, "message");
			Requires.NotNullOrEmpty(accessToken, "accessToken");
			Contract.Ensures(Contract.Result<AccessToken>() != null);
			throw new NotImplementedException();
		}
	}
}
