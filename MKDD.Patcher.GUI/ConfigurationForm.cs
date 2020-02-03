using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;

namespace MKDD.Patcher.GUI
{
    public partial class ConfigurationForm : Form
    {
        private GuiConfig mConfiguration;

        public ConfigurationForm(GuiConfig configuration)
        {
            InitializeComponent();

            tbFilesDir.Text = configuration.Patcher.FilesDir;
            tbModsDir.Text = configuration.Patcher.ModsDir;
            tbBinDir.Text = configuration.Patcher.BinDir;
            tbOutDir.Text = configuration.Patcher.OutDir;
            tbCacheDir.Text = configuration.Patcher.CacheDir;
            tbArcPackPath.Text = configuration.Patcher.ArcPackPath;
            tbArcExtract.Text = configuration.Patcher.ArcExtractPath;
           
            mConfiguration = configuration;
        }

        private string SelectDirOrDefault( string description, string defaultValue )
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dlg.Description = description;
            dlg.UseDescriptionForTitle = true;
            if ( dlg.ShowDialog().GetValueOrDefault() )
                return dlg.SelectedPath;
            else
                return defaultValue;
        }

        private string SelectFileOrDefault( string description, string fileName, string defaultValue )
        {
            var dlg = new OpenFileDialog();
            dlg.Title = description;
            dlg.FileName = fileName;
            if ( dlg.ShowDialog() == DialogResult.OK )
                return dlg.FileName;
            else
                return defaultValue;
        }

        private void btnFilesDir_Click( object sender, EventArgs e )
        {
            tbFilesDir.Text = SelectDirOrDefault( "Select the Mario Kart Double Dash extracted files directory", tbFilesDir.Text );

            if ( Directory.Exists( tbFilesDir.Text ) )
            {
                SetUnsetPathsBasedOnFilesDirectory( tbFilesDir.Text );
            }
        }

        private void btnModsDir_Click( object sender, EventArgs e )
        {
            tbModsDir.Text = SelectDirOrDefault( "Select the directory to store the mods in", tbModsDir.Text );
        }

        private void btnBinDir_Click( object sender, EventArgs e )
        {
            tbBinDir.Text = SelectDirOrDefault( "Select the intermediary output directory", tbBinDir.Text );
        }

        private void btnOutDir_Click( object sender, EventArgs e )
        {
            tbOutDir.Text = SelectDirOrDefault( "Select the final output directory", tbOutDir.Text );
        }

        private void btnCacheDir_Click( object sender, EventArgs e )
        {
            tbCacheDir.Text = SelectDirOrDefault( "Select the cache directory", tbCacheDir.Text );
        }

        private void btnArcPackPath_Click( object sender, EventArgs e )
        {
            tbArcPackPath.Text = SelectFileOrDefault( "Select ArcPackPath.exe", "ArcPackPath.exe", tbArcPackPath.Text );
        }

        private void btnArcExtractPath_Click( object sender, EventArgs e )
        {
            tbArcExtract.Text = SelectFileOrDefault( "Select ArcExtract.exe", "ArcExtract.exe", tbArcExtract.Text );
        }

        private void SetUnsetPathsBasedOnFilesDirectory(string filesDirectory)
        {
            var rootDirectory = Path.GetDirectoryName(filesDirectory);
            if ( string.IsNullOrWhiteSpace( tbFilesDir.Text ) ) tbFilesDir.Text = Path.Combine( rootDirectory, "files" );
            if ( string.IsNullOrWhiteSpace( tbModsDir.Text ) ) tbModsDir.Text = Path.Combine( rootDirectory, "mods" );
            if ( string.IsNullOrWhiteSpace( tbBinDir.Text ) ) tbBinDir.Text = Path.Combine( rootDirectory, "mods/.bin" );
            if ( string.IsNullOrWhiteSpace( tbOutDir.Text ) ) tbOutDir.Text = tbFilesDir.Text;
            if ( string.IsNullOrWhiteSpace( tbCacheDir.Text ) ) tbCacheDir.Text = Path.Combine( rootDirectory, "mods/.cache" );
        }

        private void btnOK_Click( object sender, EventArgs e )
        {
            if (!Directory.Exists( tbFilesDir.Text ) )
            {
                MessageBox.Show( "Files directory doesn't exist!" );
                return;
            }

            if (!File.Exists( tbArcPackPath.Text) )
            {
                MessageBox.Show( "ArcPack.exe could not be found at the specified path!" );
                return;
            }

            if (!File.Exists( tbArcExtract.Text))
            {
                MessageBox.Show( "ArcExtract.exe could not be found at the specified path!" );
                return;
            }

            mConfiguration.Patcher.FilesDir = tbFilesDir.Text;
            mConfiguration.Patcher.ModsDir = tbModsDir.Text;
            mConfiguration.Patcher.BinDir = tbBinDir.Text;
            mConfiguration.Patcher.OutDir = tbOutDir.Text;
            mConfiguration.Patcher.CacheDir = tbCacheDir.Text;
            mConfiguration.Patcher.ArcPackPath = tbArcPackPath.Text;
            mConfiguration.Patcher.ArcExtractPath = tbArcExtract.Text;
            DialogResult = DialogResult.OK;
        }

        private void tbFilesDir_TextChanged( object sender, EventArgs e )
        {
            tbModsDir.ReadOnly = false;
            btnModsDir.Enabled = true;

            tbBinDir.ReadOnly = false;
            btnBinDir.Enabled = true;

            tbOutDir.ReadOnly = false;
            btnOutDir.Enabled = true;

            tbCacheDir.ReadOnly = false;
            btnCacheDir.Enabled = true;

            tbArcPackPath.ReadOnly = false;
            btnArcPackPath.Enabled = true;

            tbArcExtract.ReadOnly = false;
            btnArcExtractPath.Enabled = true;
        }
    }
}
