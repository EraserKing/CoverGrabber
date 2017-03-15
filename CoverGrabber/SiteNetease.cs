using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json.Linq;

namespace CoverGrabber
{
    static class SiteNetease
    {
        static public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "music.163.com";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = Utility.Cookies;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        static public string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//img[@class=\"j-img\"]");
            return coverAddressNode != null ? coverAddressNode.GetAttributeValue("data-src", "") : "";
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
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
        static public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            JArray trackNodes = JArray.Parse(HttpUtility.HtmlDecode(pageDocument.DocumentNode.SelectNodes("//div[@id=\"song-list-pre-cache\"]/textarea")[0].InnerText));

            List<string> trackList = trackNodes.Select(x => "http://music.163.com/song?id=" + x["id"].Value<string>()).ToList();
            List<List<string>> discList = new List<List<string>> { trackList };
            return discList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
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
        static public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"lyric-content\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            if (lyric.EndsWith("\n展开"))
            {
                lyric = lyric.Substring(0, lyric.Length - 3);
            }

            if (lyric == "暂时没有歌词，求歌词" ||
                lyric == "纯音乐，无歌词")
            {
                lyric = "";
            }
            return HttpUtility.HtmlDecode(lyric);
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/div/div/h2");
            return titleNode != null ? HttpUtility.HtmlDecode(titleNode.InnerText) : "";
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[1]/span/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : "";
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[2]");
            yearNode.SelectSingleNode("b").Remove();
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(0, 4))) : 0;
        }
    }
}
