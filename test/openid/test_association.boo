namespace Janrain.OpenId.Test

import System
import System.Text
import NUnit.Framework
import Janrain.OpenId

[TestFixture]
class HMACSHA1AssociationTestSuite:
    [Test]
    def SerializeDeserialize():
        expiresIn as TimeSpan = TimeSpan(0, 0, 600)
        handle as string = 'handle'
        secret as (byte) = ASCIIEncoding.ASCII.GetBytes('secret')
        assoc as Association = HMACSHA1Association(handle, secret, expiresIn)
        s as (byte) = assoc.Serialize()
        assoc2 as Association = Association.Deserialize(s)
        Assert.IsTrue(assoc.Equals(assoc2))
        Assert.AreEqual(assoc.GetHashCode(), assoc2.GetHashCode())

