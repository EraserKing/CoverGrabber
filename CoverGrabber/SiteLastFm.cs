using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteLastFm
    {
        static public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "cn.last.fm";
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
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"g album-cover-wrapper \"]/a/img");
            if (coverAddressNode != null)
            {
                return (coverAddressNode.GetAttributeValue("src", ""));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTracksXpath = "//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[2]/span";
                HtmlNodeCollection trackNodes = pageDocument.DocumentNode.SelectNodes(tempTracksXpath);
                for (int j = 0; j < trackNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(trackNodes[j].InnerText));
                }
                dictList.Add(trackList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackUrlList = new List<string>();
                string tempTrackUrlsXpath = "//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[2]";
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
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
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> discList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackArtistList = new List<string>();
                string tempTrackArtistsXpath = "//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[1]";
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
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
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        static public string ParseTrackLyric(HtmlDocument pageDocument)
        {
            return ("");
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"crumb-wrapper\"]/h1");
            if (titleNode != null)
            {
                string title = HttpUtility.HtmlDecode(titleNode.InnerText.Trim());
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
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"crumb-wrapper\"]/div/a");
            if (artistNode != null)
            {
                return (HttpUtility.HtmlDecode(artistNode.InnerText.Trim()));
            }
            else
            {
                return ("");
            }
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"g3\"]/dl/dd[2]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(0, 4))));
            }
            else
            {
                return (0);
            }
        }

    }
}
