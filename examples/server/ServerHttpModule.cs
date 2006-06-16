using System;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using Janrain.OpenId;
using Janrain.OpenId.Server;
using Janrain.OpenId.Store;

namespace Janrain.OpenId.Server.Asp
{
    public class ServerHttpModule : IHttpModule
    {
        private String[] rewriteTable;

        public void Init( HttpApplication appl )
        {   
            appl.AcquireRequestState += new EventHandler(this.ServeRequest);
            appl.BeginRequest += new System.EventHandler(Rewrite);
            this.rewriteTable = new String[]{"user", "xrds"};
        }

        private void ServeRequest(Object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;
            HttpSessionState session = context.Session;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
        }

        public static string ExtractUserName(Uri url, HttpRequest request)
        {
            UriBuilder builder = new UriBuilder(
                request.Url.AbsoluteUri);
            builder.Query = null;
            builder.Password = null;
            builder.UserName = null;
            builder.Fragment = null;
            builder.Path = Combine(
                HttpRuntime.AppDomainAppVirtualPath, "~/user/");

            String prefix = builder.ToString();
            String url_str = url.AbsoluteUri;
            if (url_str.StartsWith(prefix))
                return url_str.Substring(prefix.Length,
                                         url_str.Length - prefix.Length);
            
            return null;
        }

        // Rewrite pretty paths
        private void Rewrite(Object sender, EventArgs e)
        {
            HttpContext current = HttpContext.Current;
            HttpRequest request = current.Request;
            HttpResponse response = current.Response;
            String path = request.Url.AbsolutePath;
            foreach (String rpath in rewriteTable)
            {
                String prefix = String.Format("/{0}/", rpath);
                String newpath = String.Format("~/{0}.aspx", rpath);
                if (RewriteUserMatch(prefix, newpath))
                   return;
                
                if (path.StartsWith(newpath))
                {
                    String name = request.QueryString["name"];
                    
                    response.Redirect(Combine(
                        HttpRuntime.AppDomainAppVirtualPath,
                        String.Format("~/{0}/{1}", rpath, name)));
                }
            }
        }

        private bool RewriteUserMatch(string prefix, string newpath)
        {
            HttpContext current = HttpContext.Current;
            String path = current.Request.Url.AbsolutePath;
            if (path.StartsWith(prefix))
            {
                String name = path.Substring(prefix.Length,
                                         path.Length - prefix.Length);
                String qs = String.Format("name={0}",
                                          HttpUtility.UrlEncode(name));
                String x = current.Response.ApplyAppPathModifier(newpath);
                current.RewritePath(x, "", qs);
                return true;
            }
            return false;
        }

        // Lifted from Mono's  System.Web.Util internal class UrlUtils 
        private static string Combine (string basePath, string relPath)
        {
                if (relPath == null)
                        throw new ArgumentNullException ("relPath");

                int rlength = relPath.Length;
                if (rlength == 0)
                        return "";

                relPath = relPath.Replace ("\\", "/");
                if (IsRooted (relPath))
                        return Canonic (relPath);

                char first = relPath [0];
                if (rlength < 3 || first == '~' || first == '/' || first == '\\') {
                        if (basePath == null || (basePath.Length == 1 && basePath [0] == '/'))
                                basePath = String.Empty;

                        string slash = (first == '/') ? "" : "/";
                        if (first == '~') {
                                if (rlength == 1) {
                                        relPath = "";
                                } else if (rlength > 1 && relPath [1] == '/') {
                                        relPath = relPath.Substring (2);
                                        slash = "/";
                                }

                                string appvpath = HttpRuntime.AppDomainAppVirtualPath;
                                if (appvpath.EndsWith ("/"))
                                        slash = "";

                                return Canonic (appvpath + slash + relPath);
                        }

                        return Canonic (basePath + slash + relPath);
                }

                if (basePath == null || basePath == "" || basePath [0] == '~')
                        basePath = HttpRuntime.AppDomainAppVirtualPath;

                if (basePath.Length <= 1)
                        basePath = String.Empty;

                return Canonic (basePath + "/" + relPath);
        }

        static char [] path_sep = {'\\', '/'};
        
        private static string Canonic (string path)
        {
                string [] parts = path.Split (path_sep);
                int end = parts.Length;
                
                int dest = 0;
                
                for (int i = 0; i < end; i++) {
                        string current = parts [i];
                        if (current == "." )
                                continue;

                        if (current == "..") {
                                if (dest == 0) {
                                        if (i == 1) // see bug 52599
                                                continue;

                                        throw new HttpException ("Invalid path.");
                                }

                                dest --;
                                continue;
                        }

                        parts [dest++] = current;
                }

                if (dest == 0)
                        return "/";

                return String.Join ("/", parts, 0, dest);
        }

        private static bool IsRooted (string path)
        {
                if (path == null || path == "")
                        return true;

                char c = path [0];
                if (c == '/' || c == '\\')
                        return true;

                return false;
        }



        public void Dispose()
        {
        }
    }
}    
