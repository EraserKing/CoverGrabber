using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using TagLib;

namespace CoverGrabber
{
    public partial class mainForm : Form
    {
        Utility.ParseCoverAddress parseCoverAddress;
        Utility.ParseTrackList parseTrackList;
        Utility.ParseTrackUrlList parseTrackUrlList;
        Utility.ParseTrackArtistList parseTrackArtistList;
        Utility.ParseTrackLyric parseTrackLyric;
        Utility.ParseAlbumTitle parseAlbumTitle;
        Utility.ParseAlbumArtist parseAlbumArtist;
        Utility.ParseAlbumYear parseAlbumYear;

        public mainForm()
        {
            InitializeComponent();
        }

        private void folderB_Click(object sender, EventArgs e)
        {
            if (this.fbd.ShowDialog() == DialogResult.OK)
            {
                if (this.folder.Text != "")
                {
                    DialogResult dr = MessageBox.Show("Do you want to replace the folder text box, or append it to the list?\nYes: Replace\nNo: Append to the list", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            {
                                this.folder.Text = this.fbd.SelectedPath;
                                break;
                            }
                        case DialogResult.No:
                            {
                                this.folder.Text = (this.folder.Text == "" ? "" : this.folder.Text + "; ") + this.fbd.SelectedPath;
                                break;
                            }
                    }
                }
                else
                {
                    this.folder.Text = this.fbd.SelectedPath;
                }
            }
        }

        private void coverC_CheckedChanged(object sender, EventArgs e)
        {
            this.resizeSize.Enabled = this.coverC.Checked;
        }

        private void goB_Click(object sender, EventArgs e)
        {
            GrabOptions grabOptions = new GrabOptions();
            grabOptions.site = Sites.Null;
            grabOptions.localFolder = this.folder.Text;
            grabOptions.webPageUrl = this.url.Text;
            grabOptions.needCover = this.coverC.Checked;
            grabOptions.resizeSize = (int)this.resizeSize.Value;
            grabOptions.needId3 = this.id3C.Checked;
            grabOptions.needLyric = this.lyricC.Checked;

            this.InitializeEnvironment(ref grabOptions);
            if (grabOptions.site == Sites.Null)
            {
                MessageBox.Show("Not supported site.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.folder.Enabled = false;
            this.folderB.Enabled = false;
            this.url.Enabled = false;
            this.coverC.Enabled = false;
            this.resizeSize.Enabled = false;
            this.id3C.Enabled = false;
            this.lyricC.Enabled = false;
            this.goB.Enabled = false;

            this.bw.RunWorkerAsync(grabOptions);
        }

        private void bw_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.doGrab(this.bw, e.Argument);
        }

        private void bw_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            this.tssP.Value = e.ProgressPercentage;
            ProgressOptions progressOptions = (ProgressOptions)e.UserState;

            this.tssL.Text = progressOptions.statusMessage;

            switch (progressOptions.objectName)
            {
                case (ProgressReportObject.AlbumTitle):
                    {
                        this.titleL.Text = progressOptions.objectValue;
                        break;
                    }
                case (ProgressReportObject.AlbumArtist):
                    {
                        this.artiseL.Text = progressOptions.objectValue;
                        break;
                    }
                case (ProgressReportObject.AlbumCover):
                    {
                        this.coverP.ImageLocation = progressOptions.objectValue;
                        break;
                    }
                case (ProgressReportObject.Text):
                    {
                        this.trackT.AppendText(progressOptions.objectValue);
                        break;
                    }
                case (ProgressReportObject.TextClear):
                    {
                        this.trackT.Clear();
                        break;
                    }
                case (ProgressReportObject.VerifyCode):
                    {
                        this.verifyCodeP.ImageLocation = progressOptions.objectValue;
                        break;
                    }
            }
        }

        private void bw_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.folder.Enabled = true;
            this.folderB.Enabled = true;
            this.url.Enabled = true;
            this.coverC.Enabled = true;
            this.resizeSize.Enabled = true;
            this.id3C.Enabled = true;
            this.lyricC.Enabled = true;
            this.goB.Enabled = true;
        }

