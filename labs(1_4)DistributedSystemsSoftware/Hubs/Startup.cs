using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(labs_1_4_DistributedSystemsSoftware.Startup))]

namespace labs_1_4_DistributedSystemsSoftware
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}