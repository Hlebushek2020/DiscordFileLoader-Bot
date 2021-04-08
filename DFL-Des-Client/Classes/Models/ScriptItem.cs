using DFL_Des_Client.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DFL_Des_Client.Classes.Models
{
    public class ScriptItem //: INotifyPropertyChanged
    {
        public GetUrlCommand Command { get; set; }
        public string ChannelName { get; set; }
        public int Count { get; set; }
        public ulong MessageId { get; set; }
    }
}
