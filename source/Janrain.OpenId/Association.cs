using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Janrain.OpenId
{
    public abstract class Association : ICloneable
    {

        #region Member Variables

        protected static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private string _handle;
        private DateTime _issued;
        protected TimeSpan _expiresIn;
        protected byte[] _key;

        #endregion

        #region Properties

        public string Handle
        {
            get
            {
                return _handle;
            }
            set
            {
                _handle = value;
            }
        }

        public DateTime Issued
        {
            get
            {
                return _issued;
            }
            set
            {
                _issued = value;
            }
        }

        public byte[] Secret
        {
            get
            {
                return _key;
            }
        }

        public uint IssuedUnix
        {
            get
            {
                return uint.Parse((Issued - UNIX_EPOCH).TotalSeconds.ToString());
            }
        }

        public DateTime Expires
        {
            get
            {
                return Issued.Add(_expiresIn);
            }
        }

        public bool IsExpired
        {
            get
            {
                return Expires < DateTime.Now;
            }
        }

        public long ExpiresIn
        {
            get
            {
                TimeSpan time = this.Expires.ToUniversalTime().Subtract(DateTime.UtcNow);

                return (long)time.Seconds;
            }
        }

        #endregion

        #region Abstract Methods

        public abstract string AssociationType();
        public abstract string SignDict(string[] fields, NameValueCollection data, string prefix);
        public abstract byte[] Sign(NameValueCollection l);

        #endregion

        #region ICloneable Members

        object ICloneable.Clone()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region Methods

        public virtual byte[] Serialize()
        {
            Dictionary<string, string> dict = new Dictionary<string,string>();

            dict.Add("version", "2");
            dict.Add("handle", this.Handle);
            dict.Add("secret", CryptUtil.ToBase64String(this.Secret).ToString());
            dict.Add("issued", this.IssuedUnix.ToString());
            dict.Add("expires_in", Convert.ToInt32(this._expiresIn.TotalSeconds).ToString());
            dict.Add("assoc_type", this.AssociationType());

            return KVUtil.DictToKV(dict);
        }

        public virtual Association Deserialize(byte[] data)
        {
            Dictionary<string, string> kvpairs = (Dictionary<string, string>) KVUtil.KVToDict(data);
            string version = kvpairs["version"];

            if (version != "2")
                throw new NotSupportedException("Unknown version: " + version);

            string assoc_type = kvpairs["assoc_type"];
            if (assoc_type == "HMAC-SHA1")
                return new HMACSHA1Association(kvpairs);
            else
                throw new NotSupportedException("Unknown association type: " + assoc_type);
        }

        #endregion

    }

    // TODO Move this class out to it's own file
    public class HMACSHA1Association : Association
    {

        #region Constructor(s)

        public HMACSHA1Association(string handle, byte[] secret, TimeSpan expiresIn)
        {
            this.Handle = handle;
            this._key = secret;
            this.Issued = UNIX_EPOCH.Add(new TimeSpan(0, 0, Convert.ToInt32((DateTime.Now - UNIX_EPOCH).TotalSeconds)));
        }

        public HMACSHA1Association(IDictionary kvpairs)
        {
            this.Handle = kvpairs["handle"].ToString();
            this._key = Convert.FromBase64String(kvpairs["secret"].ToString());

            int seconds = Convert.ToInt32(kvpairs["issued"]);
            this.Issued = UNIX_EPOCH.Add(new TimeSpan(0, 0, seconds));

            seconds = Convert.ToInt32(kvpairs["expires_in"]);
            this._expiresIn = new TimeSpan(0, 0, seconds);
        }

        #endregion

        #region Methods

        public override string AssociationType()
        {
            return "HMAC-SHA1";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj.GetType() == typeof(HMACSHA1Association))
            {
                HMACSHA1Association a = (HMACSHA1Association) obj;
                if (a.Handle != this.Handle)
                    return false;

                if (CryptUtil.ToBase64String(a.Secret) != CryptUtil.ToBase64String(this.Secret))
                    return false;

                if (a.Expires != this.Expires)
                    return false;

                if (a._expiresIn != this._expiresIn)
                    return false;

                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {

            HMACSHA1 hmac = new HMACSHA1(this.Secret);
            CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);
            byte[] hbytes = ASCIIEncoding.ASCII.GetBytes(this.Handle);

            cs.Write(hbytes, 0, hbytes.Length);
            cs.Close();

            byte[] hash = hmac.Hash;
            hmac.Clear();

            long val = 0;
            for (int i = 0; i < hash.Length; i++)
            {
                val = val ^ (long)hash[i];
            }
            val = val ^ this.Expires.ToFileTimeUtc();

            return (int) val;
        }

        public override string SignDict(string[] fields, NameValueCollection data, string prefix)
        {
            
            NameValueCollection l = new NameValueCollection();

            foreach (string field in fields)
            {
                l.Add(field, data[(prefix + field)]);
            }
            return CryptUtil.ToBase64String(Sign(l));
        }

        public override byte[] Sign(NameValueCollection l)
        {
            byte[] data = KVUtil.SeqToKV(l, false);
            HMACSHA1 hmac = new HMACSHA1(this.Secret);
            byte[] hash = hmac.ComputeHash(data);
            hmac.Clear();
            return hash;
        }

        #endregion

    }
}
