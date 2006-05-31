namespace Janrain.OpenId.Test

import System
import System.Collections
import System.Collections.Specialized
import System.Web
import NUnit.Framework
import Janrain.OpenId
import Janrain.OpenId.Server
import Janrain.OpenId.Store


def nvcEqual(nvc1 as NameValueCollection, nvc2 as NameValueCollection):
    Assert.AreEqual(List(List((k, nvc1[k])) for k as string in nvc1),
                    List(List((k, nvc2[k])) for k as string in nvc2))

def dictToNVC(dict as IDictionary):
    nvc = NameValueCollection()
    for pair as DictionaryEntry in dict:
        nvc.Add(cast(string, pair.Key), cast(string, pair.Value))
    return nvc

def dictEqual(dict1 as IDictionary, dict2 as IDictionary):
    nvcEqual(dictToNVC(dict1), dictToNVC(dict2))


[TestFixture]
class TestProtocolError:
    [Test]
    def BrowserWithReturnTo():
        return_to = "http://rp.unittest/consumer"
        # will be a ProtocolError raised by Decode or CheckIDRequest.answer
        args = {
            'openid.mode': 'monkeydance',
            'openid.identity': 'http://wagu.unittest/',
            'openid.return_to': return_to,
            }
        e = ProtocolException(dictToNVC(args), "plucky")
        Assert.IsTrue(e.HasReturnTo)
        expected_args = {
            'openid.mode': 'error',
            'openid.error': 'plucky',
            }

        Assert.AreEqual(UriUtil.CreateQueryString(dictToNVC(expected_args)),
                        e.EncodeToUrl().Query[1:])

    [Test]
    def NoReturnTo():
        # will be a ProtocolError raised by Decode or CheckIDRequest.answer
        args = {
            'openid.mode': 'zebradance',
            'openid.identity': 'http://wagu.unittest/',
            }
        e = ProtocolException(dictToNVC(args), "waffles")
        Assert.IsFalse(e.HasReturnTo)
        expected = System.Text.Encoding.UTF8.GetBytes(
            "mode:error\nerror:waffles\n")
        actual = e.EncodeToKVForm()
        Assert.AreEqual(expected, actual)


