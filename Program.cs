using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace mkdd_patch
{
    public class Config
    {
        public string FilesDir { get; set; }
        public string ModsDir { get; set; }
        public string OutDir { get; set; }
        public string CacheDir { get; set; }
        public string ArcPackPath { get; set; }
        public string ArcExtractPath { get; set; }

        public Config()
        {
            FilesDir = "files";
            ModsDir = "mods";
            OutDir = ModsDir + "/.out";
            CacheDir = ModsDir + "/.cache";
            ArcPackPath = string.Empty;
            ArcExtractPath = string.Empty;
        }
    }

    public class Cache
    {
        public const int CURRENT_VERSION = 1;

        public int Version { get; set; }
        public HashSet<string> ArchiveDirs { get; set; }

        public Cache()
        {
            Version = CURRENT_VERSION;
            ArchiveDirs = new HashSet<string>();
        }
    }

    public class Program
    {
        public static Config Config { get; set; }

        static void Main( string[] args )
        {
            var asmName = Assembly.GetExecutingAssembly().GetName();
            Console.WriteLine( $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE (2020)" );
            Console.WriteLine();

            Config = LoadConfig( "config.json" );
            if ( !ValidateConfig() )
                return;

            var cacheFilesDir = Path.GetFullPath( Path.Combine( Config.CacheDir, "files" ) );
            var cache = InitializeCache( Config.FilesDir, cacheFilesDir );
            var modsProcessed = ProcessMods( Config.FilesDir, cacheFilesDir, cache.ArchiveDirs );
            if ( modsProcessed > 0 )
                ProcessBinDir( Config.OutDir, cache.ArchiveDirs );
            else
                Console.WriteLine( "No mods available to install" );

            Console.WriteLine( "Done!" );
        }

        private static bool ValidateConfig()
        {
            // Validate config
            if ( !Directory.Exists( Config.FilesDir ) )
            {
                Console.WriteLine( "Files directory not found. Make sure that the executable is placed in the right directory & the config is set up correctly." );
                return false;
            }

            if ( !Directory.Exists( Config.ModsDir ) )
            {
                Console.WriteLine( "Mod directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( Config.ModsDir );
            }

            if ( !Directory.Exists( Config.OutDir ) )
            {
                Console.WriteLine( "Out directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( Config.OutDir );
            }

            if ( !Directory.Exists( Config.CacheDir ) )
            {
                Console.WriteLine( "Cache directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( Config.CacheDir );
            }

            if ( !File.Exists( Config.ArcPackPath ) )
            {
                Console.WriteLine( "Can't find ArcPack.exe. Verify the path in the config." );
                return false;
            }

            if ( !File.Exists( Config.ArcExtractPath ) )
            {
                Console.WriteLine( "Can't find ArcExtract.exe. Verify the path in the config." );
                return false;
            }

            return true;
        }

        private static Config LoadConfig( string configPath )
        {
            Config config = null;

            if ( File.Exists( configPath ) )
            {
                try
                {
                    config = JsonConvert.DeserializeObject<Config>( File.ReadAllText( configPath ) );
                }
                finally
                {
                }
            }

            if ( config == null )
            {
                config = new Config();
                File.WriteAllText( configPath, JsonConvert.SerializeObject( config, Formatting.Indented ) );
            }

            return config;
        }

        private static Cache InitializeCache( string rootFilesDir, string cacheFilesDir )
        {
            var cacheJsonPath = Path.Combine( Config.CacheDir, "cache.json" );
            Cache cache = null;
            if ( File.Exists( cacheJsonPath ) )
                cache = JsonConvert.DeserializeObject<Cache>( File.ReadAllText( cacheJsonPath ) );

            if ( cache == null || cache.Version != Cache.CURRENT_VERSION )
            {
                cache = new Cache();
                Console.WriteLine( "Building cache... please wait" );
                Directory.Delete( Config.CacheDir, true );
                UnpackARCs( rootFilesDir, cacheFilesDir, cache );

                // Process unpacked ARCs
                while ( true )
                {
                    if ( UnpackARCs( cacheFilesDir, cacheFilesDir, cache ) == 0 )
                        break;
                }

                File.WriteAllText( cacheJsonPath, JsonConvert.SerializeObject( cache, Formatting.Indented ) );
            }

            cache.ArchiveDirs = new HashSet<string>( cache.ArchiveDirs, StringComparer.InvariantCultureIgnoreCase );
            return cache;
        }

        private static int ProcessMods( string rootFilesDir, string cacheFilesDir, HashSet<string> archiveDirSet )
        {
            Console.WriteLine( "Processing mods" );

            // Delete previous output
            Directory.Delete( Config.OutDir, true );
            Directory.CreateDirectory( Config.OutDir );

            // Iterate over mods
            Directory.CreateDirectory( Config.ModsDir );
            int modsProcessed = 0;
            foreach ( var modDir in Directory.EnumerateDirectories( Config.ModsDir ) )
            {
                var modDirName = Path.GetFileName(modDir);
                if ( modDirName == ".bin" || modDirName == ".cache" )
                    continue;

                Console.WriteLine( $"Processing mod {modDirName}" );
                ProcessModDir( rootFilesDir, cacheFilesDir, modDir, archiveDirSet, Path.Combine( modDir, "files" ), isArcDir: false );
                ++modsProcessed;
            }

            return modsProcessed;
        }

        private static void ProcessModDir( string rootFilesDir, string cacheFilesDir, string modDir, HashSet<string> archiveDirSet, string dir, bool isArcDir )
        {
            foreach ( var entryName in Directory.EnumerateFileSystemEntries( dir ) )
            {
                var relEntryPath = Path.GetRelativePath( dir, entryName );

                if ( File.Exists(entryName) )
                {
                    if ( !isArcDir )
                    {
                        // Copy file to out directory
                        Console.WriteLine( $"Copying {relEntryPath}" );
                        File.Copy( entryName, Path.Combine( Config.OutDir, relEntryPath ), true );
                    }
                }
                else
                {
                    if ( archiveDirSet.Contains( relEntryPath ) )
                    {
                        // This directory is an archive
                        var entryBinDir = Path.Combine( Config.OutDir, relEntryPath );

                        if ( !Directory.Exists( entryBinDir ) )
                        {
                            // Copy original files from cache to out directory
                            var entryCacheDir = Path.Combine( cacheFilesDir, relEntryPath );
                            Debug.Assert( Directory.Exists( entryCacheDir ) );
                            CopyDirectoryContents( entryCacheDir, entryBinDir );
                        }

                        // Overwrite the files in the out directory
                        CopyDirectoryContents( entryName, entryBinDir );

                        // Recurse into contents of archive
                        var arcRootDir = Directory.EnumerateDirectories(entryName).SingleOrDefault();
                        if ( arcRootDir == null )
                            throw new InvalidOperationException( $"Unable to determine archive root directory for {entryName}. Make sure there is only 1 directory inside." );

                        ProcessModDir( rootFilesDir, cacheFilesDir, modDir, archiveDirSet, arcRootDir, isArcDir: true );
                    }
                    else
                    {
                        // Recurse
                        ProcessModDir( rootFilesDir, cacheFilesDir, modDir, archiveDirSet, entryName, isArcDir );
                    }
                }
            }
        }

        private static void ProcessBinDir( string dir, HashSet<string> archiveDirSet )
        {
            Console.WriteLine( "Processing output" );

            // Recursively process directories
            var archiveStack = new Stack<(string, string)>();
            void ProcessBinDirRecursive( string curDir )
            {
                foreach ( var dirPath in Directory.EnumerateDirectories( curDir ) )
                {
                    var relDirPath = Path.GetRelativePath( dir, dirPath );

                    if ( archiveDirSet.Contains( relDirPath ) )
                    {
                        // This directory is an archive
                        // Build it
                        archiveStack.Push( (dirPath, relDirPath) );
                    }

                    // Recurse
                    ProcessBinDirRecursive( dirPath );
                }
            }

            ProcessBinDirRecursive( dir );

            // Pack archives LIFO so nested archives play nicely
            while (archiveStack.Count > 0)
            {
                (var dirPath, var relDirPath) = archiveStack.Pop();
                var arcRootDirPath = Directory.EnumerateDirectories(dirPath).FirstOrDefault();
                if ( arcRootDirPath == null )
                {
                    Console.WriteLine( $"WARNING: Expected ARC root directory inside {dirPath}. Using directory as root instead." );
                    arcRootDirPath = dirPath;
                }

                var process = new Process();
                process.StartInfo = new ProcessStartInfo( Config.ArcPackPath, arcRootDirPath ) { RedirectStandardOutput = false, CreateNoWindow = true };
                process.Start();
                Console.WriteLine( $"Packing {relDirPath}" );
                process.WaitForExit();

                File.Copy( arcRootDirPath + ".arc", dirPath + ".arc", true );
                Directory.Delete( dirPath, true );
            }
        }

        private static void CopyDirectoryContents( string sourceDir, string destDir )
        {
            Console.WriteLine( $"Copying {sourceDir} to {destDir}" );

            // Now create all of the directories
            foreach ( string dirPath in Directory.GetDirectories( sourceDir, "*",
                SearchOption.AllDirectories ) )
                Directory.CreateDirectory( dirPath.Replace( sourceDir, destDir ) );

            // Copy all the files & replace any files with the same name
            foreach ( string newPath in Directory.GetFiles( sourceDir, "*.*",
                SearchOption.AllDirectories ) )
                File.Copy( newPath, newPath.Replace( sourceDir, destDir ), true );
        }

        private static int UnpackARCs( string inputFilesDir, string outputFilesDir, Cache cache )
        {
            var processes = new List<(Process Process, string CacheArcFilePath)>();
            foreach ( var filePath in Directory.EnumerateFiles( inputFilesDir, "*.arc", SearchOption.AllDirectories ) )
            {
                UnpackARC( inputFilesDir, outputFilesDir, processes, filePath, cache );
            }

            WaitForExtractionAndCleanup( processes );
            return processes.Count;
        }

        private static void WaitForExtractionAndCleanup( List<(Process Process, string CacheArcFilePath)> processes )
        {
            // Wait for completion & cleanup
            foreach ( (var process, string cacheArcFilePath) in processes )
            {
                process.WaitForExit();
                File.Delete( cacheArcFilePath );
            }
        }

        private static void UnpackARC( string inputFilesDir, string outputFilesDir, string filePath, Cache cache )
        {
            var processes = new List<(Process Process, string CacheArcFilePath)>();
            UnpackARC( inputFilesDir, outputFilesDir, processes, filePath, cache );
            WaitForExtractionAndCleanup( processes );
        }

        private static void UnpackARC( string inputFilesDir, string outputFilesDir, 
            List<(Process Process, string CacheArcFilePath)> processes, string filePath, Cache cache )
        {
            // Either we copy the archive or we copy the extracted contents
            // copying the archive is faster so we'll do that instead and later delete it.
            var relFilePath = Path.GetRelativePath( inputFilesDir, filePath );
            var relDirPath = Path.Combine( Path.GetDirectoryName( relFilePath ), Path.GetFileNameWithoutExtension( relFilePath ) );
            var cacheArcDirPath = Path.Combine( outputFilesDir, relDirPath );
            var cacheArcFilePath = Path.Combine( cacheArcDirPath, Path.GetFileName( filePath ) );
            cache.ArchiveDirs.Add( relDirPath );

            if ( !Directory.Exists( cacheArcDirPath ) )
            {
                Console.WriteLine( $"Unpacking {relFilePath}" );

                // Copy archive to cache (but only if not already in cache)
                Directory.CreateDirectory( cacheArcDirPath );
                if ( !File.Exists( cacheArcFilePath ) )
                {
                    File.Copy( filePath, cacheArcFilePath );

                    // If the directory the arc is being extracted to is in the same directory as the file itself, delete the original file
                    if ( Path.GetDirectoryName( filePath ).Equals( Path.GetDirectoryName( cacheArcDirPath ), StringComparison.InvariantCultureIgnoreCase ) )
                        File.Delete( filePath );
                }

                // Extract it
                var process = new Process();
                process.StartInfo = new ProcessStartInfo( Config.ArcExtractPath, Path.GetFullPath( cacheArcFilePath ) )
                {
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };
                process.Start();
                processes.Add( (process, Path.GetFullPath( cacheArcFilePath )) );
            }
        }

        public static string MakeRelative( string filePath, string referencePath )
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri( fileUri ).ToString();
        }
    }
}
