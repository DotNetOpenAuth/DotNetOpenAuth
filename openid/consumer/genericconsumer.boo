namespace Janrain.OpenId.Consumer

import System
import System.Collections
import System.Collections.Specialized
import System.Net
import System.Security.Cryptography
import System.Text
import Mono.Security.Cryptography
import Janrain.OpenId
import Janrain.OpenId.Store


class GenericConsumer:
    private static TOKEN_LIFETIME as uint = 120

    private static final DH_SHA1 = "DH-SHA1"
    private static final HMAC_SHA1 = "HMAC-SHA1"

    private store as IAssociationStore
    private fetcher as Fetcher

    def constructor(store as IAssociationStore, fetcher as Fetcher):
        self.store = store
        self.fetcher = fetcher

    def Begin(service_endpoint as ServiceEndpoint):
        nonce = CryptUtil.CreateNonce()
        token = GenToken(service_endpoint)
        assoc = GetAssociation(service_endpoint.ServerUrl)
        request = AuthRequest(token, assoc, service_endpoint)
        request.ReturnToArgs.Add('nonce', nonce)
        return request


    def Complete(query as IDictionary, token as string):
        mode as string = query['openid.mode']
        if mode is null:
            mode = "<no mode specified>"

        identity_url as Uri
        server_id as Uri
        server_url as Uri

        pieces = SplitToken(token)
        if pieces is not null:
            identity_url, server_id, server_url = pieces

        if mode == 'cancel':
            raise CancelException(identity_url)
        elif mode == 'error':
            error = query['openid.error']
            raise FailureException(identity_url, error)
        elif mode == 'id_res':
            if identity_url is null:
                raise FailureException(identity_url, 'No session state found')

            response = DoIdRes(query, identity_url, server_id, server_url)
            CheckNonce(response, query['nonce'])
            return response
        else:
            raise FailureException(identity_url, 'Invalid openid.mode: ${mode}')

    private def CheckNonce(response as ConsumerResponse, nonce as string):
        for part in /&/.Split(response.ReturnTo.Query[1:]):
            key, val = /=/.Split(part)
            if key == "nonce":
                if val != nonce:
                    raise FailureException(response.IdentityUrl,
                                           "Nonce mismatch")
                else:
                    return

        raise FailureException(response.IdentityUrl,
                               "Nonce missing from return_to: " +
                               response.ReturnTo.AbsoluteUri)

    private def MakeKVPost(args as NameValueCollection, server_url as Uri):
        body = ASCIIEncoding.ASCII.GetBytes(UriUtil.CreateQueryString(args))

        try:
            return self.fetcher.Post(server_url, body)
        except e as FetchException:
            if e.response is null:
                # XXX: log network failure
                pass
            elif e.response.code == HttpStatusCode.BadRequest:
                # XXX: log this
                pass
            else:
                # XXX: log this
                pass

            return null

    private def DoIdRes(query as IDictionary, consumer_id as Uri,
                        server_id as Uri, server_url as Uri):
        
        getRequired = do(key as string):
            val = cast(string, query["openid." + key])
            if val is null:
                msg = 'Missing required field: ${key}'
                raise FailureException(consumer_id, msg)
            return val

        user_setup_url as string = query['openid.user_setup_url']
        if user_setup_url is not null:
            raise SetupNeededException(consumer_id, Uri(user_setup_url))

        return_to = getRequired('return_to')
        server_id2 = getRequired('identity')
        assoc_handle = getRequired('assoc_handle')

        if server_id.AbsoluteUri != server_id:
            raise FailureException(consumer_id, 'Server ID (delegate) mismatch')

        assoc = self.store.GetAssociation(server_url, assoc_handle)
        if assoc is null:
            # It's not an association we know about.  Dumb mode is our
            # only possible path for recovery.
            if not CheckAuth(query, server_url):
                raise FailureException(consumer_id,
                                       'check_authentication failed')
                
            return ConsumerResponse(consumer_id, query, query['openid.signed'])
                
        if assoc.ExpiresIn <= 0:
            msg = 'Association with ${server_url} expired'
            raise FailureException(consumer_id, msg)

        # Check the signature
        sig = getRequired('sig')
        signed = getRequired('signed')
        signed_array = /,/.Split(signed)
        v_sig = assoc.SignDict(signed_array, query, 'openid.')

        if v_sig != sig:
            raise FailureException(consumer_id, 'Bad signature')

        return ConsumerResponse(consumer_id, query, signed)


    private def CheckAuth(query as IDictionary, server_url as Uri):
        request = CreateCheckAuthRequest(query)
        if request is null:
            return false

        response = MakeKVPost(request, server_url)
        if response is null:
            return false

        return ProcessCheckAuthResponse(response, server_url)


    private def CreateCheckAuthRequest(query as IDictionary):
        signed = query['openid.signed']
        if signed is null:
            #XXX: oidutil.log('No signature present; checkAuth aborted')
            return null

        # Arguments that are always passed to the server and not
        # included in the signature.
        whitelist = (of string: 'assoc_handle', 'sig', 'signed', 'invalidate_handle')
        signed_array = /,/.Split(signed) + whitelist

        check_args = NameValueCollection()
        for pair as DictionaryEntry in query:
            key = cast(string, pair.Key)
            if key.StartsWith('openid.') and key[7:] in signed_array:
                check_args.Add(key, pair.Value)

        check_args['openid.mode'] = 'check_authentication'
        return check_args


    private def ProcessCheckAuthResponse(response as IDictionary,
                                         server_url as Uri):
        is_valid as string = response['is_valid']
        if is_valid == 'true':
            invalidate_handle as string = response['invalidate_handle']
            if invalidate_handle is not null:
                self.store.RemoveAssociation(server_url, invalidate_handle)
            
            return true

        # XXX: Log this
        return false


    private def GenToken(endpoint as ServiceEndpoint):
        sep = (of byte: 0,)
        timestamp = DateTime.UtcNow.ToFileTimeUtc().ToString()
        suffix = (ASCIIEncoding.ASCII.GetBytes(timestamp) + sep +
                  ASCIIEncoding.ASCII.GetBytes(endpoint.IdentityUrl.AbsoluteUri)
                  + sep +
                  ASCIIEncoding.ASCII.GetBytes(endpoint.ServerId.AbsoluteUri) +
                  sep + 
                  ASCIIEncoding.ASCII.GetBytes(endpoint.ServerUrl.AbsoluteUri))
        
        hmac = HMACSHA1(self.store.AuthKey)
        prefix = hmac.ComputeHash(suffix)
        return CryptUtil.ToBase64String(prefix + suffix)


    private def SplitToken(token as string):
        tok = Convert.FromBase64String(token)
        if tok.Length < 20:
            # XXX: log this
            return null

        sig = tok[:20]

        hmac = HMACSHA1(self.store.AuthKey)
        if hmac.ComputeHash(tok[20:]) != sig:
            # XXX: log this
            return null

        SplitArray = do():
            chunks = []
            prev = 20
            delim as byte = 0
            while (idx = Array.IndexOf(tok, delim, prev)) != -1:
                chunks.Add(ASCIIEncoding.ASCII.GetString(
                        tok, prev, idx - prev))
                prev = idx + 1

            chunks.Add(ASCIIEncoding.ASCII.GetString(
                        tok, prev, len(tok) - prev))
            return chunks

        strs = SplitArray()

        # Check if timestamp has expired
        ts = DateTime.FromFileTimeUtc(Convert.ToInt64(strs.Pop(0)))
        ts += TimeSpan(0, 0, cast(int, TOKEN_LIFETIME))
        if ts < DateTime.UtcNow:
            # XXX: log this
            return null

        for i, s in enumerate(strs):
            try:
                strs[i] = Uri(s)
            except e as UriFormatException:
                return null

        return strs

    private def GetAssociation(server_url as Uri):
        if self.store.IsDumb:
            return null

        assoc = self.store.GetAssociation(server_url)
        if (assoc is null) or (assoc.ExpiresIn < TOKEN_LIFETIME):
            dh as DiffieHellman, args = CreateAssociationRequest(server_url)

            response = MakeKVPost(args, server_url)
            if response is null:
                assoc = null
            else:
                assoc = ParseAssociation(response, dh, server_url)

        return assoc

    private static def CreateAssociationRequest(server_url as Uri):
        args = NameValueCollection()
        args.Add('openid.mode', 'associate')
        args.Add('openid.assoc_type', HMAC_SHA1)

        if server_url.Scheme != Uri.UriSchemeHttps:
            # Initiate Diffie-Hellman Exchange
            dh = CryptUtil.CreateDiffieHellman()
            dhPublic = dh.CreateKeyExchange()
            cpub = CryptUtil.UnsignedToBase64(dhPublic)
            args.Add('openid.session_type', DH_SHA1)
            args.Add('openid.dh_consumer_public', cpub)

            dhps = dh.ExportParameters(true)
            if (dhps.P != CryptUtil.DEFAULT_MOD or
                dhps.G != CryptUtil.DEFAULT_GEN):
                args.Add('openid.dh_modulus',
                         CryptUtil.UnsignedToBase64(dhps.P))
                args.Add('openid.dh_gen', CryptUtil.UnsignedToBase64(dhps.G))
        
        return (dh, args)


    protected class MissingParameterException(ApplicationException):
        def constructor(key as string):
            super('Query missing key: ${key}')

    protected def ParseAssociation(resp as FetchResponse, dh as DiffieHellman,
                                   server_url as Uri):
        results = KVUtil.KVToDict(resp.data)

        getParameter = do(key as string):
            val as string = results[key]
            if val is null:
                raise MissingParameterException(
                    "Query args missing key: ${key}")
            return val

        getDecoded = do(key as string):
            try:
                return Convert.FromBase64String(getParameter(key))
            except e as FormatException:
                raise MissingParameterException(
                    "Query argument ${key} not Base64")

        try:
            if getParameter('assoc_type') != HMAC_SHA1:
                # XXX: log this
                return null

            session_type as string = results['session_type']
            if session_type is null:
                secret = getDecoded("mac_key")
            elif session_type == DH_SHA1:
                dh_server_public = getDecoded("dh_server_public")
                enc_mac_key = getDecoded("enc_mac_key")
                secret = CryptUtil.SHA1XorSecret(
                    dh, dh_server_public, enc_mac_key)
            else:
                # XXX: log this
                return null
            
            assocHandle = getParameter('assoc_handle')
            expiresIn = TimeSpan(0, 0, Convert.ToInt32(
                    getParameter('expires_in')))
            assoc = HMACSHA1Association(assocHandle, secret, expiresIn)
            self.store.StoreAssociation(server_url, assoc)
            return assoc
        except e as MissingParameterException:
            # XXX: log this
            return null