[TestFixture]
class TestDecode:
    ALT_MODULUS = Convert.FromBase64String('AMqt3ewWZ/xotfoV1TxOFTLdJFYaGi1HoSwBq+oeAHMfaSGqxAdCMR/fnmNLtxMb7hryQCYVVDiakQQl4ETojINZsBD1rSuA4pyxpbAnsZ2eAab2Om9F5dftL/aioAhQUKfQzzB8PbUdJJA1WQe0QnwjqY3x64q+8rogm7ev/oan\n')
    ALT_GEN = (of byte: 5)

    id_url as Uri
    rt_url as Uri
    tr_url as string
    assoc_handle as string
    decoder as Decoder
    
    [SetUp]
    def Init():
        self.id_url = Uri("http://decoder.am.unittest/")
        self.rt_url = Uri("http://rp.unittest/foobot/?qux=zam")
        self.tr_url = "http://rp.unittest/"
        self.assoc_handle = "{assoc}{handle}"
        self.decoder = Decoder()

    def decode(args as IDictionary):
        return self.decoder.Decode(dictToNVC(args))

    def failUnlessRaises(type as Type, args as IDictionary):
        try:
            self.decode(args)
        except e as Exception:
            if type.IsInstanceOfType(e):
                return
            
            raise e

        Assert.Fail("Failed to raise ${type}")
    
    [Test]
    def None():
        args = {}
        r = self.decode(args)
        Assert.AreEqual(r, null)

    [Test]
    def Irrelevant():
        args = {
            'pony': 'spotted',
            'sreg.mutant_power': 'decaffinator',
            }
        r = self.decode(args)
        Assert.AreEqual(r, null)

    [Test]
    def Bad():
        args = {
            'openid.mode': 'twos-compliment',
            'openid.pants': 'zippered',
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def CheckidImmediate():
        args = {
            'openid.mode': 'checkid_immediate',
            'openid.identity': self.id_url.AbsoluteUri,
            'openid.assoc_handle': self.assoc_handle,
            'openid.return_to': self.rt_url.AbsoluteUri,
            'openid.trust_root': self.tr_url,
            # should be ignored
            'openid.some.extension': 'junk',
            }
        r = cast(CheckIdRequest, self.decode(args))
        Assert.AreEqual(r.Mode, "checkid_immediate")
        Assert.AreEqual(r.immediate, true)
        Assert.AreEqual(r.identity, self.id_url)
        Assert.AreEqual(r.trust_root, self.tr_url)
        Assert.AreEqual(r.ReturnTo, self.rt_url)
        Assert.AreEqual(r.assoc_handle, self.assoc_handle)

    [Test]
    def CheckidSetup():
        args = {
            'openid.mode': 'checkid_setup',
            'openid.identity': self.id_url.AbsoluteUri,
            'openid.assoc_handle': self.assoc_handle,
            'openid.return_to': self.rt_url.AbsoluteUri,
            'openid.trust_root': self.tr_url,
            }
        r = cast(CheckIdRequest, self.decode(args))
        Assert.AreEqual(r.Mode, "checkid_setup")
        Assert.AreEqual(r.immediate, false)
        Assert.AreEqual(r.identity, self.id_url)
        Assert.AreEqual(r.trust_root, self.tr_url)
        Assert.AreEqual(r.ReturnTo, self.rt_url)

    [Test]
    def CheckidSetupNoIdentity():
        args = {
            'openid.mode': 'checkid_setup',
            'openid.assoc_handle': self.assoc_handle,
            'openid.return_to': self.rt_url.AbsoluteUri,
            'openid.trust_root': self.tr_url,
            }
        try:
            result = self.decode(args)
        except err as ProtocolException:
            Assert.IsNotNull(err.query)
            return

        Assert.Fail(
            "Expected ProtocolError, instead returned with ${result}")


    [Test]
    def CheckidSetupNoReturn():
        args = {
            'openid.mode': 'checkid_setup',
            'openid.identity': self.id_url.AbsoluteUri,
            'openid.assoc_handle': self.assoc_handle,
            'openid.trust_root': self.tr_url,
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def CheckidSetupBadReturn():
        args = {
            'openid.mode': 'checkid_setup',
            'openid.identity': self.id_url.AbsoluteUri,
            'openid.assoc_handle': self.assoc_handle,
            'openid.return_to': 'not a url',
            }
        try:
            result = self.decode(args)
        except err as MalformedReturnUrl:
            Assert.IsTrue(err.query is not null)
            return

        Assert.Fail("Expected ProtocolError, instead returned with ${result}")

    [Test]
    def CheckidSetupUntrustedReturn():
        args = {
            'openid.mode': 'checkid_setup',
            'openid.identity': self.id_url.AbsoluteUri,
            'openid.assoc_handle': self.assoc_handle,
            'openid.return_to': self.rt_url.AbsoluteUri,
            'openid.trust_root': 'http://not-the-return-place.unittest/',
            }
        try:
            result = self.decode(args)
        except err as UntrustedReturnUrl:
            Assert.IsNotNull(err.query)
            return
        
        Assert.Fail(
            "Expected UntrustedReturnUrl, instead returned with ${result}")

    [Test]
    def CheckAuth():
        args = {
            'openid.mode': 'check_authentication',
            'openid.assoc_handle': '{dumb}{handle}',
            'openid.sig': 'sigblob',
            'openid.signed': 'foo,bar,mode',
            'openid.foo': 'signedval1',
            'openid.bar': 'signedval2',
            'openid.baz': 'unsigned',
            }
        r = cast(CheckAuthRequest, self.decode(args))
        Assert.AreEqual(r.Mode, 'check_authentication')
        Assert.AreEqual(r.sig, 'sigblob')
        nvc = NameValueCollection()
        nvc.Add('foo', 'signedval1')
        nvc.Add('bar', 'signedval2')
        nvc.Add('mode', 'id_res')
        nvcEqual(r.signed, nvc)
        # XXX: test error cases (i.e. missing required fields)

    [Test]
    def CheckAuthMissingSignedField():
        args = {
            'openid.mode': 'check_authentication',
            'openid.assoc_handle': '{dumb}{handle}',
            'openid.sig': 'sigblob',
            'openid.signed': 'foo,bar,mode',
            'openid.foo': 'signedval1',
            'openid.baz': 'unsigned',
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def CheckAuthMissingSignature():
        args = {
            'openid.mode': 'check_authentication',
            'openid.assoc_handle': '{dumb}{handle}',
            'openid.signed': 'foo,bar,mode',
            'openid.foo': 'signedval1',
            'openid.bar': 'signedval2',
            'openid.baz': 'unsigned',
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def CheckAuthAndInvalidate():
        args = {
            'openid.mode': 'check_authentication',
            'openid.assoc_handle': '{dumb}{handle}',
            'openid.invalidate_handle': '[[SMART_handle]]',
            'openid.sig': 'sigblob',
            'openid.signed': 'foo,bar,mode',
            'openid.foo': 'signedval1',
            'openid.bar': 'signedval2',
            'openid.baz': 'unsigned',
            }
        r = cast(CheckAuthRequest, self.decode(args))
        Assert.AreEqual(r.invalidate_handle, '[[SMART_handle]]')

    [Test]
    def AssociateDH():
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "Rzup9265tw==",
            }
        r = cast(AssociateRequest, self.decode(args))
        Assert.AreEqual(r.Mode, "associate")
        session = cast(DiffieHellmanServerSession, r.session)
        Assert.AreEqual("DH-SHA1", session.SessionType)
        Assert.AreEqual("HMAC-SHA1", r.assoc_type)
        Assert.IsNotNull(session.consumer_pubkey)

    [Test]
    def AssociateDHMissingKey():
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            }
        # Using DH-SHA1 without supplying dh_consumer_public is an error.
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def AssociateDHpubKeyNotB64():
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "donkeydonkeydonkey",
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def AssociateDHModGen():
        # test dh with non-default but valid values for dh_modulus and dh_gen
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "Rzup9265tw==",
            'openid.dh_modulus': CryptUtil.UnsignedToBase64(ALT_MODULUS),
            'openid.dh_gen': CryptUtil.UnsignedToBase64(ALT_GEN)
            }
        r = cast(AssociateRequest, self.decode(args))
        Assert.AreEqual(r.Mode, "associate")
        Assert.AreEqual(r.session.SessionType, "DH-SHA1")
        Assert.AreEqual(r.assoc_type, "HMAC-SHA1")
        session = cast(DiffieHellmanServerSession, r.session)
        dhparams = session.dh.ExportParameters(false)
        Assert.AreEqual(dhparams.P, ALT_MODULUS[1:])
        Assert.AreEqual(dhparams.G, ALT_GEN)
        Assert.IsNotNull(session.consumer_pubkey)

    [Test]
    def AssociateDHCorruptModGen():
        # test dh with non-default but valid values for dh_modulus and dh_gen
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "Rzup9265tw==",
            'openid.dh_modulus': 'pizza',
            'openid.dh_gen': 'gnocchi',
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def AssociateDHMissingModGen():
        # test dh with non-default but valid values for dh_modulus and dh_gen
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "Rzup9265tw==",
            'openid.dh_modulus': 'pizza',
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def AssociateWeirdSession():
        args = {
            'openid.mode': 'associate',
            'openid.session_type': 'FLCL6',
            'openid.dh_consumer_public': "YQ==\n",
            }
        self.failUnlessRaises(ProtocolException, args)

    [Test]
    def AssociatePlain():
        args = {
            'openid.mode': 'associate',
            }
        r = cast(AssociateRequest, self.decode(args))
        Assert.AreEqual(r.Mode, "associate")
        Assert.AreEqual(r.session.SessionType, "plaintext")
        Assert.AreEqual(r.assoc_type, "HMAC-SHA1")

    [Test]
    def Nomode():
        args = {
            'openid.session_type': 'DH-SHA1',
            'openid.dh_consumer_public': "my public keeey",
            }
        self.failUnlessRaises(ProtocolException, args)


[TestFixture]
class TestEncode:
    encoder as Encoder


    [SetUp]
    def Init():
        self.encoder = Encoder()

    def encode(enc as IEncodable):
        return self.encoder.Encode(enc)

    def failUnlessRaises(type as Type, enc as IEncodable):
        try:
            self.encode(enc)
        except e as Exception:
            if type.IsInstanceOfType(e):
                return
            
            raise e

        Assert.Fail("Failed to raise ${type}")

    [Test]
    def IdRes():
        request = CheckIdRequest(Uri('http://bombom.unittest/'),
                                 Uri('http://burr.unittest/999'),
                                 'http://burr.unittest/', false, null)
        response = Response(request)
        response.fields = {
            'mode': 'id_res',
            'identity': request.identity.AbsoluteUri,
            'return_to': request.ReturnTo.AbsoluteUri,
            }
        webresponse = self.encode(response)
        Assert.AreEqual(HttpCode.HTTP_REDIRECT, webresponse.Code)
        location = webresponse.Headers['location']
        Assert.IsNotNull(location)
        Assert.IsTrue(
            location.StartsWith(request.ReturnTo.AbsoluteUri),
            "${location} does not start with ${request.ReturnTo.AbsoluteUri}")

        nvc = NameValueCollection()
        for pair as DictionaryEntry in response.fields:
            nvc.Add("openid." + pair.Key, pair.Value)
        Assert.AreEqual(UriUtil.CreateQueryString(nvc),
                        Uri(location, true).Query[1:])

    [Test]
    def Cancel():
        request = CheckIdRequest(Uri('http://bombom.unittest/'),
                                 Uri('http://burr.unittest/999'),
                                 'http://burr.unittest/', false, null)
        response = Response(request)
        response.fields = {
            'mode': 'cancel',
            }
        webresponse = self.encode(response)
        Assert.AreEqual(HttpCode.HTTP_REDIRECT, webresponse.Code)
        Assert.IsNotNull(webresponse.Headers['location'])

    [Test]
    def AssocReply():
        request = AssociateRequest(NameValueCollection())
        response = Response(request)
        response.fields = {'assoc_handle': "every-zig"}
        webresponse = self.encode(response)
        body = System.Text.Encoding.UTF8.GetBytes("assoc_handle:every-zig\n")
        Assert.AreEqual(HttpCode.HTTP_OK, webresponse.Code)
        Assert.AreEqual(0, webresponse.Headers.Count)
        Assert.AreEqual(body, webresponse.Body)

    [Test]
    def CheckAuthReply():
        request = CheckAuthRequest('a_sock_monkey', 'siggggg',
                                   NameValueCollection(), null)
        response = Response(request)
        response.fields = {
            'is_valid': 'true',
            'invalidate_handle': 'xXxX:xXXx'
            }
        body = System.Text.Encoding.UTF8.GetBytes(
            "invalidate_handle:xXxX:xXXx\nis_valid:true\n")
        webresponse = self.encode(response)
        Assert.AreEqual(HttpCode.HTTP_OK, webresponse.Code)
        Assert.AreEqual(0, webresponse.Headers.Count)
        Assert.AreEqual(body, webresponse.Body)

    [Test]
    def UnencodableError():
        args = {
            'openid.identity': 'http://limu.unittest/',
            }
        e = ProtocolException(dictToNVC(args), "wet paint")
        self.failUnlessRaises(EncodingException, e)

    [Test]
    def EncodableError():
        args = {
            'openid.mode': 'associate',
            'openid.identity': 'http://limu.unittest/',
            }
        body = System.Text.Encoding.UTF8.GetBytes(
            "mode:error\nerror:snoot\n")
        
        e = ProtocolException(dictToNVC(args), "snoot")
        webresponse = self.encoder.Encode(e)
        Assert.AreEqual(HttpCode.HTTP_ERROR, webresponse.Code)
        Assert.AreEqual(0, webresponse.Headers.Count)
        Assert.AreEqual(body, webresponse.Body)


[TestFixture]
class TestSigningEncode:

    _dumb_key as Uri
    _normal_key as Uri
    store as IAssociationStore
    signatory as Signatory
    request as CheckIdRequest
    response as Response
    encoder as SigningEncoder

    [SetUp]
    def Init():
        self._dumb_key = Signatory._dumb_key
        self._normal_key = Signatory._normal_key
        self.store = TestMemoryStore()

        self.request = CheckIdRequest(Uri('http://bombom.unittest/'),
                                      Uri('http://burr.unittest/999'),
                                      'http://burr.unittest/', false, null)
        self.response = Response(self.request)
        self.response.fields = {
            'mode': 'id_res',
            'identity': self.request.identity.AbsoluteUri,
            'return_to': self.request.ReturnTo.AbsoluteUri,
            }
        self.response.signed += ['mode','identity','return_to']
        self.signatory = Signatory(self.store)
        self.encoder = SigningEncoder(self.signatory)

    def encode(enc as IEncodable):
        return self.encoder.Encode(enc)

    def failUnlessRaises(type as Type, enc as IEncodable):
        try:
            self.encode(enc)
        except e as Exception:
            if type.IsInstanceOfType(e):
                return
            
            raise e

        Assert.Fail("Failed to raise ${type}")

    [Test]
    def IdRes():
        assoc_handle = '{bicycle}{shed}'
        secret = System.Text.Encoding.ASCII.GetBytes('sekrit')
        assoc = HMACSHA1Association(assoc_handle, secret, TimeSpan(0, 0, 60))
        self.store.StoreAssociation(self._normal_key, assoc)
        self.request.assoc_handle = assoc_handle
        webresponse = self.encode(self.response)
        Assert.AreEqual(HttpCode.HTTP_REDIRECT, webresponse.Code)
        Assert.IsNotNull(webresponse.Headers['location'])
        
        qs = (/\?/.Split(webresponse.Headers['location']))[1]
        pairs = List(pair for pair as string in /&/.Split(qs))
        query = List((/=/.Split(pair))[0] for pair in pairs)
        Assert.IsTrue('openid.sig' in query)
        Assert.IsTrue('openid.assoc_handle' in query)
        Assert.IsTrue('openid.signed' in query)

    [Test]
    def IdResDumb():
        webresponse = self.encode(self.response)
        Assert.AreEqual(HttpCode.HTTP_REDIRECT, webresponse.Code)
        Assert.IsNotNull(webresponse.Headers['location'])

        qs = (/\?/.Split(webresponse.Headers['location']))[1]
        pairs = List(pair for pair as string in /&/.Split(qs))
        query = List((/=/.Split(pair))[0] for pair in pairs)
        Assert.IsTrue('openid.sig' in query)
        Assert.IsTrue('openid.assoc_handle' in query)
        Assert.IsTrue('openid.signed' in query)

    [Test]
    def ForgotStore():
        self.encoder.signatory = null
        self.failUnlessRaises(ArgumentException, self.response)

    [Test]
    def Cancel():
        request as CheckIdRequest = CheckIdRequest(
            Uri('http://bombom.unittest/'), Uri('http://burr.unittest/999'),
            'http://burr.unittest/', false, null)

        response as Response = Response(request)
        response.fields['mode'] = 'cancel'
        response.signed = []
        webresponse = self.encode(response)
        Assert.AreEqual(HttpCode.HTTP_REDIRECT, webresponse.Code)
        Assert.IsNotNull(webresponse.Headers['location'])

        query = NameValueCollection()
        qs = (/\?/.Split(webresponse.Headers['location']))[1]
        for pair as string in /&/.Split(qs):
            key, val = /=/.Split(pair)
            query.Add(key, val)

        Assert.IsNull(query['openid.sig'], query['openid.sig'])

    [Test]
    def AssocReply():
        request as AssociateRequest = AssociateRequest(NameValueCollection())
        response as Response = Response(request)
        response.fields = {'assoc_handle': "every-zig"}
        webresponse = self.encode(response)
        body = System.Text.Encoding.ASCII.GetBytes("assoc_handle:every-zig\n")
        Assert.AreEqual(HttpCode.HTTP_OK, webresponse.Code)
        Assert.AreEqual(0, webresponse.Headers.Count)
        Assert.AreEqual(body, webresponse.Body)

    [Test]
    def AlreadySigned():
        self.response.fields['sig'] = 'priorSig=='
        self.failUnlessRaises(Janrain.OpenId.Server.AlreadySignedException,
                              self.response)



[TestFixture]
class TestCheckID():
    request as CheckIdRequest

    [SetUp]
    def Init():
        self.request = CheckIdRequest(Uri('http://bambam.unittest/'),
                                      Uri('http://bar.unittest/999'),
                                      'http://bar.unittest/', false, null)

    [Test]
    def TrustRootInvalid():
        self.request.trust_root = "http://foo.unittest/17"
        self.request.return_to = Uri("http://foo.unittest/39")
        Assert.IsFalse(self.request.TrustRootValid)

    [Test]
    def TrustRootValid():
        self.request.trust_root = "http://foo.unittest/"
        self.request.return_to = Uri("http://foo.unittest/39")
        Assert.IsTrue(self.request.TrustRootValid)

    [Test]
    def AnswerAllow():
        answer = self.request.Answer(true, null)
        Assert.AreEqual(self.request, answer.Request)
        dictEqual({'mode': 'id_res',
                   'identity': self.request.identity.AbsoluteUri,
                   'return_to': self.request.ReturnTo.AbsoluteUri,
                   }, answer.fields)

        Assert.AreEqual(["identity", "mode", "return_to"],
                        List(answer.Signed).Sort())

    [Test]
    def AnswerAllowNoTrustRoot():
        self.request.trust_root = null
        answer = self.request.Answer(true, null)
        Assert.AreEqual(self.request, answer.Request)
        dictEqual({'mode': 'id_res',
                   'identity': self.request.identity.AbsoluteUri,
                   'return_to': self.request.ReturnTo.AbsoluteUri,
                   }, answer.fields)
        Assert.AreEqual(["identity", "mode", "return_to"],
                        List(answer.Signed).Sort())

    [Test]
    def AnswerImmediateDeny():
        self.request.mode = 'checkid_immediate'
        self.request.immediate = true
        server_url = Uri("http://setup-url.unittest/")

        answer = self.request.Answer(false, server_url)
        Assert.AreEqual(self.request, answer.Request)
        Assert.AreEqual(2, answer.fields.Count)
        Assert.AreEqual("id_res", answer.fields['mode'])
        Assert.IsTrue(cast(string, answer.fields['user_setup_url']).StartsWith(
            server_url.AbsoluteUri))
        Assert.AreEqual(0, answer.signed.Count)

    [Test]
    def AnswerSetupDeny():
        answer = self.request.Answer(false, null)
        dictEqual({'mode': 'cancel'}, answer.fields)
        Assert.AreEqual(0, answer.signed.Count)

    [Test]
    def EncodeToUrl():
        server_url = Uri('http://openid-server.unittest.com/')
        result = self.request.EncodeToUrl(server_url)

        # How to check?  How about a round-trip test.
        result_args = NameValueCollection()
        for pair as string in /&/.Split(result.Query[1:]):
            key, val = /=/.Split(pair)
            result_args.Add(HttpUtility.UrlDecode(key),
                            HttpUtility.UrlDecode(val))

        rebuilt_request = CheckIdRequest(result_args)
        Assert.AreEqual(self.request.identity, rebuilt_request.identity)
        Assert.AreEqual(self.request.trust_root, rebuilt_request.trust_root)
        Assert.AreEqual(self.request.mode, rebuilt_request.mode)
        Assert.AreEqual(self.request.return_to, rebuilt_request.return_to)
        Assert.AreEqual(self.request.immediate, rebuilt_request.immediate)

    [Test]
    def GetCancelUrl():
        url = self.request.GetCancelUrl()
        expected = Uri(self.request.ReturnTo.AbsoluteUri +
                       '?openid.mode=cancel', true)
        Assert.AreEqual(expected, url)

    [Test]
    def GetCancelUrlimmed():
        self.request.mode = 'checkid_immediate'
        self.request.immediate = true
        try:
            self.request.GetCancelUrl()
        except e as ApplicationException:
            return

        Assert.Fail("Failed to raise ${ApplicationException}")


[TestFixture]
class TestCheckIdExtension:
    request as CheckIdRequest
    response as Response

    [SetUp]
    def Init():
        self.request = CheckIdRequest(Uri('http://bambam.unittest/'),
                                      Uri('http://bar.unittest/999'),
                                      'http://bar.unittest/', false, null)
        self.response = Response(self.request)
        self.response.fields['mode'] = 'id_res'
        self.response.fields['blue'] = 'star'
        self.response.signed += ['mode','identity','return_to']

    [Test]
    def AddField():
        nmspace = 'mj12'
        self.response.AddField(nmspace, 'bright', 'potato', true)
        expected = {'blue': 'star',
                    'mode': 'id_res',
                    'mj12.bright': 'potato'}
        dictEqual(expected , self.response.fields)
        Assert.AreEqual(['mode', 'identity', 'return_to', 'mj12.bright'],
                        self.response.signed)

    [Test]
    def AddFieldNoNamespace():
        self.response.AddField('', 'dark', 'pages', true)
        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'dark': 'pages'}, self.response.fields)

    [Test]
    def AddFieldUnsigned():
        nmspace = 'mj12'
        self.response.AddField(nmspace, 'dull', 'lemon', false)
        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'mj12.dull': 'lemon'}, self.response.fields)
        Assert.AreEqual(['mode', 'identity', 'return_to'],
                        self.response.signed)

    [Test]
    def AddFields():
        nmspace = 'mi5'
        self.response.AddFields(nmspace,
                                {'tangy': 'suspenders', 'bravo': 'inclusion'},
                                true)

        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'mi5.tangy': 'suspenders',
                   'mi5.bravo': 'inclusion'}, self.response.fields)

        Assert.AreEqual(['mode', 'identity', 'return_to', 'mi5.tangy',
                         'mi5.bravo'], self.response.signed)

    [Test]
    def AddFieldsUnsigned():
        nmspace = 'mi5'
        self.response.AddFields(nmspace,
                                {'strange': 'conditioner',
                                 'elemental': 'blender'},
                                false)

        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'mi5.strange': 'conditioner',
                   'mi5.elemental': 'blender'}, self.response.fields)

        Assert.AreEqual(['mode', 'identity', 'return_to'], self.response.signed)

    [Test]
    def Update():
        eresponse = Response(null)
        eresponse.fields = {'shape': 'heart', 'content': 'strings,wire'}
        eresponse.signed = ['content']
        self.response.Update('box', eresponse)
        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'box.shape': 'heart',
                   'box.content': 'strings,wire'}, self.response.fields)

        Assert.AreEqual(['mode', 'identity', 'return_to', 'box.content'],
                        self.response.signed)

    [Test]
    def UpdateNoNamespace():
        eresponse = Response(null)
        eresponse.fields = {'species': 'pterodactyl', 'saturation': 'day-glo'}
        eresponse.signed = ['species']
        self.response.Update(null, eresponse)
        dictEqual({'blue': 'star',
                   'mode': 'id_res',
                   'species': 'pterodactyl',
                   'saturation': 'day-glo'}, self.response.fields)

        Assert.AreEqual(['mode', 'identity', 'return_to', 'species'],
                        self.response.signed)


