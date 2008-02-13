using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DotNetOpenId
{
    public abstract class Association
    {

        #region Constructor(s)

        protected Association(string handle, byte[] key, TimeSpan expiresIn, DateTime issued) {
            this.handle = handle;
            this.key = key;
            this.expiresIn = expiresIn;
            this.issued = issued;
        }

        #endregion

        #region Member Variables

        protected internal readonly static DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
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
            get { return (long)(this.Expires - DateTime.UtcNow).TotalSeconds; }
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

        #region Methods

        public virtual byte[] Serialize()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

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
            Dictionary<string, string> kvpairs = (Dictionary<string, string>)KVUtil.KVToDict(data, data.Length);
            string version = kvpairs["version"];

            if (version != "2")
                throw new NotSupportedException("Unknown version: " + version);

            string assoc_type = kvpairs["assoc_type"];
            if (assoc_type == "HMAC-SHA1")
                return new HmacSha1Association(kvpairs);
            else
                throw new NotSupportedException("Unknown association type: " + assoc_type);
        }

        #endregion


        public override string ToString()
        {
            string returnString = @"Association.Handle= '{0}'
Association.Issued = '{1}'
Association.Secret = '{2}' 
Association.IssuedUnix = '{3}' 
Association.Expires = '{4}' 
Association.IsExpired = '{5}' 
Association.ExpiresIn = '{6}' ";
            return String.Format(returnString, Handle, Issued.ToString(), Secret.ToString(), IssuedUnix, Expires.ToString(), IsExpired, ExpiresIn);
        }

    }
}
