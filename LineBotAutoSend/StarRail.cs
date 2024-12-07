using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using HtmlAgilityPack;
using static Project.Model.StarRailModel;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

//解析星瓊鐵道資訊
namespace LineBotAutoSend
{
    public class StarRail
    {
        private IConfigurationSection _section;
        private string _officialInfoLink;
        private string _mainPage;

        public StarRail(IConfigurationRoot config)
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

            return model.Where(artical =>
            {
                if (reVersion.IsMatch(artical.Title) &&
                reVersionName.IsMatch(artical.Title))
                    return true;

                return false;
            }).ToList();
            
        }

    }
}
