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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
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

					string assoc_type = getParameter(Protocol.Constants.openidnp.assoc_type);
					switch (assoc_type) {
						case Protocol.Constants.SignatureAlgorithms.HMAC_SHA256:
						case Protocol.Constants.SignatureAlgorithms.HMAC_SHA1:
							byte[] secret;

							string session_type;
							if (!Args.TryGetValue(Protocol.Constants.openidnp.session_type, out session_type) ||
								session_type == Protocol.Constants.SessionType.NoEncryption11 ||
								session_type == Protocol.Constants.SessionType.NoEncryption20) {
								secret = getDecoded(Protocol.Constants.mac_key);
							} else if (Protocol.Constants.SessionType.DH_SHA1.Equals(session_type, StringComparison.Ordinal)) {
								byte[] dh_server_public = getDecoded(Protocol.Constants.openidnp.dh_server_public);
								byte[] enc_mac_key = getDecoded(Protocol.Constants.enc_mac_key);
								secret = CryptUtil.SHAHashXorSecret(CryptUtil.Sha1, DH, dh_server_public, enc_mac_key);
							} else if (Protocol.Constants.SessionType.DH_SHA256.Equals(session_type, StringComparison.Ordinal)) {
								byte[] dh_server_public = getDecoded(Protocol.Constants.openidnp.dh_server_public);
								byte[] enc_mac_key = getDecoded(Protocol.Constants.enc_mac_key);
								secret = CryptUtil.SHAHashXorSecret(CryptUtil.Sha256, DH, dh_server_public, enc_mac_key);
							} else {
								throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
									Strings.InvalidOpenIdQueryParameterValue,
									Protocol.Constants.openid.session_type, session_type));
							}

							string assocHandle = getParameter(Protocol.Constants.openidnp.assoc_handle);
							TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(Protocol.Constants.openidnp.expires_in), CultureInfo.CurrentUICulture));

							if (assoc_type == Protocol.Constants.SignatureAlgorithms.HMAC_SHA1) {
								association = new HmacSha1Association(assocHandle, secret, expiresIn);
							} else if (assoc_type == Protocol.Constants.SignatureAlgorithms.HMAC_SHA256) {
								association = new HmacSha256Association(assocHandle, secret, expiresIn);
							} else {
								throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
									Strings.InvalidOpenIdQueryParameterValue,
									Protocol.Constants.openid.assoc_type, assoc_type));
							}
							break;
						default:
							throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
								Strings.InvalidOpenIdQueryParameterValue,
								Protocol.Constants.openid.assoc_type, assoc_type));
					}
				}
				return association;
			}
		}
	}
}
