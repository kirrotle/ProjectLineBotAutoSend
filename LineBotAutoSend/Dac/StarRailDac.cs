using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineBotAutoSend.Dac
{
    public class StarRailDac : _Dac
    {
        public StarRailDac() : base()
        {
        }

        public string GetCurrentVersion()
        {
            string sqlStr = @"
                SELECT MAX(CONVERT(DECIMAL(3,1),VERSION_ID)) 
                FROM STAR_RAIL
            ";

            return ExecuteQuery<string>(sqlStr).FirstOrDefault();
        }
    }
}
