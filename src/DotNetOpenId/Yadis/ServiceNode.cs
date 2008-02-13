using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Janrain.Yadis
{
    [Serializable]
    internal class ServiceNode : XrdNode, IComparable
    {
        public ServiceNode(XmlNode node, XmlDocument xmldoc, XmlNamespaceManager xmlns_manager)
            : base(node, xmldoc, xmlns_manager)
        {
        }

        public int CompareTo(object that)
        {
            if (that is ServiceNode)
            {
                return this.Priority.CompareTo(((ServiceNode)that).Priority);
            }
            return 0;
        }

        public TypeNode[] TypeNodes()
        {
            List<TypeNode> typeNodeList = new List<TypeNode>();
            XmlNodeList typeNodes = base.node.SelectNodes("./xrd:Type", base.xmlnsManager);
            foreach (XmlNode node in typeNodes)
            {
                typeNodeList.Add(new TypeNode(node, base.xmldoc, base.xmlnsManager));
            }
            return typeNodeList.ToArray();
        }

        public UriNode[] UriNodes()
        {
            List<UriNode> uriNodesList = new List<UriNode>();
            XmlNodeList uriNodes = base.node.SelectNodes("./xrd:URI", base.xmlnsManager);
            foreach (XmlNode node in uriNodes)
            {
                UriNode uriNode = new UriNode(this, node, base.xmldoc, base.xmlnsManager);
                uriNodesList.Add(uriNode);
            }
            return uriNodesList.ToArray();
        }

        public int Priority
        {
            get
            {
                XmlAttribute namedItem = (XmlAttribute)base.node.Attributes.GetNamedItem("priority");
                return Convert.ToInt32(namedItem.Value);
            }
        }
    }
}