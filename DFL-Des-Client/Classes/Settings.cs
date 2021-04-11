using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DFL_Des_Client.Classes
{
    public class Settings
    {
        [JsonIgnore]
        public static string ProgramResourceFolder { get; } = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\SergeyGovorunov\\YukoClient(DFLC)";

        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 10000;
        public ulong UserId { get; set; }
        public ulong DiscordServerId { get; set; }
        public string ImageCollectionExe { get; set; } = string.Empty;
        public int MaxDownloadThreads { get; set; } = 4; 

        public Dictionary<ulong, string> ChannelIds { get; } = new Dictionary<ulong, string>();

        public void Save()
        {
            Directory.CreateDirectory(ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter($"{ProgramResourceFolder}\\settings.json", false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static Settings GetInstance()
        {
            string settingsFile = $"{ProgramResourceFolder}\\settings.json";
            if (File.Exists(settingsFile))
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFile, Encoding.UTF8));
            return new Settings();
        }
    }
}
