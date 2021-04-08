using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFL_BotAndServer.Enums
{
    public enum GetUrlCommand : byte
    {
        One = 0,
        After = 1,
        Around = 2,
        Before = 3,
        End = 4,
        All = 5
    }
}
