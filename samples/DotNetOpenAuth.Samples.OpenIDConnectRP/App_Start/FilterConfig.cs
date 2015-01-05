using System.Web;
using System.Web.Mvc;

namespace DotNetOpenAuth.Samples.OpenIDConnectRP {
    public class FilterConfig {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
