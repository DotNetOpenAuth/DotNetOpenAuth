using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Security;
using System.Globalization;

namespace DotNetOpenId {
	internal class TraceUtil {
		static TraceSwitch openIDTraceSwitch;
		/// <summary>
		/// Gets the switch that indicates whether the user of this library desires for trace messages
		/// to be emitted.
		/// </summary>
		public static TraceSwitch Switch {
			get {
				if (openIDTraceSwitch == null) { openIDTraceSwitch = new TraceSwitch("OpenID", "OpenID Trace Switch"); }
				return openIDTraceSwitch;
			}
		}

		/// <summary>
		/// Serialize obj to an xml string.
		/// </summary>
		public static string ToString(object obj) {
			XmlSerializer serializer = new XmlSerializer(obj.GetType());
			using (StringWriter writer = new StringWriter(CultureInfo.CurrentUICulture)) {
				serializer.Serialize(writer, obj);
				return writer.ToString();
			}
		}

		public static string ToString(NameValueCollection collection) {
			using (StringWriter sw = new StringWriter(CultureInfo.CurrentUICulture)) {
				foreach (string key in collection.Keys) {
					sw.WriteLine("{0} = '{1}'", key, collection[key]);
				}
				return sw.ToString();
			}
		}
	}
}
