//-----------------------------------------------------------------------
// <copyright file="IClientDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A description of a client from an Authorization Server's point of view.
	/// </summary>
	public interface IClientDescription {
		/// <summary>
		/// Gets the callback to use when an individual authorization request
		/// does not include an explicit callback URI.
		/// </summary>
		/// <value>An absolute URL; or <c>null</c> if none is registered.</value>
		Uri DefaultCallback { get; }

		/// <summary>
		/// Gets the type of the client.
		/// </summary>
		ClientType ClientType { get; }

		/// <summary>
		/// Gets a value indicating whether a non-empty secret is registered for this client.
		/// </summary>
		bool HasNonEmptySecret { get; }

		/// <summary>
		/// Determines whether a callback URI included in a client's authorization request 
		/// is among those allowed callbacks for the registered client.
		/// </summary>
		/// <param name="callback">The absolute URI the client has requested the authorization result be received at.  Never null.</param>
		/// <returns>
		/// <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// At the point this method is invoked, the identity of the client has <em>not</em>
		/// been confirmed.  To avoid open redirector attacks, the alleged client's identity
		/// is used to lookup a list of allowable callback URLs to make sure that the callback URL
		/// the actual client is requesting is one of the expected ones.
		/// </para>
		/// <para>
		/// From OAuth 2.0 section 2.1: 
		/// The authorization server SHOULD require the client to pre-register
		/// their redirection URI or at least certain components such as the
		/// scheme, host, port and path.  If a redirection URI was registered,
		/// the authorization server MUST compare any redirection URI received at
		/// the authorization endpoint with the registered URI.
		/// </para>
		/// </remarks>
		bool IsCallbackAllowed(Uri callback);

		/// <summary>
		/// Checks whether the specified client secret is correct.
		/// </summary>
		/// <param name="secret">The secret obtained from the client.</param>
		/// <returns><c>true</c> if the secret matches the one in the authorization server's record for the client; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// All string equality checks, whether checking secrets or their hashes,
		/// should be done using <see cref="MessagingUtilities.EqualsConstantTime"/> to mitigate timing attacks.
		/// </remarks>
		bool IsValidClientSecret(string secret);
	}
}
