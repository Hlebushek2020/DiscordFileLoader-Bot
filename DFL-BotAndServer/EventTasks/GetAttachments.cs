using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DFL_BotAndServer
{
    public partial class YukoBot : IDisposable
    {
        private async Task GetAttachments(BotClient botClient, ulong channelId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachments");
            try
            {
                DiscordChannel discordChannel = null;
                try
                {
                    discordChannel = await discordClient.GetChannelAsync(channelId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR C] {ex.Message}");
                    botClient.SendError(ChannelNotFound);
                    return;
                }

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR M] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                int countBase = count;
                int limit = MessageLimit;

                if (count >= limit)
                    count -= limit;
                else
                {
                    limit = count;
                    count = 0;
                }

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAsync(limit: limit);
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id}] [{limit}|{messages.Count}|{countBase}] Request completed");

                ulong endId = messages.Last().Id;

                botClient.SendAttachments(messages, count > 0);

                while (count != 0)
                {
                    if (count >= limit)
                        count -= limit;
                    else
                    {
                        limit = count;
                        count = 0;
                    }

                    Thread.Sleep(1000);

                    messages = await discordChannel.GetMessagesBeforeAsync(endId, limit);
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id}] [{limit}|{messages.Count}|{countBase}] Request completed");

                    if (messages.Count < MessageLimit)
                        count = 0;
                    else
                        endId = messages.Last().Id;

                    botClient.SendAttachments(messages, count > 0);
                }
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

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachments Completed");
        }
    }
}
