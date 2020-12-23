using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Net.Mime;
using Newtonsoft.Json;

namespace KAI_Schedule.Configuration
{
    static class ConfigManager
    {
        public static Config Config { get; set; }
        public static string Path { get; set; } = "config.json";

        public static void Initialize()
        {
            if (File.Exists(Path))
            {
                var json = File.ReadAllText(Path, new UTF8Encoding(false));
                Config = JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                var json = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
                File.WriteAllText(Path, json, new UTF8Encoding(false));
                throw new ApplicationException("No configuration presented. Generating new default configuration file.");
            }
        }
    }
}
