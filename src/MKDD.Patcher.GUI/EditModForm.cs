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

namespace MKDD.Patcher.GUI
{
    public partial class EditModForm : Form
    {
        private readonly GuiConfig mConfig;
        private readonly ModEditVm mVm;

        public ModInfo ModInfo { get; private set; }

        public EditModForm( GuiConfig config, ModInfo modInfo )
        {
            InitializeComponent();
            Text = modInfo == null ? "Create new mod" : $"Editing mod: {modInfo.Title}";
            mConfig = config;
            mVm = new ModEditVm( modInfo ?? new ModInfo() );
            BindTextToVm( tbTitle, nameof( ModEditVm.Title ) );
            BindTextToVm( tbDescription, nameof( ModEditVm.Description ) );
            BindTextToVm( tbVersion, nameof( ModEditVm.Version ) );
            BindTextToVm( tbAuthor, nameof( ModEditVm.Authors ) );
        }

        private void BindTextToVm( TextBox tb, string propertyName )
        {
            tb.DataBindings.Add( new Binding( nameof( TextBox.Text ), mVm, propertyName ) );
        }

        private void btnOK_Click( object sender, EventArgs e )
        {
            ModInfo = mVm.ModInfo;

            if ( string.IsNullOrEmpty( ModInfo.RootDir ) || !Directory.Exists( ModInfo.RootDir ) )
                ModInfo.RootDir = GetUniqueDirectoryPath( Path.Combine( mConfig.Patcher.ModsDir, ModInfo.Title ) );

            ModInfo.FilesDir = Path.Combine( ModInfo.RootDir, "files" );
        }

        private string GetUniqueDirectoryPath( string path )
        {
            if ( !Directory.Exists( path ) )
                return path;


            var counter = 1;
            var newPath = $"{path} ({counter})";
            while ( Directory.Exists( newPath ) )
                newPath = $"{path} ({++counter})";

            return newPath;
        }
    }
}
