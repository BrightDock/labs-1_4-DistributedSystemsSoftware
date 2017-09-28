using Microsoft.AspNet.SignalR;
using System.Collections.Generic;

namespace labs_1_4_DistributedSystemsSoftware.Hubs
{
    public abstract class Hub<T> : Hub where T : Hub
    {
        private static IHubContext hubContext;
        /// <summary>Gets the hub context.</summary>
        /// <value>The hub context.</value>
        public static IHubContext HubContext
        {
            get
            {
                if (hubContext == null)
                    hubContext = GlobalHost.ConnectionManager.GetHubContext<T>();
                return hubContext;
            }
        }
    }
}