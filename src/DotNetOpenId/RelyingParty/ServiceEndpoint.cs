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
		public const string OpenId10Namespace ="http://openid.net/xmlns/1.0";
		public const string OpenId12Type = "http://openid.net/signon/1.2";
		public const string OpenId11Type = "http://openid.net/signon/1.1";
		public const string OpenId10Type = "http://openid.net/signon/1.0";

		public static readonly string[] OpenIdTypeUris = { 
			OpenId12Type,
			OpenId11Type,
			OpenId10Type };

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

		string[] typeUris;

		internal ServiceEndpoint(Identifier claimedIdentifier, Uri providerEndpoint, string[] typeUris, Identifier providerLocalIdentifier) {
			if (providerLocalIdentifier == null) throw new ArgumentNullException("providerLocalIdentifier");
			if (providerEndpoint == null) throw new ArgumentNullException("providerEndpoint");
			ClaimedIdentifier = claimedIdentifier;
			ProviderEndpoint = providerEndpoint;
			this.typeUris = typeUris;
			ProviderLocalIdentifier = providerLocalIdentifier;
		}

		internal static ServiceEndpoint Create(Identifier yadisClaimedIdentifier, UriElement uriNode) {
			var typeNodes = uriNode.Service.TypeElements;

			List<string> typeUriList = new List<string>();
			foreach (var t in typeNodes) {
				typeUriList.Add(t.Uri);
			}
			string[] typeUris = typeUriList.ToArray();

			List<string> matchesList = new List<string>();
			foreach (string u in OpenIdTypeUris) {
				foreach (var t in typeNodes) {
					if (u == t.Uri) {
						matchesList.Add(u);
					}
				}
			}

			string[] matches = matchesList.ToArray();

			if ((matches.Length == 0) || (uriNode.Uri == null)) {
				return null; // No matching openid type uris
			}

			return new ServiceEndpoint(
				yadisClaimedIdentifier, uriNode.Uri, typeUris,
				uriNode.Service.ProviderLocalIdentifier ?? yadisClaimedIdentifier);
		}

		static ServiceEndpoint Create(Identifier claimedIdentifier, string html) {
			Uri providerEndpoint = null;
			Identifier providerLocalIdentifier = claimedIdentifier;
			foreach (NameValueCollection values in ByteParser.HeadTagAttrs(html, "link")) {
				switch (values["rel"]) {
					case ProtocolConstants.OpenIdServer:
						providerEndpoint = new Uri(values["href"]);
						break;
					case ProtocolConstants.OpenIdDelegate:
						providerLocalIdentifier = values["href"];
						break;
				}
			}
			if (providerEndpoint == null) {
				return null; // html did not contain openid.server link
			}
			string[] typeUris = { OpenId10Type };
			return new ServiceEndpoint(claimedIdentifier, providerEndpoint, typeUris, providerLocalIdentifier);
		}

		public bool UsesExtension(Uri extensionUri) {
			return Array.IndexOf(typeUris, extensionUri) >= 0;
		}

		public static ServiceEndpoint Discover(Identifier userSuppliedIdentifier) {
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			DiscoveryResult result = Yadis.Yadis.Discover(userSuppliedIdentifier);
			if (result == null)
				return null;

			Identifier claimedIdentifier = new UriIdentifier(result.NormalizedUri);

			if (result.IsXRDS) {
				var xrds = new XrdsDocument(result.ResponseText);

				foreach (var xrd in xrds.XrdElements) {
					foreach (var uri in xrd.ServiceUris) {
						ServiceEndpoint ep = Create(claimedIdentifier, uri);
						if (ep != null) return ep;
					}
				}
			} else {
				ServiceEndpoint ep = Create(claimedIdentifier, result.ResponseText);
				if (ep != null) return ep;
			}

			return null;
		}
	}
}