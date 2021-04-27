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
        private async Task GetAttacmentsBefore(BotClient botClient, ulong channelId, ulong messageId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] GetAttacmentsBefore");
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

                int countBase = count;
                int limit = MessageLimit;

                IReadOnlyList<DiscordMessage> messages;

                while (count != 0)
                {
                    if (count >= limit)
                        count -= limit;
                    else
                    {
                        limit = count;
                        count = 0;
                    }

                    messages = await discordChannel.GetMessagesBeforeAsync(messageId, limit);
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id} {botClient.UserId}] [{limit}|{messages.Count}|{countBase}] Request completed");

                    if (messages.Count < MessageLimit)
                        count = 0;

                    botClient.SendAttachments(messages, count > 0);

                    if (count > 0)
                    {
                        messageId = messages.First().Id;
                        Thread.Sleep(1000);
                    }
                }
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

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id} {botClient.UserId}] GetAttacmentsBefore Completed");
        }
    }
}
