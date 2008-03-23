using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetOpenId.Provider {
	internal abstract class ProviderSession {
		protected ProviderSession(Protocol protocol) {
			Protocol = protocol;
		}

		protected Protocol Protocol { get; private set; }
		public abstract string SessionType { get; }
		public abstract Dictionary<string, string> Answer(byte[] secret);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		public static ProviderSession CreateSession(IDictionary<string, string> query) {
			Protocol protocol = Protocol.Detect(query);
			string session_type = Util.GetOptionalArg(query, protocol.openid.session_type);
			if (protocol.Args.SessionType.NoEncryption.Equals(session_type, StringComparison.Ordinal) || session_type == null) {
				return new PlainTextProviderSession(protocol);
			} else if (protocol.Args.SessionType.DH_SHA1.Equals(session_type)) {
				return new DiffieHellmanProviderSession(protocol, query);
			} else {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValue,
					Protocol.Default.openid.session_type, session_type), query);
			}
		}
	}

	/// <summary>
	/// An object that knows how to handle association requests with no session type.
	/// </summary>
	internal class PlainTextProviderSession : ProviderSession {
		public PlainTextProviderSession(Protocol protocol) : base(protocol) { }
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

		public DiffieHellmanProviderSession(Protocol protocol, IDictionary<string, string> query)
			: base(protocol) {
			sessionType = Util.GetRequiredArg(query, Protocol.Default.openid.session_type);

			string missing;
			string dh_modulus = Util.GetOptionalArg(query, Protocol.Default.openid.dh_modulus);
			string dh_gen = Util.GetOptionalArg(query, Protocol.Default.openid.dh_gen);
			byte[] dh_modulus_bytes = new byte[0];
			byte[] dh_gen_bytes = new byte[0];

			if (dh_modulus == null ^ dh_gen == null) {
				// Only one of an atomic arg pair was included.  They must either both
				// be omitted or both be specified.
				missing = (dh_modulus == null) ? "modulus" : "generator";
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.MissingOpenIdQueryParameter, missing), query);
			}

			if (!String.IsNullOrEmpty(dh_modulus) || !String.IsNullOrEmpty(dh_gen)) {
				try {
					dh_modulus_bytes = Convert.FromBase64String(dh_modulus);
				} catch (FormatException) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValueBadBase64,
						Protocol.Default.openid.dh_modulus, dh_modulus), query);
				}

				try {
					dh_gen_bytes = Convert.FromBase64String(dh_gen);
				} catch (FormatException) {
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValueBadBase64,
						Protocol.Default.openid.dh_gen, dh_gen), query);
				}
			} else {
				dh_modulus_bytes = CryptUtil.DEFAULT_MOD;
				dh_gen_bytes = CryptUtil.DEFAULT_GEN;
			}

			_dh = new DiffieHellmanManaged(dh_modulus_bytes, dh_gen_bytes, 1024);

			string consumer_pubkey = Util.GetRequiredArg(query, Protocol.Default.openid.dh_consumer_public);
			try {
				_consumer_pubkey = Convert.FromBase64String(consumer_pubkey);
			} catch (FormatException) {
				throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
					Strings.InvalidOpenIdQueryParameterValueBadBase64,
					Protocol.Default.openid.dh_consumer_public, consumer_pubkey), query);
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
