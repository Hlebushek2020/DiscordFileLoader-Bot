using DFL_BotAndServer.Enums;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DFL_BotAndServer
{
    public class BotClient
    {
        public ulong Id { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        public DateTime LastActivity { get; private set; } = DateTime.Now;

        private readonly TcpClient client;
        private readonly NetworkStream networkStream;
        private readonly BinaryReader binaryReader;
        private readonly BinaryWriter binaryWriter;

        private BotClientVersion version;
        private Task processTask;
        private volatile bool isRuning = true;

        #region Events
        public delegate void DisconnectEventHandler(ulong id);
        public event DisconnectEventHandler DisconnectEvent;

        public delegate void GetChannelIdsEventHandler(BotClient botClient, ulong discordServerId);
        public event GetChannelIdsEventHandler GetChannelIdsEvent;

        #region Attachment Events
        public delegate void GetAttachmentsEventHandler(BotClient botClient, ulong channelId, int count = int.MaxValue);
        public event GetAttachmentsEventHandler GetAttachmentsEvent;

        public delegate void GetAttachmentEventHandler(BotClient botClient, ulong channelId, ulong messageId);
        public event GetAttachmentEventHandler GetAttachmentEvent;

        public delegate void GetAttachmentsAfterEventHandler(BotClient botClient, ulong channelId, ulong messageId, int count);
        public event GetAttachmentsAfterEventHandler GetAttachmentsAfterEvent;

        public delegate void GetAttacmentsAroundEventHandler(BotClient botClient, ulong channelId, ulong messageId, int count);
        public event GetAttacmentsAroundEventHandler GetAttacmentsAroundEvent;

        public delegate void GetAttacmentsBeforeEventHandler(BotClient botClient, ulong channelId, ulong messageId, int count);
        public event GetAttacmentsBeforeEventHandler GetAttacmentsBeforeEvent;
        #endregion
        #endregion

        public BotClient(TcpClient client)
        {
            this.client = client;
            networkStream = client.GetStream();
            binaryReader = new BinaryReader(networkStream, Encoding.UTF8);
            binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8);
            Id = binaryReader.ReadUInt64();
        }

        ~BotClient() => Dispose(false);

        public async void RunAsync()
        {
            processTask = Process();
            await processTask;
        }

        private async Task Process()
        {
            try
            {
                version = new BotClientVersion(binaryReader);
                if (!version.CheckCompatibility())
                {
                    SendError("Текущая версия клиента не совместима с текущей версией бота");
                    isRuning = false;
                    DisconnectEvent?.Invoke(Id);
                }
                else
                    binaryWriter.Write(true);

                while (isRuning)
                {
                    if ((DateTime.Now - LastActivity).TotalMinutes > 3.0)
                        DisconnectEvent?.Invoke(Id);

                    if (networkStream.DataAvailable)
                    {
                        BotClientCommands clientCommand = (BotClientCommands)binaryReader.ReadByte();

                        if (clientCommand == BotClientCommands.EndSession)
                            DisconnectEvent?.Invoke(Id);
                        else if (clientCommand == BotClientCommands.GetChannelIds)
                        {
                            ulong discordServerId = binaryReader.ReadUInt64();
                            GetChannelIdsEvent?.Invoke(this, discordServerId);
                        }
                        else if (clientCommand == BotClientCommands.GetUrls)
                        {
                            GetUrlCommand urlCommand = (GetUrlCommand)binaryReader.ReadByte();

                            ulong channelId = binaryReader.ReadUInt64();
                            ulong messageId = binaryReader.ReadUInt64();
                            int count = binaryReader.ReadInt32();

                            switch (urlCommand)
                            {
                                case GetUrlCommand.One:
                                    GetAttachmentEvent?.Invoke(this, channelId, messageId);
                                    break;
                                case GetUrlCommand.All:
                                    GetAttachmentsEvent?.Invoke(this, channelId);
                                    break;
                                case GetUrlCommand.End:
                                    GetAttachmentsEvent?.Invoke(this, channelId, count);
                                    break;

                                case GetUrlCommand.After:
                                    GetAttachmentsAfterEvent?.Invoke(this, channelId, messageId, count);
                                    break;
                                case GetUrlCommand.Around:
                                    GetAttacmentsAroundEvent?.Invoke(this, channelId, messageId, count);
                                    break;
                                case GetUrlCommand.Before:
                                    GetAttacmentsBeforeEvent?.Invoke(this, channelId, messageId, count);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                if (!ex.NativeErrorCode.Equals(10035))
                    DisconnectEvent?.Invoke(Id);
            }
        }

        public void SendChannels(IList<DiscordChannel> discordChannels)
        {
            LastActivity = DateTime.Now;

            binaryWriter.Write(true);
            binaryWriter.Write(discordChannels.Count);
            foreach (DiscordChannel discordChannel in discordChannels)
            {
                binaryWriter.Write(discordChannel.Name);
                binaryWriter.Write(discordChannel.Id);
            }
        }

        public void SendAttachments(IReadOnlyList<DiscordMessage> messages, bool isNext)
        {
            LastActivity = DateTime.Now;

            binaryWriter.Write(true);
            binaryWriter.Write(messages.Count);
            foreach (DiscordMessage message in messages)
            {
                binaryWriter.Write(message.Attachments.Count);
                foreach (DiscordAttachment attachment in message.Attachments)
                    binaryWriter.Write(attachment.Url);
            }
            binaryWriter.Write(isNext);
        }

        public void SendAttachments(DiscordMessage message)
        {
            LastActivity = DateTime.Now;

            binaryWriter.Write(true);
            binaryWriter.Write(1);
            binaryWriter.Write(message.Attachments.Count);
            foreach (DiscordAttachment attachment in message.Attachments)
                binaryWriter.Write(attachment.Url);
            binaryWriter.Write(false);
        }

        public void SendError(string error)
        {
            LastActivity = DateTime.Now;

            // Отсылаем флаг, указывающий что произошла ошибка
            binaryWriter.Write(false);
            binaryWriter.Write(error);
        }

        public void Disconnect() => DisconnectEvent?.Invoke(Id);

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

                if (binaryReader != null)
                    binaryReader.Dispose();
                if (binaryWriter != null)
                    binaryWriter.Dispose();
                if (networkStream != null)
                    networkStream.Dispose();
                if (client != null)
                    client.Dispose();

                //BeginSendEvent = null;
                //EndSendEvent = null;
                //MessageEvent = null;
                //DisconnectEvent = null;
            }

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Waiting()
        {
            if (processTask != null)
                processTask.Wait();
        }
    }
}
