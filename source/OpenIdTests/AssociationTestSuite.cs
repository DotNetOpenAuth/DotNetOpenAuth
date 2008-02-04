using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Janrain.OpenId;

namespace OpenIdTests
{
    [TestFixture]
    public class AssociationTestSuite
    {

        [Test]
        public void SerializeDeserialize()
        {
            TimeSpan expiresIn = new TimeSpan(0, 0, 600);
            string handle = "handle";
            byte[] secret = ASCIIEncoding.ASCII.GetBytes("secret");
            
            
            Association assoc = new HmacSha1Association(handle, secret, expiresIn);

            byte[] s = assoc.Serialize();

            Association assoc2 = assoc.Deserialize(s);
            

            Assert.IsTrue(assoc.Equals(assoc2));
            Assert.AreEqual(assoc.GetHashCode(), assoc2.GetHashCode());

        }

    }
}
