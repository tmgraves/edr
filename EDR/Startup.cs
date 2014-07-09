using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EDR.Startup))]
namespace EDR
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
