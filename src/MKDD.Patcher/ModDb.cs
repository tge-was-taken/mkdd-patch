using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MKDD.Patcher
{
    public class ModDb
    {
        private ILogger mLogger;
        private PatcherConfig mConfiguration;

        public string BaseDirectory { get; }

        public List<ModInfo> Mods { get; }

        public ModDb(ILogger logger, PatcherConfig configuration, string modsDirectory )
        {
            mLogger = logger;
            mConfiguration = configuration;
            BaseDirectory = modsDirectory;
            Mods = new List<ModInfo>();

            Initialize();
        }

        private void Initialize()
        {
            mLogger.Information( "Initializing Mod DB" );

            Directory.CreateDirectory( BaseDirectory );
            foreach ( var modDir in Directory.EnumerateDirectories( BaseDirectory ) )
            {
                if ( IsSpecialDirectory( modDir ) )
                {
                    mLogger.Information( $"Skipping {modDir} as it is a special directory" );
                    continue;
                }

                var modDirName = Path.GetFileName(modDir);
                mLogger.Information( $"Loading mod directory {modDirName}" );
                var modInfoPath = Path.Combine(modDir, ModInfo.FILENAME);
                if ( !ModInfo.TryLoad( modInfoPath, out var modInfo ) )
                {
                    // Create new mod info and save it
                    modInfo = ModInfo.CreateDefaultForDirectory( modDir );
                    modInfo.Save( modInfoPath );
                }

                Mods.Add( modInfo );
            }
        }
     
        private bool IsSpecialDirectory( string modDir )
        {
            return  PathHelper.AreEqual( modDir, mConfiguration.FilesDir ) ||
                    PathHelper.AreEqual( modDir, mConfiguration.BinDir ) ||
                    PathHelper.AreEqual( modDir, mConfiguration.ModsDir ) ||
                    PathHelper.AreEqual( modDir, mConfiguration.OutDir ) ||
                    PathHelper.AreEqual( modDir, mConfiguration.CacheDir );
        }
    }
}
