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
    public partial class Patcher
    {
        private const string BAA_PATH = "AudioRes\\GCKart.baa";
        private readonly ILogger mLogger;
        private readonly PatcherConfig mConfiguration;
        private readonly ModDb mModDb;
        private readonly DirectoryFileSystem mOutDir;
        private readonly DirectoryFileSystem mFilesDir;
        private readonly DirectoryFileSystem mCacheFilesDir;
        private readonly DirectoryFileSystem mBinDir;

        public Patcher( ILogger logger, PatcherConfig configuration, ModDb modDb )
        {
            mLogger = logger;
            mConfiguration = configuration;
            mModDb = modDb;

            mLogger.Information( "Validating configuration" );
            if ( !ValidateConfig() )
                throw new ArgumentException( "Invalid configuration", nameof( configuration ) );

            mOutDir = new DirectoryFileSystem( mLogger, mConfiguration.OutDir );
            mFilesDir = new DirectoryFileSystem( mLogger, mConfiguration.FilesDir );
            mCacheFilesDir = new DirectoryFileSystem( mLogger, Path.GetFullPath( Path.Combine( mConfiguration.CacheDir, "files" ) ) );
            mBinDir = new DirectoryFileSystem( mLogger, mConfiguration.BinDir );
        }

        public void Patch( MergeOrder order, List<string> modFilter = null )
        {
            var cache = InitializeCache();
            var modsProcessed = ProcessMods(cache, order, modFilter);
            if ( modsProcessed > 0 )
            {
                ProcessBinDir( mBinDir, cache );
            }
            else
            {
                mLogger.Warning( "No mods available to install" );
            }

            if (!PathHelper.AreEqual(mConfiguration.BinDir, mConfiguration.OutDir))
            {
                // Copy bin directory contents to out directory
                mBinDir.CopyDirectory( ".", mOutDir, ".", true );
            }

            mLogger.Information( "Patching done!" );
        }

        private bool ValidateConfig()
        {
            // Validate config
            if ( !Directory.Exists( mConfiguration.FilesDir ) )
            {
                mLogger.Fatal( "Files directory not found. Make sure that the executable is placed in the right directory & the config is set up correctly." );
                return false;
            }

            if ( !Directory.Exists( mConfiguration.ModsDir ) )
            {
                mLogger.Warning( "Mod directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( mConfiguration.ModsDir );
            }

            if ( !Directory.Exists( mConfiguration.BinDir) )
            {
                mLogger.Warning( "Bin directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( mConfiguration.BinDir );
            }

            if ( !Directory.Exists( mConfiguration.OutDir ) )
            {
                mLogger.Warning( "Out directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( mConfiguration.OutDir );
            }

            if ( !Directory.Exists( mConfiguration.CacheDir ) )
            {
                mLogger.Warning( "Cache directory doesn't exist. Creating new directory..." );
                Directory.CreateDirectory( mConfiguration.CacheDir );
            }

            if ( !File.Exists( mConfiguration.ArcPackPath ) )
            {
                mLogger.Fatal( "Can't find ArcPack.exe. Verify the path in the config." );
                return false;
            }

            if ( !File.Exists( mConfiguration.ArcExtractPath ) )
            {
                mLogger.Fatal( "Can't find ArcExtract.exe. Verify the path in the config." );
                return false;
            }

            return true;
        }

        private Cache InitializeCache()
        {
            // Try to load cache
            var cacheJsonPath = Path.Combine( mConfiguration.CacheDir, "cache.json" );
            Cache cache = null;
            if ( File.Exists( cacheJsonPath ) )
                cache = JsonConvert.DeserializeObject<Cache>( File.ReadAllText( cacheJsonPath ) );

            if ( cache == null || cache.Version != Cache.CURRENT_VERSION )
            {
                // Rebuild cache if it doesn't exist, or is outdated.
                cache = new Cache();
                mLogger.Information( "Building cache... please wait" );
                Directory.Delete( mConfiguration.CacheDir );
                CacheFiles( mFilesDir, cache );

                // Process unpacked ARCs
                while ( true )
                {
                    if ( CacheFiles( mFilesDir, cache ) == 0 )
                        break;
                }

                File.WriteAllText( cacheJsonPath, JsonConvert.SerializeObject( cache, Formatting.Indented ) );
            }

            cache.ContainerDirs = new Dictionary<string, ContainerType>( cache.ContainerDirs, StringComparer.InvariantCultureIgnoreCase );
            return cache;
        }

        private int ProcessMods( Cache cache, MergeOrder order, List<string> modFilter = null )
        {
            mLogger.Information( "Processing mods" );

            var filteredMods = new List<(int Index, ModInfo ModInfo)>();

            // Iterate over mods
            foreach ( var mod in mModDb.Mods )
            {
                // TODO: maybe use a GUID instead of the title for matching
                if ( modFilter == null || modFilter.Contains( mod.Title ) )
                {
                    var index = modFilter.FindIndex( x => x.Equals( mod.Title, StringComparison.InvariantCultureIgnoreCase ));
                    filteredMods.Add( (index, mod) );
                }
            }

            // Order the mods we collected based on their index in the filter list
            var orderedMods = order == MergeOrder.TopToBottom ? 
                    filteredMods.OrderBy(x => x.Index) : 
                    filteredMods.OrderByDescending(x => x.Index);

            foreach ( var mod in orderedMods )
            {
                mLogger.Information($"Processing mod {mod.ModInfo.Title}");
                var modFilesDir = new DirectoryFileSystem(mLogger, mod.ModInfo.FilesDir);
                ProcessModDir( mod.ModInfo, modFilesDir, cache, ".", ModDirectoryType.Normal );
            }

            return filteredMods.Count;
        }

        private enum ModDirectoryType
        {
            Normal,
            Archive
        }

        private void ProcessModDir( ModInfo modInfo, IFileSystem modFilesDir, Cache cache, string dir, ModDirectoryType dirType )
        {
            foreach ( var entryName in modFilesDir.EnumerateFileSystemEntries( dir, "*.*", SearchOption.TopDirectoryOnly ) )
            {
                if ( modFilesDir.FileExists( entryName ) )
                {
                    if ( dirType != ModDirectoryType.Archive )
                    {
                        // Copy file to out directory
                        modFilesDir.CopyFile( entryName, mBinDir, entryName, true );
                    }
                }
                else
                {
                    if ( cache.ContainerDirs.ContainsKey( entryName ) )
                    {
                        // This directory is a container
                        var containerType = cache.ContainerDirs[entryName];
                        var modContainerInfo = modInfo.Containers.FirstOrDefault(x => PathHelper.AreEqual(x.Path, entryName));
                        var mergeContents = modContainerInfo == null || modContainerInfo.Merge;

                        switch ( containerType )
                        {
                            case ContainerType.ARC:
                                {
                                    if ( mergeContents && !mBinDir.DirectoryExists( entryName ) )
                                    {
                                        // Copy original files from cache to out directory
                                        Debug.Assert( mCacheFilesDir.DirectoryExists( entryName ) );
                                        mCacheFilesDir.CopyDirectory( entryName, mBinDir, entryName, true );
                                    }

                                    // Overwrite the files in the out directory
                                    modFilesDir.CopyDirectory( entryName, mBinDir, entryName, true );

                                    // Recurse into contents of archive
                                    var arcRootDir = modFilesDir.EnumerateDirectories(entryName, "*.*", SearchOption.TopDirectoryOnly).SingleOrDefault();
                                    if ( arcRootDir == null )
                                        throw new InvalidOperationException( $"Unable to determine archive root directory for {entryName}. Make sure there is only 1 directory inside." );

                                    ProcessModDir( modInfo, modFilesDir, cache, arcRootDir, ModDirectoryType.Archive );
                                }
                                break;
                            case ContainerType.AW:
                                {
                                    // Overwrite the files in the out directory
                                    modFilesDir.CopyDirectory( entryName, mBinDir, entryName, true );
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        // Recurse
                        ProcessModDir( modInfo, modFilesDir, cache, entryName, ModDirectoryType.Normal );
                    }
                }
            }
        }

        private void ProcessBinDir( IFileSystem binDir, Cache cache )
        {
            mLogger.Information( "Processing bin directory" );

            // Recursively process directories
            var archiveStack = new Stack<(string, string, ContainerType)>();
            void ProcessOutputDirRecursive( string curDir )
            {
                foreach ( var dirPath in binDir.EnumerateDirectories( curDir, "*.*", SearchOption.TopDirectoryOnly ) )
                {
                    if ( cache.ContainerDirs.ContainsKey( dirPath ) )
                    {
                        // This directory is an archive
                        // Build it
                        archiveStack.Push( (dirPath, dirPath, cache.ContainerDirs[dirPath]) );
                    }

                    // Recurse
                    ProcessOutputDirRecursive( dirPath );
                }
            }

            ProcessOutputDirRecursive( "." );

            // Pack archives LIFO so nested archives play nicely
            while ( archiveStack.Count > 0 )
            {
                (var dirPath, var relDirPath, var containerType) = archiveStack.Pop();

                switch ( containerType )
                {
                    case ContainerType.ARC:
                        PackARC( binDir, relDirPath );
                        break;
                    case ContainerType.AW:
                        PatchAW( dirPath, relDirPath );
                        break;
                    default:
                        break;
                }
            }
        }

        private void PatchAW( string dirPath, string relDirPath )
        {
            // Get base files
            if ( !mBinDir.FileExists( BAA_PATH ) )
                mCacheFilesDir.CopyFile( BAA_PATH, mBinDir, BAA_PATH, true );

            var awFileName = Path.ChangeExtension( relDirPath, ".aw");
            if ( !mBinDir.FileExists( awFileName ) )
                mCacheFilesDir.CopyFile( awFileName, mBinDir, awFileName, true );

            // Build BAA patch
            mLogger.Information( "Build BAA patch" );
            BAAPatch patch;
            using ( var baaStream = mBinDir.OpenFile( BAA_PATH, FileMode.Open, FileAccess.Read ) )
            using ( var awStream = mBinDir.OpenFile( awFileName, FileMode.Open, FileAccess.Read ) )
            {
                var baaPatchBuilder = new BAAPatchBuilder(mLogger);
                baaPatchBuilder.SetBAAStream( baaStream );
                baaPatchBuilder.PatchAW( awStream, mBinDir, dirPath );
                patch = baaPatchBuilder.Build();
            }

            // Overwrite BAA in output directory
            using ( var baaStream = mBinDir.CreateFile( BAA_PATH ) )
            {
                mLogger.Information( $"Writing {BAA_PATH}" );
                patch.BAAStream.CopyTo( baaStream );
            }

            // Overwrite wave files in output directory
            foreach ( var awPatch in patch.AWStreams )
            {
                var patchedAwFileName = "AudioRes/Waves/" + awPatch.Key;
                mLogger.Information( $"Writing {awFileName}" );
                using ( var awStream = mBinDir.CreateFile( patchedAwFileName ) )
                    awPatch.Value.CopyTo( awStream );
            }

            // Delete directory from output
            mBinDir.DeleteDirectory( dirPath, true );
        }

        private void PackARC( IFileSystem fs, string dirPath )
        {
            var arcRootDirPath = fs.EnumerateDirectories(dirPath, "*.*", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if ( arcRootDirPath == null )
            {
                mLogger.Error( $"Expected ARC root directory inside {dirPath}. Using directory as root instead." );
                arcRootDirPath = dirPath;
            }

            var physArcRootDirPath = fs.GetPhysicalPath( arcRootDirPath );
            var process = new Process();
            process.StartInfo = new ProcessStartInfo( Path.GetFullPath( mConfiguration.ArcPackPath ), physArcRootDirPath ) { RedirectStandardOutput = false, CreateNoWindow = true };
            process.Start();
            mLogger.Information( $"Packing {dirPath}" );
            process.WaitForExit();

            fs.CopyFile( arcRootDirPath + ".arc", dirPath + ".arc", true );
            fs.DeleteDirectory( dirPath, true );
        }

        private int CacheFiles( IFileSystem filesDir, Cache cache )
        {
            var runProcessJobs = new List<RunProcessJob>();
            foreach ( var filePath in filesDir.EnumerateFiles( ".", "*.*", SearchOption.AllDirectories ) )
                CacheFile( runProcessJobs, filesDir, filePath, cache );

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
                    File.Delete( item );           
            }
        }

        private void CacheFile( List<RunProcessJob> runProcessJobs, IFileSystem filesDir, string filePath, Cache cache )
        {
            // Either we copy the archive or we copy the extracted contents
            // copying the archive is faster so we'll do that instead and later delete it.
            var extension = Path.GetExtension( filePath ).ToLowerInvariant();
            var dirPath = PathHelper.GetFilePathWithoutExtension( filePath );

            switch ( extension )
            {
                case ".arc":
                    CacheARC( runProcessJobs, filesDir, filePath, dirPath, cache );
                    break;

                case ".aw":
                    filesDir.CopyFile( filePath, mCacheFilesDir, filePath, true );
                    cache.ContainerDirs[dirPath] = ContainerType.AW;
                    break;

                case ".baa":
                    filesDir.CopyFile( filePath, mCacheFilesDir, filePath, true );
                    break;

                default:
                    break;
            }
        }

        private void CacheARC( List<RunProcessJob> runProcessJobs, IFileSystem filesDir, string filePath, string dirPath, Cache cache )
        {
            cache.ContainerDirs[filePath] = ContainerType.ARC;
            mLogger.Information( $"Unpacking {filePath}" );

            // Copy archive to cache (but only if not already in cache)
            mCacheFilesDir.CreateDirectory( dirPath );
            if ( !mCacheFilesDir.FileExists( filePath ) )
            {
                filesDir.CopyFile( filePath, mCacheFilesDir, filePath, false );

                // If the directory the arc is being extracted to is in the same directory as the file itself, delete the original file
                if ( PathHelper.AreInSameDirectory( filePath, dirPath ) )
                    mCacheFilesDir.DeleteFile( filePath );
            }

            // Extract it
            var process = new Process();
            process.StartInfo = new ProcessStartInfo( mConfiguration.ArcExtractPath, Path.GetFullPath( mCacheFilesDir.GetPhysicalPath( filePath ) ) )
            {
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };
            process.Start();
            runProcessJobs.Add( new RunProcessJob( process ) { TemporaryFiles = { Path.GetFullPath( mCacheFilesDir.GetPhysicalPath( filePath ) ) } } );
        }
    }
}
