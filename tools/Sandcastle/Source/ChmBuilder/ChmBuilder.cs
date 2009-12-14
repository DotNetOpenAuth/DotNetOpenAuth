// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Microsoft.Ddue.Tools.CommandLine;


namespace Microsoft.Ddue.Tools
{

    /// <summary>
    /// <language id="1033" codepage="65001" name="0x409 English (United States)" />
    /// <language id="2052" codepage="936" name="0x804 Chinese (PRC)" />
    /// </summary>
    internal struct LangInfo
    {
        public int CodePage;
        public int ID;
        public string Name;
    }

    internal struct KKeywordInfo
    {
        public string File;
        public string MainEntry;
        public string SubEntry;
    }

    public class ChmBuilder
    {

        private ChmBuilderArgs _args;
        private XPathDocument _config;
        //private bool _metadata;

        //defalut topic of chm: get this value when gerenating hhc, save to hhp
        private string _defaultTopic = string.Empty;
        private bool _hasToc;

        private int _indentCount = 0;
        //private string _htmlDirectory;
        //private string _tocFile;
        //private string _projectName;
        //private string _outputDirectory;
        private LangInfo _lang;

        //store all "K" type Keywords
        List < KKeywordInfo > kkwdTable = new List < KKeywordInfo >();

        //store all titles from html files
        Hashtable titleTable = new Hashtable();


        public ChmBuilder(ChmBuilderArgs args)
        {
            this._args = args;
            _args.htmlDirectory = StripEndBackSlash(Path.GetFullPath(_args.htmlDirectory));
            if (String.IsNullOrEmpty(_args.tocFile))
                _hasToc = false;
            else
                _hasToc = true;
            _args.outputDirectory = StripEndBackSlash(Path.GetFullPath(_args.outputDirectory));
            _config = new XPathDocument(args.configFile);
            LoadLanginfo(_args.langid);
        }
        public static int Main(string[] args)
        {
            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new StringOption("html", "Specify a html directory.", "htmlDirectory"));
            options.Add(new StringOption("project", "Specify a project name.", "projectName"));
            options.Add(new StringOption("toc", "Specify a toc file.", "tocFile"));
            options.Add(new StringOption("lcid", "Specify a language id.If unspecified, 1033 is used.", "languageId"));
            options.Add(new StringOption("out", "Specify an output directory. If unspecified, Chm is used.", "outputDirectory"));
            options.Add(new BooleanOption("metadata", "Specify whether output metadata or not. Default value is false."));
            options.Add(new StringOption("config", "Specify a configuration file. If unspecified, ChmBuilder.config is used", "configFilePath"));

            ParseArgumentsResult results = options.ParseArguments(args);
            if (results.Options["?"].IsPresent)
            {
                Console.WriteLine("ChmBuilder /html: /project: /toc: /out: /metadata:");
                options.WriteOptionSummary(Console.Out);
                return (0);
            }

            ChmBuilderArgs cbArgs = new ChmBuilderArgs();

            // check for invalid options
            if (!results.Success)
            {
                results.WriteParseErrors(Console.Out);
                return (1);
            }

            // check for missing or extra assembly directories
            if (results.UnusedArguments.Count != 0)
            {
                Console.WriteLine("No non-option arguments are supported.");
                return (1);
            }

