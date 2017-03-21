using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CoverGrabber
{
    public partial class SortResultForm : Form
    {
        public bool Accepted;

        public SortResultForm(List<string> fileList, List<List<string>> trackNamesByDiscs, Dictionary<string, Tuple<int, int>> localToRemoteMap)
        {
            InitializeComponent();
            sortListView.BeginUpdate();
            foreach (string localFile in fileList)
            {
                Tuple<int, int> map = localToRemoteMap[localFile];
                string remoteTrack = trackNamesByDiscs[map.Item1][map.Item2];

                sortListView.Items.Add(new ListViewItem(new [] {localFile, remoteTrack}));
            }
            sortListView.EndUpdate();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Accepted = true;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Accepted = false;
            Close();
        }
    }
}