class MockSignatory(Signatory):
    public is_valid = true
    public assocs as List
        
    def constructor(assoc as (object)):
        super(TestMemoryStore())
        self.assocs = [assoc]

    override def Verify(assoc_handle as string, sig as string,
                        signed_pairs as NameValueCollection):
        signed_pairs.Clear()
        if (true, assoc_handle) in self.assocs:
            return self.is_valid
        else:
            return false

    override def GetAssociation(assoc_handle as string, dumb as bool):
        if (dumb, assoc_handle) in self.assocs:
            secret = System.Text.Encoding.ASCII.GetBytes('sekrit')
            return HMACSHA1Association(assoc_handle, secret, TimeSpan(0, 0, 60))
        else:
            return null

    override def Invalidate(assoc_handle as string, dumb as bool):
        if (dumb, assoc_handle) in self.assocs:
            self.assocs.Remove((dumb, assoc_handle))


class TestCheckAuth:
    assoc_handle as string
    request as CheckAuthRequest
    signatory as MockSignatory

    [SetUp]
    def Init():
        self.assoc_handle = 'mooooooooo'
        self.request = CheckAuthRequest(
            self.assoc_handle, 'signarture',
            dictToNVC({'one': 'alpha', 'two': 'beta'}),
            null)

        self.signatory = MockSignatory((true, self.assoc_handle))

    [Test]
    def Valid():
        r = self.request.Answer(self.signatory)
        dictEqual({'is_valid': 'true'}, r.fields)
        Assert.AreEqual(self.request, r.Request)

    [Test]
    def Invalid():
        self.signatory.is_valid = false
        r = self.request.Answer(self.signatory)
        dictEqual({'is_valid': 'false'}, r.fields)

    [Test]
    def Replay():
        r = self.request.Answer(self.signatory)
        r = self.request.Answer(self.signatory)
        dictEqual({'is_valid': 'false'}, r.fields)

    [Test]
    def Invalidatehandle():
        self.request.invalidate_handle = "bogusHandle"
        r = self.request.Answer(self.signatory)
        dictEqual({'is_valid': 'true', 'invalidate_handle': "bogusHandle"},
                  r.fields)
        Assert.AreEqual(self.request, r.Request)

    [Test]
    def InvalidatehandleNo():
        assoc_handle = 'goodhandle'
        self.signatory.assocs.Add((false, 'goodhandle'))
        self.request.invalidate_handle = assoc_handle
        r = self.request.Answer(self.signatory)
        dictEqual({'is_valid': 'true'}, r.fields)


