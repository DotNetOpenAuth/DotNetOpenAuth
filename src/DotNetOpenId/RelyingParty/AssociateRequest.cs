using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	class AssociateRequest : DirectRequest {
		/// <param name="dh">Optional.  Supplied only if Diffie-Hellman is used for encrypting the association secret key.</param>
		AssociateRequest(ServiceEndpoint provider, IDictionary<string, string> args, DiffieHellman dh)
			: base(provider, args) {
			DH = dh;
		}
		public DiffieHellman DH { get; private set; }

		public static AssociateRequest Create(ServiceEndpoint provider) {
			var args = new Dictionary<string, string>();
			Protocol protocol = provider.Protocol;

			bool useSha256 = provider.Protocol.Version.Major >= 2;

			args.Add(protocol.openid.mode, protocol.Args.Mode.associate);
			args.Add(protocol.openid.assoc_type, useSha256 ?
				protocol.Args.SignatureAlgorithm.HMAC_SHA256 :
				protocol.Args.SignatureAlgorithm.HMAC_SHA1);

			DiffieHellman dh = null;

			if (provider.ProviderEndpoint.Scheme == Uri.UriSchemeHttps) {
				args.Add(Protocol.Default.openid.session_type, protocol.Args.SessionType.NoEncryption);
			} else {
				// Initiate Diffie-Hellman Exchange
				dh = CryptUtil.CreateDiffieHellman();

				byte[] dhPublic = dh.CreateKeyExchange();
				string cpub = CryptUtil.UnsignedToBase64(dhPublic);

				args.Add(protocol.openid.session_type, useSha256 ?
					protocol.Args.SessionType.DH_SHA256 :
					protocol.Args.SessionType.DH_SHA1);
				args.Add(Protocol.Default.openid.dh_consumer_public, cpub);

				DHParameters dhps = dh.ExportParameters(true);

				if (dhps.P != CryptUtil.DEFAULT_MOD || dhps.G != CryptUtil.DEFAULT_GEN) {
					args.Add(Protocol.Default.openid.dh_modulus, CryptUtil.UnsignedToBase64(dhps.P));
					args.Add(Protocol.Default.openid.dh_gen, CryptUtil.UnsignedToBase64(dhps.G));
				}
			}

			return new AssociateRequest(provider, args, dh);
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
