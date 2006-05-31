namespace Janrain.OpenId.Server

import System.Collections.Specialized

class CheckAuthRequest(AssociatedRequest):
    Mode as string:
        get:
            return "check_authentication"

    internal sig as string
    internal signed as NameValueCollection
    internal invalidate_handle as string

    internal def constructor(assoc_handle as string, sig as string,
                             signed as NameValueCollection,
                             invalidate_handle as string):
        self.assoc_handle = assoc_handle
        self.sig = sig
        self.signed = signed
        self.invalidate_handle = invalidate_handle


    def constructor(query as NameValueCollection):
        qget = do(key as string):
            val = query.Get("openid." + key)
            if val is null:
                raise ProtocolException(
                    query, "${Mode} request missing required parameter ${key}")

            return val

        self.assoc_handle = qget("assoc_handle")
        self.sig = qget("sig")
        signedStr = qget("signed")

        self.invalidate_handle = query.Get("openid.invalidate_handle")

        signedList = @/,/.Split(signedStr)
        self.signed = NameValueCollection()
        
        sget = do(key as string):
            val = query.Get("openid." + key)
            if val is null:
                raise ProtocolException(
                    query, "Couldn't find signed field ${key}")

            return val

        for field in signedList:
            if field == "mode":
                value = "id_res"
            else:
                value = sget(field)

            self.signed.Add(field, value)

    def Answer(signatory as Signatory):
        is_valid = signatory.Verify(self.assoc_handle, self.sig, self.signed)
        # Now invalidate the assoc_handle so that this checkAuth
        # message cannot be replayed
        signatory.Invalidate(self.assoc_handle, true)
        response = Response(self)
        response.Fields["is_valid"] = (is_valid and "true") or "false"

        if self.invalidate_handle:
            assoc = signatory.GetAssociation(self.invalidate_handle, false)
            if assoc is null:
                response.Fields["invalidate_handle"] = self.invalidate_handle

        return response