class TestAssociate:
    # TODO: test DH with non-default values for modulus and gen.
    # (important to do because we actually had it broken for a while.)
    
    request as AssociateRequest
    store as TestMemoryStore
    signatory as Signatory
    assoc as Association

    [SetUp]
    def Init():
        self.request = AssociateRequest(NameValueCollection())
        self.store = TestMemoryStore()
        self.signatory = Signatory(self.store)
        self.assoc = self.signatory.CreateAssociation(false)

    [Test]
    def Dh():
        dh = CryptUtil.CreateDiffieHellman()
        dhPublic = dh.CreateKeyExchange()
        dhparams = dh.ExportParameters(true)
        cpub = CryptUtil.CreateDiffieHellman()
        nvc = dictToNVC({'openid.dh_consumer_public': cpub})
        session = DiffieHellmanServerSession(nvc)
        self.request = AssociateRequest(NameValueCollection())
        self.request.session = session
        response = self.request.Answer(self.assoc)
        rfg = do(key as string):
            return response.fields[key]

        Assert.AreEqual("HMAC-SHA1", rfg("assoc_type"))
        Assert.AreEqual(self.assoc.Handle, rfg("assoc_handle"))
        Assert.IsNull(rfg("mac_key"))
        Assert.AreEqual("DH-SHA1", rfg("session_type"))
        Assert.IsNotNull(rfg("enc_mac_key"))
        Assert.IsNotNull(rfg("dh_server_public"))

        enc_key = Convert.FromBase64String(rfg("enc_mac_key"))
        spub = Convert.FromBase64String(rfg("dh_server_public"))
        secret = CryptUtil.SHA1XorSecret(dh, spub, enc_key)
        Assert.AreEqual(dhparams.X, secret)

    [Test]
    def Plaintext():
        response = self.request.Answer(self.assoc)
        rfg = do(key as string):
            return response.fields[key]

        Assert.AreEqual("HMAC-SHA1", rfg("assoc_type"))
        Assert.AreEqual(self.assoc.Handle, rfg("assoc_handle"))

        Assert.AreEqual("1209600", rfg("expires_in"))
        Assert.AreEqual(CryptUtil.ToBase64String(self.assoc.Secret),
                        rfg("mac_key"))
        Assert.IsNull(rfg("session_type"))
        Assert.IsNull(rfg("enc_mac_key"))
        Assert.IsNull(rfg("dh_server_public"))


