using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CoverGrabber.Site;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace CoverGrabber
{
    public partial class MainForm : Form
    {
        List<string> _sortedFileList = new List<string>();
        Dictionary<string, ISite> _supportedSites = new Dictionary<string, ISite>();

        public MainForm()
        {
            InitializeComponent();

            List<ISite> availableSites = new List<ISite>
            {
                new SiteXiami(),
                new SiteItunes(),
                new SiteLastFm(),
                new SiteMusicBrainz(),
                new SiteNetease(),
                new SiteVgmdb(),
            };

            foreach (ISite site in availableSites)
            {
                foreach (string supportedSite in site.SupportedHost)
                {
                    _supportedSites.Add(supportedSite, site);
                }
            }
        }

        private void folderB_Click(object sender, EventArgs e)
        {
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                if (folder.Text != string.Empty)
                {
                    DialogResult dr = MessageBox.Show("Do you want to replace the folder text box, or append it to the list?\nYes: Replace\nNo: Append to the list", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            {
                                folder.Text = fbd.SelectedPath;
                                break;
                            }
                        case DialogResult.No:
                            {
                                folder.Text = (folder.Text == string.Empty ? string.Empty : folder.Text + "; ") + fbd.SelectedPath;
                                break;
                            }
                    }
                }
                else
                {
                    folder.Text = fbd.SelectedPath;
                }
            }
        }

        private void coverC_CheckedChanged(object sender, EventArgs e)
        {
            resizeSize.Enabled = coverC.Checked;
        }

        private void goB_Click(object sender, EventArgs e)
        {
            GrabOptions options = new GrabOptions
            {
                SiteInterface = null,
                LocalFolder = folder.Text,
                WebPageUrl = url.Text,
                NeedCover = coverC.Checked,
                ResizeSize = (int)resizeSize.Value,
                NeedId3 = id3C.Checked,
                NeedLyric = lyricC.Checked
            };
            if (sAutoRs.Checked)
            {
                options.SortMode = EnumSortMode.Auto;
            }
            else if (sNaturallyRs.Checked)
            {
                options.SortMode = EnumSortMode.Natural;
            }
            else if (sManuallyRs.Checked)
            {
                options.SortMode = EnumSortMode.Manual;
            }
            options.FileList = _sortedFileList;

            try
            {
                Utility.InitializeEnvironment(ref options, _supportedSites);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Not supported site.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!options.NeedCover && !options.NeedId3 && !options.NeedLyric)
            {
                MessageBox.Show("You haven't pick any task.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            folder.Enabled = false;
            folderB.Enabled = false;
            url.Enabled = false;
            coverC.Enabled = false;
            resizeSize.Enabled = false;
            id3C.Enabled = false;
            lyricC.Enabled = false;
            goB.Enabled = false;
            sNaturallyRs.Enabled = false;
            sAutoRs.Enabled = false;
            sManuallyRs.Enabled = false;
            sortB.Enabled = false;


            bw.RunWorkerAsync(options);
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            DoGrab(e.Argument);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != -1)
            {
                tssP.Value = e.ProgressPercentage;
            }
            ProgressOptions progressOptions = (ProgressOptions)e.UserState;

            if (progressOptions.StatusMessage != null)
            {
                tssL.Text = progressOptions.StatusMessage;
            }

            switch (progressOptions.ObjectName)
            {
                case (EnumProgressReportObject.AlbumTitle):
                    {
                        titleL.Text = progressOptions.ObjectValue;
                        break;
                    }
                case (EnumProgressReportObject.AlbumArtist):
                    {
                        artiseL.Text = progressOptions.ObjectValue;
                        break;
                    }
                case (EnumProgressReportObject.AlbumCover):
                    {
                        coverP.ImageLocation = progressOptions.ObjectValue;
                        break;
                    }
                case (EnumProgressReportObject.Text):
                    {
                        trackT.AppendText(progressOptions.ObjectValue);
                        break;
                    }
                case (EnumProgressReportObject.TextClear):
                    {
                        trackT.Clear();
                        break;
                    }
                case (EnumProgressReportObject.VerifyCode):
                    {
                        verifyCodeP.ImageLocation = progressOptions.ObjectValue;
                        break;
                    }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            folder.Enabled = true;
            folderB.Enabled = true;
            url.Enabled = true;
            coverC.Enabled = true;
            resizeSize.Enabled = true;
            id3C.Enabled = true;
            lyricC.Enabled = true;
            goB.Enabled = true;
            sNaturallyRs.Enabled = true;
            sAutoRs.Enabled = true;
            sManuallyRs.Enabled = true;
            sortB.Enabled = sManuallyRs.Checked;
        }

        private void DoGrab(object grabOptions)
        {
            GrabOptions options = (GrabOptions)grabOptions;
            List<string> fileList = new List<string>();
            CleanProgress();

            try
            {
                SetProgress(0, "Getting information for local tracks...", EnumProgressReportObject.Skip, string.Empty);
                switch (options.SortMode)
                {
                    case EnumSortMode.Auto:
                    case EnumSortMode.Natural:
                        fileList = Utility.GenerateFileList(options.LocalFolder);
                        break;
                    case EnumSortMode.Manual:
                        fileList = options.FileList;
                        break;
                }
                SetProgress(10, "Getting remote page information...", EnumProgressReportObject.Skip, string.Empty);
                HtmlDocument albumPage = Utility.DownloadPage(options.SiteInterface.ConvertAlbumUrl(options.WebPageUrl), options.SiteInterface);

                SetProgress(20, "Getting album info...", EnumProgressReportObject.Skip, string.Empty);
                AlbumInfo albumInfo = options.SiteInterface.ParseAlbum(albumPage);

                SetProgress(30, "Getting tracks info...", new Dictionary<EnumProgressReportObject, string>
                    {
                        {EnumProgressReportObject.TextClear, string.Empty},
                        {EnumProgressReportObject.Text, string.Join(Environment.NewLine, albumInfo.TrackNamesByDiscs.Select(x => string.Join(Environment.NewLine, x)))},
                        {EnumProgressReportObject.AlbumTitle, albumInfo.AlbumTitle},
                        {EnumProgressReportObject.AlbumArtist, albumInfo.AlbumArtistName}
                    });

                if (fileList.Count != albumInfo.TrackNamesByDiscs.Sum(x => x.Count))
                {
                    throw new FileCountNotMatchException(fileList.Count, albumInfo.TrackNamesByDiscs.Sum(x => x.Count));
                }

                // Notice fileList after this step is already sorted.
                if (options.SortMode == EnumSortMode.Auto && !Utility.TryToMatchFiles(ref fileList, albumInfo.TrackNamesByDiscs))
                {
                    throw new FileMatchException();
                }

                SetProgress(40, "Getting cover image...", EnumProgressReportObject.Skip, string.Empty);
                if (options.NeedCover)
                {
                    albumInfo.NeedCover = true;
                    albumInfo.CoverImagePath = Utility.GenerateCover(albumInfo.CoverImagePath, options.ResizeSize);
                    SetProgress(-1, null, EnumProgressReportObject.AlbumCover, albumInfo.CoverImagePath);
                }

                if (options.NeedLyric)
                {
                    albumInfo.NeedLyric = true;
                    Utility.DownloadLyrics(ref albumInfo, options.SiteInterface, SetProgress);
                }

                SetProgress(90, "Writing local files...", EnumProgressReportObject.Skip, string.Empty);
                Utility.WriteFiles(albumInfo, fileList, options, SetProgress);

                SetProgress(100, "Done", EnumProgressReportObject.Skip, string.Empty);
                MessageBox.Show("Done.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (FileCountNotMatchException ex)
            {
                MessageBox.Show($"You have {ex.LocalCount} tracks in local folder(s), but {ex.RemoteCount} tracks on album page.", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                CleanProgress();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show($"Folder {options.LocalFolder} doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress();
            }
            catch (FileMatchException)
            {
                MessageBox.Show("Auto file match failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress();
            }
            catch (DownloadCoverException)
            {
                MessageBox.Show("Download cover image filed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress();
            }
            catch (WritingFileException ex)
            {
                MessageBox.Show($"Writing information for track {ex.TrackNumber} failed.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress();
            }
            catch (DownloadLyricException ex)
            {
                MessageBox.Show($"Downloading lyrics for track {ex.TrackNumber} failed.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CleanProgress();
            }
        }

        private void SetProgress(int progress, string statusMessage, EnumProgressReportObject objectName, string objectValue)
        {
            ProgressOptions progressOptions = new ProgressOptions
            {
                StatusMessage = statusMessage,
                ObjectName = objectName,
                ObjectValue = objectValue
            };
            bw.ReportProgress(progress, progressOptions);
        }

        private void SetProgress(int progress, string statusMessage, Dictionary<EnumProgressReportObject, string> parameters)
        {
            foreach (var kvp in parameters)
            {
                ProgressOptions progressOptions = new ProgressOptions
                {
                    StatusMessage = statusMessage,
                    ObjectName = kvp.Key,
                    ObjectValue = kvp.Value
                };
                bw.ReportProgress(progress, progressOptions);
            }
        }

        private void CleanProgress()
        {
            SetProgress(0, string.Empty, EnumProgressReportObject.Skip, string.Empty);
        }

        private void sNatuallyRs_CheckedChanged(object sender, EventArgs e)
        {
            sortB.Enabled = !sNaturallyRs.Checked;
        }

        private void sAutoRs_CheckedChanged(object sender, EventArgs e)
        {
            sortB.Enabled = !sAutoRs.Checked;
        }

        private void sManuallyRs_CheckedChanged(object sender, EventArgs e)
        {
            sortB.Enabled = sManuallyRs.Checked;
        }

        private void mainForm_Load(object sender, EventArgs e)
        {

        }

        private void sortB_Click(object sender, EventArgs e)
        {
            try
            {
                if (_sortedFileList == null)
                {
                    _sortedFileList = Utility.GenerateFileList(folder.Text);
                }
            }
            catch (DirectoryNotFoundException e1)
            {
                MessageBox.Show($"Folder {e1.Message} doesn't exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SortForm sf = new SortForm(_sortedFileList);
            sf.ShowDialog();
            _sortedFileList = sf.Files;
        }

        private void folder_TextChanged(object sender, EventArgs e)
        {
            sNaturallyRs.Checked = true;
            sortB.Enabled = false;
            _sortedFileList = null;
        }
    }
}
