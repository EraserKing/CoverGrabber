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
        public string objectName;
        public string objectValue;
    }
}
