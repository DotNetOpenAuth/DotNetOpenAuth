using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Janrain.Yadis;

namespace DotNetOpenId.RelyingParty {
	internal class ServiceEndpoint {
		public static readonly Uri OPENID_1_0_NS = new Uri("http://openid.net/xmlns/1.0");
		public static readonly Uri OPENID_1_2_TYPE = new Uri("http://openid.net/signon/1.2");
		public static readonly Uri OPENID_1_1_TYPE = new Uri("http://openid.net/signon/1.1");
		public static readonly Uri OPENID_1_0_TYPE = new Uri("http://openid.net/signon/1.0");

		public static readonly Uri[] OPENID_TYPE_URIS = { 
			OPENID_1_2_TYPE,
			OPENID_1_1_TYPE,
			OPENID_1_0_TYPE };

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
		public bool UsedYadis { get; private set; }

		Uri[] typeUris;

		internal static Identifier ExtractDelegate(ServiceNode serviceNode) {
			XmlNamespaceManager nsmgr = serviceNode.XmlNsManager;
			nsmgr.PushScope();
			nsmgr.AddNamespace("openid", OPENID_1_0_NS.AbsoluteUri);
			XmlNodeList delegateNodes = serviceNode.Node.SelectNodes("./openid:Delegate", nsmgr);
			Identifier providerLocalIdentifier = null;
			foreach (XmlNode delegateNode in delegateNodes) {
				try {
					providerLocalIdentifier = Identifier.Parse(delegateNode.InnerXml);
					break;
				} catch (UriFormatException) {
					continue;
				}
			}
			nsmgr.PopScope();
			return providerLocalIdentifier;
		}

		internal ServiceEndpoint(Identifier claimedIdentifier, Uri providerEndpoint, Uri[] typeUris, Identifier providerLocalIdentifier, bool usedYadis) {
			ClaimedIdentifier = claimedIdentifier;
			ProviderEndpoint = providerEndpoint;
			this.typeUris = typeUris;
			ProviderLocalIdentifier = providerLocalIdentifier;
			UsedYadis = usedYadis;
		}

		internal ServiceEndpoint(Identifier yadisClaimedIdentifier, UriNode uriNode) {
			ServiceNode serviceNode = uriNode.ServiceNode;

			TypeNode[] typeNodes = serviceNode.TypeNodes();

			List<Uri> typeUriList = new List<Uri>();
			foreach (TypeNode t in typeNodes) {
				typeUriList.Add(t.Uri);
			}
			Uri[] typeUris = typeUriList.ToArray();

			List<Uri> matchesList = new List<Uri>();
			foreach (Uri u in OPENID_TYPE_URIS) {
				foreach (TypeNode t in typeNodes) {
					if (u == t.Uri) {
						matchesList.Add(u);
					}
				}
			}

			Uri[] matches = matchesList.ToArray();

			if ((matches.Length == 0) || (uriNode.Uri == null)) {
				throw new ArgumentException("No matching openid type uris");
			}
			ClaimedIdentifier = yadisClaimedIdentifier;
			ProviderEndpoint = uriNode.Uri;
			this.typeUris = typeUris;
			ProviderLocalIdentifier = ExtractDelegate(serviceNode);
			UsedYadis = true;
		}

		public ServiceEndpoint(Identifier claimedIdentifier, string html) {
			ClaimedIdentifier = claimedIdentifier;
			ProviderLocalIdentifier = claimedIdentifier;
			foreach (NameValueCollection values in ByteParser.HeadTagAttrs(html, "link")) {
				switch (values["rel"]) {
					case ProtocolConstants.OpenIdServer:
						ProviderEndpoint = new Uri(values["href"]);
						break;
					case ProtocolConstants.OpenIdDelegate:
						ProviderLocalIdentifier = Identifier.Parse(values["href"]);
						break;
				}
			}
			if (ProviderEndpoint == null) {
				throw new ArgumentException("html did not contain openid.server link");
			}
			typeUris = new Uri[] { OPENID_1_0_TYPE };
			UsedYadis = false;
		}

		public bool UsesExtension(Uri extension_uri) {
			//TODO: I think that all Arrays of stuff could use generics...
			foreach (Uri u in this.typeUris) {
				if (u == extension_uri)
					return true;
			}
			return false;
		}

		public static ServiceEndpoint Discover(Identifier userSuppliedIdentifier) {
			// Fix this to support XRIs.
			if (userSuppliedIdentifier == null) throw new ArgumentNullException("userSuppliedIdentifier");
			UriIdentifier userSuppliedIdentifierUri = userSuppliedIdentifier as UriIdentifier;
			if (userSuppliedIdentifierUri == null) throw new NotSupportedException();

			DiscoveryResult result = Janrain.Yadis.Yadis.Discover(userSuppliedIdentifierUri.Uri);
			if (result == null)
				return null;

			Identifier claimedIdentifier = new UriIdentifier(result.NormalizedUri);

			if (result.IsXRDS) {
				Xrd xrds_node = new Xrd(result.ResponseText);

				foreach (UriNode uri_node in xrds_node.UriNodes()) {
					try {
						return new ServiceEndpoint(claimedIdentifier, uri_node);
					} catch (ArgumentException) { }
				}
			} else {
				try {
					return new ServiceEndpoint(claimedIdentifier, result.ResponseText);
				} catch (ArgumentException) { }
			}

			return null;
		}

	}
}