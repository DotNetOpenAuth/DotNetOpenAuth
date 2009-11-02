// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.CommandLine;

namespace Microsoft.Ddue.Tools
{
    class BuildAssemblerConsole
    {
        public static int Main(string[] args)
        {
            ConsoleApplication.WriteBanner();

            #region read command line arguments, and setup config

            // specify options
            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new StringOption("config", "Specify a configuration file.", "configFilePath"));

            // process options
            ParseArgumentsResult results = options.ParseArguments(args);

            // process help option
            if (results.Options["?"].IsPresent)
            {
                Console.WriteLine("TocBuilder [options] rootDirectory");
                options.WriteOptionSummary(Console.Out);
                return (0);
            }

            // check for invalid options
            if (!results.Success)
            {
                results.WriteParseErrors(Console.Out);
                return (1);
            }

            // check for manifest

            if (results.UnusedArguments.Count != 1)
            {
                Console.WriteLine("You must supply exactly one manifest file.");
                return (1);
            }

            string manifest = results.UnusedArguments[0];

            // Load the configuration file
            XPathDocument configuration;
            try
            {
                if (results.Options["config"].IsPresent)
                {
                    configuration = ConsoleApplication.GetConfigurationFile((string)results.Options["config"].Value);
                }
                else
                {
                    configuration = ConsoleApplication.GetConfigurationFile();
                }
            }
            catch (IOException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The specified configuration file could not be loaded. The error message is: {0}", e.Message));
                return (1);
            }
            catch (XmlException e)
            {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The specified configuration file is not well-formed. The error message is: {0}", e.Message));
                return (1);
            }

            #endregion

            // create a BuildAssembler to do the work
            BuildAssembler buildAssembler = new BuildAssembler();

            try {

                // load the context
                XPathNavigator contextNode = configuration.CreateNavigator().SelectSingleNode("/configuration/dduetools/builder/context");
                if (contextNode != null) buildAssembler.Context.Load(contextNode);

                // load the build components
                XPathNavigator componentsNode = configuration.CreateNavigator().SelectSingleNode("/configuration/dduetools/builder/components");
                if (componentsNode != null) buildAssembler.AddComponents(componentsNode);

                // proceed thorugh the build manifest, processing all topics named there
                int count = buildAssembler.Apply(manifest);
               
                ConsoleApplication.WriteMessage(LogLevel.Info, String.Format("Processed {0} topics", count));

            } finally {
                buildAssembler.Dispose();
            }

            return (0);
        }
    }
}
