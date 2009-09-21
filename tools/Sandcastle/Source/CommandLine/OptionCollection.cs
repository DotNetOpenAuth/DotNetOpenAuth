// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace Microsoft.Ddue.Tools.CommandLine {


    public sealed class OptionCollection : ICollection < Option >, ICollection, IEnumerable < Option >, IEnumerable {
        private Dictionary < string, Option > map = new Dictionary < string, Option >();

        private List < Option > options = new List < Option >();


        public int Count {
            get {
                return (options.Count);
            }
        }

        // Extras for ICollection<Option>

        bool ICollection < Option >.IsReadOnly {
            get {
                return (false);
            }
        }

        // Extras for ICollection

        bool ICollection.IsSynchronized {
            get {
                return (false);
            }
        }

        Object ICollection.SyncRoot {
            get {
                return (this);
            }
        }

        public Option this[string name] {
            get {
                Option option;
                if (map.TryGetValue(name, out option)) {
                    return (option);
                } else {
                    return (null);
                }
            }
        }

        public void Add(Option option) {
            if (option == null) throw new ArgumentNullException("option");
            map.Add(option.Name, option);
            options.Add(option);
        }


        public void Clear() {
            options.Clear();
            map.Clear();
        }

        bool ICollection < Option >.Contains(Option option) {
            return (options.Contains(option));
        }

        void ICollection < Option >.CopyTo(Option[] array, int startIndex) {
            options.CopyTo(array, startIndex);
        }

        void ICollection.CopyTo(Array array, int startIndex) {
            ((ICollection)options).CopyTo(array, startIndex);
        }

        // Extras for IEnumerable<Option>

        IEnumerator < Option > IEnumerable < Option >.GetEnumerator() {
            return (options.GetEnumerator());
        }

        // Extras for IEnumerable

        IEnumerator IEnumerable.GetEnumerator() {
            return (options.GetEnumerator());
        }

        // Parse arguments -- the main show

        public ParseArgumentsResult ParseArguments(string[] args) {

            // keep track of results
            ParseArgumentsResult results = new ParseArgumentsResult();
            results.options = this;

            // parse arguments
            ParseArguments(args, results);

            return (results);

        }

        public bool Remove(Option option) {
            int index = options.IndexOf(option);
            if (index < 0) return (false);
            options.RemoveAt(index);
            map.Remove(option.Name);
            return (true);
        }

        // Print help

        public void WriteOptionSummary(TextWriter writer) {
            if (writer == null) throw new ArgumentNullException("writer");
            foreach (Option option in options) {
                writer.WriteLine();
                option.WriteTemplate(writer);
                writer.WriteLine(option.Description);
            }
        }

        private void ParseArguments(string[] args, ParseArgumentsResult results) {

            foreach (string arg in args) {
                if (arg.Length == 0) continue;
                if (arg[0] == '/') {
                    // option processing
                    // find the named option
                    int index = 1;
                    while (index < arg.Length) {
                        if ((!Char.IsLetter(arg, index)) && (arg[index] != '?')) break;
                        index++;
                    }
                    string key = arg.Substring(1, index - 1);
                    string value = arg.Substring(index);
                    // invoke the appropriate logic
                    if (map.ContainsKey(key)) {
                        Option option = (Option)map[key];
                        ParseResult result = option.ParseArgument(value);
                        if (result != ParseResult.Success) {
                            results.errors.Add(arg, result);
                        }
                    } else {
                        results.errors.Add(arg, ParseResult.UnrecognizedOption);
                    }
                } else if (arg[0] == '@') {
                    string responseFile = arg.Substring(1);
                    List < string > responses = new List < string >();
                    using (TextReader reader = File.OpenText(responseFile)) {
                        while (true) {
                            string response = reader.ReadLine();
                            if (response == null) break;
                            responses.Add(response);
                        }
                    }
                    ParseArguments(responses.ToArray(), results);
                } else {
                    // non-option processing
                    results.nonoptions.Add(arg);
                }
            }

            // make sure the required arguments were present
            foreach (Option option in map.Values) {
                option.processed = true;
                if ((option.IsRequired) && (!option.IsPresent)) {
                    results.errors.Add(option.Name, ParseResult.MissingOption);
                }
            }

        }

    }

}
