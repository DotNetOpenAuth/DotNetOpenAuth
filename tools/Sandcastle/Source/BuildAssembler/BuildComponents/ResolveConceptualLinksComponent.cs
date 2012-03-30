// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Text.RegularExpressions;
using System.Web;


namespace Microsoft.Ddue.Tools {

	public class ResolveConceptualLinksComponent : BuildComponent {

        // Instantiation logic

		public ResolveConceptualLinksComponent (BuildAssembler assembler, XPathNavigator configuration) : base(assembler, configuration) {

            string showBrokenLinkTextValue = configuration.GetAttribute("showBrokenLinkText", String.Empty);
            if (!String.IsNullOrEmpty(showBrokenLinkTextValue)) showBrokenLinkText = Convert.ToBoolean(showBrokenLinkTextValue);

			XPathNodeIterator targetsNodes = configuration.Select("targets");
			foreach (XPathNavigator targetsNode in targetsNodes) {

                // the base directory containing target; required
                string baseValue = targetsNode.GetAttribute("base", String.Empty);
                if (String.IsNullOrEmpty(baseValue)) WriteMessage(MessageLevel.Error, "Every targets element must have a base attribute that specifies the path to a directory of target metadata files.");
                baseValue = Environment.ExpandEnvironmentVariables(baseValue);
                if (!Directory.Exists(baseValue)) WriteMessage(MessageLevel.Error, String.Format("The specified target metadata directory '{0}' does not exist.", baseValue));

                // an xpath expression to construct a file name
                // (not currently used; pattern is hard-coded to $target.cmp.xml
                string filesValue = targetsNode.GetAttribute("files", String.Empty);

                // an xpath expression to construct a url
                string urlValue = targetsNode.GetAttribute("url", String.Empty);
                XPathExpression urlExpression;
                if (String.IsNullOrEmpty(urlValue)) {
                    urlExpression = XPathExpression.Compile("concat(/metadata/topic/@id,'.htm')");
                } else {
                    urlExpression = CompileXPathExpression(urlValue);
                }
                
                // an xpath expression to construct link text
                string textValue = targetsNode.GetAttribute("text", String.Empty);
                XPathExpression textExpression;
                if (String.IsNullOrEmpty(textValue)) {
                    textExpression = XPathExpression.Compile("string(/metadata/topic/title)");
                } else {
                    textExpression = CompileXPathExpression(textValue);
                }

                // the type of link to create to targets found in the directory; required
                string typeValue = targetsNode.GetAttribute("type", String.Empty);
                if (String.IsNullOrEmpty(typeValue)) WriteMessage(MessageLevel.Error, "Every targets element must have a type attribute that specifies what kind of link to create to targets found in that directory.");
                
                // convert the link type to an enumeration member
                LinkType type = LinkType.None;
                try {
                    type = (LinkType) Enum.Parse(typeof(LinkType), typeValue, true);
                } catch (ArgumentException) {
                    WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a valid link type.", typeValue));
                }

                // we have all the required information; create a TargetDirectory and add it to our collection
                TargetDirectory targetDirectory = new TargetDirectory(baseValue, urlExpression, textExpression, type);
                targetDirectories.Add(targetDirectory);

            }

            WriteMessage(MessageLevel.Info, String.Format("Collected {0} targets directories.", targetDirectories.Count));	

		}

        private XPathExpression CompileXPathExpression (string xpath) {
            XPathExpression expression = null;
            try {
                expression = XPathExpression.Compile(xpath);
            } catch (ArgumentException e) {
                WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a valid XPath expression. The error message is: {1}", xpath, e.Message));
            } catch (XPathException e) {
                WriteMessage(MessageLevel.Error, String.Format("'{0}' is not a valid XPath expression. The error message is: {1}", xpath, e.Message));
            }
            return (expression);
        }

        // Conceptual link resolution logic

		public override void Apply (XmlDocument document, string key) {
			ResolveConceptualLinks(document, key);
		}

        private bool showBrokenLinkText = false;

        private string BrokenLinkDisplayText (string target, string text) {
            if (showBrokenLinkText) {
                return(String.Format("{0}", text));
            } else {
                return(String.Format("[{0}]", target));
            }
        }

        private TargetDirectoryCollection targetDirectories = new TargetDirectoryCollection();

        private static XPathExpression conceptualLinks = XPathExpression.Compile("//conceptualLink");

        private static Regex validGuid = new Regex(@"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$", RegexOptions.Compiled);

