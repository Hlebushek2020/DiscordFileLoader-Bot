using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DFL_BotAndServer.Commands;
using System.Linq;
using System.Threading;
using DFL_Des_Client.Classes;
using DSharpPlus.EventArgs;
using DFL_BotAndServer.Interfaces;

namespace DFL_BotAndServer
{
    public partial class YukoBot : IDisposable
    {
        private static YukoBot yukoBot;

        private const string ServerNotFound = "Сервер не найден или бот не авторизован. Действие отклонено.";
        private const string ChannelNotFound = "Канал не найден или бот не авторизован. Действие отклонено.";
        private const string UserNotFound = "Вас нет на этом сервере. Действие отклонено.";
        private const int MessageLimit = 100;

        public bool IsDisposed { get; private set; } = false;
        public int ClientCount { get => clients.Count; }
        public DateTime StartDateTime { get; private set; }

        private readonly DiscordClient discordClient;
        private readonly TcpListener tcpListener;

        private Task processTask;
        private volatile bool isRuning = false;

        private readonly Dictionary<Guid, BotClient> clients = new Dictionary<Guid, BotClient>();

        public static YukoBot GetInstance()
        {
            if (yukoBot == null)
                yukoBot = new YukoBot(Settings.GetInstance());
            return yukoBot;
        }

        private YukoBot(Settings settings)
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
            commands.RegisterCommands<AdminBotCommands>();

            commands.CommandErrored += Commands_CommandErrored;

            discordClient.Ready += DiscordClient_Ready;
            discordClient.SocketErrored += DiscordClient_SocketErrored;

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Initialization Server ...");
            tcpListener = new TcpListener(IPAddress.Parse(settings.InternalAddress), settings.Port);

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Waiting for launch");
        }

        ~YukoBot() => Dispose(false);

        public IEnumerable<IReadOnlyBotClient> GetClientList() => clients.Select(x => x.Value);

        private Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            if (e.Exception.HResult == -2146233088)
                return e.Context.RespondAsync("Я не знаю такой команды :(");
            if (e.Exception.HResult == -2147024809)
                return e.Context.RespondAsync("Ой, в команде ошибка, я не знаю что делать :(");
            return e.Context.RespondAsync($"[{e.Exception.HResult}] {e.Exception.Message}");
        }

        private Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            sender.UpdateStatusAsync(new DiscordActivity("на тебя (≧◡≦) | >yuko help", ActivityType.Watching));

        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [CRIT ERROR] {e.Exception.Message}");
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        public Task RunAsync()
        {
            processTask = Process();
            return processTask;
        }

        private async Task Process()
        {
            if (isRuning) return;

            isRuning = true;

            while (!InternetСheck.Check(out int code, out string message))
            {
                Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [ERROR] [{code}] Network problems " +
                    $"{(string.IsNullOrEmpty(message) ? message : $"({message})")}, re-check after 30 seconds ...");
                Thread.Sleep(30000);
            }
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Network Ok");

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] Authorization ...");
            await discordClient.ConnectAsync();

            StartDateTime = DateTime.Now;

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Start ...");
            tcpListener.Start();

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Waiting for connections");
            while (isRuning)
            {
                try
                {
                    if (!tcpListener.Pending())
                    {
                        Thread.Sleep(50);
                        continue;
                    }

                    BotClient botClient = new BotClient(tcpListener.AcceptTcpClient());
                    Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Clitnt {botClient.Id} connected");

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

        private async void BotClient_GetChannelIdsEvent(BotClient botClient, ulong discordServerId) =>
            await GetChannelIds(botClient, discordServerId);

        private async void BotClient_GetAttacmentsAroundEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttacmentsAround(botClient, channelId, messageId, count);

        private async void BotClient_GetAttacmentsBeforeEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttacmentsBefore(botClient, channelId, messageId, count);

        private async void BotClient_GetAttachmentsAfterEvent(BotClient botClient, ulong channelId, ulong messageId, int count) =>
            await GetAttachmentsAfter(botClient, channelId, messageId, count);

        private async void BotClient_GetAttachmentsEvent(BotClient botClient, ulong channelId, int count = int.MaxValue) =>
            await GetAttachments(botClient, channelId, count);

        private async void BotClient_GetAttachmentEvent(BotClient botClient, ulong channelId, ulong messageId) =>
            await GetAttachment(botClient, channelId, messageId);

        #region Disconnect Event
        private void BotClient_DisconnectEvent(Guid id)
        {
            clients[id].Dispose();
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Client {clients[id].Id} disconnected");
            clients.Remove(id);
        }
        #endregion

        #endregion

        public void Shutdown()
        {
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] Shutdown ...");
            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Stopping the listener ...");

            isRuning = false;

            if (tcpListener != null)
                tcpListener.Stop();

            if (processTask != null)
                processTask.Wait();

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Server] Waiting for clients to disconnect ...");
            while (clients.Count > 0)
                clients.ElementAt(0).Value.Waiting();

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] [Discord Api] Disconnect ...");
            if (discordClient != null)
                discordClient.DisconnectAsync().Wait();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
            {
                if (isRuning)
                    Shutdown();

                if (processTask != null)
                    processTask.Dispose();

                if (discordClient != null)
                    discordClient.Dispose();
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