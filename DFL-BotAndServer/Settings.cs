using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace DFL_BotAndServer
{
    public class Settings
    {
        [JsonIgnore]
        private static Settings settings = null;

        public int Port { get; set; } = 10000;
        public ulong AdminId { get; set; } = 0;
        public string PublicAddress { get; set; } = "empty";
        public string InternalAddress { get; set; } = "0.0.0.0";
        public string Token { get; set; } = "empty";
        public string ActualAppUrl { get; set; } = "empty";
        public bool WriteLogToFile { get; set; } = true;

        public static bool Availability()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

            if (File.Exists(settingsPath))
                return true;

            using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(new Settings(), Formatting.Indented));

            return false;
        }

        public static Settings GetInstance()
        {
            if (settings == null)
                settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText($"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/settings.json", Encoding.UTF8));
            return settings;
        }
    }
}
