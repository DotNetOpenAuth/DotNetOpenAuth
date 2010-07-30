//-----------------------------------------------------------------------
// <copyright file="Client.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace OAuthServiceProvider.Code {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;
	using DotNetOpenAuth.OAuth2;

	public partial class Client : IConsumerDescription {
		#region IConsumerDescription Members

		/// <summary>
		/// Gets the client secret.
		/// </summary>
		string IConsumerDescription.Secret {
			get { return this.ClientSecret; }
		}

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
		List<Uri> IConsumerDescription.AllowedCallbacks {
			get { return string.IsNullOrEmpty(this.Callback) ? new List<Uri>() : new List<Uri>(new Uri[] { new Uri(this.Callback) }); }
		}

		#endregion
	}
}