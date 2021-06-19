using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DFL_BotAndServer.Commands
{
    public static class GeneralCommands
    {
        public static string Access(CommandContext commandContext)
        {
            DiscordGuild guild = commandContext.Member.Guild;
            ulong botId = commandContext.Client.CurrentUser.Id;
            IEnumerable<DiscordChannel> channels = guild.GetChannelsAsync().Result;
            StringBuilder sb = new StringBuilder();
            channels = channels.Where(x => !x.IsCategory && x.Users.Where(x => x.Id == botId).Any());
            foreach (DiscordChannel discordChannel in channels)
            {
                sb.AppendLine(discordChannel.Name);
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
