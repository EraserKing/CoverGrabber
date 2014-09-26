using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Linq;
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

        GrabOptions grabOptions = new GrabOptions();
        List<string> sortedFileList = new List<string>();

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
            GrabOptions options = new GrabOptions();
            options.site = Sites.Null;
            options.localFolder = this.folder.Text;
            options.webPageUrl = this.url.Text;
            options.needCover = this.coverC.Checked;
            options.resizeSize = (int)this.resizeSize.Value;
            options.needId3 = this.id3C.Checked;
            options.needLyric = this.lyricC.Checked;
            if (this.sAutoRs.Checked)
            {
                options.sortMode = "Auto";
            }
            else if (this.sNaturallyRs.Checked)
            {
                options.sortMode = "Naturally";
            }
            else if (this.sManuallyRs.Checked)
            {
                options.sortMode = "Manually";
            }
            options.fileList = this.sortedFileList;

            this.InitializeEnvironment(ref options);
            if (options.site == Sites.Null)
            {
                MessageBox.Show("Not supported site.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            grabOptions = options;

            this.folder.Enabled = false;
            this.folderB.Enabled = false;
            this.url.Enabled = false;
            this.coverC.Enabled = false;
            this.resizeSize.Enabled = false;
            this.id3C.Enabled = false;
            this.lyricC.Enabled = false;
            this.goB.Enabled = false;
            this.sNaturallyRs.Enabled = false;
            this.sAutoRs.Enabled = false;
            this.sManuallyRs.Enabled = false;
            this.sortB.Enabled = false;
            

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
            this.sNaturallyRs.Enabled = true;
            this.sAutoRs.Enabled = true;
            this.sManuallyRs.Enabled = true;
            this.sortB.Enabled = this.sManuallyRs.Checked;
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
            #endregion

            #region Check local folder and generate files list
            SetProgress(Bw, 0, "Getting information for local tracks...", ProgressReportObject.Skip, "");
            try
            {
                if (options.sortMode == "Auto" || options.sortMode == "Naturally")
                {
                    fileList = GenerateFileList(options.localFolder);
                }
                else if (options.sortMode == "Manually")
                {
                    fileList = options.fileList;
                }
            }
            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show("Folder " + e.Message + " doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion

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
            #endregion

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
            #endregion

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
            #endregion

            #region Generate file list in auto sort mode
            if (options.sortMode == "Auto")
            {
                bool ifValid;
                Dictionary<string, Tuple<int, int>> localToRemoteMap;
                List<string> sortedFileList;
                sortedFileList = Utility.AutoSortFile(fileList, trackNamesByDiscs, out ifValid, out localToRemoteMap);
                string promptMessage = "The auto sort result is:\n";
                var queryResult = from key in localToRemoteMap.Keys
                                  orderby key
                                  select key;
                foreach (var key in queryResult)
                {
                    promptMessage += key + " => " + trackNamesByDiscs[localToRemoteMap[key].Item1][localToRemoteMap[key].Item2] + "\n";
                }
                if (ifValid)
                {
                    promptMessage += "To accept this, press Yes. To continue to the naturally sort, press No. To cancel, press Cancel.";
                    DialogResult dr = MessageBox.Show(promptMessage, "Auto sort result", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.Yes)
                    {
                        fileList = sortedFileList;
                    }
                    else if (dr == DialogResult.Cancel)
                    {
                        CleanProgress(Bw);
                        return;
                    }
                }
                else
                {
                    string missingFiles = "";
                    foreach(var key in fileList)
                    {
                        if (!localToRemoteMap.ContainsKey(key))
                        {
                            missingFiles += key + "\n";
                        }
                    }
                    if(missingFiles!= "")
                    {
                        promptMessage += "\n These files do not have matched tracks:\n";
                        promptMessage += missingFiles;
                    }
                    promptMessage += "You cannot continue in Auto mode, but you can sort them manually.";
                    MessageBox.Show(promptMessage, "Auto sort result", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
            }
            #endregion

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
            #endregion

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
            #endregion

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
                                    case (Sites.Netease):
                                        {
                                            lyric = this.parseTrackLyric(Utility.DownloadPage("http://music.163.com" + trackUrl, options.site));
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
            #endregion

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
            #endregion

            #region Clean up
            SetProgress(Bw, 100, "Done", ProgressReportObject.Skip, "");

            MessageBox.Show("Done.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            #endregion
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
                // Rename http://music.163.com/#/album?id=76460 to http://music.163.com/album?id=76460, otherwise it doesn't work
                grabOptions.webPageUrl = grabOptions.webPageUrl.Replace("/#/", "/");
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

            if (grabOptions.webPageUrl.StartsWith(@"http://cn.last.fm/"))
            {
                grabOptions.site = Sites.LastFm;
                this.parseCoverAddress = SiteLastFm.ParseCoverAddress;
                this.parseTrackList = SiteLastFm.ParseTrackList;
                this.parseTrackUrlList = SiteLastFm.ParseTrackUrlList;
                this.parseTrackArtistList = SiteLastFm.ParseTrackArtistList;
                this.parseTrackLyric = SiteLastFm.ParseTrackLyric;
                this.parseAlbumTitle = SiteLastFm.ParseAlbumTitle;
                this.parseAlbumArtist = SiteLastFm.ParseAlbumArtist;
                this.parseAlbumYear = SiteLastFm.ParseAlbumYear;

                grabOptions.needLyric = false;
                return;
            }

            if (grabOptions.webPageUrl.StartsWith(@"http://vgmdb.net/"))
            {
                grabOptions.site = Sites.VgmDb;
                this.parseCoverAddress = SiteVgmdb.ParseCoverAddress;
                this.parseTrackList = SiteVgmdb.ParseTrackList;
                this.parseTrackUrlList = SiteVgmdb.ParseTrackUrlList;
                this.parseTrackArtistList = SiteVgmdb.ParseTrackArtistList;
                this.parseTrackLyric = SiteVgmdb.ParseTrackLyric;
                this.parseAlbumTitle = SiteVgmdb.ParseAlbumTitle;
                this.parseAlbumArtist = SiteVgmdb.ParseAlbumArtist;
                this.parseAlbumYear = SiteVgmdb.ParseAlbumYear;

                grabOptions.needLyric = false;
                return;
            }

            if (grabOptions.webPageUrl.StartsWith(@"http://musicbrainz.org/") ||
                grabOptions.webPageUrl.StartsWith(@"https://musicbrainz.org/"))
            {
                grabOptions.site = Sites.MusicBrainz;
                this.parseCoverAddress = SiteMusicBrainz.ParseCoverAddress;
                this.parseTrackList = SiteMusicBrainz.ParseTrackList;
                this.parseTrackUrlList = SiteMusicBrainz.ParseTrackUrlList;
                this.parseTrackArtistList = SiteMusicBrainz.ParseTrackArtistList;
                this.parseTrackLyric = SiteMusicBrainz.ParseTrackLyric;
                this.parseAlbumTitle = SiteMusicBrainz.ParseAlbumTitle;
                this.parseAlbumArtist = SiteMusicBrainz.ParseAlbumArtist;
                this.parseAlbumYear = SiteMusicBrainz.ParseAlbumYear;

                grabOptions.needLyric = false;
                return;
            }
            if (grabOptions.webPageUrl.StartsWith(@"http://itunes.apple.com/") ||
                grabOptions.webPageUrl.StartsWith(@"https://itunes.apple.com/"))
            {
                grabOptions.site = Sites.ItunesStore;
                this.parseCoverAddress = SiteItunes.ParseCoverAddress;
                this.parseTrackList = SiteItunes.ParseTrackList;
                this.parseTrackUrlList = SiteItunes.ParseTrackUrlList;
                this.parseTrackArtistList = SiteItunes.ParseTrackArtistList;
                this.parseTrackLyric = SiteItunes.ParseTrackLyric;
                this.parseAlbumTitle = SiteItunes.ParseAlbumTitle;
                this.parseAlbumArtist = SiteItunes.ParseAlbumArtist;
                this.parseAlbumYear = SiteItunes.ParseAlbumYear;

                grabOptions.needLyric = false;
                return;
            }
        }

        private void sNatuallyRs_CheckedChanged(object sender, EventArgs e)
        {
            if (sNaturallyRs.Checked)
            {
                this.sortB.Enabled = false;
            }
        }

        private void sAutoRs_CheckedChanged(object sender, EventArgs e)
        {
            if (sAutoRs.Checked)
            {
                this.sortB.Enabled = false;
            }
        }

        private void sManuallyRs_CheckedChanged(object sender, EventArgs e)
        {
            if (sManuallyRs.Checked)
            {
                this.sortB.Enabled = true;
            }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }

        private void sortB_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.sortedFileList == null)
                {
                    this.sortedFileList = GenerateFileList(this.folder.Text);
                }
            }
            catch (DirectoryNotFoundException e1)
            {
                MessageBox.Show("Folder " + e1.Message + " doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            sortForm sf = new sortForm(this.sortedFileList);
            sf.ShowDialog();
            this.sortedFileList = sf.files;
        }

        private void folder_TextChanged(object sender, EventArgs e)
        {
            this.sNaturallyRs.Checked = true;
            this.sortB.Enabled = false;
            this.sortedFileList = null;
        }

    }
}
