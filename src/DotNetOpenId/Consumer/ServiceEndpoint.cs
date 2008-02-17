using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Janrain.Yadis;

namespace DotNetOpenId.Consumer {
	public class ServiceEndpoint {
		public static readonly Uri OPENID_1_0_NS = new Uri("http://openid.net/xmlns/1.0");
		public static readonly Uri OPENID_1_2_TYPE = new Uri("http://openid.net/signon/1.2");
		public static readonly Uri OPENID_1_1_TYPE = new Uri("http://openid.net/signon/1.1");
		public static readonly Uri OPENID_1_0_TYPE = new Uri("http://openid.net/signon/1.0");

		public static readonly Uri[] OPENID_TYPE_URIS = { 
			OPENID_1_2_TYPE,
			OPENID_1_1_TYPE,
			OPENID_1_0_TYPE };

		/// <summary>
		/// The URL given as the OpenId URL, which may not be the same as the Provider-issued
		/// OpenId URL.
		/// </summary>
		public Uri IdentityUrl { get; private set; }
		/// <summary>
		/// The OpenId provider URL used for programmatic authentication.
		/// </summary>
		public Uri ServerUrl { get; private set; }
		/// <summary>
		/// The OpenId provider-issued identity URL.
		/// </summary>
		public Uri DelegateUrl { get; private set; }
		public bool UsedYadis { get; private set; }

		Uri[] typeUris;

		/// <summary>
		/// Gets the DelegateUrl if supplied, otherwise the IdentityUrl.
		/// </summary>
		public Uri ServerId {
			get { return DelegateUrl ?? IdentityUrl; }
		}

		internal static Uri ExtractDelegate(ServiceNode serviceNode) {
			XmlNamespaceManager nsmgr = serviceNode.XmlNsManager;
			nsmgr.PushScope();
			nsmgr.AddNamespace("openid", OPENID_1_0_NS.AbsoluteUri);
			XmlNodeList delegateNodes = serviceNode.Node.SelectNodes("./openid:Delegate", nsmgr);
			Uri delegateUrl = null;
			foreach (XmlNode delegateNode in delegateNodes) {
				try {
					delegateUrl = new Uri(delegateNode.InnerXml);
					break;
				} catch (UriFormatException) {
					continue;
				}
			}
			nsmgr.PopScope();
			return delegateUrl;
		}

		internal ServiceEndpoint(Uri identityUrl, Uri serverUrl, Uri[] typeUris, Uri delegateUrl, bool usedYadis) {
			IdentityUrl = identityUrl;
			ServerUrl = serverUrl;
			this.typeUris = typeUris;
			DelegateUrl = delegateUrl;
			UsedYadis = usedYadis;
		}

		internal ServiceEndpoint(Uri yadisUrl, UriNode uriNode) {
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
			IdentityUrl = yadisUrl;
			ServerUrl = uriNode.Uri;
			this.typeUris = typeUris;
			DelegateUrl = ExtractDelegate(serviceNode);
			UsedYadis = true;
		}

		public ServiceEndpoint(Uri uri, string html) {
			object[] objArray = ByteParser.HeadTagAttrs(html, "link");
			foreach (NameValueCollection values in objArray) {
				string text = values["rel"];
				if (text != null) {
					string uriString = values["href"];
					if (uriString != null) {
						if ((text == "openid.server") && (ServerUrl == null)) {
							try {
								ServerUrl = new Uri(uriString);
							} catch (UriFormatException) {
							}
						}
						if ((text == "openid.delegate") && (DelegateUrl == null)) {
							try {
								DelegateUrl = new Uri(uriString);
								continue;
							} catch (UriFormatException) {
								continue;
							}
						}
					}
				}
			}
			if (this.ServerUrl == null) {
				throw new ArgumentException("html did not contain openid.server link");
			}
			IdentityUrl = uri;
			this.typeUris = new Uri[] { OPENID_1_0_TYPE };
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
	}
}