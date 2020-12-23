using System.Threading.Tasks;
using KAI_Schedule.Configuration;

namespace KAI_Schedule
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                ConfigManager.Initialize();
            }
            catch (ApplicationException e)
            {
                Console.WriteLine(e.Message);
                return;
            }
            

            var telegram = new Providers.Telegram();

            await Task.Delay(-1);
        }
    }
}
