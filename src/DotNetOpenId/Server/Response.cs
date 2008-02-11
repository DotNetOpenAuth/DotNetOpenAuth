using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId;
using DotNetOpenId.Consumer;

namespace DotNetOpenId.Server
{
    public class Response : IEncodable
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
            get { return (string[])_signed.ToArray(typeof(string)); }
        }

        public bool NeedsSigning
        {
            get
            {
                  return (
                    (this.Request.Mode == QueryStringArgs.Modes.checkid_setup ||
                     this.Request.Mode == QueryStringArgs.Modes.checkid_immediate)
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
            if (this.Signed != null && Array.IndexOf(this.Signed, key) < 0)
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

        #endregion

        #region IEncodable Members

        public EncodingType WhichEncoding
        {
            get
            {
                if (this.Request.Mode == QueryStringArgs.Modes.checkid_setup || this.Request.Mode == QueryStringArgs.Modes.checkid_immediate)
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
                nvc.Add(QueryStringArgs.openid.Prefix + pair.Key.ToString(), pair.Value.ToString());
            }

            CheckIdRequest checkidreq = (CheckIdRequest)this.Request;
            UriBuilder builder = new UriBuilder(checkidreq.ReturnTo);
            UriUtil.AppendQueryArgs(builder, nvc);

            return new Uri(builder.ToString());
        }

        public byte[] EncodeToKVForm()
        {
            return KVUtil.DictToKV(this.Fields);
        }

        #endregion

        public override string ToString()
        {
            string returnString = String.Format("Response.NeedsSigning = {0}", this.NeedsSigning);             
            foreach (string key in Fields.Keys)
            {
                returnString += Environment.NewLine +  String.Format("ResponseField[{0}] = '{1}'", key, Fields[key]);
            }
            return returnString;            
        }               


    }
}
