using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetOpenId.Yadis {
	internal class ContentType {
		public const string Html = "text/html";
		public const string XHtml = "application/xhtml+xml";
		public const string Xrds = "application/xrds+xml";

		public NameValueCollection Parameters { get; private set; }
		public string SubType { get; set; }
		public string Type { get; set; }

		public ContentType(string contentType) {
			string message = String.Format("\"{0}\" does not appear to be a valid content type", contentType);
			Parameters = new NameValueCollection();
			const char SEMI = ';';
			const char SLASH = '/';
			const char EQUALS = '=';
			string[] parts = contentType.Split(new char[] { SEMI });
			try {
				string[] slashedArray = parts[0].Split(new char[] { SLASH });
				Type = slashedArray[0];
				SubType = slashedArray[1];
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException(message);
			}
			Type = Type.Trim();
			SubType = SubType.Trim();

			for (int i = 1; i < parts.Length; i++) {
				string param = parts[i];

				string k;
				string v;
				try {
					string[] equalsArray = param.Split(new char[] { EQUALS });
					k = equalsArray[0];
					v = equalsArray[1];
				} catch (IndexOutOfRangeException) {
					throw new ArgumentException(message);
				}

				Parameters[k.Trim()] = v.Trim();
			}
		}

		public string MediaType {
			get { return String.Format("{0}/{0}", Type, SubType); }
		}
	}
}
