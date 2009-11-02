// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Ddue.Tools.CommandLine {

    public sealed class ParseArgumentsResult {

        internal Dictionary < string, ParseResult > errors = new Dictionary < string, ParseResult >();

        internal List < string > nonoptions = new List < string >();

        // data

        internal OptionCollection options;

        internal ParseArgumentsResult() { }

        // accessors

        public OptionCollection Options {
            get {
                return (options);
            }
        }

        public bool Success {
            get {
                if (errors.Count == 0) {
                    return (true);
                } else {
                    return (false);
                }
            }
        }

        public ReadOnlyCollection < string > UnusedArguments {

            get {
                return (new ReadOnlyCollection < string >(nonoptions));
            }
        }

        public void WriteParseErrors(TextWriter writer) {

            if (writer == null) throw new ArgumentNullException("writer");
            foreach (KeyValuePair < string, ParseResult > error in errors) {
                writer.WriteLine("{0}: {1}", error.Value, error.Key);

            }

        }

    }


}
