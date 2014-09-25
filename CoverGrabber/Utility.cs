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
        public string code;
        public string sessionID;
        public string apply;
        public string referer;
        public string localVerifyCode;
    };

    static class Utility
    {
        public delegate string ParseCoverAddress(HtmlDocument PageDocument);
        public delegate List<List<string>> ParseTrackList(HtmlDocument PageDocument);
        public delegate List<List<string>> ParseTrackUrlList(HtmlDocument PageDocument);
        public delegate List<List<string>> ParseTrackArtistList(HtmlDocument PageDocument);
        public delegate string ParseTrackLyric(HtmlDocument PageDocument);
        public delegate string ParseAlbumTitle(HtmlDocument PageDocument);
        public delegate string ParseAlbumArtist(HtmlDocument PageDocument);
        public delegate uint ParseAlbumYear(HtmlDocument PageDocument);

        /// <summary>
        /// The cookies which assists verify code (otherwise verify code / 403 handler doesn't work)
        /// </summary>
        static public CookieContainer cookies = new CookieContainer();

        /// <summary>
        /// Download a page (UTF-8) and return the page content
        /// </summary>
        /// <param name="Url">URL to download</param>
        /// <param name="Site">Site to download from</param>
        /// <returns>The page content as string</returns>
        static public HtmlDocument DownloadPage(string Url, Sites Site)
        {
        Start:
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            bool alreadyHandledXiami403 = false;
            switch (Site)
            {
                case (Sites.Xiami):
                    {
                        SiteXiami.InitializeRequest(ref request, Url);
                        break;
                    }
                case (Sites.Netease):
                    {
                        SiteNetease.InitializeRequest(ref request, Url);
                        break;
                    }
                case (Sites.ItunesStore):
                    {
                        SiteItunes.InitializeRequest(ref request, Url);
                        break;
                    }
                case (Sites.MusicBrainz):
                    {
                        SiteMusicBrainz.InitializeRequest(ref request, Url);
                        break;
                    }
                case (Sites.VgmDb):
                    {
                        SiteVgmdb.InitializeRequest(ref request, Url);
                        break;
                    }
                case (Sites.LastFm):
                    {
                        SiteLastFm.InitializeRequest(ref request, Url);
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

                cookies.Add(response.Cookies);
                responseText = objReader.ReadToEnd();

                HtmlDocument responsePage = new HtmlDocument();
                responsePage.LoadHtml(responseText);
                return (responsePage);
            }
            catch (WebException e)
            {
                switch (Site)
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

                                            SiteXiami.handleXiamiForbidden(exceptionText);
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
        /// <param name="Url">URL to download</param>
        /// <param name="FilePath">File path to save</param>
        static public void DownloadFile(string Url, string FilePath)
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(Url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();
            Stream fileStream = new FileStream(FilePath, System.IO.FileMode.Create);
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
        /// <param name="SourceFilePath">The file before resize</param>
        /// <param name="DestFilePath">The file after resize</param>
        /// <param name="MaxSize">The max size on width/height</param>
        /// <returns>Whether resize succeeds</returns>
        static public void ResizeImage(string SourceFilePath, string DestFilePath, int MaxSize)
        {
            int newHeight;
            int newWidth;
            Image largeImage = new Bitmap(SourceFilePath);

            if (File.Exists(DestFilePath))
            {
                File.Delete(DestFilePath);
            }

            /* If the new size is smaller than the original size, resize it
             * Otherwise directly copy to create duplication
             * */
            if (largeImage.Width > MaxSize && largeImage.Width > MaxSize)
            {
                if (largeImage.Height > largeImage.Width)
                {
                    newHeight = MaxSize;
                    newWidth = (int)(MaxSize * ((double)largeImage.Width / (double)largeImage.Height));
                }
                else if (largeImage.Height < largeImage.Width)
                {
                    newHeight = (int)(MaxSize * ((double)largeImage.Height / (double)largeImage.Width));
                    newWidth = MaxSize;
                }
                else
                {
                    newHeight = MaxSize;
                    newWidth = MaxSize;
                }

                Image templateImage = new Bitmap(newWidth, newHeight);
                Graphics templateGraphics = Graphics.FromImage(templateImage);

                templateGraphics.InterpolationMode = InterpolationMode.High;
                templateGraphics.SmoothingMode = SmoothingMode.HighQuality;
                templateGraphics.Clear(Color.White);
                templateGraphics.DrawImage(largeImage, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, largeImage.Width, largeImage.Height), GraphicsUnit.Pixel);
                templateImage.Save(DestFilePath, System.Drawing.Imaging.ImageFormat.Jpeg);

                templateGraphics.Dispose();
                templateImage.Dispose();
            }
            else
            {
                File.Copy(SourceFilePath, DestFilePath);
            }
            largeImage.Dispose();
        }

        /// <summary>
        /// Download, save and resize cover image
        /// </summary>
        /// <param name="AlbumPage">Album page (which contains cover)</param>
        /// <param name="ResizeSize">The maximum size of width/height after resize (0 for skipping resizing)</param>
        /// <returns>The file path after resizing</returns>
        static public string GenerateCover(string remoteCoverUrl, int ResizeSize)
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
            if (ResizeSize != 0)
            {
                ResizeImage(largeTempFile, smallTempFile, ResizeSize);
            }
            File.Delete(largeTempFile);
            return (smallTempFile);
        }

        /// <summary>
        /// Write ID3 tags for a file
        /// </summary>
        /// <param name="FilePath">The file path to write</param>
        /// <param name="Id3Info">ID3 tags</param>
        /// <param name="Options">Options (whether need to write info, cover, lyrics)</param>
        static public void WriteFile(string FilePath, Id3 Id3Info, GrabOptions Options)
        {
            TagLib.File trackFile = TagLib.File.Create(FilePath);

            trackFile.RemoveTags(TagLib.TagTypes.Id3v1);
            trackFile.RemoveTags(TagLib.TagTypes.Ape);
            TagLib.Id3v2.Tag.DefaultVersion = 3;
            TagLib.Id3v2.Tag.ForceDefaultVersion = true;

            if (Options.needId3)
            {
                trackFile.Tag.Album = Id3Info.AlbumTitle;
                trackFile.Tag.AlbumArtists = Id3Info.AlbumArtists;
                trackFile.Tag.Title = Id3Info.TrackName;
                trackFile.Tag.Performers = Id3Info.Performers;
                trackFile.Tag.Year = Id3Info.Year;
                trackFile.Tag.Disc = Id3Info.Disc;
                trackFile.Tag.DiscCount = Id3Info.DiscCount;
                trackFile.Tag.Track = Id3Info.Track;
                trackFile.Tag.TrackCount = Id3Info.TrackCount;
            }

            if (Options.needCover)
            {
                trackFile.Tag.Pictures = Id3Info.CoverImageList.ToArray();
            }

            if (Options.needLyric)
            {
                trackFile.Tag.Lyrics = Id3Info.Lyrics;
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
