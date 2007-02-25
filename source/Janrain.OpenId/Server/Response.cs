using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using Janrain.OpenId;

namespace Janrain.OpenId.Server
{
    class Response : IEncodable
    {

        private Hashtable _fields;
        private ListDictionary _signed;
        private Request _request;

        public Response(Request request)
        {
            this.Request = request;
            _signed = new ListDictionary();
            _fields = new Hashtable();
        }

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
            get { return (string[])_signed; ; }
        }

        #region IEncodable Members

        public EncodingType WhichEncoding
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public Uri EncodeToUrl()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte[] EncodeToKVForm()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
