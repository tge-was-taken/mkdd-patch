namespace MKDD.Patcher.GUI
{
    partial class ConfigurationForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnOK = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbFilesDir = new System.Windows.Forms.TextBox();
            this.btnFilesDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbModsDir = new System.Windows.Forms.TextBox();
            this.tbBinDir = new System.Windows.Forms.TextBox();
            this.tbOutDir = new System.Windows.Forms.TextBox();
            this.tbCacheDir = new System.Windows.Forms.TextBox();
            this.tbArcPackPath = new System.Windows.Forms.TextBox();
            this.tbArcExtract = new System.Windows.Forms.TextBox();
            this.btnModsDir = new System.Windows.Forms.Button();
            this.btnBinDir = new System.Windows.Forms.Button();
            this.btnOutDir = new System.Windows.Forms.Button();
            this.btnCacheDir = new System.Windows.Forms.Button();
            this.btnArcPackPath = new System.Windows.Forms.Button();
            this.btnArcExtractPath = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btnOK, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(7);
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(584, 361);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.btnOK.Location = new System.Drawing.Point(254, 322);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 29);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.96307F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 75.32446F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 7.712471F));
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tbFilesDir, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnFilesDir, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label5, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.label6, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.label7, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.tbModsDir, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.tbBinDir, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.tbOutDir, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.tbCacheDir, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.tbArcPackPath, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.tbArcExtract, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.btnModsDir, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.btnBinDir, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.btnOutDir, 2, 3);
            this.tableLayoutPanel2.Controls.Add(this.btnCacheDir, 2, 4);
            this.tableLayoutPanel2.Controls.Add(this.btnArcPackPath, 2, 5);
            this.tableLayoutPanel2.Controls.Add(this.btnArcExtractPath, 2, 6);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(10, 10);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 7;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(564, 306);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 43);
            this.label2.TabIndex = 3;
            this.label2.Text = "Mods directory";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 43);
            this.label1.TabIndex = 0;
            this.label1.Text = "Files directory";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbFilesDir
            // 
            this.tbFilesDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbFilesDir.Location = new System.Drawing.Point(101, 11);
            this.tbFilesDir.Name = "tbFilesDir";
            this.tbFilesDir.Size = new System.Drawing.Size(411, 20);
            this.tbFilesDir.TabIndex = 1;
            this.tbFilesDir.TextChanged += new System.EventHandler(this.tbFilesDir_TextChanged);
            // 
            // btnFilesDir
            // 
            this.btnFilesDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnFilesDir.Location = new System.Drawing.Point(522, 10);
            this.btnFilesDir.Name = "btnFilesDir";
            this.btnFilesDir.Size = new System.Drawing.Size(38, 23);
            this.btnFilesDir.TabIndex = 2;
            this.btnFilesDir.Text = "...";
            this.btnFilesDir.UseVisualStyleBackColor = true;
            this.btnFilesDir.Click += new System.EventHandler(this.btnFilesDir_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 43);
            this.label3.TabIndex = 4;
            this.label3.Text = "Intermediary output directory";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 43);
            this.label4.TabIndex = 5;
            this.label4.Text = "Final output directory";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 43);
            this.label5.TabIndex = 6;
            this.label5.Text = "Cache directory";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 215);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 43);
            this.label6.TabIndex = 7;
            this.label6.Text = "ArcPack path";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 258);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(80, 48);
            this.label7.TabIndex = 8;
            this.label7.Text = "ArcExtract path";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tbModsDir
            // 
            this.tbModsDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbModsDir.Location = new System.Drawing.Point(101, 54);
            this.tbModsDir.Name = "tbModsDir";
            this.tbModsDir.ReadOnly = true;
            this.tbModsDir.Size = new System.Drawing.Size(411, 20);
            this.tbModsDir.TabIndex = 10;
            // 
            // tbBinDir
            // 
            this.tbBinDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbBinDir.Location = new System.Drawing.Point(101, 97);
            this.tbBinDir.Name = "tbBinDir";
            this.tbBinDir.ReadOnly = true;
            this.tbBinDir.Size = new System.Drawing.Size(411, 20);
            this.tbBinDir.TabIndex = 11;
            // 
            // tbOutDir
            // 
            this.tbOutDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbOutDir.Location = new System.Drawing.Point(101, 140);
            this.tbOutDir.Name = "tbOutDir";
            this.tbOutDir.ReadOnly = true;
            this.tbOutDir.Size = new System.Drawing.Size(411, 20);
            this.tbOutDir.TabIndex = 12;
            // 
            // tbCacheDir
            // 
            this.tbCacheDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbCacheDir.Location = new System.Drawing.Point(101, 183);
            this.tbCacheDir.Name = "tbCacheDir";
            this.tbCacheDir.ReadOnly = true;
            this.tbCacheDir.Size = new System.Drawing.Size(411, 20);
            this.tbCacheDir.TabIndex = 13;
            // 
            // tbArcPackPath
            // 
            this.tbArcPackPath.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbArcPackPath.Location = new System.Drawing.Point(101, 226);
            this.tbArcPackPath.Name = "tbArcPackPath";
            this.tbArcPackPath.ReadOnly = true;
            this.tbArcPackPath.Size = new System.Drawing.Size(411, 20);
            this.tbArcPackPath.TabIndex = 14;
            // 
            // tbArcExtract
            // 
            this.tbArcExtract.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tbArcExtract.Location = new System.Drawing.Point(101, 272);
            this.tbArcExtract.Name = "tbArcExtract";
            this.tbArcExtract.ReadOnly = true;
            this.tbArcExtract.Size = new System.Drawing.Size(411, 20);
            this.tbArcExtract.TabIndex = 15;
            // 
            // btnModsDir
            // 
            this.btnModsDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnModsDir.Enabled = false;
            this.btnModsDir.Location = new System.Drawing.Point(522, 53);
            this.btnModsDir.Name = "btnModsDir";
            this.btnModsDir.Size = new System.Drawing.Size(38, 23);
            this.btnModsDir.TabIndex = 17;
            this.btnModsDir.Text = "...";
            this.btnModsDir.UseVisualStyleBackColor = true;
            this.btnModsDir.Click += new System.EventHandler(this.btnModsDir_Click);
            // 
            // btnBinDir
            // 
            this.btnBinDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnBinDir.Enabled = false;
            this.btnBinDir.Location = new System.Drawing.Point(522, 96);
            this.btnBinDir.Name = "btnBinDir";
            this.btnBinDir.Size = new System.Drawing.Size(38, 23);
            this.btnBinDir.TabIndex = 18;
            this.btnBinDir.Text = "...";
            this.btnBinDir.UseVisualStyleBackColor = true;
            this.btnBinDir.Click += new System.EventHandler(this.btnBinDir_Click);
            // 
            // btnOutDir
            // 
            this.btnOutDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnOutDir.Enabled = false;
            this.btnOutDir.Location = new System.Drawing.Point(522, 139);
            this.btnOutDir.Name = "btnOutDir";
            this.btnOutDir.Size = new System.Drawing.Size(38, 23);
            this.btnOutDir.TabIndex = 19;
            this.btnOutDir.Text = "...";
            this.btnOutDir.UseVisualStyleBackColor = true;
            this.btnOutDir.Click += new System.EventHandler(this.btnOutDir_Click);
            // 
            // btnCacheDir
            // 
            this.btnCacheDir.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCacheDir.Enabled = false;
            this.btnCacheDir.Location = new System.Drawing.Point(522, 182);
            this.btnCacheDir.Name = "btnCacheDir";
            this.btnCacheDir.Size = new System.Drawing.Size(38, 23);
            this.btnCacheDir.TabIndex = 20;
            this.btnCacheDir.Text = "...";
            this.btnCacheDir.UseVisualStyleBackColor = true;
            this.btnCacheDir.Click += new System.EventHandler(this.btnCacheDir_Click);
            // 
            // btnArcPackPath
            // 
            this.btnArcPackPath.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnArcPackPath.Enabled = false;
            this.btnArcPackPath.Location = new System.Drawing.Point(522, 225);
            this.btnArcPackPath.Name = "btnArcPackPath";
            this.btnArcPackPath.Size = new System.Drawing.Size(38, 23);
            this.btnArcPackPath.TabIndex = 21;
            this.btnArcPackPath.Text = "...";
            this.btnArcPackPath.UseVisualStyleBackColor = true;
            this.btnArcPackPath.Click += new System.EventHandler(this.btnArcPackPath_Click);
            // 
            // btnArcExtractPath
            // 
            this.btnArcExtractPath.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnArcExtractPath.Enabled = false;
            this.btnArcExtractPath.Location = new System.Drawing.Point(522, 270);
            this.btnArcExtractPath.Name = "btnArcExtractPath";
            this.btnArcExtractPath.Size = new System.Drawing.Size(38, 23);
            this.btnArcExtractPath.TabIndex = 22;
            this.btnArcExtractPath.Text = "...";
            this.btnArcExtractPath.UseVisualStyleBackColor = true;
            this.btnArcExtractPath.Click += new System.EventHandler(this.btnArcExtractPath_Click);
            // 
            // ConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ConfigurationForm";
            this.Text = "Configuration";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbFilesDir;
        private System.Windows.Forms.Button btnFilesDir;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbModsDir;
        private System.Windows.Forms.TextBox tbBinDir;
        private System.Windows.Forms.TextBox tbOutDir;
        private System.Windows.Forms.TextBox tbCacheDir;
        private System.Windows.Forms.TextBox tbArcPackPath;
        private System.Windows.Forms.TextBox tbArcExtract;
        private System.Windows.Forms.Button btnModsDir;
        private System.Windows.Forms.Button btnBinDir;
        private System.Windows.Forms.Button btnOutDir;
        private System.Windows.Forms.Button btnCacheDir;
        private System.Windows.Forms.Button btnArcPackPath;
        private System.Windows.Forms.Button btnArcExtractPath;
    }
}