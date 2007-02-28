using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Janrain.Yadis;

namespace Janrain.OpenId.Consumer
{
    class ServiceEndpoint
    {
        public static readonly Uri OPENID_1_0_NS = new Uri("http://openid.net/xmlns/1.0");
        public static readonly Uri OPENID_1_2_TYPE = new Uri("http://openid.net/signon/1.2");
        public static readonly Uri OPENID_1_1_TYPE = new Uri("http://openid.net/signon/1.1");
        public static readonly Uri OPENID_1_0_TYPE = new Uri("http://openid.net/signon/1.0");

        public static readonly Uri[] OPENID_TYPE_URIS = { OPENID_1_2_TYPE, 
                                    OPENID_1_1_TYPE, 
                                    OPENID_1_0_TYPE };

        private Uri _identityUrl;
        public Uri IdentityUrl
        {
            get
            {
                return _identityUrl;
            }
        }

        private Uri _serverUrl;
        public Uri ServerUrl
        {
            get
            {
                return _serverUrl;
            }
        }

        private Uri _delegateUrl;
        public Uri DelegateUrl
        {
            get
            {
                return _delegateUrl;
            }
        }

        private bool _usedYadis;
        public bool UsedYadis
        {
            get
            {
                return _usedYadis;
            }
        }

        Uri[] _typeUris;

        public Uri ServerId
        {
            get
            {
                if (this._delegateUrl == null)
                {
                    return this._identityUrl;
                }
                return this._delegateUrl;
            }
        }

        public static Uri ExtractDelegate(ServiceNode serviceNode)
        {
            XmlNamespaceManager nsmgr = serviceNode.XmlNsManager;
            nsmgr.PushScope();
            nsmgr.AddNamespace("openid", OPENID_1_0_NS.AbsoluteUri);
            XmlNodeList delegateNodes = serviceNode.Node.SelectNodes("./openid:Delegate", nsmgr);
            Uri delegateUrl = null;
            foreach (XmlNode delegateNode in delegateNodes)
            {
                try
                {
                    delegateUrl = new Uri(delegateNode.InnerXml);
                    break;
                }
                catch (UriFormatException)
                {
                    continue;
                }
            }
            nsmgr.PopScope();
            return delegateUrl;
        }

        internal ServiceEndpoint(Uri identityUrl, Uri serverUrl, Uri[] typeUris, Uri delegateUrl, bool usedYadis)
        {
            this._identityUrl = identityUrl;
            this._serverUrl = serverUrl;
            this._typeUris = typeUris;
            this._delegateUrl = delegateUrl;
            this._usedYadis = usedYadis;
        }

        public ServiceEndpoint(Uri yadisUrl, UriNode uriNode)
        {
            ServiceNode serviceNode = uriNode.ServiceNode;

            TypeNode[] typeNodes = serviceNode.TypeNodes();
            
            List<Uri> typeUriList = new List<Uri>();
            foreach (TypeNode t in typeNodes)
            {
                typeUriList.Add(t.Uri);
            }
            Uri[] typeUris = typeUriList.ToArray();

            List<Uri> matchesList = new List<Uri>();
            foreach (Uri u in OPENID_TYPE_URIS)
            {
                foreach (TypeNode t in typeNodes)
                {
                    if (u == t.Uri)
                    {
                        matchesList.Add(u);
                    }
                }
            }

            Uri[] matches = matchesList.ToArray();
            
            if ((matches.Length == 0) || (uriNode.Uri == null))
            {
                throw new ArgumentException("No matching openid type uris");
            }
            this._identityUrl = yadisUrl;
            this._serverUrl = uriNode.Uri;
            this._typeUris = typeUris;
            this._delegateUrl = ExtractDelegate(serviceNode);
            this._usedYadis = true;
        }

        public ServiceEndpoint(Uri uri, string html)
        {
            object[] objArray = ByteParser.HeadTagAttrs(html, "link");
            foreach(NameValueCollection values in objArray)
            {
                string text = values["rel"];
                if (text != null)
                {
                    string uriString = values["href"];
                    if (uriString != null)
                    {
                        if ((text == "openid.server") && (this._serverUrl == null))
                        {
                            try
                            {
                                this._serverUrl = new Uri(uriString);
                            }
                            catch (UriFormatException)
                            {
                            }
                        }
                        if ((text == "openid.delegate") && (this._delegateUrl == null))
                        {
                            try
                            {
                                this._delegateUrl = new Uri(uriString);
                                continue;
                            }
                            catch (UriFormatException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            if (this.ServerUrl == null)
            {
                throw new ArgumentException("html did not contain openid.server link");
            }
            this._identityUrl = uri;
            this._typeUris = new Uri[] { OPENID_1_0_TYPE };
            this._usedYadis = false;
        }

        public bool UsesExtension(Uri extension_uri)
        {
            //TODO: I think that all Arrays of stuff could use generics...
           foreach(Uri u in this._typeUris)
           {
               if (u == extension_uri)
                   return true;
           }
           return false;
        }

 

 


 


 


 

    }
}
