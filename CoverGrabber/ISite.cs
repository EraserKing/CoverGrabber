using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace CoverGrabber
{
    public interface ISite
    {
        void InitializeRequest(ref HttpWebRequest request, string url);
        string ParseCoverAddress(HtmlDocument pageDocument);
        List<List<string>> ParseTrackList(HtmlDocument pageDocument);
        List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument);
        List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument);
        string ParseTrackLyric(HtmlDocument pageDocument);
        string ParseAlbumTitle(HtmlDocument pageDocument);
        string ParseAlbumArtist(HtmlDocument pageDocument);
        uint ParseAlbumYear(HtmlDocument pageDocument);

        /// <summary>
        /// Some sites have special URL for album page - not the page user sees in browser
        /// Convert the URL to the fact URL
        /// </summary>
        /// <param name="originalUrl">Original URL</param>
        /// <returns>Converted URL</returns>
        string ConvertAlbumUrl(string originalUrl);

        List<string> SupportedHost { get; }
        bool SupportId3 { get; }
        bool SupportCover { get; }
        bool SupportLyric { get; }

        CookieContainer CookieContainer { get; }
    }
}
