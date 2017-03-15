using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteVgmdb
    {
        static public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "vgmdb.net";
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
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//img[@id=\"coverart\"]");
            if (coverAddressNode != null)
            {
                return ("http://vgmdb.net" + coverAddressNode.GetAttributeValue("src", ""));
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTracksXpath = "//div[@id=\"tracklist\"]/span[1]/table[" + i + "]/tr/td[2]";
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackUrlsXpath = "//div[@id=\"tracklist\"]/span[1]/table[" + i + "]/tr/td[2]";
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
                for (int j = 0; j < trackUrlNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(""));
                }
                dictList.Add(trackList);
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackArtistsXpath = "//div[@id=\"tracklist\"]/span[1]/table[" + i + "]/tr/td[2]";
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
                for (int j = 0; j < trackArtistNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(""));
                }
                dictList.Add(trackList);
            }
            return (dictList);
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
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"innermain\"]/h1/span[1]");
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
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        static public string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//table[@id=\"album_infobit_large\"]/tr[10]/td[2]");
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
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        static public uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//table[@id=\"album_infobit_large\"]/tr[2]/td[2]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(yearNode.InnerText.Length - 5, 4))));
            }
            else
            {
                return (0);
            }
        }

    }
}
