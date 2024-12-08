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
            StarRailService starRail = new StarRailService(builder);
            starRail.ParserForwardInfo();

            //Console.WriteLine("發送資訊");
            //LineBot lineBot = new LineBot(builder);
            //lineBot.TestSendMessage();
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
