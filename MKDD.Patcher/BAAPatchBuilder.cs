using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MKDD.Patcher.Audio;
using MKDD.Patcher.IO;
using Serilog;

namespace MKDD.Patcher
{
    public class BAAPatchBuilder
    {
        private ILogger mLogger;
        private IConfiguration mConfiguration;
        private LoggedIO mIO;
        private BAAParser mBAAParser;
        private Stream mBAAStream;
        private List<WaveGroup> mWaveGroups;
        private Dictionary<string, Stream> mNewAWStreams;

        public BAAPatchBuilder(ILogger logger, IConfiguration configuration)
        {
            mLogger = logger;
            mIO = new LoggedIO( mLogger );
            mWaveGroups = new List<WaveGroup>();
            mNewAWStreams = new Dictionary<string, Stream>();
            mConfiguration = configuration;
            mBAAParser = new BAAParser( mLogger );
        }

        public BAAPatchBuilder SetBAAStream(Stream baaStream)
        {
            mBAAStream = baaStream;
            mWaveGroups = mBAAParser.Parse( mBAAStream );
            return this;
        }

        public BAAPatchBuilder PatchAW(Stream awStream, string replacementWavesDir)
        {
            var waveGroupName = Path.GetFileNameWithoutExtension(replacementWavesDir);
            var waveGroup = mWaveGroups.Where( x => Path.GetFileNameWithoutExtension(x.ArchiveName).Equals(waveGroupName)).FirstOrDefault();

            mLogger.Information( $"Patching wave group {waveGroupName}" );
            var waveBytes = ReadWaveGroupRawWaves( awStream, waveGroup );

            foreach ( var file in Directory.EnumerateFiles( replacementWavesDir, "*.wav", SearchOption.TopDirectoryOnly ) )
            {
                var indexValue = Regex.Match(Path.GetFileNameWithoutExtension(file), @"(0_)?(?<index>\d+)")
                    .Groups["index"].Value;
                var index = int.Parse(indexValue);
                ref var waveInfo = ref waveGroup.WaveInfo[index];

                mLogger.Information( $"Mapped {file} to index {index}" );
                if ( PathHelper.HasExtension( file, ".wav" ) )
                {
                    mLogger.Information( $"Encoding {file} to ADPCM" );
                    var encodeInfo = AudioHelper.EncodeWavToAdpcm( file, AdpcmFormat.Adpcm4 );

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
                    waveBytes[index] = File.ReadAllBytes( file );
                }
            }

            mLogger.Information( $"Building new AW" );
            var newAwStream = new MemoryStream();
            using ( var writer = new BinaryIOStream( newAwStream, IOMode.Write, Endianness.Big, Encoding.Default, true ) )
            {
                for ( int i = 0; i < waveGroup.WaveInfo.Length; i++ )
                {
                    writer.Position = AlignmentHelper.Align( writer.Position, 32 );
                    waveGroup.WaveInfo[i].WaveStart = ( uint )writer.Position;
                    writer.WriteBytes( waveBytes[i], 0, waveBytes[i].Length );
                    waveGroup.WaveInfo[i].WaveSize = ( uint )( writer.Position - waveGroup.WaveInfo[i].WaveStart );
                    writer.Align( 32 );
                }
            }

            newAwStream.Position = 0;
            mNewAWStreams[waveGroup.ArchiveName] = newAwStream;
            return this;
        }

        private static byte[][] ReadWaveGroupRawWaves( Stream awStream, WaveGroup waveGroup )
        {
            var waveBytes = new byte[waveGroup.WaveInfo.Length][];
            using ( var reader = new BinaryIOStream( awStream, IOMode.Read, Endianness.Big, Encoding.Default, true ) )
            {
                for ( int i = 0; i < waveGroup.WaveInfo.Length; i++ )
                {
                    reader.Seek( waveGroup.WaveInfo[i].WaveStart, Origin.Begin );
                    waveBytes[i] = reader.ReadBytes( ( int )( waveGroup.WaveInfo[i].WaveSize ) );
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

            using (var writer = new BinaryIOStream( newBAAStream, IOMode.Write, Endianness.Big, Encoding.ASCII, true))
            {
                foreach ( var waveGroup in mWaveGroups )
                {
                    foreach ( var waveInfo in waveGroup.WaveInfo )
                    {
                        writer.Seek( waveInfo.FilePosition, Origin.Begin );
                        waveInfo.Serialize( writer );
                    }
                }
            }

            newBAAStream.Position = 0;
            return new BAAPatch( newBAAStream, mNewAWStreams );
        }
    }
}
