using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.IO;

/*
	packer, version 2.0 (beta) (2005/02/01)
	Copyright 2004-2005, Dean Edwards
	Web: http://dean.edwards.name/

	This software is licensed under the CC-GNU LGPL
	Web: http://creativecommons.org/licenses/LGPL/2.1/
    
    Ported to C# by Jesse Hansen, twindagger2k@msn.com
    modified slightly by Andrew Arnott
*/

// http://dean.edwards.name/packer/

namespace Dean.Edwards
{
	/// <summary>
	/// Packs a javascript file into a smaller area, removing unnecessary characters from the output.
	/// </summary>
    internal class ECMAScriptPacker
    {
        /// <summary>
        /// The encoding level to use. See http://dean.edwards.name/packer/usage/ for more info.
        /// </summary>
        public enum PackerEncoding { None = 0, Numeric = 10, Mid = 36, Normal = 62, HighAscii = 95 };

        private PackerEncoding encoding = PackerEncoding.Normal;
        private bool fastDecode = true;
        private bool specialChars = false;
        private bool enabled = true;

        string IGNORE = "$1";

	    /// <summary>
	    /// The encoding level for this instance
	    /// </summary>
        public PackerEncoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <summary>
        /// Adds a subroutine to the output to speed up decoding
        /// </summary>
        public bool FastDecode
        {
            get { return fastDecode; }
            set { fastDecode = value; }
        }

        /// <summary>
        /// Replaces special characters
        /// </summary>
        public bool SpecialChars
        {
            get { return specialChars; }
            set { specialChars = value; }
        }