		private void ResolveConceptualLinks (XmlDocument document, string key) {

			// find links
			XPathNodeIterator linkIterator = document.CreateNavigator().Select(conceptualLinks);

			// copy them to an array, because enumerating through an XPathNodeIterator
			// fails when the nodes in it are altered
			XPathNavigator[] linkNodes = BuildComponentUtilities.ConvertNodeIteratorToArray(linkIterator);
			
			foreach (XPathNavigator linkNode in linkNodes) {

                ConceptualLinkInfo link = ConceptualLinkInfo.Create(linkNode);

                // determine url, text, and link type
                string url = null;
                string text = null;
                LinkType type = LinkType.None;
                bool isValidLink = validGuid.IsMatch(link.Target);
                if (isValidLink) {
                    // a valid link; try to fetch target info
                    TargetInfo target = GetTargetInfoFromCache(link.Target.ToLower());
                    if (target == null) {
                        // no target found; issue warning, set link style to none, and text to in-source fall-back
                        //type = LinkType.None;
                        type = LinkType.Index;
                        text = BrokenLinkDisplayText(link.Target, link.Text);
                        WriteMessage(MessageLevel.Warn, String.Format("Unknown conceptual link target '{0}'.", link.Target));
                    } else {
                        // found target; get url, text, and type from stored info
                        url = target.Url;
                        text = target.Text;
                        type = target.Type;
                    }
                } else {
                    // not a valid link; issue warning, set link style to none, and text to invalid target
                    //type = LinkType.None;
                    type = LinkType.Index;
                    text = BrokenLinkDisplayText(link.Target, link.Text);
                    WriteMessage(MessageLevel.Warn, String.Format("Invalid conceptual link target '{0}'.", link.Target));
                }

                // write opening link tag and target info
                XmlWriter writer = linkNode.InsertAfter();
                switch (type) {
                    case LinkType.None:
                        writer.WriteStartElement("span");
                        writer.WriteAttributeString("class", "nolink");
                        break;
                    case LinkType.Local:
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", url);
                        break;
                    case LinkType.Index:
                        writer.WriteStartElement("mshelp", "link", "http://msdn.microsoft.com/mshelp");
                        writer.WriteAttributeString("keywords", link.Target.ToLower());
                        writer.WriteAttributeString("tabindex", "0");
                        break;
                    case LinkType.Id:
                        string xhelp = String.Format("ms-xhelp://?Id={0}", link.Target);
                        writer.WriteStartElement("a");
                        writer.WriteAttributeString("href", xhelp);
                        break;
                }

                // write the link text
                writer.WriteString(text);

                // write the closing link tag
                writer.WriteEndElement();
                writer.Close();

                // delete the original tag
                linkNode.DeleteSelf();
            }
		}




		// a simple caching system for target names

		private TargetInfo GetTargetInfoFromCache (string target) {

			TargetInfo info;
			if (!cache.TryGetValue(target, out info)) {
				info = targetDirectories.GetTargetInfo(target + ".cmp.xml");

				if (cache.Count >= cacheSize) cache.Clear();

				cache.Add(target, info);
			}

            return(info);

		}

		private static int cacheSize = 1000;

		private Dictionary<string,TargetInfo> cache = new Dictionary<string,TargetInfo>(cacheSize);

        //private CustomContext context = new CustomContext();

	}

	// different types of links

	internal enum LinkType {
		None,		// not active
		Local,		// a href
		Index,  	// mshelp:link keyword
        Id          // ms-xhelp link
        //Regex       // regular expression with match/replace
	}

    // a representation of a targets directory, along with all the assoicated expressions used to
    // find target metadat files in it, and extract urls and link text from those files

    internal class TargetDirectory {

        private string directory;

        private XPathExpression fileExpression = XPathExpression.Compile("concat($target,'.cmp.htm')");

        private XPathExpression urlExpression = XPathExpression.Compile("concat(/metadata/topic/@id,'.htm')");

        private XPathExpression textExpression = XPathExpression.Compile("string(/metadata/topic/title)");

        private LinkType type;

        public string Directory {
            get {
                return (directory);
            }
        }

        public XPathExpression UrlExpression {
            get {
                return (urlExpression);
            }
        }

        public XPathExpression TextExpression {
            get {
                return (textExpression);
            }
        }


        public LinkType LinkType {
            get {
                return (type);
            }
        }

        public TargetDirectory (string directory, LinkType type) {
            if (directory == null) throw new ArgumentNullException("directory");
            this.directory = directory;
            this.type = type;
        }

