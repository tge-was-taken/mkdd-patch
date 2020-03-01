using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amicitia.IO;
using Amicitia.IO.Binary;
using Microsoft.Extensions.Configuration;
using MKDD.Patcher.Audio;
using Serilog;

namespace MKDD.Patcher
{
    public class BAAPatchBuilder
    {
        private ILogger mLogger;
        private LoggedIO mIO;
        private BAAParser mBAAParser;
        private Stream mBAAStream;
        private List<FileWaveGroup> mWaveGroups;
        private Dictionary<string, Stream> mNewAWStreams;

        public BAAPatchBuilder(ILogger logger)
        {
            mLogger = logger;
            mIO = new LoggedIO( mLogger );
            mWaveGroups = new List<FileWaveGroup>();
            mNewAWStreams = new Dictionary<string, Stream>();
            mBAAParser = new BAAParser( mLogger );
        }

        public BAAPatchBuilder SetBAAStream(Stream baaStream)
        {
            mBAAStream = baaStream;
            mWaveGroups = mBAAParser.Parse( mBAAStream );
            return this;
        }

        public BAAPatchBuilder PatchAW(Stream awStream, IFileSystem fs, string replacementWavesDir)
        {
            var waveGroupName = Path.GetFileNameWithoutExtension(replacementWavesDir);
            var waveGroup = mWaveGroups.Where( x => Path.GetFileNameWithoutExtension(x.ArchiveName).Equals(waveGroupName)).FirstOrDefault();

            mLogger.Information( $"Patching wave group {waveGroupName}" );
            var waveBytes = ReadWaveGroupRawWaves( awStream, waveGroup );

            foreach ( var file in fs.EnumerateFiles( replacementWavesDir, "*.wav", SearchOption.TopDirectoryOnly ) )
            {
                var indexValue = Regex.Match(Path.GetFileNameWithoutExtension(file), @"(0_)?(?<index>\d+)")
                    .Groups["index"].Value;
                var index = int.Parse(indexValue);
                ref var waveInfo = ref waveGroup.FileWaveInfo[index].WaveInfo;

                mLogger.Information( $"Mapped {file} to index {index}" );
                using ( var fileStream = fs.OpenFile( file, FileMode.Open, FileAccess.Read ) )
                {
                    if ( PathHelper.HasExtension( file, ".wav" ) )
                    {
                        mLogger.Information( $"Encoding {file} to ADPCM" );
                        var encodeInfo = AudioHelper.EncodeWavToAdpcm( fileStream, AdpcmFormat.Adpcm4 );

                        // Update wave info
                        waveInfo.SampleRate = encodeInfo.SampleRate;
                        waveInfo.LoopStart = ( uint )encodeInfo.LoopStart;
                        waveInfo.LoopEnd = ( uint )encodeInfo.LoopEnd;
                        waveInfo.SampleCount = ( uint )encodeInfo.SampleCount;
                        var loopHistory = encodeInfo.History[waveInfo.LoopStart / 16];
                        waveInfo.HistoryLast = ( ushort )loopHistory.Last;
                        waveInfo.HistoryPenult = ( ushort )loopHistory.Penult;
                        waveBytes[index] = encodeInfo.Data;
                    }
                    else
                    {
                        mLogger.Information( $"Injecting raw file {file}" );
                        var fileBytes = new byte[fileStream.Length];
                        fileStream.Write( fileBytes, 0, fileBytes.Length );
                        waveBytes[index] = fileBytes;
                    }
                }
            }

            mLogger.Information( $"Building new AW" );
            var newAwStream = new MemoryStream();
            using ( var writer = new BinaryValueWriter( newAwStream, Amicitia.IO.Streams.StreamOwnership.Retain, Endianness.Big, Encoding.Default ) )
            {
                for ( int i = 0; i < waveGroup.FileWaveInfo.Length; i++ )
                {
                    writer.Align( 32 );
                    waveGroup.FileWaveInfo[i].WaveInfo.WaveStart = ( uint )writer.Position;
                    writer.WriteArray( waveBytes[i] );
                    waveGroup.FileWaveInfo[i].WaveInfo.WaveSize = ( uint )( writer.Position - waveGroup.FileWaveInfo[i].WaveInfo.WaveStart );
                    writer.Align( 32 );
                }
            }

            newAwStream.Position = 0;
            mNewAWStreams[waveGroup.ArchiveName] = newAwStream;
            return this;
        }

        private static byte[][] ReadWaveGroupRawWaves( Stream awStream, FileWaveGroup waveGroup )
        {
            var waveBytes = new byte[waveGroup.FileWaveInfo.Length][];
            using ( var reader = new BinaryValueReader( awStream, Amicitia.IO.Streams.StreamOwnership.Retain, Endianness.Big, Encoding.Default ) )
            {
                for ( int i = 0; i < waveGroup.FileWaveInfo.Length; i++ )
                {
                    reader.Seek( waveGroup.FileWaveInfo[i].WaveInfo.WaveStart, SeekOrigin.Begin );
                    waveBytes[i] = reader.ReadArray<byte>( ( int )( waveGroup.FileWaveInfo[i].WaveInfo.WaveSize ) );
                }
            }

            return waveBytes;
        }

        public BAAPatch Build()
        {
            mLogger.Information( "Patching BAA" );
            var newBAAStream = new MemoryStream();
            mBAAStream.Position = 0;
            mBAAStream.CopyTo( newBAAStream );
            newBAAStream.Position = 0;

            using (var writer = new BinaryValueWriter( newBAAStream, Amicitia.IO.Streams.StreamOwnership.Retain, Endianness.Big, Encoding.ASCII))
            {
                foreach ( var waveGroup in mWaveGroups )
                {
                    foreach ( var waveInfo in waveGroup.FileWaveInfo )
                    {
                        writer.Seek( waveInfo.BinarySourceInfo.StartOffset, SeekOrigin.Begin );
                        writer.Write( waveInfo.WaveInfo );
                    }
                }
            }

            newBAAStream.Position = 0;
            return new BAAPatch( newBAAStream, mNewAWStreams );
        }
    }
}
