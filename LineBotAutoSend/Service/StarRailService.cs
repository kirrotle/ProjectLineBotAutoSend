using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Project.Dac;
using static Project.Model.StarRailModel;
using System.Web;

//解析星瓊鐵道資訊
namespace Project.Service
{
    public class StarRailService
    {
        private IConfigurationSection _section;
        private StarRailDac _dac;
        private string _officialInfoLink;
        private string _mainPage;

        public StarRailService(IConfigurationRoot config)
        {
            _section = config.GetSection("StarRail");
            _officialInfoLink = _section.GetSection("OfficialInfoLink")?.Value ?? "";
            _mainPage = _section.GetSection("MainPage")?.Value ?? "";
            _dac = new StarRailDac();
        }

        /// <summary>
        /// 解析前瞻資訊文章取得兌換碼
        /// </summary>
        public string ParserForwardInfo()
        {
            //取得各個文章的標題與網址
            List<ArticleModel> articles = ParserArticle();

            //將不相關的標題給篩選掉
            articles = FilterArticle(articles);

            //搜索內部文章是否有兌換碼
            List<string> redeemList = GetArticleListRedeemCode(articles);

            //將兌換碼組成訊息
            string message = ProcessMesage(redeemList);

            return message;
        }

        /// <summary>
        /// 取得文章列表內的兌換碼
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        private List<string> GetArticleListRedeemCode(List<ArticleModel> models)
        {
            foreach (ArticleModel article in models)
            {
                List<string> result = ParserRedeemCode(article.Url);
                if (result.Count == 3)
                {
                    _dac.InsertVersion(article);
                    return result;
                }
            }

            return new List<string>();
        }

        private List<string> ParserRedeemCode(string url)
        {
            List<string> redeemCodeList = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            //兌換碼有兩種過濾方式,都找找看
            List<HtmlNode> htmlNodes = new List<HtmlNode>();
            List<string> xpathList = new List<string>() 
            {
                "//font/b/font",
                "//font[@color='#cc99ff']"
            };

            xpathList.ForEach(xpath =>
            {
                HtmlNodeCollection collect = doc.DocumentNode.SelectNodes(xpath);
                if (collect != null)
                {
                    htmlNodes.AddRange(collect);
                }
            });

            htmlNodes.ForEach(node =>
            {
                if (IsRedeemCode(node.InnerText))
                    redeemCodeList.Add(node.InnerText);
            });

            //將重複的兌換碼刪除
            redeemCodeList = redeemCodeList.Distinct().ToList();

            return redeemCodeList;
        }

        /// <summary>
        /// 判斷是否為ReedomCode
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsRedeemCode(string text)
        {
            //兌換碼為大寫英文及數字組字組合而成,共為12碼
            Regex regex = new Regex("^[A-Z1-9]{12}$");

            return regex.IsMatch(text);
        }

        /// <summary>
        /// 取得個文章的標題與網址
        /// </summary>
        /// <param name="htmlDoc"></param>
        /// <returns></returns>
        private List<ArticleModel> ParserArticle()
        {
            List<ArticleModel> model = new List<ArticleModel>();

            //初始化
            HtmlWeb web = new HtmlWeb();
            HtmlDocument htmlDoc = web.Load(_officialInfoLink);

            //取得各文章標題
            HtmlNodeCollection titleNodes = htmlDoc.DocumentNode.SelectNodes(".//div[@class='b-list__tile']");
            var _ = titleNodes.Select(titleNode =>
            {
                ArticleModel article = new ArticleModel();

                HtmlNode node = titleNode.SelectSingleNode("./p");
                if (node == null)
                    return false;

                article.Title = HttpUtility.HtmlDecode(node.InnerText);
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
                IsNextVersion(article))
                    return true;

                return false;
            }).ToList();
        }

        /// <summary>
        /// 判斷是否為下個版本
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private bool IsNextVersion(ArticleModel model)
        {
            List<string> possibleVersion = GetNextPossibleVersion();

            foreach (string ver in possibleVersion)
            {
                if (model.Title.Contains(ver))
                {
                    model.Version = ver;
                    return true;
                }
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
            string currentVersion = _dac.GetCurrentVersion();
            string[] verArr = currentVersion.Split(".");

            if (verArr.Length == 2)
                return (int.Parse(verArr[0]), int.Parse(verArr[1]));

            return (100, 100);
        }

        private string ProcessMesage (List<string> redeemList)
        {
            if (redeemList.Count == 0)
                return "目前沒有擷取到鐵道的兌換碼＞﹏＜,等待下個版本";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("取到鐵道兌換碼了╰(*°▽°*)╯");
            redeemList.ForEach(redeem =>
            {
                builder.AppendLine(redeem);
            });

            return builder.ToString();
        }
    }
}
