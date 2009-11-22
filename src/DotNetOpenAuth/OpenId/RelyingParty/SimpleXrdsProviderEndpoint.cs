//-----------------------------------------------------------------------
// <copyright file="SimpleXrdsProviderEndpoint.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId.RelyingParty {
	using System;
	using DotNetOpenAuth.OpenId.Messages;
using System.Collections.ObjectModel;
	using DotNetOpenAuth.Messaging;

	/// <summary>
	/// A very simple IXrdsProviderEndpoint implementation for verifying that all positive
	/// assertions (particularly unsolicited ones) are received from OP endpoints that
	/// are deemed permissible by the host RP.
	/// </summary>
	internal class SimpleXrdsProviderEndpoint : IXrdsProviderEndpoint {
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleXrdsProviderEndpoint"/> class.
		/// </summary>
		/// <param name="positiveAssertion">The positive assertion.</param>
		internal SimpleXrdsProviderEndpoint(PositiveAssertionResponse positiveAssertion) {
			this.Uri = positiveAssertion.ProviderEndpoint;
			this.Version = positiveAssertion.Version;
			this.Capabilities = new ReadOnlyCollection<string>(EmptyList<string>.Instance);
		}

		#region IXrdsProviderEndpoint Properties

		/// <summary>
		/// Gets the priority associated with this service that may have been given
		/// in the XRDS document.
		/// </summary>
		public int? ServicePriority {
			get { return null; }
		}

		/// <summary>
		/// Gets the priority associated with the service endpoint URL.
		/// </summary>
		/// <remarks>
		/// When sorting by priority, this property should be considered second after
		/// <see cref="ServicePriority"/>.
		/// </remarks>
		public int? UriPriority {
			get { return null; }
		}

		#endregion

		#region IProviderEndpoint Members

		/// <summary>
		/// Gets the detected version of OpenID implemented by the Provider.
		/// </summary>
		public Version Version { get; private set; }

		/// <summary>
		/// Gets the URL that the OpenID Provider receives authentication requests at.
		/// </summary>
		public Uri Uri { get; private set; }

		/// <summary>
		/// Gets the collection of service type URIs found in the XRDS document describing this Provider.
		/// </summary>
		/// <value>Should never be null, but may be empty.</value>
		public ReadOnlyCollection<string> Capabilities { get; private set; }

		#endregion
	}
}
