using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DFL_BotAndServer
{
    public partial class YukoBot : IDisposable
    {
        private async Task GetAttachment(BotClient botClient, ulong channelId, ulong messageId)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] GetAttachment");
            try
            {
                DiscordChannel discordChannel = null;
                try
                {
                    discordChannel = await discordClient.GetChannelAsync(channelId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] [ERROR C] {ex.Message}");
                    botClient.SendError(ChannelNotFound);
                    return;
                }

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.UserId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] [ERROR M] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                DiscordMessage discordMessage = await discordChannel.GetMessageAsync(messageId);
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id} {botClient.UserId}] Request completed");

                botClient.SendAttachments(discordMessage);
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] [ERROR] {ex.Message}";
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

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] GetAttachment Completed");
        }
    }
}
