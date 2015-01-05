using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DotNetOpenAuth.Samples.OpenIDConnectRP.Startup))]
namespace DotNetOpenAuth.Samples.OpenIDConnectRP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
