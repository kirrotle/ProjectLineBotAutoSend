using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using Project.Dac;
using static Project.Model.StarRailModel;

//解析星瓊鐵道資訊
namespace Project.Service
{
    public class StarRailService
    {
        private IConfigurationSection _section;
        private string _officialInfoLink;
        private string _mainPage;

        public StarRailService(IConfigurationRoot config)
        {
            _section = config.GetSection("StarRail");
            _officialInfoLink = _section.GetSection("OfficialInfoLink")?.Value ?? "";
            _mainPage = _section.GetSection("MainPage")?.Value ?? "";
        }

        /// <summary>
        /// 解析前瞻資訊文章取得兌換碼
        /// </summary>
        public string ParserForwardInfo()
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(_officialInfoLink);

            //取得個文章的標題與網址
            List<ArticleModel> articles = ParserArticle(htmlDoc);

            //將不相關的標題給篩選掉
            articles = FilterArticle(articles);

            //搜索內部文章是否有兌換碼
            

            return "";
        }

        /// <summary>
        /// 取得文章列表內的兌換碼
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private string GetArticleListRedeemCode(List<ArticleModel> models)
        {
            models.ForEach(article => { });
            return "";
        }

        /// <summary>
        /// 取得個文章的標題與網址
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <returns></returns>
        private List<ArticleModel> ParserArticle(HtmlDocument htmlDoc)
        {
            List<ArticleModel> model = new List<ArticleModel>();

            //取得各文章標題
            HtmlNodeCollection titleNodes = htmlDoc.DocumentNode.SelectNodes(".//div[@class='b-list__tile']");
            var _ = titleNodes.Select(titleNode =>
            {
                ArticleModel article = new ArticleModel();

                HtmlNode node = titleNode.SelectSingleNode("./p");
                if (node == null)
                    return false;

                article.Title = node.InnerText;
                article.Url = _mainPage + node.Attributes["href"].Value;

                model.Add(article);
                return true;
            }).ToList();

            return model;
        }

        /// <summary>
        /// 只保留特定字樣的標題與網址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private List<ArticleModel> FilterArticle(List<ArticleModel> model)
        {
            //取得有版本號的(怕之後有.10之類的,所以先設定有兩位也OK)
            Regex reVersion = new Regex("[\\d]{1,2}\\.[\\d]{1,2}");
            //取得有版本名稱的
            Regex reVersionName = new Regex("「.*」");

            return model.Where(article =>
            {
                if (reVersion.IsMatch(article.Title) &&
                reVersionName.IsMatch(article.Title) &&
                IsNextVersion(article.Title))
                    return true;

                return false;
            }).ToList();
        }

        /// <summary>
        /// 判斷是否為下個版本
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool IsNextVersion(string title)
        {
            List<string> possibleVersion = GetNextPossibleVersion();

            foreach(string ver in possibleVersion)
            {
                if (title.Contains(ver))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 取得可能的下個版本名稱
        /// </summary>
        /// <returns></returns>
        private List<string> GetNextPossibleVersion()
        {
            List<string> result = new List<string>();
            (int mainVersion, int subVersion) = GetCurrentVersion();

            //下個版本只有兩種可能
            //一種是((主版本+1).1)
            //另一種是(主版本.(次版本+1))
            result.Add($"{mainVersion + 1}.1");
            result.Add($"{mainVersion}.{subVersion + 1}");

            return result;
        }

        /// <summary>
        /// 取得目前的版本
        /// </summary>
        /// <returns></returns>
        private (int mainVersion, int subVersion) GetCurrentVersion()
        {
            StarRailDac dac = new StarRailDac();
            string currentVersion = dac.GetCurrentVersion();
            string[] verArr = currentVersion.Split(".");

            if (verArr.Length == 2)
                return (int.Parse(verArr[0]), int.Parse(verArr[1]));

            return (100, 100);
        }

    }
}
