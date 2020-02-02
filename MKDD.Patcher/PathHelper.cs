using System;
using System.IO;

namespace MKDD.Patcher
{
    public static class PathHelper
    {
        /// <summary>
        /// Gets the relative path based on the given reference path.
        /// </summary>
        /// <param name="referencePath"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets if both paths are equal to one another; they point to the same file system object.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Same( string a, string b )
        {
            return Path.GetFullPath( a ).Equals( Path.GetFullPath( b ), StringComparison.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Gets if both paths reside in the same directory.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreInSameDirectory( string a, string b )
        {
            return ( Path.GetDirectoryName( a ).Equals( Path.GetDirectoryName( b ), StringComparison.InvariantCultureIgnoreCase ) );
        }

        /// <summary>
        /// Gets the full file path without the extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFilePathWithoutExtension( string filePath )
        {
            return Path.Combine( Path.GetDirectoryName( filePath ), Path.GetFileNameWithoutExtension( filePath ) );
        }

        public static bool HasExtension( string filePath, string extension )
        {
            return Path.GetExtension( filePath ).Equals( extension, StringComparison.InvariantCultureIgnoreCase );
        }
    }
}
