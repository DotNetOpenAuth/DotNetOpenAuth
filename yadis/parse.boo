namespace Janrain.Yadis

import System
import System.Collections
import System.Collections.Specialized
import System.Text
import System.Text.RegularExpressions


class ContentType:
    [Property(Type)]
    type as string

    [Property(SubType)]
    sub_type as string

    [Getter(Parameters)]
    parameters as NameValueCollection

    MediaType as string:
        get:
            return "${self.type}/${self.sub_type}"
    
    def constructor(ct as string):
        err_msg = "\"${ct}\" does not appear to be a valid content type"
        self.parameters = NameValueCollection()
        
        semi = char(';')
        slash = char('/')
        parts = ct.Split(semi)
        try:
            type, sub_type = parts[0].Split(slash)
        except why as IndexOutOfRangeException:
            raise ArgumentException(err_msg)
        self.type = type.Trim()
        self.sub_type = sub_type.Trim()
        
        for param in parts[1:]:
            try:
                k, v = param.Split(char('='))
            except why as IndexOutOfRangeException:
                raise ArgumentException(err_msg)
                
            self.parameters[k.Trim()] = v.Trim()


class ByteParser:
    # Keep users from instantiating
    private def constructor():
        pass

    private static flags = (((RegexOptions.IgnoreCase | RegexOptions.Compiled)
                             | RegexOptions.Singleline)
                            | RegexOptions.IgnorePatternWhitespace)

    private static removedRe = Regex('<!--.*?-->|<!\\[CDATA\\[.*?\\]\\]>|<script\\b[^>]*>.*?</script>', flags)

    private static tagExpr = """
# Starts with the tag name at a word boundary, where the tag name is
# not a namespace
<{0}\b(?!:)
    
# All of the stuff up to a ">", hopefully attributes.
(?<attrs>[^>]*?)
    
(?: # Match a short tag
    />
    
|   # Match a full tag
    >
    
    (?<contents>.*?)
    
    # Closed by
    (?: # One of the specified close tags
        </?{1}\s*>
    
    # End of the string
    |   \Z
    
    )
    
)
    """

    
    private static def TagMatcher(tagName as string, *closeTags as (string)):
        if closeTags.Length > 0:
            sb = StringBuilder()
            sb.AppendFormat('(?:{0}', tagName)
            for closeTag in closeTags:
                sb.AppendFormat('|{0}', closeTag)
            sb.Append(')')
            closers = sb.ToString()
        else:
            closers = tagName

        return Regex(String.Format(tagExpr, tagName, closers), flags)

    private static htmlRe = TagMatcher('html')
    private static headRe = TagMatcher('head', *(of string: 'body'))
    private static entityRe = Regex('&(?<entity>amp|lt|gt|quot);')
    private static xmlDeclRe = Regex('^<\\?xml\\b(?<attrs>.+)\\?>', flags)
    private static attrRe = Regex("""
# Must start with a sequence of word-characters, followed by an equals sign
(?<attrname>(\w|-)+)=

# Then either a quoted or unquoted attribute
(?:

 # Match everything that's between matching quote marks
 (?<qopen>["\'])(?<attrval>.*?)\k<qopen>
|

 # If the value is not quoted, match up to whitespace
 (?<attrval>(?:[^\s<>/]|/(?!>))+)
)

|

(?<endtag>[<>])
    """, flags)
        
        
    static def XmlEncoding(data as (byte), length as int,
                           encoding as Encoding):
        if encoding is null:
            encoding = UTF8Encoding.UTF8

        xml = encoding.GetString(data, 0, length)
        declMo = xmlDeclRe.Match(xml)

        if declMo.Success:
            attrs_group = declMo.Groups["attrs"]
            attrs = NameValueCollection()

            attrMo = attrRe.Match(attrs_group.Value)
            while attrMo.Success:
                if attrMo.Groups['endtag'].Success:
                    break
                if attrMo.Groups['attrname'].Value.ToLower() == "encoding":
                    return attrMo.Groups['attrval'].Value

                attrMo = attrMo.NextMatch()

        return null


    private static def ReplaceEntities(html as string):
        m = entityRe.Match(html)
        while m.Success:
            tok = m.Groups['entity'].ToString()
            if tok == 'amp':
                repl = '&'
            elif tok == 'lt':
                repl = '<'
            elif tok == 'gt':
                repl = '>'
            elif tok == 'quot':
                repl = '"'
            else:
                repl = null

            html = ((html.Substring(0, m.Index) + repl) +
                    html.Substring((m.Index + m.Length)))
            m = entityRe.Match(html, (m.Index + repl.Length))

        return html

    static def HeadTagAttrs(html as string, tag_name as string):
        result = []
        html = removedRe.Replace(html, '')
        htmlMo = htmlRe.Match(html)
        if not htmlMo.Success:
            return result.ToArray()

        headMo = headRe.Match(html, htmlMo.Index, htmlMo.Length)
        if not headMo.Success:
            return result.ToArray()

        attrName as string
        attrVal as string

        tagRe = TagMatcher(tag_name)
        tagMo = tagRe.Match(html, headMo.Index, headMo.Length)
        while tagMo.Success:
            start = tagMo.Index + len(tag_name) + 1
            length = tagMo.Index + tagMo.Length - start
            attrMo = attrRe.Match(html, start, length)

            attrs = NameValueCollection()
            while attrMo.Success:
                if attrMo.Groups['endtag'].Success:
                    break 
                attrName = attrMo.Groups['attrname'].Value
                attrVal = ReplaceEntities(attrMo.Groups['attrval'].Value)
                attrs[attrName] = attrVal

                attrMo = attrMo.NextMatch()

            result.Add(attrs)
            tagMo = tagMo.NextMatch()

        return result.ToArray()

