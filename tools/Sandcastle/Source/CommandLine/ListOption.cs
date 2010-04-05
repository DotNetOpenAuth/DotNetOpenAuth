// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.CommandLine {

    public class ListOption : Option {

        private string template = "xxxx";

        private List < string > values = new List < string >();

        public ListOption(string name) : base(name) { }

        public ListOption(string name, string description) : base(name, description) { }

        public ListOption(string name, string description, string template) : base(name, description) {
            this.template = template;
        }

        public override Object Value {
            get {
                return (values.ToArray());
            }
        }

        internal override ParseResult ParseArgument(string argument) {
            if (argument.Length == 0) return (ParseResult.MalformedArgument);
            if (argument[0] != ':') return (ParseResult.MalformedArgument);
            present = true;
            string[] atoms = argument.Substring(1).Split(',');
            foreach (string atom in atoms) values.Add(atom);
            return (ParseResult.Success);
        }

        internal override void WriteTemplate(TextWriter writer) {
            writer.WriteLine("/{0}:{1}[,{1},{1},...]", Name, template);
        }

    }

}
