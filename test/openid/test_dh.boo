namespace Janrain.OpenId.Test

import System
import System.IO
import System.Text
import Mono.Security.Cryptography
import NUnit.Framework
import Janrain.OpenId


def Test1():
    dh1 = CryptUtil.CreateDiffieHellman()
    dh2 = CryptUtil.CreateDiffieHellman()
    secret1 = CryptUtil.ToBase64String(dh1.DecryptKeyExchange(dh2.CreateKeyExchange()))
    secret2 = CryptUtil.ToBase64String(dh2.DecryptKeyExchange(dh1.CreateKeyExchange()))
    Assert.AreEqual(secret1, secret2, "huh?")
    return secret1


[TestFixture]
class DiffieHellmanTestSuite:
    
    [Test]
    def Test():
        s1 as string = Test1()
        s2 as string = Test1()
        if s1 == s2:
            Assert.Fail("${s1} ${s2}")
    
    [Test]
    def TestPublic():
        sr = StreamReader('../test/openid/dhpriv')
        try:
            line as String
            while (line = sr.ReadLine()) != null:
                parts = /\s/.Split(line.Trim())
                x = Convert.FromBase64String(parts[0])
                dh = DiffieHellmanManaged(CryptUtil.DEFAULT_MOD, CryptUtil.DEFAULT_GEN, x)
                pub = dh.CreateKeyExchange()
                y = Convert.FromBase64String(parts[1])
                if y[0] == 0 and y[1] <= 127:
                    y = y[1:]
                Assert.AreEqual(y, Convert.FromBase64String(
                                CryptUtil.UnsignedToBase64(pub)), line)
        ensure:
            sr.Close()
        



