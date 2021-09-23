using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace Discord
{
    class Discord
    {
        public DiscordRpc.EventHandlers handlers;
        public DiscordRpc.RichPresence presence;
        public string appID = "889905515235258408";
        public void Initialize()
        {
            handlers = new DiscordRpc.EventHandlers();
            DiscordRpc.Initialize(appID, ref handlers, true, null);
        }

        public void UpdatePresence(string state)
        {
            presence.state = state;
            DiscordRpc.UpdatePresence(ref presence);
        }

        public void Shutdown()
        {
            DiscordRpc.Shutdown();
        }
    }
}
