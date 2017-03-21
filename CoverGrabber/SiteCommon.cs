using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CoverGrabber
{
    /// <summary>
    /// Some regular actions shared across different sites
    /// </summary>
    public static class SiteCommon
    {
        /// <summary>
        /// Initialize the site related info options
        /// </summary>
        /// <param name="options">The options to update</param>
        /// <param name="supportedSites">The list of available sites to check</param>
        public static void InitializeEnvironment(ref GrabOptions options, Dictionary<string, ISite> supportedSites)
        {
            string host = new Uri(options.WebPageUrl).Host;
            if (supportedSites.ContainsKey(host))
            {
                options.SiteInterface = supportedSites[host];
                options.NeedCover &= supportedSites[host].SupportCover;
                options.NeedLyric &= supportedSites[host].SupportLyric;
                options.NeedId3 &= supportedSites[host].SupportId3;
                return;
            }
            throw new NotImplementedException($"Host {host} is not supported.");
        }


        /// <summary>
        /// Download lyrics 
        /// </summary>
        /// <param name="albumInfo">The album info</param>
        /// <param name="site">The site implementing ISite</param>
        /// <param name="setProgress">Delegate to update progress</param>
        public static void DownloadLyrics(ref AlbumInfo albumInfo, ISite site, DelegateSetProgress setProgress)
        {
            var trackUrlListByDiscs = albumInfo.TrackUrlListByDiscs;
            int remoteTrackQuantity = albumInfo.TrackNamesByDiscs.Sum(x => x.Count);

            setProgress(50, "Getting lyrics...", EnumProgressReportObject.Skip, "");
            List<List<string>> lyricsByDiscs = new List<List<string>>();
            for (int i = 0, currentTrackNumber = 0; i < trackUrlListByDiscs.Count; i++)
            {
                List<string> lyricInDisc = new List<string>();
                for (int j = 0; j < trackUrlListByDiscs[i].Count; j++)
                {
                    try
                    {
                        setProgress(50 + (int)(40.0 * currentTrackNumber / remoteTrackQuantity), $"Downloading track {currentTrackNumber}...",
                            EnumProgressReportObject.Skip, "");

                        string lyric = site.ParseTrackLyric(trackUrlListByDiscs[i][j]);
                        if (!string.IsNullOrWhiteSpace(lyric))
                        {
                            setProgress(-1, null, EnumProgressReportObject.Text,
                                $"{Environment.NewLine}First line of lyric for track {currentTrackNumber}:{Environment.NewLine}{lyric.Split('\n')[0]}");
                        }
                        Thread.Sleep(500);
                        lyricInDisc.Add(lyric);
                        currentTrackNumber++;
                    }
                    catch (Exception)
                    {
                        throw new DownloadLyricException(currentTrackNumber);
                    }
                }
                lyricsByDiscs.Add(lyricInDisc);
            }
            albumInfo.LyricsByDiscs = lyricsByDiscs;
        }


        /// <summary>
        /// Write ID3 tags in a series of files
        /// </summary>
        /// <param name="albumInfo">The album info</param>
        /// <param name="fileList">The list of file path to write</param>
        /// <param name="options">Options (whether need to write info, cover, lyrics)</param>
        /// <param name="setProgress">Delegate to update progress</param>
        static public void WriteFiles(AlbumInfo albumInfo, List<string> fileList, GrabOptions options, DelegateSetProgress setProgress)
        {
            for (int i = 0, currentTrackNumber = 0; i < albumInfo.TrackNamesByDiscs.Count; i++)
            {
                for (int j = 0; j < albumInfo.TrackNamesByDiscs[i].Count; j++)
                {
                    setProgress(90 + (int)(10.0 * currentTrackNumber / fileList.Count), $"Writing track {currentTrackNumber} ...",
                        EnumProgressReportObject.Skip, "");
                    try
                    {
                        Utility.WriteSingleFile(fileList[currentTrackNumber], albumInfo[i, j], options);
                    }
                    catch (Exception)
                    {
                        throw new WritingFileException(currentTrackNumber);
                    }
                    currentTrackNumber++;
                }
            }
        }
    }
}
