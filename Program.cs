using System;
using System.Threading.Tasks;
using KAI_Schedule.Configuration;
using KAI_Schedule.Providers;

namespace KAI_Schedule
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ConfigManager.Initialize();

            var telegram = new Providers.Telegram();

            await Task.Delay(-1);
        }
    }
}
