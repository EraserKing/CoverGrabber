using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TagLib;
using File = System.IO.File;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using Tag = TagLib.Id3v2.Tag;

namespace CoverGrabber
{
    public struct VerifyCode
    {
        public string Code;
        public string SessionId;
        public string Apply;
        public string Referer;
        public string LocalVerifyCode;
    };

    static class Utility
    {
        /*
        /// <summary>
        /// The cookies which assists verify code (otherwise verify code / 403 handler doesn't work)
        /// </summary>
        static public readonly CookieContainer Cookies = new CookieContainer();
        */

        /// <summary>
        /// Download a page (UTF-8) and return the page content
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <param name="site">The site implementing ISite</param>
        /// <returns>The page content as string</returns>
        static public HtmlDocument DownloadPage(string url, ISite site)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            site.InitializeRequest(ref request, url);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream objStream = response.GetResponseStream();
            StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);

            site.CookieContainer.Add(response.Cookies);
            string responseText = objReader.ReadToEnd();

            HtmlDocument responsePage = new HtmlDocument();
            responsePage.LoadHtml(responseText);
            return (responsePage);
        }

        /// <summary>
        /// Download a file (stream), store and return whether download succeeds
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <param name="filePath">File path to save</param>
        static public void DownloadFile(string url, string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Stream fileStream = new FileStream(filePath, FileMode.Create);
            byte[] contentBytes = new byte[1024 * 32];
            int remainingSize = responseStream.Read(contentBytes, 0, contentBytes.Length);
            while (remainingSize > 0)
            {
                fileStream.Write(contentBytes, 0, remainingSize);
                remainingSize = responseStream.Read(contentBytes, 0, contentBytes.Length);
            }
            fileStream.Close();
            responseStream.Close();
            response.Close();
            request.Abort();
        }

        /// <summary>
        /// Read an image and resize it (the max size on width/height is specified) with scale kept
        /// </summary>
        /// <param name="sourceFilePath">The file before resize</param>
        /// <param name="destFilePath">The file after resize</param>
        /// <param name="maxSize">The max size on width/height</param>
        /// <returns>Whether resize succeeds</returns>
        private static void ResizeImage(string sourceFilePath, string destFilePath, int maxSize)
        {
            Image largeImage = new Bitmap(sourceFilePath);

            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }

            /* If the new size is smaller than the original size, resize it
             * Otherwise directly copy to create duplication
             * */
            if (largeImage.Width > maxSize && largeImage.Width > maxSize)
            {
                int newHeight;
                int newWidth;
                if (largeImage.Height > largeImage.Width)
                {
                    newHeight = maxSize;
                    newWidth = (int)(maxSize * (largeImage.Width / (double)largeImage.Height));
                }
                else if (largeImage.Height < largeImage.Width)
                {
                    newHeight = (int)(maxSize * (largeImage.Height / (double)largeImage.Width));
                    newWidth = maxSize;
                }
                else
                {
                    newHeight = maxSize;
                    newWidth = maxSize;
                }

                Image templateImage = new Bitmap(newWidth, newHeight);
                Graphics templateGraphics = Graphics.FromImage(templateImage);

                templateGraphics.InterpolationMode = InterpolationMode.High;
                templateGraphics.SmoothingMode = SmoothingMode.HighQuality;
                templateGraphics.Clear(Color.White);
                templateGraphics.DrawImage(largeImage, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, largeImage.Width, largeImage.Height), GraphicsUnit.Pixel);
                templateImage.Save(destFilePath, ImageFormat.Jpeg);

                templateGraphics.Dispose();
                templateImage.Dispose();
            }
            else
            {
                File.Copy(sourceFilePath, destFilePath);
            }
            largeImage.Dispose();
        }

        /// <summary>
        /// Download, save and resize cover image
        /// </summary>
        /// <param name="remoteCoverUrl"></param>
        /// <param name="resizeSize">The maximum size of width/height after resize (0 for skipping resizing)</param>
        /// <returns>The file path after resizing</returns>
        static public string GenerateCover(string remoteCoverUrl, int resizeSize)
        {
            try
            {
                string originalImagePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remoteCoverUrl) + ".jpg");
                string resizedImagePath = Path.Combine(Path.GetTempPath(), Path.GetFileName(remoteCoverUrl) + "_resized.jpg");
                if (File.Exists(originalImagePath))
                {
                    File.Delete(originalImagePath);
                }
                if (File.Exists(resizedImagePath))
                {
                    File.Delete(resizedImagePath);
                }

                DownloadFile(remoteCoverUrl, originalImagePath);
                if (resizeSize != 0)
                {
                    ResizeImage(originalImagePath, resizedImagePath, resizeSize);
                }
                File.Delete(originalImagePath);
                return resizedImagePath;
            }
            catch (Exception)
            {
                throw new DownloadCoverException();
            }
        }

        /// <summary>
        /// Write ID3 tags for a file
        /// </summary>
        /// <param name="filePath">The file path to write</param>
        /// <param name="id3">ID3 tags</param>
        /// <param name="options">Options (whether need to write info, cover, lyrics)</param>
        static private void WriteSingleFile(string filePath, Id3 id3, GrabOptions options)
        {
            TagLib.File trackFile = TagLib.File.Create(filePath);

            trackFile.RemoveTags(TagTypes.Id3v1);
            trackFile.RemoveTags(TagTypes.Ape);
            Tag.DefaultVersion = 3;
            Tag.ForceDefaultVersion = true;

            if (options.NeedId3)
            {
                trackFile.Tag.Album = id3.AlbumTitle;
                trackFile.Tag.AlbumArtists = id3.AlbumArtists;
                trackFile.Tag.Title = id3.TrackName;
                trackFile.Tag.Performers = id3.Performers;
                trackFile.Tag.Year = id3.Year;
                trackFile.Tag.Disc = id3.Disc;
                trackFile.Tag.DiscCount = id3.DiscCount;
                trackFile.Tag.Track = id3.Track;
                trackFile.Tag.TrackCount = id3.TrackCount;
            }

            if (options.NeedCover)
            {
                trackFile.Tag.Pictures = id3.CoverImageList.ToArray();
            }

            if (options.NeedLyric && !string.IsNullOrEmpty(id3.Lyrics))
            {
                trackFile.Tag.Lyrics = id3.Lyrics;
            }

            trackFile.Save();
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
                        WriteSingleFile(fileList[currentTrackNumber], albumInfo[i, j], options);
                    }
                    catch (Exception)
                    {
                        throw new WritingFileException(currentTrackNumber);
                    }
                    currentTrackNumber++;
                }
            }
        }

        /// <summary>
        /// Parse ID3 info from the album page
        /// </summary>
        /// <param name="site">The site implementing ISite</param>
        /// <param name="albumPage">The album page</param>
        /// <returns>The album info</returns>
        public static AlbumInfo ParseId3(ISite site, HtmlDocument albumPage)
        {
            AlbumInfo albumInfo = new AlbumInfo
            {
                TrackNamesByDiscs = site.ParseTrackList(albumPage),
                TrackUrlListByDiscs = site.ParseTrackUrlList(albumPage),
                ArtistNamesByDiscs = site.ParseTrackArtistList(albumPage),
                AlbumTitle = site.ParseAlbumTitle(albumPage),
                AlbumArtistName = site.ParseAlbumArtist(albumPage),
                AlbumYear = site.ParseAlbumYear(albumPage)
            };
            return albumInfo;
        }

        /// <summary>
        /// Download lyrics 
        /// </summary>
        /// <param name="albumInfo">The album info</param>
        /// <param name="site">The site implementing ISite</param>
        /// <param name="setProgress">Delegate to update progress</param>
        static public void DownloadLyrics(ref AlbumInfo albumInfo, ISite site, DelegateSetProgress setProgress)
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
                        setProgress(50 + (int)(40.0 * currentTrackNumber / remoteTrackQuantity), $"Downloading track {currentTrackNumber}...", EnumProgressReportObject.Skip, "");

                        string lyric = site.ParseTrackLyric(DownloadPage(trackUrlListByDiscs[i][j], site));
                        if (!string.IsNullOrWhiteSpace(lyric))
                        {
                            setProgress(-1, null, EnumProgressReportObject.Text, $"{Environment.NewLine}First line of lyric for track {currentTrackNumber}:{Environment.NewLine}{lyric.Split('\n')[0]}");
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
        /// Sort file in auto sort mode
        /// </summary>
        /// <param name="localFileList">The list of local files</param>
        /// <param name="remoteTracksByDiscs">The list of remote files, two-layers</param>
        /// <param name="mismatchedFiles">The files which cannot be mismatched</param>
        /// <param name="localToRemoteMap">The file in the disc - track</param>
        /// <returns>The sorted file list</returns>
        private static List<string> AutoSortFile(List<string> localFileList, List<List<string>> remoteTracksByDiscs, out List<string> mismatchedFiles, out Dictionary<string, Tuple<int, int>> localToRemoteMap)
        {
            List<string> sortResult = new List<string>(localFileList.Count);
            // Generate a list in which files are sorted naturally.
            mismatchedFiles = new List<string>(localFileList.ToArray());

            localToRemoteMap = new Dictionary<string, Tuple<int, int>>();

            // A bit tricky: arrange from the last one, to make sure tracks "A" and "A(z)" are matched first for "A(z)" then "A". 
            for (int i = remoteTracksByDiscs.Count - 1; i >= 0; i--)
            {
                for (int j = remoteTracksByDiscs[i].Count - 1; j >= 0; j--)
                {
                    string remoteTrackName = remoteTracksByDiscs[i][j];

                    // For each remote track, search all local files
                    for (int k = mismatchedFiles.Count - 1; k >= 0; k--)
                    {
                        // If we're lucky enough to get such a local file
                        if (Path.GetFileNameWithoutExtension(mismatchedFiles[k]).IndexOf(remoteTrackName, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            localToRemoteMap.Add(mismatchedFiles[k], new Tuple<int, int>(i, j));
                            sortResult.Add(mismatchedFiles[k]);
                            mismatchedFiles.RemoveAt(k);
                            break;
                        }
                    }
                }
            }
            return sortResult;
        }

        /// <summary>
        /// Try to match local files to remote trackes
        /// </summary>
        /// <param name="fileList">Local files</param>
        /// <param name="trackNamesByDiscs">Remote tracks (disc - track)</param>
        /// <returns>If all of local files can be matched well</returns>
        static public bool TryToMatchFiles(ref List<string> fileList, List<List<string>> trackNamesByDiscs)
        {
            List<string> mismatchedFiles;
            Dictionary<string, Tuple<int, int>> localToRemoteMap;

            var sortedFileList = AutoSortFile(fileList, trackNamesByDiscs, out mismatchedFiles, out localToRemoteMap);
            string promptMessage = $"The auto sort result is:{Environment.NewLine}";
            promptMessage += string.Join(Environment.NewLine, localToRemoteMap.Keys.OrderBy(x => x).Select(x => string.Concat(x, "=>", trackNamesByDiscs[localToRemoteMap[x].Item1][localToRemoteMap[x].Item2])));

            if (mismatchedFiles.Count == 0)
            {
                promptMessage += $"{Environment.NewLine}To accept this, press Yes. To continue to the naturally sort, press No. To cancel, press Cancel.";
                DialogResult dr = MessageBox.Show(promptMessage, "Auto sort result", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dr == DialogResult.Yes)
                {
                    fileList = sortedFileList;

                }
                else if (dr == DialogResult.Cancel)
                {
                    return false;
                }
            }
            else
            {
                promptMessage += $"{Environment.NewLine}These files do not have matched tracks:{Environment.NewLine}";
                promptMessage += string.Join(Environment.NewLine, mismatchedFiles);
                promptMessage += $"{Environment.NewLine}You cannot continue in Auto mode, but you can sort them manually.";
                MessageBox.Show(promptMessage, "Auto sort result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Generate a list of files to update
        /// </summary>
        /// <param name="folders">The folders containing files</param>
        /// <returns>The list of files inside</returns>
        public static List<string> GenerateFileList(string folders)
        {
            List<string> fileList = new List<string>();

            foreach (string folderPath in folders.Split(';'))
            {
                fileList.AddRange(new DirectoryInfo(folderPath).GetFiles("*.m4a").Select(y => y.FullName));
                fileList.AddRange(new DirectoryInfo(folderPath).GetFiles("*.mp3").Select(y => y.FullName));
            }
            return fileList;
        }

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
                options.NeedCover = supportedSites[host].SupportCover;
                options.NeedLyric = supportedSites[host].SupportLyric;
                options.NeedId3 = supportedSites[host].SupportId3;
                return;
            }
            throw new NotImplementedException($"Host {host} is not supported.");
        }
    }
}
