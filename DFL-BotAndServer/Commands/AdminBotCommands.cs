using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DFL_BotAndServer.Commands
{
    public class AdminBotCommands : BaseCommandModule
    {
        private const string NoAdmin = "Ай-яй-яй! Нельзя!";

        [Command("shutdown")]
        [Aliases("down")]
        [Description("Выключить бота")]
        [Hidden]
        public async Task Shutdown(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            if (!commandContext.User.Id.Equals(settings.AdminId))
                await commandContext.RespondAsync(NoAdmin);
            else
            {
                await commandContext.RespondAsync("Ok");
                YukoBot.GetInstance().Shutdown();
            }
        }

        [Command("client-count")]
        [Description("Количество подключенных клиентов")]
        [Hidden]
        public async Task ClientCount(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            if (!commandContext.User.Id.Equals(settings.AdminId))
                await commandContext.RespondAsync(NoAdmin);
            else
                await commandContext.RespondAsync($"Client count: {YukoBot.GetInstance().ClientCount}");
        }

        [Command("client-list")]
        [Description("Список клиентов")]
        [Hidden]
        public async Task ClientList(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            if (!commandContext.User.Id.Equals(settings.AdminId))
                await commandContext.RespondAsync(NoAdmin);
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (KeyValuePair<ulong, DateTime> client in YukoBot.GetInstance().GetClientList())
                    stringBuilder.AppendLine($"[{client.Value.ToLongTimeString()}] {client.Key}");
                if (stringBuilder.Length == 0)
                    stringBuilder.Append("No Clients");
                await commandContext.RespondAsync(stringBuilder.ToString());
            }
        }

        [Command("active-time")]
        [Description("Время работы бота")]
        [Hidden]
        public async Task ActiveTime(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            if (!commandContext.User.Id.Equals(settings.AdminId))
                await commandContext.RespondAsync(NoAdmin);
            else
            {
                TimeSpan timeSpan = DateTime.Now - YukoBot.GetInstance().StartDateTime;
                await commandContext.RespondAsync($"{timeSpan.Days} Days {timeSpan.Hours} Hours {timeSpan.Minutes} Minutes {timeSpan.Seconds} Seconds");
            }
        }
    }
}
