using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.IO.Compression;

namespace NetCoreTest.System.Net
{
    [TestClass]
    public class HttpClientTest
    {
        [TestMethod]
        public void Test()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddHttpClient();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var httpclientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var httpClient = httpclientFactory.CreateClient();
            //var jsonContent = JsonContent.Create(new JClass { Test = "AA" });
            //httpClient.DefaultRequestHeaders.Add("content-type", "application/json");
            //var request = new HttpRequestMessage(HttpMethod.Post, "") { Content = jsonContent };
            var result = httpClient.PostAsJsonAsync("http://gtja-test.yiliantech.com:9001/api/token", new { Test = "AA" }).Result;
            var resp = result.Content.ReadFromJsonAsync<ApiResp>().Result;
            Assert.AreEqual(resp.errcode, 1);
        }

        private static MultipartFormDataContent JsonCompress(object data)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
            var stream = new MemoryStream();
            using (var zipper = new GZipStream(stream, CompressionMode.Compress, true))
            {
                zipper.Write(bytes, 0, bytes.Length);
            }
            MultipartFormDataContent multipartContent = new();
            multipartContent.Add(new StreamContent(stream), "gzipContent");
            return multipartContent;
        }

        class ApiResp
        {
            public int errcode { get; set; }

            public string errmsg { get; set; }

            public object data { get; set; }
        }
    }
}