class TestServer:
    store as TestMemoryStore
    server as Janrain.OpenId.Server.Server

    [SetUp]
    def Init():
        self.store = TestMemoryStore()
        self.server = Janrain.OpenId.Server.Server(self.store)

    [Test]
    def Associate():
        request as Request = AssociateRequest(NameValueCollection())
        response = self.server.HandleRequest(request)
        Assert.IsNotNull(response.fields["assoc_handle"])

    [Test]
    def CheckAuth():
        request as Request = CheckAuthRequest('arrrrrf', '0x3999',
                                              NameValueCollection(), null)
        response = self.server.HandleRequest(request)
        Assert.IsNotNull(response.fields["is_valid"])


# class TestSignatory:
#     [SetUp]
#     def Init():
#         self.store = _memstore.MemoryStore()
#         self.signatory = server.Signatory(self.store)
#         self._dumb_key = self.signatory._dumb_key
#         self._normal_key = self.signatory._normal_key

#     def makeAssoc(dumb as bool, lifetime as TimeSpan):
#         if lifetime is null:
#             lifetime = TimeSpan(0, 0, 60)
#         assoc_handle = '{bling}'
#         assoc = association.Association.fromExpiresIn(lifetime, assoc_handle,
#                                                       'sekrit', 'HMAC-SHA1')

