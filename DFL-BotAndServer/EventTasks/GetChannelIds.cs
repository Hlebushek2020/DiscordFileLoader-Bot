using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DFL_BotAndServer
{
    public partial class YukoBot : IDisposable
    {
        private async Task GetChannelIds(BotClient botClient, ulong discordServerId)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetChannelIds");
            try
            {
                DiscordGuild discordGuild = null;
                try
                {
                    discordGuild = await discordClient.GetGuildAsync(discordServerId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR G] {ex.Message}");
                    botClient.SendError(ServerNotFound);
                    return;
                }

                DiscordMember discordMemberBot = null;
                DiscordMember discordMemberUsr = null;
                try
                {
                    discordMemberBot = await discordGuild.GetMemberAsync(discordClient.CurrentUser.Id);
                    discordMemberUsr = await discordGuild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR MB] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                IReadOnlyList<DiscordChannel> discordChannels = await discordGuild.GetChannelsAsync();

                discordChannels = discordChannels.Where(x => !x.IsCategory && x.Users.Contains(discordMemberBot) && x.Users.Contains(discordMemberUsr)).ToList();

                botClient.SendChannels(discordChannels);
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                try
                {
                    if (ex is IOException || ex is SocketException)
                        botClient.Disconnect();
                    else
                    {
                        if (!botClient.IsDisposed)
                            botClient.SendError(ex.Message);
                    }
                }
                catch { }
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetChannelIds Completed");
        }
    }
}
