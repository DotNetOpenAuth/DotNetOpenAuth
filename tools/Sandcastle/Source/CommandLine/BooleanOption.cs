// Copyright (c) Microsoft Corporation.  All rights reserved.
//

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.CommandLine {

    public sealed class BooleanOption : Option {

        public BooleanOption(string name) : base(name) { }

        public BooleanOption(string name, string description) : base(name, description) { }

        internal override ParseResult ParseArgument(string argument) {
            if ((argument != "+") && (argument != "-")) return (ParseResult.MalformedArgument);
            if (present) return (ParseResult.MultipleOccurance);
            present = true;
            if (argument == "+") {
                value = true;
            } else {
                value = false;
            }
            return (ParseResult.Success);
        }

        internal override void WriteTemplate(TextWriter writer) {
            writer.WriteLine("/{0}+|-", Name);
        }

    }

}
