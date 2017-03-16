using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace CoverGrabber.Site
{
    public class SiteMusicBrainz : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "musicbrainz.org"
        };

        public string ConvertAlbumUrl(string originalUrl) => originalUrl;

        public bool SupportId3 { get; } = true;
        public bool SupportCover { get; } = true;
        public bool SupportLyric { get; } = false;

        public CookieContainer CookieContainer { get; } = new CookieContainer();

        public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "musicbrainz.org";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = CookieContainer;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        public string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"cover-art\"]/img");
            return coverAddressNode != null ? coverAddressNode.GetAttributeValue("src", string.Empty) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            JObject jsonRoot = GetJsonContent(pageDocument);
            JArray discNodes = (JArray)jsonRoot["mediums"];
            List<List<string>> dictList = new List<List<string>>();

            for (int i = 0; i < discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                JArray trackNodesInDisc = (JArray)jsonRoot["mediums"][i]["tracks"];
                for (int j = 0; j < trackNodesInDisc.Count; j++)
                {
                    trackList.Add(jsonRoot["mediums"][i]["tracks"][j]["name"].Value<string>());
                }
                dictList.Add(trackList);
            }
            return dictList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            JObject jsonRoot = GetJsonContent(pageDocument);
            JArray discNodes = (JArray)jsonRoot["mediums"];
            List<List<string>> dictList = new List<List<string>>();

            for (int i = 0; i < discNodes.Count; i++)
            {
                List<string> trackUrlList = new List<string>();
                JArray trackUrlNodesInDisc = (JArray)jsonRoot["mediums"][i]["tracks"];
                for (int j = 0; j < trackUrlNodesInDisc.Count; j++)
                {
                    trackUrlList.Add(jsonRoot["mediums"][i]["tracks"][j]["recording"]["gid"].Value<string>());
                }
                dictList.Add(trackUrlList);
            }
            return dictList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            JObject jsonRoot = GetJsonContent(pageDocument);
            JArray discNodes = (JArray)jsonRoot["mediums"];
            List<List<string>> dictList = new List<List<string>>();

            for (int i = 0; i < discNodes.Count; i++)
            {
                List<string> trackArtistList = new List<string>();

                JArray trackArtistNodesInDisc = (JArray)jsonRoot["mediums"][i]["tracks"];
                for (int j = 0; j < trackArtistNodesInDisc.Count; j++)
                {
                    trackArtistList.Add(jsonRoot["mediums"][i]["tracks"][j]["artistCredit"][0]["name"].Value<string>());
                }
                dictList.Add(trackArtistList);
            }
            return dictList;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            return string.Empty;
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"releaseheader\"]/h1/a/bdi");
            return titleNode?.InnerText ?? string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"releaseheader\"]/p/a/bdi");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//span[@class=\"release-date\"][1]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)) : 0;
        }

        private JObject GetJsonContent(HtmlDocument pageDocument)
        {
            string fullPageContent = pageDocument.DocumentNode.InnerHtml;
            string jsonContent = fullPageContent.Substring(fullPageContent.IndexOf("MB.Release.init(", StringComparison.Ordinal) + "MB.Release.init(".Length);
            jsonContent = jsonContent.Substring(0, jsonContent.IndexOf("</script>", StringComparison.Ordinal)).Trim();
            jsonContent = jsonContent.Substring(0, jsonContent.LastIndexOf(")", StringComparison.Ordinal)).Trim();

            JObject jsonRoot = JObject.Parse(jsonContent);
            return jsonRoot;
        }
    }
}
