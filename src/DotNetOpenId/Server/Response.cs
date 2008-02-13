using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId;
using DotNetOpenId.Consumer;
using System.Collections.Generic;

namespace DotNetOpenId.Server
{
    public class Response : IEncodable
    {

        #region Private Members

        private Dictionary<string, string> _fields;
        private List<string> _signed;
        private Request _request;

        #endregion

        #region Constructor(s)

        public Response(Request request)
        {
            this.Request = request;
            _signed = new List<string>();
            _fields = new Dictionary<string, string>();
            
        }

        #endregion

        #region Properties

        public Request Request
        {
            get { return _request; }
            set { _request = value; }
        }

        public IDictionary<string, string> Fields
        {
            get { return _fields; }
        }

        public string[] Signed
        {
            get { return _signed.ToArray(); }
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
            if (this._signed != null && !this._signed.Contains(key))
            {
                _signed.Add(key);
            }
        }

        public void AddFields(string nmspace, IDictionary<string, string> fields, bool signed)
        {
            foreach (var pair in fields)
            {
                this.AddField(nmspace, pair.Key, pair.Value, signed);
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
            var nvc = new Dictionary<string, string>();


            foreach (var pair in this.Fields)
            {
                nvc.Add(QueryStringArgs.openid.Prefix + pair.Key, pair.Value);
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
