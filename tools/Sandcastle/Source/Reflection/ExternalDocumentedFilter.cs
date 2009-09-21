// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Xml.XPath;
using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    // exposes all apis for which documentation is written

    // this includes all visible members except for property and event accessors (e.g. get_ methods) and delegate members (e.g. Invoke).

    // enumeration members are included

    public class ExternalDocumentedFilter : ApiFilter {

        public ExternalDocumentedFilter() : base() { }

        public ExternalDocumentedFilter(XPathNavigator configuration) : base(configuration) { }

        public override bool IsExposedMember(Member member) {
            if (member == null) throw new ArgumentNullException("member");
            // if the member isn't visible, we certainly won't expose it...
            if (!member.IsVisibleOutsideAssembly) return (false);
            // ...but there are also some visible members we won't expose.
            TypeNode type = member.DeclaringType;
            // member of delegates are not exposed
            if (type.NodeType == NodeType.DelegateNode) return (false);
            // accessor methods for properties and events are not exposed
            if (member.IsSpecialName && (member.NodeType == NodeType.Method)) {
                string name = member.Name.Name;
                if (NameContains(name, "get_")) return (false);
                if (NameContains(name, "set_")) return (false);
                if (NameContains(name, "add_")) return (false);
                if (NameContains(name, "remove_")) return (false);
                if (NameContains(name, "raise_")) return (false);
            }

            // the value field of enumerations is not exposed
            if (member.IsSpecialName && (type.NodeType == NodeType.EnumNode) && (member.NodeType == NodeType.Field)) {
                string name = member.Name.Name;
                if (name == "value__") return (false);
            }

            // protected members of sealed types are not exposed
            // change of plan -- yes they are
            // if (type.IsSealed && (member.IsFamily || member.IsFamilyOrAssembly)) return(false);

            // One more test to deal with a case: a private method is an explicit implementation for
            // a property accessor, but is not marked with the special name flag. To find these, test for
            // the accessibility of the methods they implement
            if (member.IsPrivate && member.NodeType == NodeType.Method) {
                Method method = (Method)member;
                MethodList implements = method.ImplementedInterfaceMethods;
                if ((implements.Length > 0) && (!IsExposedMember(implements[0]))) return (false);
            }

            // okay, passed all tests, the member is exposed as long as the filters allow it
            return (base.IsExposedMember(member));
        }

        // we are satistied with the default namespace expose test, so don't override it

        public override bool IsExposedType(TypeNode type) {
            if (type == null) throw new ArgumentNullException("type");
            // expose any visible types allowed by the base filter
            if (type.IsVisibleOutsideAssembly) {
                return (base.IsExposedType(type));
            } else {
                return (false);
            }
            // return(type.IsVisibleOutsideAssembly);
        }

        private static bool NameContains(string name, string substring) {
            return (name.Contains(substring));
        }

    }

}
