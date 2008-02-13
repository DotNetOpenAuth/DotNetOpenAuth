using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Janrain.Yadis
{
    [Serializable]
    internal class Xrd
    {
        protected XmlDocument xmldoc = new XmlDocument();
        protected XmlNamespaceManager xmlnsManager;
        protected const string XRD_NAMESPACE = "xri://$xrd*($v*2.0)";
        protected const string XRDS_NAMESPACE = "xri://$xrds";

        public Xrd(string text)
        {
            this.xmldoc.LoadXml(text);
            this.xmlnsManager = new XmlNamespaceManager(this.xmldoc.NameTable);
            this.xmlnsManager.AddNamespace("xrds", "xri://$xrds");
            this.xmlnsManager.AddNamespace("xrd", "xri://$xrd*($v*2.0)");
        }

        public ServiceNode[] ServiceNodes()
        {
            List<ServiceNode> serviceNodeList = new List<ServiceNode>();
            string xpath = "/xrds:XRDS/xrd:XRD[last()]/xrd:Service";
            XmlNodeList serviceNodes = this.xmldoc.SelectNodes(xpath, this.xmlnsManager);
            foreach (XmlNode node in serviceNodes)
            {
                serviceNodeList.Add(new ServiceNode(node, this.xmldoc, this.xmlnsManager));
            }
            return serviceNodeList.ToArray();
        }

        public UriNode[] UriNodes()
        {
            List<UriNode> uriNodeList = new List<UriNode>();
            ServiceNode[] serviceNodesArray = this.ServiceNodes();
            foreach (ServiceNode serviceNode in serviceNodesArray)
            {
                UriNode[] nodes = serviceNode.UriNodes();
                uriNodeList.AddRange(nodes);
            }
            uriNodeList.Sort();
            return uriNodeList.ToArray();
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
