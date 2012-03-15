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

	/// <summary>
	/// A default implementation of the <see cref="IClientDescription"/> interface.
	/// </summary>
	public class ClientDescription : IClientDescription {
		/// <summary>
		/// A delegate that determines whether the callback is allowed.
		/// </summary>
		private readonly Func<Uri, bool> isCallbackAllowed;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientDescription"/> class.
		/// </summary>
		/// <param name="secret">The secret.</param>
		/// <param name="defaultCallback">The default callback.</param>
		/// <param name="clientType">Type of the client.</param>
		/// <param name="isCallbackAllowed">A delegate that determines whether the callback is allowed.</param>
		public ClientDescription(string secret, Uri defaultCallback, ClientType clientType, Func<Uri, bool> isCallbackAllowed = null) {
			this.Secret = secret;
			this.DefaultCallback = defaultCallback;
			this.ClientType = clientType;
			this.isCallbackAllowed = isCallbackAllowed;
		}

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		public string Secret { get; private set; }

		/// <summary>
		/// Gets the callback to use when an individual authorization request
		/// does not include an explicit callback URI.
		/// </summary>
		/// <value>
		/// An absolute URL; or <c>null</c> if none is registered.
		/// </value>
		public Uri DefaultCallback { get; private set; }

		/// <summary>
		/// Gets the type of the client.
		/// </summary>
		public ClientType ClientType { get; private set; }

		/// <summary>
		/// Determines whether a callback URI included in a client's authorization request
		/// is among those allowed callbacks for the registered client.
		/// </summary>
		/// <param name="callback">The absolute URI the client has requested the authorization result be received at.</param>
		/// <returns>
		///   <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
		/// </returns>
		public bool IsCallbackAllowed(Uri callback) {
			if (this.isCallbackAllowed != null) {
				return this.isCallbackAllowed(callback);
			}

			return EqualityComparer<Uri>.Default.Equals(this.DefaultCallback, callback);
		}
	}
}