        public TargetDirectory (string directory, XPathExpression urlExpression, XPathExpression textExpression, LinkType type) {
            if (directory == null) throw new ArgumentNullException("directory");
            if (urlExpression == null) throw new ArgumentNullException("urlExpression");
            if (textExpression == null) throw new ArgumentNullException("textExpression");
            this.directory = directory;
            this.urlExpression = urlExpression;
            this.textExpression = textExpression;
            this.type = type;
        }

        private XPathDocument GetDocument (string file) {
            string path = Path.Combine(directory, file);
            if (File.Exists(path)) {
                XPathDocument document = new XPathDocument(path);
                return (document);
            } else {
                return (null);
            }
        }

        public TargetInfo GetTargetInfo (string file) {
            XPathDocument document = GetDocument(file);
            if (document == null) {
                return(null);
            } else {
                XPathNavigator context = document.CreateNavigator();

                string url = context.Evaluate(urlExpression).ToString();
                string text = context.Evaluate(textExpression).ToString();
                TargetInfo info = new TargetInfo(url, text, type);
                return(info);
            }
        }

        public TargetInfo GetTargetInfo (XPathNavigator linkNode, CustomContext context) {

            // compute the metadata file name to open
            XPathExpression localFileExpression = fileExpression.Clone();
            localFileExpression.SetContext(context);
            string file = linkNode.Evaluate(localFileExpression).ToString();
            if (String.IsNullOrEmpty(file)) return (null);

            // load the metadata file
            XPathDocument metadataDocument = GetDocument(file);
            if (metadataDocument == null) return (null);

            // querry the metadata file for the target url and link text
            XPathNavigator metadataNode = metadataDocument.CreateNavigator();
            XPathExpression localUrlExpression = urlExpression.Clone();
            localUrlExpression.SetContext(context);
            string url = metadataNode.Evaluate(localUrlExpression).ToString();
            XPathExpression localTextExpression = textExpression.Clone();
            localTextExpression.SetContext(context);
            string text = metadataNode.Evaluate(localTextExpression).ToString();

            // return this information
            TargetInfo info = new TargetInfo(url, text, type);
            return (info);
        }



    }

    // our collection of targets directories

    internal class TargetDirectoryCollection {

        public TargetDirectoryCollection() {}

        private List<TargetDirectory> targetDirectories = new List<TargetDirectory>();

        public int Count {
            get {
                return(targetDirectories.Count);
            }
        }

        public void Add (TargetDirectory targetDirectory) {
            targetDirectories.Add(targetDirectory);
        }

        public TargetInfo GetTargetInfo (string file) {
            foreach (TargetDirectory targetDirectory in targetDirectories) {
                TargetInfo info = targetDirectory.GetTargetInfo(file);
                if (info != null) return (info);
            }
            return (null);
        }

        public TargetInfo GetTargetInfo (XPathNavigator linkNode, CustomContext context) {
            foreach (TargetDirectory targetDirectory in targetDirectories) {
                TargetInfo info = targetDirectory.GetTargetInfo(linkNode, context);
                if (info != null) return (info);
            }
            return (null);
        }

    }

    // a representation of a resolved target, containing all the information necessary to actually write out the link

    internal class TargetInfo {

        private string url;

        private string text;

        private LinkType type;

        public string Url {
            get {
                return(url);
            }
        }

        public string Text {
            get {
                return(text);
            }
        }

        public LinkType Type {
            get {
                return(type);
            }
        }

        internal TargetInfo (string url, string text, LinkType type) {
            if (url == null) throw new ArgumentNullException("url");
            if (text == null) throw new ArgumentNullException("url");
            this.url = url;
            this.text = text;
            this.type = type;
        }
    }

    // a representation of a conceptual link

    internal class ConceptualLinkInfo {

        private string target;

        private string text;

        private bool showText = false;

        public string Target {
            get {
                return (target);
            }
        }

        public string Text {
            get {
                return (text);
            }
        }

        public bool ShowText {
            get {
                return (showText);
            }
        }

        private ConceptualLinkInfo () { }

        public static ConceptualLinkInfo Create (XPathNavigator node) {
            if (node == null) throw new ArgumentNullException("node");
            
            ConceptualLinkInfo info = new ConceptualLinkInfo();
            
            info.target = node.GetAttribute("target", String.Empty);
            info.text = node.ToString();
            
            return(info);
        }

    }

}