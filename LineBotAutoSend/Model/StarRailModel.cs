using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Model
{
    public class StarRailModel
    {
        /// <summary>
        /// 文章
        /// </summary>
        public class ArticleModel
        {
            /// <summary>
            /// 文章標題
            /// </summary>
            public string Title { get; set; } = "";

            /// <summary>
            /// 文章連結網址
            /// </summary>
            public string Url { get; set; } = "";

            /// <summary>
            /// 版本
            /// </summary>
            public string Version { get; set; } = "";

            /// <summary>
            /// 版本資訊
            /// </summary>
            public string VersionInfo { get; set; } = "";


        }
    }
}
