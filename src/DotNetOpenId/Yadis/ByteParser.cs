using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace Janrain.Yadis
{
    [Serializable]
    internal class ByteParser
    {
		private static readonly Regex attrRe;
		private static readonly Regex entityRe;
		private const RegexOptions flags = (RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly string tagExpr = "\n# Starts with the tag name at a word boundary, where the tag name is\n# not a namespace\n<{0}\\b(?!:)\n    \n# All of the stuff up to a \">\", hopefully attributes.\n(?<attrs>[^>]*?)\n    \n(?: # Match a short tag\n    />\n    \n|   # Match a full tag\n    >\n    \n    (?<contents>.*?)\n    \n    # Closed by\n    (?: # One of the specified close tags\n        </?{1}\\s*>\n    \n    # End of the string\n    |   \\Z\n    \n    )\n    \n)\n    ";
		private static readonly string startTagExpr = "\n# Starts with the tag name at a word boundary, where the tag name is\n# not a namespace\n<{0}\\b(?!:)\n    \n# All of the stuff up to a \">\", hopefully attributes.\n(?<attrs>[^>]*?)\n    \n(?: # Match a short tag\n    />\n    \n|   # Match a full tag\n    >\n    )\n    ";

		private static readonly Regex headRe;
		private static readonly Regex htmlRe = TagMatcher("html", new string[0]);
		private static readonly Regex removedRe = new Regex(@"<!--.*?-->|<!\[CDATA\[.*?\]\]>|<script\b[^>]*>.*?</script>", flags);
        private static Regex xmlDeclRe;

        static ByteParser()
        {
            string[] closeTags = new string[] { "body" };
            headRe = TagMatcher("head", closeTags);
            entityRe = new Regex("&(?<entity>amp|lt|gt|quot);");
            xmlDeclRe = new Regex(@"^<\?xml\b(?<attrs>.+)\?>", flags);
            attrRe = new Regex("\n# Must start with a sequence of word-characters, followed by an equals sign\n(?<attrname>(\\w|-)+)=\n\n# Then either a quoted or unquoted attribute\n(?:\n\n # Match everything that's between matching quote marks\n (?<qopen>[\"\\'])(?<attrval>.*?)\\k<qopen>\n|\n\n # If the value is not quoted, match up to whitespace\n (?<attrval>(?:[^\\s<>/]|/(?!>))+)\n)\n\n|\n\n(?<endtag>[<>])\n    ", flags);
        }

        private ByteParser(){}

        public static NameValueCollection[] HeadTagAttrs(string html, string tag_name)
        {
            List<NameValueCollection> result = new List<NameValueCollection>();
            html = removedRe.Replace(html, "");
            Match match = htmlRe.Match(html);
            if (match.Success)
            {
                Match match2 = headRe.Match(html, match.Index, match.Length);
                if (!match2.Success)
                {
                    return result.ToArray();
                }
                string text = null;
                string text2 = null;
				Regex regex = StartTagMatcher(tag_name);
                for (Match match3 = regex.Match(html, match2.Index, match2.Length); match3.Success; match3 = match3.NextMatch())
                {
                    int beginning = (match3.Index + tag_name.Length) + 1;
                    int length = (match3.Index + match3.Length) - beginning;
                    Match match4 = attrRe.Match(html, beginning, length);
                    NameValueCollection values = new NameValueCollection();
                    while (match4.Success)
                    {
                        if (match4.Groups["endtag"].Success)
                        {
                            break;
                        }
                        text = match4.Groups["attrname"].Value;
                        text2 = ReplaceEntities(match4.Groups["attrval"].Value);
                        values[text] = text2;
                        match4 = match4.NextMatch();
                    }
                    result.Add(values);
                }
            }
            return result.ToArray();
        }

        private static string ReplaceEntities(string html)
        {
            string text2;
            for (Match match = entityRe.Match(html); match.Success; match = entityRe.Match(html, (int)(match.Index + text2.Length)))
            {
                string text = match.Groups["entity"].ToString();
                if (text == "amp")
                {
                    text2 = "&";
                }
                else if (text == "lt")
                {
                    text2 = "<";
                }
                else if (text == "gt")
                {
                    text2 = ">";
                }
                else if (text == "quot")
                {
                    text2 = "\"";
                }
                else
                {
                    text2 = null;
                }
                html = (html.Substring(0, match.Index) + text2) + html.Substring(match.Index + match.Length);
            }
            return html;
        }

        private static Regex TagMatcher(string tagName, params string[] closeTags)
        {
            string text2;
            if (closeTags.Length > 0)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("(?:{0}", tagName);
                int index = 0;
                string[] textArray = closeTags;
                int length = textArray.Length;
                while (index < length)
                {
                    string text = textArray[index];
                    index++;
                    builder.AppendFormat("|{0}", text);
                }
                builder.Append(")");
                text2 = builder.ToString();
            }
            else
            {
                text2 = tagName;
            }
            return new Regex(string.Format(tagExpr, tagName, text2), flags);
        }

		private static Regex StartTagMatcher(string tag_name) {
			return new Regex(string.Format(startTagExpr, tag_name), flags);
		}

        public static string XmlEncoding(string data, int length, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            Match match = xmlDeclRe.Match(data);
            if (match.Success)
            {
                Group group = match.Groups["attrs"];
                NameValueCollection values = new NameValueCollection();
                for (Match match2 = attrRe.Match(group.Value); match2.Success; match2 = match2.NextMatch())
                {
                    if (match2.Groups["endtag"].Success)
                    {
                        break;
                    }
                    if (match2.Groups["attrname"].Value.ToLower() == "encoding")
                    {
                        return match2.Groups["attrval"].Value;
                    }
                }
            }
            return null;
        }

        public static string XmlEncoding(byte[] data, int length, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            string input = encoding.GetString(data, 0, length);
            return XmlEncoding(data, length, encoding);
        }
    }


}
