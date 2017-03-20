using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace CoverGrabber.Site
{
    public class SiteLastFm : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "cn.last.fm"
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
            request.Host = "cn.last.fm";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = CookieContainer;
        }

        public AlbumInfo ParseAlbum(HtmlDocument pageDocument)
        {
            return new AlbumInfo
            {
                AlbumArtistName = ParseAlbumArtist(pageDocument),
                AlbumTitle = ParseAlbumTitle(pageDocument),
                AlbumYear = ParseAlbumYear(pageDocument),
                ArtistNamesByDiscs = ParseTrackArtistList(pageDocument),
                CoverImagePath = ParseCoverAddress(pageDocument),
                TrackNamesByDiscs = ParseTrackList(pageDocument),
                TrackUrlListByDiscs = ParseTrackUrlList(pageDocument)
            };
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        private string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"g album-cover-wrapper \"]/a/img");
            return coverAddressNode?.GetAttributeValue("src", string.Empty) ?? string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        private List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                HtmlNodeCollection trackNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[2]/span");
                List<string> trackList = trackNodes.Select(t => HttpUtility.HtmlDecode(t.InnerText)).ToList();
                dictList.Add(trackList);
            }
            return dictList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        private List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[2]");
                List<string> trackUrlList = trackUrlNodes.Select(t => HttpUtility.HtmlDecode(t.GetAttributeValue("href", string.Empty))).ToList();
                dictList.Add(trackUrlList);
            }
            return dictList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        private List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]");

            List<List<string>> discList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes("//table[@id=\"albumTracklist\"]/tbody/tr/td[3]/a[1]");
                List<string> trackArtistList = trackArtistNodes.Select(t => HttpUtility.HtmlDecode(t.InnerText)).ToList();
                discList.Add(trackArtistList);
            }
            return discList;
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
        private string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"crumb-wrapper\"]/h1");
            return titleNode != null ? HttpUtility.HtmlDecode(titleNode.InnerText.Trim()) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        private string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"crumb-wrapper\"]/div/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText.Trim()) : string.Empty;
        }
        
        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        private uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"g3\"]/dl/dd[2]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(0, 4))) : 0;
        }

    }
}
