using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace MKDD.Patcher
{
    public enum ContainerType
    {
        ARC,
        AW
    }

    public partial class Patcher
    {
        private readonly ILogger mLogger;
        private readonly IConfiguration mConfiguration;
        private readonly LoggedIO mIO;

        public Patcher( ILogger logger, IConfiguration configuration )
        {
            mLogger = logger;
            mConfiguration = configuration;
            mIO = new LoggedIO( logger );

            mLogger.Information( "Validating configuration" );
            if ( !ValidateConfig() )
                throw new ArgumentException( "Invalid configuration", nameof( configuration ) );
        }

        private string GetCacheFilesDir()
        {
            return Path.GetFullPath( Path.Combine( mConfiguration["CacheDir"], "files" ) );
        }

        private string GetBinFilePathFromRelPath( string relPath )
        {
            return Path.Combine( mConfiguration["BinDir"], relPath ); ;
        }

        private string GetCacheFilePathFromRelPath( string relPath )
        {
            return Path.Combine( mConfiguration["CacheDir"] + "/files", relPath );
        }

        public void Patch()
        {
            var cacheFilesDir = GetCacheFilesDir();
            var cache = InitializeCache( mConfiguration["FilesDir"], cacheFilesDir );
            var modsProcessed = ProcessMods( mConfiguration["FilesDir"], cacheFilesDir, cache );
            if ( modsProcessed > 0 )
            {
                ProcessBinDir( mConfiguration["BinDir"], cache );
            }
            else
            {
                mLogger.Warning( "No mods available to install" );
            }

            if (!PathIsSame(mConfiguration["BinDir"], mConfiguration["OutDir"]))
            {
                // Copy bin directory contents to out directory
                CopyDirectoryContents( mConfiguration["BinDir"], mConfiguration["OutDir"] );
            }

            mLogger.Information( "Patching done!" );
        }

        private bool ValidateConfig()
        {
            // Validate config
            if ( !Directory.Exists( mConfiguration["FilesDir"] ) )
            {
                mLogger.Fatal( "Files directory not found. Make sure that the executable is placed in the right directory & the config is set up correctly." );
                return false;
            }

            if ( !Directory.Exists( mConfiguration["ModsDir"] ) )
            {
                mLogger.Warning( "Mod directory doesn't exist. Creating new directory..." );
                mIO.CreateDirectory( mConfiguration["ModsDir"] );
            }

            if ( !Directory.Exists( mConfiguration["BinDir"] ) )
            {
                mLogger.Warning( "Bin directory doesn't exist. Creating new directory..." );
                mIO.CreateDirectory( mConfiguration["BinDir"] );
            }

            if ( !Directory.Exists( mConfiguration["OutDir"] ) )
            {
                mLogger.Warning( "Out directory doesn't exist. Creating new directory..." );
                mIO.CreateDirectory( mConfiguration["OutDir"] );
            }

            if ( !Directory.Exists( mConfiguration["CacheDir"] ) )
            {
                mLogger.Warning( "Cache directory doesn't exist. Creating new directory..." );
                mIO.CreateDirectory( mConfiguration["CacheDir"] );
            }

            if ( !File.Exists( mConfiguration["ArcPackPath"] ) )
            {
                mLogger.Fatal( "Can't find ArcPack.exe. Verify the path in the config." );
                return false;
            }

            if ( !File.Exists( mConfiguration["ArcExtractPath"] ) )
            {
                mLogger.Fatal( "Can't find ArcExtract.exe. Verify the path in the config." );
                return false;
            }

            return true;
        }

        private Cache InitializeCache( string rootFilesDir, string cacheFilesDir )
        {
            // Try to load cache
            var cacheJsonPath = Path.Combine( mConfiguration["CacheDir"], "cache.json" );
            Cache cache = null;
            if ( File.Exists( cacheJsonPath ) )
                cache = JsonConvert.DeserializeObject<Cache>( File.ReadAllText( cacheJsonPath ) );

            if ( cache == null || cache.Version != Cache.CURRENT_VERSION )
            {
                // Rebuild cache if it doesn't exist, or is outdated.
                cache = new Cache();
                mLogger.Information( "Building cache... please wait" );
                mIO.DeleteDirectory( mConfiguration["CacheDir"] );
                CacheFiles( rootFilesDir, cacheFilesDir, cache );

                // Process unpacked ARCs
                while ( true )
                {
                    if ( CacheFiles( cacheFilesDir, cacheFilesDir, cache ) == 0 )
                        break;
                }

                File.WriteAllText( cacheJsonPath, JsonConvert.SerializeObject( cache, Formatting.Indented ) );
            }

            cache.ContainerDirs = new Dictionary<string, ContainerType>( cache.ContainerDirs, StringComparer.InvariantCultureIgnoreCase );
            return cache;
        }

        private int ProcessMods( string rootFilesDir, string cacheFilesDir, Cache cache )
        {
            mLogger.Information( "Processing mods" );

            // Iterate over mods
            int modsProcessed = 0;
            foreach ( var modDir in Directory.EnumerateDirectories( mConfiguration["ModsDir"] ) )
            {
                var modDirName = Path.GetFileName(modDir);
                if ( modDirName == ".bin" || modDirName == ".cache" )
                    continue;

                mLogger.Information( $"Processing mod {modDirName}" );
                var modFilesDir = Path.Combine( modDir, "files" );
                ProcessModDir( modDir, modFilesDir, cache, modFilesDir, isArcDir: false );
                ++modsProcessed;
            }

            return modsProcessed;
        }

        private void ProcessModDir( string modDir, string modFilesDir, Cache cache, string dir, bool isArcDir )
        {
            foreach ( var entryName in Directory.EnumerateFileSystemEntries( dir ) )
            {
                var relEntryPath = GetRelativePath(modFilesDir, entryName);

                if ( File.Exists( entryName ) )
                {
                    if ( !isArcDir )
                    {
                        // Copy file to out directory
                        mIO.CopyFile( entryName, GetBinFilePathFromRelPath( relEntryPath ), true );
                    }
                }
                else
                {
                    if ( cache.ContainerDirs.ContainsKey( relEntryPath ) )
                    {
                        var containerType = cache.ContainerDirs[relEntryPath];

                        // This directory is an archive
                        var entryBinDir = GetBinFilePathFromRelPath( relEntryPath );

                        switch ( containerType )
                        {
                            case ContainerType.ARC:
                                {

                                    if ( !Directory.Exists( entryBinDir ) )
                                    {
                                        // Copy original files from cache to out directory
                                        var entryCacheDir = GetCacheFilePathFromRelPath( relEntryPath );
                                        Debug.Assert( Directory.Exists( entryCacheDir ) );
                                        CopyDirectoryContents( entryCacheDir, entryBinDir );
                                    }

                                    // Overwrite the files in the out directory
                                    CopyDirectoryContents( entryName, entryBinDir );

                                    // Recurse into contents of archive
                                    var arcRootDir = Directory.EnumerateDirectories(entryName).SingleOrDefault();
                                    if ( arcRootDir == null )
                                        throw new InvalidOperationException( $"Unable to determine archive root directory for {entryName}. Make sure there is only 1 directory inside." );

                                    ProcessModDir( modDir, modFilesDir, cache, arcRootDir, isArcDir: true );
                                }
                                break;
                            case ContainerType.AW:
                                {
                                    // Overwrite the files in the out directory
                                    CopyDirectoryContents( entryName, entryBinDir );
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // Recurse
                        ProcessModDir( modDir, modFilesDir, cache, entryName, isArcDir );
                    }
                }
            }
        }

        public enum FileMissingPolicy
        {
            CopyFromCache
        }

        private string GetFileFromOutputDir( string relPath, FileMissingPolicy policy )
        {
            var outFilePath = GetBinFilePathFromRelPath( relPath);
            if ( !File.Exists( outFilePath ) )
            {
                switch ( policy )
                {
                    case FileMissingPolicy.CopyFromCache:
                        {
                            var cacheFilePath = GetCacheFilePathFromRelPath(relPath);
                            mIO.CopyFile( cacheFilePath, outFilePath, true );
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return outFilePath;
        }

        private void ProcessBinDir( string outputDir, Cache cache )
        {
            mLogger.Information( "Processing output" );

            // Recursively process directories
            var archiveStack = new Stack<(string, string, ContainerType)>();
            void ProcessOutputDirRecursive( string curDir )
            {
                foreach ( var dirPath in Directory.EnumerateDirectories( curDir ) )
                {
                    var relDirPath = GetRelativePath( outputDir, dirPath );

                    if ( cache.ContainerDirs.ContainsKey( relDirPath ) )
                    {
                        // This directory is an archive
                        // Build it
                        archiveStack.Push( (dirPath, relDirPath, cache.ContainerDirs[relDirPath]) );
                    }

                    // Recurse
                    ProcessOutputDirRecursive( dirPath );
                }
            }

            ProcessOutputDirRecursive( outputDir );

            // Pack archives LIFO so nested archives play nicely
            while ( archiveStack.Count > 0 )
            {
                (var dirPath, var relDirPath, var containerType) = archiveStack.Pop();

                switch ( containerType )
                {
                    case ContainerType.ARC:
                        PackARC( dirPath, relDirPath );
                        break;
                    case ContainerType.AW:
                        {
                            // Get base files
                            var baaFilePath = GetFileFromOutputDir( "AudioRes\\GCKart.baa", FileMissingPolicy.CopyFromCache );
                            var awFilePath = GetFileFromOutputDir( Path.ChangeExtension( relDirPath, ".aw"), FileMissingPolicy.CopyFromCache );

                            // Build BAA patch
                            mLogger.Information( "Build BAA patch" );
                            BAAPatch patch;
                            using ( var baaStream = File.OpenRead( baaFilePath ) )
                            using ( var awStream = File.OpenRead( awFilePath ) )
                            {
                                var baaPatchBuilder = new BAAPatchBuilder(mLogger, mConfiguration);
                                baaPatchBuilder.SetBAAStream( baaStream );
                                baaPatchBuilder.PatchAW( awStream, dirPath );
                                patch = baaPatchBuilder.Build();
                            }

                            // Overwrite BAA in output directory
                            using ( var baaStream = File.Create( baaFilePath ) )
                            {
                                mLogger.Information( $"Writing {baaFilePath}" );
                                patch.BAAStream.CopyTo( baaStream );
                            }

                            // Overwrite wave files in output directory
                            foreach ( var awPatch in patch.AWStreams )
                            {
                                var outFilePath = GetBinFilePathFromRelPath( "AudioRes/Waves/" + awPatch.Key );
                                mLogger.Information( $"Writing {outFilePath}" );
                                using ( var awStream = File.Create( outFilePath ) )
                                    awPatch.Value.CopyTo( awStream );
                            }

                            // Delete directory from output
                            mIO.DeleteDirectory( dirPath );
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void PackARC( string dirPath, string relDirPath )
        {
            var arcRootDirPath = Directory.EnumerateDirectories(dirPath).FirstOrDefault();
            if ( arcRootDirPath == null )
            {
                mLogger.Error( $"Expected ARC root directory inside {dirPath}. Using directory as root instead." );
                arcRootDirPath = dirPath;
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo( mConfiguration["ArcPackPath"], arcRootDirPath ) { RedirectStandardOutput = false, CreateNoWindow = true };
            process.Start();
            mLogger.Information( $"Packing {relDirPath}" );
            process.WaitForExit();

            mIO.CopyFile( arcRootDirPath + ".arc", dirPath + ".arc", true );
            mIO.DeleteDirectory( dirPath );
        }

        private void CopyDirectoryContents( string sourceDir, string destDir )
        {
            mLogger.Information( $"Copying {sourceDir} to {destDir}" );

            // Now create all of the directories
            foreach ( string dirPath in Directory.GetDirectories( sourceDir, "*",
                SearchOption.AllDirectories ) )
                mIO.CreateDirectory( dirPath.Replace( sourceDir, destDir ) );

            // Copy all the files & replace any files with the same name
            foreach ( string newPath in Directory.GetFiles( sourceDir, "*.*",
                SearchOption.AllDirectories ) )
                mIO.CopyFile( newPath, newPath.Replace( sourceDir, destDir ), true );
        }

        private int CacheFiles( string inputFilesDir, string outputFilesDir, Cache cache )
        {
            var runProcessJobs = new List<RunProcessJob>();
            foreach ( var filePath in Directory.EnumerateFiles( inputFilesDir, "*.*", SearchOption.AllDirectories ) )
                CacheFile( inputFilesDir, outputFilesDir, runProcessJobs, filePath, cache );

            WaitForRunProcessJobCompletion( runProcessJobs );
            return runProcessJobs.Count;
        }

        private void WaitForRunProcessJobCompletion( List<RunProcessJob> runProcessJobs )
        {
            // Wait for completion & cleanup
            foreach ( var job in runProcessJobs )
            {
                job.Process.WaitForExit();

                foreach ( var item in job.TemporaryFiles )
                    mIO.DeleteFile( item );           
            }
        }

        private void CacheFile( string inputFilesDir, string outputFilesDir,
            List<RunProcessJob> runProcessJobs, string filePath, Cache cache )
        {
            // Either we copy the archive or we copy the extracted contents
            // copying the archive is faster so we'll do that instead and later delete it.
            var extension = Path.GetExtension( filePath ).ToLowerInvariant();
            var relFilePath = GetRelativePath( inputFilesDir, filePath );
            var relDirPath = Path.Combine( Path.GetDirectoryName( relFilePath ), Path.GetFileNameWithoutExtension( relFilePath ) );
            var outArcDirPath = Path.Combine( outputFilesDir, relDirPath );
            var outArcFilePath = Path.Combine( outArcDirPath, Path.GetFileName( filePath ) );

            switch ( extension )
            {
                case ".arc":
                    CacheARC( runProcessJobs, filePath, cache, relFilePath, relDirPath, outArcDirPath, outArcFilePath );
                    break;

                case ".aw":
                    mIO.CopyFile( filePath, GetCacheFilePathFromRelPath( relFilePath ), true );
                    cache.ContainerDirs[relDirPath] = ContainerType.AW;
                    break;

                case ".baa":
                    mIO.CopyFile( filePath, GetCacheFilePathFromRelPath( relFilePath ), true );
                    break;

                default:
                    break;
            }
        }

        private void CacheARC( List<RunProcessJob> runProcessJobs, string filePath, Cache cache, string relFilePath, string relDirPath, string cacheArcDirPath, string cacheArcFilePath )
        {
            cache.ContainerDirs[relDirPath] = ContainerType.ARC;

            //if ( !Directory.Exists( cacheArcDirPath ) )
            {
                mLogger.Information( $"Unpacking {relFilePath}" );

                // Copy archive to cache (but only if not already in cache)
                mIO.CreateDirectory( cacheArcDirPath );
                if ( !File.Exists( cacheArcFilePath ) )
                {
                    mIO.CopyFile( filePath, cacheArcFilePath, false );

                    // If the directory the arc is being extracted to is in the same directory as the file itself, delete the original file
                    if ( Path.GetDirectoryName( filePath ).Equals( Path.GetDirectoryName( cacheArcDirPath ), StringComparison.InvariantCultureIgnoreCase ) )
                        mIO.DeleteFile( filePath );
                }

                // Extract it
                var process = new Process();
                process.StartInfo = new ProcessStartInfo( mConfiguration["ArcExtractPath"], Path.GetFullPath( cacheArcFilePath ) )
                {
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };
                process.Start();
                runProcessJobs.Add( new RunProcessJob( process ) { TemporaryFiles = { Path.GetFullPath( cacheArcFilePath ) } } );
            }
        }

        public static string GetRelativePath( string referencePath, string filePath )
        {
            var fullFilePath = Path.GetFullPath(filePath);
            var fullReferencePath = Path.GetFullPath(referencePath);
            if ( fullFilePath.StartsWith( fullReferencePath ) )
            {
                return fullFilePath.Substring( fullReferencePath.Length + 1 );
            }
            else
            {
                var fileUri = new Uri(filePath);
                var referenceUri = new Uri(referencePath);
                return referenceUri.MakeRelativeUri( fileUri ).ToString();
            }
        }

        public static bool PathIsSame(string a, string b)
        {
            return Path.GetFullPath( a ).Equals( Path.GetFullPath( b ), StringComparison.InvariantCultureIgnoreCase );
        }
    }
}
