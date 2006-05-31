namespace Janrain.OpenId.Test

import System
import System.Collections
import System.Collections.Specialized
import System.IO
import System.Net
import System.Security.Cryptography
import System.Text
import System.Web
import Mono.Security.Cryptography
import NUnit.Framework
import Janrain.OpenId
import Janrain.OpenId.Consumer
import Janrain.OpenId.Store

class TestMemoryStore(IAssociationStore):

    serverAssocs as Hashtable
    nonces as Hashtable
    authKey as (byte) = array(byte, 20)

    def constructor():
        self.serverAssocs = Hashtable()
        self.nonces = Hashtable()
        RNGCryptoServiceProvider().GetBytes(self.authKey)

    AuthKey as (byte):
        get:
            return cast((byte), self.authKey.Clone())

    IsDumb as bool:
        get:
            return false

    private def GetServerAssocs(server_url as Uri) as ServerAssocs:
        if serverAssocs[server_url] is null:
            serverAssocs.Add(server_url, ServerAssocs())
        return cast(ServerAssocs, serverAssocs[server_url])

    def StoreAssociation(server_url as Uri, assoc as Association):
        assocs as ServerAssocs = GetServerAssocs(server_url)
        assocs.Set(cast(Association, assoc.Clone()))

    def GetAssociation(server_url as Uri) as Association:
        return GetServerAssocs(server_url).Best()

    def GetAssociation(server_url as Uri, handle as string) as Association:
        return GetServerAssocs(server_url).Get(handle)

    def RemoveAssociation(server_url as Uri, handle as string) as bool:
        return GetServerAssocs(server_url).Remove(handle)

    def StoreNonce(nonce as string):
        self.nonces[nonce] = 0

    def UseNonce(nonce as string) as bool:
        ret as bool = (self.nonces[nonce] is not null)
        self.nonces.Remove(nonce)
        return ret

    private class ServerAssocs:

        assocs as Hashtable

        def constructor():
            self.assocs = Hashtable()

        def Set(assoc as Association):
            self.assocs.Add(assoc.Handle, assoc)

        def Get(handle as string) as Association:
            return cast(Association, self.assocs[handle])

        def Remove(handle as string) as bool:
            ret as bool = (self.assocs[handle] is null)
            self.assocs.Remove(handle)
            return ret

        def Best() as Association:
            best as Association = null
            for assoc as Association in self.assocs.Values:
                if (best is null) or (best.Issued < assoc.Issued):
                    best = assoc
            return best


internal class AssociationInfo:

    public secret as (byte)

    public handle as string

    public def constructor(secret as (byte), handle as string):
        self.secret = secret
        self.handle = handle

internal class FormParser:

    private def constructor():
        pass

    private static def AddRawKeyValue(key as StringBuilder, val as StringBuilder, form as NameValueCollection):
        form.Add(HttpUtility.UrlDecode(key.ToString(), Encoding.UTF8), HttpUtility.UrlDecode(val.ToString(), Encoding.UTF8))
        key.Length = 0
        val.Length = 0

    public static def Parse(data as (byte)) as NameValueCollection:
        form = NameValueCollection()
        input as Stream = MemoryStream(data)
        s = StreamReader(input, Encoding.UTF8)
        key = StringBuilder()
        value = StringBuilder()
        c as int
        while (c = s.Read()) != (-1):
            if c == char('='):
                value.Length = 0
                while (c = s.Read()) != (-1):
                    if c == char('&'):
                        AddRawKeyValue(key, value, form)
                        break 
                    else:
                        value.Append(cast(char, c))
                if c == (-1):
                    AddRawKeyValue(key, value, form)
                    return form
            else:
                if c == char('&'):
                    AddRawKeyValue(key, value, form)
                else:
                    key.Append(cast(char, c))
        if c == (-1):
            AddRawKeyValue(key, value, form)
        return form

