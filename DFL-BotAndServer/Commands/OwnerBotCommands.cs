using DFL_BotAndServer.Interfaces;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace DFL_BotAndServer.Commands
{
    [RequireOwner]
    public class OwnerBotCommands : YukoBaseCommandModule
    {
        //private const string NoAdmin = "Ай-яй-яй! Нельзя!";

        public OwnerBotCommands()
        {
            ModuleName = "Для Создателя";
        }

        [Command("shutdown")]
        [Aliases("down")]
        [Description("Выключить бота")]
        public async Task Shutdown(CommandContext commandContext)
        {
            //Settings settings = Settings.GetInstance();

            //if (!commandContext.User.Id.Equals(settings.AdminId))
            //    await commandContext.RespondAsync(NoAdmin);
            //else
            //{
            await commandContext.RespondAsync("Ok");
            YukoBot.GetInstance().Shutdown();
            //}
        }

        [Command("client-count")]
        [Description("Количество подключенных клиентов")]
        public async Task ClientCount(CommandContext commandContext)
        {
            //Settings settings = Settings.GetInstance();

            //if (!commandContext.User.Id.Equals(settings.AdminId))
            //    await commandContext.RespondAsync(NoAdmin);
            //else
            await commandContext.RespondAsync($"Client count: {YukoBot.GetInstance().ClientCount}");
        }

        [Command("client-list")]
        [Description("Список клиентов")]
        public async Task ClientList(CommandContext commandContext)
        {
            //Settings settings = Settings.GetInstance();

            //if (!commandContext.User.Id.Equals(settings.AdminId))
            //    await commandContext.RespondAsync(NoAdmin);
            //else
            //{
            StringBuilder stringBuilder = new StringBuilder();
            List<IReadOnlyBotClient> clients = YukoBot.GetInstance().GetClientList().ToList();
            if (clients.Count != 0)
            {
                stringBuilder.AppendLine("**Id\t|\tUser Id\t|\tActive\t|\tClient Version**");
                foreach (IReadOnlyBotClient client in clients)
                {
                    stringBuilder.Append(client.Id);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(client.LastActivity.ToLongTimeString());
                    stringBuilder.Append("\t");
                    stringBuilder.Append(client.UserId);
                    stringBuilder.Append("\t");
                    stringBuilder.Append(client.Version);
                    stringBuilder.Append("\n");
                }
            }
            else
            {
                stringBuilder.Append("No Clients");
            }
            await commandContext.RespondAsync(stringBuilder.ToString());
            //}
        }

        [Command("active-time")]
        [Description("Время работы бота")]
        public async Task ActiveTime(CommandContext commandContext)
        {
            //Settings settings = Settings.GetInstance();

            //if (!commandContext.User.Id.Equals(settings.AdminId))
            //    await commandContext.RespondAsync(NoAdmin);
            //else
            //{
            TimeSpan timeSpan = DateTime.Now - YukoBot.GetInstance().StartDateTime;
            await commandContext.RespondAsync($"{timeSpan.Days} Days {timeSpan.Hours} Hours {timeSpan.Minutes} Minutes {timeSpan.Seconds} Seconds");
            //}
        }

        [Command("admin.access")]
        [Aliases("adm.acc")]
        [Description("Показывает каналы к которым у бота есть доступ")]
        public async Task Access(CommandContext commandContext)
        {
            await commandContext.RespondAsync(GeneralCommands.Access(commandContext));
        }
    }
}
