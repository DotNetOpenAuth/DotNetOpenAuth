using System;
using System.Collections.Specialized;
using Org.Mentalis.Security.Cryptography;
using System.Text;

namespace DotNetOpenId.Server
{
    public abstract class ServerSession
    {
        #region Properties

        public string SessionType { get; set; }

        #endregion

        #region Methods

        public abstract NameValueCollection Answer(byte[] secret);

        #endregion

    }

    /// <summary>
    /// An object that knows how to handle association requests with no session type.
    /// </summary>
    public class PlainTextServerSession : ServerSession
    {

        #region Constructor(s)

        public PlainTextServerSession()
        {
            this.SessionType = "plaintext";
        }

        #endregion

        #region Methods

        public override NameValueCollection Answer(byte[] secret)
        {
            NameValueCollection nvc = new NameValueCollection();

            nvc.Add(QueryStringArgs.mac_key, CryptUtil.ToBase64String(secret));

            return nvc;
        }

        #endregion

    }

    /// <summary>
    /// An object that knows how to handle association requests with the Diffie-Hellman session type.
    /// </summary>
    public class DiffieHellmanServerSession : ServerSession
    {

        #region Private Members

        private byte[] _consumer_pubkey;
        private DiffieHellman _dh;

        #endregion

        #region Constructor(s)

        public DiffieHellmanServerSession(NameValueCollection query)
        {
            string missing;
            string dh_modulus = query.Get(QueryStringArgs.openid.dh_modulus);
            string dh_gen = query.Get(QueryStringArgs.openid.dh_gen);
            byte[] dh_modulus_bytes = new byte[0];
            byte[] dh_gen_bytes = new byte[0];

            this.SessionType = QueryStringArgs.DH_SHA1;

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

        #endregion

        #region Methods

        public override NameValueCollection Answer(byte[] secret)
        {
            byte[] mac_key = CryptUtil.SHA1XorSecret(_dh, _consumer_pubkey, secret);
            NameValueCollection nvc = new NameValueCollection();

            nvc.Add(QueryStringArgs.openidnp.dh_server_public, CryptUtil.UnsignedToBase64(_dh.CreateKeyExchange()));
            nvc.Add(QueryStringArgs.enc_mac_key, CryptUtil.ToBase64String(mac_key));

            return nvc;
        }

        #endregion

    }

}
