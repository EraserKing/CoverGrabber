using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace CoverGrabber
{
    static class SiteMusicBrainz
    {
        static public void InitializeRequest(ref HttpWebRequest Request, string Url)
        {
            Request.Method = "GET";
            Request.Accept = "Accept: text/html";
            Request.Headers.Set("Accept-Encoding", "deflate");
            Request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            Request.Headers.Set("Cache-Control", "max-age=0");
            Request.Referer = Url;
            Request.Host = "musicbrainz.org";
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
            HtmlNode coverAddressNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"cover-art\"]/img");
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
        /// <param name="PageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        static public List<List<string>> ParseTrackList(HtmlDocument PageDocument)
        {
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//table[@class=\"tbl\"]/tr[@class=\"subh\"]");

            List<List<string>> dictList = new List<List<string>>();

            int currentLine = 1;
            for (int i = 0; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackXpath = "//table[@class=\"tbl\"]/tbody/tr[" + currentLine.ToString() + "]";
                HtmlNode trackNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackXpath);
                // Reach the end
                if (trackNode == null)
                {
                    dictList.Add(trackList);
                    break;
                }
                // New disc - first line
                if (trackNode.GetAttributeValue("class", "") == "subh" && currentLine == 1)
                {
                    currentLine++;
                    continue;
                }
                // New disc - not first line
                if (trackNode.GetAttributeValue("class", "") == "subh" && currentLine != 1)
                {
                    dictList.Add(trackList);
                    currentLine++;
                    continue;
                }
                tempTrackXpath += "/td[2]/span/a/bdi";
                trackNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackXpath);
                trackList.Add(HttpUtility.HtmlDecode(trackNode.InnerText));

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
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//table[@class=\"tbl\"]/tr[@class=\"subh\"]");

            List<List<string>> dictList = new List<List<string>>();

            int currentLine = 1;
            for (int i = 0; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackUrlXpath = "//table[@class=\"tbl\"]/tbody/tr[" + currentLine.ToString() + "]";
                HtmlNode trackUrlNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackUrlXpath);
                // Reach the end
                if (trackUrlNode == null)
                {
                    dictList.Add(trackList);
                    break;
                }
                // New disc - first line
                if (trackUrlNode.GetAttributeValue("class", "") == "subh" && currentLine == 1)
                {
                    currentLine++;
                    continue;
                }
                // New disc - not first line
                if (trackUrlNode.GetAttributeValue("class", "") == "subh" && currentLine != 1)
                {
                    dictList.Add(trackList);
                    currentLine++;
                    continue;
                }
                tempTrackUrlXpath += "/td[2]/span/a";
                trackUrlNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackUrlXpath);
                trackList.Add(HttpUtility.HtmlDecode(trackUrlNode.GetAttributeValue("href", "")));

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
            HtmlNodeCollection discNodes = PageDocument.DocumentNode.SelectNodes("//table[@class=\"tbl\"]/tr[@class=\"subh\"]");

            List<List<string>> dictList = new List<List<string>>();

            int currentLine = 1;
            for (int i = 0; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackArtistXpath = "//table[@class=\"tbl\"]/tbody/tr[" + currentLine.ToString() + "]";
                HtmlNode trackArtistNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackArtistXpath);
                // Reach the end
                if (trackArtistNode == null)
                {
                    dictList.Add(trackList);
                    break;
                }
                // New disc - first line
                if (trackArtistNode.GetAttributeValue("class", "") == "subh" && currentLine == 1)
                {
                    currentLine++;
                    continue;
                }
                // New disc - not first line
                if (trackArtistNode.GetAttributeValue("class", "") == "subh" && currentLine != 1)
                {
                    dictList.Add(trackList);
                    currentLine++;
                    continue;
                }
                tempTrackArtistXpath += "/td[3]/a/bdi";
                trackArtistNode = PageDocument.DocumentNode.SelectSingleNode(tempTrackArtistXpath);
                trackList.Add(HttpUtility.HtmlDecode(trackArtistNode.InnerText));

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
            HtmlNode titleNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"releaseheader\"]/h1/a/bdi");
            if (titleNode != null)
            {
                return (titleNode.InnerText);
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
            HtmlNode artistNode = PageDocument.DocumentNode.SelectSingleNode("//div[@class=\"releaseheader\"]/p/a/bdi");
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
            HtmlNode yearNode = PageDocument.DocumentNode.SelectSingleNode("//span[@class=\"release-date\"][1]");
            if (yearNode != null)
            {
                return (UInt32.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)));
            }
            else
            {
                return (0);
            }
        }
    }
}