        /// <summary>
        /// Packer enabled
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public ECMAScriptPacker()
        {
            Encoding = PackerEncoding.Normal;
            FastDecode = true;
            SpecialChars = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encoding">The encoding level for this instance</param>
        /// <param name="fastDecode">Adds a subroutine to the output to speed up decoding</param>
        /// <param name="specialChars">Replaces special characters</param>
        public ECMAScriptPacker(PackerEncoding encoding, bool fastDecode, bool specialChars)
        {
            Encoding = encoding;
            FastDecode = fastDecode;
            SpecialChars = specialChars;
        }

        /// <summary>
        /// Packs the script
        /// </summary>
        /// <param name="script">the script to pack</param>
        /// <returns>the packed script</returns>
        public string Pack(string script)
        {
            if (enabled)
            {
                script += "\n";
                script = basicCompression(script);
                if (SpecialChars)
                    script = encodeSpecialChars(script);
                if (Encoding != PackerEncoding.None)
                    script = encodeKeywords(script);
            }
            return script;
        }

        //zero encoding - just removal of whitespace and comments
        private string basicCompression(string script)
        {
            ParseMaster parser = new ParseMaster();
            // make safe
            parser.EscapeChar = '\\';
            // protect strings
            parser.Add("'[^'\\n\\r]*'", IGNORE);
            parser.Add("\"[^\"\\n\\r]*\"", IGNORE);
            // remove comments
            parser.Add("\\/\\/[^\\n\\r]*[\\n\\r]");
            parser.Add("\\/\\*[^*]*\\*+([^\\/][^*]*\\*+)*\\/");
            // protect regular expressions
            parser.Add("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?)", "$2");
            parser.Add("[^\\w\\$\\/'\"*)\\?:]\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?", IGNORE);
            // remove: ;;; doSomething();
            if (specialChars)
                parser.Add(";;[^\\n\\r]+[\\n\\r]");
            // remove redundant semi-colons
            parser.Add(";+\\s*([};])", "$2");
            // remove white-space
            parser.Add("(\\b|\\$)\\s+(\\b|\\$)", "$2 $3");
            parser.Add("([+\\-])\\s+([+\\-])", "$2 $3");
            parser.Add("\\s+");
            // done
            return parser.Exec(script);
        }

        WordList encodingLookup;
        private string encodeSpecialChars(string script)
        {
            ParseMaster parser = new ParseMaster();
            // replace: $name -> n, $$name -> na
            parser.Add("((\\$+)([a-zA-Z\\$_]+))(\\d*)", 
                new ParseMaster.MatchGroupEvaluator(encodeLocalVars));

            // replace: _name -> _0, double-underscore (__name) is ignored
            Regex regex = new Regex("\\b_[A-Za-z\\d]\\w*");
            
            // build the word list
            encodingLookup = analyze(script, regex, new EncodeMethod(encodePrivate));

            parser.Add("\\b_[A-Za-z\\d]\\w*", new ParseMaster.MatchGroupEvaluator(encodeWithLookup));
            
            script = parser.Exec(script);
            return script;
        }

        private string encodeKeywords(string script)
        {
		    // escape high-ascii values already in the script (i.e. in strings)
		    if (Encoding == PackerEncoding.HighAscii) script = escape95(script);
		    // create the parser
            ParseMaster parser = new ParseMaster();
            EncodeMethod encode = getEncoder(Encoding);

            // for high-ascii, don't encode single character low-ascii
            Regex regex = new Regex(
                    (Encoding == PackerEncoding.HighAscii) ? "\\w\\w+" : "\\w+"
                );
            // build the word list
            encodingLookup = analyze(script, regex, encode);

            // encode
            parser.Add((Encoding == PackerEncoding.HighAscii) ? "\\w\\w+" : "\\w+",
                new ParseMaster.MatchGroupEvaluator(encodeWithLookup));

            // if encoded, wrap the script in a decoding function
            return (script == string.Empty) ? "" : bootStrap(parser.Exec(script), encodingLookup);
        }

        private string bootStrap(string packed, WordList keywords)
        {
		    // packed: the packed script
		    packed = "'" + escape(packed) + "'";

		    // ascii: base for encoding
            int ascii = Math.Min(keywords.Sorted.Count, (int) Encoding);
            if (ascii == 0)
                ascii = 1;

		    // count: number of words contained in the script
		    int count = keywords.Sorted.Count;

		    // keywords: list of words contained in the script
            foreach (object key in keywords.Protected.Keys)
            {
                keywords.Sorted[(int) key] = "";
            }
		    // convert from a string to an array
            StringBuilder sbKeywords = new StringBuilder("'");
            foreach (string word in keywords.Sorted)
                sbKeywords.Append(word + "|");
            sbKeywords.Remove(sbKeywords.Length-1, 1);
            string keywordsout = sbKeywords.ToString() + "'.split('|')";

            string encode;
            string inline = "c";

            switch (Encoding)
            {
                case PackerEncoding.Mid:
                    encode = "function(c){return c.toString(36)}";
                    inline += ".toString(a)";
                    break;
                case PackerEncoding.Normal:
                    encode = "function(c){return(c<a?\"\":e(parseInt(c/a)))+" +
                        "((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))}";
                    inline += ".toString(a)";
                    break;
                case PackerEncoding.HighAscii:
                    encode = "function(c){return(c<a?\"\":e(c/a))+" +
                        "String.fromCharCode(c%a+161)}";
                    inline += ".toString(a)";
                    break;
                default:
                    encode = "function(c){return c}";
                    break;
            }
            
            // decode: code snippet to speed up decoding
            string decode = "";
            if (fastDecode)
            {
                decode = "if(!''.replace(/^/,String)){while(c--)d[e(c)]=k[c]||e(c);k=[function(e){return d[e]}];e=function(){return'\\\\w+'};c=1;}";
                if (Encoding == PackerEncoding.HighAscii)
                    decode = decode.Replace("\\\\w", "[\\xa1-\\xff]");
                else if (Encoding == PackerEncoding.Numeric)
                    decode = decode.Replace("e(c)", inline);
                if (count == 0)
                    decode = decode.Replace("c=1", "c=0");
            }

            // boot function
            string unpack = "function(p,a,c,k,e,d){while(c--)if(k[c])p=p.replace(new RegExp('\\\\b'+e(c)+'\\\\b','g'),k[c]);return p;}";
            Regex r;
            if (fastDecode)
            {
                //insert the decoder
                r = new Regex("\\{");
                unpack = r.Replace(unpack, "{" + decode + ";", 1);
            }

            if (Encoding == PackerEncoding.HighAscii)
            {
                // get rid of the word-boundries for regexp matches
                r = new Regex("'\\\\\\\\b'\\s*\\+|\\+\\s*'\\\\\\\\b'");
                unpack = r.Replace(unpack, "");
            }
            if (Encoding == PackerEncoding.HighAscii || ascii > (int) PackerEncoding.Normal || fastDecode)
            {
                // insert the encode function
                r = new Regex("\\{");
                unpack = r.Replace(unpack, "{e=" + encode + ";", 1);
            }
            else
            {
                r = new Regex("e\\(c\\)");
                unpack = r.Replace(unpack, inline);
            }
            // no need to pack the boot function since i've already done it
            string _params = "" + packed + "," + ascii + "," + count + "," + keywordsout;
            if (fastDecode)
            {
                //insert placeholders for the decoder
                _params += ",0,{}";
            }
            // the whole thing
            return "eval(" + unpack + "(" + _params + "))\n";
        }

        private string escape(string input)
        {
            Regex r = new Regex("([\\\\'])");
            return r.Replace(input, "\\$1");
        }

        private EncodeMethod getEncoder(PackerEncoding encoding)
        {
            switch (encoding)
            {
                case PackerEncoding.Mid:
                    return new EncodeMethod(encode36);
                case PackerEncoding.Normal:
                    return new EncodeMethod(encode62);
                case PackerEncoding.HighAscii:
                    return new EncodeMethod(encode95);
                default:
                    return new EncodeMethod(encode10);
            }
        }

        private string encode10(int code)
        {
            return code.ToString();
        }

        //lookups seemed like the easiest way to do this since 
        // I don't know of an equivalent to .toString(36)
        private static string lookup36 = "0123456789abcdefghijklmnopqrstuvwxyz";

        private string encode36(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(36, i)) % 36;
                encoded = lookup36[digit] + encoded;
                code -= digit * (int) Math.Pow(36, i++);
            } while (code > 0);
            return encoded;
        }

