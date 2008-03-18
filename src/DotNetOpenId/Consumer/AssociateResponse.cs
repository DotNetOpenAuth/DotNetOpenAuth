using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Globalization;
using System.Diagnostics;

namespace DotNetOpenId.Consumer {
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

					try {
						Association assoc;
						string assoc_type = getParameter(QueryStringArgs.openidnp.assoc_type);
						switch (assoc_type) {
							case QueryStringArgs.HMAC_SHA1:
								byte[] secret;

								string session_type;
								if (!Args.TryGetValue(QueryStringArgs.openidnp.session_type, out session_type) ||
									session_type == QueryStringArgs.plaintext ||
									session_type == string.Empty) {
									secret = getDecoded(QueryStringArgs.mac_key);
								} else if (QueryStringArgs.DH_SHA1.Equals(session_type, StringComparison.Ordinal)) {
									byte[] dh_server_public = getDecoded(QueryStringArgs.openidnp.dh_server_public);
									byte[] enc_mac_key = getDecoded(QueryStringArgs.enc_mac_key);
									secret = CryptUtil.SHA1XorSecret(DH, dh_server_public, enc_mac_key);
								} else // # XXX: log this
									return null;

								string assocHandle = getParameter(QueryStringArgs.openidnp.assoc_handle);
								TimeSpan expiresIn = new TimeSpan(0, 0, Convert.ToInt32(getParameter(QueryStringArgs.openidnp.expires_in), CultureInfo.CurrentUICulture));

								assoc = new HmacSha1Association(assocHandle, secret, expiresIn);
								break;
							default:
								Trace.TraceError("Unrecognized assoc_type '{0}'.", assoc_type);
								assoc = null;
								break;
						}

						return assoc;
					} catch (OpenIdException ex) {
						if (TraceUtil.Switch.TraceError) {
							Trace.TraceError(ex.ToString());
						}
						return null;
					}
				}
				return association;
			}
		}
	}
}
