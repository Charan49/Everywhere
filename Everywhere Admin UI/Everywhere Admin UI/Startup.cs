using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Everywhere_Admin_UI.Startup))]
namespace Everywhere_Admin_UI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
