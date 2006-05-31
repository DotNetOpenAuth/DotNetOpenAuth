namespace Janrain.OpenId.Test

import System
import System.Text
import System.Collections
import NUnit.Framework
import Janrain.OpenId

[TestFixture]
class KVUtilTestSuite:

    static def KVDictTest(kvform as (byte), dict as Hash):
        # Convert KVForm to dict
        d = KVUtil.KVToDict(kvform)

        # make sure it parses to expected dict
        
        for de as DictionaryEntry in dict:
            Assert.AreEqual(de.Value, d[de.Key], UTF8Encoding.UTF8.GetString(kvform))
        #Assert.AreEqual(dict, d, UTF8Encoding.UTF8.GetString(kvform))

        # Convert back to KVForm and round-trip back to dict to make
        # sure that *** dict -> kv -> dict is identity. ***
        kv = KVUtil.DictToKV(d)
        d2 = KVUtil.KVToDict(kv)
        for de as DictionaryEntry in d:
            Assert.AreEqual(de.Value, d2[de.Key], UTF8Encoding.UTF8.GetString(kvform))

    [Test]
    def KVDict():
        kvdict_cases = [
            ('', {}),
            ('college:harvey mudd\n', {'college':'harvey mudd'}),
            ('city:claremont\nstate:CA\n', {'city':'claremont', 'state':'CA'}),
            ('is_valid:true\ninvalidate_handle:{HMAC-SHA1:2398410938412093}\n',
             {'is_valid':'true',
              'invalidate_handle':'{HMAC-SHA1:2398410938412093}'}),
            
            ('x\n', {}),
            ('x\nx\n', {}),
            ('East is least\n', {}),

            # But not from blank lines (because LJ generates them)
            ('x\n\n', {}),

            # Warning from empty key
            (':\n', {'':''}),
            (':missing key\n', {'':'missing key'}),
            
            # Warnings from leading or trailing whitespace in key or value
            (' street:foothill blvd\n', {'street':'foothill blvd'}),
            ('major: computer science\n', {'major':'computer science'}),
            (' dorm : east \n', {'dorm':'east'}),
            
            # Warnings from missing trailing newline
            ('e^(i*pi)+1:0', {'e^(i*pi)+1':'0'}),
            ('east:west\nnorth:south', {'east':'west', 'north':'south'}),
            ]
        
        for kvform as string, dict as Hash in kvdict_cases:
            KVDictTest(UTF8Encoding.UTF8.GetBytes(kvform), dict)


