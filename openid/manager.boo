namespace Janrain.OpenId.Consumer

import System
import System.Web.SessionState
import Janrain.Yadis


class ServiceEndpointManager:
    session as HttpSessionState

    def constructor(session as HttpSessionState):
        self.session = session

    def GetNextService(openid_url as Uri, prefix as string):
        key = prefix + openid_url.AbsoluteUri
        endpoints as (ServiceEndpoint) = self.session[key]
        
        if endpoints is null:
            endpoints = GetServiceEndpoints(openid_url)
            if endpoints is null:
                return null

        endpoint = endpoints[0]
        rest = endpoints[1:]
        if len(rest) > 0:
            self.session[key] = rest
        else:
            self.session.Remove(key)
        
        return endpoint

    def Cleanup(openid_url as Uri, prefix as string):
        key = prefix + openid_url.AbsoluteUri
        self.session.Remove(key)

    protected def GetServiceEndpoints(openid_url as Uri):
        result = Yadis.Discover(openid_url)
        if result is null:
            return null
            
        identity_url = result.NormalizedUri
        endpoints = []
        if result.IsXRDS:
            xrds_node = Xrd(result.ResponseText)
            for uri_node as UriNode in xrds_node.UriNodes():
                try:
                    endpoints.Add(ServiceEndpoint(identity_url, uri_node))
                except e as ArgumentException:
                    continue
        else:
            try:
                endpoints.Add(ServiceEndpoint(
                        identity_url, result.ResponseText))
            except e as ArgumentException:
                pass
        
        if len(endpoints) > 0:
            return array(ServiceEndpoint, endpoints)

        return null

    