#         self.store.storeAssociation((dumb and self._dumb_key) or self._normal_key, assoc)
#         return assoc_handle

#     [Test]
#     def Sign():
#         request = server.OpenIDRequest()
#         assoc_handle = '{assoc}{lookatme}'
#         self.store.storeAssociation(
#             self._normal_key,
#             association.Association.fromExpiresIn(60, assoc_handle,
#                                                   'sekrit', 'HMAC-SHA1'))
#         request.assoc_handle = assoc_handle
#         response = server.OpenIDResponse(request)
#         response.fields = {
#             'foo': 'amsigned',
#             'bar': 'notsigned',
#             'azu': 'alsosigned',
#             }
#         response.signed = ['foo', 'azu']
#         sresponse = self.signatory.sign(response)
#         Assert.AreEqual(sresponse.fields.get('assoc_handle'),
#                              assoc_handle)
#         Assert.AreEqual(sresponse.fields.get('signed'),
#                              'foo,azu')
#         Assert.IsTrue(sresponse.fields.get('sig'))
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def SignDumb():
#         request = server.OpenIDRequest()
#         request.assoc_handle = None
#         response = server.OpenIDResponse(request)
#         response.fields = {
#             'foo': 'amsigned',
#             'bar': 'notsigned',
#             'azu': 'alsosigned',
#             }
#         response.signed = ['foo', 'azu']
#         sresponse = self.signatory.sign(response)
#         assoc_handle = sresponse.fields.get('assoc_handle')
#         Assert.IsTrue(assoc_handle)
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         Assert.IsTrue(assoc)
#         Assert.AreEqual(sresponse.fields.get('signed'),
#                              'foo,azu')
#         Assert.IsTrue(sresponse.fields.get('sig'))
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def SignExpired():
#         request = server.OpenIDRequest()
#         assoc_handle = '{assoc}{lookatme}'
#         self.store.storeAssociation(
#             self._normal_key,
#             association.Association.fromExpiresIn(-10, assoc_handle,
#                                                   'sekrit', 'HMAC-SHA1'))
#         Assert.IsTrue(self.store.getAssociation(self._normal_key, assoc_handle))

