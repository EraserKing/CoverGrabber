using System;

namespace CoverGrabber
{
    public class FileCountNotMatchException : Exception
    {
        public FileCountNotMatchException(int localCount, int remoteCount)
        {
            throw new Exception($"Local folder contains {localCount} files but remote page contains {remoteCount} tracks");
        }
    }

    public class DownloadCoverException : Exception
    {
        public DownloadCoverException(string pageUrl)
        {
            throw new Exception($"Failed downloading cover {pageUrl}");
        }
    }

    public class DownloadPageException : Exception
    {
        public DownloadPageException(string pageUrl)
        {
            throw new Exception($"Failed downloading page {pageUrl}");
        }
    }

    public class ParsePageException : Exception
    {
        public ParsePageException(string node, string reason)
        {
            throw new Exception($"Failed parsing page on node {node} because {reason}");
        }
    }

    public class FileMatchException : Exception
    {
        public FileMatchException(string folder)
        {
            throw new Exception($"Cannot enumerate files in folder {folder}");
        }
    }

    public class WritingFileException : Exception
    {
        public WritingFileException(int trackNumber)
        {
            throw new Exception($"Failed writing file for track {trackNumber}");
        }
    }

    public class DownloadLyricException : Exception
    {
        public DownloadLyricException(int trackNumber)
        {
            throw new Exception($"Failed download lyric for track {trackNumber}");
        }
    }
}
