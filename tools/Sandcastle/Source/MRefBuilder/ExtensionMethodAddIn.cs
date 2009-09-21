// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

using Microsoft.Ddue.Tools.Reflection;

namespace Microsoft.Ddue.Tools {

    // Extension method add in

    public class ExtensionMethodAddIn : MRefBuilderAddIn {

        private Dictionary < TypeNode, List < Method > > index = new Dictionary < TypeNode, List < Method > >();

        private bool isExtensionMethod = false;

        private ManagedReflectionWriter reflector;

        public ExtensionMethodAddIn(ManagedReflectionWriter reflector, XPathNavigator configuration) : base(reflector, configuration) {
            this.reflector = reflector;
            reflector.RegisterStartTagCallback("apis", new MRefBuilderCallback(RecordExtensionMethods));
            reflector.RegisterEndTagCallback("elements", new MRefBuilderCallback(AddExtensionMethods));
            reflector.RegisterStartTagCallback("apidata", new MRefBuilderCallback(AddExtensionSubsubgroup));
        }

        private void AddExtensionMethod(XmlWriter writer, TypeNode type, Method extensionMethodTemplate, TypeNode specialization) {

            // Construct the extension method for naming
            Method extensionMethodTemplate2 = extensionMethodTemplate;
            if (extensionMethodTemplate2.IsGeneric && (specialization != null)) extensionMethodTemplate2 = extensionMethodTemplate.GetTemplateInstance(type, specialization);
            TypeNode extensionMethodTemplateReturnType = extensionMethodTemplate2.ReturnType;
            ParameterList extensionMethodTemplateParameters = extensionMethodTemplate2.Parameters;

            ParameterList extensionMethodParameters = new ParameterList();
            for (int i = 1; i < extensionMethodTemplateParameters.Length; i++) {
                Parameter extensionMethodParameter = extensionMethodTemplateParameters[i];
                extensionMethodParameters.Add(extensionMethodParameter);
            }
            Method extensionMethod = new Method(extensionMethodTemplate.DeclaringType, new AttributeList(), extensionMethodTemplate.Name, extensionMethodParameters, extensionMethodTemplate.ReturnType, null);
            extensionMethod.Flags = extensionMethodTemplate.Flags & ~MethodFlags.Static;

            //Method specializedMethod == extensionMethod.Get

            // Get the names
            string extensionMethodId = reflector.ApiNamer.GetMemberName(extensionMethod);
            string extensionMethodTemplateId = reflector.ApiNamer.GetMemberName(extensionMethodTemplate);

            writer.WriteStartElement("element");
            writer.WriteAttributeString("api", extensionMethodTemplateId);
            writer.WriteAttributeString("display-api", extensionMethodId);
            writer.WriteAttributeString("source", "extension");
            isExtensionMethod = true;
            reflector.WriteMember(extensionMethod);
            isExtensionMethod = false;
            writer.WriteEndElement();

        }

        private void AddExtensionMethods(XmlWriter writer, Object info) {

            MemberDictionary members = info as MemberDictionary;
            if (members == null) return;

            TypeNode type = members.Type;

            InterfaceList contracts = type.Interfaces;
            foreach (Interface contract in contracts) {
                List < Method > extensionMethods = null;
                if (index.TryGetValue(contract, out extensionMethods)) {
                    foreach (Method extensionMethod in extensionMethods) AddExtensionMethod(writer, type, extensionMethod, null);
                }
                if (contract.IsGeneric && (contract.TemplateArguments != null) && (contract.TemplateArguments.Length > 0)) {
                    Interface templateContract = (Interface)ReflectionUtilities.GetTemplateType(contract);
                    TypeNode specialization = contract.TemplateArguments[0];
                    if (index.TryGetValue(templateContract, out extensionMethods)) {
                        foreach (Method extensionMethod in extensionMethods) {
                            if (IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0])) {
                                AddExtensionMethod(writer, type, extensionMethod, specialization);
                            }
                        }
                    }
                }
            }

