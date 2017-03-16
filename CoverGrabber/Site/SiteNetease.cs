using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace CoverGrabber.Site
{
    public class SiteNetease : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "music.163.com"
        };

        public string ConvertAlbumUrl(string originalUrl) => originalUrl.Replace("/#/", "/");

        public bool SupportId3 { get; } = true;
        public bool SupportCover { get; } = true;
        public bool SupportLyric { get; } = true;

        public CookieContainer CookieContainer { get; } = new CookieContainer();

        public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "music.163.com";
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
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//img[@class=\"j-img\"]");
            return coverAddressNode?.GetAttributeValue("data-src", string.Empty) ?? string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            JArray trackNodes = JArray.Parse(HttpUtility.HtmlDecode(pageDocument.DocumentNode.SelectNodes("//div[@id=\"song-list-pre-cache\"]/textarea")[0].InnerText));

            List<string> trackList = trackNodes.Select(x => x["name"].Value<string>()).ToList();
            List<List<string>> discList = new List<List<string>> { trackList };
            return discList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            JArray trackNodes = JArray.Parse(HttpUtility.HtmlDecode(pageDocument.DocumentNode.SelectNodes("//div[@id=\"song-list-pre-cache\"]/textarea")[0].InnerText));

            // The lyric cannot be extracted from song page. Here I just pass in an API call for lyrics instead.
            // I'll think about a better way in the future.
            List<string> trackList = trackNodes.Select(x => "http://music.163.com/api/song/lyric?id=" + x["id"].Value<string>() + "&lv=-1&kv=-1&tv=-1").ToList();
            List<List<string>> discList = new List<List<string>> { trackList };
            return discList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            JArray trackNodes = JArray.Parse(HttpUtility.HtmlDecode(pageDocument.DocumentNode.SelectNodes("//div[@id=\"song-list-pre-cache\"]/textarea")[0].InnerText));
            List<string> trackList = trackNodes.Select(x => JObject.Parse(x["artists"][0].ToString())["name"].Value<string>()).ToList();
            List<List<string>> discList = new List<List<string>> { trackList };
            return discList;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            // As I described in ParseTrackUrlList, here a JSON is passed in.
            JObject lyricObject = JObject.Parse(pageDocument.DocumentNode.InnerHtml);

            string lyric = lyricObject["lrc"]?["lyric"]?.Value<string>() ?? string.Empty;
            return Regex.Replace(lyric, @"\[.+?\]", Environment.NewLine).Trim();
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/div/div/h2");
            return titleNode != null ? HttpUtility.HtmlDecode(titleNode.InnerText) : "";
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[1]/span/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : "";
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[2]");
            if (yearNode != null)
            {
                yearNode.SelectSingleNode("b").Remove();
                return uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(0, 4)));
            }
            else
            {
                return 0;
            }
        }
    }
}
