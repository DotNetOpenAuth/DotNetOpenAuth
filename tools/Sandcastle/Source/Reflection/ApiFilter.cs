// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class ApiFilter {

#region Member Variables

        private RootFilter apiFilter = new RootFilter(true);

        private RootFilter attributeFilter = new RootFilter(true);

        private Dictionary < string, bool > namespaceCache = new Dictionary < string, bool >();

#endregion

#region Constructors

        // stored filters

        public ApiFilter() {
            // apiFilters.Add(new UserFilter("System.Reflection", "", "", true));
            // apiFilters.Add(new UserFilter("System", "Object", "", true));
            // apiFilters.Add(new UserFilter("", "", "", false));

            // attributeFilters.Add(new UserFilter("System.Runtime.InteropServices", "ComVisibleAttribute", "", false));
        }

        public ApiFilter(XPathNavigator configuration) {

            if (configuration == null) throw new ArgumentNullException("configuration");

            // API filter nodes
            XPathNavigator apiFilterNode = configuration.SelectSingleNode("apiFilter");
            if (apiFilterNode != null) {
                XmlReader configurationReader = apiFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                apiFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }

            // Attribute filter nodes
            XPathNavigator attributeFilterNode = configuration.SelectSingleNode("attributeFilter");
            if (attributeFilterNode != null) {
                XmlReader configurationReader = attributeFilterNode.ReadSubtree();
                configurationReader.MoveToContent();
                attributeFilter = new RootFilter(configurationReader);
                configurationReader.Close();
            }

        }

#endregion

#region Public API

        public virtual bool IsDocumentedInterface(TypeNode type)
        {
            if (type == null) throw new ArgumentException("type");
            return (apiFilter.IsExposedType(type));
        }

        public virtual bool HasExposedMembers(TypeNode type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return (apiFilter.HasExposedMembers(type));
        }

        // exposure logic for artibrary APIs
        // call the appropriate particular exposure logic

        public virtual bool IsExposedApi(Member api) {

            Namespace space = api as Namespace;
            if (space != null) return (IsExposedNamespace(space));

            TypeNode type = api as TypeNode;
            if (type != null) return (IsExposedType(type));

            return (IsExposedMember(api));
        }

        public virtual bool IsExposedAttribute(AttributeNode attribute) {
            if (attribute == null) throw new ArgumentNullException("attribute");

            // check whether attribte type is exposed
            TypeNode attributeType = attribute.Type;
            if (!IsExposedType(attributeType)) return (false);

            // check whether expressions used to instantiate attribute are exposed
            ExpressionList expressions = attribute.Expressions;
            for (int i = 0; i < expressions.Count; i++) {
                if (!IsExposedExpression(expressions[i])) return (false);
            }

            // apply user filters to attribute
            return (attributeFilter.IsExposedType(attributeType));
        }

        public virtual bool IsExposedMember(Member member) {
            if (member == null) throw new ArgumentNullException("member");
            return (apiFilter.IsExposedMember(member));
        }

        // namespce logic
        // a namespace is exposed if any type in it is exposed

        public virtual bool IsExposedNamespace(Namespace space) {
            if (space == null) throw new ArgumentNullException("space");
            string name = space.Name.Name;

            // look in cache to see if namespace exposure is already determined
            bool exposed;
            if (!namespaceCache.TryGetValue(name, out exposed)) {
                // it is not; determine exposure now

                // the namespace is exposed if any types in it are exposed              
                exposed = NamespaceContainesExposedTypes(space) ?? false;

                // the namespace is also exposed if it contains exposed members, even if all types are hidden
                if (!exposed)
                {
                    exposed = NamespaceContainsExposedMembers(space);
                }

                // cache the result 
                namespaceCache.Add(name, exposed);

            }
            return (exposed);
        }

        // type and member logic
        // by default, types and members are exposed if a user filter says it, or if no user filter forbids it

        public virtual bool IsExposedType(TypeNode type) {
            if (type == null) throw new ArgumentNullException("type");
            return (apiFilter.IsExposedType(type));
        }

#endregion

#region Implementation

        private bool IsExposedExpression(Expression expression) {
            if (expression.NodeType == NodeType.Literal) {
                Literal literal = (Literal)expression;
                TypeNode type = literal.Type;
                if (!IsExposedType(type)) return (false);
                if (type.FullName == "System.Type") {
                    // if the value is itself a type, we need to test whether that type is visible
                    TypeNode value = literal.Value as TypeNode;
                    if ((value != null) && !IsExposedType(value)) return (false);
                }
                return (true);
            } else if (expression.NodeType == NodeType.NamedArgument) {
                NamedArgument assignment = (NamedArgument)expression;
                return (IsExposedExpression(assignment.Value));
            } else {
                throw new InvalidOperationException("Encountered unrecognized expression");
            }
        }

        private bool? NamespaceContainesExposedTypes(Namespace space)
        {
            TypeNodeList types = space.Types;

            for (int i = 0; i < types.Count; i++)
            {
                TypeNode type = types[i];
                if (IsExposedType(type)) return (true);
            }

            if (apiFilter.NamespaceFilterCount < 1)
            {
                return null; //this apiFilter does not contain any namespaces
            }

            return (false);
        }


        /** <summary>Check for any exposed members in any of the types.
         * Returns true if the type has an exposed memeber filter and
         * it is matched. This is used to determine if the namespace
         * should be visited if the namespace and all types are set to 
         * false for exposed, we still want to visit them if any members
         * are set to true.
         * </summary> */
        private bool NamespaceContainsExposedMembers(Namespace space)
        {
            TypeNodeList types = space.Types;
            for (int i = 0; i < types.Count; i++)
            {
                TypeNode type = types[i];

                if (HasExposedMembers(type)) return true;
            }
            return (false);
        }

#endregion

    }

}
