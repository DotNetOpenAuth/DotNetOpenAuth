//-----------------------------------------------------------------------
// <copyright file="RelyingPartyEndpointDescription.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// A description of some OpenID Relying Party endpoint.
	/// </summary>
	/// <remarks>
	/// This is an immutable type.
	/// </remarks>
	internal class RelyingPartyEndpointDescription {
		/// <summary>
		/// Initializes a new instance of the <see cref="RelyingPartyEndpointDescription"/> class.
		/// </summary>
		/// <param name="returnTo">The return to.</param>
		/// <param name="supportedServiceTypeUris">
		/// The Type URIs of supported services advertised on a relying party's XRDS document.
		/// </param>
		internal RelyingPartyEndpointDescription(Uri returnTo, string[] supportedServiceTypeUris) {
			Requires.NotNull(returnTo, "returnTo");
			Requires.NotNull(supportedServiceTypeUris, "supportedServiceTypeUris");

			this.ReturnToEndpoint = returnTo;
			this.Protocol = GetProtocolFromServices(supportedServiceTypeUris);
		}

		/// <summary>
		/// Gets the URL to the login page on the discovered relying party web site.
		/// </summary>
		public Uri ReturnToEndpoint { get; private set; }

		/// <summary>
		/// Gets the OpenId protocol that the discovered relying party supports.
		/// </summary>
		public Protocol Protocol { get; private set; }

		/// <summary>
		/// Derives the highest OpenID protocol that this library and the OpenID Provider have
		/// in common.
		/// </summary>
		/// <param name="supportedServiceTypeUris">The supported service type URIs.</param>
		/// <returns>The best OpenID protocol version to use when communicating with this Provider.</returns>
		[SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "OpenID", Justification = "Spelling correct")]
		private static Protocol GetProtocolFromServices(string[] supportedServiceTypeUris) {
			Protocol protocol = Protocol.FindBestVersion(p => p.RPReturnToTypeURI, supportedServiceTypeUris);
			if (protocol == null) {
				throw new InvalidOperationException("Unable to determine the version of OpenID the Relying Party supports.");
			}
			return protocol;
		}
	}
}
