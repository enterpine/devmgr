using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(devmgr.Startup))]
namespace devmgr
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
