using System.ComponentModel;
using System.Web.WebPages.Razor;
using DotNetOpenAuth.Web;

namespace DotNetOpenAuth.WebPages
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PreApplicationStartCode
    {
        public static void Start()
        {
            WebPageRazorHost.AddGlobalImport("DotNetOpenAuth.Web");
            OAuthWebSecurity.RegisterDataProvider(new WebPagesOAuthDataProvider());
        }
    }
}