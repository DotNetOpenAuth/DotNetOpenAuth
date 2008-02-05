using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Janrain.Yadis
{
    [Serializable]
    public class UriNode : XrdNode, IComparable
    {
        protected ServiceNode serviceNode;

        public UriNode(XrdNode serviceNode, XmlNode node, XmlDocument xmldoc, XmlNamespaceManager xmlnsManager)
            : base(node, xmldoc, xmlnsManager)
        {
            this.serviceNode = (ServiceNode)serviceNode;
        }

        public int CompareTo(object that)
        {
            if (that is UriNode)
            {
                UriNode node = (UriNode)that;
                int num = this.serviceNode.CompareTo(node.serviceNode);
                if (num == 0)
                {
                    return this.Priority.CompareTo(node.Priority);
                }
                return num;
            }
            return 0;
        }

        public override string ToString()
        {
            return base.node.InnerXml;
        }

        // Properties
        public int Priority
        {
            get
            {
                XmlAttribute namedItem = (XmlAttribute)base.node.Attributes.GetNamedItem("priority");
                return Convert.ToInt32(namedItem.Value);
            }
        }

        public ServiceNode ServiceNode
        {
            get
            {
                return this.serviceNode;
            }
        }

        public Uri Uri
        {
            get
            {
                try
                {
                    return new Uri(this.ToString());
                }
                catch (UriFormatException)
                {
                    return null;
                }
            }
        }
    }
}
