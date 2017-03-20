using System.ComponentModel;
using System.Windows.Forms;

namespace CoverGrabber
{
    partial class SortForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.fileLv = new System.Windows.Forms.ListView();
            this.TopB = new System.Windows.Forms.Button();
            this.UpB = new System.Windows.Forms.Button();
            this.DownB = new System.Windows.Forms.Button();
            this.BottomB = new System.Windows.Forms.Button();
            this.OkB = new System.Windows.Forms.Button();
            this.CancelB = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fileLv
            // 
            this.fileLv.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileLv.FullRowSelect = true;
            this.fileLv.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.fileLv.Location = new System.Drawing.Point(13, 13);
            this.fileLv.Name = "fileLv";
            this.fileLv.Size = new System.Drawing.Size(778, 409);
            this.fileLv.TabIndex = 0;
            this.fileLv.UseCompatibleStateImageBehavior = false;
            this.fileLv.View = System.Windows.Forms.View.Details;
            this.fileLv.SelectedIndexChanged += new System.EventHandler(this.fileLv_SelectedIndexChanged);
            // 
            // TopB
            // 
            this.TopB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TopB.Location = new System.Drawing.Point(797, 12);
            this.TopB.Name = "TopB";
            this.TopB.Size = new System.Drawing.Size(75, 23);
            this.TopB.TabIndex = 1;
            this.TopB.Text = "Top";
            this.TopB.UseVisualStyleBackColor = true;
            this.TopB.Click += new System.EventHandler(this.TopB_Click);
            // 
            // UpB
            // 
            this.UpB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.UpB.Location = new System.Drawing.Point(797, 41);
            this.UpB.Name = "UpB";
            this.UpB.Size = new System.Drawing.Size(75, 23);
            this.UpB.TabIndex = 2;
            this.UpB.Text = "Up";
            this.UpB.UseVisualStyleBackColor = true;
            this.UpB.Click += new System.EventHandler(this.UpB_Click);
            // 
            // DownB
            // 
            this.DownB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DownB.Location = new System.Drawing.Point(797, 70);
            this.DownB.Name = "DownB";
            this.DownB.Size = new System.Drawing.Size(75, 23);
            this.DownB.TabIndex = 3;
            this.DownB.Text = "Down";
            this.DownB.UseVisualStyleBackColor = true;
            this.DownB.Click += new System.EventHandler(this.DownB_Click);
            // 
            // BottomB
            // 
            this.BottomB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BottomB.Location = new System.Drawing.Point(797, 99);
            this.BottomB.Name = "BottomB";
            this.BottomB.Size = new System.Drawing.Size(75, 23);
            this.BottomB.TabIndex = 4;
            this.BottomB.Text = "Bottom";
            this.BottomB.UseVisualStyleBackColor = true;
            this.BottomB.Click += new System.EventHandler(this.BottomB_Click);
            // 
            // OkB
            // 
            this.OkB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OkB.Location = new System.Drawing.Point(635, 428);
            this.OkB.Name = "OkB";
            this.OkB.Size = new System.Drawing.Size(75, 23);
            this.OkB.TabIndex = 5;
            this.OkB.Text = "OK";
            this.OkB.UseVisualStyleBackColor = true;
            this.OkB.Click += new System.EventHandler(this.OkB_Click);
            // 
            // CancelB
            // 
            this.CancelB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelB.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelB.Location = new System.Drawing.Point(716, 428);
            this.CancelB.Name = "CancelB";
            this.CancelB.Size = new System.Drawing.Size(75, 23);
            this.CancelB.TabIndex = 6;
            this.CancelB.Text = "Cancel";
            this.CancelB.UseVisualStyleBackColor = true;
            this.CancelB.Click += new System.EventHandler(this.CancelB_Click);
            // 
            // sortForm
            // 
            this.AcceptButton = this.OkB;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.CancelB;
            this.ClientSize = new System.Drawing.Size(884, 461);
            this.Controls.Add(this.CancelB);
            this.Controls.Add(this.OkB);
            this.Controls.Add(this.BottomB);
            this.Controls.Add(this.DownB);
            this.Controls.Add(this.UpB);
            this.Controls.Add(this.TopB);
            this.Controls.Add(this.fileLv);
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "sortForm";
            this.Text = "Sorting";
            this.ResumeLayout(false);

        }

        #endregion

        private ListView fileLv;
        private Button TopB;
        private Button UpB;
        private Button DownB;
        private Button BottomB;
        private Button OkB;
        private Button CancelB;
    }
}