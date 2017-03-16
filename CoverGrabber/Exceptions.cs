using System;

namespace CoverGrabber
{
    public class FileCountNotMatchException : Exception
    {
        public int LocalCount { get; }
        public int RemoteCount { get; }
        public FileCountNotMatchException(int localCount, int remoteCount)
        {
            LocalCount = localCount;
            RemoteCount = remoteCount;
        }
    }

    public class DownloadCoverException : Exception
    {
    }

    public class FileMatchException : Exception
    {
    }

    public class WritingFileException : Exception
    {
        public int TrackNumber { get; }
        public WritingFileException(int trackNumber)
        {
            TrackNumber = trackNumber;
        }
    }

    public class DownloadLyricException : Exception
    {
        public int TrackNumber { get; }
        public DownloadLyricException(int trackNumber)
        {
            TrackNumber = trackNumber;
        }
    }
}
