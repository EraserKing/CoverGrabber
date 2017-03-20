using System.Collections.Generic;
using TagLib;

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
        public EnumSortMode SortMode;
        public List<string> FileList;
        public ISite SiteInterface;
    }

    public struct ProgressOptions
    {
        public string StatusMessage;
        public EnumProgressReportObject ObjectName;
        public string ObjectValue;
    }

    public class AlbumInfo
    {
        public List<List<string>> TrackNamesByDiscs;
        public List<List<string>> TrackUrlListByDiscs;
        public List<List<string>> ArtistNamesByDiscs;
        public string AlbumTitle;
        public string AlbumArtistName;
        public uint AlbumYear;
        public string CoverImagePath;
        public List<List<string>> LyricsByDiscs;

        public bool NeedCover = false;
        public bool NeedLyric = false;

        public Id3 this[int i, int j] => new Id3
        {
            AlbumTitle = AlbumTitle,
            AlbumArtists = AlbumArtistName.Split(';'),
            TrackName = TrackNamesByDiscs[i][j],
            Disc = (uint)(i + 1),
            DiscCount = (uint)TrackNamesByDiscs.Count,
            Track = (uint)(j + 1),
            TrackCount = (uint)TrackNamesByDiscs[i].Count,
            Performers = (string.IsNullOrEmpty(ArtistNamesByDiscs[i][j]) ? AlbumArtistName : ArtistNamesByDiscs[i][j]).Split(';'),
            Year = AlbumYear,
            CoverImageList = NeedCover ? new List<Picture> { new Picture(CoverImagePath) } : null,
            Lyrics = NeedLyric ? LyricsByDiscs?[i][j] ?? string.Empty : null
        };
    }

    public struct Id3
    {
        public string AlbumTitle;
        public string[] AlbumArtists;
        public string TrackName;
        public uint Disc;
        public uint DiscCount;
        public uint Track;
        public uint TrackCount;
        public string Lyrics;
        public string[] Performers;
        public uint Year;
        public List<Picture> CoverImageList;
    }

    public enum EnumSortMode
    {
        Auto,
        Natural,
        Manual
    }

    public enum EnumProgressReportObject
    {
        AlbumTitle,
        AlbumArtist,
        AlbumCover,
        Text,
        TextClear,
        VerifyCode,
        Skip
    }

    public delegate void DelegateSetProgress(int percentage, string caption, EnumProgressReportObject reportObjecct, string reportContent);
}
