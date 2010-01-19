// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.CommandLine {

    public sealed class SwitchOption : Option {

        public SwitchOption(string name) : base(name) { }

        public SwitchOption(string name, string description) : base(name, description) { }

        internal override ParseResult ParseArgument(string argument) {
            if (argument.Length > 0) return (ParseResult.MalformedArgument);
            if (present) return (ParseResult.MultipleOccurance);
            present = true;
            return (ParseResult.Success);
        }

        internal override void WriteTemplate(TextWriter writer) {
            writer.WriteLine("/{0}", Name);
        }

    }

}
