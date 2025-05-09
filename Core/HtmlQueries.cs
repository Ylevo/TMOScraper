using HtmlAgilityPack;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace TMOScraper.Core
{
    public static class HtmlQueries
    {
        private static readonly char[] forbiddenPathCharacters = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).Union(['+']).ToArray();

        private static readonly Dictionary<string, string> forbiddenCharPlaceholdersMap = new()
        {
            { "\\", "{backslash}" },
            { "/", "{slash}" },
            { ":", "{colon}" },
            { "*", "{asterisk}" },
            { "?", "{question_mark}" },
            { "\"", "{quote}" },
            { "<", "{less_than}" },
            { ">", "{greater_than}" },
            { "|", "{pipe}" },
        };

        public static List<string>? GetScanGroups(HtmlDocument doc)
        {
            var nodes = doc.DocumentNode.SelectNodes(@"//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]
                                                                 //div[1][contains(concat(' ',normalize-space(@class),' '),' text-truncate ')]
                                                                 /span");
            List<string>? groups = nodes?.Select(n => string.Join('+', n.ParentNode.InnerText.Split(',', StringSplitOptions.TrimEntries).Select(g => RemoveForbbidenCharsFromTitle(g)))).Distinct().ToList();

            return groups;
        }

        public static string GetGroupName(HtmlDocument doc)
        {
            return RemoveForbbidenCharsFromTitle(doc.DocumentNode.SelectSingleNode("//div[@id='app']//h1").InnerText);
        }

        public static string GetMangoTitleFromMangoPage(HtmlDocument doc)
        {
            return RemoveForbbidenCharsFromTitle(doc.DocumentNode.SelectSingleNode("//div[@id='app']//h2").InnerText).Replace(' ', '-');
        }

        public static string GetMangoTitleFromChapterPage(HtmlDocument doc)
        {
            return RemoveForbbidenCharsFromTitle(doc.DocumentNode.SelectSingleNode("//div[@id='app']//h1").InnerText).Replace(' ', '-');
        }

        public static string GetChapterNumberFromChapterPage(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectSingleNode("//div[@id='app']//h4").InnerText.Contains("ONE SHOT") ? "000"
                                : "c" + GetAndPadChapterNumber(doc.DocumentNode.SelectSingleNode("//div[@id='app']//h2").InnerText.Trim());
        }

        public static string GetChapterTitleFromChapterPage(HtmlDocument doc)
        {
            if (doc.DocumentNode.SelectSingleNode("//div[@id='app']//h4").InnerText.Contains("ONE SHOT"))
            {
                return "";
            }

            string groupNameString = string.Join(' ', doc.DocumentNode.SelectSingleNode("//div[@id='app']//h2").Elements("a").Select(d => WebUtility.HtmlDecode(d.InnerText)).ToArray());
            string chapterTitle = Regex.Match(doc.DocumentNode.SelectSingleNode("//title").InnerText, $@"(?<=Capítulo \d+\.\d+ )(.+)(?= - {groupNameString})").Value;

            return ReplaceForbiddenCharsFromChapterTitle(chapterTitle);
        }

        public static string GetGroupNameFromChapterPage(HtmlDocument doc)
        {
            return string.Join('+', doc.DocumentNode.SelectSingleNode("//div[@id='app']//h2").Elements("a").Select(d => RemoveForbbidenCharsFromTitle(d.InnerText)).ToArray());
        }

        public static List<string> GetGroupMangos(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//div[contains(concat(' ',normalize-space(@class),' '),' proyect-item ')]/a")?.Select(m => m.Attributes["href"].Value.Trim()).ToList() ?? [];
        }

        public static List<string> GetChapterImages(HtmlDocument doc)
        {
            return doc.DocumentNode.SelectNodes("//img[contains(concat(' ',normalize-space(@class),' '),' viewer-img ')]")?.Select(img => img.Attributes["data-src"].Value.Trim()).ToList() ?? [];
        }

        public static SortedDictionary<string, (string, string, string)[]> GetChaptersLinks(HtmlDocument doc)
        {
            SortedDictionary<string, (string, string, string)[]> chapters = new();
            var chaptersNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");
            IEnumerable<HtmlNode> uploadedChaptersNodes;
            string chapterh2, chapterNumber, uploadedChapterLink, groupName, chapterTitle;

            for (int i = 0; i < chaptersNodes.Count; ++i)
            {
                chapterh2 = chaptersNodes[i].Descendants("a").First().InnerText.Trim();
                chapterNumber = GetAndPadChapterNumber(chapterh2);
                chapterTitle = ReplaceForbiddenCharsFromChapterTitle(string.Join(' ', [.. chapterh2.Split(' ').Take(Range.StartAt(2))]));

                uploadedChaptersNodes = chaptersNodes[i].Descendants("li");
                (string, string, string)[] uploadedChapters = new (string, string, string)[uploadedChaptersNodes.Count()];

                for (int x = 0; x < uploadedChaptersNodes.Count(); ++x)
                {
                    uploadedChapterLink = uploadedChaptersNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                    groupName = string.Join('+', uploadedChaptersNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries).Select(g => RemoveForbbidenCharsFromTitle(g)).Take(5));
                    uploadedChapters[x] = (groupName, uploadedChapterLink, chapterTitle);
                }

                if (chapters.TryGetValue(chapterNumber, out (string, string, string)[]? value))
                {
                    chapters[chapterNumber] = value.Concat(uploadedChapters).ToArray();
                }
                else
                {
                    chapters.Add(chapterNumber, uploadedChapters);
                }
            }

            return chapters;
        }

        public static SortedDictionary<string, (string, string, string)[]> GetOneShotLinks(HtmlDocument doc)
        {
            string uploadLink, groupName;
            SortedDictionary<string, (string, string, string)[]> chapter = new();
            var uploadNodes = doc.DocumentNode.SelectNodes("//li[contains(concat(' ',normalize-space(@class),' '),' upload-link ')]");

            (string, string, string)[] uploadLinks = new (string, string, string)[uploadNodes.Count];

            for (int x = 0; x < uploadNodes.Count; ++x)
            {
                uploadLink = uploadNodes.ElementAt(x).Descendants("a").Last().Attributes["href"].Value;
                groupName = string.Join('+', uploadNodes.ElementAt(x).Descendants("span").First().InnerText.Split(',', StringSplitOptions.TrimEntries).Select(g => RemoveForbbidenCharsFromTitle(g)));
                uploadLinks[x] = (groupName, uploadLink, "");
            }

            chapter.Add("000", uploadLinks);

            return chapter;
        }
        
        public static string GetAndPadChapterNumber(string chapterh2)
        {
            string chapterNumber = Regex.Match(chapterh2, "(?<=Capítulo )(\\d+.\\d+)").Value;
            var splits = chapterNumber.Split('.');
            return splits[0].PadLeft(3, '0') + (int.Parse(splits[1]) > 0 ? "." + splits[1].Replace("0", "") : "");
        }

        public static string SanitizeInput(string input)
        {
            // Normalize the entire string if we find any surrogate pair unicode character, as we truncate the string later it might cause unicode characters to be split in half
            if (input.Any(c => Char.IsLowSurrogate(c) || Char.IsHighSurrogate(c)))
            {
                var newStringBuilder = new StringBuilder();
                newStringBuilder.Append(input.Normalize(NormalizationForm.FormKD)
                                                .Where(x => x < 128)
                                                .ToArray());
                input = newStringBuilder.ToString();
            }

            return input;
        }

        public static string RemoveForbbidenCharsFromTitle(string input)
        {
            // Split the string using forbidden characters to remove them, join them with a space as delimiter, truncate to 40, trim and remove spaces in excess
            return Regex.Replace(string.Join(" ", WebUtility.HtmlDecode(SanitizeInput(input)).Split(forbiddenPathCharacters)).Truncate(40).Trim(new char[] { ' ', '.' }), @"\s+", " ");
        }

        public static string ReplaceForbiddenCharsFromChapterTitle(string input)
        {
            input = SanitizeInput(WebUtility.HtmlDecode(input));

            foreach (var kvp in forbiddenCharPlaceholdersMap)
            {
                input = input.Replace(kvp.Key.ToString(), kvp.Value);
            }

            return Regex.Replace(input, @"\s+", " ").Trim();
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
