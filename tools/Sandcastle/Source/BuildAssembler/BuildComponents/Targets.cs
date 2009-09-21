// Copyright (c) Microsoft Corporation.  All rights reserved.
//
using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;
using System.Xml.XPath;

namespace BuildComponents {

    public partial class Target {

        internal string id;

        internal string container;

        internal string file;

        public string Id {
            get {
                return (id);
            }
        }

        public string Container {
            get {
                return (container);
            }
        }

        public string File {
            get {
                return (file);
            }
        }

    }

    // Namespace

    public partial class NamespaceTarget : Target {

        internal string name;

        public string Name {
            get {
                return (name);
            }
        }

    }

    // Type

    public partial class TypeTarget : Target {

        // apidata

        protected string name;

        protected string subgroup;

        // containers

        protected NamespaceReference containingNamespace;

        protected SimpleTypeReference containingType;

        protected string containingAssembly;

        // templates

        protected string[] templates;

        // typedata

        private object visibility;

        private bool isAbstract;

        private bool isSealed;

        private bool isSerializable;

        // family

        private SimpleTypeReference parentType;

        // other

        public string Name {
            get {
                return (name);
            }
        }

        public NamespaceReference Namespace {
            get {
                return (containingNamespace);
            }
        }

        public SimpleTypeReference OuterType {
            get {
                return (containingType);
            }
        }

        public string[] Templates {
            get {
                return (templates);
            }
        }

    }

    // Construction of targets from Xml

    public partial class Target {

        public static Target Create (XmlReader api) {

            string id = api.GetAttribute("id");

            Target target = null;
            api.ReadToFollowing("apidata");
            string group = api.GetAttribute("group");
            switch (group) {
                case "namespace":
                    target = NamespaceTarget.Create(api);
                break;
            }

            target.id = id;

            return (target);
        }

        protected static XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");


    }

    public partial class NamespaceTarget {

        public static NamespaceTarget Create (XmlReader apidata) {
            NamespaceTarget target = new NamespaceTarget();
            string name = apidata.GetAttribute("name");

            // This is not locale-independent.
            if (String.IsNullOrEmpty(target.name)) name = "(Default Namespace)";

            target.name = name;

            return (target);
        }

        public static NamespaceTarget Create (XPathNavigator api) {

            NamespaceTarget target = new NamespaceTarget();
            target.name = (string)api.Evaluate(apiNameExpression);

            // This is not locale-independent.
            if (String.IsNullOrEmpty(target.name)) target.name = "(Default Namespace)";

            return (target);
        }

    }


    public partial class TypeTarget {

        public static TypeTarget Create (XmlReader api) {

            api.ReadToFollowing("apidata");
            string subgroup = api.GetAttribute("subgroup");

            api.ReadToFollowing("typedata");
            string visibilityValue = api.GetAttribute("visibility");
            string abstractValue = api.GetAttribute("abstract");
            string sealedValue = api.GetAttribute("sealed");
            string serializableValue = api.GetAttribute("serealizable");

            api.ReadToFollowing("library");
            string containingAssemblyValue = api.GetAttribute("assembly");

            api.ReadToFollowing("namespace");
            NamespaceReference containingNamespace = NamespaceReference.Create(api);

            TypeTarget target = new TypeTarget();
            target.containingAssembly = containingAssemblyValue;
            target.containingNamespace = containingNamespace;

            return (target);

        }
 
    }

}
