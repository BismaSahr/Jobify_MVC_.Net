using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Jobify.Startup))]
namespace Jobify
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
