// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using System.Compiler;
using Microsoft.Ddue.Tools.CommandLine;
using Microsoft.Ddue.Tools.Reflection;


namespace Microsoft.Ddue.Tools {

    public class MRefBuilder {

        public static int Main(string[] args) {

            // write banner
            ConsoleApplication.WriteBanner();

            // specify options
            OptionCollection options = new OptionCollection();
            options.Add(new SwitchOption("?", "Show this help page."));
            options.Add(new StringOption("out", "Specify an output file. If unspecified, output goes to the console.", "outputFilePath"));
            options.Add(new StringOption("config", "Specify a configuration file. If unspecified, MRefBuilder.config is used", "configFilePath"));
            options.Add(new ListOption("dep", "Speficy assemblies to load for dependencies.", "dependencyAssembly"));
            // options.Add( new BooleanOption("namespaces", "Control whether information on namespaces in provided.") );
            options.Add(new BooleanOption("internal", "Specify whether to document internal as well as externally exposed APIs."));

            // process options
            ParseArgumentsResult results = options.ParseArguments(args);
            if (results.Options["?"].IsPresent) {
                Console.WriteLine("MRefBuilder [options] assemblies");
                options.WriteOptionSummary(Console.Out);
                return (0);
            }

            // check for invalid options
            if (!results.Success) {
                results.WriteParseErrors(Console.Out);
                return (1);
            }

            // check for missing or extra assembly directories
            if (results.UnusedArguments.Count < 1) {
                Console.WriteLine("Specify at least one assembly to reflect.");
                return (1);
            }

            // load the configuration file
            XPathDocument config;
            string configDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configFile = Path.Combine(configDirectory, "MRefBuilder.config");
            if (results.Options["config"].IsPresent) {
                configFile = (string)results.Options["config"].Value;
                configDirectory = Path.GetDirectoryName(configFile);
            }
            try {
                config = new XPathDocument(configFile);
            } catch (IOException e) {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while attempting to read the configuration file '{0}'. The error message is: {1}", configFile, e.Message));
                return (1);
            } catch (UnauthorizedAccessException e) {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while attempting to read the configuration file '{0}'. The error message is: {1}", configFile, e.Message));
                return (1);
            } catch (XmlException e) {
                ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The configuration file '{0}' is not well-formed. The error message is: {1}", configFile, e.Message));
                return (1);
            }

            // adjust the target platform
            XPathNodeIterator platformNodes = config.CreateNavigator().Select("/configuration/dduetools/platform");
            if (platformNodes.MoveNext()) {
                XPathNavigator platformNode = platformNodes.Current;
                string version = platformNode.GetAttribute("version", String.Empty);
                string path = platformNode.GetAttribute("path", String.Empty);
                path = Environment.ExpandEnvironmentVariables(path);
                if (!Directory.Exists(path)) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The specifed target platform directory '{0}' does not exist.", path));
                    return (1);
                }
                if (version == "2.0") {
                    TargetPlatform.SetToV2(path);
                } else if (version == "1.1") {
                    TargetPlatform.SetToV1_1(path);
                } else if (version == "1.0") {
                    TargetPlatform.SetToV1(path);
                } else {
                    Console.WriteLine("Unknown target platform version '{0}'.", version);
                    return (1);
                }
            }

            // create a namer
            ApiNamer namer = new OrcasNamer();
            XPathNavigator namerNode = config.CreateNavigator().SelectSingleNode("/configuration/dduetools/namer");
            if (namerNode != null) {
                string assemblyPath = namerNode.GetAttribute("assembly", String.Empty);
                string typeName = namerNode.GetAttribute("type", String.Empty);

                assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);
                if (!Path.IsPathRooted(assemblyPath)) assemblyPath = Path.Combine(configDirectory, assemblyPath);

