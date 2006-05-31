namespace Janrain.OpenId

import System
import System.IO
import System.Collections
import System.Collections.Specialized
import System.Security.Cryptography
import System.Text

abstract class Association(ICloneable):
    static UNIX_EPOCH as DateTime = date(1970, 1, 1, 0, 0, 0, 0)
    
    virtual AssociationType as string:
        get:
            pass


    [Getter(Handle)]
    handle as string
    
    [Getter(Issued)]
    issued as DateTime

    [Getter(Secret)]
    protected key as (byte)
    
    expiresIn as TimeSpan
    
    IssuedUnix as uint:
        get:
            return cast(uint, (self.issued - UNIX_EPOCH).TotalSeconds)
    
    protected Expires as DateTime:
        get:
            return self.issued.Add(self.expiresIn)
    
    IsExpired as bool:
        get:
            return (self.issued.Add(self.expiresIn) < DateTime.UtcNow)
    
    ExpiresIn as long:
        get:
            return cast(long, (self.Expires - DateTime.UtcNow).Seconds)
    
    def Clone() as object:
        return Deserialize(self.Serialize())

    virtual def Serialize() as (byte):
        data = {'version': '2',
		'handle': self.handle,
		'secret': CryptUtil.ToBase64String(self.Secret),
		'issued': self.IssuedUnix.ToString(),
		'expires_in': cast(int, self.expiresIn.TotalSeconds).ToString(),
		'assoc_type': self.AssociationType,
		}

        return KVUtil.DictToKV(data)

    static def Deserialize(data as (byte)) as Association:
        kvpairs = KVUtil.KVToDict(data)
        version as string = kvpairs['version']
        if version != '2':
            msg = String.Format('Unknown version: {0}', version)
            raise NotSupportedException(msg)

        assoc_type as string = kvpairs['assoc_type']
        if assoc_type == 'HMAC-SHA1':
            return HMACSHA1Association(kvpairs)
        else:
            raise NotSupportedException(
                String.Format('Unknown Association type: {0}', assoc_type))

    abstract def SignDict(fields as (string), data as IDictionary, prefix as string) as string:
        pass

    abstract def Sign(l as NameValueCollection) as (byte):
        pass
    

class HMACSHA1Association(Association):
    override AssociationType:
        get:
            return 'HMAC-SHA1'

    def constructor(handle as string, secret as (byte), expiresIn as TimeSpan):
        self.handle = handle
        self.key = secret
        self.issued = UNIX_EPOCH.Add(TimeSpan(0, 0, cast(
                    int, (DateTime.UtcNow - UNIX_EPOCH).TotalSeconds)))
        self.expiresIn = TimeSpan(0, 0, cast(int, expiresIn.TotalSeconds))

    protected def constructor(kvpairs as IDictionary):
        self.handle = kvpairs['handle']
        self.key = Convert.FromBase64String(kvpairs['secret'])
        seconds as int = Convert.ToInt32(kvpairs['issued'])
        self.issued = UNIX_EPOCH.Add(TimeSpan(0, 0, seconds))
        seconds = Convert.ToInt32(kvpairs['expires_in'])
        self.expiresIn = TimeSpan(0, 0, seconds)

    override def Equals(o as object) as bool:
        if o is null:
            return false

        if o isa HMACSHA1Association:
            a = cast(HMACSHA1Association, o)
            if a.handle != self.handle:
                return false
            if (CryptUtil.ToBase64String(a.Secret) !=
                CryptUtil.ToBase64String(self.Secret)):
                return false
            if a.Expires != self.Expires:
                return false
            if a.expiresIn != self.expiresIn:
                return false

            return true

        return false

    override def GetHashCode() as int:
        hmac = HMACSHA1(self.Secret)
        cs = CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write)
        hbytes as (byte) = ASCIIEncoding.ASCII.GetBytes(self.handle)
        cs.Write(hbytes, 0, hbytes.Length)
        cs.Close()
        hash as (byte) = hmac.Hash
        hmac.Clear()
        val as long = 0
        for i in range(0, hash.Length):
            val = val ^ cast(long, hash[i])
        val = val ^ self.Expires.ToFileTimeUtc()
        return cast(int, val)

    override def SignDict(fields as (string), data as IDictionary,
                          prefix as string):
        l = NameValueCollection()
        for field as string in fields:
            l.Add(field, data[(prefix + field)])
        return CryptUtil.ToBase64String(Sign(l))

    override def Sign(l as NameValueCollection) as (byte):
        data as (byte) = KVUtil.SeqToKV(l, false)
        hmac = HMACSHA1(self.Secret)
        hash as (byte) = hmac.ComputeHash(data)
        hmac.Clear()
        return hash

