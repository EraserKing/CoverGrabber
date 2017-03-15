using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;

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
        public delegate string ParseCoverAddress(HtmlDocument pageDocument);
        public delegate List<List<string>> ParseTrackList(HtmlDocument pageDocument);
        public delegate List<List<string>> ParseTrackUrlList(HtmlDocument pageDocument);
        public delegate List<List<string>> ParseTrackArtistList(HtmlDocument pageDocument);
        public delegate string ParseTrackLyric(HtmlDocument pageDocument);
        public delegate string ParseAlbumTitle(HtmlDocument pageDocument);
        public delegate string ParseAlbumArtist(HtmlDocument pageDocument);
        public delegate uint ParseAlbumYear(HtmlDocument pageDocument);

        /// <summary>
        /// The cookies which assists verify code (otherwise verify code / 403 handler doesn't work)
        /// </summary>
        static public CookieContainer Cookies = new CookieContainer();

        /// <summary>
        /// Download a page (UTF-8) and return the page content
        /// </summary>
        /// <param name="url">URL to download</param>
        /// <param name="site">Site to download from</param>
        /// <returns>The page content as string</returns>
        static public HtmlDocument DownloadPage(string url, Sites site)
        {
        Start:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            bool alreadyHandledXiami403 = false;
            switch (site)
            {
                case (Sites.Xiami):
                    {
                        SiteXiami.InitializeRequest(ref request, url);
                        break;
                    }
                case (Sites.Netease):
                    {
                        SiteNetease.InitializeRequest(ref request, url);
                        break;
                    }
                case (Sites.ItunesStore):
                    {
                        SiteItunes.InitializeRequest(ref request, url);
                        break;
                    }
                case (Sites.MusicBrainz):
                    {
                        SiteMusicBrainz.InitializeRequest(ref request, url);
                        break;
                    }
                case (Sites.VgmDb):
                    {
                        SiteVgmdb.InitializeRequest(ref request, url);
                        break;
                    }
                case (Sites.LastFm):
                    {
                        SiteLastFm.InitializeRequest(ref request, url);
                        break;
                    }
            }

            string responseText = "";
            try
            {
                Stream objStream;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                objStream = response.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream, Encoding.UTF8, true);

                Cookies.Add(response.Cookies);
                responseText = objReader.ReadToEnd();

                HtmlDocument responsePage = new HtmlDocument();
                responsePage.LoadHtml(responseText);
                return (responsePage);
            }
            catch (WebException e)
            {
                switch (site)
                {
                    case (Sites.Xiami):
                        {
                            // If we already handled 403 page (modify cookies) and the error still occurs, something more weird is happening.
                            if (alreadyHandledXiami403 == true)
                            {
                                throw (e);
                            }
                            else
                            {
                                // The error page is a 403 but with some contents indicating how to modify cookies.
                                HttpWebResponse hwexception = (HttpWebResponse)e.Response;

                                // If it's 403 Forbidden, modify the cookies and try again.
                                switch (hwexception.StatusCode)
                                {
                                    case (HttpStatusCode.Forbidden):
                                        {
                                            Stream exceptionStream = hwexception.GetResponseStream();
                                            StreamReader exceptionReader = new StreamReader(exceptionStream, Encoding.UTF8, true);
                                            string exceptionText = exceptionReader.ReadToEnd();

                                            SiteXiami.HandleXiamiForbidden(exceptionText);
                                            alreadyHandledXiami403 = true;
                                            goto Start;
                                        }
                                }
                            }
                            break;
                        }
                }
            }
            // Actually we should never reach here
            return (new HtmlDocument());
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
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Stream fileStream = new FileStream(filePath, System.IO.FileMode.Create);
            byte[] contentBytes = new byte[1024 * 32];
            int remainingSize = responseStream.Read(contentBytes, 0, (int)contentBytes.Length);
            while (remainingSize > 0)
            {
                fileStream.Write(contentBytes, 0, remainingSize);
                remainingSize = responseStream.Read(contentBytes, 0, (int)contentBytes.Length);
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
        static public void ResizeImage(string sourceFilePath, string destFilePath, int maxSize)
        {
            int newHeight;
            int newWidth;
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
                if (largeImage.Height > largeImage.Width)
                {
                    newHeight = maxSize;
                    newWidth = (int)(maxSize * ((double)largeImage.Width / (double)largeImage.Height));
                }
                else if (largeImage.Height < largeImage.Width)
                {
                    newHeight = (int)(maxSize * ((double)largeImage.Height / (double)largeImage.Width));
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
                templateImage.Save(destFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

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
        /// <param name="AlbumPage">Album page (which contains cover)</param>
        /// <param name="resizeSize">The maximum size of width/height after resize (0 for skipping resizing)</param>
        /// <returns>The file path after resizing</returns>
        static public string GenerateCover(string remoteCoverUrl, int resizeSize)
        {
            string largeTempFile = Path.GetTempPath() + Path.GetFileName(remoteCoverUrl) + ".jpg";
            string smallTempFile = Path.GetTempPath() + Path.GetFileName(remoteCoverUrl) + "s.jpg";
            if (File.Exists(largeTempFile))
            {
                File.Delete(largeTempFile);
            }
            if (File.Exists(smallTempFile))
            {
                File.Delete(smallTempFile);
            }
            DownloadFile(remoteCoverUrl, largeTempFile);
            if (resizeSize != 0)
            {
                ResizeImage(largeTempFile, smallTempFile, resizeSize);
            }
            File.Delete(largeTempFile);
            return (smallTempFile);
        }

        /// <summary>
        /// Write ID3 tags for a file
        /// </summary>
        /// <param name="filePath">The file path to write</param>
        /// <param name="id3Info">ID3 tags</param>
        /// <param name="options">Options (whether need to write info, cover, lyrics)</param>
        static public void WriteFile(string filePath, Id3 id3Info, GrabOptions options)
        {
            TagLib.File trackFile = TagLib.File.Create(filePath);

            trackFile.RemoveTags(TagLib.TagTypes.Id3v1);
            trackFile.RemoveTags(TagLib.TagTypes.Ape);
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            if (options.NeedId3)
            {
                trackFile.Tag.Album = id3Info.AlbumTitle;
                trackFile.Tag.AlbumArtists = id3Info.AlbumArtists;
                trackFile.Tag.Title = id3Info.TrackName;
                trackFile.Tag.Performers = id3Info.Performers;
                trackFile.Tag.Year = id3Info.Year;
                trackFile.Tag.Disc = id3Info.Disc;
                trackFile.Tag.DiscCount = id3Info.DiscCount;
                trackFile.Tag.Track = id3Info.Track;
                trackFile.Tag.TrackCount = id3Info.TrackCount;
            }

            if (options.NeedCover)
            {
                trackFile.Tag.Pictures = id3Info.CoverImageList.ToArray();
            }

            if (options.NeedLyric)
            {
                trackFile.Tag.Lyrics = id3Info.Lyrics;
            }

            trackFile.Save();
        }

        /// <summary>
        /// Sort file in auto sort mode
        /// </summary>
        /// <param name="localFileList">The list of local files</param>
        /// <param name="remoteTracksByDiscs">The list of remote files, two-layers</param>
        /// <param name="ifValid">Indicate whether the output list is valid. Not valid if not full map</param>
        /// <param name="localToRemoteMap"></param>
        /// <returns></returns>
        static public List<string> AutoSortFile(List<string> localFileList, List<List<string>> remoteTracksByDiscs, out bool ifValid, out Dictionary<string, Tuple<int, int>> localToRemoteMap)
        {
            List<string> sortResult = new List<string>(localFileList.Count);
            // Generate a list in which files are sorted naturally.
            List<string> naturalFileList = new List<string>(localFileList.ToArray());

            ifValid = true;
            localToRemoteMap = new Dictionary<string, Tuple<int, int>>();
            int remoteTrackNumber = -1;

            // A bit tricky: arrange from the last one, to make sure tracks "A" and "A(z)" are matched first for "A(z)" then "A". 
            for (int i = remoteTracksByDiscs.Count - 1; i >= 0; i--)
            {
                for (int j = remoteTracksByDiscs[i].Count - 1; j >= 0; j--)
                {
                    remoteTrackNumber++;
                    string remoteTrackName = remoteTracksByDiscs[i][j];

                    // For each remote track, search all local files
                    for (int k = naturalFileList.Count - 1; k >= 0; k--)
                    {
                        string localFullFileName = naturalFileList[k];
                        if (localFullFileName == "")
                        {
                            continue;
                        }
                        string localFileName = localFullFileName.Substring(localFullFileName.LastIndexOf("\\") + 1);
                        localFileName = localFileName.Substring(0, localFileName.LastIndexOf("."));

                        // If we're lucky enough to get such a local file
                        if (localFileName.IndexOf(remoteTrackName, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            localToRemoteMap.Add(naturalFileList[k], new Tuple<int, int>(i, j));
                            sortResult.Add(naturalFileList[k]);
                            naturalFileList[k] = "";
                            break;
                        }
                    }
                }
            }
            if (localToRemoteMap.Count != naturalFileList.Count)
            {
                ifValid = false;
            }
            return (sortResult);
        }
    }
}
