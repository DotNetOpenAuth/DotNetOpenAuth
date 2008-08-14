using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DotNetOpenId.Provider {
	[DebuggerDisplay("{SessionType}")]
	internal abstract class ProviderSession {
		protected ProviderSession(OpenIdProvider provider) {
			if (provider == null) throw new ArgumentNullException("provider");
			Provider = provider;
		}

		protected OpenIdProvider Provider { get; private set; }
		protected Protocol Protocol { get { return Provider.Protocol; } }
		public abstract string SessionType { get; }
		public abstract Dictionary<string, string> Answer(byte[] secret);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		public static ProviderSession CreateSession(OpenIdProvider provider) {
			if (provider == null) throw new ArgumentNullException("provider");
			Protocol protocol = provider.Protocol;
			string session_type = protocol.Version.Major >= 2 ?
				Util.GetRequiredArg(provider.Query, protocol.openid.session_type) :
				(Util.GetOptionalArg(provider.Query, protocol.openid.session_type) ?? "");

			if (protocol.Args.SessionType.NoEncryption.Equals(session_type, StringComparison.Ordinal)) {
				return new PlainTextProviderSession(provider);
			} else if (Array.IndexOf(protocol.Args.SessionType.AllDiffieHellman, session_type) >= 0) {
				return new DiffieHellmanProviderSession(provider);
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentCulture,
					Strings.InvalidOpenIdQueryParameterValue,
					protocol.openid.session_type, session_type), provider.Query) {
						ExtraArgsToReturn = AssociateRequest.CreateAssociationTypeHints(provider),
					};
			}
		}
	}

	/// <summary>
	/// An object that knows how to handle association requests with no session type.
	/// </summary>
	internal class PlainTextProviderSession : ProviderSession {
		public PlainTextProviderSession(OpenIdProvider provider) : base(provider) {
			// Extra requirements for OpenId 2.0 compliance.  Although the 1.0 spec
			// doesn't require use of an encrypted session, it's stupid not to encrypt
			// the shared secret key, so we'll enforce the rule for 1.0 and 2.0 RPs.
			if (!provider.RequestUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)) {
				throw new OpenIdException(Strings.EncryptionRequired, provider.Query);
			}
		}
		public override string SessionType {
			get { return Protocol.Args.SessionType.NoEncryption; }
		}

		public override Dictionary<string, string> Answer(byte[] secret) {
			var nvc = new Dictionary<string, string>();
			nvc.Add(Protocol.openidnp.mac_key, Convert.ToBase64String(secret));
			return nvc;
		}
	}

	/// <summary>
	/// An object that knows how to handle association requests with the Diffie-Hellman session type.
	/// </summary>
	internal class DiffieHellmanProviderSession : ProviderSession, IDisposable {
		byte[] consumerPublicKey;
		DiffieHellman dh;
		string sessionType;

		public DiffieHellmanProviderSession(OpenIdProvider provider)
			: base(provider) {
			sessionType = Util.GetRequiredArg(provider.Query, Protocol.openid.session_type);
			Debug.Assert(Array.IndexOf(Protocol.Args.SessionType.AllDiffieHellman, sessionType) >= 0, "We should not have been invoked if this wasn't a recognized DH session request.");

			byte[] dh_modulus = Util.GetOptionalBase64Arg(Provider.Query, Protocol.openid.dh_modulus) ?? DiffieHellmanUtil.DEFAULT_MOD;
			byte[] dh_gen = Util.GetOptionalBase64Arg(Provider.Query, Protocol.openid.dh_gen) ?? DiffieHellmanUtil.DEFAULT_GEN;
			dh = new DiffieHellmanManaged(dh_modulus, dh_gen, 1024);

			consumerPublicKey = Util.GetRequiredBase64Arg(Provider.Query, Protocol.openid.dh_consumer_public);
		}

		public override string SessionType {
			get { return sessionType; }
		}

		public override Dictionary<string, string> Answer(byte[] secret) {
			byte[] mac_key = DiffieHellmanUtil.SHAHashXorSecret(DiffieHellmanUtil.Lookup(Protocol, SessionType),
				dh, consumerPublicKey, secret);
			var nvc = new Dictionary<string, string>();

			nvc.Add(Protocol.openidnp.dh_server_public, DiffieHellmanUtil.UnsignedToBase64(dh.CreateKeyExchange()));
			nvc.Add(Protocol.openidnp.enc_mac_key, Convert.ToBase64String(mac_key));

			return nvc;
		}

		#region IDisposable Members

		~DiffieHellmanProviderSession() {
			Dispose(false);
		}
		void Dispose(bool disposing) {
			if (disposing) {
				if (dh != null) {
					((IDisposable)dh).Dispose();
					dh = null;
				}
			}
		}
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
