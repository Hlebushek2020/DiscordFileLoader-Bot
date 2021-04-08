using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DFL_BotAndServer
{
    public struct BotClientVersion
    {
        private const int MinMajor = 1;
        private const int MinMinor = 0;

        public int Major { get; }
        public int Minor { get; }

        public bool CheckCompatibility() => (Major > MinMajor) || (Major == MinMajor && Minor >= MinMinor);

        public BotClientVersion(BinaryReader reader)
        {
            Major = reader.ReadInt32();
            Minor = reader.ReadInt32();
        }
    }
}
