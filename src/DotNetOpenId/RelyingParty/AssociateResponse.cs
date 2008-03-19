using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class AssociateResponse : DirectResponse {
		public AssociateResponse(Uri provider, IDictionary<string, string> args, DiffieHellman dh)
			: base(provider, args) {
			DH = dh;
		}
		public DiffieHellman DH { get; private set; }

		Association association;
		public Association Association {
			get {
				if (association == null) {
					Converter<string, string> getParameter = delegate(string key) {
						string val;
						if (!Args.TryGetValue(key, out val) || string.IsNullOrEmpty(val))
							throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture, Strings.MissingOpenIdQueryParameter, key));
						return val;
					};

					Converter<string, byte[]> getDecoded = delegate(string key) {
						try {
							return Convert.FromBase64String(getParameter(key));
						} catch (FormatException ex) {
							throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
								Strings.ExpectedBase64OpenIdQueryParameter, key), null, ex);
						}
					};

					string assoc_type = getParameter(QueryStringArgs.openidnp.assoc_type);
					switch (assoc_type) {
						case QueryStringArgs.SignatureAlgorithms.HMAC_SHA256:
						case QueryStringArgs.SignatureAlgorithms.HMAC_SHA1:
							byte[] secret;

							string session_type;
							if (!Args.TryGetValue(QueryStringArgs.openidnp.session_type, out session_type) ||
								session_type == QueryStringArgs.SessionType.NoEncryption11 ||
								session_type == QueryStringArgs.SessionType.NoEncryption20) {
								secret = getDecoded(QueryStringArgs.mac_key);
							} else if (QueryStringArgs.SessionType.DH_SHA1.Equals(session_type, StringComparison.Ordinal)) {
								byte[] dh_server_public = getDecoded(QueryStringArgs.openidnp.dh_server_public);
								byte[] enc_mac_key = getDecoded(QueryStringArgs.enc_mac_key);
								secret = CryptUtil.SHA1XorSecret(DH, dh_server_public, enc_mac_key);
							} else {
								throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
									Strings.InvalidOpenIdQueryParameterValue,
									QueryStringArgs.openid.session_type, session_type));
							}

							string assocHandle = getParameter(QueryStringArgs.openidnp.assoc_handle);
							TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(QueryStringArgs.openidnp.expires_in), CultureInfo.CurrentUICulture));

							if (assoc_type == QueryStringArgs.SignatureAlgorithms.HMAC_SHA1) {
								association = new HmacSha1Association(assocHandle, secret, expiresIn);
							} else if (assoc_type == QueryStringArgs.SignatureAlgorithms.HMAC_SHA256) {
								association = new HmacSha256Association(assocHandle, secret, expiresIn);
							} else {
								throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
									Strings.InvalidOpenIdQueryParameterValue,
									QueryStringArgs.openid.assoc_type, assoc_type));
							}
							break;
						default:
							throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
								Strings.InvalidOpenIdQueryParameterValue,
								QueryStringArgs.openid.assoc_type, assoc_type));
					}
				}
				return association;
			}
		}
	}
}
