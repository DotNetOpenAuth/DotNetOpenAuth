using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Janrain.Yadis
{
    [Serializable]
    internal class XrdNode
    {
        protected XmlNode node;
        protected XmlDocument xmldoc;
        protected XmlNamespaceManager xmlnsManager;

        public XrdNode(XmlNode node, XmlDocument xmldoc, XmlNamespaceManager xmlnsManager)
        {
            this.node = node;
            this.xmldoc = xmldoc;
            this.xmlnsManager = xmlnsManager;
        }

        public XmlNode Node
        {
            get
            {
                return this.node;
            }
        }

        public XmlDocument XmlDoc
        {
            get
            {
                return this.xmldoc;
            }
        }

        public XmlNamespaceManager XmlNsManager
        {
            get
            {
                return this.xmlnsManager;
            }
        }
    }
}
