using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Security;

namespace DotNetOpenId {
	public class TraceUtil {
		static TraceSwitch openIDTraceSwitch;
		static TraceSwitch openIDPageOutputTraceSwitch;
		public static TraceSwitch Switch {
			get {
				if (openIDTraceSwitch == null) { openIDTraceSwitch = new TraceSwitch("OpenID", "OpenID Trace Switch"); }
				return openIDTraceSwitch;
			}
		}
		public static bool TracePageOutput {
			get {
				if (openIDPageOutputTraceSwitch == null) { openIDPageOutputTraceSwitch = new TraceSwitch("OpenIDEntireResponse", "OpenID Trace Switch"); }
				return openIDPageOutputTraceSwitch.Level > TraceLevel.Warning;
			}
		}

		const string providerCategory = "OpenID Provider";
		const string consumerCategory = "OpenID Consumer";

		public static void ProviderTrace(string message) {
			System.Diagnostics.Trace.WriteLine(message, providerCategory);
		}

		public static void ConsumerTrace(string message) {
			System.Diagnostics.Trace.WriteLine(message, consumerCategory);
		}

		/// <summary>
		/// Serialize obj to an xml string.
		/// </summary>
		public static string ToString(object obj) {
			XmlSerializer serializer = new XmlSerializer(obj.GetType());
			using (StringWriter writer = new StringWriter()) {
				serializer.Serialize(writer, obj);
				return writer.ToString();
			}
		}

		public static string ToString(NameValueCollection collection) {
			using (StringWriter sw = new StringWriter()) {
				foreach (string key in collection.Keys) {
					sw.WriteLine("{0} = '{1}'", key, collection[key]);
				}
				return sw.ToString();
			}
		}
	}
}
