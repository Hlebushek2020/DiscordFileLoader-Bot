using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DFL_BotAndServer.Commands;
using System.Linq;
using DSharpPlus.Exceptions;
using System.Threading;
using System.IO;

namespace DFL_BotAndServer
{
    public class YukoBot : IDisposable
    {
        private const string ServerNotFound = "Сервер не найден или бот не авторизован. Действие отклонено.";
        private const string UserNotFound = "Вас нет на этом сервере. Действие отклонено.";
        //private const string ServerNotFound = "Я не знаю такого сервера (T_T)";
        //private const string UserNotFound = "Я не могу найти вас на этом сервере (T_T)";

        public bool IsDisposed { get; private set; } = false;

        private readonly DiscordClient discordClient;
        private readonly TcpListener tcpListener;

        private Task processTask;
        private volatile bool isRuning = true;

        private readonly Dictionary<ulong, BotClient> clients = new Dictionary<ulong, BotClient>();

        public YukoBot(Settings settings)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Initialization Discord Api ...");
            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Error
            });

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { ">yuko" }
            });
            commands.SetHelpFormatter<CustomHelpFormatter>();
            commands.RegisterCommands<BotCommands>();

            commands.CommandErrored += Commands_CommandErrored;

            discordClient.Ready += DiscordClient_Ready;

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Initialization Server ...");
            tcpListener = new TcpListener(IPAddress.Parse(settings.InternalAddress), settings.Port);

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Waiting for launch");
        }

        ~YukoBot() => Dispose(false);

        private Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception.HResult == -2146233088)
                e.Context.RespondAsync("Я не знаю такой команды :(");
            else if (e.Exception.HResult == -2147024809)
                e.Context.RespondAsync("Ой, в команде ошибка, я не знаю что делать :(");
            else
                e.Context.RespondAsync($"[{e.Exception.HResult}] {e.Exception.Message}");
            return Task.CompletedTask;
        }

        private Task DiscordClient_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e) =>
            sender.UpdateStatusAsync(new DiscordActivity("≧◡≦ | >yuko help", ActivityType.Playing));

        public Task RunAsync()
        {
            processTask = Process();
            return processTask;
        }

        private async Task Process()
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] Authorization ...");
            await discordClient.ConnectAsync();

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Start ...");
            tcpListener.Start();

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Waiting for connections");
            while (isRuning)
            {
                try
                {
                    BotClient botClient = new BotClient(tcpListener.AcceptTcpClient());

                    if (clients.ContainsKey(botClient.Id))
                    {
                        clients[botClient.Id].Dispose();
                        clients.Remove(botClient.Id);
                        Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Client {botClient.Id} reconnected");
                    }
                    else
                    {
                        Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Clitnt {botClient.Id} connected");
                    }

                    clients.Add(botClient.Id, botClient);

                    botClient.GetChannelIdsEvent += BotClient_GetChannelIdsEvent;

                    botClient.GetAttachmentEvent += BotClient_GetAttachmentEvent;
                    botClient.GetAttachmentsEvent += BotClient_GetAttachmentsEvent;
                    botClient.GetAttachmentsAfterEvent += BotClient_GetAttachmentsAfterEvent;
                    botClient.GetAttacmentsAroundEvent += BotClient_GetAttacmentsAroundEvent;
                    botClient.GetAttacmentsBeforeEvent += BotClient_GetAttacmentsBeforeEvent;

                    botClient.DisconnectEvent += BotClient_DisconnectEvent;

                    botClient.RunAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [ADD-CLIENT] [ERROR] {ex.Message}");
                }
            }
        }

        #region BotClient Events

        #region GetChannelIds Event
        private async void BotClient_GetChannelIdsEvent(BotClient botClient, ulong discordServerId) =>
            await GetChannelIds(botClient, discordServerId);

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
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
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
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                IReadOnlyList<DiscordChannel> discordChannels = await discordGuild.GetChannelsAsync();

                botClient.SendChannels(discordChannels.Where(x => x.Users.Contains(discordMemberBot) && x.Users.Contains(discordMemberUsr)).ToList());
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetChannelIds Completed");
        }
        #endregion

        #region GetAttacmentsAround Event
        private async void BotClient_GetAttacmentsAroundEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttacmentsAround(botClient, channelId, messageId, count);

        private async Task GetAttacmentsAround(BotClient botClient, ulong channelId, ulong messageId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttacmentsBefore");
            try
            {
                DiscordChannel discordChannel = await discordClient.GetChannelAsync(channelId);

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                if (count > 100)
                    count = 100;

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesBeforeAsync(messageId, count);
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id}] [{count}|{messages.Count}] Request completed");

                botClient.SendAttachments(messages, count > 0);
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttacmentsBefore Completed");
        }
        #endregion

        #region GetAttacmentsBefore Event
        private async void BotClient_GetAttacmentsBeforeEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttacmentsBefore(botClient, channelId, messageId, count);

        private async Task GetAttacmentsBefore(BotClient botClient, ulong channelId, ulong messageId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttacmentsBefore");
            try
            {
                int countBase = count;

                DiscordChannel discordChannel = await discordClient.GetChannelAsync(channelId);

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                int limit = 100;

                if (count >= limit)
                    count -= limit;
                else
                {
                    limit = count;
                    count = 0;
                }

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesBeforeAsync(messageId, limit);
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

                    if (messages.Count < 100)
                        count = 0;

                    botClient.SendAttachments(messages, count > 0);

                    endId = messages.Last().Id;
                }
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttacmentsBefore Completed");
        }
        #endregion

        #region GetAttachmentsAfter Event
        private async void BotClient_GetAttachmentsAfterEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttachmentsAfter(botClient, channelId, messageId, count);

        private async Task GetAttachmentsAfter(BotClient botClient, ulong channelId, ulong messageId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachmentsAfter");
            try
            {
                int countBase = count;

                DiscordChannel discordChannel = await discordClient.GetChannelAsync(channelId);

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                int limit = 100;

                if (count >= limit)
                    count -= limit;
                else
                {
                    limit = count;
                    count = 0;
                }

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAfterAsync(messageId, limit);
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

                    messages = await discordChannel.GetMessagesAfterAsync(endId, limit);
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id}] [{limit}|{messages.Count}|{countBase}] Request completed");

                    if (messages.Count < 100)
                        count = 0;

                    botClient.SendAttachments(messages, count > 0);

                    endId = messages.Last().Id;
                }
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachmentsAfter Completed");
        }
        #endregion

        #region GetAttachments Event
        private async void BotClient_GetAttachmentsEvent(BotClient botClient, ulong channelId, int count = int.MaxValue) =>
            await GetAttachments(botClient, channelId, count);

        private async Task GetAttachments(BotClient botClient, ulong channelId, int count)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachments");
            try
            {
                int countBase = count;

                DiscordChannel discordChannel = await discordClient.GetChannelAsync(channelId);

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                int limit = 100;

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

                    if (messages.Count < 100)
                        count = 0;

                    botClient.SendAttachments(messages, count > 0);

                    endId = messages.Last().Id;
                }
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachments Completed");
        }
        #endregion

        #region GetAttachment Event
        private async void BotClient_GetAttachmentEvent(BotClient botClient, ulong channelId, ulong messageId) =>
            await GetAttachment(botClient, channelId, messageId);

        private async Task GetAttachment(BotClient botClient, ulong channelId, ulong messageId)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachment");
            try
            {
                DiscordChannel discordChannel = await discordClient.GetChannelAsync(channelId);

                DiscordMember discordMember = null;
                try
                {
                    discordMember = await discordChannel.Guild.GetMemberAsync(botClient.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}");
                    botClient.SendError(UserNotFound);
                    return;
                }

                DiscordMessage discordMessage = await discordChannel.GetMessageAsync(messageId);
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] [{botClient.Id}] Request completed");

                botClient.SendAttachments(discordMessage);
            }
            catch (Exception ex)
            {
                string error = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] [ERROR] {ex.Message}";
                Console.WriteLine(error);
                if (ex is IOException)
                    botClient.Disconnect();
                else
                    botClient.SendError(ex.Message);
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] [{botClient.Id}] GetAttachment Completed");
        }
        #endregion

        #region Disconnect Event
        private void BotClient_DisconnectEvent(ulong id)
        {
            clients[id].Dispose();
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Client {clients[id].Id} disconnected");
            clients.Remove(id);
        }
        #endregion

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                isRuning = false;

                if (processTask != null)
                {
                    processTask.Wait();
                    processTask.Dispose();
                }
                if (tcpListener != null)
                    tcpListener.Stop();

                foreach (KeyValuePair<ulong, BotClient> client in clients)
                    client.Value.Dispose();

                if (discordClient != null)
                {
                    discordClient.DisconnectAsync().Wait();
                    discordClient.Dispose();
                }
            }

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}