using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CoverGrabber.Site
{
    public class SiteXiami : ISite
    {
        public List<string> SupportedHost { get; } = new List<string>
        {
            "www.xiami.com"
        };

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

        public AlbumInfo ParseAlbum(string albumUrl)
        {
            if (!albumUrl.StartsWith("http://www.xiami.com/album/"))
            {
                throw new NotImplementedException();
            }
            string albumId = albumUrl.Replace("http://www.xiami.com/album/", "");
            albumId = albumId.Substring(0, albumId.IndexOf('?'));

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
                JObject page;
                try
                {
                    page = JObject.Parse(Utility.DownloadPage($"http://www.xiami.com/album/songs/id/{albumId}/page/{pageNumber}", this).DocumentNode.InnerHtml);
                }
                catch (Exception)
                {
                    throw new DownloadPageException($"http://www.xiami.com/album/songs/id/{albumId}/page/{pageNumber}");
                }
                // Seems there's a hidden API we can directly call instead of analyzing the page

                // For the page after the last page, data node is null
                // We must break here otherwise it leads to a crash (unexpected page)
                if (!page["data"].HasValues)
                {
                    break;
                }

                if (pageNumber == 1)
                {
                    JToken firstNode = page["data"].First();
                    try
                    {
                        albumInfo.AlbumTitle = firstNode["title"].Value<string>();
                        albumInfo.AlbumArtistName = firstNode["artist_name"].Value<string>();
                        albumInfo.AlbumYear = (uint)Utility.ConvertUnixTimeStampToDateTime(firstNode["demoCreateTime"].Value<long>(), true).Year;
                        albumInfo.CoverImagePath = $"http://pic.xiami.net/{firstNode["album_logo"].Value<string>().Replace("_1.jpg", ".jpg")}";
                    }
                    catch (Exception)
                    {
                        throw new ParsePageException("first track", "at least one property was not found");
                    }
                }

                foreach (var songNode in page["data"])
                {
                    try
                    {
                        int disc = songNode["cdSerial"].Value<int>();
                        if (currentDisc != disc)
                        {
                            albumInfo.ArtistNamesByDiscs.Add(new List<string>());
                            albumInfo.LyricsByDiscs.Add(new List<string>());
                            albumInfo.TrackNamesByDiscs.Add(new List<string>());
                            albumInfo.TrackUrlListByDiscs.Add(new List<string>());
                            currentDisc = disc;
                        }
                    }
                    catch (Exception)
                    {
                        throw new ParsePageException("disc number", "cannot find disc number from track info");
                    }

                    try
                    {
                        albumInfo.ArtistNamesByDiscs[currentDisc - 1].Add(albumInfo.AlbumArtistName == songNode["singers"]?.Value<string>() ? null : songNode["singers"].Value<string>());
                        albumInfo.TrackNamesByDiscs[currentDisc - 1].Add(songNode["songName"].Value<string>());
                        albumInfo.TrackUrlListByDiscs[currentDisc - 1].Add(songNode["lyricInfo"].HasValues ? $"http://www.xiami.com/song/{songNode["songId"].Value<string>()}" : null);
                    }
                    catch (Exception)
                    {
                        throw new ParsePageException($"song {songNode["id"].ToObject<string>()}", "cannot parse track info");
                    }
                }
            }
            return albumInfo;
        }

        /// <summary>
        /// Parse track page and return lyric
        /// </summary>
        /// <param name="trackUrl">Track URL</param>
        /// <returns>Lyric</returns>
        public string ParseTrackLyric(string trackUrl)
        {
            if (trackUrl == null)
            {
                return null;
            }
            HtmlDocument pageDocument = Utility.DownloadPage(trackUrl, this);
            HtmlNode lyricNode = pageDocument.DocumentNode.SelectSingleNode("//div[@class=\"lrc_main\"]");

            return lyricNode != null ? HttpUtility.HtmlDecode(lyricNode.InnerText.Trim()) : null;
        }
    }
}
