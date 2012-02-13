//-----------------------------------------------------------------------
// <copyright file="Model.Client.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;

	using DotNetOpenAuth.OAuth2;

	public partial class Client : IClientDescription {
		#region IConsumerDescription Members

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string IClientDescription.Secret {
			get { return this.ClientSecret; }
		}

		/// <summary>
		/// Gets the callback to use when an individual authorization request
		/// does not include an explicit callback URI.
		/// </summary>
		/// <value>
		/// An absolute URL; or <c>null</c> if none is registered.
		/// </value>
		Uri IClientDescription.DefaultCallback {
			get { return string.IsNullOrEmpty(this.CallbackAsString) ? null : new Uri(this.CallbackAsString); }
		}

		/// <summary>
		/// Determines whether a callback URI included in a client's authorization request
		/// is among those allowed callbacks for the registered client.
		/// </summary>
		/// <param name="callback">The absolute URI the client has requested the authorization result be received at.</param>
		/// <returns>
		///   <c>true</c> if the callback URL is allowable for this client; otherwise, <c>false</c>.
		/// </returns>
		bool IClientDescription.IsCallbackAllowed(Uri callback) {
			return string.IsNullOrEmpty(this.CallbackAsString) || callback == new Uri(this.CallbackAsString);
		}

		#endregion
	}
}
