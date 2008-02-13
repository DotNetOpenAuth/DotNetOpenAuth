using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Janrain.Yadis
{
    [Serializable]
    internal class TypeNode : XrdNode
    {
        public TypeNode(XmlNode node, XmlDocument xmldoc, XmlNamespaceManager xmlnsManager)
            : base(node, xmldoc, xmlnsManager)
        {
        }

        public override string ToString()
        {
            return base.node.InnerXml;
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