        private void doGrab(BackgroundWorker Bw, object Options)
        {
            GrabOptions options = (GrabOptions)Options;
            HtmlAgilityPack.HtmlDocument albumPage;

            string albumArtistName = "";
            string albumTitle = "";
            uint albumYear = 0;

            List<List<string>> trackNamesByDiscs = new List<List<string>>();
            List<List<string>> artistNamesByDiscs = new List<List<string>>();
            List<List<string>> lyricsByDiscs = new List<List<string>>();
            List<string> fileList = new List<string>();

            string smallTempFile = "";

            int localTrackQuantity = 0;
            int remoteTrackQuantity = 0;

            int currentTrackIndex = 0;
            int currentDiscIndex = 0;

            #region Preparion work
            CleanProgress(Bw);
            if (!options.needCover && !options.needId3 && !options.needLyric)
            {
                MessageBox.Show("You haven't pick any task.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion Preparation work

            #region Check local folder and generate files list
            SetProgress(Bw, 0, "Getting information for local tracks...", ProgressReportObject.Skip, "");
            try
            {
                fileList = GenerateFileList(options.localFolder);
            }
            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show("Folder " + e.Message + " doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Check local folder and generate files list

            #region Get remote page
            SetProgress(Bw, 10, "Getting remote page information...", ProgressReportObject.Skip, "");
            try
            {
                albumPage = Utility.DownloadPage(options.webPageUrl, options.site);
            }
            catch (Exception e)
            {
                MessageBox.Show("Accessing album page " + options.webPageUrl + " failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Get remote page

            #region Generate track lists
            SetProgress(Bw, 20, "Getting tracks list...", ProgressReportObject.Skip, "");
            try
            {
                trackNamesByDiscs = this.parseTrackList(albumPage);
            }
            catch (Exception e)
            {
                MessageBox.Show("Parsing track lists from " + options.webPageUrl + " failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Generate track lists

            #region Compare if quantity tracks in local and on page equal
            foreach (var trackNamesInDisc in trackNamesByDiscs)
            {
                remoteTrackQuantity += trackNamesInDisc.Count;
            }
            localTrackQuantity = fileList.Count;
            if (remoteTrackQuantity != localTrackQuantity)
            {
                MessageBox.Show("You have " + localTrackQuantity.ToString() + " tracks in local folder(s), but " + remoteTrackQuantity.ToString() + " tracks on album page.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Compare if quantity tracks in local and on page equal

            #region Get cover
            if (options.needCover)
            {
                SetProgress(Bw, 30, "Getting cover image...", ProgressReportObject.Skip, "");
                try
                {
                    string largeCoverUrl = this.parseCoverAddress(albumPage);
                    smallTempFile = Utility.GenerateCover(largeCoverUrl, options.resizeSize);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Downloading cover failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
                SetProgress(Bw, 30, "Getting cover image...", ProgressReportObject.AlbumCover, smallTempFile);
            }
            #endregion Get cover

            #region Get ID3
            if (options.needId3)
            {
                SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.Skip, "");
                try
                {
                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.TextClear, "");

                    artistNamesByDiscs = this.parseTrackArtistList(albumPage);
                    foreach (var trackList in trackNamesByDiscs)
                    {
                        foreach (string track in trackList)
                        {
                            SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.Text, track + "\n");
                        }
                    }
                    albumTitle = this.parseAlbumTitle(albumPage);
                    albumArtistName = this.parseAlbumArtist(albumPage);
                    albumYear = this.parseAlbumYear(albumPage);

                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.AlbumTitle, albumTitle);
                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.AlbumArtist, albumArtistName);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Parsing page failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
            }
            #endregion Get ID3

            #region Get lyrics
            if (options.needLyric)
            {
                SetProgress(Bw, 50, "Getting lyrics...", ProgressReportObject.Skip, "");

                currentTrackIndex = 0;
                List<List<string>> trackUrlListByDiscs = this.parseTrackUrlList(albumPage);

                foreach (var trackUrlInDisc in trackUrlListByDiscs)
                {
                    List<string> lyricInDisc = new List<string>();
                    foreach (string trackUrl in trackUrlInDisc)
                    {
                        try
                        {
                            string lyric = "";
                            SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", ProgressReportObject.Skip, "");

                            if (trackUrl != "")
                            {
                                switch (options.site)
                                {
                                    case (Sites.Xiami):
                                        {
                                            lyric = this.parseTrackLyric(Utility.DownloadPage("http://www.xiami.com" + trackUrl, options.site));
                                            break;
                                        }
                                }
                                if (lyric != "")
                                {
                                    SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", ProgressReportObject.Text, "\nFirst line of lyric for track " + (currentTrackIndex + 1).ToString() + ":\n");
                                    SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", ProgressReportObject.Text, lyric.Split("\n".ToCharArray())[0]);
                                }
                                System.Threading.Thread.Sleep(500);
                            }
                            currentTrackIndex++;
                            lyricInDisc.Add(lyric);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show("Downloading lyrics for track " + (currentTrackIndex + 1).ToString() + " failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            CleanProgress(Bw);
                            return;
                        }
                    }
                    currentDiscIndex++;
                    lyricsByDiscs.Add(lyricInDisc);
                }
            }
            #endregion Get lyrics

            #region Write file
            currentTrackIndex = 0;
            SetProgress(Bw, 90, "Writing local files...", ProgressReportObject.Skip, "");

            for (int i = 0; i < trackNamesByDiscs.Count; i++)
            {
                for (int j = 0; j < trackNamesByDiscs[i].Count; j++)
                {
                    SetProgress(Bw, 90 + (int)(10.0 * currentTrackIndex / localTrackQuantity), "Writing track " + (currentTrackIndex + 1).ToString() + " ...", ProgressReportObject.Skip, "");

                    try
                    {
                        Id3 id3 = new Id3();

                        if (options.needId3)
                        {
                            id3.AlbumTitle = albumTitle;
                            id3.AlbumArtists = albumArtistName.Split(";".ToCharArray());
                            id3.TrackName = trackNamesByDiscs[i][j];
                            id3.Disc = (uint)(i + 1);
                            id3.DiscCount = (uint)(trackNamesByDiscs.Count);
                            id3.Track = (uint)(j + 1);
                            id3.TrackCount = (uint)(trackNamesByDiscs[i].Count);
                            id3.Performers = (artistNamesByDiscs[i][j] != "" ? artistNamesByDiscs[i][j] : albumArtistName).Split(";".ToCharArray());
                            id3.Year = albumYear;
                        }
                        if (options.needCover)
                        {
                            id3.CoverImageList = new List<Picture>();
                            id3.CoverImageList.Add(new Picture(smallTempFile));
                        }
                        if (options.needLyric)
                        {
                            id3.Lyrics = lyricsByDiscs[i][j];
                        }
                        Utility.WriteFile(fileList[currentTrackIndex], id3, options);
                        currentTrackIndex++;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Writing information for track " + (currentTrackIndex + 1).ToString() + " failed.\n" + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        CleanProgress(Bw);
                        break;
                    }
                }
            }
            #endregion Write file

            #region Clean up
            SetProgress(Bw, 100, "Done", ProgressReportObject.Skip, "");

            MessageBox.Show("Done.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            #endregion Clean up
        }

        private static void SetProgress(BackgroundWorker Bw, int Progress, string StatusMessage, ProgressReportObject ObjectName, string ObjectValue)
        {
            ProgressOptions progressOptions = new ProgressOptions();
            progressOptions.statusMessage = StatusMessage;
            progressOptions.objectName = ObjectName;
            progressOptions.objectValue = ObjectValue;
            Bw.ReportProgress(Progress, progressOptions);
        }

        private static void CleanProgress(BackgroundWorker Bw)
        {
            SetProgress(Bw, 0, "", ProgressReportObject.Skip, "");
        }

        private static List<string> GenerateFileList(string FolderStrings)
        {
            List<string> fileList = new List<string>();

            string[] folderLists = FolderStrings.Split(";".ToCharArray());
            foreach (string singleFolder in folderLists)
            {
                if (!Directory.Exists(singleFolder))
                {
                    throw (new DirectoryNotFoundException(singleFolder));
                }
                DirectoryInfo directory = new DirectoryInfo(singleFolder);
                {
                    foreach (FileInfo file in directory.GetFiles("*.m4a"))
                    {
                        fileList.Add(file.FullName);
                    }
                    foreach (FileInfo file in directory.GetFiles("*.mp3"))
                    {
                        fileList.Add(file.FullName);
                    }
                }

            }
            return (fileList);
        }

        private void InitializeEnvironment(ref GrabOptions grabOptions)
        {
            if (grabOptions.webPageUrl.StartsWith(@"http://www.xiami.com/album/"))
            {
                grabOptions.site = Sites.Xiami;
                this.parseCoverAddress = SiteXiami.ParseCoverAddress;
                this.parseTrackList = SiteXiami.ParseTrackList;
                this.parseTrackUrlList = SiteXiami.ParseTrackUrlList;
                this.parseTrackArtistList = SiteXiami.ParseTrackArtistList;
                this.parseTrackLyric = SiteXiami.ParseTrackLyric;
                this.parseAlbumTitle = SiteXiami.ParseAlbumTitle;
                this.parseAlbumArtist = SiteXiami.ParseAlbumArtist;
                this.parseAlbumYear = SiteXiami.ParseAlbumYear;
                return;
            }

            if (grabOptions.webPageUrl.StartsWith(@"http://music.163.com/"))
            {
                grabOptions.site = Sites.Netease;
                this.parseCoverAddress = SiteNetease.ParseCoverAddress;
                this.parseTrackList = SiteNetease.ParseTrackList;
                this.parseTrackUrlList = SiteNetease.ParseTrackUrlList;
                this.parseTrackArtistList = SiteNetease.ParseTrackArtistList;
                this.parseTrackLyric = SiteNetease.ParseTrackLyric;
                this.parseAlbumTitle = SiteNetease.ParseAlbumTitle;
                this.parseAlbumArtist = SiteNetease.ParseAlbumArtist;
                this.parseAlbumYear = SiteNetease.ParseAlbumYear;
                return;
            }
        }
    }
}
