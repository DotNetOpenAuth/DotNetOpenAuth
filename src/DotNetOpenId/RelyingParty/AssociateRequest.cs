using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class AssociateRequest : DirectRequest {
		AssociateRequest(Uri provider, IDictionary<string, string> args, DiffieHellman dh)
			: base(provider, args) {
			DH = dh;
		}
		public DiffieHellman DH { get; private set; }

		public static AssociateRequest Create(Uri serverUrl) {
			var args = new Dictionary<string, string>();

			args.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.associate);
			args.Add(QueryStringArgs.openid.assoc_type, QueryStringArgs.HMAC_SHA1);

			DiffieHellman dh = null;

			if (serverUrl.Scheme == Uri.UriSchemeHttps) {
				args.Add(QueryStringArgs.openid.session_type, QueryStringArgs.SessionType.NoEncryption20);
			} else {
				// Initiate Diffie-Hellman Exchange
				dh = CryptUtil.CreateDiffieHellman();

				byte[] dhPublic = dh.CreateKeyExchange();
				string cpub = CryptUtil.UnsignedToBase64(dhPublic);

				args.Add(QueryStringArgs.openid.session_type, QueryStringArgs.SessionType.DH_SHA1);
				args.Add(QueryStringArgs.openid.dh_consumer_public, cpub);

				DHParameters dhps = dh.ExportParameters(true);

				if (dhps.P != CryptUtil.DEFAULT_MOD || dhps.G != CryptUtil.DEFAULT_GEN) {
					args.Add(QueryStringArgs.openid.dh_modulus, CryptUtil.UnsignedToBase64(dhps.P));
					args.Add(QueryStringArgs.openid.dh_gen, CryptUtil.UnsignedToBase64(dhps.G));
				}
			}

			return new AssociateRequest(serverUrl, args, dh);
		}
		AssociateResponse response;
		public AssociateResponse Response {
			get {
				if (response == null) {
					try {
						response = new AssociateResponse(Provider, GetResponse(), DH);
					} catch (OpenIdException) {
						// Silently fail at associate attempt, since we can recover
						// using dumb mode.
						if (TraceUtil.Switch.TraceWarning) {
							Trace.TraceWarning("Association attempt with {0} provider failed.", Provider);
						}
					}
				}
				return response;
			}
		}
	}
}
