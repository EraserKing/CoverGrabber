namespace CoverGrabber
{
    partial class mainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.folderL = new System.Windows.Forms.Label();
            this.folder = new System.Windows.Forms.TextBox();
            this.url = new System.Windows.Forms.TextBox();
            this.urlL = new System.Windows.Forms.Label();
            this.folderB = new System.Windows.Forms.Button();
            this.extractC = new System.Windows.Forms.CheckBox();
            this.coverC = new System.Windows.Forms.CheckBox();
            this.Id3C = new System.Windows.Forms.CheckBox();
            this.coverP = new System.Windows.Forms.PictureBox();
            this.fbd = new System.Windows.Forms.FolderBrowserDialog();
            this.titleL = new System.Windows.Forms.Label();
            this.artiseL = new System.Windows.Forms.Label();
            this.trackT = new System.Windows.Forms.TextBox();
            this.lyricC = new System.Windows.Forms.CheckBox();
            this.resizeSize = new System.Windows.Forms.NumericUpDown();
            this.goB = new System.Windows.Forms.Button();
            this.sts = new System.Windows.Forms.StatusStrip();
            this.tssL = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssDummy = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssP = new System.Windows.Forms.ToolStripProgressBar();
            this.verifyCodeP = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.coverP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.resizeSize)).BeginInit();
            this.sts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.verifyCodeP)).BeginInit();
            this.SuspendLayout();
            // 
            // folderL
            // 
            this.folderL.AutoSize = true;
            this.folderL.Location = new System.Drawing.Point(9, 13);
            this.folderL.Name = "folderL";
            this.folderL.Size = new System.Drawing.Size(41, 12);
            this.folderL.TabIndex = 0;
            this.folderL.Text = "Folder";
            // 
            // folder
            // 
            this.folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.folder.Location = new System.Drawing.Point(56, 11);
            this.folder.Name = "folder";
            this.folder.Size = new System.Drawing.Size(433, 21);
            this.folder.TabIndex = 1;
            // 
            // url
            // 
            this.url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.url.Location = new System.Drawing.Point(56, 36);
            this.url.Name = "url";
            this.url.Size = new System.Drawing.Size(433, 21);
            this.url.TabIndex = 2;
            // 
            // urlL
            // 
            this.urlL.AutoSize = true;
            this.urlL.Location = new System.Drawing.Point(9, 39);
            this.urlL.Name = "urlL";
            this.urlL.Size = new System.Drawing.Size(23, 12);
            this.urlL.TabIndex = 3;
            this.urlL.Text = "URL";
            // 
            // folderB
            // 
            this.folderB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.folderB.Location = new System.Drawing.Point(495, 11);
            this.folderB.Name = "folderB";
            this.folderB.Size = new System.Drawing.Size(75, 21);
            this.folderB.TabIndex = 4;
            this.folderB.Text = "&Browse...";
            this.folderB.UseVisualStyleBackColor = true;
            this.folderB.Click += new System.EventHandler(this.folderB_Click);
            // 
            // extractC
            // 
            this.extractC.AutoSize = true;
            this.extractC.Enabled = false;
            this.extractC.Location = new System.Drawing.Point(56, 64);
            this.extractC.Name = "extractC";
            this.extractC.Size = new System.Drawing.Size(66, 16);
            this.extractC.TabIndex = 6;
            this.extractC.Text = "Extract";
            this.extractC.UseVisualStyleBackColor = true;
            // 
            // coverC
            // 
            this.coverC.AutoSize = true;
            this.coverC.Checked = true;
            this.coverC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.coverC.Location = new System.Drawing.Point(121, 64);
            this.coverC.Name = "coverC";
            this.coverC.Size = new System.Drawing.Size(78, 16);
            this.coverC.TabIndex = 7;
            this.coverC.Text = "Set cover";
            this.coverC.UseVisualStyleBackColor = true;
            this.coverC.CheckedChanged += new System.EventHandler(this.coverC_CheckedChanged);
            // 
            // Id3C
            // 
            this.Id3C.AutoSize = true;
            this.Id3C.Checked = true;
            this.Id3C.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Id3C.Location = new System.Drawing.Point(282, 64);
            this.Id3C.Name = "Id3C";
            this.Id3C.Size = new System.Drawing.Size(66, 16);
            this.Id3C.TabIndex = 8;
            this.Id3C.Text = "Set ID3";
            this.Id3C.UseVisualStyleBackColor = true;
            // 
            // coverP
            // 
            this.coverP.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.coverP.Location = new System.Drawing.Point(420, 99);
            this.coverP.Name = "coverP";
            this.coverP.Size = new System.Drawing.Size(150, 138);
            this.coverP.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.coverP.TabIndex = 10;
            this.coverP.TabStop = false;
            // 
            // titleL
            // 
            this.titleL.AutoSize = true;
            this.titleL.Location = new System.Drawing.Point(53, 82);
            this.titleL.Name = "titleL";
            this.titleL.Size = new System.Drawing.Size(23, 12);
            this.titleL.TabIndex = 11;
            this.titleL.Text = "   ";
            // 
            // artiseL
            // 
            this.artiseL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.artiseL.AutoSize = true;
            this.artiseL.Location = new System.Drawing.Point(417, 84);
            this.artiseL.Name = "artiseL";
            this.artiseL.Size = new System.Drawing.Size(23, 12);
            this.artiseL.TabIndex = 12;
            this.artiseL.Text = "   ";
            // 
            // trackT
            // 
            this.trackT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.trackT.Location = new System.Drawing.Point(56, 99);
            this.trackT.Multiline = true;
            this.trackT.Name = "trackT";
            this.trackT.ReadOnly = true;
            this.trackT.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.trackT.Size = new System.Drawing.Size(358, 139);
            this.trackT.TabIndex = 13;
            // 
            // lyricC
            // 
            this.lyricC.AutoSize = true;
            this.lyricC.Checked = true;
            this.lyricC.CheckState = System.Windows.Forms.CheckState.Checked;
            this.lyricC.Location = new System.Drawing.Point(346, 64);
            this.lyricC.Name = "lyricC";
            this.lyricC.Size = new System.Drawing.Size(84, 16);
            this.lyricC.TabIndex = 14;
            this.lyricC.Text = "Set lyrics";
            this.lyricC.UseVisualStyleBackColor = true;
            // 
            // resizeSize
            // 
            this.resizeSize.Location = new System.Drawing.Point(199, 61);
            this.resizeSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.resizeSize.Minimum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.resizeSize.Name = "resizeSize";
            this.resizeSize.Size = new System.Drawing.Size(56, 21);
            this.resizeSize.TabIndex = 15;
            this.resizeSize.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // goB
            // 
            this.goB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.goB.Location = new System.Drawing.Point(495, 36);
            this.goB.Name = "goB";
            this.goB.Size = new System.Drawing.Size(75, 21);
            this.goB.TabIndex = 17;
            this.goB.Text = "&Go";
            this.goB.UseVisualStyleBackColor = true;
            this.goB.Click += new System.EventHandler(this.goB_Click);
            // 
            // sts
            // 
            this.sts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssL,
            this.tssDummy,
            this.tssP});
            this.sts.Location = new System.Drawing.Point(0, 250);
            this.sts.Name = "sts";
            this.sts.Size = new System.Drawing.Size(584, 22);
            this.sts.TabIndex = 18;
            this.sts.Text = "sts";
            // 
            // tssL
            // 
            this.tssL.Name = "tssL";
            this.tssL.Size = new System.Drawing.Size(0, 17);
            // 
            // tssDummy
            // 
            this.tssDummy.Name = "tssDummy";
            this.tssDummy.Size = new System.Drawing.Size(367, 17);
            this.tssDummy.Spring = true;
            // 
            // tssP
            // 
            this.tssP.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.tssP.Name = "tssP";
            this.tssP.Size = new System.Drawing.Size(200, 16);
            // 
            // verifyCodeP
            // 
            this.verifyCodeP.Location = new System.Drawing.Point(470, 60);
            this.verifyCodeP.Name = "verifyCodeP";
            this.verifyCodeP.Size = new System.Drawing.Size(100, 28);
            this.verifyCodeP.TabIndex = 19;
            this.verifyCodeP.TabStop = false;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 272);
            this.Controls.Add(this.verifyCodeP);
            this.Controls.Add(this.sts);
            this.Controls.Add(this.goB);
            this.Controls.Add(this.resizeSize);
            this.Controls.Add(this.lyricC);
            this.Controls.Add(this.trackT);
            this.Controls.Add(this.artiseL);
            this.Controls.Add(this.titleL);
            this.Controls.Add(this.coverP);
            this.Controls.Add(this.Id3C);
            this.Controls.Add(this.coverC);
            this.Controls.Add(this.extractC);
            this.Controls.Add(this.folderB);
            this.Controls.Add(this.urlL);
            this.Controls.Add(this.url);
            this.Controls.Add(this.folder);
            this.Controls.Add(this.folderL);
            this.MinimumSize = new System.Drawing.Size(600, 280);
            this.Name = "mainForm";
            this.Text = "Cover Grabber";
            ((System.ComponentModel.ISupportInitialize)(this.coverP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.resizeSize)).EndInit();
            this.sts.ResumeLayout(false);
            this.sts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.verifyCodeP)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label folderL;
        private System.Windows.Forms.TextBox folder;
        private System.Windows.Forms.TextBox url;
        private System.Windows.Forms.Label urlL;
        private System.Windows.Forms.Button folderB;
        private System.Windows.Forms.CheckBox extractC;
        private System.Windows.Forms.CheckBox coverC;
        private System.Windows.Forms.CheckBox Id3C;
        private System.Windows.Forms.PictureBox coverP;
        private System.Windows.Forms.FolderBrowserDialog fbd;
        private System.Windows.Forms.Label titleL;
        private System.Windows.Forms.Label artiseL;
        private System.Windows.Forms.TextBox trackT;
        private System.Windows.Forms.CheckBox lyricC;
        private System.Windows.Forms.NumericUpDown resizeSize;
        private System.Windows.Forms.Button goB;
        private System.Windows.Forms.StatusStrip sts;
        private System.Windows.Forms.ToolStripProgressBar tssP;
        private System.Windows.Forms.ToolStripStatusLabel tssL;
        private System.Windows.Forms.PictureBox verifyCodeP;
        private System.Windows.Forms.ToolStripStatusLabel tssDummy;
    }
}

