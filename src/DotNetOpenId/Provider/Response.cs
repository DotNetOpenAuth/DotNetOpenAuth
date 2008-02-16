using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using DotNetOpenId;
using DotNetOpenId.Consumer;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotNetOpenId.Provider
{
    internal class Response : IEncodable
    {
        private List<string> _signed;

        public Response(Request request)
        {
            Request = request;
            _signed = new List<string>();
            Fields = new Dictionary<string, string>();
            
        }

        public Request Request { get; set; }
        public IDictionary<string, string> Fields { get; private set; }
        public string[] Signed
        {
            get { return _signed.ToArray(); }
        }
        public bool NeedsSigning
        {
            get
            {
                return Request is CheckIdRequest && Signed.Length > 0;
            }
        }

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

        #region IEncodable Members

        public EncodingType EncodingType
        {
            get
            {
                return Request.RequestType == RequestType.CheckIdRequest ? 
                    EncodingType.UrlRedirection : EncodingType.KVForm;
            }
        }

        public IDictionary<string, string> EncodedFields
        {
            get
            {
                var nvc = new Dictionary<string, string>();

                foreach (var pair in Fields)
                {
                    if (Request.RequestType == RequestType.CheckIdRequest)
                    {
                        nvc.Add(QueryStringArgs.openid.Prefix + pair.Key, pair.Value);
                    }
                    else
                    {
                        nvc.Add(pair.Key, pair.Value);
                    }
                }

                return nvc;
            }
        }
        public Uri BaseUri
        {
            get
            {
                if (Request.RequestType != RequestType.CheckIdRequest)
                {
                    throw new InvalidOperationException("Encoding to URL is only appropriate on CheckIdRequest requests.");
                }
                CheckIdRequest checkidreq = (CheckIdRequest)Request;
                return checkidreq.ReturnTo;
            }
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
