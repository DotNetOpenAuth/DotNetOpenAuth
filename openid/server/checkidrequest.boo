namespace Janrain.OpenId.Server

import System
import System.Collections.Specialized
import Janrain.OpenId

class UntrustedReturnUrl(ProtocolException):
    return_to as Uri
    trust_root as string
    
    def constructor(query as NameValueCollection, return_to as Uri,
                    trust_root as string):
        super(query, "return_to ${return_to.AbsoluteUri} " +
              "not under trust_root ${trust_root}")
        self.return_to = return_to
        self.trust_root = trust_root


class MalformedReturnUrl(ProtocolException):
    return_to as string

    def constructor(query as NameValueCollection, return_to as string):
        super(query, "")
        self.return_to = return_to


class MalformedTrustRoot(ProtocolException):
    def constructor(query as NameValueCollection, text as string):
        super(query, text)


class CheckIdRequest(AssociatedRequest):
    [Getter(Immediate)]
    internal immediate as bool

    [Getter(TrustRoot)]
    internal trust_root as string
    
    [Getter(IdentityUrl)]
    internal identity as Uri

    [Getter(Mode)]
    internal mode as string

    [Getter(ReturnTo)]
    internal return_to as Uri

    def constructor(identity as Uri, return_to as Uri, trust_root as string,
                    immediate as bool, assoc_handle as string):
        self.assoc_handle = assoc_handle
        self.identity = identity
        self.return_to = return_to
        if trust_root is null:
            self.trust_root = return_to.AbsoluteUri
        else:
            self.trust_root = trust_root
            
        self.immediate = immediate
        if immediate:
            self.mode = "checkid_immediate"
        else:
            self.mode = "checkid_setup"

        try:
            TrustRoot(self.return_to.AbsoluteUri)
        except e as ArgumentException:
            raise MalformedReturnUrl(null, self.return_to.AbsoluteUri)

        if not self.TrustRootValid:
            raise UntrustedReturnUrl(null, self.return_to, self.trust_root)

    def constructor(query as NameValueCollection):
        mode as string = query['openid.mode']
        if mode == "checkid_immediate":
            self.immediate = true
            self.mode = "checkid_immediate"
        else:
            self.immediate = false
            self.mode = "checkid_setup"

        getField = do(field as string):
            value = query.Get("openid." + field)
            if value is null:
                raise ProtocolException(
                    query,
                    "Missing required field ${field}")
            return value

        identity as string = getField('identity')
        try:
            self.identity = Uri(identity, true)
        except e as UriFormatException:
            raise ProtocolException(query, "openid.identity not a valid url ${identity}")

        return_to as string = getField('return_to')
        try:
            self.return_to = Uri(return_to, true)
        except e as UriFormatException:
            raise MalformedReturnUrl(query, return_to)
        
        self.trust_root = query.Get('openid.trust_root')
        if self.trust_root is null:
            self.trust_root = self.return_to.AbsoluteUri
        self.assoc_handle = query.Get('openid.assoc_handle')

        try:
            TrustRoot(self.return_to.AbsoluteUri)
        except e as ArgumentException:
            raise MalformedReturnUrl(query, self.return_to.AbsoluteUri)

        if not self.TrustRootValid:
            raise UntrustedReturnUrl(query, self.return_to, self.trust_root)

    TrustRootValid:
        get:
            if not self.trust_root:
                return true
            tr = TrustRoot(self.trust_root)
            if tr is null:
                raise MalformedTrustRoot(null, self.trust_root)
            return tr.ValidateUrl(self.return_to)

    def Answer(allow as bool, server_url as Uri):
        mode as string
        if allow or self.immediate:
            mode = 'id_res'
        else:
            mode = 'cancel'

        response = Response(self)

        if allow:
            fields = {'mode': mode,
                      'identity': self.identity.AbsoluteUri,
                      'return_to': self.return_to.AbsoluteUri}
            response.AddFields(null, fields, true)
        else:
            response.AddField(null, 'mode', mode, false)
            if self.immediate:
                if server_url is null:
                    raise ApplicationException(
                        "setup_url is required for allow=False " +
                        "in immediate mode.")
                # Make a new request just like me, but with immediate=False.
                setup_request = CheckIdRequest(
                    self.identity, self.return_to, self.trust_root, false,
                    self.assoc_handle)
                setup_url = setup_request.EncodeToUrl(server_url)
                response.AddField(null, 'user_setup_url',
                                  setup_url.AbsoluteUri, false)

        return response

    def EncodeToUrl(server_url as Uri):
        q = NameValueCollection()
        q['openid.mode'] = self.mode
        q['openid.identity'] = self.identity.AbsoluteUri
        q['openid.return_to'] = self.return_to.AbsoluteUri
        if self.trust_root is not null:
            q['openid.trust_root'] = self.trust_root
        if self.assoc_handle is not null:
            q['openid.assoc_handle'] = self.assoc_handle

        builder = UriBuilder(server_url)
        UriUtil.AppendQueryArgs(builder, q)
        return Uri(builder.ToString(), true)

    def GetCancelUrl():
        if self.immediate:
            raise ApplicationException(
                "Cancel is not an appropriate response to " +
                "immediate mode requests.")

        builder = UriBuilder(self.return_to)
        args = NameValueCollection()
        args['openid.mode'] = 'cancel'
        UriUtil.AppendQueryArgs(builder, args)
        
        return Uri(builder.ToString(), true)
