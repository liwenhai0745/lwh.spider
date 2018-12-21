using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace lwh.spider.Spilder
{
    public class MyHttpClienHanlder : HttpClientHandler
    {

        public Dictionary<string, string> DictParas=null;

        public MyHttpClienHanlder( Dictionary<string, string> _DictParas ) {
            this.DictParas = _DictParas;
        }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            foreach (var item in DictParas)
            {
                request.Headers.Add(item.Key, item.Value);

            }
            return base.SendAsync(request, cancellationToken);
        }
    }
}