internal class TestFetcher(Fetcher):

    public getResponses as Hashtable

    private assoc as AssociationInfo

    public def constructor(user_url as Uri, userPage as string,
                           ainfo as AssociationInfo):
        self.assoc = ainfo
        self.getResponses = Hashtable()
        data as (byte) = Encoding.UTF8.GetBytes(userPage)
        resp = FetchResponse(HttpStatusCode.OK, user_url, 'UTF-8', data, data.Length)
        self.getResponses.Add(user_url, resp)

    private def Associate(data as (byte)) as (byte):
        q as NameValueCollection = FormParser.Parse(data)
        Assert.AreEqual(q.Count, 6)
        Assert.AreEqual(q['openid.mode'], 'associate')
        Assert.AreEqual(q['openid.assoc_type'], 'HMAC-SHA1')
        Assert.AreEqual(q['openid.session_type'], 'DH-SHA1')
        d = DiffieHellmanManaged(
            Convert.FromBase64String(q['openid.dh_modulus']),
            Convert.FromBase64String(q['openid.dh_gen']), 1024)
        enc_mac_key = CryptUtil.SHA1XorSecret(
            d, Convert.FromBase64String(q['openid.dh_consumer_public']),
            self.assoc.secret)
        dh_public = d.CreateKeyExchange()
        spub  = CryptUtil.UnsignedToBase64(dh_public)
        reply = {'assoc_type': 'HMAC-SHA1',
                 'assoc_handle': self.assoc.handle,
                 'expires_in': '600',
                 'session_type': 'DH-SHA1',
                 'dh_server_public': spub,
                 'enc_mac_key': CryptUtil.ToBase64String(enc_mac_key)}
        return KVUtil.DictToKV(reply)

    private def Response(url as Uri, data as (byte)) as FetchResponse:
        if data is null:
            raise FetchException(FetchResponse(HttpStatusCode.NotFound, url, 'UTF-8', array(byte, 0), 0), 'Not Found')
        return FetchResponse(HttpStatusCode.OK, url, 'UTF-8', data, data.Length)

    #region Fetcher Members
    public override def Get(uri as Uri, maxRead as uint) as FetchResponse:
        ret = cast(FetchResponse, self.getResponses[uri])
        if ret is null:
            ret = Response(uri, null)
        return ret

    public override def Post(uri as Uri, body as (byte), maxRead as uint) as FetchResponse:
        if Encoding.UTF8.GetString(body).IndexOf('openid.mode=associate') < 0:
            return Response(uri, null)
        return Response(uri, Associate(body))
    #endregion

internal class BadFetcher(Fetcher):

    private resp as FetchResponse

    private message as string

    public def constructor(resp as FetchResponse):
        self.resp = resp
        self.message = 'barf'

    public override def Get(uri as Uri, maxRead as uint) as FetchResponse:
        raise FetchException(self.resp, self.message)

    public override def Post(uri as Uri, body as (byte), maxRead as uint) as FetchResponse:
        raise FetchException(self.resp, self.message)

