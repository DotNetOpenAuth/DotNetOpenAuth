// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class AssemblyResolver {

        private Dictionary < string, AssemblyNode > cache = new Dictionary < string, AssemblyNode >();

        private bool useGac = true;

        public AssemblyResolver() {

        }

        public AssemblyResolver(XPathNavigator configuration) {

            string useGacValue = configuration.GetAttribute("use-gac", String.Empty);
            if (!String.IsNullOrEmpty(useGacValue)) useGac = Convert.ToBoolean(useGacValue);

        }

        public event EventHandler < AssemblyReferenceEventArgs > UnresolvedAssemblyReference;

        public bool UseGac {
            get {
                return (useGac);
            }
            set {
                useGac = value;
            }
        }

        public virtual void Add(AssemblyNode assembly) {
            if (assembly == null) throw new ArgumentNullException("assembly");
            string name = assembly.StrongName;
            assembly.AssemblyReferenceResolution += new Module.AssemblyReferenceResolver(ResolveReference);
            assembly.AssemblyReferenceResolutionAfterProbingFailed += new Module.AssemblyReferenceResolver(UnresolvedReference);
            cache[name] = assembly;
            //Console.WriteLine("added {0}; cache now contains {1}", name, cache.Count);
        }

        public virtual AssemblyNode ResolveReference(AssemblyReference reference, Module module) {

            if (reference == null) throw new ArgumentNullException("reference");

            //Console.WriteLine("resolving {0}", reference.StrongName);

            // try to get it from the cache
            string name = reference.StrongName;
            if (cache.ContainsKey(name)) return (cache[name]);

            // try to get it from the gac
            if (useGac) {
                string location = GlobalAssemblyCache.GetLocation(reference);
                if (location != null) {
                    AssemblyNode assembly = AssemblyNode.GetAssembly(location, null, false, false, false, false);
                    if (assembly != null) {
                        Add(assembly);
                        return (assembly);
                    }
                }
            }

            // couldn't find it; return null
            // Console.WriteLine("returning null on request for {0}", reference.StrongName);
            //OnUnresolvedAssemblyReference(reference, module);
            return (null);

        }

        protected virtual void OnUnresolvedAssemblyReference(AssemblyReference reference, Module module) {
            if (UnresolvedAssemblyReference != null) UnresolvedAssemblyReference(this, new AssemblyReferenceEventArgs(reference, module));
        }

        private AssemblyNode UnresolvedReference(AssemblyReference reference, Module module) {
            OnUnresolvedAssemblyReference(reference, module);
            return (null);
        }

    }

}
