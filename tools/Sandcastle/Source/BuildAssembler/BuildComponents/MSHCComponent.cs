using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Ddue.Tools;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// Sandcastle component converting Microsoft Help 2.0 output to Microsoft Help System output.
    /// </summary>
    public class MSHCComponent : BuildComponent
    {
        // component tag names in the configuration file
        private class ConfigurationTag
        {
            public const string Data = "data";
        }

        // component attribute names in the configuration file
        private class ConfigurationAttr
        {
            public const string Locale = "locale";
            public const string SelfBranded = "self-branded";
            public const string TopicVersion = "topic-version";
            public const string TocFile = "toc-file";
            public const string TocParent = "toc-parent";
            public const string TocParentVersion = "toc-parent-version";
        }

        // XPath expressions to navigate the TOC file
        private class TocXPath
        {
            public const string Topics = "/topics";
            public const string Topic = "topic";
        }

        // attribute names in the TOC file
        private class TocAttr
        {
            public const string Id = "id";
        }

        // Microsoft Help 2.0 namespace info
        private class Help2Namespace
        {
            public const string Prefix = "MSHelp";
            public const string Uri = "http://msdn.microsoft.com/mshelp";
        }

        // XPath expressions to navigate Microsoft Help 2.0 data in the document
        private class Help2XPath
        {
            public const string Head = "head";
            public const string Xml = "xml";
            public const string TocTitle = "MSHelp:TOCTitle";
            public const string Attr = "MSHelp:Attr[@Name='{0}']";
            public const string Keyword = "MSHelp:Keyword[@Index='{0}']";
        }

        // Microsoft Help 2.0 tag attributes in the document
        private class Help2Attr
        {
            public const string Value = "Value";
            public const string Term = "Term";
            public const string Title = "Title";
        }

        // Microsoft Help 2.0 attribute values in the document
        private class Help2Value
        {
            public const string K = "K";
            public const string F = "F";
            public const string Locale = "Locale";
            public const string AssetID = "AssetID";
            public const string DevLang = "DevLang";
            public const string Abstract = "Abstract";
        }

        // Microsoft Help System tags
        private class MHSTag
        {
            public const string Meta = "meta";
        }

        // Microsoft Help System meta tag attributes
        private class MHSMetaAttr
        {
            public const string Name = "name";
            public const string Content = "content";
        }

        // Microsoft Help System meta names
        private class MHSMetaName
        {
            public const string SelfBranded = "SelfBranded";
            public const string ContentType = "ContentType";
            public const string Locale = "Microsoft.Help.Locale";
            public const string TopicLocale = "Microsoft.Help.TopicLocale";
            public const string Id = "Microsoft.Help.Id";
            public const string TopicVersion = "Microsoft.Help.TopicVersion";
            public const string TocParent = "Microsoft.Help.TocParent";
            public const string TocParentVersion = "Microsoft.Help.TOCParentTopicVersion";
            public const string TocOrder = "Microsoft.Help.TocOrder";
            public const string Title = "Title";
            public const string Keywords = "Microsoft.Help.Keywords";
            public const string F1 = "Microsoft.Help.F1";
            public const string Category = "Microsoft.Help.Category";
            public const string Description = "Description";
        }

        // Microsoft Help System meta default values 
        private class MHSDefault
        {
            public const bool SelfBranded = true;
            public const string Locale = "en-us";
            public const string Reference = "Reference";
            public const string TopicVersion = "100";
            public const string TocParent = "-1";
            public const string TocParentVersion = "100";
            public const string TocFile = "./toc.xml";
            public const string ShortName = "MHS";
        }

        // TOC information of a document
        private class TocInfo
        {
            private string _parent;
            private string _parentVersion;
            private int _order;

            public TocInfo(string parent, string parentVersion, int order)
            {
                _parent = parent;
                _parentVersion = parentVersion;
                _order = order;
            }

            public string Parent { get { return _parent; }}
            public string ParentVersion { get { return _parentVersion; } }
            public int Order { get { return _order; } }
        }

        private XmlDocument _document;
        private XmlNode _head;
        private XmlNode _xml;

        private string _locale = string.Empty;
        private bool _selfBranded = MHSDefault.SelfBranded;
        private string _topicVersion = MHSDefault.TopicVersion;
        private string _tocParent = MHSDefault.TocParent;
        private string _tocParentVersion = MHSDefault.TocParentVersion;
        private Dictionary<string, TocInfo> _toc = new Dictionary<string, TocInfo>(); 
        /// <summary>
        /// Creates a new instance of the <see cref="MHSComponent"/> class.
        /// </summary>
        /// <param name="assembler">The active <see cref="BuildAssembler"/>.</param>
        /// <param name="configuration">The current <see cref="XPathNavigator"/> of the configuration.</param>
        public MSHCComponent(BuildAssembler assembler, XPathNavigator configuration)
            : base(assembler, configuration)
        {
            string tocFile = MHSDefault.TocFile;
            XPathNavigator data = configuration.SelectSingleNode(ConfigurationTag.Data);
            if (data != null)
            {
                string value = data.GetAttribute(ConfigurationAttr.Locale, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    _locale = value;

                value = data.GetAttribute(ConfigurationAttr.SelfBranded, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    _selfBranded = bool.Parse(value);

                value = data.GetAttribute(ConfigurationAttr.TopicVersion, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    _topicVersion = value;

                value = data.GetAttribute(ConfigurationAttr.TocParent, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    _tocParent = value;

                value = data.GetAttribute(ConfigurationAttr.TocParentVersion, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    _tocParentVersion = value;

                value = data.GetAttribute(ConfigurationAttr.TocFile, string.Empty);
                if (!string.IsNullOrEmpty(value))
                    tocFile = value;
            }
            LoadToc(Path.GetFullPath(Environment.ExpandEnvironmentVariables(tocFile)));
        }

        #region Public
        /// <summary>
        /// Applies Microsoft Help System transformation to the output document.
        /// </summary>
        /// <param name="document">The <see cref="XmlDocument"/> to apply transformation to.</param>
        /// <param name="key">Topic key of the output document.</param>
        public override void Apply(XmlDocument document, string key)
        {
            _document = document;

            ModifyAttribute("id", "mainSection");
            ModifyAttribute("class", "members");
            FixHeaderBottomBackground("nsrBottom", "headerBottom");

            XmlElement html = _document.DocumentElement;
            _head = html.SelectSingleNode(Help2XPath.Head);
            if (_head == null)
            {
                _head = document.CreateElement(Help2XPath.Head);
                if (!html.HasChildNodes)
                    html.AppendChild(_head);
                else
                    html.InsertBefore(_head, html.FirstChild);
            }

            AddMHSMeta(MHSMetaName.SelfBranded, _selfBranded.ToString().ToLower());
            AddMHSMeta(MHSMetaName.ContentType, MHSDefault.Reference);
            AddMHSMeta(MHSMetaName.TopicVersion, _topicVersion);

            string locale = _locale;
            string id = Guid.NewGuid().ToString();
            _xml = _head.SelectSingleNode(Help2XPath.Xml);
            if (_xml != null)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(_document.NameTable);
                if (!nsmgr.HasNamespace(Help2Namespace.Prefix))
                    nsmgr.AddNamespace(Help2Namespace.Prefix, Help2Namespace.Uri);

                XmlElement elem = _xml.SelectSingleNode(Help2XPath.TocTitle, nsmgr) as XmlElement;
                if (elem != null)
                    AddMHSMeta(MHSMetaName.Title, elem.GetAttribute(Help2Attr.Title));

                foreach (XmlElement keyword in _xml.SelectNodes(string.Format(Help2XPath.Keyword, Help2Value.K), nsmgr))
                    AddMHSMeta(MHSMetaName.Keywords, keyword.GetAttribute(Help2Attr.Term), true);

                foreach (XmlElement keyword in _xml.SelectNodes(string.Format(Help2XPath.Keyword, Help2Value.F), nsmgr))
                    AddMHSMeta(MHSMetaName.F1, keyword.GetAttribute(Help2Attr.Term), true);

                foreach (XmlElement lang in _xml.SelectNodes(string.Format(Help2XPath.Attr, Help2Value.DevLang), nsmgr))
                    AddMHSMeta(MHSMetaName.Category, Help2Value.DevLang + ":" + lang.GetAttribute(Help2Attr.Value), true);

                elem = _xml.SelectSingleNode(string.Format(Help2XPath.Attr, Help2Value.Abstract), nsmgr) as XmlElement;
                if (elem != null)
                    AddMHSMeta(MHSMetaName.Description, elem.GetAttribute(Help2Attr.Value));

                elem = _xml.SelectSingleNode(string.Format(Help2XPath.Attr, Help2Value.AssetID), nsmgr) as XmlElement;
                if (elem != null)
                    id = elem.GetAttribute(Help2Attr.Value);

                if (string.IsNullOrEmpty(locale))
                {
                    elem = _xml.SelectSingleNode(string.Format(Help2XPath.Attr, Help2Value.Locale), nsmgr) as XmlElement;
                    if (elem != null)
                        locale = elem.GetAttribute(Help2Attr.Value);
                }
            }

            if (string.IsNullOrEmpty(locale))
                locale = MHSDefault.Locale;

            AddMHSMeta(MHSMetaName.Locale, locale);
            AddMHSMeta(MHSMetaName.TopicLocale, locale);
            AddMHSMeta(MHSMetaName.Id, id);

            if (_toc.ContainsKey(id))
            {
                TocInfo tocInfo = _toc[id];
                AddMHSMeta(MHSMetaName.TocParent, tocInfo.Parent);
                if (tocInfo.Parent != MHSDefault.TocParent)
                    AddMHSMeta(MHSMetaName.TocParentVersion, tocInfo.ParentVersion);
                AddMHSMeta(MHSMetaName.TocOrder, tocInfo.Order.ToString());
            }

        }

        #endregion

        #region Private
        // loads TOC structure from a file
        private void LoadToc(string path)
        {
            _toc.Clear();
            using (Stream stream = File.OpenRead(path))
            {
                XPathDocument document = new XPathDocument(stream);
                XPathNavigator navigator = document.CreateNavigator();
                LoadToc(navigator.SelectSingleNode(TocXPath.Topics), _tocParent, _tocParentVersion);
            }
        }
        // loads TOC structure from an XPathNavigator
        private void LoadToc(XPathNavigator navigator, string parent, string parentVersion)
        {
            int i = -1;
            XPathNodeIterator interator = navigator.SelectChildren(TocXPath.Topic, string.Empty);
            while (interator.MoveNext())
            {
                XPathNavigator current = interator.Current;
                string id = current.GetAttribute(TocAttr.Id, string.Empty);
                if (!string.IsNullOrEmpty(id))
                {
                    TocInfo info = new TocInfo(parent, parentVersion, ++i);
                    _toc.Add(id, info);
                    LoadToc(current, id, _topicVersion);
                }
            }
        }
        
        // Adds Microsoft Help System meta data to the output document
        private XmlElement AddMHSMeta(string name, string content)
        {
            return AddMHSMeta(name, content, false);
        }

        // Adds Microsoft Help System meta data to the output document
        private XmlElement AddMHSMeta(string name, string content, bool multiple)
        {
            if (string.IsNullOrEmpty(content))
                return null;
            XmlElement elem = null;
            if (!multiple)
                elem = _document.SelectSingleNode(string.Format(@"//meta[@{0}]", name)) as XmlElement;
            if (elem == null)
            {
                elem = _document.CreateElement(MHSTag.Meta);
                elem.SetAttribute(MHSMetaAttr.Name, name);
                elem.SetAttribute(MHSMetaAttr.Content, content);
                _head.AppendChild(elem);
            }
            return elem;
        }

        // Modifies an attribute value to prevent conflicts with Microsoft Help System branding
        private void ModifyAttribute(string name, string value)
        {
            XmlNodeList list = _document.SelectNodes(string.Format(@"//*[@{0}='{1}']", name, value));
            foreach (XmlElement elem in list)
                elem.SetAttribute(name, value + MHSDefault.ShortName);
        }

        // Works around a Microsoft Help System issue ('background' attribute isn't supported):
        // adds a hidden image so that its path will be transformed by MHS runtime handler,
        // sets the 'background' attribute to the transformed path on page load
        private void FixHeaderBottomBackground(string className, string newId)
        {
            XmlElement elem = _document.SelectSingleNode(string.Format(@"//*[@class='{0}']", className)) as XmlElement;
            if (elem == null)
                return;

            string src = elem.GetAttribute("background");
            if (string.IsNullOrEmpty(src))
                return;
            elem.SetAttribute("id", newId);

            XmlElement img = _document.CreateElement("img");
            img.SetAttribute("src", src);
            img.SetAttribute("id", newId + "Image");
            img.SetAttribute("style", "display: none");

            elem.AppendChild(img);
        }

        #endregion
    }
}
