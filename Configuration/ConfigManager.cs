using System;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace KAI_Schedule.Configuration
{
    static class ConfigManager
    {
        public static Config Config { get; set; }
        public static string Path { get; set; } = "config.json";

        public static void Initialize()
        {
            var json = String.Empty;

            if (File.Exists(Path))
            {
                json = File.ReadAllText(Path, new UTF8Encoding(false));
                Config = JsonConvert.DeserializeObject<Config>(json);
            }
            else
            {
                Console.WriteLine("No configuration presented. Generating new default configuration file.");
                json = JsonConvert.SerializeObject(new Config(), Formatting.Indented);
                File.WriteAllText(Path, json, new UTF8Encoding(false));
            }
        }
    }
}
