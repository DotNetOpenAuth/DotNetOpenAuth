using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Janrain.OpenId;

namespace Janrain.OpenId.Server
{
    class Response : IEncodable
    {

        #region Private Members

        private Hashtable _fields;
        private ArrayList _signed;
        private Request _request;

        #endregion

        #region Constructor(s)

        public Response(Request request)
        {
            this.Request = request;
            _signed = new ArrayList();
            _fields = new Hashtable();
            
        }

        #endregion

        #region Properties

        public Request Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public IDictionary Fields
        {
            get { return _fields; }
        }

        public string[] Signed
        {
            get { return (string[]) _signed.ToArray(); }
        }

        public bool NeedsSigning
        {
            get
            {
                  return (
                    (this.Request.Mode == "checkid_setup" ||
                     this.Request.Mode == "checkid_immediate")
                     &&
                    (this.Signed.Length > 0)
                    );
            }
        }

        #endregion

        #region Methods

        public void AddField(string nmspace, string key, string value, bool signed)
        {
            if (nmspace != null && nmspace != String.Empty)
            {
                key = nmspace + "." + key;
            }

            this.Fields[key] = value;
            if (this.Signed != null && !Util.InArray(this.Signed, key))
            {
                _signed.Add(key);
            }
        }

        public void AddFields(string nmspace, IDictionary fields, bool signed)
        {
            foreach (DictionaryEntry pair in fields)
            {
                this.AddField(nmspace, (string)pair.Key, (string)pair.Value, signed);
            }
        }

        public void Update(string nmspace, Response other)
        {
            Hashtable nmspaced_fields = new Hashtable();
            ArrayList nmspaced_signed = new ArrayList();


            if (nmspace == null || nmspace == String.Empty)
            {
                nmspaced_fields = (Hashtable) other.Fields;
                nmspaced_signed = new ArrayList(this.Signed);
            }
            else
            {
                foreach (DictionaryEntry pair in other.Fields)
                {
                    nmspaced_fields.Add(nmspace + "." + pair.Key.ToString(), pair.Value);
                }

                foreach (string k in other.Signed)
                {
                    nmspaced_signed.Add(nmspace + "." + k);
                }
            }

        }

        #endregion

        #region IEncodable Members

        public EncodingType WhichEncoding
        {
            get
            {
                if (this.Request.Mode == "checkid_setup" || this.Request.Mode == "checkid_immediate")
                {
                    return EncodingType.ENCODE_URL;
                }
                else
                {
                    return EncodingType.ENCODE_KVFORM;
                }
            }
        }

        public Uri EncodeToUrl()
        {
            NameValueCollection nvc = new NameValueCollection();


            foreach (DictionaryEntry pair in this.Fields)
            {
                nvc.Add("openid." + pair.Key.ToString(), pair.Value.ToString());
            }

            CheckIdRequest checkidreq = (CheckIdRequest)this.Request;
            UriBuilder builder = new UriBuilder(checkidreq.ReturnTo);
            Util.AppendQueryArgs(ref builder, nvc);

            return new Uri(builder.ToString(), true);
        }

        public byte[] EncodeToKVForm()
        {
            return KVUtil.DictToKV(this.Fields);
        }

        #endregion
    }
}
