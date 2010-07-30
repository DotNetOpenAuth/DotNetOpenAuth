//-----------------------------------------------------------------------
// <copyright file="Model.Client.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace RelyingPartyLogic {
	using System;
	using System.Collections.Generic;

	using DotNetOpenAuth.OAuth2;

	public partial class Client : IConsumerDescription {
		/// <summary>
		/// Gets the allowed callback URIs that this client has pre-registered with the service provider, if any.
		/// </summary>
		/// <value>
		/// The URIs that user authorization responses may be directed to; must not be <c>null</c>, but may be empty.
		/// </value>
		/// <remarks>
		/// The first element in this list (if any) will be used as the default client redirect URL if the client sends an authorization request without a redirect URL.
		/// If the list is empty, any callback is allowed for this client.
		/// </remarks>
		public List<Uri> AllowedCallbacks {
			get {
				var result = new List<Uri>();
				if (this.CallbackAsString != null) {
					result.Add(new Uri(this.CallbackAsString));
				}

				return result;
			}
		}

		#region IConsumerDescription Members

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string IConsumerDescription.Secret {
			get { return this.ClientSecret; }
		}

		#endregion
	}
}
