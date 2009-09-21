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
    public class ChmBuilderArgs
    {
        public string configFile;
        public string htmlDirectory;
        public int langid;
        public bool metadata;
        public string outputDirectory;
        public string projectName;
        public string tocFile;

        public ChmBuilderArgs()
        {
            this.langid = 1033;
            this.tocFile = string.Empty;
            this.metadata = false;
            string configDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            this.configFile = Path.Combine(configDirectory, "ChmBuilder.config");
        }

    }
}
