using DFL_BotAndServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DFL_Des_Client.Classes
{
    public static class InternetСheck
    {
        public static bool Check(out int code, out string message)
        {
            bool result = false;
            code = -1;
            message = "";
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply pingReply = ping.Send(Settings.GetInstance().InternetCheckAddress);
                    if (pingReply.Status == IPStatus.Success)
                        result = true;
                    code = (int)pingReply.Status;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            return result;
        }
    }
}