using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScrapper.Core.PageFetcher
{
    internal class HtmlAgiPageFetcher : IPageFetcher
    {
        private HtmlDocument document = new();
        private readonly HtmlWeb webClient = new();

        public HtmlAgiPageFetcher() 
        {
        }

        public async Task<string> GetPage(string url, CancellationToken token, PageType page = PageType.Default)
        {
            switch(page) 
            {
                case PageType.Default:
                    return await GetDefault(url, token);
                case PageType.Chapter:
                    return await GetChapter(url, token);
                default:
                    return "";
            }
        }

        private async Task<string> GetDefault(string url, CancellationToken token)
        {
            int counter = 1;
            bool goodToGo = false;

            while (!goodToGo)
            {
                token.ThrowIfCancellationRequested();
                document = await webClient.LoadFromWebAsync(url, token);

                switch (webClient.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new HttpRequestException("Error: 404 not found. Check your URL.", null, HttpStatusCode.NotFound);
                    case HttpStatusCode.TooManyRequests:
                        //AddLog($"Ratelimit hit. Waiting around 3-5 seconds ... ({counter++})");
                        //await Task.Delay(random.Next(3000, 5000));
                        break;
                    case HttpStatusCode.OK:
                        goodToGo = true;
                        break;
                    default:
                        //AddLog("Loading page failed. Status code : " + webClient.StatusCode);
                        //AddLog("Retrying ...");
                        //await Task.Delay(random.Next(1000, 1500));
                        break;
                }
            }
            return document.ParsedText;
        }

        private async Task<string> GetChapter(string url, CancellationToken token)
        {
           
        }
    }
}
