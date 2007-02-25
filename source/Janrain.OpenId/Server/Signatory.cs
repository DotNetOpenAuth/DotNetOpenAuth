using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Security.Cryptography;
using Janrain.OpenId.Store;

namespace Janrain.OpenId.Server
{
    public class Signatory
    {
        public static readonly TimeSpan SECRET_LIFETIME = new TimeSpan(0, 0, 14 * 24 * 60 * 60);

        #region Private Members

        private static readonly Uri _normal_key = new Uri("http://localhost/|normal");
        private static readonly Uri _dumb_key = new Uri("http://localhost/|dumb");
        private IAssociationStore _store;

        #endregion

        #region Constructor(s)

        public Signatory(IAssociationStore store)
        {
            if (store == null)
                throw new ArgumentNullException("store");

            _store = store;
        }

        #endregion

        #region Methods

        public void Sign(Response response)
        {
            NameValueCollection nvc = new NameValueCollection();
            Association assoc;
            string assoc_handle = ((AssociatedRequest)response.Request).AssocHandle;

            if (assoc_handle != null && assoc_handle != "")
            {
                assoc = this.GetAssociation(assoc_handle, false);

                if (assoc == null)
                {
                    response.Fields["invalidate_handle"] = assoc_handle;
                    assoc = this.CreateAssociation(true);
                }
            }
            else
            {
                assoc = this.CreateAssociation(true);
            }

            response.Fields["assoc_handle"] = assoc.Handle;

            foreach (DictionaryEntry pair in response.Fields)
            {
                nvc.Add(pair.Key.ToString(), pair.Value.ToString());
            }

            string sig = assoc.SignDict(response.Signed, nvc, "");
            string signed = String.Join(",", response.Signed);

            response.Fields["sig"] = sig;
            response.Fields["signed"] = signed;

        }

        public virtual bool Verify(string assoc_handle, string sig, NameValueCollection signed_pairs)
        {
            Association assoc = this.GetAssociation(assoc_handle, true);
            string expected_sig;


            if (assoc == null)
                return false;

            expected_sig = CryptUtil.ToBase64String(assoc.Sign(signed_pairs));

            return (sig == expected_sig);
        }

        public virtual Association CreateAssociation(bool dumb)
        {
            RNGCryptoServiceProvider generator = new RNGCryptoServiceProvider();
            Uri key;
            byte[] secret = new byte[20];
            byte[] uniq_bytes = new byte[4];
            string uniq;
            double seconds;
            string handle;
            Association assoc;


            generator.GetBytes(secret);
            generator.GetBytes(uniq_bytes);

            uniq = CryptUtil.ToBase64String(uniq_bytes);

            TimeSpan time = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            seconds = time.TotalSeconds;

            handle = "{{HMAC-SHA1}{" + seconds + "}{" + uniq + "}";

            assoc = new HMACSHA1Association(handle, secret, SECRET_LIFETIME);

            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            _store.StoreAssociation(key, assoc);

            return assoc;
        }

        public virtual Association GetAssociation(string assoc_handle, bool dumb)
        {
            Uri key;


            if (assoc_handle == null)
                throw new ArgumentNullException("assoc_handle");

            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            Association assoc = _store.GetAssociation(key, assoc_handle);
            if (assoc != null && assoc.ExpiresIn <= 0)
            {
                _store.RemoveAssociation(key, assoc_handle);
                assoc = null;
            }

            return assoc;
        }

        public virtual void Invalidate(string assoc_handle, bool dumb)
        {
            Uri key;


            if (dumb)
                key = _dumb_key;
            else
                key = _normal_key;

            _store.RemoveAssociation(key, assoc_handle);
        }

        #endregion

    }
}
