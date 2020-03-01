using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace MKDD.Patcher
{
    public class DirectoryFileSystem : IFileSystem
    {
        private ILogger mLogger;

        public string BasePath { get; }

        public DirectoryFileSystem( ILogger logger, string directoryPath )
        {
            mLogger = logger;
            BasePath = directoryPath;
        }

        public void CopyDirectory( string sourcePath, string destPath, bool overwrite )
        {
            CopyDirectory( sourcePath, this, destPath, overwrite );
        }

        public void CopyFile( string sourceName, string destName, bool overwrite )
        {
            File.Copy( GetPhysicalPath( sourceName ), GetPhysicalPath( destName ), overwrite );
        }

        public void CreateDirectory( string name )
        {
            Directory.CreateDirectory( GetPhysicalPath( name ) );
        }

        public Stream CreateFile( string name )
        {
            var path = GetPhysicalPath(name);
            Directory.CreateDirectory( Path.GetDirectoryName( path ) );
            return File.Create( path );
        }

        public void DeleteDirectory( string name, bool recursive )
        {
            Directory.Delete( GetPhysicalPath( name ), recursive );
        }

        public void DeleteFile( string name )
        {
            File.Delete( GetPhysicalPath( name ) );
        }

        public bool DirectoryExists( string name )
        {
            return Directory.Exists( GetPhysicalPath( name ) );
        }

        public IEnumerable<string> EnumerateDirectories( string directory, string searchPattern, SearchOption searchOption )
        {
            return Directory.EnumerateDirectories( GetPhysicalPath( directory ), searchPattern, searchOption )
                .Select( x => GetVirtualPath( x ) );
        }

        public IEnumerable<string> EnumerateFiles( string directory, string searchPattern, SearchOption searchOption )
        {
            return Directory.EnumerateFiles( GetPhysicalPath( directory ), searchPattern, searchOption )
                .Select( x => GetVirtualPath( x ) );
        }

        public IEnumerable<string> EnumerateFileSystemEntries( string directory, string searchPattern, SearchOption searchOption )
        {
            return Directory.EnumerateFileSystemEntries( GetPhysicalPath( directory ), searchPattern, searchOption )
                .Select( x => GetVirtualPath( x ) );
        }

        public bool FileExists( string name )
        {
            return File.Exists( GetPhysicalPath( name ) );
        }

        public string GetPhysicalPath( string name )
        {
            return Path.Combine( BasePath, name );
        }

        public string GetVirtualPath( string name )
        {
            return PathHelper.GetRelativePath( BasePath, name );
        }

        public Stream OpenFile( string name, FileMode mode, FileAccess access )
        {
            return File.Open( GetPhysicalPath( name ), mode, access );
        }

        public void CopyFile( string sourceName, IFileSystem destFs, string destName, bool overwrite )
        {
            using ( var file = OpenFile( sourceName, FileMode.Open, FileAccess.Read ) )
            {
                if ( overwrite || !destFs.FileExists( destName ) )
                {
                    using ( var newFile = destFs.CreateFile( destName ) )
                        file.CopyTo( newFile );
                }
            }
        }

        public void CopyDirectory( string sourcePath, IFileSystem destFs, string destPath, bool overwrite )
        {
            mLogger.Information( $"Copying {sourcePath} to {destPath}" );

            // Now create all of the directories
            destFs.CreateDirectory( destPath );
            foreach ( string dirPath in EnumerateDirectories( sourcePath, "*",
                SearchOption.AllDirectories ) )
                destFs.CreateDirectory( dirPath.Replace( sourcePath, destPath ) );

            // Copy all the files & replace any files with the same name
            foreach ( string filePath in EnumerateFiles( sourcePath, "*.*",
                SearchOption.AllDirectories ) )
                CopyFile( filePath, destFs, filePath.Replace( sourcePath, destPath ), overwrite );
        }
    }
}
