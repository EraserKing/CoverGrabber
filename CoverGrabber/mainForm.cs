using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
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

        private void cleanUpStatus()
        {
            this.tssP.Value = 0;
            this.tssL.Text = "";
        }

        private void coverC_CheckedChanged(object sender, EventArgs e)
        {
            this.resizeSize.Enabled = this.coverC.Checked;
        }

        private void goB_Click(object sender, EventArgs e)
        {
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
            this.cleanUpStatus();
            if (!this.coverC.Checked && !this.Id3C.Checked && !this.lyricC.Checked)
            {
                MessageBox.Show("You haven't pick any task.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            #endregion Preparation work

            #region Check local folder and generate files list

            this.tssP.Value = 0;
            this.tssL.Text = "Getting information for local tracks...";
            string[] folderLists = this.folder.Text.Split(";".ToCharArray());
            foreach (string singleFolder in folderLists)
            {
                if (!Directory.Exists(singleFolder))
                {
                    MessageBox.Show("Folder " + singleFolder + " doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.cleanUpStatus();
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
            this.tssP.Value = 10;
            this.tssL.Text = "Getting remote page information...";
            try
            {
                string htmlContent = Utility.DownloadPage(this.url.Text);
                albumPage = new HtmlAgilityPack.HtmlDocument();
                albumPage.LoadHtml(htmlContent);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Accessing album page " + url.Text + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cleanUpStatus();
                return;
            }
            #endregion Get remote page

            #region Generate track lists
            this.tssP.Value = 20;
            this.tssL.Text = "Getting tracks list...";
            try
            {
                trackNamesByDiscs = Utility.ParseTrackList(albumPage);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Parsing track lists from " + url.Text + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cleanUpStatus();
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
                this.cleanUpStatus();
                return;
            }
            #endregion Compare if quantity tracks in local and on page equal

            #region Get cover
            if (this.coverC.Checked)
            {
                this.tssP.Value = 30;
                this.tssL.Text = "Getting cover image...";
                try
                {
                    string remoteCoverUrl = Utility.ParseCoverAddress(albumPage);
                    largeTempFile = System.IO.Path.GetTempPath() + System.IO.Path.GetFileName(remoteCoverUrl) + ".jpg";
                    smallTempFile = System.IO.Path.GetTempPath() + System.IO.Path.GetFileName(remoteCoverUrl) + "s.jpg";
                    Utility.DownloadFile(remoteCoverUrl, largeTempFile);
                    Utility.ResizeImage(largeTempFile, smallTempFile, (int)this.resizeSize.Value);
                    this.coverP.ImageLocation = smallTempFile;
                    this.coverP.Refresh();
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Downloading cover failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.cleanUpStatus();
                    return;
                }
            }
            #endregion Get cover

            #region Get ID3
            if (this.Id3C.Checked)
            {
                this.tssP.Value = 40;
                this.tssL.Text = "Getting ID3 information...";
                try
                {
                    this.trackT.Clear();

                    artistNamesByDiscs = Utility.ParseTrackArtistList(albumPage);
                    foreach (ArrayList trackList in trackNamesByDiscs)
                    {
                        foreach (string track in trackList)
                        {
                            this.trackT.AppendText(track + "\n");
                        }
                    }
                    albumTitle = Utility.parseTitle(albumPage);
                    albumArtistName = Utility.parseArtist(albumPage);
                    albumYear = Utility.parseYear(albumPage);

                    this.titleL.Text = albumTitle;
                    this.artiseL.Text = albumArtistName;

                    this.titleL.Refresh();
                    this.artiseL.Refresh();
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Parsing page failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.cleanUpStatus();
                    return;
                }
            }
            #endregion Get ID3

            #region Get lyrics
            if (this.lyricC.Checked)
            {
                this.tssP.Value = 50;
                this.tssL.Text = "Getting lyrics...";

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
                                this.tssP.Value = 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity);
                                this.tssL.Text = "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...";
                                this.sts.Refresh();
                                if (trackUrl != "")
                                {
                                    string trackHtmlContent = Utility.DownloadPage("http://www.xiami.com" + trackUrl);
                                    HtmlAgilityPack.HtmlDocument trackPage = new HtmlAgilityPack.HtmlDocument();
                                    trackPage.LoadHtml(trackHtmlContent);

                                    // If code exists, or it's an error page, keep asking verify code, until it's correct, or user entered nothing to break
                                    while (trackPage.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]") != null ||
                                        trackPage.DocumentNode.SelectSingleNode("//p[@id=\"youxianchupin\"]") != null)
                                    {
                                        VerifyCode verifyCode = Utility.GetVerifyCode(trackPage);
                                        this.verifyCodeP.ImageLocation = verifyCode.localVerifyCode;
                                        this.verifyCodeP.Refresh();

                                        string verifyCodeText = Microsoft.VisualBasic.Interaction.InputBox("Enter the verify code", "Verify Code", "");

                                        if (verifyCodeText == "")
                                        {
                                            MessageBox.Show("You aborted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                            this.cleanUpStatus();
                                            return;
                                        }
                                        else
                                        {
                                            verifyCode.code = verifyCodeText;

                                            Utility.PostVerifyCode(verifyCode);

                                            // After posting, reload track page and see if everything goes fine
                                            trackHtmlContent = Utility.DownloadPage("http://www.xiami.com" + trackUrl);
                                            trackPage = new HtmlAgilityPack.HtmlDocument();
                                            trackPage.LoadHtml(trackHtmlContent);
                                        }
                                    }
                                    lyric = Utility.parseTrackLyric(trackPage);
                                    if (lyric != "")
                                    {
                                        this.trackT.AppendText("\n\nFirst line of lyric for track " + (currentTrackIndex + 1).ToString() + ":\n");
                                        this.trackT.AppendText(lyric.Split("\n".ToCharArray())[0]); // Just show first line
                                    }
                                    System.Threading.Thread.Sleep(500);
                                }
                                currentTrackIndex++;
                                lyricInDisc.Add(lyric);
                            }
                            catch (Exception e2)
                            {
                                MessageBox.Show("Downloading lyrics for track " + (currentTrackIndex + 1).ToString() + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                this.cleanUpStatus();
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
                    this.cleanUpStatus();
                    return;
                }
            }
            #endregion Get lyrics

            #region Ready to start write file
            this.extractC.Enabled = false;
            this.coverC.Enabled = false;
            this.Id3C.Enabled = false;
            this.resizeSize.Enabled = false;
            this.lyricC.Enabled = false;
            this.goB.Enabled = false;
            this.folderB.Enabled = false;
            #endregion Ready to start write file

            #region Write file
            currentDiscIndex = 0;
            currentTrackIndex = 0;
            this.tssP.Value = 90;
            this.tssL.Text = "Writing local files...";

            for (int i = 0; i < trackNamesByDiscs.Count; i++)
            {
                ArrayList tracksInDisc = (ArrayList)trackNamesByDiscs[i];
                ArrayList trackArtistsInDisc = new ArrayList();
                ArrayList lyricsInDisc = new ArrayList();

                if (this.Id3C.Checked)
                {
                    trackArtistsInDisc = (ArrayList)artistNamesByDiscs[i];
                }
                if (this.lyricC.Checked)
                {
                    lyricsInDisc = (ArrayList)lyricsByDiscs[i];
                }

                for (int j = 0; j < tracksInDisc.Count; j++)
                {
                    this.tssP.Value = 90 + (int)(10.0 * currentTrackIndex / localTrackQuantity);
                    this.tssL.Text = "Writing track " + (currentTrackIndex + 1).ToString() + " ...";
                    this.sts.Refresh();

                    try
                    {
                        TagLib.File trackFile = TagLib.File.Create((string)fileList[currentTrackIndex]);

                        //trackFile.RemoveTags(TagTypes.AllTags);
                        TagLib.Id3v2.Tag.DefaultVersion = 3;
                        TagLib.Id3v2.Tag.ForceDefaultVersion = true;

                        if (this.Id3C.Checked)
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
                                trackFile.Tag.Performers = "".Split(";".ToCharArray());
                            }
                            trackFile.Tag.Title = currentTrackName;
                            trackFile.Tag.Year = albumYear;
                        }

                        if (this.coverC.Checked)
                        {
                            List<Picture> coverImageList = new List<Picture>();
                            coverImageList.Add(new Picture(smallTempFile));
                            trackFile.Tag.Pictures = coverImageList.ToArray();
                        }

                        if (this.lyricC.Checked)
                        {
                            trackFile.Tag.Lyrics = (string)lyricsInDisc[j];
                        }

                        trackFile.Save();
                        currentTrackIndex++;
                    }
                    catch (Exception e2)
                    {
                        MessageBox.Show("Writing information for track " + (currentTrackIndex + 1).ToString() + " failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.cleanUpStatus();
                        break;
                    }
                }
            }
            #endregion Write file

            #region Clean up
            this.tssP.Value = 100;
            this.tssL.Text = "Done";
            this.sts.Refresh();

            this.coverC.Enabled = true;
            this.Id3C.Enabled = true;
            this.resizeSize.Enabled = true;
            this.lyricC.Enabled = true;
            this.goB.Enabled = true;
            this.folderB.Enabled = true;
            MessageBox.Show("Done.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            #endregion Clean up
        }
    }
}
