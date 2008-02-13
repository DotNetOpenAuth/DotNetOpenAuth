using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

namespace DotNetOpenId {
    public class HmacSha1Association : Association {

        #region Constructor(s)

        public HmacSha1Association(string handle, byte[] secret, TimeSpan expiresIn)
            : base(handle, secret, expiresIn, CutToSecond(DateTime.UtcNow)) {
        }

        public HmacSha1Association(IDictionary<string, string> kvpairs)
            : base(kvpairs["handle"], Convert.FromBase64String(kvpairs["secret"]),
            new TimeSpan(0, 0, Convert.ToInt32(kvpairs["expires_in"])),
            UNIX_EPOCH.Add(new TimeSpan(0, 0, Convert.ToInt32(kvpairs["issued"])))) {
        }

        #endregion

        #region Methods

        static DateTime CutToSecond(DateTime dateTime) {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond));
        }

        public override string AssociationType {
            get { return "HMAC-SHA1"; }
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;

            if (obj.GetType() == typeof(HmacSha1Association)) {
                HmacSha1Association a = (HmacSha1Association)obj;
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

        public override int GetHashCode() {
            HMACSHA1 hmac = new HMACSHA1(this.Secret);
            CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);

            byte[] hbytes = ASCIIEncoding.ASCII.GetBytes(this.Handle);

            cs.Write(hbytes, 0, hbytes.Length);
            cs.Close();

            byte[] hash = hmac.Hash;
            hmac.Clear();

            long val = 0;
            for (int i = 0; i < hash.Length; i++) {
                val = val ^ (long)hash[i];
            }

            val = val ^ this.Expires.ToFileTimeUtc();

            return (int)val;
        }

        public override string SignDict(ICollection<string> fields, IDictionary<string, string> data, string prefix) {
            var l = new Dictionary<string, string>();

            foreach (string field in fields) {
                l.Add(field, data[prefix + field]);
            }

            return CryptUtil.ToBase64String(Sign(l));
        }

        public override byte[] Sign(IDictionary<string, string> l) {
            byte[] data = KVUtil.SeqToKV(l, false);

            HMACSHA1 hmac = new HMACSHA1(this.Secret);

            byte[] hash = hmac.ComputeHash(data);
            hmac.Clear();

            return hash;
        }
        #endregion


    }
}