using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetOpenId {

	internal static class ProtocolConstants {
		internal const string OpenId11Server = "openid.server";
		internal const string OpenId11Delegate = "openid.delegate";
		internal const string OpenId20Provider = "openid2.provider";
		internal const string OpenId20LocalId = "openid2.local_id";
	}

	internal static class QueryStringArgs {
		/// <summary>openid. variables that don't include the "openid." prefix.</summary>
		internal static class openidnp {
			internal const string ns = "ns";
			internal const string return_to = "return_to";
			internal const string mode = "mode";
			internal const string error = "error";
			internal const string identity = "identity";
			internal const string claimed_id = "claimed_id";
			internal const string expires_in = "expires_in";
			internal const string assoc_type = "assoc_type";
			internal const string assoc_handle = "assoc_handle";
			internal const string session_type = "session_type";
			internal const string is_valid = "is_valid";
			internal const string sig = "sig";
			internal const string signed = "signed";
			internal const string user_setup_url = "user_setup_url";
			internal const string trust_root = "trust_root";
			internal const string realm = "realm";
			internal const string invalidate_handle = "invalidate_handle";
			internal const string dh_modulus = "dh_modulus";
			internal const string dh_gen = "dh_gen";
			internal const string dh_consumer_public = "dh_consumer_public";
			internal const string dh_server_public = "dh_server_public";

			internal static class sregnp {
				internal const string policy_url = "policy_url";
				internal const string optional = "optional";
				internal const string required = "required";
				internal const string nickname = "nickname";
				internal const string email = "email";
				internal const string fullname = "fullname";
				internal const string dob = "dob";
				internal const string gender = "gender";
				internal const string postcode = "postcode";
				internal const string country = "country";
				internal const string language = "language";
				internal const string timezone = "timezone";
			}
		}
		/// <summary>openid. variables that include the "openid." prefix.</summary>
		internal static class openid {
			internal const string Prefix = "openid.";

			internal const string ns = Prefix + openidnp.ns;
			internal const string return_to = Prefix + openidnp.return_to;
			internal const string mode = Prefix + openidnp.mode;
			internal const string error = Prefix + openidnp.error;
			internal const string identity = Prefix + openidnp.identity;
			internal const string claimed_id = Prefix + openidnp.claimed_id;
			internal const string expires_in = Prefix + openidnp.expires_in;
			internal const string assoc_type = Prefix + openidnp.assoc_type;
			internal const string assoc_handle = Prefix + openidnp.assoc_handle;
			internal const string session_type = Prefix + openidnp.session_type;
			internal const string is_valid = Prefix + openidnp.is_valid;
			internal const string sig = Prefix + openidnp.sig;
			internal const string signed = Prefix + openidnp.signed;
			internal const string user_setup_url = Prefix + openidnp.user_setup_url;
			internal const string trust_root = Prefix + openidnp.trust_root;
			internal const string realm = Prefix + openidnp.realm;
			internal const string invalidate_handle = Prefix + openidnp.invalidate_handle;
			internal const string dh_modulus = Prefix + openidnp.dh_modulus;
			internal const string dh_gen = Prefix + openidnp.dh_gen;
			internal const string dh_consumer_public = Prefix + openidnp.dh_consumer_public;
			internal const string dh_server_public = Prefix + openidnp.dh_server_public;
		}
		internal const string enc_mac_key = "enc_mac_key";
		internal const string mac_key = "mac_key";
		internal const string sreg_ns = "http://openid.net/extensions/sreg/1.1";
		internal const string sreg_compatibility_alias = "sreg";
		internal static class SessionType {
			internal const string DH_SHA1 = "DH-SHA1";
			internal const string DH_SHA256 = "DH-SHA256";
			internal const string NoEncryption20 = "no-encryption";
			internal const string NoEncryption11 = "";
		}
		internal static class SignatureAlgorithms {
			internal const string HMAC_SHA1 = "HMAC-SHA1";
			internal const string HMAC_SHA256 = "HMAC-SHA256";
		}
		internal static class OpenIdNs {
			internal const string v10 = "http://openid.net/signon/1.0";
			internal const string v11 = "http://openid.net/signon/1.1";
			internal const string v20 = "http://specs.openid.net/auth/2.0";
		}

		internal static class Modes {
			internal const string cancel = "cancel";
			internal const string error = "error";
			internal const string id_res = "id_res";
			internal const string checkid_immediate = "checkid_immediate";
			internal const string checkid_setup = "checkid_setup";
			internal const string check_authentication = "check_authentication";
			internal const string associate = "associate";
		}
		internal static class Genders {
			internal const string Male = "M";
			internal const string Female = "F";
		}
		internal static class IsValid {
			internal const string True = "true";
		}

		/// <summary>
		/// Used by ASP.NET on a login page to determine where a successful
		/// login should be redirected to.
		/// </summary>
		internal const string ReturnUrl = "ReturnUrl";
	}
}
