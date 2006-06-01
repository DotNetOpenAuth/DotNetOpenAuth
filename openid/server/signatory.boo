namespace Janrain.OpenId.Server

import System
import System.Collections
import System.Collections.Specialized
import System.Security.Cryptography
import Janrain.OpenId
import Janrain.OpenId.Store

class Signatory:
     # 14 days, in seconds
    public final static SECRET_LIFETIME = TimeSpan(0, 0, 14 * 24 * 60 * 60)

    # keys have a bogus server URL in them because the filestore
    # really does expect that key to be a URL.  This seems a little
    # silly for the server store, since I expect there to be only one
    # server URL.
    internal static final _normal_key = Uri('http://localhost/|normal')
    internal static final _dumb_key = Uri('http://localhost/|dumb')

    store as IAssociationStore

    def constructor(store as IAssociationStore):
        if store is null:
            raise ArgumentNullException("store")
        self.store = store

    virtual def Verify(assoc_handle as string, sig as string,
               signed_pairs as NameValueCollection):
        assoc = self.GetAssociation(assoc_handle, true)
        if assoc is null:
            #XXX: log this
            return false

        # Not using Association.CheckSignature here is intentional;
        # Association should not know things like "the list of signed pairs is
        # in the request's 'signed' parameter and it is comma-separated."
        expected_sig = CryptUtil.ToBase64String(assoc.Sign(signed_pairs))

        return sig == expected_sig

    def Sign(response as Response):
        assoc_handle = cast(AssociatedRequest, response.Request).assoc_handle
        if assoc_handle:
            # normal mode
            assoc = self.GetAssociation(assoc_handle, false)
            if assoc is null:
                # fall back to dumb mode
                response.Fields['invalidate_handle'] = assoc_handle
                assoc = self.CreateAssociation(true)
        else:
            # dumb mode.
            assoc = self.CreateAssociation(true)

        response.Fields['assoc_handle'] = assoc.Handle
        nvc = NameValueCollection()
        for pair as DictionaryEntry in response.Fields:
            nvc.Add(pair.Key, pair.Value)
        sig = assoc.SignDict(response.Signed, nvc, '')
        signed = String.Join(',', response.Signed)
        response.Fields['sig'] = sig
        response.Fields['signed'] = signed

    def CreateAssociation(dumb as bool):
        generator = RNGCryptoServiceProvider()
        secret = array(byte, 20)
        generator.GetBytes(secret)
        uniq_bytes = array(byte, 4)
        generator.GetBytes(uniq_bytes)
        uniq = CryptUtil.ToBase64String(uniq_bytes)
        seconds = (DateTime.Now - date(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds
        handle = "{{HMAC-SHA1}{${seconds}}{${uniq}}"

        assoc = HMACSHA1Association(handle, secret, self.SECRET_LIFETIME)
        if dumb:
            key = self._dumb_key
        else:
            key = self._normal_key
        self.store.StoreAssociation(key, assoc)
        return assoc

    virtual def GetAssociation(assoc_handle as string, dumb as bool):
        if assoc_handle is null:
            raise ArgumentNullException("assoc_handle")

        if dumb:
            key = self._dumb_key
        else:
            key = self._normal_key

        assoc = self.store.GetAssociation(key, assoc_handle)
        if assoc is not null and assoc.ExpiresIn <= 0:
            # XXX: log this
            self.store.RemoveAssociation(key, assoc_handle)
            assoc = null
        return assoc

    virtual def Invalidate(assoc_handle as string, dumb as bool):
        if dumb:
            key = self._dumb_key
        else:
            key = self._normal_key

        self.store.RemoveAssociation(key, assoc_handle)
