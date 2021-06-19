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
            Dictionary<string, List<string[]>> sortedCommand = new Dictionary<string, List<string[]>>();

            foreach (Command command in subcommands)
            {
                if (command.Module is null)
                    continue;

                if (!(command.Module is SingletonCommandModule))
                    continue;

                YukoBaseCommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as YukoBaseCommandModule;

                if (string.IsNullOrEmpty(yukoModule.ModuleName))
                    continue;

                if (!sortedCommand.ContainsKey(yukoModule.ModuleName))
                    sortedCommand.Add(yukoModule.ModuleName, new List<string[]>());

                StringBuilder aliases = new StringBuilder();
                foreach (string alias in command.Aliases)
                    aliases.Append(alias).Append(", ");

                string fullNameCommand = command.Name;
                if (aliases.Length > 0)
                {
                    aliases.Remove(aliases.Length - 2, 2);
                    fullNameCommand = $"{fullNameCommand} ({aliases})";
                }
                sortedCommand[yukoModule.ModuleName].Add(new string[] { fullNameCommand, command.Description });
            }

            foreach (var commandsEntry in sortedCommand)
            {
                embed.AddField(commandsEntry.Key, new string('=', commandsEntry.Key.Length));
                foreach (string[] item in commandsEntry.Value)
                {
                    embed.AddField(item[0], item[1]);
                }
            }

            return this;
        }
    }
}
