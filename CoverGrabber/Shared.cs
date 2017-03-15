using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;

namespace CoverGrabber
{
    public struct GrabOptions
    {
        public string LocalFolder;
        public string WebPageUrl;
        public bool NeedCover;
        public int ResizeSize;
        public bool NeedId3;
        public bool NeedLyric;
        public string SortMode;
        public Sites Site;
        public List<string> FileList;
    }

    public struct ProgressOptions
    {
        public string StatusMessage;
        public ProgressReportObject ObjectName;
        public string ObjectValue;
    }

    public struct Id3
    {
        public string AlbumTitle;
        public string[] AlbumArtists;
        public string TrackName;
        public string[] TrackArtists;
        public uint Disc;
        public uint DiscCount;
        public uint Track;
        public uint TrackCount;
        public string Lyrics;
        public string[] Performers;
        public uint Year;
        public List<TagLib.Picture> CoverImageList;
    }

    public enum ProgressReportObject
    {
        AlbumTitle,
        AlbumArtist,
        AlbumCover,
        Text,
        TextClear,
        VerifyCode,
        Skip
    }

    public enum Sites
    {
        Null,
        Xiami,
        Netease,
        AmazonJp,
        LastFm,
        VgmDb,
        MusicBrainz,
        ItunesStore
    }
}
