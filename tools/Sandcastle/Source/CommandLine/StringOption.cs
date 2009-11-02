// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.CommandLine {

    public sealed class StringOption : Option {

        private string template = "xxxx";

        public StringOption(string name) : base(name) { }

        public StringOption(string name, string description) : base(name, description) { }

        public StringOption(string name, string description, string template) : base(name, description) {
            this.template = template;
        }

        public string Template {
            get {
                return (template);
            }
            set {
                template = value;
            }
        }

        internal override ParseResult ParseArgument(string argument) {
            if (!(argument.Length > 0)) return (ParseResult.MalformedArgument);
            if (argument[0] != ':') return (ParseResult.MalformedArgument);
            if (present) return (ParseResult.MultipleOccurance);
            present = true;
            value = argument.Substring(1);
            return (ParseResult.Success);
        }

        internal override void WriteTemplate(TextWriter writer) {
            writer.WriteLine("/{0}:{1}", Name, template);
        }

    }

}