#         request.assoc_handle = assoc_handle
#         response = server.OpenIDResponse(request)
#         response.fields = {
#             'foo': 'amsigned',
#             'bar': 'notsigned',
#             'azu': 'alsosigned',
#             }
#         response.signed = ['foo', 'azu']
#         sresponse = self.signatory.sign(response)

#         new_assoc_handle = sresponse.fields.get('assoc_handle')
#         Assert.IsTrue(new_assoc_handle)
#         self.failIfEqual(new_assoc_handle, assoc_handle)

#         Assert.AreEqual(sresponse.fields.get('invalidate_handle'),
#                              assoc_handle)

#         Assert.AreEqual(sresponse.fields.get('signed'),
#                              'foo,azu')
#         Assert.IsTrue(sresponse.fields.get('sig'))

#         # make sure the expired association is gone
#         self.failIf(self.store.getAssociation(self._normal_key, assoc_handle))

#         # make sure the new key is a dumb mode association
#         Assert.IsTrue(self.store.getAssociation(self._dumb_key, new_assoc_handle))
#         self.failIf(self.store.getAssociation(self._normal_key, new_assoc_handle))
#         Assert.IsTrue(self.messages)

#     [Test]
#     def SignInvalidHandle():
#         request = server.OpenIDRequest()
#         assoc_handle = '{bogus-assoc}{notvalid}'

