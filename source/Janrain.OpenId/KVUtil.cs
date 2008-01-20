using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace Janrain.OpenId
{
    public class KVUtil
    {

        #region Constructor(s)

        public KVUtil()
        {
        }

        #endregion

        #region Methods

        private static void Error(string message, bool strict)
        {
            if (strict)
                throw new ArgumentException(message);
        }

        public static byte[] SeqToKV(NameValueCollection seq, bool strict)
        {
            MemoryStream ms = new MemoryStream();

            foreach (string key in seq.Keys)
            {
                string val = seq[key];
                if (key.IndexOf('\n') >= 0)
                    throw new ArgumentException("Invalid input for SeqToKV: key contains newline");

                if (key.Trim().Length != key.Length)
                    Error("Key has whitespace at beginning or end: " + key, strict);

                if (val.Trim().Length != val.Length)
                    Error("Value has whitespace at beginning or end: " + val, strict);

                byte[] line = Encoding.UTF8.GetBytes(key + ":" + val + "\n");
                ms.Write(line, 0, line.Length);
            }

            return ms.ToArray();
        }

        public static byte[] DictToKV(IDictionary dict)
        {
            NameValueCollection data = new NameValueCollection(dict.Count);

            foreach (DictionaryEntry pair in dict)
            {
                data.Add(pair.Key.ToString(), pair.Value.ToString());
            }

            return SeqToKV(data, false);
        }

        public static IDictionary KVToDict(byte[] data)
        {
            StringReader reader = new StringReader(UTF8Encoding.UTF8.GetString(data));
            Dictionary<string,string> dict = new Dictionary<string,string>();
            int line_num = 0;
            string line = reader.ReadLine();

            while (line != null)
            {
                line_num++;
                if (line.Trim().Length > 0)
                {
                    string[] parts = line.Split(new char[] { ':' }, 2);
                    if (parts.Length != 2)
                    {
                        Error("Line " + line_num.ToString() + " does not contain a color.", false);
                    }
                    else
                    {
                        dict[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                line = reader.ReadLine();
            }
            return dict;
        }

        #endregion

    }
}
