using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace DotNetOpenId.Provider
{
	internal abstract class ProviderSession {
		public abstract string SessionType { get; }
		public abstract Dictionary<string, string> Answer(byte[] secret);
		public static ProviderSession CreateSession(NameValueCollection query) {
			string session_type = query[QueryStringArgs.openid.session_type];

			switch (session_type) {
				case null:
					return new PlainTextProviderSession();
				case QueryStringArgs.DH_SHA1:
					return new DiffieHellmanProviderSession(query);
				default:
					throw new ProtocolException(query, "Unknown session type " + session_type);
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
            get { return QueryStringArgs.plaintext; }
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
    internal class DiffieHellmanProviderSession : ProviderSession
    {
        byte[] _consumer_pubkey;
        DiffieHellman _dh;

        public DiffieHellmanProviderSession(NameValueCollection query)
        {
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

                throw new ProtocolException(query, "If non-default modulus or generator is supplied, both must be supplied. Missing: " + missing);
            }
            
            if (!String.IsNullOrEmpty(dh_modulus) || !String.IsNullOrEmpty(dh_gen))
            {
                try
                {
                    dh_modulus_bytes = Convert.FromBase64String(dh_modulus);                    
                }
                catch (FormatException)
                {
                    throw new ProtocolException(query, "dh_modulus isn't properly base64ed");
                }

                try
                {
                    dh_gen_bytes = Convert.FromBase64String(dh_gen);
                }
                catch (FormatException)
                {
                    throw new ProtocolException(query, "dh_gen isn't properly base64ed");
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
                throw new ProtocolException(query, "Public key for DH-SHA1 session not found in query");

            try
            {
                _consumer_pubkey = Convert.FromBase64String(consumer_pubkey);
            }
            catch (FormatException)
            {
                throw new ProtocolException(query, "consumer_pubkey isn't properly base64ed");
            }
        }

        public override string SessionType
        {
            get { return QueryStringArgs.DH_SHA1; }
        }

        public override Dictionary<string, string> Answer(byte[] secret)
        {
            byte[] mac_key = CryptUtil.SHA1XorSecret(_dh, _consumer_pubkey, secret);
            var nvc = new Dictionary<string, string>();

            nvc.Add(QueryStringArgs.openidnp.dh_server_public, CryptUtil.UnsignedToBase64(_dh.CreateKeyExchange()));
            nvc.Add(QueryStringArgs.enc_mac_key, CryptUtil.ToBase64String(mac_key));

            return nvc;
        }
    }
}
