using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Janrain.Yadis;
using System.Xml.XPath;
using System.IO;
using DotNetOpenId.Yadis;

namespace DotNetOpenId.RelyingParty {
	/// <summary>
	/// Represents information discovered about a user-supplied Identifier.
	/// </summary>
	internal class ServiceEndpoint {
		public const string OpenId10Namespace = "http://openid.net/xmlns/1.0";
		public const string OpenId12Type = "http://openid.net/signon/1.2";
		public const string OpenId11Type = "http://openid.net/signon/1.1";
		public const string OpenId10Type = "http://openid.net/signon/1.0";
		/// <summary>
		/// The XRD/Service/Type value discovered in an XRDS document when
		/// "discovering" on a Claimed Identifier (http://andrewarnott.yahoo.com)
		/// </summary>
		public const string OpenId20Type = "http://specs.openid.net/auth/2.0/signon";

		public static readonly string[] OpenIdClaimedIdentifierTypeUris = { 
			OpenId12Type,
			OpenId11Type,
			OpenId10Type,
			OpenId20Type };

		/// <summary>
		/// The XRD/Service/Type value discovered in an XRDS document when
		/// "discovering" on an OP Identifier rather than a Claimed Identifier.
		/// (http://yahoo.com)
		/// </summary>
		public const string OPIdentifierServiceTypeUri = "http://specs.openid.net/auth/2.0/server";

		public static readonly string[] OpenIdProviderIdentifierTypeUris = {
			OPIdentifierServiceTypeUri,
			};

		/// <summary>
		/// Used as the Claimed Identifier and the OP Local Identifier when
		/// the User Supplied Identifier is an OP Identifier.
		/// </summary>
		public const string ClaimedIdentifierForOPIdentifier = "http://specs.openid.net/auth/2.0/identifier_select";

		/// <summary>
		/// The URL which accepts OpenID Authentication protocol messages.
		/// </summary>
		/// <remarks>
		/// Obtained by performing discovery on the User-Supplied Identifier. 
		/// This value MUST be an absolute HTTP or HTTPS URL.
		/// </remarks>
		public Uri ProviderEndpoint { get; private set; }
		/// <summary>
		/// An Identifier for an OpenID Provider.
		/// </summary>
		public Identifier ProviderIdentifier { get; private set; }
		/// <summary>
		/// An Identifier that was presented by the end user to the Relying Party, 
		/// or selected by the user at the OpenID Provider. 
		/// During the initiation phase of the protocol, an end user may enter 
		/// either their own Identifier or an OP Identifier. If an OP Identifier 
		/// is used, the OP may then assist the end user in selecting an Identifier 
		/// to share with the Relying Party.
		/// </summary>
		public Identifier UserSuppliedIdentifier { get; private set; }
		/// <summary>
		/// The Identifier that the end user claims to own.
		/// </summary>
		public Identifier ClaimedIdentifier { get; private set; }
		/// <summary>
		/// An alternate Identifier for an end user that is local to a 
		/// particular OP and thus not necessarily under the end user's 
		/// control.
		/// </summary>
		public Identifier ProviderLocalIdentifier { get; private set; }
		/// <summary>
		/// Gets the list of services available at this OP Endpoint for the
		/// claimed Identifier.
		/// </summary>
		public string[] ProviderSupportedServiceTypeUris { get; private set; }

		internal ServiceEndpoint(Identifier claimedIdentifier, Uri providerEndpoint, Identifier providerLocalIdentifier) {
			if (claimedIdentifier == null) throw new ArgumentNullException("claimedIdentifier");
			if (providerEndpoint == null) throw new ArgumentNullException("providerEndpoint");
			ClaimedIdentifier = claimedIdentifier;
			ProviderEndpoint = providerEndpoint;
			ProviderLocalIdentifier = providerLocalIdentifier ?? claimedIdentifier;
		}

		internal static ServiceEndpoint Create(Identifier yadisClaimedIdentifier, UriElement serviceTypeUri) {
			return new ServiceEndpoint(
				yadisClaimedIdentifier, serviceTypeUri.Uri,
				serviceTypeUri.Service.ProviderLocalIdentifier ?? yadisClaimedIdentifier);
		}

		public bool UsesExtension(string extensionUri) {
			return Array.IndexOf(ProviderSupportedServiceTypeUris, extensionUri) >= 0;
		}

		/// <summary>
		/// Finds the first Service described in an XrdsDocument that supports
		/// an OpenID protocol supported by this library.
		/// </summary>
		/// <returns>
		/// The service type URI that matches one of the supported ones in this library.
		/// This type URI can be connected to the supporting service.
		/// </returns>
		static UriElement findCompatibleService(XrdsDocument xrds) {
			// Scan the XRDS document for services compatible with OpenID.
			foreach (var xrd in xrds.XrdElements) {
				foreach (var uri in xrd.ServiceUris) {
					// See if this particular service type URI is one of the
					// supported OpenID protocols in this library.
					if (Array.IndexOf(OpenIdClaimedIdentifierTypeUris, uri.Uri) >= 0) {
						return uri;
					}
				}
			}
			return null;
		}
	}
}