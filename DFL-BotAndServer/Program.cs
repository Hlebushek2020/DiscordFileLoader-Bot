using System;

namespace DFL_BotAndServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Yuko[Bot]";

            if (!Settings.Availability())
                return;
            
            Console.SetOut(new MultiLog(Console.Out));

            using (YukoBot yukoBot = new YukoBot(Settings.GetInstance()))
                yukoBot.RunAsync().GetAwaiter().GetResult();
        }
    }

}
