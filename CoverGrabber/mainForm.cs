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
            this.folder.Enabled = false;
            this.folderB.Enabled = false;
            this.url.Enabled = false;
            this.coverC.Enabled = false;
            this.resizeSize.Enabled = false;
            this.id3C.Enabled = false;
            this.lyricC.Enabled = false;
            this.goB.Enabled = false;

            GrabOptions grabOptions = new GrabOptions();
            grabOptions.localFolder = this.folder.Text;
            grabOptions.webPageUrl = this.url.Text;
            grabOptions.needCover = this.coverC.Checked;
            grabOptions.resizeSize = (int)this.resizeSize.Value;
            grabOptions.needId3 = this.id3C.Checked;
            grabOptions.needLyric = this.lyricC.Checked;

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

            switch(progressOptions.objectName)
            {
                case(ProgressReportObject.AlbumTitle):
                    {
                        this.titleL.Text = progressOptions.objectValue;
                        break;
                    }
                case(ProgressReportObject.AlbumArtist):
                    {
                        this.artiseL.Text = progressOptions.objectValue;
                        break;
                    }
                case(ProgressReportObject.AlbumCover):
                    {
                        this.coverP.ImageLocation = progressOptions.objectValue;
                        break;
                    }
                case(ProgressReportObject.Text):
                    {
                        this.trackT.AppendText(progressOptions.objectValue);
                        break;
                    }
                case(ProgressReportObject.TextClear):
                    {
                        this.trackT.Clear();
                        break;
                    }
                case(ProgressReportObject.VerifyCode):
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

            ArrayList trackNamesByDiscs = new ArrayList();
            ArrayList artistNamesByDiscs = new ArrayList();
            ArrayList lyricsByDiscs = new ArrayList();
            ArrayList fileList = new ArrayList();

            string largeTempFile = "";
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

            string[] folderLists = options.localFolder.Split(";".ToCharArray());
            foreach (string singleFolder in folderLists)
            {
                if (!Directory.Exists(singleFolder))
                {
                    MessageBox.Show("Folder " + singleFolder + " doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
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
            #endregion Check local folder and generate files list

            #region Get remote page
            SetProgress(Bw, 10, "Getting remote page information...", ProgressReportObject.Skip, "");
            try
            {
                string htmlContent = Utility.DownloadPage(options.webPageUrl);
                albumPage = new HtmlAgilityPack.HtmlDocument();
                albumPage.LoadHtml(htmlContent);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Accessing album page " + options.webPageUrl + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Get remote page

            #region Generate track lists
            SetProgress(Bw, 20, "Getting tracks list...", ProgressReportObject.Skip, "");
            try
            {
                trackNamesByDiscs = Utility.ParseTrackList(albumPage);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Parsing track lists from " + options.webPageUrl + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress(Bw);
                return;
            }
            #endregion Generate track lists

            #region Compare if quantity tracks in local and on page equal
            foreach (ArrayList trackNamesInDisc in trackNamesByDiscs)
            {
                remoteTrackQuantity += trackNamesInDisc.Count;
            }
            localTrackQuantity = fileList.Count;
            if (remoteTrackQuantity != localTrackQuantity)
            {
                MessageBox.Show("You have " + localTrackQuantity.ToString() + " tracks in local folder(s), but " + remoteTrackQuantity.ToString() + " tracks in album page.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    string remoteCoverUrl = Utility.ParseCoverAddress(albumPage);
                    largeTempFile = Path.GetTempPath() + Path.GetFileName(remoteCoverUrl) + ".jpg";
                    smallTempFile = Path.GetTempPath() + Path.GetFileName(remoteCoverUrl) + "s.jpg";
                    Utility.DownloadFile(remoteCoverUrl, largeTempFile);
                    Utility.ResizeImage(largeTempFile, smallTempFile, (int)this.resizeSize.Value);
                    SetProgress(Bw, 30, "Getting cover image...", ProgressReportObject.AlbumCover, smallTempFile);
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Downloading cover failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
            }
            #endregion Get cover

            #region Get ID3
            if (options.needId3)
            {
                SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.Skip, "");
                try
                {
                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.TextClear, "");

                    artistNamesByDiscs = Utility.ParseTrackArtistList(albumPage);
                    foreach (ArrayList trackList in trackNamesByDiscs)
                    {
                        foreach (string track in trackList)
                        {
                            SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.Text, track + "\n");
                        }
                    }
                    albumTitle = Utility.ParseAlbumTitle(albumPage);
                    albumArtistName = Utility.ParseAlbumArtist(albumPage);
                    albumYear = Utility.ParseAlbumYear(albumPage);

                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.AlbumTitle, albumTitle);
                    SetProgress(Bw, 40, "Getting ID3 information...", ProgressReportObject.AlbumArtist, albumArtistName);
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Parsing page failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
            }
            #endregion Get ID3

            #region Get lyrics
            if (options.needLyric)
            {
                SetProgress(Bw, 50, "Getting lyrics...", ProgressReportObject.Skip, "");

                try
                {
                    currentTrackIndex = 0;
                    ArrayList trackUrlListByDiscs = Utility.ParseTrackUrlList(albumPage);

                    foreach (ArrayList trackUrlInDisc in trackUrlListByDiscs)
                    {
                        ArrayList lyricInDisc = new ArrayList();
                        foreach (string trackUrl in trackUrlInDisc)
                        {
                            try
                            {
                                string lyric = "";
                                SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", ProgressReportObject.Skip, "");

                                if (trackUrl != "")
                                {
                                    string trackHtmlContent = Utility.DownloadPage("http://www.xiami.com" + trackUrl);
                                    HtmlAgilityPack.HtmlDocument trackPage = new HtmlAgilityPack.HtmlDocument();
                                    trackPage.LoadHtml(trackHtmlContent);

                                    // Commented since I never met verify code since then.
                                    //// If code exists, or it's an error page, keep asking verify code, until it's correct, or user entered nothing to break
                                    //while (trackPage.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]") != null ||
                                    //    trackPage.DocumentNode.SelectSingleNode("//p[@id=\"youxianchupin\"]") != null)
                                    //{
                                    //    VerifyCode verifyCode = Utility.GetVerifyCode(trackPage);
                                    //    SetProgress(Bw, 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity), "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...", "VERIFY_CODE", verifyCode.localVerifyCode);

                                    //    string verifyCodeText = Microsoft.VisualBasic.Interaction.InputBox("Enter the verify code", "Verify Code", "");

                                    //    if (verifyCodeText == "")
                                    //    {
                                    //        MessageBox.Show("You aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    //        CleanProgress(Bw);
                                    //        return;
                                    //    }
                                    //    else
                                    //    {
                                    //        verifyCode.code = verifyCodeText;

                                    //        Utility.PostVerifyCode(verifyCode);

                                    //        // After posting, reload track page and see if everything goes fine
                                    //        trackHtmlContent = Utility.DownloadPage("http://www.xiami.com" + trackUrl);
                                    //        trackPage = new HtmlAgilityPack.HtmlDocument();
                                    //        trackPage.LoadHtml(trackHtmlContent);
                                    //    }
                                    //}
                                    lyric = Utility.ParseTrackLyric(trackPage);
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
                            catch (Exception e2)
                            {
                                MessageBox.Show("Downloading lyrics for track " + (currentTrackIndex + 1).ToString() + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                CleanProgress(Bw);
                                return;
                            }
                        }
                        currentDiscIndex++;
                        lyricsByDiscs.Add(lyricInDisc);
                    }
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Downloading lyrics failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    CleanProgress(Bw);
                    return;
                }
            }
            #endregion Get lyrics

            #region Write file
            currentDiscIndex = 0;
            currentTrackIndex = 0;
            SetProgress(Bw, 90, "Writing local files...", ProgressReportObject.Skip, "");

            for (int i = 0; i < trackNamesByDiscs.Count; i++)
            {
                ArrayList tracksInDisc = (ArrayList)trackNamesByDiscs[i];
                ArrayList trackArtistsInDisc = new ArrayList();
                ArrayList lyricsInDisc = new ArrayList();

                if (options.needId3)
                {
                    trackArtistsInDisc = (ArrayList)artistNamesByDiscs[i];
                }
                if (options.needLyric)
                {
                    lyricsInDisc = (ArrayList)lyricsByDiscs[i];
                }

                for (int j = 0; j < tracksInDisc.Count; j++)
                {
                    SetProgress(Bw, 90 + (int)(10.0 * currentTrackIndex / localTrackQuantity), "Writing track " + (currentTrackIndex + 1).ToString() + " ...", ProgressReportObject.Skip, "");

                    try
                    {
                        TagLib.File trackFile = TagLib.File.Create((string)fileList[currentTrackIndex]);

                        trackFile.RemoveTags(TagTypes.Id3v1);
                        trackFile.RemoveTags(TagTypes.Ape);
                        TagLib.Id3v2.Tag.DefaultVersion = 3;
                        TagLib.Id3v2.Tag.ForceDefaultVersion = true;

                        if (options.needId3)
                        {
                            string currentTrackName = (string)tracksInDisc[j];
                            string currentTrackArtist = (string)trackArtistsInDisc[j];

                            trackFile.Tag.Album = albumTitle;
                            trackFile.Tag.AlbumArtists = albumArtistName.Split(";".ToCharArray());

                            trackFile.Tag.Disc = (uint)(i + 1);
                            trackFile.Tag.DiscCount = (uint)(trackNamesByDiscs.Count);
                            trackFile.Tag.Track = (uint)(j + 1);
                            trackFile.Tag.TrackCount = (uint)(tracksInDisc.Count);

                            if (currentTrackArtist != "")
                            {
                                trackFile.Tag.Performers = currentTrackArtist.Split(";".ToCharArray());
                            }
                            else
                            {
                                trackFile.Tag.Performers = albumArtistName.Split(";".ToCharArray());
                            }
                            trackFile.Tag.Title = currentTrackName;
                            trackFile.Tag.Year = albumYear;
                        }

                        if (options.needCover)
                        {
                            List<Picture> coverImageList = new List<Picture>();
                            coverImageList.Add(new Picture(smallTempFile));
                            trackFile.Tag.Pictures = coverImageList.ToArray();
                        }

                        if (options.needLyric)
                        {
                            trackFile.Tag.Lyrics = (string)lyricsInDisc[j];
                        }

                        trackFile.Save();
                        currentTrackIndex++;
                    }
                    catch (Exception e2)
                    {
                        MessageBox.Show("Writing information for track " + (currentTrackIndex + 1).ToString() + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
