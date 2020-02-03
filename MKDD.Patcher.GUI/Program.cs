using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKDD.Patcher.GUI
{
    public class GuiModInfo
    {
        public string Title { get; set; }
        public bool Enabled { get; set; }
    }

    public class GuiConfig
    {
        public const string FILE_PATH = "mkdd-patcher-gui.cfg.json";

        public PatcherConfig Patcher { get; set; }
        public List<GuiModInfo> Mods { get; set; }

        public GuiConfig()
        {
            Patcher = new PatcherConfig();
            Mods = new List<GuiModInfo>();
        }

        public static GuiConfig Load( string path )
        {
            return JsonConvert.DeserializeObject<GuiConfig>( File.ReadAllText( path ) );
        }

        public void Save( string path )
        {
            File.WriteAllText( path, JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var asmName = Assembly.GetExecutingAssembly().GetName();
            var logger = new LoggerConfiguration()
                .WriteTo.File($"{asmName.Name}.log")
                .CreateLogger();

            logger.Information( $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE ({DateTime.Now.Year})\n" );

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            var configuration = TryLoadConfiguration( logger );
            string reason = null;
            if ( configuration == null || !ValidateConfiguration( logger, configuration, out reason ) )
            {
                configuration = ReconfigureConfiguration( reason );
                configuration.Save( GuiConfig.FILE_PATH );
            }

            Application.Run( new MainForm( logger, configuration ) );
        }

        private static GuiConfig ReconfigureConfiguration( string reason )
        {
            if ( reason != null )
            {
                MessageBox.Show( reason, "Invalid configuration.", MessageBoxButtons.OK );
            }

            // Force the user to configure properly
            var configuration = new GuiConfig();
            using ( var dialog = new ConfigurationForm( configuration ) )
            {
                DialogResult result;
                do
                {
                    result = dialog.ShowDialog();
                    if ( result != DialogResult.OK )
                        MessageBox.Show( "You must specify a valid configuration for the program to be able to run" );
                } while ( result != DialogResult.OK );
            }

            return configuration;
        }

        private static GuiConfig TryLoadConfiguration( ILogger logger )
        {
            GuiConfig configuration = null;
            try
            {
                if ( File.Exists( GuiConfig.FILE_PATH ) )
                {
                    configuration = JsonConvert.DeserializeObject<GuiConfig>( File.ReadAllText( GuiConfig.FILE_PATH ) );
                }
                else
                {
                    logger.Error( $"Config file {GuiConfig.FILE_PATH} doesn't exist. " );
                }
            }
            catch ( Exception e )
            {
                logger.Error( $"Config failed to load: {e.Message}" );
            }

            return configuration;
        }

        private static bool ValidateConfiguration(ILogger logger, GuiConfig configuration, out string errorMessage)
        {
            errorMessage = null;

            // Validate config
            if ( !Directory.Exists( configuration.Patcher.FilesDir ) )
            {
                errorMessage = "Files directory not found.";
                return false;
            }

            if ( !Directory.Exists( configuration.Patcher.ModsDir ) )
            {
                logger.Warning( "Mod directory didn't exist exist. A new directory was created." );
                Directory.CreateDirectory( configuration.Patcher.ModsDir );
            }

            if ( !Directory.Exists( configuration.Patcher.BinDir ) )
            {
                logger.Warning( "Bin directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( configuration.Patcher.BinDir );
            }

            if ( !Directory.Exists( configuration.Patcher.OutDir ) )
            {
                logger.Warning( ( "Out directory doesn't exist. Creating new directory..." ) );
                Directory.CreateDirectory( configuration.Patcher.OutDir );
            }

            if ( !Directory.Exists( configuration.Patcher.CacheDir ) )
            {
                logger.Warning( ( "Cache directory doesn't exist. Creating new directory..." ) );
                Directory.CreateDirectory( configuration.Patcher.CacheDir );
            }

            if ( !File.Exists( configuration.Patcher.ArcPackPath ) )
            {
                errorMessage = "Can't find ArcPack.exe.";
                return false;
            }

            if ( !File.Exists( configuration.Patcher.ArcExtractPath ) )
            {
                errorMessage = "Can't find ArcExtract.exe.";
                return false;
            }

            return true;
        }
    }

    public class InvalidConfigException : Exception
    {
        public InvalidConfigException(string message)
            : base(message)
        {
        }
    }
}
