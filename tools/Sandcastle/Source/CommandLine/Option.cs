// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Ddue.Tools.CommandLine {

    public abstract class Option {

        internal bool processed;

        protected bool present;
        [CLSCompliant(false)]
        protected object value;
        private string description;

        // Data Members

        private string name;
        private bool required;

        // Constructors

        protected Option(string name) {
            foreach (char character in name) {
                if (!(Char.IsLetter(character) || (character == '?'))) throw new ArgumentException("Names must consist of letters.", "name");
            }
            this.name = name;
        }

        protected Option(string name, string description) : this(name) {
            this.description = description;
        }

        protected Option(string name, string description, bool required) : this(name, description) {
            this.required = required;
        }

        public string Description {
            get {
                return (description);
            }
            set {
                if (processed) throw new InvalidOperationException();
                description = value;
            }
        }

        public virtual bool IsPresent {
            get {
                if (!processed) throw new InvalidOperationException();
                return (present);
            }
        }

        public bool IsRequired {
            get {
                return (required);
            }
            set {
                if (processed) throw new InvalidOperationException();
                required = value;
            }
        }

        // Accessors

        public string Name {
            get {
                return (name);
            }
            set {
                if (processed) throw new InvalidOperationException();
                name = value;
            }
        }

        public virtual Object Value {
            get {
                if (!processed) throw new InvalidOperationException();
                return (value);
            }
        }

        // To be implemented by children

        internal abstract ParseResult ParseArgument(string args);

        internal abstract void WriteTemplate(TextWriter writer);

    }

}
