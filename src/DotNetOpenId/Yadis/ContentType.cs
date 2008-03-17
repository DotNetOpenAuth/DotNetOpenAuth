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

		public static implicit operator string(ContentType contentType) {
			return contentType.ToString();
		}
		public static implicit operator ContentType(string contentType) {
			return new ContentType(contentType);
		}

		public ContentType(string contentType) {
			ArgumentException ex = new ArgumentException(
				String.Format("\"{0}\" does not appear to be a valid content type", contentType),
				"contentType");
			Parameters = new NameValueCollection();
			string[] parts = contentType.Split(';');
			string[] slashedArray = parts[0].Split('/');
			if (slashedArray.Length < 2) throw ex;
			Type = slashedArray[0].Trim();
			SubType = slashedArray[1].Trim();

			for (int i = 1; i < parts.Length; i++) {
				string param = parts[i];

				string k, v;
				string[] equalsArray = param.Split('=');
				if (equalsArray.Length < 2) throw ex;
				k = equalsArray[0];
				v = equalsArray[1];

				Parameters[k.Trim()] = v.Trim();
			}
		}

		public string MediaType {
			get { return String.Format("{0}/{0}", Type, SubType); }
		}

		public override string ToString() {
			return MediaType;
		}
	}
}
