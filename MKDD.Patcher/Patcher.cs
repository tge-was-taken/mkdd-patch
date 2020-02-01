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
    public class Patcher
    {
        private readonly ILogger mLogger;
        private readonly IConfiguration mConfiguration;

        public Patcher( ILogger logger, IConfiguration configuration )
        {
            mLogger = logger;
            mConfiguration = configuration;

            mLogger.Information( "Validating configuration" );
            if ( !ValidateConfig() )
                throw new ArgumentException( "Invalid configuration", nameof( configuration ) );
        }

        private void CreateDirectory( string directory )
        {
            if ( !Directory.Exists( directory ) )
            {
                mLogger.Information( $"Creating directory: {directory}" );
                Directory.CreateDirectory( directory );
            }
        }

        private void DeleteDirectory( string directory )
        {
            mLogger.Information( $"Deleting directory: {directory}" );
            Directory.Delete( directory, true );
        }

        private void CopyFile( string srcFilePath, string dstFilePath, bool overwrite )
        {
            if ( overwrite && File.Exists( dstFilePath ) )
            {
                mLogger.Information( $"Overwriting file {dstFilePath} with {srcFilePath}" );
            }
            else
            {
                mLogger.Information( $"Copying file {srcFilePath} to {dstFilePath}" );
            }

            File.Copy( srcFilePath, dstFilePath, overwrite );
        }

        private void DeleteFile( string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                mLogger.Information( $"Deleting file {filePath}" );
                File.Delete( filePath );
            }
        }

        public void Patch()
        {
            var cacheFilesDir = Path.GetFullPath( Path.Combine( mConfiguration["CacheDir"], "files" ) );
            var cache = InitializeCache( mConfiguration["FilesDir"], cacheFilesDir );
            var modsProcessed = ProcessMods( mConfiguration["FilesDir"], cacheFilesDir, cache.ArchiveDirs );
            if ( modsProcessed > 0 )
            {
                ProcessBinDir( mConfiguration["OutDir"], cache.ArchiveDirs );
            }
            else
            {
                mLogger.Warning( "No mods available to install" );
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
                CreateDirectory( mConfiguration["ModsDir"] );
            }

            if ( !Directory.Exists( mConfiguration["OutDir"] ) )
            {
                mLogger.Warning( "Out directory doesn't exist. Creating new directory..." );
                CreateDirectory( mConfiguration["OutDir"] );
            }

            if ( !Directory.Exists( mConfiguration["CacheDir"] ) )
            {
                mLogger.Warning( "Cache directory doesn't exist. Creating new directory..." );
                CreateDirectory( mConfiguration["CacheDir"] );
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
                DeleteDirectory( mConfiguration["CacheDir"] );
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

        private int ProcessMods( string rootFilesDir, string cacheFilesDir, HashSet<string> archiveDirSet )
        {
            mLogger.Information( "Processing mods" );

            CreateDirectory( mConfiguration["OutDir"] );

            // Iterate over mods
            CreateDirectory( mConfiguration["ModsDir"] );
            int modsProcessed = 0;
            foreach ( var modDir in Directory.EnumerateDirectories( mConfiguration["ModsDir"] ) )
            {
                var modDirName = Path.GetFileName(modDir);
                if ( modDirName == ".bin" || modDirName == ".cache" )
                    continue;

                mLogger.Information( $"Processing mod {modDirName}" );
                var modFilesDir = Path.Combine( modDir, "files" );
                ProcessModDir( rootFilesDir, cacheFilesDir, modDir, modFilesDir, archiveDirSet, Path.Combine( modDir, "files" ), isArcDir: false );
                ++modsProcessed;
            }

            return modsProcessed;
        }

        private void ProcessModDir( string rootFilesDir, string cacheFilesDir, string modDir, string modFilesDir, HashSet<string> archiveDirSet, string dir, bool isArcDir )
        {
            foreach ( var entryName in Directory.EnumerateFileSystemEntries( dir ) )
            {
                var relEntryPath = GetRelativePath(modFilesDir, entryName);

                if ( File.Exists( entryName ) )
                {
                    if ( !isArcDir )
                    {
                        // Copy file to out directory
                        CopyFile( entryName, Path.Combine( mConfiguration["OutDir"], relEntryPath ), true );
                    }
                }
                else
                {
                    if ( archiveDirSet.Contains( relEntryPath ) )
                    {
                        // This directory is an archive
                        var entryBinDir = Path.Combine( mConfiguration["OutDir"], relEntryPath );

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

                        ProcessModDir( rootFilesDir, cacheFilesDir, modDir, modFilesDir, archiveDirSet, arcRootDir, isArcDir: true );
                    }
                    else
                    {
                        // Recurse
                        ProcessModDir( rootFilesDir, cacheFilesDir, modDir, modFilesDir, archiveDirSet, entryName, isArcDir );
                    }
                }
            }
        }

        private void ProcessBinDir( string dir, HashSet<string> archiveDirSet )
        {
            mLogger.Information( "Processing output" );

            // Recursively process directories
            var archiveStack = new Stack<(string, string)>();
            void ProcessBinDirRecursive( string curDir )
            {
                foreach ( var dirPath in Directory.EnumerateDirectories( curDir ) )
                {
                    var relDirPath = GetRelativePath( dir, dirPath );

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
            while ( archiveStack.Count > 0 )
            {
                (var dirPath, var relDirPath) = archiveStack.Pop();
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

                CopyFile( arcRootDirPath + ".arc", dirPath + ".arc", true );
                DeleteDirectory( dirPath );
            }
        }

        private void CopyDirectoryContents( string sourceDir, string destDir )
        {
            mLogger.Information( $"Copying {sourceDir} to {destDir}" );

            // Now create all of the directories
            foreach ( string dirPath in Directory.GetDirectories( sourceDir, "*",
                SearchOption.AllDirectories ) )
                CreateDirectory( dirPath.Replace( sourceDir, destDir ) );

            // Copy all the files & replace any files with the same name
            foreach ( string newPath in Directory.GetFiles( sourceDir, "*.*",
                SearchOption.AllDirectories ) )
                CopyFile( newPath, newPath.Replace( sourceDir, destDir ), true );
        }

        private int UnpackARCs( string inputFilesDir, string outputFilesDir, Cache cache )
        {
            var processes = new List<(Process Process, string CacheArcFilePath)>();
            foreach ( var filePath in Directory.EnumerateFiles( inputFilesDir, "*.arc", SearchOption.AllDirectories ) )
            {
                UnpackARC( inputFilesDir, outputFilesDir, processes, filePath, cache );
            }

            WaitForExtractionAndCleanup( processes );
            return processes.Count;
        }

        private void WaitForExtractionAndCleanup( List<(Process Process, string CacheArcFilePath)> processes )
        {
            // Wait for completion & cleanup
            foreach ( (var process, string cacheArcFilePath) in processes )
            {
                process.WaitForExit();
                DeleteFile( cacheArcFilePath );
            }
        }

        private void UnpackARC( string inputFilesDir, string outputFilesDir, string filePath, Cache cache )
        {
            var processes = new List<(Process Process, string CacheArcFilePath)>();
            UnpackARC( inputFilesDir, outputFilesDir, processes, filePath, cache );
            WaitForExtractionAndCleanup( processes );
        }

        private void UnpackARC( string inputFilesDir, string outputFilesDir,
            List<(Process Process, string CacheArcFilePath)> processes, string filePath, Cache cache )
        {
            // Either we copy the archive or we copy the extracted contents
            // copying the archive is faster so we'll do that instead and later delete it.
            var relFilePath = GetRelativePath( inputFilesDir, filePath );
            var relDirPath = Path.Combine( Path.GetDirectoryName( relFilePath ), Path.GetFileNameWithoutExtension( relFilePath ) );
            var cacheArcDirPath = Path.Combine( outputFilesDir, relDirPath );
            var cacheArcFilePath = Path.Combine( cacheArcDirPath, Path.GetFileName( filePath ) );
            cache.ArchiveDirs.Add( relDirPath );

            if ( !Directory.Exists( cacheArcDirPath ) )
            {
                mLogger.Information( $"Unpacking {relFilePath}" );

                // Copy archive to cache (but only if not already in cache)
                CreateDirectory( cacheArcDirPath );
                if ( !File.Exists( cacheArcFilePath ) )
                {
                    CopyFile( filePath, cacheArcFilePath, false );

                    // If the directory the arc is being extracted to is in the same directory as the file itself, delete the original file
                    if ( Path.GetDirectoryName( filePath ).Equals( Path.GetDirectoryName( cacheArcDirPath ), StringComparison.InvariantCultureIgnoreCase ) )
                        DeleteFile( filePath );
                }

                // Extract it
                var process = new Process();
                process.StartInfo = new ProcessStartInfo( mConfiguration["ArcExtractPath"], Path.GetFullPath( cacheArcFilePath ) )
                {
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };
                process.Start();
                processes.Add( (process, Path.GetFullPath( cacheArcFilePath )) );
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
    }
}
