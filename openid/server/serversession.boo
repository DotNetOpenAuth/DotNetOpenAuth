namespace Janrain.OpenId.Server

import System
import System.Collections.Specialized
import Mono.Security.Cryptography
import Janrain.OpenId

abstract class ServerSession:
    [Getter(SessionType)]
    session_type as string

    abstract def Answer(secret as (byte)) as NameValueCollection:
        pass


class PlainTextServerSession(ServerSession):
    def constructor():
        session_type = 'plaintext'

    override def Answer(secret as (byte)):
        nvc = NameValueCollection()
        nvc.Add('mac_key', CryptUtil.ToBase64String(secret))
        return nvc


class DiffieHellmanServerSession(ServerSession):
    internal consumer_pubkey as (byte)
    internal dh as DiffieHellman

    def constructor(query as NameValueCollection):
        self.session_type = "DH-SHA1"
        dh_modulus = query.Get('openid.dh_modulus')
        dh_gen = query.Get('openid.dh_gen')
        if (dh_modulus is null and dh_gen is not null or
            dh_gen is null and dh_modulus is not null):
            if dh_modulus is null:
                missing = 'modulus'
            else:
                missing = 'generator'

            raise ProtocolException(
                query,
                'If non-default modulus or generator is ' +
                'supplied, both must be supplied. ' +
                'Missing ${missing}')

        if dh_modulus or dh_gen:
            try:
                dh_modulus_bytes = Convert.FromBase64String(dh_modulus)
            except err as FormatException:
                raise ProtocolException(
                    query, "dh_modulus isn't properly base64ed")
                
            try:
                dh_gen_bytes = Convert.FromBase64String(dh_gen)
            except err as FormatException:
                raise ProtocolException(query, "dh_gen isn't properly base64ed")
        else:
            dh_modulus_bytes = CryptUtil.DEFAULT_MOD
            dh_gen_bytes = CryptUtil.DEFAULT_GEN

        self.dh = DiffieHellmanManaged(dh_modulus_bytes, dh_gen_bytes, 1024)

        consumer_pubkey as string = query.Get('openid.dh_consumer_public')
        if consumer_pubkey is null:
            raise ProtocolException(
                query, "Public key for DH-SHA1 session not found in query")

        try:
            self.consumer_pubkey = Convert.FromBase64String(consumer_pubkey)
        except err as FormatException:
            raise ProtocolException(
                query, "consumer_pubkey isn't properly base64ed")
        
    override def Answer(secret as (byte)):
        mac_key = CryptUtil.SHA1XorSecret(self.dh, self.consumer_pubkey, secret)
        nvc = NameValueCollection()
        nvc.Add('dh_server_public', CryptUtil.UnsignedToBase64(
                self.dh.CreateKeyExchange()))
        nvc.Add('enc_mac_key', CryptUtil.ToBase64String(mac_key))
        return nvc

