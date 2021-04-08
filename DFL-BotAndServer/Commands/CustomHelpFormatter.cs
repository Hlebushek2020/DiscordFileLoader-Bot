using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace DFL_BotAndServer.Commands
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder embed;

        public CustomHelpFormatter(CommandContext commandContext) : base(commandContext) =>
            embed = new DiscordEmbedBuilder();

        public override CommandHelpMessage Build()
        {
            embed.Title = "≧◡≦ | >yuko";
            embed.Description = "Бот предназначен для скачивания вложений(я) из сообщений(я). Больше информации тут: https://github.com/Hlebushek2020/DiscordFileLoader-Bot";
            embed.Color = DiscordColor.Orange;
            embed.Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}" };
            return new CommandHelpMessage(embed: embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            embed.AddField(command.Name, command.Description);
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            foreach (var cmd in subcommands)
                if (string.IsNullOrEmpty(cmd.Description))
                    embed.AddField(cmd.Name, "empty");
                else
                    embed.AddField(cmd.Name, cmd.Description);
            return this;
        }
    }
}