            if (!results.Options["html"].IsPresent)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "You must specify a html directory.");
                return (1);
            }
            cbArgs.htmlDirectory = (string)results.Options["html"].Value;
            if (!Directory.Exists(cbArgs.htmlDirectory))
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Direcotry: {0} not found.", cbArgs.htmlDirectory));
                return (1);
            }

            if (!results.Options["project"].IsPresent)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, "You must specify a project name.");
                return (1);
            }
            cbArgs.projectName = (string)results.Options["project"].Value;

            if (results.Options["lcid"].IsPresent)
            {
                try
                {
                    cbArgs.langid = Convert.ToInt32(results.Options["lcid"].Value);
                }
                catch
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("{0} is not a valid integer.", results.Options["lcid"].Value));
                    return (1);
                }
            }


            if (results.Options["toc"].IsPresent)
            {
                cbArgs.tocFile = (string)results.Options["toc"].Value;
                if (!File.Exists(cbArgs.tocFile))
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("File: {0} not found.", cbArgs.tocFile));
                    return (1);
                }
            }

            if (!results.Options["out"].IsPresent)
                cbArgs.outputDirectory = "Chm";
            else
                cbArgs.outputDirectory = (string)results.Options["out"].Value;
            if (!Directory.Exists(cbArgs.outputDirectory))
            {
                Directory.CreateDirectory(cbArgs.outputDirectory);
                //ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Direcotry: {0} not found.", cbArgs.outputDirectory));
                //return (1);
            }

            if (results.Options["metadata"].IsPresent && (bool)results.Options["metadata"].Value)
            {
                cbArgs.metadata = true;
            }

            if (results.Options["config"].IsPresent)
            {
                cbArgs.configFile = (string)results.Options["config"].Value;
            }
            if (!File.Exists(cbArgs.configFile))
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Config file: {0} not found.", cbArgs.configFile));
                return (1);
            }

            try
            {
                ChmBuilder chmBuilder = new ChmBuilder(cbArgs);
                chmBuilder.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return (1);
            }
            return 0;
        }

        //there are some special characters in hxs html, just convert them to what we want
        public static string ReplaceMarks(string input)
        {
            string ret = input.Replace("%3C", "<");
            ret = ret.Replace("%3E", ">");
            ret = ret.Replace("%2C", ",");
            return ret;
        }

        /// <summary>
        /// eg: "c:\tmp\" to "c:\tmp"
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string StripEndBackSlash(string dir)
        {
            if (dir.EndsWith("\\"))
                return dir.Substring(0, dir.Length - 1);
            else
                return dir;
        }

        public void Run()
        {
            WriteHtmls();
            WriteHhk();
            if (_hasToc) WriteHhc();
            WriteHhp();
        }

        private static int CompareKeyword(KKeywordInfo x, KKeywordInfo y)
        {
            if (x.MainEntry != y.MainEntry)
                return (x.MainEntry.CompareTo(y.MainEntry));
            else
            {
                string s1 = x.SubEntry;
                string s2 = y.SubEntry;
                if (s1 == null)
                    s1 = string.Empty;
                if (s2 == null)
                    s2 = string.Empty;
                return (s1.CompareTo(s2));
            }
        }

        /// <summary>
        /// read chmTitle from chmBuilder.config
        /// </summary>
        /// <returns></returns>
        private string GetChmTitle()
        {

            XPathNodeIterator iter = _config.CreateNavigator().Select("/configuration/chmTitles/title");
            while (iter.MoveNext())
            {
                if (iter.Current.GetAttribute("projectName", string.Empty).ToLower() == _args.projectName.ToLower())
                    return iter.Current.Value;
            }

            //if no title found, set title to projectname
            return _args.projectName;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InsertSeealsoIndice()
        {
            kkwdTable.Sort(CompareKeyword);
            string lastMainEntry = string.Empty;
            for (int i = 0; i < kkwdTable.Count; i++)
            {
                if (!string.IsNullOrEmpty(kkwdTable[i].SubEntry))
                {
                    if (i > 0)
                        lastMainEntry = kkwdTable[i - 1].MainEntry;
                    if (lastMainEntry != kkwdTable[i].MainEntry)
                    {
                        KKeywordInfo seealso = new KKeywordInfo();
                        seealso.MainEntry = kkwdTable[i].MainEntry;
                        kkwdTable.Insert(i, seealso);
                    }
                }
            }
        }

        /// <summary>
        /// load language info from config file
        /// </summary>
        /// <param name="lcid"></param>
        private void LoadLanginfo(int lcid)
        {
            XPathNavigator node = _config.CreateNavigator().SelectSingleNode(String.Format("/configuration/languages/language[@id='{0}']", lcid.ToString()));
            if (node != null)
            {
                _lang = new LangInfo();
                _lang.ID = lcid;
                _lang.CodePage = Convert.ToInt32(node.GetAttribute("codepage", string.Empty));
                _lang.Name = node.GetAttribute("name", string.Empty);
            }
            else
            {
                throw new ArgumentException(String.Format("language {0} is not found in config file.", lcid));
            }
        }

        private void WriteHhc()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(_args.tocFile, settings);

            //<param name="Local" value="Html\15ed547b-455d-808c-259e-1eaa3c86dccc.htm"> 
            //"html" before GUID
            string _localFilePrefix = _args.htmlDirectory.Substring(_args.htmlDirectory.LastIndexOf('\\') + 1);

            string fileAttr;
            string titleValue;
            using (StreamWriter sw = new StreamWriter(String.Format("{0}\\{1}.hhc", _args.outputDirectory, _args.projectName), false, Encoding.GetEncoding(_lang.CodePage)))
            {
                sw.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML/EN\">");
                sw.WriteLine("<HTML>");
                sw.WriteLine("  <BODY>");

                bool bDefaultTopic = true;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "topic")
                            {
                                _indentCount = reader.Depth;
                                fileAttr = reader.GetAttribute("file") + ".htm";
                                if (titleTable.Contains(fileAttr))
                                    titleValue = (string)titleTable[fileAttr];
                                else
                                    titleValue = String.Empty;

                                WriteHhcLine(sw, "<UL>");
                                WriteHhcLine(sw, "  <LI><OBJECT type=\"text/sitemap\">");
                                WriteHhcLine(sw, String.Format("    <param name=\"Name\" value=\"{0}\">", titleValue));
                                WriteHhcLine(sw, String.Format("    <param name=\"Local\" value=\"{0}\\{1}\">", _localFilePrefix, fileAttr));
                                if (bDefaultTopic)
                                {
                                    _defaultTopic = _localFilePrefix + "\\" + reader.GetAttribute("file") + ".htm";
                                    bDefaultTopic = false;
                                }
                                WriteHhcLine(sw, "  </OBJECT></LI>");
                                if (reader.IsEmptyElement)
                                {
                                    WriteHhcLine(sw, "</UL>");
                                }
                            }
                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name == "topic")
                            {
                                _indentCount = reader.Depth;
                                WriteHhcLine(sw, "</UL>");
                            }
                            break;

                        default:
                            //Console.WriteLine(reader.Name);
                            break;
                    }
                }
                sw.WriteLine("  </BODY>");
                sw.WriteLine("</HTML>");
            }
        }

        private void WriteHhcLine(TextWriter writer, string value)
        {
            //write correct indent space
            writer.WriteLine();
            for (int i = 0; i < _indentCount - 1; i++)
                writer.Write("  ");
            writer.Write(value);
        }

        private void WriteHhk()
        {
            int iPrefix = _args.outputDirectory.Length + 1;
            bool isIndent = false;


            InsertSeealsoIndice();
            using (StreamWriter sw = new StreamWriter(String.Format("{0}\\{1}.hhk", _args.outputDirectory, _args.projectName), false, Encoding.GetEncoding(_lang.CodePage)))
            {
                sw.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML/EN\">");
                sw.WriteLine("<HTML>");
                sw.WriteLine("  <BODY>");
                sw.WriteLine("    <UL>");

                foreach (KKeywordInfo ki in kkwdTable)
                {
                    if (!string.IsNullOrEmpty(ki.MainEntry))
                    {
                        string kwdValue = ki.MainEntry;
                        if (!string.IsNullOrEmpty(ki.SubEntry))
                        {
                            if (!isIndent)
                            {
                                isIndent = true;
                                sw.WriteLine("    <UL>");
                            }
                            kwdValue = ki.SubEntry;
                        }
                        else
                        {
                            if (isIndent)
                            {
                                isIndent = false;
                                sw.WriteLine("    </UL>");
                            }
                        }

                        sw.WriteLine("      <LI><OBJECT type=\"text/sitemap\">");
                        sw.WriteLine(String.Format("        <param name=\"Name\" value=\"{0}\">", kwdValue));
                        if (String.IsNullOrEmpty(ki.File))
                            sw.WriteLine(String.Format("        <param name=\"See Also\" value=\"{0}\">", kwdValue));
                        else
                            sw.WriteLine(String.Format("        <param name=\"Local\" value=\"{0}\">", ki.File.Substring(iPrefix)));
                        sw.WriteLine("      </OBJECT><LI>");
                    }
                }

                sw.WriteLine("    </UL>");
                sw.WriteLine("  </BODY>");
                sw.WriteLine("</HTML>");
            }
        }


        /// <summary>
        /// In hhp.template, {0} is projectName, {1} is defalutTopic, {2}:Language, {3}:Title 
        /// </summary>
        private void WriteHhp()
        {
            string hhpFile = String.Format("{0}\\{1}.hhp", _args.outputDirectory, _args.projectName);
            Encoding ei = Encoding.GetEncoding(_lang.CodePage);

            using (FileStream writer = File.OpenWrite(hhpFile))
            {
                string var0 = _args.projectName;
                string var1 = _defaultTopic;
                string var2 = _lang.Name;
                string var3 = GetChmTitle();

                XPathNodeIterator iter = _config.CreateNavigator().Select("/configuration/hhpTemplate/line");

                while (iter.MoveNext())
                {
                    String line = iter.Current.Value;
                    AddText(writer, String.Format(line, var0, var1, var2, var3), ei);
                    AddText(writer, "\r\n", ei);
                }
            }
        }

        private void AddText(FileStream fs, string value, Encoding ei)
        {
            byte[] info = ei.GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        private void WriteHtmls()
        {
            string _outhtmldir = _args.outputDirectory + _args.htmlDirectory.Substring(_args.htmlDirectory.LastIndexOf('\\'));
            HxsChmConverter converter = new HxsChmConverter(_args.htmlDirectory, _outhtmldir, titleTable, kkwdTable, _args.metadata);
            converter.Process();
        }
    }

    /// <summary>
    /// Convert hxs-ready html page to chm-ready page
    /// 1. strip of xmlisland;
    /// 2. <mshelp:link> link tiltle </link> ==> <span class="nolink">link title</span>
    /// </summary>
    internal class HxsChmConverter
    {
        private string _currentFile;
        private string _currentTitle;
        private string _htmlDir;
        List < KKeywordInfo > _kkeywords;
        private bool _metadata;
        private string _outputDir;

        Hashtable _titles;

        private int _topicCount = 0;

        public HxsChmConverter(string htmlDir, string outputDir, Hashtable titles, List < KKeywordInfo > kkeywords, bool metadata)
        {
            _htmlDir = htmlDir;
            _outputDir = outputDir;
            _titles = titles;
            _kkeywords = kkeywords;
            _metadata = metadata;
        }

        public void Process()
        {
            _topicCount = 0;
            ProcessDirectory(_htmlDir, _outputDir);
            Console.WriteLine("Processed {0} files.", _topicCount);
        }

        private void ProcessDirectory(string srcDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            string[] fileEntries = Directory.GetFiles(srcDir);
            foreach (string fileName in fileEntries)
            {
                string destFile = destDir + fileName.Substring(fileName.LastIndexOf('\\'));

                FileInfo fi = new FileInfo(fileName);
                string extion = fi.Extension.ToLower();
                //process .htm and .html files, just copy other files, like css, gif. TFS DCR 318537
                if (extion == ".htm" || extion == ".html")
                {
                    try
                    {
                        ProcessFile(fileName, destFile);
                    }
                        /*
                    catch (XmlException ex)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Invalid XML file {0}", fileName));
                        ConsoleApplication.WriteMessage(LogLevel.Error, ex.Message);
                        _stop = true;
                        return;
                    }
                     */
                    catch (Exception)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("failed to process file {0}", fileName));
                        throw;
                    }
                }
                else
                    File.Copy(fileName, destFile, true);
            }

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(srcDir, "*", SearchOption.TopDirectoryOnly);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                string newSubdir = destDir + "\\" + di.Name;
                ProcessDirectory(subdirectory, newSubdir);
            }
        }

        private void ProcessFile(string srcFile, string destFile)
        {
            //Console.WriteLine("Processing:{0}",srcFile);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = false;
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(srcFile, settings);

            XmlWriterSettings settings2 = new XmlWriterSettings();
            settings2.Indent = false;
            settings2.IndentChars = "\t";
            settings2.OmitXmlDeclaration = true;
            XmlWriter writer = XmlWriter.Create(destFile, settings2);

            _currentTitle = String.Empty;
            _currentFile = destFile;

            _topicCount++;

            while (reader.Read())
            {
                if (_metadata == false && reader.Name.ToLower() == "xml" && reader.NodeType == XmlNodeType.Element)
                {
                    //skip xml data island
                    reader.ReadOuterXml();
                }

                switch (reader.NodeType)
                {

                    case XmlNodeType.Element:
                        string elementName = reader.Name.ToLower();

                        //skip <mshelp:link> node, 
                        if (elementName == "mshelp:link")
                        {
                            writer.WriteStartElement("span");
                            writer.WriteAttributeString("class", "nolink");
                            reader.MoveToContent();
                        }

                        else
                        {
                            if (!String.IsNullOrEmpty(reader.Prefix))
                                writer.WriteStartElement(reader.Prefix, reader.LocalName, null);
                            else
                                writer.WriteStartElement(reader.Name);

                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    if (!String.IsNullOrEmpty(reader.Prefix))
                                        writer.WriteAttributeString(reader.Prefix, reader.LocalName, null, reader.Value);
                                    else
                                        //If we write the following content to output file, we will get xmlexception saying the 2003/5 namespace is redefined. So hard code to skip "xmlns".
                                        //<pre>My.Computer.FileSystem.RenameFile(<span class="literal" xmlns="http://ddue.schemas.microsoft.com/authoring/2003/5">
                                        if (!(reader.Depth > 2 && reader.Name.StartsWith("xmlns")))
                                            writer.WriteAttributeString(reader.Name, reader.Value);
                                }
                                // Move the reader back to the element node.
                                reader.MoveToElement();
                            }

                            //read html/head/title, save it to _currentTitle
                            if (reader.Depth == 2 && elementName == "title")
                            {
                                if (!reader.IsEmptyElement) //skip <Title/> node, fix bug 425406
                                {
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.Text)
                                    {
                                        _currentTitle = reader.Value;
                                        writer.WriteRaw(reader.Value);
                                    }
                                }
                            }

                            if (reader.IsEmptyElement)
                                writer.WriteEndElement();
                        }
                        break;

                    case XmlNodeType.Text:
                        writer.WriteValue(reader.Value);
                        break;

                    case XmlNodeType.EndElement:
                        writer.WriteFullEndElement();
                        break;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        writer.WriteWhitespace(reader.Value);
                        break;


                    default:
                        //Console.WriteLine(reader.Name);
                        break;
                }
            }

            writer.Close();

            ReadXmlIsland(srcFile);

            _titles.Add(destFile.Substring(destFile.LastIndexOf("\\") + 1), _currentTitle);
        }


        /// <summary>
        /// As XmlReader is forward only and we added support for leaving xmlisland data. 
        /// We have to use another xmlreader to find TocTile, keywords etc.
        /// </summary>
        /// <param name="filename"></param>
        private void ReadXmlIsland(string filename)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = false;
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(filename, settings);

            //Fix TFS bug 289403: search if there is comma in k keyword except those in () or <>. 
            //sample1: "StoredNumber (T1,T2) class, about StoredNumber (T1,T2) class";
            //sample2: "StoredNumber <T1,T2> class, about StoredNumber <T1,T2> class";
            Regex r = new Regex(@",([^\)\>]+|([^\<\>]*\<[^\<\>]*\>[^\<\>]*)?|([^\(\)]*\([^\(\)]*\)[^\(\)]*)?)$");

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.Name.ToLower() == "mshelp:toctitle")
                    {
                        string titleAttr = reader.GetAttribute("Title");
                        if (!String.IsNullOrEmpty(titleAttr))
                            _currentTitle = titleAttr;
                    }

                    if (reader.Name.ToLower() == "mshelp:keyword")
                    {
                        string indexType = reader.GetAttribute("Index");
                        if (indexType == "K")
                        {
                            KKeywordInfo kkwdinfo = new KKeywordInfo();
                            string kkeyword = reader.GetAttribute("Term");
                            if (!string.IsNullOrEmpty(kkeyword))
                            {
                                kkeyword = ChmBuilder.ReplaceMarks(kkeyword);
                                Match match = r.Match(kkeyword);
                                if (match.Success)
                                {
                                    kkwdinfo.MainEntry = kkeyword.Substring(0, match.Index);
                                    kkwdinfo.SubEntry = kkeyword.Substring(match.Index + 1).TrimStart(new char[] { ' ' });
                                }
                                else
                                {
                                    kkwdinfo.MainEntry = kkeyword;
                                }

                                kkwdinfo.File = _currentFile;
                                _kkeywords.Add(kkwdinfo);
                            }
                        }
                    }
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "xml")
                        return;
                }
            }
        }
    }
}
