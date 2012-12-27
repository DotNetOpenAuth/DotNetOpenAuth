//-----------------------------------------------------------------------
// <copyright file="OpenIdXrdsHelper.cs" company="Outercurve Foundation">
//     Copyright (c) Outercurve Foundation. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.OpenId {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using DotNetOpenAuth.Xrds;
	using Validation;

	/// <summary>
	/// Utility methods for working with XRDS documents.
	/// </summary>
	internal static class OpenIdXrdsHelper {
		/// <summary>
		/// Finds the Relying Party return_to receiving endpoints.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of Relying Party descriptors for the return_to endpoints.</returns>
		/// <remarks>
		/// This is useful for Providers to send unsolicited assertions to Relying Parties,
		/// or for Provider's to perform RP discovery/verification as part of authentication.
		/// </remarks>
		internal static IEnumerable<RelyingPartyEndpointDescription> FindRelyingPartyReceivingEndpoints(this XrdsDocument xrds) {
			Requires.NotNull(xrds, "xrds");

			return from service in xrds.FindReturnToServices()
				   from uri in service.UriElements
				   select new RelyingPartyEndpointDescription(uri.Uri, service.TypeElementUris);
		}

		/// <summary>
		/// Finds the icons the relying party wants an OP to display as part of authentication,
		/// per the UI extension spec.
		/// </summary>
		/// <param name="xrds">The XrdsDocument to search.</param>
		/// <returns>A sequence of the icon URLs in preferred order.</returns>
		internal static IEnumerable<Uri> FindRelyingPartyIcons(this XrdsDocument xrds) {
			Requires.NotNull(xrds, "xrds");

			return from xrd in xrds.XrdElements
				   from service in xrd.OpenIdRelyingPartyIcons
				   from uri in service.UriElements
				   select uri.Uri;
		}

		/// <summary>
		/// Enumerates the XRDS service elements that describe OpenID Relying Party return_to URLs
		/// that can receive authentication assertions.
		/// </summary>
		/// <param name="xrds">The XrdsDocument instance to use in this process.</param>
		/// <returns>A sequence of service elements.</returns>
		private static IEnumerable<ServiceElement> FindReturnToServices(this XrdsDocument xrds) {
			Requires.NotNull(xrds, "xrds");

			return from xrd in xrds.XrdElements
				   from service in xrd.OpenIdRelyingPartyReturnToServices
				   select service;
		}
	}
}
