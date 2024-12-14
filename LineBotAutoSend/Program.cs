using Microsoft.Extensions.Configuration;
using System.Reflection;
using Project.Service;

namespace LineBotAutoSend
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IConfigurationRoot builder = Builder();
            
            Console.WriteLine("蒐集資訊");
            string Message = "";

            StarRailService starRail = new StarRailService(builder);
            Message += starRail.ParserForwardInfo();

            Console.WriteLine("發送資訊");
            LineBotService lineBot = new LineBotService(builder);
            lineBot.SendMessage(Message);
        }

        public static IConfigurationRoot Builder()
        {
            string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            return new ConfigurationBuilder().
                SetBasePath(basePath).
                AddJsonFile("appsettings.json").
                Build();
        }
    }
}
