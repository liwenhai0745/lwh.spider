using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace lwh.spider
{
   public class ShareData
    {
        /// <summary>
        /// 地址集合
        /// </summary>
        public static BlockingCollection<string> Urls = new BlockingCollection<string>();

        public static BlockingCollection<string> DoingUrls = new BlockingCollection<string>();

        /// <summary>
        /// 图片集合
        /// </summary>
        public static BlockingCollection<string> ImagesUrls = new BlockingCollection<string>();

        public static void AddUrl(string url) {
            if (!Urls.ToArray().Contains(url)) {
                Urls.Add(url);
            }
        }

        public static void AddImgUrl(string url)
        {
            if (!ImagesUrls.ToArray().Contains(url))
            {
                ImagesUrls.Add(url);
            }
        }

        
    }
}
