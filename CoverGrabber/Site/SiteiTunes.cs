using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace CoverGrabber.Site
{
    public class SiteItunes: ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "itunes.apple.com"
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
            request.Host = "itunes.apple.com";
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
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lockup product album music\"]/a/div[@class=\"artwork\"]/img[@class=\"artwork\"]");

            string coverSize170 = coverAddressNode.GetAttributeValue("src-swap-high-dpi", "");
            string coverSize600 = coverSize170.Replace("170x170", "600x600");
            string coverSize1200 = coverSize170.Replace("170x170", "1200x1200");

            if (CheckIfFileExist(coverSize1200))
            {
                return coverSize1200;
            }
            if (CheckIfFileExist(coverSize600))
            {
                return coverSize600;
            }
            if (CheckIfFileExist(coverSize170))
            {
                return coverSize170;
            }
            return string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        private List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                HtmlNodeCollection trackNodes = pageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[2]/span");
                // Since there may be <, >, etc. so need to decode
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                string tempTrackUrlsXpath = "//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[2]/span";
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes(tempTrackUrlsXpath);
                for (int j = 0; j < trackUrlNodes.Count; j++)
                {
                    trackList.Add(string.Empty);
                }
                dictList.Add(trackList);
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                string tempTrackArtistsXpath = "//div[@class=\"track-list album music\"]/div[@class=\"tracklist-content-box\"]/table/tbody/tr/td[3]/a/span";
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes(tempTrackArtistsXpath);
                List<string> trackList = trackArtistNodes.Select(t => HttpUtility.HtmlDecode(t.InnerText)).ToList();
                dictList.Add(trackList);
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
            return "";
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        private string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"][@class=\"intro\"]/div[1]/h1");
            return titleNode != null ? HttpUtility.HtmlDecode(titleNode.InnerText) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        private string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"][@class=\"intro\"]/div[1]/h2");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        private uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//li[@class=\"release-date\"]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(yearNode.InnerText.Length - 5, 4))) : 0;
        }

        private bool CheckIfFileExist(string url)
        {
            bool ifExist = true;
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.GetResponseStream();
            }
            catch (Exception)
            {
                ifExist = false;
            }
            return ifExist;
        }
    }
}
