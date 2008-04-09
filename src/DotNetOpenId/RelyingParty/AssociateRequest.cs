using System;
using System.Collections.Generic;
using System.Text;
using Org.Mentalis.Security.Cryptography;
using System.Diagnostics;

namespace DotNetOpenId.RelyingParty {
	[DebuggerDisplay("Mode: {Args[\"openid.mode\"]}, {Args[\"openid.assoc_type\"]}, OpenId: {Protocol.Version}")]
	class AssociateRequest : DirectRequest {
		/// <param name="dh">Optional.  Supplied only if Diffie-Hellman is used for encrypting the association secret key.</param>
		AssociateRequest(ServiceEndpoint provider, IDictionary<string, string> args, DiffieHellman dh)
			: base(provider, args) {
			DH = dh;
		}
		public DiffieHellman DH { get; private set; }

		public static AssociateRequest Create(ServiceEndpoint provider) {
			bool useSha256 = provider.Protocol.Version.Major >= 2;
			string assoc_type = useSha256 ?
				provider.Protocol.Args.SignatureAlgorithm.HMAC_SHA256 :
				provider.Protocol.Args.SignatureAlgorithm.HMAC_SHA1;
			string session_type = useSha256 ?
					provider.Protocol.Args.SessionType.DH_SHA256 :
					provider.Protocol.Args.SessionType.DH_SHA1;
			return Create(provider, assoc_type, session_type);
		}

		public static AssociateRequest Create(ServiceEndpoint provider, string assoc_type, string session_type) {
			if (provider == null) throw new ArgumentNullException("provider");
			if (assoc_type == null) throw new ArgumentNullException("assoc_type");
			if (session_type == null) throw new ArgumentNullException("session_type");
			Debug.Assert(Array.IndexOf(provider.Protocol.Args.SignatureAlgorithm.All, assoc_type) >= 0);
			Debug.Assert(Array.IndexOf(provider.Protocol.Args.SessionType.All, session_type) >= 0);

			var args = new Dictionary<string, string>();
			Protocol protocol = provider.Protocol;

			args.Add(protocol.openid.mode, protocol.Args.Mode.associate);
			args.Add(protocol.openid.assoc_type, assoc_type);

			DiffieHellman dh = null;

			if (provider.ProviderEndpoint.Scheme == Uri.UriSchemeHttps) {
				args.Add(protocol.openid.session_type, protocol.Args.SessionType.NoEncryption);
			} else {
				// Initiate Diffie-Hellman Exchange
				dh = CryptUtil.CreateDiffieHellman();

				byte[] dhPublic = dh.CreateKeyExchange();
				string cpub = CryptUtil.UnsignedToBase64(dhPublic);

				args.Add(protocol.openid.session_type, session_type);
				args.Add(protocol.openid.dh_consumer_public, cpub);

				DHParameters dhps = dh.ExportParameters(true);

				if (dhps.P != CryptUtil.DEFAULT_MOD || dhps.G != CryptUtil.DEFAULT_GEN) {
					args.Add(protocol.openid.dh_modulus, CryptUtil.UnsignedToBase64(dhps.P));
					args.Add(protocol.openid.dh_gen, CryptUtil.UnsignedToBase64(dhps.G));
				}
			}

			return new AssociateRequest(provider, args, dh);
		}
		AssociateResponse response;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)] // code execution in getter
		public AssociateResponse Response {
			get {
				if (response == null) {
					try {
						response = new AssociateResponse(Provider, GetResponse(), DH);
					} catch (OpenIdException ex) {
						if (ex.Query != null) {
							response = new AssociateResponse(Provider, ex.Query, DH);
						}
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
