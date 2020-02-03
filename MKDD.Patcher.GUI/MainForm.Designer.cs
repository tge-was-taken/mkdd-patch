namespace MKDD.Patcher.GUI
{
    partial class MainForm
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
            this.msMain = new System.Windows.Forms.MenuStrip();
            this.tsmiSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSave = new System.Windows.Forms.Button();
            this.lytMain = new System.Windows.Forms.TableLayoutPanel();
            this.lytModGrid = new System.Windows.Forms.TableLayoutPanel();
            this.dgvMods = new System.Windows.Forms.DataGridView();
            this.clmEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.clmTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmAuthors = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lytSideBar = new System.Windows.Forms.TableLayoutPanel();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.msMain.SuspendLayout();
            this.lytMain.SuspendLayout();
            this.lytModGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMods)).BeginInit();
            this.lytSideBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // msMain
            // 
            this.msMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiSettings,
            this.tsmiAbout});
            this.msMain.Location = new System.Drawing.Point(0, 0);
            this.msMain.Name = "msMain";
            this.msMain.Size = new System.Drawing.Size(584, 24);
            this.msMain.TabIndex = 0;
            this.msMain.Text = "menuStrip1";
            // 
            // tsmiSettings
            // 
            this.tsmiSettings.Name = "tsmiSettings";
            this.tsmiSettings.Size = new System.Drawing.Size(61, 20);
            this.tsmiSettings.Text = "Settings";
            this.tsmiSettings.Click += new System.EventHandler(this.tsmiSettings_Click);
            // 
            // tsmiAbout
            // 
            this.tsmiAbout.Name = "tsmiAbout";
            this.tsmiAbout.Size = new System.Drawing.Size(52, 20);
            this.tsmiAbout.Text = "About";
            this.tsmiAbout.Click += new System.EventHandler(this.tsmiAbout_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnSave.Location = new System.Drawing.Point(242, 294);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lytMain
            // 
            this.lytMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lytMain.ColumnCount = 1;
            this.lytMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytMain.Controls.Add(this.btnSave, 0, 1);
            this.lytMain.Controls.Add(this.lytModGrid, 0, 0);
            this.lytMain.Location = new System.Drawing.Point(12, 27);
            this.lytMain.Name = "lytMain";
            this.lytMain.RowCount = 2;
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.lytMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.lytMain.Size = new System.Drawing.Size(560, 322);
            this.lytMain.TabIndex = 1;
            // 
            // lytModGrid
            // 
            this.lytModGrid.ColumnCount = 2;
            this.lytModGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 95F));
            this.lytModGrid.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.lytModGrid.Controls.Add(this.dgvMods, 0, 0);
            this.lytModGrid.Controls.Add(this.lytSideBar, 1, 0);
            this.lytModGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytModGrid.Location = new System.Drawing.Point(3, 3);
            this.lytModGrid.Name = "lytModGrid";
            this.lytModGrid.RowCount = 1;
            this.lytModGrid.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytModGrid.Size = new System.Drawing.Size(554, 283);
            this.lytModGrid.TabIndex = 1;
            // 
            // dgvMods
            // 
            this.dgvMods.AllowUserToAddRows = false;
            this.dgvMods.AllowUserToOrderColumns = true;
            this.dgvMods.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMods.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmEnabled,
            this.clmTitle,
            this.clmVersion,
            this.clmAuthors,
            this.clmDescription});
            this.dgvMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMods.Location = new System.Drawing.Point(3, 3);
            this.dgvMods.Name = "dgvMods";
            this.dgvMods.ReadOnly = true;
            this.dgvMods.RowHeadersVisible = false;
            this.dgvMods.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMods.Size = new System.Drawing.Size(520, 277);
            this.dgvMods.TabIndex = 2;
            // 
            // clmEnabled
            // 
            this.clmEnabled.HeaderText = "Enabled";
            this.clmEnabled.Name = "clmEnabled";
            this.clmEnabled.ReadOnly = true;
            this.clmEnabled.Width = 50;
            // 
            // clmTitle
            // 
            this.clmTitle.HeaderText = "Title";
            this.clmTitle.Name = "clmTitle";
            this.clmTitle.ReadOnly = true;
            this.clmTitle.Width = 150;
            // 
            // clmVersion
            // 
            this.clmVersion.HeaderText = "Version";
            this.clmVersion.Name = "clmVersion";
            this.clmVersion.ReadOnly = true;
            this.clmVersion.Width = 50;
            // 
            // clmAuthors
            // 
            this.clmAuthors.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.clmAuthors.HeaderText = "Author(s)";
            this.clmAuthors.Name = "clmAuthors";
            this.clmAuthors.ReadOnly = true;
            this.clmAuthors.Width = 74;
            // 
            // clmDescription
            // 
            this.clmDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clmDescription.HeaderText = "Description";
            this.clmDescription.Name = "clmDescription";
            this.clmDescription.ReadOnly = true;
            // 
            // lytSideBar
            // 
            this.lytSideBar.ColumnCount = 1;
            this.lytSideBar.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.lytSideBar.Controls.Add(this.btnDown, 0, 1);
            this.lytSideBar.Controls.Add(this.btnUp, 0, 0);
            this.lytSideBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lytSideBar.Location = new System.Drawing.Point(526, 0);
            this.lytSideBar.Margin = new System.Windows.Forms.Padding(0);
            this.lytSideBar.Name = "lytSideBar";
            this.lytSideBar.RowCount = 2;
            this.lytSideBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytSideBar.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.lytSideBar.Size = new System.Drawing.Size(28, 283);
            this.lytSideBar.TabIndex = 3;
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDown.Location = new System.Drawing.Point(3, 144);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(22, 30);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "↓";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnUp
            // 
            this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUp.Location = new System.Drawing.Point(3, 108);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(22, 30);
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "↑";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.lytMain);
            this.Controls.Add(this.msMain);
            this.MainMenuStrip = this.msMain;
            this.Name = "MainForm";
            this.Text = "MKDD Patcher GUI";
            this.msMain.ResumeLayout(false);
            this.msMain.PerformLayout();
            this.lytMain.ResumeLayout(false);
            this.lytModGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMods)).EndInit();
            this.lytSideBar.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip msMain;
        private System.Windows.Forms.ToolStripMenuItem tsmiSettings;
        private System.Windows.Forms.ToolStripMenuItem tsmiAbout;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TableLayoutPanel lytMain;
        private System.Windows.Forms.TableLayoutPanel lytModGrid;
        private System.Windows.Forms.DataGridView dgvMods;
        private System.Windows.Forms.DataGridViewCheckBoxColumn clmEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmAuthors;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmDescription;
        private System.Windows.Forms.TableLayoutPanel lytSideBar;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
    }
}

