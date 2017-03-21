using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;

namespace CoverGrabber.Site
{
    public class SiteVgmdb : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "vgmdb.net"
        };

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
            request.Host = "vgmdb.net";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = CookieContainer;
        }

        public AlbumInfo ParseAlbum(string albumUrl)
        {
            HtmlDocument pageDocument = Utility.DownloadPage(albumUrl, this);
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
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//img[@id=\"coverart\"]");
            return coverAddressNode != null ? $"http://vgmdb.net{coverAddressNode.GetAttributeValue("src", string.Empty)}" : string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        private List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                HtmlNodeCollection trackNodes = pageDocument.DocumentNode.SelectNodes($"//div[@id=\"tracklist\"]/span[1]/table[{i}]/tr/td[2]");
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                HtmlNodeCollection trackUrlNodes = pageDocument.DocumentNode.SelectNodes($"//div[@id=\"tracklist\"]/span[1]/table[{i}]/tr/td[2]");
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
            HtmlNodeCollection discNodes = pageDocument.DocumentNode.SelectNodes("//div[@id=\"tracklist\"]/span[1]/table");

            List<List<string>> dictList = new List<List<string>>();
            for (int i = 1; i <= discNodes.Count; i++)
            {
                List<string> trackList = new List<string>();
                HtmlNodeCollection trackArtistNodes = pageDocument.DocumentNode.SelectNodes($"//div[@id=\"tracklist\"]/span[1]/table[{i}]/tr/td[2]");
                for (int j = 0; j < trackArtistNodes.Count; j++)
                {
                    trackList.Add(string.Empty);
                }
                dictList.Add(trackList);
            }
            return dictList;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="trackUrl">Track URL</param>
        /// <returns>Lyric</returns>
        public string ParseTrackLyric(string trackUrl)
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
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"innermain\"]/h1/span[1]");
            return titleNode != null ? HttpUtility.HtmlDecode(titleNode.InnerText) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        private string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//table[@id=\"album_infobit_large\"]/tr[10]/td[2]");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        private uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//table[@id=\"album_infobit_large\"]/tr[2]/td[2]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText.Substring(yearNode.InnerText.Length - 5, 4))) : 0;
        }

    }
}
