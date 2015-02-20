using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace CoverGrabber
{
    public partial class sortForm : Form
    {
        public List<string> files;

        public sortForm()
        {
            InitializeComponent();
        }

        public sortForm(List<string> fileList)
        {
            InitializeComponent();
            this.fileLv.Clear();
            this.fileLv.BeginUpdate();
            this.fileLv.Columns.Add("Path", 690);
            this.fileLv.Columns.Add("Size", 60, HorizontalAlignment.Right);
            this.files = fileList;
            foreach (var filePath in fileList)
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = filePath;
                lvi.SubItems.Add(((new FileInfo(filePath).Length) / (1024 * 1024 * 1.0)).ToString("F2") + " MB");
                this.fileLv.Items.Add(lvi);
            }
            this.fileLv.EndUpdate();
        }

        private void TopB_Click(object sender, EventArgs e)
        {
            this.fileLv.BeginUpdate();
            if (this.fileLv.SelectedItems.Count != 0)
            {
                int i = this.fileLv.SelectedItems.Count - 1;
                int j = this.fileLv.SelectedItems.Count - 1;
                for (; j >= 0; j--)
                {
                    ListViewItem lvi = this.fileLv.SelectedItems[i];
                    this.fileLv.Items.MoveItem(0, lvi);
                }
            }
            this.fileLv.EndUpdate();
            this.fileLv.Focus();
        }

        private void UpB_Click(object sender, EventArgs e)
        {
            this.fileLv.BeginUpdate();


            if (this.fileLv.SelectedItems.Count != 0 && this.fileLv.SelectedItems[0].Index > 0)
            {
                
                int i = this.fileLv.SelectedItems.Count - 1;
                int j = this.fileLv.SelectedItems.Count - 1;
                int newPos = this.fileLv.SelectedItems[0].Index - 1;
                for (; j >= 0; j--)
                {
                    ListViewItem lvi = this.fileLv.SelectedItems[i];
                    this.fileLv.Items.MoveItem(newPos, lvi);
                }
            }
            this.fileLv.EndUpdate();
            this.fileLv.Focus();
        }

        private void DownB_Click(object sender, EventArgs e)
        {
            this.fileLv.BeginUpdate();

            if (this.fileLv.SelectedItems.Count != 0 && this.fileLv.SelectedItems[0].Index < this.fileLv.Items.Count - 1)
            {
                int i = 0;
                int j = 0;
                int newPos = this.fileLv.SelectedItems[this.fileLv.SelectedItems.Count - 1].Index + 1;
                for (; j <= this.fileLv.SelectedItems.Count - 1; j++)
                {
                    ListViewItem lvi = this.fileLv.SelectedItems[i];
                    this.fileLv.Items.MoveItem(newPos, lvi);
                }
            }
            this.fileLv.EndUpdate();
            this.fileLv.Focus();
        }

        private void BottomB_Click(object sender, EventArgs e)
        {
            this.fileLv.BeginUpdate();
            if (this.fileLv.SelectedItems.Count != 0)
            {
                int i = 0;
                int j = 0;
                for (; j <= this.fileLv.SelectedItems.Count - 1; j++)
                {
                    ListViewItem lvi = this.fileLv.SelectedItems[i];
                    this.fileLv.Items.MoveItem(this.fileLv.Items.Count - 1, lvi);
                }
            }
            this.fileLv.EndUpdate();
            this.fileLv.Focus();
        }

        private void fileLv_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void OkB_Click(object sender, EventArgs e)
        {
            List<string> t = new List<string>();
            foreach (ListViewItem lvi in this.fileLv.Items)
            {
                t.Add(lvi.Text);
            }
            this.files = t;
            this.Close();
        }

        private void CancelB_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
    public static class LVExtension
    {
        public static void MoveItem(this ListView.ListViewItemCollection lviS, int NewPos, ListViewItem lvi)
        {
            lviS.Remove(lvi);
            lviS.Insert(NewPos, lvi);
        }
    }
}
