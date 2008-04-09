using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId;
using DotNetOpenId.RelyingParty;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DotNetOpenId.Provider {
	[DebuggerDisplay("OpenId: {Protocol.Version}")]
	internal class EncodableResponse : MarshalByRefObject, IEncodable {
		public static EncodableResponse PrepareDirectMessage(Protocol protocol) {
			EncodableResponse response = new EncodableResponse(protocol);
			if (protocol.QueryDeclaredNamespaceVersion != null)
				response.Fields.Add(protocol.openid.ns, protocol.QueryDeclaredNamespaceVersion);
			return response;
		}
		public static EncodableResponse PrepareIndirectMessage(Protocol protocol, Uri baseRedirectUrl, string preferredAssociationHandle) {
			EncodableResponse response = new EncodableResponse(protocol, baseRedirectUrl, preferredAssociationHandle);
			if (protocol.QueryDeclaredNamespaceVersion != null)
				response.Fields.Add(protocol.openidnp.ns, protocol.QueryDeclaredNamespaceVersion);
			return response;
		}
		EncodableResponse(Protocol protocol) {
			if (protocol == null) throw new ArgumentNullException("protocol");
			Signed = new List<string>();
			Fields = new Dictionary<string, string>();
			Protocol = protocol;
		}
		EncodableResponse(Protocol protocol, Uri baseRedirectUrl, string preferredAssociationHandle)
			: this(protocol) {
			if (baseRedirectUrl == null) throw new ArgumentNullException("baseRedirectUrl");
			RedirectUrl = baseRedirectUrl;
			PreferredAssociationHandle = preferredAssociationHandle;
		}

		public IDictionary<string, string> Fields { get; private set; }
		public List<string> Signed { get; private set; }
		public Protocol Protocol { get; private set; }
		public bool NeedsSigning { get { return Signed.Count > 0; } }
		public string PreferredAssociationHandle { get; private set; }

		#region IEncodable Members

		public EncodingType EncodingType { 
			get { return RedirectUrl != null ? EncodingType.RedirectBrowserUrl : EncodingType.ResponseBody; }
		}

		public IDictionary<string, string> EncodedFields {
			get {
				var nvc = new Dictionary<string, string>();

				foreach (var pair in Fields) {
					if (EncodingType == EncodingType.RedirectBrowserUrl) {
						nvc.Add(Protocol.openid.Prefix + pair.Key, pair.Value);
					} else {
						nvc.Add(pair.Key, pair.Value);
					}
				}

				return nvc;
			}
		}
		
		public Uri RedirectUrl { get; set; }

		#endregion

		public override string ToString() {
			string returnString = String.Format(CultureInfo.CurrentUICulture,
				"Response.NeedsSigning = {0}", this.NeedsSigning);
			foreach (string key in Fields.Keys) {
				returnString += Environment.NewLine + String.Format(CultureInfo.CurrentUICulture,
					"ResponseField[{0}] = '{1}'", key, Fields[key]);
			}
			return returnString;
		}
	}
}
