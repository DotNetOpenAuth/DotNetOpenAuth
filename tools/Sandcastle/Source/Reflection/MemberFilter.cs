// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using System.Compiler;

namespace Microsoft.Ddue.Tools.Reflection {

    public class MemberFilter
    {

#region Member Variables

        private bool exposed;

        private string name;

#endregion

#region Constructors
        public MemberFilter(string name, bool exposed) {
            if (name == null) throw new ArgumentNullException("name");
            this.name = name;
            this.exposed = exposed;
        }

        public MemberFilter(XmlReader configuration) {
            if ((configuration.NodeType != XmlNodeType.Element) || (configuration.Name != "member")) throw new InvalidOperationException();
            name = configuration.GetAttribute("name");
            exposed = Convert.ToBoolean(configuration.GetAttribute("expose"));
        }
#endregion

#region Public API
        public bool? IsExposedMember(Member member) {
            if (member.Name.Name == name) {
                return (exposed);
            } else {
                return (null);
            }
        }
#endregion

    }

}
