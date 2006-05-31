namespace Janrain.Yadis

import System
import System.Xml
#import System.Xml.XPath

class XrdNode:
    [Getter(Node)]
    node as XmlNode

    [Getter(XmlDoc)]
    xmldoc as XmlDocument

    [Getter(XmlNsManager)]
    xmlns_manager as XmlNamespaceManager
    
    def constructor(node as XmlNode, xmldoc as XmlDocument,
                    xmlns_manager as XmlNamespaceManager):
        self.node = node
        self.xmldoc = xmldoc
        self.xmlns_manager = xmlns_manager


class UriNode(XrdNode, IComparable):
    Priority:
        get:
            attr = cast(XmlAttribute, self.node.Attributes.GetNamedItem(
                "priority"))
            return Convert.ToInt32(attr.Value)

    Uri:
        get:
            try:
                return Uri(self.ToString())
            except e as UriFormatException:
                return null

    [Getter(ServiceNode)]
    service_node as ServiceNode

    def constructor(service_node as XrdNode, node as XmlNode,
                    xmldoc as XmlDocument,
                    xmlns_manager as XmlNamespaceManager):
        super(node, xmldoc, xmlns_manager)
        self.service_node = service_node

    def CompareTo(that as object) as int:
        if that isa UriNode:
            xthat = cast(UriNode, that)
            sct = self.service_node.CompareTo(xthat.service_node)
            if sct == 0:
                return self.Priority.CompareTo(xthat.Priority)
            else:
                return sct
        else:
            return 0

    def ToString():
        return self.node.InnerXml
    

class TypeNode(XrdNode):
    Uri:
        get:
            try:
                return Uri(self.ToString())
            except e as UriFormatException:
                return null

    def constructor(node as XmlNode,
                    xmldoc as XmlDocument,
                    xmlns_manager as XmlNamespaceManager):
        super(node, xmldoc, xmlns_manager)

    def ToString():
        return self.node.InnerXml
    

class ServiceNode(XrdNode, IComparable):
    Priority:
        get:
            attr = cast(XmlAttribute, self.node.Attributes.GetNamedItem(
                "priority"))
            return Convert.ToInt32(attr.Value)

    def constructor(node as XmlNode,
                    xmldoc as XmlDocument,
                    xmlns_manager as XmlNamespaceManager):
        super(node, xmldoc, xmlns_manager)
    

    def TypeNodes():
        type_node_list = []
        type_nodes = self.node.SelectNodes('./xrd:Type', self.xmlns_manager)
        for type_node as XmlNode in type_nodes:
            type_node_list.Add(
                TypeNode(type_node, self.xmldoc, self.xmlns_manager))
        
        return array(TypeNode, type_node_list)

    def UriNodes():
        uri_node_list = []
        uri_nodes = self.node.SelectNodes('./xrd:Uri', self.xmlns_manager)
        for uri_node as XmlNode in uri_nodes:
            uri_node_list.Add(
                UriNode(self, uri_node, self.xmldoc, self.xmlns_manager))
        
        return array(UriNode, uri_node_list)

    def CompareTo(that as object) as int:
        if that isa ServiceNode:
            return self.Priority.CompareTo(cast(ServiceNode, that).Priority)
        else:
            return 0


class Xrd:
    static final XRDS_NAMESPACE = 'xri://$xrds'
    static final XRD_NAMESPACE = 'xri://$xrd*($v*2.0)'

    [Getter(XmlDoc)]
    xmldoc as XmlDocument

    [Getter(XmlNsManager)]
    xmlns_manager as XmlNamespaceManager

    def constructor(text as string):
        #stream = MemoryStream(resp.data, 0, resp.length)
        xmldoc = XmlDocument()
        xmldoc.Load(text)
        xmlns_manager = XmlNamespaceManager(xmldoc.NameTable)
        xmlns_manager.AddNamespace('xrds', XRDS_NAMESPACE)
        xmlns_manager.AddNamespace('xrd', XRD_NAMESPACE)

    def ServiceNodes():
        service_node_list = []
        query = '/xrds:XRDS/xrd:XRD[last()]/xrd:Service'
        service_nodes = self.xmldoc.SelectNodes(query, self.xmlns_manager)
        for service_node as XmlNode in service_nodes:
            service_node_list.Add(
                ServiceNode(service_node, self.xmldoc, self.xmlns_manager))
        
        return array(ServiceNode, service_node_list)

    def UriNodes():
        uri_nodes as (UriNode) = (,)
        for service_node in ServiceNodes():
            uri_nodes += service_node.UriNodes()

        Array.Sort(uri_nodes)
        return uri_nodes

    
