using System;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using System.Configuration;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Xml.Xsl;
using System.Reflection;
using System.Runtime.CompilerServices;
using DotNetOpenId;
using DotNetOpenId.Provider;

// nicked from http://www.codeproject.com/aspnet/URLRewriter.asp

	public class URLRewriter : IConfigurationSectionHandler {
		protected XmlNode _oRules=null;

        protected URLRewriter() { }

		public string GetSubstitution(string zPath) {
			Regex oReg;

			foreach(XmlNode oNode in _oRules.SelectNodes("rule")) {
				oReg=new Regex(oNode.SelectSingleNode("url/text()").Value,RegexOptions.IgnoreCase);
				Match oMatch=oReg.Match(zPath);

				if(oMatch.Success) {
					return oReg.Replace(zPath,oNode.SelectSingleNode("rewrite/text()").Value);
				}
			}

			return zPath;
		}

		public static void Process() {
            URLRewriter oRewriter = (URLRewriter)System.Configuration.ConfigurationManager.GetSection("system.web/urlrewrites");

			string zSubst=oRewriter.GetSubstitution(HttpContext.Current.Request.Path);

            #region  Trace 
            if (TraceUtil.Switch.TraceInfo)
            {
                string basicTraceMessage = String.Format("Rewriting url '{0}' to '{1}' ", HttpContext.Current.Request.Url.ToString(), zSubst);
                TraceUtil.ServerTrace(basicTraceMessage);
            }

            #endregion		    

			if(zSubst.Length>0) {
				HttpContext.Current.RewritePath(zSubst);
			}
		}

		#region Implementation of IConfigurationSectionHandler
		public object Create(object parent, object configContext, XmlNode section) {			
			_oRules=section;

			// TODO: Compile all Regular Expressions

			return this;
		}
		#endregion
	}
