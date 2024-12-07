using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using isRock.LineBot;
using System.Reflection;

namespace LineBotAutoSend
{
    internal class LineBot
    {
        private IConfigurationSection _section;
        private string _channelAccessToken;
        private string _lineUserId;
        private Bot _bot;

        public LineBot(IConfigurationRoot config)
        {
            _section = config.GetSection("LineBot");
            _channelAccessToken = _section.GetSection("LineChannelAccessToken")?.Value?.ToString() ?? "";
            _lineUserId = _section.GetSection("LineUserId")?.Value?.ToString() ?? "";
            _bot = new Bot(_channelAccessToken);
        }

        public bool SendMessage(string message)
        {
            string result = _bot.PushMessage(_lineUserId, message);
            return true;
        }

        public bool TestSendMessage()
        {
            string currentTime = DateTime.Now.ToString("F");
            string message = $"現在時間為{currentTime}\n測試LineBot發送訊息正常";
            _bot.PushMessage(_lineUserId,message);
            return true;
        }
    }
}
