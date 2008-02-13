using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;

namespace DotNetOpenId.Server
{
    /// <summary>
    /// A message did not conform to the OpenID protocol.
    /// </summary>
    public class ProtocolException : Exception, IEncodable
    {

        #region Private Members

        private NameValueCollection _query = new NameValueCollection();

        #endregion

        #region Constructor(s)

        public ProtocolException(NameValueCollection query, string text)
            : base(text)
        {
            _query = query;
        }

        #endregion

        #region Properties

        public bool HasReturnTo
        {
            get
            {
                return (_query[QueryStringArgs.openid.return_to] != null);
            }
        }

        #endregion

        #region IEncodable Members

        public EncodingType WhichEncoding
        {
            get 
            {
                if (this.HasReturnTo)
                    return EncodingType.ENCODE_URL;

                string mode = _query.Get(QueryStringArgs.openid.mode);
                if (mode != null)
                    if (mode != QueryStringArgs.Modes.checkid_setup &&
                        mode != QueryStringArgs.Modes.checkid_immediate)
                        return EncodingType.ENCODE_KVFORM;

                // Notes from the original port
                //# According to the OpenID spec as of this writing, we are
                //# probably supposed to switch on request type here (GET
                //# versus POST) to figure out if we're supposed to print
                //# machine-readable or human-readable content at this
                //# point.  GET/POST seems like a pretty lousy way of making
                //# the distinction though, as it's just as possible that
                //# the user agent could have mistakenly been directed to
                //# post to the server URL.

                //# Basically, if your request was so broken that you didn't
                //# manage to include an openid.mode, I'm not going to worry
                //# too much about returning you something you can't parse.
                return EncodingType.ENCODE_NONE;
            }
        }

        public Uri EncodeToUrl()
        {
            string return_to = _query.Get(QueryStringArgs.openid.return_to);
            if (return_to == null)
                throw new ApplicationException("return_to URL has not been set.");

            var q = new Dictionary<string, string>();
            q.Add(QueryStringArgs.openid.mode, QueryStringArgs.Modes.error);
            q.Add(QueryStringArgs.openid.error, this.Message);

            UriBuilder builder = new UriBuilder(return_to);
            UriUtil.AppendQueryArgs(builder, q);

            return new Uri(builder.ToString());
        }

        public byte[] EncodeToKVForm()
        {
            var d = new Dictionary<string, string>();

            d.Add(QueryStringArgs.openidnp.mode, QueryStringArgs.Modes.error);
            d.Add(QueryStringArgs.openidnp.error, this.Message);

            return KVUtil.DictToKV(d);
        }

        #endregion

    }
}