        private static string lookup62 = lookup36 + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string encode62(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(62, i)) % 62;
                encoded = lookup62[digit] + encoded;
                code -= digit * (int) Math.Pow(62, i++);
            } while (code > 0);
            return encoded;
        }

        private static string lookup95 = "¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

        private string encode95(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(95, i)) % 95;
                encoded = lookup95[digit] + encoded;
                code -= digit * (int) Math.Pow(95, i++);
            } while (code > 0);
            return encoded;
        }

        private string escape95(string input)
        {
            Regex r = new Regex("[\xa1-\xff]");
            return r.Replace(input, new MatchEvaluator(escape95Eval));
        }

        private string escape95Eval(Match match)
        {
            return "\\x" + ((int) match.Value[0]).ToString("x"); //return hexadecimal value
        }

        private string encodeLocalVars(Match match, int offset)
        {
            int length = match.Groups[offset + 2].Length;
            int start = length - Math.Max(length - match.Groups[offset + 3].Length, 0);
            return match.Groups[offset + 1].Value.Substring(start, length) + 
                match.Groups[offset + 4].Value;
        }

        private string encodeWithLookup(Match match, int offset)
        {
            return (string) encodingLookup.Encoded[match.Groups[offset].Value];
        }

        private delegate string EncodeMethod(int code);

        private string encodePrivate(int code)
        {
            return "_" + code;
        }

        private WordList analyze(string input, Regex regex, EncodeMethod encodeMethod)
        {
            // analyse
            // retreive all words in the script
            MatchCollection all = regex.Matches(input);
            WordList rtrn;
            rtrn.Sorted = new StringCollection(); // list of words sorted by frequency
            rtrn.Protected = new HybridDictionary(); // dictionary of word->encoding
            rtrn.Encoded = new HybridDictionary(); // instances of "protected" words
            if (all.Count > 0)
            {
                StringCollection unsorted = new StringCollection(); // same list, not sorted
                HybridDictionary Protected = new HybridDictionary(); // "protected" words (dictionary of word->"word")
                HybridDictionary values = new HybridDictionary(); // dictionary of charCode->encoding (eg. 256->ff)
                HybridDictionary count = new HybridDictionary(); // word->count
                int i = all.Count, j = 0;
                string word;
                // count the occurrences - used for sorting later
                do
                {
                    word = "$" + all[--i].Value;
                    if (count[word] == null)
                    {
                        count[word] = 0;
                        unsorted.Add(word);
                        // make a dictionary of all of the protected words in this script
                        //  these are words that might be mistaken for encoding
                        Protected["$" + (values[j] = encodeMethod(j))] = j++;
                    }
                    // increment the word counter
                    count[word] = (int) count[word] + 1;
                } while (i > 0);
                /* prepare to sort the word list, first we must protect
                    words that are also used as codes. we assign them a code
                    equivalent to the word itself.
                   e.g. if "do" falls within our encoding range
                        then we store keywords["do"] = "do";
                   this avoids problems when decoding */
                i = unsorted.Count;
                string[] sortedarr = new string[unsorted.Count];
                do
                {
                    word = unsorted[--i];
                    if (Protected[word] != null)
                    {
                        sortedarr[(int) Protected[word]] = word.Substring(1);
                        rtrn.Protected[(int) Protected[word]] = true;
                        count[word] = 0;
                    }
                } while (i > 0);
                string[] unsortedarr = new string[unsorted.Count];
                unsorted.CopyTo(unsortedarr, 0);
                // sort the words by frequency
                Array.Sort(unsortedarr, (IComparer) new CountComparer(count));
                j = 0;
                /*because there are "protected" words in the list
                  we must add the sorted words around them */
                do 
                {
				    if (sortedarr[i] == null) 
                        sortedarr[i] = unsortedarr[j++].Substring(1);
				    rtrn.Encoded[sortedarr[i]] = values[i];
			    } while (++i < unsortedarr.Length);
                rtrn.Sorted.AddRange(sortedarr);
            }
            return rtrn;
        }

        private struct WordList
        {
            public StringCollection Sorted;
            public HybridDictionary Encoded;
            public HybridDictionary Protected;
        }

        private class CountComparer : IComparer
        {
            HybridDictionary count;

            public CountComparer(HybridDictionary count)
            {
                this.count = count;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                return (int) count[y] - (int) count[x];
            }

            #endregion
        }
    }
}
