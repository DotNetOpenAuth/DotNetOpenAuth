using System.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

// nicked from http://www.codeproject.com/aspnet/URLRewriter.asp
namespace ProviderPortal {
	public class URLRewriter : IConfigurationSectionHandler {
		protected XmlNode _oRules = null;

		protected URLRewriter() { }

		public string GetSubstitution(string zPath) {
			Regex oReg;

			foreach (XmlNode oNode in _oRules.SelectNodes("rule")) {
				// get the url and rewrite nodes
				XmlNode oUrlNode = oNode.SelectSingleNode("url");
				XmlNode oRewriteNode = oNode.SelectSingleNode("rewrite");

				// check validity of the values
				if (oUrlNode == null || string.IsNullOrEmpty(oUrlNode.InnerText)
					|| oRewriteNode == null || string.IsNullOrEmpty(oRewriteNode.InnerText)) {
					Trace.TraceWarning("Invalid urlrewrites rule discovered in web.config file.");
					continue;
				}

				oReg = new Regex(oUrlNode.InnerText, RegexOptions.IgnoreCase);

				// if match, return the substitution
				Match oMatch = oReg.Match(zPath);
				if (oMatch.Success) {
					return oReg.Replace(zPath, oRewriteNode.InnerText);
				}
			}

			return null; // no rewrite
		}

		public static void Process() {
			URLRewriter oRewriter = (URLRewriter)System.Configuration.ConfigurationManager.GetSection("urlrewrites");

			string zSubst = oRewriter.GetSubstitution(HttpContext.Current.Request.Path);

			if (!string.IsNullOrEmpty(zSubst)) {
				Trace.TraceInformation("Rewriting url '{0}' to '{1}' ", HttpContext.Current.Request.Path, zSubst);
				HttpContext.Current.RewritePath(zSubst);
			}
		}

		#region Implementation of IConfigurationSectionHandler
		public object Create(object parent, object configContext, XmlNode section) {
			_oRules = section;

			return this;
		}
		#endregion
	}
}