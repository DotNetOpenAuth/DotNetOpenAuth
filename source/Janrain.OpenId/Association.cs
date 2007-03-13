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
        private string handle;
        private DateTime issued;
        protected TimeSpan expiresIn;
        protected byte[] key;

        #endregion

        #region Properties

        public string Handle
        {
            get { return handle; }
            set { handle = value; }
        }

        public DateTime Issued
        {
            get { return this.issued; }
			set { this.issued = value; }
        }

        public byte[] Secret
        {
            get { return key; }
        }

        public uint IssuedUnix
        {
			get { return (uint)((this.issued - UNIX_EPOCH).TotalSeconds); }
        }

        public DateTime Expires
        {
            get { return this.issued.Add(this.expiresIn); }
        }

        public bool IsExpired
        {
            get { return this.Expires < DateTime.UtcNow; }
        }

        public long ExpiresIn
        {
            get { return (long)(this.Expires - DateTime.UtcNow).Seconds; }
        }

        #endregion

        #region Abstract Methods

		public abstract string AssociationType
		{
			get;
		}

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
            dict.Add("secret", CryptUtil.ToBase64String(this.Secret));
            dict.Add("issued", this.IssuedUnix.ToString());
            dict.Add("expires_in", Convert.ToInt32(this.expiresIn.TotalSeconds).ToString());
            dict.Add("assoc_type", this.AssociationType);

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
            this.key = secret;
            this.Issued = UNIX_EPOCH.Add(new TimeSpan(0, 0, Convert.ToInt32((DateTime.Now - UNIX_EPOCH).TotalSeconds)));
        }

        public HMACSHA1Association(IDictionary kvpairs)
        {
            this.Handle = kvpairs["handle"].ToString();
            this.key = Convert.FromBase64String(kvpairs["secret"].ToString());

            int seconds = Convert.ToInt32(kvpairs["issued"]);
            this.Issued = UNIX_EPOCH.Add(new TimeSpan(0, 0, seconds));

            seconds = Convert.ToInt32(kvpairs["expires_in"]);
            this.expiresIn = new TimeSpan(0, 0, seconds);
        }

        #endregion

        #region Methods

        public override string AssociationType
        {
            get { return "HMAC-SHA1"; }
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

                if (a.expiresIn != this.expiresIn)
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
