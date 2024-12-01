

namespace LineBotAutoSend
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("發送資訊");
            LineBot lineBot = new LineBot();
            lineBot.TestSendMessage();
        }
    }
}
