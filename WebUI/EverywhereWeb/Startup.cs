using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EverywhereWeb.Startup))]
namespace EverywhereWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
