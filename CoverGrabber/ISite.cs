using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

namespace CoverGrabber
{
    public interface ISite
    {
        void InitializeRequest(ref HttpWebRequest request, string url);

        AlbumInfo ParseAlbum(HtmlDocument pageDocument);
        string ParseTrackLyric(HtmlDocument pageDocument);
        string ConvertAlbumUrl(string originalUrl);

        List<string> SupportedHost { get; }
        bool SupportId3 { get; }
        bool SupportCover { get; }
        bool SupportLyric { get; }

        CookieContainer CookieContainer { get; }
    }
}
