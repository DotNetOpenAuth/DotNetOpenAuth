namespace Janrain.OpenId

import System
import System.Collections
import System.Collections.Specialized
import System.IO
import System.Text

class KVUtil:
    private def constructor():
        pass

    private static def Error(msg as string, strict as bool):
        if strict:
            raise ArgumentException(msg)
        # XXX: log msg

    static def SeqToKV(seq as NameValueCollection, strict as bool):
        ms = MemoryStream()
        for key as string in seq:
            val as string = seq[key]
            if key.IndexOf(char('\n')) >= 0:
                raise ArgumentException(
                    'Invalid input for SeqToKV: key contains newline')

            if key.Trim().Length != key.Length:
                Error('Key has whitespace at beginning or end: ${key}', strict)

            if val.IndexOf(char('\n')) >= 0:
                raise ArgumentException(
                    'Invalid input for SeqToKV: value contains newline')

            if val.Trim().Length != val.Length:
                Error('Value has whitespace at beginning or end: ${val}',
                      strict)

            line = Encoding.UTF8.GetBytes("${key}:${val}\n")
            ms.Write(line, 0, line.Length)

        return ms.ToArray()

    static def DictToKV(dict as IDictionary):
        data = NameValueCollection()
        for pair as DictionaryEntry in dict:
            data.Add(pair.Key.ToString(), pair.Value.ToString())
        return SeqToKV(data, false)

    static def KVToDict(data as (byte)) as IDictionary:
        reader = StringReader(UTF8Encoding.UTF8.GetString(data))
        dict = {}
        line_num = 0
        while (line = reader.ReadLine()) is not null:
            line_num += 1
            if line.Trim().Length > 0:
                parts as (string) = line.Split((of char: char(':')), 2)
                if parts.Length != 2:
                    Error(
                        'Line ${line_num.ToString()} does not contain a colon',
                        false)
                else:
                    dict[parts[0].Trim()] = parts[1].Trim()
        return dict

