using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteNetease
    {
        static public void InitializeRequest(ref HttpWebRequest Request, string Url)
        {
            Request.Method = "GET";
            Request.Accept = "Accept: text/html";
            Request.Headers.Set("Accept-Encoding", "deflate");
            Request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            Request.Headers.Set("Cache-Control", "max-age=0");
            Request.Referer = Url;
            Request.Host = "music.163.com";
            Request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            Request.CookieContainer = Utility.cookies;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        static public string ParseCoverAddress(HtmlDocument PageDocument)
        {
            HtmlNode coverAddressNode = PageDocument.DocumentNode.SelectSingleNode("//img[@class=\"j-img\"]");
            if (coverAddressNode != null)
            {
                return (coverAddressNode.GetAttributeValue("data-src", ""));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//tbody[@id=\"m-song-list-module\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTracksXpath = "//tbody[@id=\"m-song-list-module\"]/tr/td[2]/div/div[1]/div/span/b/a";
                HtmlNodeCollection trackNodes = PageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(trackNodes[j].GetAttributeValue("title", "")));
                }
                dictList.Add(trackList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackUrlList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//tbody[@id=\"m-song-list-module\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackUrlList = new List<string>();
                string tempTrackUrlsXpath = "//tbody[@id=\"m-song-list-module\"]/tr/td[2]/div/div[1]/div/span/b/a";
                HtmlNodeCollection trackUrlNodes = PageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
                for (int j = 0; j < trackUrlNodes.Count; j++)
                {
                    string trackUrl = HttpUtility.HtmlDecode(trackUrlNodes[j].GetAttributeValue("href", ""));
                    trackUrlList.Add(trackUrl);
                }
                dictList.Add(trackUrlList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//tbody[@id=\"m-song-list-module\"]");

            List<List<string>> discList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackArtistList = new List<string>();
                string tempTrackArtistsXpath = "//tbody[@id=\"m-song-list-module\"]/tr/td[4]/div/span/a";
                HtmlNodeCollection trackArtistNodes = PageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
                for (int j = 0; j < trackArtistNodes.Count; j++)
                {
                    string trackArtist = HttpUtility.HtmlDecode(trackArtistNodes[j].InnerText);
                    trackArtistList.Add(trackArtist);
                }
                discList.Add(trackArtistList);
            }
            return (discList);
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        static public string ParseTrackLyric(HtmlDocument PageDocument)
        {
            string lyric = "";
            HtmlNode lyricNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"bd bd-open f-ib\"]");

            if (lyricNode != null)
            {
                lyric = lyricNode.InnerText.Trim();
            }
            if (lyric.EndsWith("\n展开"))
            {
                lyric = lyric.Substring(0, lyric.Length - 3);
            }

            //lyricNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"flag_more\"]");

            //if (lyricNode != null)
            //{
            //    lyric += lyricNode.InnerText.Trim();
            //}

            if (lyric == "暂时没有歌词，求歌词" ||
                lyric == "纯音乐，无歌词")
            {
                lyric = "";
            }
            return (HttpUtility.HtmlDecode(lyric));
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument PageDocument)
        {
            HtmlNode titleNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/div/div/h2");
            if (titleNode != null)
            {
                string title = HttpUtility.HtmlDecode(titleNode.InnerText);
                return (title);
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument PageDocument)
        {
            HtmlNode artistNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[1]/span/a");
            if (artistNode != null)
            {
                return (HttpUtility.HtmlDecode(artistNode.InnerText));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument PageDocument)
        {
            HtmlNode yearNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"topblk\"]/p[2]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerHtml.Substring(yearNode.InnerHtml.IndexOf("</b>") + 4, 4))));
            }
            else
            {
                return (0);
            }
        }

    }
}