#         request.assoc_handle = assoc_handle
#         response = server.OpenIDResponse(request)
#         response.fields = {
#             'foo': 'amsigned',
#             'bar': 'notsigned',
#             'azu': 'alsosigned',
#             }
#         response.signed = ['foo', 'azu']
#         sresponse = self.signatory.sign(response)

#         new_assoc_handle = sresponse.fields.get('assoc_handle')
#         Assert.IsTrue(new_assoc_handle)
#         self.failIfEqual(new_assoc_handle, assoc_handle)

#         Assert.AreEqual(sresponse.fields.get('invalidate_handle'),
#                              assoc_handle)

#         Assert.AreEqual(sresponse.fields.get('signed'), 'foo,azu')
#         Assert.IsTrue(sresponse.fields.get('sig'))

#         # make sure the new key is a dumb mode association
#         Assert.IsTrue(self.store.getAssociation(self._dumb_key, new_assoc_handle))
#         self.failIf(self.store.getAssociation(self._normal_key, new_assoc_handle))
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def Verify():
#         assoc_handle = '{vroom}{zoom}'
#         assoc = association.Association.fromExpiresIn(60, assoc_handle,
#                                                       'sekrit', 'HMAC-SHA1')

#         self.store.storeAssociation(self._dumb_key, assoc)

#         signed_pairs = [('foo', 'bar'),
#                         ('apple', 'orange')]

#         sig = "Ylu0KcIR7PvNegB/K41KpnRgJl0="
#         verified = self.signatory.verify(assoc_handle, sig, signed_pairs)
#         Assert.IsTrue(verified)
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def VerifyBadSig():
#         assoc_handle = '{vroom}{zoom}'
#         assoc = association.Association.fromExpiresIn(60, assoc_handle,
#                                                       'sekrit', 'HMAC-SHA1')

#         self.store.storeAssociation(self._dumb_key, assoc)

#         signed_pairs = [('foo', 'bar'),
#                         ('apple', 'orange')]

#         sig = "Ylu0KcIR7PvNegB/K41KpnRgJl0=".encode('rot13')
#         verified = self.signatory.verify(assoc_handle, sig, signed_pairs)
#         self.failIf(verified)
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def VerifyBadHandle():
#         assoc_handle = '{vroom}{zoom}'
#         signed_pairs = [('foo', 'bar'),
#                         ('apple', 'orange')]

#         sig = "Ylu0KcIR7PvNegB/K41KpnRgJl0="
#         verified = self.signatory.verify(assoc_handle, sig, signed_pairs)
#         self.failIf(verified)
#         Assert.IsTrue(self.messages)

#     [Test]
#     def GetAssoc():
#         assoc_handle = self.makeAssoc(true)
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         Assert.IsTrue(assoc)
#         Assert.AreEqual(assoc.handle, assoc_handle)
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def GetAssocExpired():
#         assoc_handle = self.makeAssoc(true, TimeSpan(0, 0, -10))
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         self.failIf(assoc, assoc)
#         Assert.IsTrue(self.messages)

#     [Test]
#     def GetAssocInvalid():
#         ah = 'no-such-handle'
#         Assert.AreEqual(
#             self.signatory.getAssociation(ah, false), None)
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def GetAssocDumbVsNormal():
#         assoc_handle = self.makeAssoc(true)
#         Assert.AreEqual(
#             self.signatory.getAssociation(assoc_handle, false), None)
#         self.failIf(self.messages, self.messages)

#     [Test]
#     def CreateAssociation():
#         assoc = self.signatory.createAssociation(false)
#         Assert.IsTrue(self.signatory.getAssociation(assoc.handle, false))
#         self.failIf(self.messages, self.messages)


#     def invalidate():
#         assoc_handle = '-squash-'
#         assoc = association.Association.fromExpiresIn(60, assoc_handle,
#                                                       'sekrit', 'HMAC-SHA1')

#         self.store.storeAssociation(self._dumb_key, assoc)
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         Assert.IsTrue(assoc)
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         Assert.IsTrue(assoc)
#         self.signatory.invalidate(assoc_handle, true)
#         assoc = self.signatory.getAssociation(assoc_handle, true)
#         self.failIf(assoc)
#         self.failIf(self.messages, self.messages)
