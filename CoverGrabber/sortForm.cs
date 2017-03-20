using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CoverGrabber
{
    public partial class SortForm : Form
    {
        public List<string> Files;

        public SortForm()
        {
            InitializeComponent();
        }

        public SortForm(List<string> fileList)
        {
            InitializeComponent();
            fileLv.Clear();
            fileLv.BeginUpdate();
            fileLv.Columns.Add("Path", 690);
            fileLv.Columns.Add("Size", 60, HorizontalAlignment.Right);
            Files = fileList;
            foreach (var filePath in fileList)
            {
                ListViewItem lvi = new ListViewItem {Text = filePath};
                lvi.SubItems.Add(((new FileInfo(filePath).Length) / (1024 * 1024 * 1.0)).ToString("F2") + " MB");
                fileLv.Items.Add(lvi);
            }
            fileLv.EndUpdate();
        }

        private void TopB_Click(object sender, EventArgs e)
        {
            fileLv.BeginUpdate();
            if (fileLv.SelectedItems.Count != 0)
            {
                int i = fileLv.SelectedItems.Count - 1;
                int j = fileLv.SelectedItems.Count - 1;
                for (; j >= 0; j--)
                {
                    ListViewItem lvi = fileLv.SelectedItems[i];
                    fileLv.Items.MoveItem(0, lvi);
                }
            }
            fileLv.EndUpdate();
            fileLv.Focus();
        }

        private void UpB_Click(object sender, EventArgs e)
        {
            fileLv.BeginUpdate();
            if (fileLv.SelectedItems.Count != 0)
            {
                int i = fileLv.SelectedItems.Count - 1;
                int j = fileLv.SelectedItems.Count - 1;
                int newPos = fileLv.SelectedItems[0].Index - 1;
                for (; j >= 0; j--)
                {
                    ListViewItem lvi = fileLv.SelectedItems[i];
                    fileLv.Items.MoveItem(newPos, lvi);
                }
            }
            fileLv.EndUpdate();
            fileLv.Focus();
        }

        private void DownB_Click(object sender, EventArgs e)
        {
            fileLv.BeginUpdate();
            if (fileLv.SelectedItems.Count != 0)
            {
                int i = 0;
                int j = 0;
                int newPos = fileLv.SelectedItems[fileLv.SelectedItems.Count - 1].Index + 1;
                for (; j <= fileLv.SelectedItems.Count - 1; j++)
                {
                    ListViewItem lvi = fileLv.SelectedItems[i];
                    fileLv.Items.MoveItem(newPos, lvi);
                }
            }
            fileLv.EndUpdate();
            fileLv.Focus();
        }

        private void BottomB_Click(object sender, EventArgs e)
        {
            fileLv.BeginUpdate();
            if (fileLv.SelectedItems.Count != 0)
            {
                int i = 0;
                int j = 0;
                for (; j <= fileLv.SelectedItems.Count - 1; j++)
                {
                    ListViewItem lvi = fileLv.SelectedItems[i];
                    fileLv.Items.MoveItem(fileLv.Items.Count - 1, lvi);
                }
            }
            fileLv.EndUpdate();
            fileLv.Focus();
        }

        private void fileLv_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void OkB_Click(object sender, EventArgs e)
        {
            List<string> t = (from ListViewItem lvi in fileLv.Items select lvi.Text).ToList();
            Files = t;
            Close();
        }

        private void CancelB_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
    public static class ListViewExtension
    {
        public static void MoveItem(this ListView.ListViewItemCollection lviS, int newPos, ListViewItem lvi)
        {
            lviS.Remove(lvi);
            lviS.Insert(newPos, lvi);
        }
    }
}
