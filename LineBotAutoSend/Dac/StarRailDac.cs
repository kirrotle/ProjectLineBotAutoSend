using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project.Model.StarRailModel;

namespace Project.Dac
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

        public void InsertVersion(ArticleModel model)
        {
            string sqlStr = @"
                INSERT INTO [dbo].[STAR_RAIL]
                (
	                [VERSION_ID],
                    [TITLE]
                )
                VALUES
                (
	                @VERSION_ID,
                    @TITLE
                )
            ";

            object paramObj = new
            {
                VERSION_ID = model.Version,
                TITLE = model.Title
            };

            ExecuteCommand(sqlStr, paramObj);
        }
    }
}
