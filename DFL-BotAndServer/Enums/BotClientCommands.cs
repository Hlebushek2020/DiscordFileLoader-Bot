using System;
using System.Collections.Generic;
using System.Text;

namespace DFL_BotAndServer.Enums
{
    public enum BotClientCommands : byte
    {
        GetUrls = 0,
        GetChannelIds = 1,
        EndSession = 2
    }
}
