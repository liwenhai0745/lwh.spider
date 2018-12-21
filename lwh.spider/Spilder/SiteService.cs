using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lwh.spider.Spilder
{
    public class SiteService
    {

        private Object lockObj = new object();

      

        //private int _EmptyUrlCount { get; set; }
        //private int EmptyUrlCount { get { return _EmptyUrlCount; } set { _EmptyUrlCount = value; if (value >= 10) { cts.Cancel(); } } }

        Dictionary<string, string> DictParas;

        public string URL { get; set; }
        public List<string> Urls { get; set; }

        public string NextUrlSelector { get; set; }


        public string ImgSelector { get; }

        private string SavePath { get; set; }


        public SiteService(string url,string _NextUrlSelector, string _imgSelector,string _savePath="", Dictionary<string, string> _DictParas=null)
        {
            if (string.IsNullOrEmpty(_savePath)) { _savePath = DateTime.Now.ToString("yyyyMMdd"); }
            SavePath = Directory.GetCurrentDirectory() + "\\Images\\" + _savePath;
            if (Directory.Exists(SavePath))
            {
                Directory.Delete(SavePath,true);
            }

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
            
            this.NextUrlSelector = _NextUrlSelector;
            this.ImgSelector = _imgSelector;
            this.URL = url;
            Urls = new List<string>();
            DictParas = _DictParas;
            if (DictParas == null) {
                DictParas = new Dictionary<string, string>();
            }

            DictParas.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36 Core/1.63.6788.400 QQBrowser/10.3.2767.400");

        }

        public  void FindUrls()
        {
            FindUrl(this.URL); ;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            List<Task> tasks = new List<Task>();

            CancellationTokenSource cts = new CancellationTokenSource();

            var taskUrl =  Task.Run(() => {
                while (!cts.IsCancellationRequested)
                {

                    if (cts.IsCancellationRequested)//IsCancellationRequested 被取消
                    {
                        Console.WriteLine("TaskURl线程{0} 被取消", Thread.CurrentThread.ManagedThreadId);
                    }
            
                    if (ShareData.Urls.Count == 0 && ShareData.DoingUrls.Count == 0)
                    {
                        //lock (lockObj)
                        //    EmptyUrlCount ++;
                        //Console.WriteLine("没有数据，休息一下！");
                        //Thread.Sleep(5000);
                        if (ShareData.Urls.Count == 0) {
                            cts.Cancel();
                        }
                    }
                    else {
                        DoUrl();
                        //lock(lockObj)
                        //EmptyUrlCount = 0;
                    }
                }
            },cts.Token);



            for (int i = 0; i < 4; i++)
            {
                var taskImages = Task.Run(() => {
                    while (!cts.IsCancellationRequested)
                    {
                        if (cts.IsCancellationRequested)//IsCancellationRequested 被取消
                        {
                            Console.WriteLine("TaskImages线程{0} 被取消", Thread.CurrentThread.ManagedThreadId);
                        }

                        if (ShareData.ImagesUrls.Count == 0&& ShareData.DoingUrls.Count==0)
                        {
                            //Console.WriteLine("【处理图片】没有数据，休息一下！");
                            //cts.Cancel();
                        }
                        else
                        {
                            DoImage();
                        }
                    }
                }, cts.Token);
                tasks.Add(taskImages);

            }


         

            tasks.Add(taskUrl);
            //Task.WaitAll(tasks.ToArray()); --阻塞线程

            // 将所有任务合成一个 Task 对象，不会阻塞 UI 线程，通过 task.ContinueWith() 获取结果
            Task taskFinal = Task.WhenAll(tasks.ToArray());

            taskFinal.ContinueWith((x) => {
                stopwatch.Stop();

                CreateHTML();
                Console.WriteLine("------------------------------------数据处理完毕["+ stopwatch.Elapsed.TotalSeconds + "s]!------------------------------------");

            });


        }


        private void CreateHTML()
        {
            DirectoryInfo info = new DirectoryInfo(SavePath);
            File.Copy(Directory.GetCurrentDirectory() + "\\my.css" ,SavePath+ "\\my.css");
            StringBuilder Html = new StringBuilder();
            Html.Append("<html>");

            Html.Append("<head >");
            Html.Append("<link rel =\"stylesheet\" id=\"kube-css\" href=\"my.css\" type=\"text/css\" media=\"all\">");
            Html.Append("</head >");


            Html.Append("<body class=\"custom-background\">");
            Html.Append("<div class=\"container\">");
            Html.Append("<div class=\"mainleft\" id=\"content\">");
            Html.Append("<div class=\"article_container row  box\">");
            Html.Append("<div id =\"post_content\">");
            Html.Append("<p>");
            //Html.Append("< br> <img src =\"file://D:/研究性代码/NetCore/Other/lwh.spider/lwh.spider/bin/Debug/netcoreapp2.1/images/TUFEI/144600A07-13.jpg\">");
            foreach (var item in info.GetFiles())
            {
                Html.Append("<br> <img src =\""+item.Name+"\">");
            }
            Html.Append("</div>");
            Html.Append("</div>");
            Html.Append("</div>");
            Html.Append("</div>");
            Html.Append("</body>");
            Html.Append("</html>");

            File.AppendAllTextAsync(SavePath + "//index.html", Html.ToString());
           
        }

        private  async void DoImage()
        {
            var url = "";
            if (ShareData.ImagesUrls.TryTake(out url))
            {
                Console.WriteLine("处理图片URL：" + url);
                using (HttpClient client = new HttpClient(new MyHttpClienHanlder(DictParas)))
                {
                    if (!Directory.Exists(SavePath))
                    {
                        Directory.CreateDirectory(SavePath);
                    }
                    var bytes = await client.GetByteArrayAsync(url);
                    string filename = url.Substring(url.LastIndexOf('/')+1);
                    if(filename.IndexOf('-')>0){
                        filename=filename.Substring(filename.IndexOf('-')+1);
                    }
                    using (FileStream fs= new System.IO.FileStream(SavePath + "//" + filename, System.IO.FileMode.CreateNew))
                    {
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        fs.Close();
                    }
                    //var fs = new System.IO.FileStream(SavePath + "//" + filename, System.IO.FileMode.CreateNew);
                    //fs.Write(bytes, 0, bytes.Length);
                    //fs.Close();
                  
                }
            }
        }

        private  void DoUrl() {
            var url = "";
            if (ShareData.Urls.TryTake(out url))
            {
                Console.WriteLine("处理URL："+url);
                FindUrl(url);
            }
           
        }

      
        private async Task<string> FindUrl(string url)
        {
            ShareData.DoingUrls.Add(url);
            Dictionary<string, string> DictParas = new Dictionary<string, string>();
            DictParas.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.26 Safari/537.36 Core/1.63.6788.400 QQBrowser/10.3.2767.400");
            using (HttpClient client = new HttpClient(new MyHttpClienHanlder(DictParas)))
            {
                var RMessage = await client.GetAsync(url);
                if (RMessage.IsSuccessStatusCode)
                {
                    AddToUrls(RMessage.Content.ReadAsStringAsync().Result);
                }
                else {
                    Console.WriteLine($"URL：{url}请求失败！");
                }
                ShareData.DoingUrls.Take();
                return "ok";

            }
        }

        private void AddToUrls(string content)
        {
            //分析出url
            // Create a new parser front-end (can be re-used)
            var parser = new HtmlParser();
            //Just get the DOM representation
            var document = parser.Parse(content);
            var Urls = document.QuerySelectorAll(NextUrlSelector);
            foreach (IHtmlAnchorElement item in Urls)
            {
                string href = item.PathName;
                if (href == "#")
                {
                    href = URL;
                }
                else
                {
                    if (!href.StartsWith("http"))
                    {
                        href = URL.Substring(0, URL.LastIndexOf('/')) + href;
                    }
                }

                if (!string.IsNullOrEmpty(href))
                    ShareData.Urls.Add(href);
            }
            var Images= document.QuerySelectorAll(ImgSelector);
            foreach (IHtmlImageElement item in Images)
            {
                string url = item.Source;
                if (!string.IsNullOrEmpty(url))
                    ShareData.AddImgUrl(url);
            }
            
        }
    }
}
