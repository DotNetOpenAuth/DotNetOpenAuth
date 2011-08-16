//-----------------------------------------------------------------------
// <copyright file="IAccessTokenAnalyzer.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
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
		/// <param name="accessToken">The access token.</param>
		/// <param name="user">The user whose data is accessible with this access token.</param>
		/// <param name="scope">The scope of access authorized by this access token.</param>
		/// <returns>A value indicating whether this access token is valid.</returns>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#", Justification = "Try pattern")]
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#", Justification = "Try pattern")]
		bool TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out HashSet<string> scope);
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
		/// <param name="accessToken">The access token.</param>
		/// <param name="user">The user whose data is accessible with this access token.</param>
		/// <param name="scope">The scope of access authorized by this access token.</param>
		/// <returns>
		/// A value indicating whether this access token is valid.
		/// </returns>
		bool IAccessTokenAnalyzer.TryValidateAccessToken(IDirectedProtocolMessage message, string accessToken, out string user, out HashSet<string> scope) {
			Contract.Requires<ArgumentNullException>(message != null);
			Contract.Requires<ArgumentException>(!String.IsNullOrEmpty(accessToken));
			Contract.Ensures(Contract.Result<bool>() == (Contract.ValueAtReturn<string>(out user) != null));

			throw new NotImplementedException();
		}
	}
}
