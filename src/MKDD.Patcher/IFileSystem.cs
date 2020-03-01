using System.Collections.Generic;
using System.IO;

namespace MKDD.Patcher
{
    public interface IFileSystem
    {
        string BasePath { get; }

        Stream CreateFile( string name );
        Stream OpenFile( string name, FileMode mode, FileAccess access );
        void CopyFile( string sourceName, string destName, bool overwrite );
        void CopyFile( string sourceName, IFileSystem destFs, string destName, bool overwrite );
        void DeleteFile( string name );
        bool FileExists( string name );

        void CreateDirectory( string name );
        void CopyDirectory( string sourcePath, string destPath, bool overwrite );
        void CopyDirectory( string sourcePath, IFileSystem destFs, string destPath, bool overwrite );
        void DeleteDirectory( string name, bool recursive );
        bool DirectoryExists( string name );

        string GetPhysicalPath( string name );
        string GetVirtualPath( string name );

        IEnumerable<string> EnumerateFileSystemEntries( string directory, string searchPattern, SearchOption searchOption );
        IEnumerable<string> EnumerateFiles( string directory, string searchPattern, SearchOption searchOption );
        IEnumerable<string> EnumerateDirectories( string directory, string searchPattern, SearchOption searchOption );
    }
}