[TestFixture]
public class ConsumerTestSuite:

    private static USER_PAGE_PAT = '<html>\r\n  <head>\r\n    <title>A user page</title>\r\n    {0}\r\n  </head>\r\n  <body>\r\n    blah blah\r\n  </body>\r\n</html>'

    private static server_url = Uri('http://server.example.com/')

    private static consumer_url = Uri('http://consumer.example.com/')

    private def Success(user_url as Uri, delegate_url as Uri, links as string,
                        immediate as AuthRequest.Mode):
        store = TestMemoryStore()
        mode as string
        if immediate == AuthRequest.Mode.IMMEDIATE:
            mode = 'checkid_immediate'
        else:
            mode = 'checkid_setup'

        endpoint = ServiceEndpoint(user_url, server_url,
                                   (ServiceEndpoint.OPENID_1_2_TYPE,),
                                   delegate_url, true)

        userPage as string = String.Format(USER_PAGE_PAT, links)
        test_handle = 'Snarky'
        info = AssociationInfo(Encoding.ASCII.GetBytes('another 20-byte key.'),
                               test_handle)
        fetcher = TestFetcher(user_url, userPage, info)
        consumer = GenericConsumer(store, fetcher)
        request = consumer.Begin(endpoint)
        return_to = Uri(consumer_url.AbsoluteUri, true)
        trust_root = consumer_url.AbsoluteUri
        redirect_url = request.CreateRedirect(trust_root, return_to, immediate)
        q = FormParser.Parse(Encoding.UTF8.GetBytes(
                redirect_url.Query.Substring(1)))
        errmsg = redirect_url.AbsoluteUri

        redirect_bldr = UriBuilder(redirect_url.AbsoluteUri)
        
        for pair as string in /&/.Split(redirect_bldr.Query):
            key, val = /=/.Split(pair) 
            if key == "openid.return_to":
                new_return_to = HttpUtility.UrlDecode(val)

        #Assert.AreEqual(q.Count, 5, errmsg)
        Assert.AreEqual(q['openid.mode'], mode, errmsg)
        Assert.AreEqual(q['openid.identity'], delegate_url.AbsoluteUri, errmsg)
        Assert.AreEqual(q['openid.trust_root'], trust_root, errmsg)
        Assert.AreEqual(q['openid.assoc_handle'], test_handle, errmsg)
        Assert.IsTrue(new_return_to.StartsWith(return_to.AbsoluteUri), errmsg)
        Assert.IsTrue(redirect_url.AbsoluteUri.StartsWith(
                server_url.AbsoluteUri), errmsg)
        query = {'nonce': request.ReturnToArgs['nonce'],
                 'openid.mode': 'id_res',
                 'openid.return_to': new_return_to,
                 'openid.identity': delegate_url.AbsoluteUri,
                 'openid.assoc_handle': test_handle}
        assoc as Association = store.GetAssociation(server_url, test_handle)
        sig = assoc.SignDict(
            (of string: 'mode', 'return_to', 'identity'), query, 'openid.')

        query.Add('openid.sig', sig)
        query.Add('openid.signed', 'mode,return_to,identity')
        result = consumer.Complete(query, request.Token)
        Assert.AreEqual(
            result.IdentityUrl.AbsoluteUri, user_url.AbsoluteUri,
            "identity_url:${result.IdentityUrl}\nuser_url:${user_url}")

    [Test]
    public def Success():
        user_url = Uri('http://www.example.com/user.html')
        links = "<link rel=\"openid.server\" href=\"${server_url}\" />"
        delegate_url = Uri('http://consumer.example.com/user')
        delegate_links = String.Format('<link rel="openid.server" href="{0}" />\r\n                <link rel="openid.delegate" href="{1}" />', server_url, delegate_url)
        Success(user_url, user_url, links, AuthRequest.Mode.SETUP)
        Success(user_url, user_url, links, AuthRequest.Mode.IMMEDIATE)
        Success(user_url, delegate_url, delegate_links, AuthRequest.Mode.SETUP)
        Success(user_url, delegate_url, delegate_links,
                AuthRequest.Mode.IMMEDIATE)

#     [Test]
#     public def BadFetch():
#         user_url = Uri('http://who.cares/')
#         store as IAssociationStore = MemoryStore()
#         consumer as Consumer
#         cases = ArrayList()
#         cases.Add(null)
#         cases.Add(HttpStatusCode.NotFound)
#         cases.Add(HttpStatusCode.BadRequest)
#         cases.Add(HttpStatusCode.InternalServerError)
#         data as (byte) = Encoding.UTF8.GetBytes('Who cares?')
#         resp as FetchResponse
#         for code as object in cases:
#             if code is null:
#                 resp = null
#             else:
#                 resp = FetchResponse(cast(HttpStatusCode, code), user_url, 'UTF-8', data, data.Length)
#             consumer = Consumer(store, BadFetcher(resp))
#             try:
#                 consumer.BeginAuth(user_url)
#                 TestTools.Assert(false, String.Format('Consumer failed to raise FetchException: {0}', code.ToString()))
#             except e as FetchException:
#                 pass

