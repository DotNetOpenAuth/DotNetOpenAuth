//-----------------------------------------------------------------------
// <copyright file="ServiceProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOAuth {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOAuth.Messaging;

	/// <summary>
	/// A description of an individual endpoint on a Service Provider.
	/// </summary>
	public class ServiceProviderEndpoint {
		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderEndpoint"/> class.
		/// </summary>
		/// <param name="locationUri">The URL of this Service Provider endpoint.</param>
		/// <param name="method">The HTTP method(s) allowed.</param>
		public ServiceProviderEndpoint(string locationUri, HttpDeliveryMethod method)
			: this(new Uri(locationUri), method) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ServiceProviderEndpoint"/> class.
		/// </summary>
		/// <param name="location">The URL of this Service Provider endpoint.</param>
		/// <param name="method">The HTTP method(s) allowed.</param>
		public ServiceProviderEndpoint(Uri location, HttpDeliveryMethod method) {
			if (location == null) {
				throw new ArgumentNullException("location");
			}
			if (method == HttpDeliveryMethod.None) {
				throw new ArgumentOutOfRangeException("method");
			}

			this.Location = location;
			this.AllowedMethods = method;
		}

		/// <summary>
		/// Gets or sets the URL of this Service Provider endpoint.
		/// </summary>
		public Uri Location { get; set; }

		/// <summary>
		/// Gets or sets the HTTP method(s) allowed.
		/// </summary>
		public HttpDeliveryMethod AllowedMethods { get; set; }
	}
}
