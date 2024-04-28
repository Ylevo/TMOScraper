using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TMOScrapper.Utils
{
    public static class Downloader
    {
        private static readonly HttpClient client = new();

        public static async Task DownloadChapter(string folderPath, HtmlNodeCollection imgNodes, CancellationToken token)
        {
            string url, filename;
            var tasks = new List<Task>();

            for (int i = 0; i < imgNodes.Count; i++)
            {
                url = imgNodes[i].Attributes["data-src"].Value;
                filename = $"{i:D3}." + url.Split('.').Last();
                tasks.Add(DownloadFile(new Uri(url), Path.Combine(folderPath, filename), filename, token));
            }

            await Task.WhenAll(tasks);
        }
        private static async Task DownloadFile(Uri uri, string path, string filename, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            using var s = await client.GetStreamAsync(uri, token);
            using var fs = new FileStream(path, FileMode.CreateNew);
            //AddLog("Downloading file " + filename + " ...");
            await s.CopyToAsync(fs, token);
        }
    }
}
