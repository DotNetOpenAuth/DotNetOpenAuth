using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetOpenId.Provider
{
	internal abstract class ProviderSession {
		public abstract string SessionType { get; }
		public abstract Dictionary<string, string> Answer(byte[] secret);
		public static ProviderSession CreateSession(NameValueCollection query) {
			string session_type = query[QueryStringArgs.openid.session_type];

			switch (session_type) {
				case null:
				case QueryStringArgs.SessionType.NoEncryption11:
				case QueryStringArgs.SessionType.NoEncryption20:
					return new PlainTextProviderSession();
				case QueryStringArgs.SessionType.DH_SHA1:
					return new DiffieHellmanProviderSession(query);
				default:
					throw new OpenIdException(string.Format(CultureInfo.CurrentUICulture,
						Strings.InvalidOpenIdQueryParameterValue, 
						QueryStringArgs.openid.session_type, session_type), query);
			}
		}
	}

    /// <summary>
    /// An object that knows how to handle association requests with no session type.
    /// </summary>
    internal class PlainTextProviderSession : ProviderSession
    {
        public override string SessionType
        {
            get { return QueryStringArgs.SessionType.NoEncryption11; }
        }

        public override Dictionary<string, string> Answer(byte[] secret)
        {
            var nvc = new Dictionary<string, string>();
            nvc.Add(QueryStringArgs.mac_key, CryptUtil.ToBase64String(secret));
            return nvc;
        }
    }

    /// <summary>
    /// An object that knows how to handle association requests with the Diffie-Hellman session type.
    /// </summary>
    internal class DiffieHellmanProviderSession : ProviderSession, IDisposable
    {
        byte[] _consumer_pubkey;
        DiffieHellman _dh;
		string sessionType;

        public DiffieHellmanProviderSession(NameValueCollection query)
        {
			sessionType = query[QueryStringArgs.openid.session_type];

            string missing;
            string dh_modulus = query.Get(QueryStringArgs.openid.dh_modulus);
            string dh_gen = query.Get(QueryStringArgs.openid.dh_gen);
            byte[] dh_modulus_bytes = new byte[0];
            byte[] dh_gen_bytes = new byte[0];

            if ((dh_modulus == null && dh_gen != null) ||
                (dh_gen == null && dh_modulus != null))
            {
                if (dh_modulus == null)
                    missing = "modulus";
                else
                    missing = "generator";

                throw new OpenIdException("If non-default modulus or generator is supplied, both must be supplied. Missing: " + missing, query);
            }
            
            if (!String.IsNullOrEmpty(dh_modulus) || !String.IsNullOrEmpty(dh_gen))
            {
                try
                {
                    dh_modulus_bytes = Convert.FromBase64String(dh_modulus);                    
                }
                catch (FormatException)
                {
					throw new OpenIdException("dh_modulus isn't properly base64ed", query);
                }

                try
                {
                    dh_gen_bytes = Convert.FromBase64String(dh_gen);
                }
                catch (FormatException)
                {
					throw new OpenIdException("dh_gen isn't properly base64ed", query);
                }                
            }
            else
            {
                dh_modulus_bytes = CryptUtil.DEFAULT_MOD;
                dh_gen_bytes = CryptUtil.DEFAULT_GEN;
            }

            _dh = new DiffieHellmanManaged(dh_modulus_bytes, dh_gen_bytes, 1024);

            string consumer_pubkey = query.Get(QueryStringArgs.openid.dh_consumer_public);
            if (consumer_pubkey == null)
				throw new OpenIdException("Public key for DH-SHA1 session not found in query", query);

            try
            {
                _consumer_pubkey = Convert.FromBase64String(consumer_pubkey);
            }
            catch (FormatException)
            {
				throw new OpenIdException("consumer_pubkey isn't properly base64ed", query);
            }
        }

        public override string SessionType {
            get { return sessionType; }
        }

        public override Dictionary<string, string> Answer(byte[] secret)
        {
            byte[] mac_key = CryptUtil.SHAHashXorSecret(CryptUtil.Sha1, _dh, _consumer_pubkey, secret);
            var nvc = new Dictionary<string, string>();

            nvc.Add(QueryStringArgs.openidnp.dh_server_public, CryptUtil.UnsignedToBase64(_dh.CreateKeyExchange()));
            nvc.Add(QueryStringArgs.enc_mac_key, CryptUtil.ToBase64String(mac_key));

            return nvc;
        }

		#region IDisposable Members

		public void Dispose() {
			if (_dh != null) {
				((IDisposable)_dh).Dispose();
			}
		}

		#endregion
	}
}
