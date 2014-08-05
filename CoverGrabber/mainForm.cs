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
                this.folder.Text = this.fbd.SelectedPath;
            }
        }

        private void coverC_CheckedChanged(object sender, EventArgs e)
        {
            this.resizeSize.Enabled = this.coverC.Checked;
        }

        private void goB_Click(object sender, EventArgs e)
        {
            HtmlAgilityPack.HtmlDocument htmlPageContent;

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

            if (!this.coverC.Checked && !this.Id3C.Checked && !this.lyricC.Checked)
            {
                MessageBox.Show("You haven't pick any task.");
                return;
            }

            #region Check local folder and generate files list
            this.tssP.Value = 0;
            this.tssL.Text = "Getting information for local tracks...";
            string[] folderLists = this.folder.Text.Split(";".ToCharArray());
            foreach (string singleFolder in folderLists)
            {
                if (!Directory.Exists(singleFolder))
                {
                    MessageBox.Show("Folder " + singleFolder + " doesn't exist.");
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
                string htmlContent = Utility.downloadPage(this.url.Text);
                htmlPageContent = new HtmlAgilityPack.HtmlDocument();
                htmlPageContent.LoadHtml(htmlContent);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Accessing page " + url + " failed.");
                return;
            }

            #endregion Get remote page

            #region Generate track lists
            this.tssP.Value = 20;
            this.tssL.Text = "Getting tracks list...";
            try
            {
                trackNamesByDiscs = Utility.parseTrackList(htmlPageContent);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Parsing track lists from " + url + " failed.");
                return;
            }

            #endregion Generate track lists

            foreach (ArrayList remoteTrackList in trackNamesByDiscs)
            {
                remoteTrackQuantity += remoteTrackList.Count;
            }
            localTrackQuantity = fileList.Count;
            if (remoteTrackQuantity != localTrackQuantity)
            {
                MessageBox.Show("You have " + localTrackQuantity.ToString() + " tracks in local folder(s), but " + remoteTrackQuantity.ToString() + " tracks in remote page.");
                return;
            }

            #region Get cover
            if (this.coverC.Checked)
            {
                this.tssP.Value = 30;
                this.tssL.Text = "Getting cover image...";
                try
                {
                    largeTempFile = System.IO.Path.GetTempFileName() + ".jpg";
                    smallTempFile = System.IO.Path.GetTempFileName() + ".jpg";
                    Utility.downloadFile(Utility.parseCoverAddress(htmlPageContent), largeTempFile);
                    Utility.resizeImage(largeTempFile, smallTempFile, (int)this.resizeSize.Value);
                    this.coverP.ImageLocation = smallTempFile;
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Downloading cover failed.");
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
                    artistNamesByDiscs = Utility.parseTrackArtistList(htmlPageContent);
                    this.trackT.Clear();
                    foreach (ArrayList trackList in trackNamesByDiscs)
                    {
                        foreach (string track in trackList)
                        {
                            this.trackT.AppendText(track + "\n");
                        }
                    }
                    albumTitle = Utility.parseTitle(htmlPageContent);
                    albumArtistName = Utility.parseArtist(htmlPageContent);
                    albumYear = Utility.parseYear(htmlPageContent);

                    this.titleL.Text = albumTitle;
                    this.artiseL.Text = albumArtistName;
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Parsing page failed.");
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
                    ArrayList trackUrlListByDiscs = Utility.parseTrackUrlList(htmlPageContent);

                    foreach (ArrayList trackUrlList in trackUrlListByDiscs)
                    {
                        ArrayList lyricInDisc = new ArrayList();
                        foreach (string trackUrl in trackUrlList)
                        {
                            try
                            {
                                string lyric = "";
                                this.tssP.Value = 50 + (int)(40.0 * currentTrackIndex / remoteTrackQuantity);
                                this.tssL.Text = "Getting lyric for track " + (currentTrackIndex + 1).ToString() + "...";
                                if (trackUrl != "")
                                {
                                    string htmlContent = Utility.downloadPage("http://www.xiami.com" + trackUrl);
                                    HtmlAgilityPack.HtmlDocument lyricPageContent = new HtmlAgilityPack.HtmlDocument();
                                    lyricPageContent.LoadHtml(htmlContent);

                                    // If code exists
                                    while (lyricPageContent.DocumentNode.SelectSingleNode("//img[@id=\"J_CheckCode\"]") != null ||
                                        lyricPageContent.DocumentNode.SelectSingleNode("//p[@id=\"youxianchupin\"]") != null)
                                    {
                                        VerifyCode verifyCode = Utility.getVerifyCode(lyricPageContent);
                                        this.verifyCodeP.ImageLocation = verifyCode.localVerifyCode;

                                        string verifyCodeText = Microsoft.VisualBasic.Interaction.InputBox("Enter the verify code", "Verify Code", "");

                                        verifyCode.code = verifyCodeText;

                                        Utility.postVerifyCode(verifyCode);

                                        htmlContent = Utility.downloadPage("http://www.xiami.com" + trackUrl);
                                        lyricPageContent = new HtmlAgilityPack.HtmlDocument();
                                        lyricPageContent.LoadHtml(htmlContent);
                                    }

                                    lyric = Utility.parseTrackLyric(lyricPageContent);
                                    this.trackT.AppendText(lyric);
                                    System.Threading.Thread.Sleep(1000);
                                }
                                currentTrackIndex++;
                                lyricInDisc.Add(lyric);
                            }
                            catch (Exception e2)
                            {
                                MessageBox.Show("Downloading lyrics for track " + (currentTrackIndex + 1).ToString() + " failed.");
                                return;
                            }
                        }
                        currentDiscIndex++;
                        lyricsByDiscs.Add(lyricInDisc);
                    }
                }
                catch (Exception e1)
                {
                    MessageBox.Show("Downloading lyrics from page failed.");
                    return;
                }
            }
            #endregion Get lyrics

            this.extractC.Enabled = false;
            this.coverC.Enabled = false;
            this.Id3C.Enabled = false;
            this.resizeSize.Enabled = false;
            this.lyricC.Enabled = false;
            this.goB.Enabled = false;
            this.folderB.Enabled = false;

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

                    TagLib.File file = TagLib.File.Create((string)fileList[currentTrackIndex]);

                    if (this.Id3C.Checked)
                    {
                        string currentTrackName = (string)tracksInDisc[j];
                        string currentTrackArtist = (string)trackArtistsInDisc[j];

                        if (trackNamesByDiscs.Count == 1)
                        {
                            file.Tag.Album = albumTitle;
                        }
                        else
                        {
                            file.Tag.Album = albumTitle + " Disc " + (i + 1).ToString();
                            file.Tag.Disc = (uint)(i + 1);
                            file.Tag.DiscCount = (uint)(trackNamesByDiscs.Count);
                        }
                        file.Tag.Track = (uint)(j + 1);
                        file.Tag.TrackCount = (uint)(tracksInDisc.Count);
                        file.Tag.AlbumArtists = albumArtistName.Split(new char[] { ';' });
                        if (currentTrackArtist != "")
                        {
                            file.Tag.Performers = currentTrackArtist.Split(new char[] { ';' });
                        }
                        else
                        {
                            file.Tag.Performers = "".Split(new char[] { ';' });
                        }
                        file.Tag.Title = currentTrackName;
                        file.Tag.Year = albumYear;
                    }

                    if (this.coverC.Checked)
                    {
                        List<Picture> pictureList = new List<Picture>();
                        pictureList.Add(new Picture(smallTempFile));
                        file.Tag.Pictures = pictureList.ToArray();
                    }

                    if (this.lyricC.Checked)
                    {
                        file.Tag.Lyrics = (string)lyricsInDisc[j];
                    }

                    file.Save();
                    currentTrackIndex++;
                }
            }

            this.coverC.Enabled = true;
            this.Id3C.Enabled = true;
            this.resizeSize.Enabled = true;
            this.lyricC.Enabled = true;
            this.goB.Enabled = true;
            this.folderB.Enabled = true;
            MessageBox.Show("Done.");
        }
    }
}
