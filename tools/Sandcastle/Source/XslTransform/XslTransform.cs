// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Microsoft.Ddue.Tools.CommandLine;


namespace Microsoft.Ddue.Tools {

    public class XslTransformer {

        public static int Main(string[] args) {

            // specify options
            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new ListOption("xsl", "Sepcify transform files.", "xsltPath"));
            options.Add(new ListOption("arg", "Sepcify arguments.", "name=value"));
            options.Add(new StringOption("out", "Specify an output file. If unspecified, output goes to the console.", "outputFilePath"));

            ConsoleApplication.WriteBanner();

            // process options
            ParseArgumentsResult results = options.ParseArguments(args);
            if (results.Options["?"].IsPresent) {
                Console.WriteLine("XslTransformer xsl_file [xml_file] [options]");
                options.WriteOptionSummary(Console.Out);
                return (0);
            }

            // check for invalid options
            if (!results.Success) {
                results.WriteParseErrors(Console.Out);
                return (1);
            }

            // check for missing or extra assembly directories
            if (results.UnusedArguments.Count != 1) {
                Console.WriteLine("Specify one input XML input file.");
                return (1);
            }

            if (!results.Options["xsl"].IsPresent) {
                Console.WriteLine("Specify at least one XSL transform file.");
                return (1);
            }

            // Load transforms
            string[] transformFiles = (string[])results.Options["xsl"].Value;
            XslCompiledTransform[] transforms = new XslCompiledTransform[transformFiles.Length];
            for (int i = 0; i < transformFiles.Length; i++) {
                string transformFile = Environment.ExpandEnvironmentVariables(transformFiles[i]);
                transforms[i] = new XslCompiledTransform();
                XsltSettings transformSettings = new XsltSettings(true, true);
                try {
                    transforms[i].Load(transformFile, transformSettings, new XmlUrlResolver());
                } catch (IOException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The transform file '{0}' could not be loaded. The error is: {1}", transformFile, e.Message));
                    return (1);
                } catch (UnauthorizedAccessException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The transform file '{0}' could not be loaded. The error is: {1}", transformFile, e.Message));
                    return (1);
                } catch (XsltException e) {
                    if (e.InnerException != null) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The transformation file '{0}' is not valid. The error is: {1}", transformFile, e.InnerException.Message));
                    } else {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The transformation file '{0}' is not valid. The error is: {1}", transformFile, e.Message));
                    }
                    return (1);
                } catch (XmlException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The transform file '{0}' is not well-formed. The error is: {1}", transformFile, e.Message));
                    return (1);
                }
            }

            // Compose the arguments
            XsltArgumentList arguments = new XsltArgumentList();
            if (results.Options["arg"].IsPresent) {
                string[] nameValueStrings = (string[])results.Options["arg"].Value;
                foreach (string nameValueString in nameValueStrings) {
                    string[] nameValuePair = nameValueString.Split('=');
                    if (nameValuePair.Length != 2) continue;
                    arguments.AddParam(nameValuePair[0], String.Empty, nameValuePair[1]);
                }
            }

            string input = Environment.ExpandEnvironmentVariables(results.UnusedArguments[0]);

            // Do each transform
            for (int i = 0; i < transforms.Length; i++) {

                ConsoleApplication.WriteMessage(LogLevel.Info, String.Format("Applying XSL transformation '{0}'.", transformFiles[i]));

                // get the transform
                XslCompiledTransform transform = transforms[i];

                // figure out where to put the output
                string output;
                if (i < (transforms.Length - 1)) {
                    try {
                        output = Path.GetTempFileName();
                        File.SetAttributes(output, FileAttributes.Temporary);
                    } catch (IOException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while attempting to create a temporary file. The error message is: {0}", e.Message));
                        return (1);
                    }
                } else {
                    if (results.Options["out"].IsPresent) {
                        output = Environment.ExpandEnvironmentVariables((string)results.Options["out"].Value);
                    } else {
                        output = null;
                    }
                }


                // Console.WriteLine("{0} -> {1}", input, output);

                // create a reader
                XmlReader reader;
                try {
                    Stream readStream = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                    reader = XmlReader.Create(readStream);
                } catch (IOException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The input file '{0}' could not be loaded. The error is: {1}", input, e.Message));
                    return (1);
                } catch (UnauthorizedAccessException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The input file '{0}' could not be loaded. The error is: {1}", input, e.Message));
                    return (1);
                } catch (XmlException e) {
                    // this exception probably only occurs later; we now catch it during the transform
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The input file '{0}' is not well-formed. The error is: {1}", input, e.Message));
                    return (1);
                }

                try {

                    // create a writer
                    XmlWriter writer;
                    try {
                        if (output == null) {
                            writer = XmlWriter.Create(Console.Out, transform.OutputSettings);
                        } else {
                            Stream writeStream = File.Open(output, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete);
                            writer = XmlWriter.Create(writeStream, transform.OutputSettings);
                        }
                    } catch (IOException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The output file '{0}' could not be loaded. The error is: {1}", output, e.Message));
                        return (1);
                    } catch (UnauthorizedAccessException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The output file '{0}' could not be loaded. The error is: {1}", output, e.Message));
                        return (1);
                    } catch (XmlException e) {
                        // can this happen?
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The output file '{0}' is not well-formed. The error is: {1}", output, e.Message));
                        return (1);
                    }

                    try {
                        // do the deed
                        transform.Transform(reader, arguments, writer);
                    } catch (XsltException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured during the transformation. The error message is: {0}",
                            (e.InnerException == null) ? e.Message : e.InnerException.Message));
                        return (1);
                    } catch (XmlException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The input file '{0}' is not well-formed. The error is: {1}", input, e.Message));
                        return (1);
                    } finally {
                        writer.Flush();
                        writer.Close();
                    }

                } finally {
                    reader.Close();
                }

                // if the last input was a temp file, delete it
                if (i > 0) {
                    // Console.WriteLine("deleting {0}", input);
                    try {
                        File.Delete(input);
                    } catch (IOException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Warn, String.Format("The temporary file '{0}' could not be deleted. The error message is: {1}", input, e.Message));
                    }
                }

                // the last output file is the next input file
                input = output;

            }

            return (0);
        }

    }

    internal class TransformInfo {

        private static XmlUrlResolver resolver = new XmlUrlResolver();

        private static XsltSettings settings = new XsltSettings(true, true);

        private string file;

        private XslCompiledTransform transform = new XslCompiledTransform();

        public TransformInfo(string file) {
            this.file = file;
            transform.Load(file, settings, resolver);
        }

        public string File {
            get {
                return (file);
            }
        }

        public XslCompiledTransform Transform {
            get {
                return (transform);
            }
        }

    }

}
