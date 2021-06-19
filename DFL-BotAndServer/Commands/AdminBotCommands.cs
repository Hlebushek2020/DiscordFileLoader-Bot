using DFL_BotAndServer.Interfaces;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus;

namespace DFL_BotAndServer.Commands
 {
    [RequireUserPermissions(Permissions.Administrator)]
    public class AdminBotCommands : YukoBaseCommandModule
    {
        //private const string NoAdmin = "Ай-яй-яй! Нельзя!";

        public AdminBotCommands()
        {
            ModuleName = "Для Админа";
        }

        [Command("access")]
        [Description("Показывает каналы к которым у бота есть доступ")]
        public async Task Access(CommandContext commandContext)
        {
            await commandContext.RespondAsync(GeneralCommands.Access(commandContext));
        }
    }
}