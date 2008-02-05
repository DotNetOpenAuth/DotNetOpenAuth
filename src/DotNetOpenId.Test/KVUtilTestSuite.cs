using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;


namespace DotNetOpenId.Test
{
    [TestFixture]
    public class KVUtilTestSuite
    {

        public static void KVDictTest(byte[] kvform, Dictionary<string,string> dict)
        {
            Dictionary<string,string> d = (Dictionary<string,string>) KVUtil.KVToDict(kvform);

            Dictionary<string, string>.KeyCollection keys = dict.Keys;

            foreach (string key in keys)
            {
                Assert.AreEqual(d[key], dict[key], d[key] + " and " + dict[key] + " do not match.");
            }
            
            //byte[] kv = KVUtil.DictToKV(d);
            //Hashtable d2 = (Hashtable) KVUtil.KVToDict(kv);

            //foreach (DictionaryEntry de in d)
            //{
            //    Assert.AreEqual(de.Value, d2[de.Key], UTF8Encoding.UTF8.GetString(kvform));
            //}
        }

        [Test]
        public void KVDict()
        {

            KVDictTest(UTF8Encoding.UTF8.GetBytes(""), new Dictionary<string, string>());

            Dictionary<string,string> d1 = new Dictionary<string,string>();
            d1.Add("college", "harvey mudd");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("college:harvey mudd\n"), d1);


            Dictionary<string,string> d2 = new Dictionary<string,string>();
            d2.Add("city", "claremont");
            d2.Add("state", "CA");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("city:claremont\nstate:CA\n"), d2);

            Dictionary<string,string> d3 = new Dictionary<string,string>();
            d3.Add("is_valid", "true");
            d3.Add("invalidate_handle", "{HMAC-SHA1:2398410938412093}");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("is_valid:true\ninvalidate_handle:{HMAC-SHA1:2398410938412093}\n"), d3);


            KVDictTest(UTF8Encoding.UTF8.GetBytes("x\n"), new Dictionary<string,string>());
            KVDictTest(UTF8Encoding.UTF8.GetBytes("x\nx\n"), new Dictionary<string, string>());
            KVDictTest(UTF8Encoding.UTF8.GetBytes("East is least\n"), new Dictionary<string, string>());
            KVDictTest(UTF8Encoding.UTF8.GetBytes("x\n\n"), new Dictionary<string, string>());

            Dictionary<string, string> d4 = new Dictionary<string, string>();
            d4.Add("", "");
            KVDictTest(UTF8Encoding.UTF8.GetBytes(":\n"), d4);

            Dictionary<string, string> d5 = new Dictionary<string, string>();
            d5.Add("", "missingkey");
            KVDictTest(UTF8Encoding.UTF8.GetBytes(":missingkey\n"), d5);

            Dictionary<string, string> d6 = new Dictionary<string, string>();
            d6.Add("street", "foothill blvd");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("street:foothill blvd\n"), d6);

            Dictionary<string, string> d7 = new Dictionary<string, string>();
            d7.Add("major", "computer science");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("major:computer science\n"), d7);

            Dictionary<string, string> d8 = new Dictionary<string, string>();
            d8.Add("dorm", "east");
            KVDictTest(UTF8Encoding.UTF8.GetBytes(" dorm : east \n"), d8);

            Dictionary<string, string> d9 = new Dictionary<string, string>();
            d9.Add("e^(i*pi)+1", "0");
            KVDictTest(UTF8Encoding.UTF8.GetBytes("e^(i*pi)+1:0"), d9);

            Dictionary<string, string> d10 = new Dictionary<string, string>();
            d10.Add("east", "west");
            d10.Add("north", "south");

            KVDictTest(UTF8Encoding.UTF8.GetBytes("east:west\nnorth:south"), d10);

  
        }

    }
}
