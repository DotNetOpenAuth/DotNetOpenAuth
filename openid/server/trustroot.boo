namespace Janrain.OpenId.Server

import System
import System.Text.RegularExpressions

class TrustRoot:
    static tr_regex = Regex("^(?<scheme>https?)://((?<wildcard>\\*)|(?<wildcard>\\*\\.)?(?<host>[a-zA-Z0-9-]+(\\.[a-zA-Z0-9-]+)*)\\.?)(:(?<port>[0-9]+))?(?<path>(/.*|$))")
    
    static top_level_domains = /\|/.Split(
        'com|edu|gov|int|mil|net|org|biz|info|name|museum|coop|aero|ac|ad|ae|' +
        'af|ag|ai|al|am|an|ao|aq|ar|as|at|au|aw|az|ba|bb|bd|be|bf|bg|bh|bi|bj|' +
        'bm|bn|bo|br|bs|bt|bv|bw|by|bz|ca|cc|cd|cf|cg|ch|ci|ck|cl|cm|cn|co|cr|' +
        'cu|cv|cx|cy|cz|de|dj|dk|dm|do|dz|ec|ee|eg|eh|er|es|et|fi|fj|fk|fm|fo|' +
        'fr|ga|gd|ge|gf|gg|gh|gi|gl|gm|gn|gp|gq|gr|gs|gt|gu|gw|gy|hk|hm|hn|hr|' +
        'ht|hu|id|ie|il|im|in|io|iq|ir|is|it|je|jm|jo|jp|ke|kg|kh|ki|km|kn|kp|' +
        'kr|kw|ky|kz|la|lb|lc|li|lk|lr|ls|lt|lu|lv|ly|ma|mc|md|mg|mh|mk|ml|mm|' +
        'mn|mo|mp|mq|mr|ms|mt|mu|mv|mw|mx|my|mz|na|nc|ne|nf|ng|ni|nl|no|np|nr|' +
        'nu|nz|om|pa|pe|pf|pg|ph|pk|pl|pm|pn|pr|ps|pt|pw|py|qa|re|ro|ru|rw|sa|' +
        'sb|sc|sd|se|sg|sh|si|sj|sk|sl|sm|sn|so|sr|st|sv|sy|sz|tc|td|tf|tg|th|' +
        'tj|tk|tm|tn|to|tp|tr|tt|tv|tw|tz|ua|ug|uk|um|us|uy|uz|va|vc|ve|vg|vi|' +
        'vn|vu|wf|ws|ye|yt|yu|za|zm|zw')
    
    unparsed as string
    scheme as string
    wildcard as bool
    host as string
    port as int
    path as string
    
    IsSane:
        get:
            if self.host == 'localhost':
                return true
            
            host_parts = /\./.Split(self.host)
            
            tld = host_parts[-1]
            if tld not in top_level_domains:
                return false
            
            if len(tld) == 2:
                if len(host_parts) == 1:
                    # entire host part is 2-letter tld
                    return false
                
                if len(host_parts[-2]) <= 3:
                    # It's a 2-letter tld, so there needs to be more than two
                    # segments specified (e.g. *.co.uk is insane)
                    return len(host_parts) > 2
            else:
                # It's a regular tld, so it needs at least one more segment
                return len(host_parts) > 1
            
            # Fell through, so not sane
            return false
    
    def constructor(unparsed as string):
        mo = tr_regex.Match(unparsed)
        if mo.Success:
            self.unparsed = unparsed
            self.scheme = mo.Groups['scheme'].Value
            self.wildcard = mo.Groups['wildcard'].Value != String.Empty
            self.host = mo.Groups['host'].Value.ToLower()
            
            port_group = mo.Groups['port']
            if port_group.Success:
                self.port = Convert.ToInt32(port_group.Value)
            elif self.scheme == "https":
                self.port = 443
            else:
                self.port = 80
            
            self.path = mo.Groups['path'].Value
            if self.path == String.Empty:
                self.path = "/"
        else:
            raise ArgumentException(
                "${unparsed} does not appear to be a valid TrustRoot")
    
    
    def ValidateUrl(url as Uri):
        if url.Scheme != self.scheme:
            return false
        
        if url.Port != self.port:
            return false
        
        if not self.wildcard:
            if url.Host != self.host:
                return false
        elif self.host != String.Empty:
            host_parts = /\./.Split(self.host)
            url_parts = /\./.Split(url.Host)
            end_parts = url_parts[len(url_parts) - len(host_parts):]
            for i, end_part in enumerate(end_parts):
                if end_part != host_parts[i]:
                    return false
        
        if url.PathAndQuery == self.path:
            return true
        
        path_len = len(self.path)
        url_prefix = url.PathAndQuery[:path_len]
        
        # must be equal up to the length of the path, at least
        if self.path != url_prefix:
            return false
        
        # These characters must be on the boundary between the end
        # of the trust root's path and the start of the URL's
        # path.
        if '?' in self.path:
            allowed = '&'
        else:
            allowed = '?/'
        
        return (self.path[len(self.path)-1] in allowed or
            url.PathAndQuery[path_len] in allowed)
    
    static def CheckSanity(trust_root as string):
        return TrustRoot(trust_root).IsSane
    
    static def CheckURL(trust_root as string, url as Uri):
        try:
            tr = TrustRoot(trust_root)
            return tr.ValidateUrl(url)
        except e as ArgumentException:
            return false
    
    def ToString():
        return self.unparsed

