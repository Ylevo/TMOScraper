using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScrapper.Utils
{
    public class HtmlParser
    {
        public virtual HtmlNodeCollection ParseScanGroups(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                 //div[1][contains(concat(' ',normalize-space(@class),' '),' text-truncate ')]
                                                                 /span");
        }

        public virtual HtmlNodeCollection ParseChapterImages(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]");
        }

        public virtual SortedDictionary<string, (string, string)[]> ParseChaptersLinks(HtmlDocument doc)
        {
            SortedDictionary<string, (string, string)[]> chapters = new();
            var chaptersNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
            IEnumerable<HtmlNode> uploadedChaptersNodes;
            string chapterNumber, uploadedChapterLink, groupName;

            for (int i = 0; i < chaptersNodes.Count; ++i)
            {
                chapterNumber = ParseAndPadChapterNumber(chaptersNodes[i].Descendants("a").First().InnerText.Substring(9).Trim());

                uploadedChaptersNodes = chaptersNodes[i].Descendants("li");
                (string, string)[] uploadedChapters = new (string, string)[uploadedChaptersNodes.Count()];

                for (int x = 0; x < uploadedChaptersNodes.Count(); ++x)
                {
                    uploadedChapterLink = uploadedChaptersNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                    groupName = String.Join('+', uploadedChaptersNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries));
                    uploadedChapters[x] = (groupName, uploadedChapterLink);
                }

                if (chapters.ContainsKey(chapterNumber))
                {
                    chapters[chapterNumber] = chapters[chapterNumber].Concat(uploadedChapters).ToArray();
                }
                else
                {
                    chapters.Add(chapterNumber, uploadedChapters);
                }
            }

            return chapters;
        }

        public virtual SortedDictionary<string, (string, string)[]> ParseOneShotLinks(HtmlDocument doc)
        {
            string uploadLink, groupName;
            SortedDictionary<string, (string, string)[]> chapter = new();
            var uploadNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");

            (string, string)[] uploadLinks = new (string, string)[uploadNodes.Count];

            for (int x = 0; x < uploadNodes.Count; ++x)
            {
                uploadLink = uploadNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                groupName = String.Join('+', uploadNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries));
                uploadLinks[x] = (groupName, uploadLink);
            }

            chapter.Add("000", uploadLinks);

            return chapter;
        }

        public virtual string ParseAndPadChapterNumber(string chapterNumber)
        {
            chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf('.') + 3);
            string split = chapterNumber.Split('.').Last();

            if (int.Parse(split) > 0)
            {
                if (split.Contains('0'))
                {
                    split = split.Replace("0", "");
                    chapterNumber = (chapterNumber.Remove(chapterNumber.IndexOf(".") + 1) + split).PadLeft(5, '0');
                }
                else
                {
                    chapterNumber = chapterNumber.PadLeft(6, '0');
                }
            }
            else
            {
                chapterNumber = chapterNumber.Substring(0, chapterNumber.IndexOf(".")).PadLeft(3, '0');
            }

            return chapterNumber;
        }

        public virtual string CleanMangoTitle(string filename)
        {
            string title = string.Join(" ", WebUtility.HtmlDecode(filename).Split(Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).ToArray())).Truncate(40).Trim().Replace(' ', '-');

            while (title.Last() == '.')
            {
                title = title.Remove(title.Length - 1, 1);
            }

            return title;
        }
    }

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
}
