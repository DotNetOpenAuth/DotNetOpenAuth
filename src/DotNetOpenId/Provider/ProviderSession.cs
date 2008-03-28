using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetOpenId.Provider {
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
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue,
					protocol.openid.session_type, session_type), provider.Query);
			}
		}
	}

	/// <summary>
	/// An object that knows how to handle association requests with no session type.
	/// </summary>
	internal class PlainTextProviderSession : ProviderSession {
		public PlainTextProviderSession(OpenIdProvider provider) : base(provider) { }
		public override string SessionType {
			get { return Protocol.Args.SessionType.NoEncryption; }
		}

		public override Dictionary<string, string> Answer(byte[] secret) {
			var nvc = new Dictionary<string, string>();
			nvc.Add(Protocol.openidnp.mac_key, CryptUtil.ToBase64String(secret));
			return nvc;
		}
	}

	/// <summary>
	/// An object that knows how to handle association requests with the Diffie-Hellman session type.
	/// </summary>
	internal class DiffieHellmanProviderSession : ProviderSession, IDisposable {
		byte[] _consumer_pubkey;
		DiffieHellman _dh;
		string sessionType;

		public DiffieHellmanProviderSession(OpenIdProvider provider)
			: base(provider) {
			sessionType = Util.GetRequiredArg(provider.Query, Protocol.openid.session_type);

			string missing;
			string dh_modulus = Util.GetOptionalArg(Provider.Query, Protocol.openid.dh_modulus);
			string dh_gen = Util.GetOptionalArg(Provider.Query, Protocol.openid.dh_gen);
			byte[] dh_modulus_bytes = new byte[0];
			byte[] dh_gen_bytes = new byte[0];

			if (dh_modulus == null ^ dh_gen == null) {
				// Only one of an atomic arg pair was included.  They must either both
				// be omitted or both be specified.
				missing = (dh_modulus == null) ? "modulus" : "generator";
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, missing), provider.Query);
			}

			if (!String.IsNullOrEmpty(dh_modulus) || !String.IsNullOrEmpty(dh_gen)) {
				try {
					dh_modulus_bytes = Convert.FromBase64String(dh_modulus);
				} catch (FormatException) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValueBadBase64,
						Protocol.openid.dh_modulus, dh_modulus), provider.Query);
				}

				try {
					dh_gen_bytes = Convert.FromBase64String(dh_gen);
				} catch (FormatException) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValueBadBase64,
						Protocol.openid.dh_gen, dh_gen), Provider.Query);
				}
			} else {
				dh_modulus_bytes = CryptUtil.DEFAULT_MOD;
				dh_gen_bytes = CryptUtil.DEFAULT_GEN;
			}

			_dh = new DiffieHellmanManaged(dh_modulus_bytes, dh_gen_bytes, 1024);

			string consumer_pubkey = Util.GetRequiredArg(Provider.Query, Protocol.openid.dh_consumer_public);
			try {
				_consumer_pubkey = Convert.FromBase64String(consumer_pubkey);
			} catch (FormatException) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValueBadBase64,
					Protocol.openid.dh_consumer_public, consumer_pubkey), Provider.Query);
			}
		}

		public override string SessionType {
			get { return sessionType; }
		}

		public override Dictionary<string, string> Answer(byte[] secret) {
			byte[] mac_key = CryptUtil.SHAHashXorSecret(CryptUtil.Sha1, _dh, _consumer_pubkey, secret);
			var nvc = new Dictionary<string, string>();

			nvc.Add(Protocol.openidnp.dh_server_public, CryptUtil.UnsignedToBase64(_dh.CreateKeyExchange()));
			nvc.Add(Protocol.openidnp.enc_mac_key, CryptUtil.ToBase64String(mac_key));

			return nvc;
		}

		#region IDisposable Members

		~DiffieHellmanProviderSession() {
			Dispose(false);
		}
		void Dispose(bool disposing) {
			if (disposing) {
				if (_dh != null) {
					((IDisposable)_dh).Dispose();
					_dh = null;
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
