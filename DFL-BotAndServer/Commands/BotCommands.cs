using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DFL_BotAndServer.Commands
{
    public class BotCommands : BaseCommandModule
    {

        [Command("get-my-id")]
        [Description("Выдает Id пользователя выполнившего данную команду.")]
        public async Task GetMyId(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"Привет {commandContext.Member.DisplayName}, ≧◡≦, твой Id:")
                .WithDescription(commandContext.Member.Id.ToString());

            await commandContext.RespondAsync(embed: discordEmbed.Build());
        }

        [Command("get-client-settings")]
        [Description("Выдает данные для подключения клиента к боту. Поле \"Id Пользователя\" содержит Id пользователя выполнившего данную команду.")]
        public async Task GetClientSettings(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"Привет {commandContext.Member.DisplayName}, ≧◡≦, вот данные для подключения:");
            discordEmbed.AddField("Хост", settings.PublicAddress);
            discordEmbed.AddField("Порт", settings.Port.ToString());
            discordEmbed.AddField("Id пользователя", commandContext.Member.Id.ToString());
            discordEmbed.AddField("Id Сервера", commandContext.Guild.Id.ToString());

            await commandContext.RespondAsync(embed: discordEmbed.Build());
        }

        [Command("get-client-app")]
        [Description("Выдает ссылку на скачивание актуальной версии клиента.")]
        public async Task GetClientApp(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"Привет {commandContext.Member.DisplayName}, ≧◡≦, вот актуальная версия приложения:")
                .WithDescription(settings.ActualAppUrl);

            await commandContext.RespondAsync(embed: discordEmbed.Build());
        }

        [Command("version")]
        [Description("Выдает текущую версию бота.")]
        public async Task Version(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"Привет {commandContext.Member.DisplayName}, ≧◡≦, текущая версия:")
                .WithDescription(Assembly.GetExecutingAssembly().GetName().Version.ToString());

            await commandContext.RespondAsync(embed: discordEmbed.Build());
        }

        [Command("kill")]
        public async Task Kill(CommandContext commandContext)
        {
            Settings settings = Settings.GetInstance();

            if (!commandContext.User.Id.Equals(settings.AdminId))
                await commandContext.RespondAsync("Ай-яй-яй! Нельзя!");
            else
            {
                await commandContext.RespondAsync("Ok");
                Environment.Exit(0);
            }
        }

    }
}
