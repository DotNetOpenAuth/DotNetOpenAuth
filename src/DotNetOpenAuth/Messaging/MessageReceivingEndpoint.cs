//-----------------------------------------------------------------------
// <copyright file="MessageReceivingEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Messaging {
	using System;
	using System.Diagnostics;

	/// <summary>
	/// An immutable description of a URL that receives messages.
	/// </summary>
	[DebuggerDisplay("{AllowedMethods} {Location}")]
	public class MessageReceivingEndpoint {
		/// <summary>
		/// Initializes a new instance of the <see cref="MessageReceivingEndpoint"/> class.
		/// </summary>
		/// <param name="locationUri">The URL of this endpoint.</param>
		/// <param name="method">The HTTP method(s) allowed.</param>
		public MessageReceivingEndpoint(string locationUri, HttpDeliveryMethods method)
			: this(new Uri(locationUri), method) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="MessageReceivingEndpoint"/> class.
		/// </summary>
		/// <param name="location">The URL of this endpoint.</param>
		/// <param name="method">The HTTP method(s) allowed.</param>
		public MessageReceivingEndpoint(Uri location, HttpDeliveryMethods method) {
			ErrorUtilities.VerifyArgumentNotNull(location, "location");
			ErrorUtilities.VerifyArgumentInRange(method != HttpDeliveryMethods.None, "method");
			ErrorUtilities.VerifyArgumentInRange((method & (HttpDeliveryMethods.PostRequest | HttpDeliveryMethods.GetRequest)) != 0, "method", MessagingStrings.GetOrPostFlagsRequired);

			this.Location = location;
			this.AllowedMethods = method;
		}

		/// <summary>
		/// Gets the URL of this endpoint.
		/// </summary>
		public Uri Location { get; private set; }

		/// <summary>
		/// Gets the HTTP method(s) allowed.
		/// </summary>
		public HttpDeliveryMethods AllowedMethods { get; private set; }
	}
}
