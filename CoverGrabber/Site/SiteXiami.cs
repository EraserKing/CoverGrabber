using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace CoverGrabber.Site
{
    public class SiteXiami : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "www.xiami.com"
        };

        public string ConvertAlbumUrl(string originalUrl) => originalUrl;

        public bool SupportId3 { get; } = true;
        public bool SupportCover { get; } = true;
        public bool SupportLyric { get; } = true;

        public CookieContainer CookieContainer { get; } = new CookieContainer();

        public void InitializeRequest(ref HttpWebRequest request, string url)
        {
            request.Method = "GET";
            request.Accept = "Accept: text/html";
            request.Headers.Set("Accept-Encoding", "deflate");
            request.Headers.Set("Accept-Language", "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
            request.Headers.Set("Cache-Control", "max-age=0");
            request.Referer = url;
            request.Host = "www.xiami.com";
            request.UserAgent = "User-Agent: Mozilla/5.0 (Windows NT 6.3; WOW64; rv:31.0) Gecko/20100101 Firefox/31.0";
            request.CookieContainer = CookieContainer;
        }

        public AlbumInfo ParseAlbum(HtmlDocument pageDocument)
        {
            string albumId = pageDocument.DocumentNode.SelectSingleNode("//link[@rel=\"canonical\"]").GetAttributeValue("href", "").Replace("http://www.xiami.com/album/", "");
            AlbumInfo albumInfo = new AlbumInfo
            {
                ArtistNamesByDiscs = new List<List<string>>(),
                LyricsByDiscs = new List<List<string>>(),
                TrackNamesByDiscs = new List<List<string>>(),
                TrackUrlListByDiscs = new List<List<string>>(),
            };
            int currentDisc = 0;

            for (int pageNumber = 1; ; pageNumber++)
            {
                // Seems there's a hidden API we can directly call instead of analyzing the page
                JObject page = JObject.Parse(Utility.DownloadPage($"http://www.xiami.com/album/songs/id/{albumId}/page/{pageNumber}", this).DocumentNode.InnerHtml);
                // For the page after the last page, data node is null
                // We must break here otherwise it leads to a crash (unexpected page)
                if (!page["data"].HasValues)
                {
                    break;
                }

                if (pageNumber == 1)
                {
                    JToken firstNode = page["data"].First();
                    albumInfo.AlbumTitle = firstNode["title"].Value<string>();
                    albumInfo.AlbumArtistName = firstNode["artist_name"].Value<string>();
                    albumInfo.AlbumYear = ParseAlbumYear(pageDocument);
                    albumInfo.CoverImagePath = "http://pic.xiami.net/" + firstNode["album_logo"].Value<string>().Replace("_1.jpg", ".jpg");
                }

                foreach (JObject songNode in page["data"])
                {
                    string songUrl = $"http://www.xiami.com/song/{songNode["songId"].Value<string>()}";
                    int disc = songNode["cdSerial"].Value<int>();
                    if (currentDisc != disc)
                    {
                        albumInfo.ArtistNamesByDiscs.Add(new List<string>());
                        albumInfo.LyricsByDiscs.Add(new List<string>());
                        albumInfo.TrackNamesByDiscs.Add(new List<string>());
                        albumInfo.TrackUrlListByDiscs.Add(new List<string>());
                        currentDisc = disc;
                    }

                    albumInfo.ArtistNamesByDiscs[currentDisc - 1].Add(albumInfo.AlbumArtistName == songNode["singers"]?.Value<string>() ? null : songNode["singer"].Value<string>());
                    albumInfo.TrackNamesByDiscs[currentDisc - 1].Add(songNode["songName"].Value<string>());
                    // TODO: If we have lyric node, add the page; otherwise skip to prevent blocked
                    albumInfo.TrackUrlListByDiscs[currentDisc - 1].Add(songUrl);
                }
            }
            return albumInfo;
        }

        /// <summary>
        /// Parse album page and get cover image URL
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Cover image URL</returns>
        private string ParseCoverAddress(HtmlDocument pageDocument)
        {
            HtmlNode coverAddressNode = pageDocument.DocumentNode.SelectSingleNode("//a[@id=\"cover_lightbox\"]");
            return coverAddressNode?.GetAttributeValue("href", string.Empty) ?? string.Empty;
        }

        /// <summary>
        /// Parse album page and return tracks list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks list per disc</returns>
        private List<List<string>> ParseTrackList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectNodes("td[@class=\"song_name\"]/a").First();
                    discList[currentDiscIndex].Add(HttpUtility.HtmlDecode(trackNode.InnerText));
                }
            }
            return discList;
        }

        /// <summary>
        /// Parge album page and return track URLs list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        private List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectNodes("td[@class=\"song_name\"]/a").First();
                    discList[currentDiscIndex].Add("http://www.xiami.com" + HttpUtility.HtmlDecode(trackNode.GetAttributeValue("href", "")));
                }
            }
            return discList;
        }

        /// <summary>
        /// Parse album page and return track artists list
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Two level ArrayList, discs list - tracks URLs list per disc</returns>
        private List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument)
        {
            List<List<string>> discList = new List<List<string>>();
            for (int i = 0; i < pageDocument.DocumentNode.SelectNodes("//strong[@class=\"trackname\"]").Count; i++)
            {
                discList.Add(new List<string>());
            }
            int currentDiscIndex = -1;
            foreach (HtmlNode row in pageDocument.DocumentNode.SelectNodes("//table[@class=\"track_list\"]/tbody/tr"))
            {
                if (row.Attributes["data-json"] == null)
                {
                    currentDiscIndex++;
                }
                else
                {
                    HtmlNode trackNode = row.SelectSingleNode("td[@class=\"song_name\"]");
                    foreach (HtmlNode node in row.SelectNodes("td[@class=\"song_name\"]/a"))
                    {
                        node.Remove();
                    }
                    discList[currentDiscIndex].Add(HttpUtility.HtmlDecode(trackNode.InnerText.Trim()));
                }
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
            HtmlNode lyricNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            return lyricNode != null ? HttpUtility.HtmlDecode(lyricNode.InnerText.Trim()) : null;
        }

        /// <summary>
        /// Parse album page and return title
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album title</returns>
        private string ParseAlbumTitle(HtmlDocument pageDocument)
        {
            HtmlNode titleNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"title\"]/h1");
            if (titleNode != null)
            {
                titleNode.SelectSingleNode("span")?.Remove();
                return titleNode.InnerText;
            }
            return string.Empty;
        }

        /// <summary>
        /// Parse album page and return artist
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album artist</returns>
        private string ParseAlbumArtist(HtmlDocument pageDocument)
        {
            HtmlNode artistNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[1]/td[2]/a");
            return artistNode != null ? HttpUtility.HtmlDecode(artistNode.InnerText.Trim()) : string.Empty;
        }

        /// <summary>
        /// Parse album page and return year
        /// </summary>
        /// <param name="pageDocument">Page as document</param>
        /// <returns>Album year</returns>
        private uint ParseAlbumYear(HtmlDocument pageDocument)
        {
            HtmlNode yearNode = pageDocument.DocumentNode.SelectSingleNode("//div[@id=\"album_info\"]/table/tr[4]/td[2]");
            return yearNode != null ? uint.Parse(HttpUtility.HtmlDecode(yearNode.InnerText).Substring(0, 4)) : 0;
        }
    }
}
