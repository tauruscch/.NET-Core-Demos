using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpClientFactoryDemo.Clients
{
    public class OrderServiceClient
    {
        IHttpClientFactory _httpClientFactory;

        public OrderServiceClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }


        public async Task<string> Get()
        {
            var client = _httpClientFactory.CreateClient();

            // 使用client发起HTTP请求，调用 RemoteSite 项目发布出来的站点
            return await client.GetStringAsync("https://localhost:5003/OrderService");
        }
    }
}
