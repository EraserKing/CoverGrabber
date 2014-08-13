using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoverGrabber
{
    public struct GrabOptions
    {
        public string localFolder;
        public string webPageUrl;
        public bool needCover;
        public int resizeSize;
        public bool needId3;
        public bool needLyric;
    }

    public struct ProgressOptions
    {
        public string statusMessage;
        public ProgressReportObject objectName;
        public string objectValue;
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
}
