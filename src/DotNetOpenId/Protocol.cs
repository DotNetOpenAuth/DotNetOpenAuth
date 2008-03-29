using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {
	public enum ProtocolVersion {
		V11,
		V20
	}

	/// <summary>
	/// Tracks the several versions of OpenID this library supports and the unique
	/// constants to each version used in the protocol.
	/// </summary>
	internal partial class Protocol {
		Protocol(QueryParameters queryBits) {
			openidnp = queryBits;
			openid = new QueryParameters(queryBits);
		}

		// Well-known, supported versions of the OpenID spec.
		public static readonly Protocol v10 = new Protocol(new QueryParameters()) {
			Version = new Version(1, 0),
			XmlNamespace = "http://openid.net/xmlns/1.0",
			QueryDeclaredNamespaceVersion = null,
			ClaimedIdentifierServiceTypeURI = "http://openid.net/signon/1.0",
			OPIdentifierServiceTypeURI = null, // not supported
			ClaimedIdentifierForOPIdentifier = null, // not supported
			HtmlDiscoveryProviderKey = "openid.server",
			HtmlDiscoveryLocalIdKey = "openid.delegate",
		};
		public static readonly Protocol v11 = new Protocol(new QueryParameters()) {
			Version = new Version(1, 1),
			XmlNamespace = "http://openid.net/xmlns/1.0",
			QueryDeclaredNamespaceVersion = null,
			ClaimedIdentifierServiceTypeURI = "http://openid.net/signon/1.1",
			OPIdentifierServiceTypeURI = null, // not supported
			ClaimedIdentifierForOPIdentifier = null, // not supported
			HtmlDiscoveryProviderKey = "openid.server",
			HtmlDiscoveryLocalIdKey = "openid.delegate",
		};
		public static readonly Protocol v20 = new Protocol(new QueryParameters() {
			Realm = "realm",
			op_endpoint = "op_endpoint",
			response_nonce = "response_nonce",
			error_code = "error_code",
			user_setup_url = null,
		}) {
			Version = new Version(2, 0),
			XmlNamespace = null, // no longer applicable
			QueryDeclaredNamespaceVersion = "http://specs.openid.net/auth/2.0",
			ClaimedIdentifierServiceTypeURI = "http://specs.openid.net/auth/2.0/signon",
			OPIdentifierServiceTypeURI = "http://specs.openid.net/auth/2.0/server",
			ClaimedIdentifierForOPIdentifier = "http://specs.openid.net/auth/2.0/identifier_select",
			HtmlDiscoveryProviderKey = "openid2.provider",
			HtmlDiscoveryLocalIdKey = "openid2.local_id",
			Args = new QueryArguments() {
				SessionType = new QueryArguments.SessionTypes() {
					NoEncryption = "no-encryption",
					DH_SHA256 = "DH-SHA256",
				},
				SignatureAlgorithm = new QueryArguments.SignatureAlgorithms() {
					HMAC_SHA256 = "HMAC-SHA256",
				},
				Mode = new QueryArguments.Modes() {
					setup_needed = "setup_needed",
				},
			},
		};
		/// <summary>
		/// A list of all supported OpenID versions, in order starting from newest version.
		/// </summary>
		public readonly static List<Protocol> AllVersions = new List<Protocol>() { v20, v11, v10 };
		/// <summary>
		/// The default (or most recent) supported version of the OpenID protocol.
		/// </summary>
		public readonly static Protocol Default = AllVersions[0];
		public static Protocol Lookup(Version version) {
			foreach (Protocol protocol in AllVersions) {
				if (protocol.Version == version) return protocol;
			}
			throw new ArgumentOutOfRangeException("version");
		}
		/// <summary>
		/// Attempts to detect the right OpenID protocol version based on the contents
		/// of an incoming query string.
		/// </summary>
		internal static Protocol Detect(IDictionary<string, string> Query) {
			if (Query == null) throw new ArgumentNullException("Query");
			return Query.ContainsKey(v20.openid.ns) ? v20 : v11;
		}

		/// <summary>
		/// The OpenID version that this <see cref="Protocol"/> instance describes.
		/// </summary>
		public Version Version;
		/// <summary>
		/// The namespace of OpenId 1.x elements in XRDS documents.
		/// </summary>
		public string XmlNamespace;
		/// <summary>
		/// The value of the openid.ns parameter that appears on the query string
		/// whenever data is passed between relying party and provider for OpenID 2.0
		/// and later.
		/// </summary>
		public string QueryDeclaredNamespaceVersion;
		/// <summary>
		/// The XRD/Service/Type value discovered in an XRDS document when
		/// "discovering" on a Claimed Identifier (http://andrewarnott.yahoo.com)
		/// </summary>
		public string ClaimedIdentifierServiceTypeURI;
		/// <summary>
		/// The XRD/Service/Type value discovered in an XRDS document when
		/// "discovering" on an OP Identifier rather than a Claimed Identifier.
		/// (http://yahoo.com)
		/// </summary>
		public string OPIdentifierServiceTypeURI;
		/// <summary>
		/// Used as the Claimed Identifier and the OP Local Identifier when
		/// the User Supplied Identifier is an OP Identifier.
		/// </summary>
		public string ClaimedIdentifierForOPIdentifier;
		/// <summary>
		/// The value of the 'rel' attribute in an HTML document's LINK tag
		/// when the same LINK tag's HREF attribute value contains the URL to an
		/// OP Endpoint URL.
		/// </summary>
		public string HtmlDiscoveryProviderKey;
		/// <summary>
		/// The value of the 'rel' attribute in an HTML document's LINK tag
		/// when the same LINK tag's HREF attribute value contains the URL to use
		/// as the OP Local Identifier.
		/// </summary>
		public string HtmlDiscoveryLocalIdKey;
		/// <summary>
		/// Parts of the protocol that define parameter names that appear in the 
		/// query string.  Each parameter name is prefixed with 'openid.'.
		/// </summary>
		public readonly QueryParameters openid;
		/// <summary>
		/// Parts of the protocol that define parameter names that appear in the 
		/// query string.  Each parameter name is NOT prefixed with 'openid.'.
		/// </summary>
		public readonly QueryParameters openidnp;
		/// <summary>
		/// The various 'constants' that appear as parameter arguments (values).
		/// </summary>
		public QueryArguments Args = new QueryArguments();

		internal class QueryParameters {
			public string Prefix = "openid.";
			public QueryParameters() { }
			public QueryParameters(QueryParameters addPrefixTo) {
				ns = addPrefix(addPrefixTo.ns);
				return_to = addPrefix(addPrefixTo.return_to);
				Realm = addPrefix(addPrefixTo.Realm);
				mode = addPrefix(addPrefixTo.mode);
				error = addPrefix(addPrefixTo.error);
				error_code = addPrefix(addPrefixTo.error_code);
				identity = addPrefix(addPrefixTo.identity);
				op_endpoint = addPrefix(addPrefixTo.op_endpoint);
				response_nonce = addPrefix(addPrefixTo.response_nonce);
				claimed_id = addPrefix(addPrefixTo.claimed_id);
				expires_in = addPrefix(addPrefixTo.expires_in);
				assoc_type = addPrefix(addPrefixTo.assoc_type);
				assoc_handle = addPrefix(addPrefixTo.assoc_handle);
				session_type = addPrefix(addPrefixTo.session_type);
				is_valid = addPrefix(addPrefixTo.is_valid);
				sig = addPrefix(addPrefixTo.sig);
				signed = addPrefix(addPrefixTo.signed);
				user_setup_url = addPrefix(addPrefixTo.user_setup_url);
				invalidate_handle = addPrefix(addPrefixTo.invalidate_handle);
				dh_modulus = addPrefix(addPrefixTo.dh_modulus);
				dh_gen = addPrefix(addPrefixTo.dh_gen);
				dh_consumer_public = addPrefix(addPrefixTo.dh_consumer_public);
				dh_server_public = addPrefix(addPrefixTo.dh_server_public);
				enc_mac_key = addPrefix(addPrefixTo.enc_mac_key);
				mac_key = addPrefix(addPrefixTo.mac_key);
			}
			string addPrefix(string original) {
				return (original != null) ? Prefix + original : null;
			}
			// These fields default to 1.x specifications, and are overridden
			// as necessary by later versions in the Protocol class initializers.
			// Null values in any version suggests that that feature is absent from that version.
			public string ns = "ns";
			public string return_to = "return_to";
			public string Realm = "trust_root";
			public string mode = "mode";
			public string error = "error";
			public string error_code = null;
			public string identity = "identity";
			public string op_endpoint = null;
			public string response_nonce = null;
			public string claimed_id = "claimed_id";
			public string expires_in = "expires_in";
			public string assoc_type = "assoc_type";
			public string assoc_handle = "assoc_handle";
			public string session_type = "session_type";
			public string is_valid = "is_valid";
			public string sig = "sig";
			public string signed = "signed";
			public string user_setup_url = "user_setup_url";
			public string invalidate_handle = "invalidate_handle";
			public string dh_modulus = "dh_modulus";
			public string dh_gen = "dh_gen";
			public string dh_consumer_public = "dh_consumer_public";
			public string dh_server_public = "dh_server_public";
			public string enc_mac_key = "enc_mac_key";
			public string mac_key = "mac_key";
		}
		internal class QueryArguments {
			public ErrorCodes ErrorCode = new ErrorCodes();
			public SessionTypes SessionType = new SessionTypes();
			public SignatureAlgorithms SignatureAlgorithm = new SignatureAlgorithms();
			public Modes Mode = new Modes();
			public IsValidValues IsValid = new IsValidValues();

			internal class ErrorCodes {
				public string UnsupportedType = "unsupported-type";
			}
			internal class SessionTypes {
				/// <summary>
				/// A preference order list of all supported session types.
				/// </summary>
				public string[] All { get { return new[] { DH_SHA256, DH_SHA1, NoEncryption }; } }
				public string[] AllDiffieHellman { get { return new[] { DH_SHA256, DH_SHA1 }; } }
				public string DH_SHA1 = "DH-SHA1";
				public string DH_SHA256 = null;
				public string NoEncryption = "";
			}
			internal class SignatureAlgorithms {
				/// <summary>
				/// A preference order list of signature algorithms we support.
				/// </summary>
				public string[] All { get { return new [] { HMAC_SHA256, HMAC_SHA1 }; } }
				public string HMAC_SHA1 = "HMAC-SHA1";
				public string HMAC_SHA256 = null;
			}
			internal class Modes {
				public string cancel = "cancel";
				public string error = "error";
				public string id_res = "id_res";
				public string checkid_immediate = "checkid_immediate";
				public string checkid_setup = "checkid_setup";
				public string check_authentication = "check_authentication";
				public string associate = "associate";
				public string setup_needed = null;
			}
			internal class IsValidValues {
				public string True = "true";
			}
		}

		/// <summary>
		/// The maximum time a user can be allowed to take to complete authentication.
		/// </summary>
		/// <remarks>
		/// This is used to calculate the length of time that nonces are stored.
		/// This is internal until we can decide whether to leave this static, or make
		/// it an instance member, or put it inside the IConsumerAppliationStore interface.
		/// </remarks>
		internal static TimeSpan MaximumUserAgentAuthenticationTime = TimeSpan.FromMinutes(5);
		/// <summary>
		/// The maximum permissible difference in clocks between relying party and 
		/// provider web servers, discounting time zone differences.
		/// </summary>
		/// <remarks>
		/// This is used when storing/validating nonces from the provider.
		/// If it is conceivable that a server's clock could be up to five minutes
		/// off from true UTC time, then the maximum time skew should be set to 
		/// ten minutes to allow one server to be five minutes ahead and the remote
		/// server to be five minutes behind and still be able to communicate.
		/// </remarks>
		internal static TimeSpan MaximumAllowableTimeSkew = TimeSpan.FromMinutes(10);

		public override bool Equals(object obj) {
			Protocol other = obj as Protocol;
			if (other == null) return false;
			return this.Version == other.Version;
		}
		public override int GetHashCode() {
			return Version.GetHashCode();
		}
	}
}
