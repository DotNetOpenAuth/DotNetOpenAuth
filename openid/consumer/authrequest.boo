namespace Janrain.OpenId.Consumer

import System
import System.Collections.Specialized
import Janrain.OpenId


class AuthRequest:
    enum Mode:
        IMMEDIATE
        SETUP

    [Property(Token)]
    token as string

    [Getter(ExtraArgs)]
    extra_args as NameValueCollection

    [Getter(ReturnToArgs)]
    return_to_args as NameValueCollection

    assoc as Association
    endpoint as ServiceEndpoint

    def constructor(token as string, assoc as Association,
                    endpoint as ServiceEndpoint):
        self.token = token
        self.assoc = assoc
        self.endpoint = endpoint

        self.extra_args = NameValueCollection()
        self.return_to_args = NameValueCollection()
        
        
    def CreateRedirect(trust_root as string, return_to as Uri, mode as Mode):
        if mode == Mode.IMMEDIATE:
            mode_str = 'checkid_immediate'
        elif mode == Mode.SETUP:
            mode_str = 'checkid_setup'

        rto_bldr = UriBuilder(return_to)
        UriUtil.AppendQueryArgs(rto_bldr, self.return_to_args)

        qs_args = NameValueCollection()
        qs_args.Add('openid.mode', mode_str)
        qs_args.Add('openid.identity', self.endpoint.ServerId.AbsoluteUri)
        qs_args.Add('openid.return_to',
                    Uri(rto_bldr.ToString(), true).AbsoluteUri)
        qs_args.Add('openid.trust_root', trust_root)

        if self.assoc is not null:
            qs_args.Add('openid.assoc_handle', self.assoc.Handle)

        redir = UriBuilder(self.endpoint.ServerUrl)
        UriUtil.AppendQueryArgs(redir, qs_args)
        UriUtil.AppendQueryArgs(redir, self.extra_args)
        return Uri(redir.ToString(), true)
