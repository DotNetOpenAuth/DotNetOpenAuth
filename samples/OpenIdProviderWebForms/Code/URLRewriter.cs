namespace OpenIdProviderWebForms.Code {
	using System.Configuration;
	using System.Diagnostics;
	using System.Text.RegularExpressions;
	using System.Web;
	using System.Xml;

	// nicked from http://www.codeproject.com/aspnet/URLRewriter.asp
	public class URLRewriter : IConfigurationSectionHandler {
		public static log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		protected XmlNode rules = null;

		protected URLRewriter() {
		}

		public static void Process() {
			URLRewriter rewriter = (URLRewriter)ConfigurationManager.GetSection("urlrewrites");

			string subst = rewriter.GetSubstitution(HttpContext.Current.Request.Path);

			if (!string.IsNullOrEmpty(subst)) {
				Logger.InfoFormat("Rewriting url '{0}' to '{1}' ", HttpContext.Current.Request.Path, subst);
				HttpContext.Current.RewritePath(subst);
			}
		}

		public string GetSubstitution(string path) {
			foreach (XmlNode node in this.rules.SelectNodes("rule")) {
				// get the url and rewrite nodes
				XmlNode urlNode = node.SelectSingleNode("url");
				XmlNode rewriteNode = node.SelectSingleNode("rewrite");

				// check validity of the values
				if (urlNode == null || string.IsNullOrEmpty(urlNode.InnerText)
					|| rewriteNode == null || string.IsNullOrEmpty(rewriteNode.InnerText)) {
					Logger.Warn("Invalid urlrewrites rule discovered in web.config file.");
					continue;
				}

				Regex reg = new Regex(urlNode.InnerText, RegexOptions.IgnoreCase);

				// if match, return the substitution
				Match match = reg.Match(path);
				if (match.Success) {
					return reg.Replace(path, rewriteNode.InnerText);
				}
			}

			return null; // no rewrite
		}

		#region Implementation of IConfigurationSectionHandler
		public object Create(object parent, object configContext, XmlNode section) {
			this.rules = section;

			return this;
		}
		#endregion
	}
}