#     [Test]
#     public def BadParse():
#         store as IAssociationStore = MemoryStore()
#         user_url = Uri('http://user.example.com/')
#         cases as (string) = ('', 'http://not.in.a.link.tag/', '<link rel="openid.server" href="not.in.html.or.head" />')
#         fetcher as Fetcher
#         consumer as GenericConsumer
#         for userPage as string in cases:
#             fetcher = TestFetcher(user_url, userPage, null)
#             consumer = GenericConsumer(store, fetcher)
#             try:
#                 consumer.Begin(user_url)
#                 TestTools.Assert(false, String.Format('Shouldn\'t have succeeded with user_page=[{0}]', userPage))
#             except e as ParseException:
#                 pass

#     private class TestConsumer(Consumer):

#         private static CONSUMER_X = 'x/a0BNdiZWTdmJDCgDrjsPZFtIOaSMEi16u0W5LkExC3L+GHPbnJkFu/jjRTZXp5Lb7Q6FdaAonAgdpFRQbo7I8XdHrdCulFuz9+hv0mn5eqGamB27MosGZcZaNwhSNyTT6KY4DEpwX6ohlRxbofZWT7CFNAzUW8ike3/N/OgTA='

#         private static CONSUMER_SPUB = 'AMwsoRFdgWDxtKRX40foZBCtnd50JT7+/MZcp6g3BNlwzz+4DN7eI5XQaqF52OKkDZPIy/2L/7PVAMhYxotXFHWyLprWoDELijzy6JmlqYDwK1UOmNqdzWo/mH+0PREjt1FbQfkda1YXcy10vLuFWfiIMHhCHew+uq9E9D8ErUEu'

#         private static CONSUMER_ENC_MAC_KEY = '/8e7cN5TYwWibiKdvlhgx/q3Zq0='

#         public def constructor(store as IAssociationStore, fetcher as Fetcher):
#             super(store, fetcher)

#         public def TestParseAssociation() as Association:
#             results = NameValueCollection()
#             results.Add('assoc_type', 'HMAC-SHA1')
#             results.Add('assoc_handle', 'myhandle')
#             results.Add('session_type', 'DH-SHA1')
#             results.Add('dh_server_public', CONSUMER_SPUB)
#             results.Add('enc_mac_key', CONSUMER_ENC_MAC_KEY)
#             results.Add('expires_in', '600')
#             dh as DiffieHellman = DiffieHellmanManaged(CryptUtil.DEFAULT_MOD, CryptUtil.DEFAULT_GEN, Convert.FromBase64String(CONSUMER_X))
#             return ParseAssociation(results, dh, Uri('http://www.google.com/'))

#     private class DumbStore(IAssociationStore):

#         public def constructor():
#             pass

#         public AuthKey as (byte):
#             get:
#                 raise NotImplementedException()

#         public IsDumb as bool:
#             get:
#                 return true

#         public def StoreAssociation(server_url as Uri, assoc as Association):
#             pass

#         public def GetAssociation(server_url as Uri) as Association:
#             raise NotImplementedException()

#         public def GetAssociation(server_url as Uri, handle as string) as Association:
#             raise NotImplementedException()

#         public def RemoveAssociation(server_url as Uri, handle as string) as bool:
#             raise NotImplementedException()

#         public def StoreNonce(nonce as string):
#             raise NotImplementedException()

#         public def UseNonce(nonce as string) as bool:
#             raise NotImplementedException()

#     private static SERVER_SECRET = 'Md7XQVWxUOidZXpGWVmHFgKYqZs='

#     [Test]
#     public def ParseAssociation():
#         store as IAssociationStore = DumbStore()
#         consumer = TestConsumer(store, SimpleFetcher())
#         assoc as Association = consumer.TestParseAssociation()
#         if assoc is null:
#             TestTools.Assert(false, 'TestParseAssociation return a null association')
#         else:
#             result as string = CryptUtil.ToBase64String(assoc.Secret)
#             TestTools.Assert((SERVER_SECRET == result), result)

