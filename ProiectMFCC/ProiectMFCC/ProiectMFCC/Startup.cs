using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProiectMFCC.Startup))]
namespace ProiectMFCC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
