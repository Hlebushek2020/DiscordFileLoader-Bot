using System;
using System.Collections.Generic;
using System.Text;

namespace DFL_BotAndServer.Interfaces
{
    public interface IReadOnlyBotClient
    {
        public Guid Id { get; }
        public ulong UserId { get; }
        public bool IsDisposed { get; }
        public DateTime LastActivity { get; }
        public BotClientVersion Version { get; }
    }
}