            TypeNode comparisonType = type;
            while (comparisonType != null) {
                List < Method > extensionMethods = null;
                if (index.TryGetValue(comparisonType, out extensionMethods)) {
                    foreach (Method extensionMethod in extensionMethods) AddExtensionMethod(writer, type, extensionMethod, null);
                }
                if (comparisonType.IsGeneric && (comparisonType.TemplateArguments != null) && (comparisonType.TemplateArguments.Length > 0)) {
                    TypeNode templateType = ReflectionUtilities.GetTemplateType(comparisonType);
                    TypeNode specialization = comparisonType.TemplateArguments[0];
                    if (index.TryGetValue(templateType, out extensionMethods)) {
                        foreach (Method extensionMethod in extensionMethods) {
                            if (IsValidTemplateArgument(specialization, extensionMethod.TemplateParameters[0])) {
                                AddExtensionMethod(writer, type, extensionMethod, specialization);
                            }
                        }
                    }
                }
                comparisonType = comparisonType.BaseType;
            }
        }

        private void AddExtensionSubsubgroup(XmlWriter writer, Object data) {
            if (isExtensionMethod) writer.WriteAttributeString("subsubgroup", "extension");
        }

        private bool HasExtensionAttribute(Method method) {
            AttributeList attributes = method.Attributes;
            foreach (AttributeNode attribute in attributes) {
                if (attribute.Type.FullName == "System.Runtime.CompilerServices.ExtensionAttribute") return (true);
            }
            return (false);
        }

        private bool IsValidTemplateArgument(TypeNode type, TypeNode parameter) {
            if (type == null) throw new ArgumentNullException("type");
            if (parameter == null) throw new ArgumentNullException("parameter");

            // check that the parameter really is a type parameter

            ITypeParameter itp = parameter as ITypeParameter;
            if (itp == null) throw new ArgumentException("parameter");

            // test constraints

            bool reference = ((itp.TypeParameterFlags & TypeParameterFlags.ReferenceTypeConstraint) > 0);
            if (reference && type.IsValueType) return (false);

            bool value = ((itp.TypeParameterFlags & TypeParameterFlags.ValueTypeConstraint) > 0);
            if (value && !type.IsValueType) return (false);

            bool constructor = ((itp.TypeParameterFlags & TypeParameterFlags.DefaultConstructorConstraint) > 0);


            InterfaceList contracts = parameter.Interfaces;
            if (contracts != null) {
                foreach (Interface contract in contracts) {
                    if (!type.IsAssignableTo(contract)) return (false);
                }
            }

            TypeNode parent = parameter.BaseType;
            if ((parent != null) && !type.IsAssignableTo(parent)) return (false);

            // okay, passed all tests

            return (true);

        }

        private void RecordExtensionMethods(XmlWriter writer, Object info) {
            NamespaceList spaces = (NamespaceList)info;
            foreach (Namespace space in spaces) {
                TypeNodeList types = space.Types;
                foreach (TypeNode type in types) {

                    MemberList members = type.Members;

                    // go through the members, looking for fields signaling attached properties
                    foreach (Member member in members) {

                        Method method = member as Method;
                        if (method == null) continue;

                        if (!reflector.ApiFilter.IsExposedMember(method)) continue;

                        if (!HasExtensionAttribute(method)) continue;

                        ParameterList parameters = method.Parameters;
                        TypeNode extendedType = parameters[0].Type;

                        // recognize Method<T>(Type<T>, ...) extension methods
                        if (method.IsGeneric && (method.TemplateParameters.Length == 1)) {
                            if (extendedType.IsGeneric && (extendedType.TemplateArguments != null) && (extendedType.TemplateArguments.Length == 1)) {
                                TypeNode arg = extendedType.TemplateArguments[0];
                                if (arg.IsTemplateParameter) {
                                    ITypeParameter gtp = (ITypeParameter)arg;
                                    if ((gtp.DeclaringMember == method) && (gtp.ParameterListIndex == 0)) {
                                        extendedType = ReflectionUtilities.GetTemplateType(extendedType);
                                    }
                                }
                            }
                        }

                        List < Method > methods = null;
                        if (!index.TryGetValue(extendedType, out methods)) {
                            methods = new List < Method >();
                            index.Add(extendedType, methods);
                        }

                        methods.Add(method);

                    }
                }

            }

            /*
            Console.WriteLine("Recorded {0} extension methods", index.Count);
            foreach (TypeNode type in index.Keys) {
                Console.WriteLine(type.FullName);
            }
            */
        }

    }

}