                try {

                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    namer = (ApiNamer)assembly.CreateInstance(typeName);

                    if (namer == null) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the component assembly '{1}'.", typeName, assemblyPath));
                        return (1);
                    }

                } catch (IOException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("A file access error occured while attempting to load the component assembly '{0}'. The error message is: {1}", assemblyPath, e.Message));
                    return (1);
                } catch (UnauthorizedAccessException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("A file access error occured while attempting to load the component assembly '{0}'. The error message is: {1}", assemblyPath, e.Message));
                    return (1);
                } catch (BadImageFormatException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The component assembly '{0}' is not a valid managed assembly.", assemblyPath));
                    return (1);
                } catch (TypeLoadException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the component assembly '{1}'.", typeName, assemblyPath));
                    return (1);
                } catch (MissingMethodException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("No appropriate constructor exists for the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath));
                    return (1);
                } catch (TargetInvocationException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while initializing the type '{0}' in the component assembly '{1}'. The error message and stack trace follows: {2}", typeName, assemblyPath, e.InnerException.ToString()));
                    return (1);
                } catch (InvalidCastException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' in the component assembly '{1}' is not a component type.", typeName, assemblyPath));
                    return (1);
                }

            }

            // create a resolver
            AssemblyResolver resolver = new AssemblyResolver();
            XPathNavigator resolverNode = config.CreateNavigator().SelectSingleNode("/configuration/dduetools/resolver");
            if (resolverNode != null) {
                string assemblyPath = resolverNode.GetAttribute("assembly", String.Empty);
                string typeName = resolverNode.GetAttribute("type", String.Empty);

                assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);
                if (!Path.IsPathRooted(assemblyPath)) assemblyPath = Path.Combine(configDirectory, assemblyPath);

                try {

                    Assembly assembly = Assembly.LoadFrom(assemblyPath);
                    resolver = (AssemblyResolver)assembly.CreateInstance(typeName, false, BindingFlags.Public | BindingFlags.Instance, null, new Object[1] { resolverNode }, null, null);

                    if (resolver == null) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the component assembly '{1}'.", typeName, assemblyPath));
                        return (1);
                    }

                } catch (IOException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("A file access error occured while attempting to load the component assembly '{0}'. The error message is: {1}", assemblyPath, e.Message));
                    return (1);
                } catch (UnauthorizedAccessException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("A file access error occured while attempting to load the component assembly '{0}'. The error message is: {1}", assemblyPath, e.Message));
                    return (1);
                } catch (BadImageFormatException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The component assembly '{0}' is not a valid managed assembly.", assemblyPath));
                    return (1);
                } catch (TypeLoadException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the component assembly '{1}'.", typeName, assemblyPath));
                    return (1);
                } catch (MissingMethodException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("No appropriate constructor exists for the type'{0}' in the component assembly '{1}'.", typeName, assemblyPath));
                    return (1);
                } catch (TargetInvocationException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while initializing the type '{0}' in the component assembly '{1}'. The error message and stack trace follows: {2}", typeName, assemblyPath, e.InnerException.ToString()));
                    return (1);
                } catch (InvalidCastException) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' in the component assembly '{1}' is not a component type.", typeName, assemblyPath));
                    return (1);
                }

            }
            resolver.UnresolvedAssemblyReference += new EventHandler < AssemblyReferenceEventArgs >(UnresolvedAssemblyReferenceHandler);

            // get a textwriter for output
            TextWriter output = Console.Out;
            if (results.Options["out"].IsPresent) {
                string file = (string)results.Options["out"].Value;
                try {
                    output = new StreamWriter(file, false, Encoding.UTF8);
                } catch (IOException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while attempting to create an output file. The error message is: {0}", e.Message));
                    return (1);
                } catch (UnauthorizedAccessException e) {
                    ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while attempting to create an output file. The error message is: {0}", e.Message));
                    return (1);
                }
            }


            // dependency directory
            string[] dependencies = new string[0];
            if (results.Options["dep"].IsPresent) dependencies = (string[])results.Options["dep"].Value;


            try {
                // create a builder
                ManagedReflectionWriter builder = new ManagedReflectionWriter(output, namer);

                // specify the resolver for the builder
                builder.Resolver = resolver;

                // builder.ApiFilter = new ExternalDocumentedFilter(config.CreateNavigator().SelectSingleNode("/configuration/dduetools"));

                // specify the filter for the builder

                if (results.Options["internal"].IsPresent && (bool)results.Options["internal"].Value) {
                    builder.ApiFilter = new AllDocumentedFilter(config.CreateNavigator().SelectSingleNode("/configuration/dduetools"));
                } else {
                    builder.ApiFilter = new ExternalDocumentedFilter(config.CreateNavigator().SelectSingleNode("/configuration/dduetools"));
                }

                // register add-ins to the builder

                XPathNodeIterator addinNodes = config.CreateNavigator().Select("/configuration/dduetools/addins/addin");
                foreach (XPathNavigator addinNode in addinNodes) {
                    string assemblyPath = addinNode.GetAttribute("assembly", String.Empty);
                    string typeName = addinNode.GetAttribute("type", String.Empty);

                    assemblyPath = Environment.ExpandEnvironmentVariables(assemblyPath);
                    if (!Path.IsPathRooted(assemblyPath)) assemblyPath = Path.Combine(configDirectory, assemblyPath);

                    try {

                        Assembly assembly = Assembly.LoadFrom(assemblyPath);
                        MRefBuilderAddIn addin = (MRefBuilderAddIn)assembly.CreateInstance(typeName, false, BindingFlags.Public | BindingFlags.Instance, null, new Object[2] { builder, addinNode }, null, null);

                        if (namer == null) {
                            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the addin assembly '{1}'.", typeName, assemblyPath));
                            return (1);
                        }

                    } catch (IOException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("A file access error occured while attempting to load the addin assembly '{0}'. The error message is: {1}", assemblyPath, e.Message));
                        return (1);
                    } catch (BadImageFormatException) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The addin assembly '{0}' is not a valid managed assembly.", assemblyPath));
                        return (1);
                    } catch (TypeLoadException) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' was not found in the addin assembly '{1}'.", typeName, assemblyPath));
                        return (1);
                    } catch (MissingMethodException) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("No appropriate constructor exists for the type '{0}' in the addin assembly '{1}'.", typeName, assemblyPath));
                        return (1);
                    } catch (TargetInvocationException e) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while initializing the type '{0}' in the addin assembly '{1}'. The error message and stack trace follows: {2}", typeName, assemblyPath, e.InnerException.ToString()));
                        return (1);
                    } catch (InvalidCastException) {
                        ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("The type '{0}' in the addin assembly '{1}' is not an MRefBuilderAddIn type.", typeName, assemblyPath));
                        return (1);
                    }

                }

                try {


                    // add a handler for unresolved assembly references
                    //builder.UnresolvedModuleHandler = new System.Compiler.Module.AssemblyReferenceResolver(AssemblyNotFound);

                    // load dependent bits
                    foreach (string dependency in dependencies) {
                        try {
                            builder.LoadAccessoryAssemblies(dependency);
                        } catch (IOException e) {
                            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while loading dependency assemblies. The error message is: {0}", e.Message));
                            return (1);
                        }
                    }

                    // parse the bits
                    foreach (string dllPath in results.UnusedArguments) {
                        try {
                            builder.LoadAssemblies(dllPath);
                        } catch (IOException e) {
                            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("An error occured while loading assemblies for reflection. The error message is: {0}", e.Message));
                            return (1);
                        }
                    }

                    ConsoleApplication.WriteMessage(LogLevel.Info, String.Format("Loaded {0} assemblies for reflection and {1} dependency assemblies.", builder.Assemblies.Length, builder.AccessoryAssemblies.Length));

                    // register callbacks

                    //builder.RegisterStartTagCallback("apis", new MRefBuilderCallback(startTestCallback));

                    //MRefBuilderAddIn addin = new XamlAttachedMembersAddIn(builder, null);

                    builder.VisitApis();

                    ConsoleApplication.WriteMessage(LogLevel.Info, String.Format("Wrote information on {0} namespaces, {1} types, and {2} members", builder.Namespaces.Length, builder.Types.Length, builder.Members.Length));

                } finally {

                    builder.Dispose();
                }

            } finally {

                // output.Close();

            }

            return (0);

        }

        private static AssemblyNode AssemblyNotFound(AssemblyReference reference, System.Compiler.Module module) {
            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Unresolved assembly reference: {0} ({1}) required by {2}", reference.Name, reference.StrongName, module.Name));
            Environment.Exit(1);
            return (null);
        }

        private static void UnresolvedAssemblyReferenceHandler(Object o, AssemblyReferenceEventArgs e) {
            ConsoleApplication.WriteMessage(LogLevel.Error, String.Format("Unresolved assembly reference: {0} ({1}) required by {2}", e.Reference.Name, e.Reference.StrongName, e.Referrer.Name));
            Environment.Exit(1);
        }

    }

}
