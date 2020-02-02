﻿using System.IO;
using Serilog;

namespace MKDD.Patcher
{
    public class LoggedIO
    {
        private ILogger mLogger;

        public LoggedIO(ILogger logger)
        {
            mLogger = logger;
        }

        public void CreateDirectory( string directory )
        {
            if ( !Directory.Exists( directory ) )
            {
                mLogger.Information( $"Creating directory: {directory}" );
                Directory.CreateDirectory( directory );
            }
        }

        public void DeleteDirectory( string directory )
        {
            mLogger.Information( $"Deleting directory: {directory}" );
            Directory.Delete( directory, true );
        }

        public void CopyFile( string srcFilePath, string dstFilePath, bool overwrite )
        {
            if ( overwrite && File.Exists( dstFilePath ) )
            {
                mLogger.Information( $"Overwriting file {dstFilePath} with {srcFilePath}" );
            }
            else
            {
                mLogger.Information( $"Copying file {srcFilePath} to {dstFilePath}" );
            }

            var fullSrcFilePath = Path.GetFullPath(srcFilePath);
            var fullDstFilePath = Path.GetFullPath(dstFilePath);
            if ( !fullSrcFilePath.Equals( fullDstFilePath, System.StringComparison.InvariantCultureIgnoreCase ) )
            {
                Directory.CreateDirectory( Path.GetDirectoryName( dstFilePath ) );
                File.Copy( srcFilePath, dstFilePath, overwrite );
            }
        }

        public void DeleteFile( string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                mLogger.Information( $"Deleting file {filePath}" );
                File.Delete( filePath );
            }
        }
    }
}
