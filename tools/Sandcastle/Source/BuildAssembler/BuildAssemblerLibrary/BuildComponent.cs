// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools {

	public abstract class BuildComponent : IDisposable {

		protected BuildComponent (BuildAssembler assembler, XPathNavigator configuration) {
            this.assembler = assembler;
			WriteMessage(MessageLevel.Info, "Instantiating component.");
		}

		public abstract void Apply (XmlDocument document, string key);

		public virtual void Apply (XmlDocument document) {
			Apply(document, null);
		}

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing) {
        }

		// shared data

        private BuildAssembler assembler;

        public BuildAssembler BuildAssembler {
            get {
                return(assembler);
            }
        }

        //private MessageHandler handler;

		private static Dictionary<string,object> data = new Dictionary<string,object>();

		protected static Dictionary<string,object> Data {
			get {
				return(data);
			}
		}

        // component messaging facility

        protected void OnComponentEvent (EventArgs e) {
            assembler.OnComponentEvent(this.GetType(), e);
        }

		protected void WriteMessage (MessageLevel level, string message) {
            if (level == MessageLevel.Ignore) return;
            MessageHandler handler = assembler.MessageHandler;
			if (handler != null) handler(this.GetType(), level, message);			
		}

	}

	public enum MessageLevel {
        Ignore,     // don't show at all
		Info,		// informational message
		Warn,		// a minor problem occured
		Error		// a major problem occured
	}


	public delegate void MessageHandler (Type component, MessageLevel level, string message);


}
