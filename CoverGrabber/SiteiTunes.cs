﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteItunes
    {
        static public void InitializeRequest(ref HttpWebRequest Request, string Url)
        {
            Request.Method = "GET";
            Request.Accept = "Accept: text/html";
            Request.Headers.Set("Accept-Encoding", "deflate");
            Request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            Request.Headers.Set("Cache-Control", "max-age=0");
            Request.Referer = Url;
            Request.Host = "itunes.apple.com";
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
            HtmlNode coverAddressNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lockup product album music\"]/a/div[@class=\"artwork\"]/img[@class=\"artwork\"]");

            string coverSize170 = coverAddressNode.GetAttributeValue("src-swap-high-dpi", "");
            string coverSize600 = coverSize170.Replace("170x170", "600x600");
            string coverSize1200 = coverSize170.Replace("170x170", "1200x1200");

            if (checkIfFileExist(coverSize1200))
            {
                return (coverSize1200);
            }
            else if (checkIfFileExist(coverSize600))
            {
                return (coverSize600);
            }
            else if (checkIfFileExist(coverSize170))
            {
                return (coverSize170);
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
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTracksXpath = "//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[2]/span";
                HtmlNodeCollection trackNodes = PageDocument.DocumentNode.SelectNodes(tempTracksXpath);
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
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackUrlList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackUrlsXpath = "//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[2]/span";
                HtmlNodeCollection trackUrlNodes = PageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
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
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        static public List<List<string>> ParseTrackArtistList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackArtistsXpath = "//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[3]/a/span";
                HtmlNodeCollection trackArtistNodes = PageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
                for (int j = 0; j < trackArtistNodes.Count; j++)
                {
                    // Since there may be <, >, etc. so need to decode
                    trackList.Add(HttpUtility.HtmlDecode(trackArtistNodes[j].InnerText));
                }
                dictList.Add(trackList);
            }
            return (dictList);
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Lyric</returns>
        static public string ParseTrackLyric(HtmlDocument PageDocument)
        {
            return ("");
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Album title</returns>
        static public string ParseAlbumTitle(HtmlDocument PageDocument)
        {
            HtmlNode titleNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"][@class=\"intro\"]/div[1]/h1");
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
            HtmlNode artistNode = PageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"][@class=\"intro\"]/div[1]/h2");
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
            HtmlNode yearNode = PageDocument.DocumentNode.SelectSingleNode("//li[@class=\"release-date\"]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(yearNode.InnerText.Length - 5, 4))));
            }
            else
            {
                return (0);
            }
        }

        static private bool checkIfFileExist(string Url)
        {
            bool ifExist = true;
            HttpWebRequest request;
            HttpWebResponse response;
            Stream responseStream;
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(Url);
                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
            }
            catch (Exception e)
            {
                ifExist = false;
            }
            return (ifExist);
        }
    }
}
