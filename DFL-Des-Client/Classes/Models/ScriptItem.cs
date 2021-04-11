using DFL_Des_Client.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFL_Des_Client.Classes.Models
{
    public class ScriptItem
    {
        public GetUrlCommand Command { get; set; }
        public string ChannelName
        {
            get
            {
                if (App.Settings.ChannelIds.ContainsKey(ChannelId))
                    return App.Settings.ChannelIds[ChannelId];
                return string.Empty;
            }
        }
        public int Count { get; set; }
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
    }
}
