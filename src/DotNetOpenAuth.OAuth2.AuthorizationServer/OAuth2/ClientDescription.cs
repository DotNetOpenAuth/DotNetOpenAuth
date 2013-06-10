//-----------------------------------------------------------------------
// <copyright file="ClientDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OAuth2 {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A default implementation of the <see cref="IClientDescription"/> interface.
	/// </summary>
	public class ClientDescription : IClientDescription {
		/// <summary>
		/// The client's secret, if any.
		/// </summary>
		private readonly string secret;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDescription"/> class
		/// to represent a confidential client (one that has an authenticating secret.)
		/// </summary>
		/// <param name="secret">The secret.</param>
		/// <param name="defaultCallback">The default callback.</param>
		public ClientDescription(string secret, Uri defaultCallback) {
			Requires.NotNullOrEmpty(secret, "secret");
			Requires.NotNull(defaultCallback, "defaultCallback");

			this.secret = secret;
			this.DefaultCallback = defaultCallback;
			this.ClientType = ClientType.Confidential;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDescription"/> class
		/// to represent a public client (one that does not have an authenticating secret.)
		/// </summary>
		/// <param name="defaultCallback">The default callback.</param>
		public ClientDescription(Uri defaultCallback) {
			Requires.NotNull(defaultCallback, "defaultCallback");

			this.DefaultCallback = defaultCallback;
			this.ClientType = ClientType.Public;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDescription"/> class.
		/// </summary>
		protected ClientDescription() {
		}

		#region IClientDescription Members

		/// <summary>
		/// Gets or sets the callback to use when an individual authorization request
		/// does not include an explicit callback URI.
		/// </summary>
		/// <value>
		/// An absolute URL; or <c>null</c> if none is registered.
		/// </value>
		public Uri DefaultCallback { get; protected set; }

		/// <summary>
		/// Gets or sets the type of the client.
		/// </summary>
		public ClientType ClientType { get; protected set; }

		/// <summary>
		/// Gets a value indicating whether a non-empty secret is registered for this client.
		/// </summary>
		public virtual bool HasNonEmptySecret {
			get { return !string.IsNullOrEmpty(this.secret); }
		}

		/// <summary>
		/// Determines whether a callback URI included in a client's authorization request
		/// is among those allowed callbacks for the registered client.
		/// </summary>
		/// <param name="callback">The absolute URI the client has requested the authorization result be received at.  Never null.</param>
		/// <returns>
		///   <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method may be overridden to allow for several callbacks to match.
		/// </remarks>
		public virtual bool IsCallbackAllowed(Uri callback) {
			return EqualityComparer<Uri>.Default.Equals(this.DefaultCallback, callback);
		}

		/// <summary>
		/// Checks whether the specified client secret is correct.
		/// </summary>
		/// <param name="secret">The secret obtained from the client.</param>
		/// <returns><c>true</c> if the secret matches the one in the authorization server's record for the client; <c>false</c> otherwise.</returns>
		/// <remarks>
		/// All string equality checks, whether checking secrets or their hashes,
		/// should be done using <see cref="MessagingUtilities.EqualsConstantTime"/> to mitigate timing attacks.
		/// </remarks>
		public virtual bool IsValidClientSecret(string secret) {
			Requires.NotNullOrEmpty(secret, "secret");

			return MessagingUtilities.EqualsConstantTime(secret, this.secret);
		}

		#endregion
	}
}
