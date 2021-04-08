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

        public string Host { get; set; } = string.Empty;
        public string ImageCollectionEditor { get; set; } = string.Empty;
        public int Port { get; set; }
        public ulong UserId { get; set; }
        public ulong DiscordServerId { get; set; }
        public Dictionary<string, ulong> ChannelIds { get; } = new Dictionary<string, ulong>();

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
