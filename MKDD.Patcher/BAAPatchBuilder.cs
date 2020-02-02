using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
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
            var waveBytes = new byte[waveGroup.WaveInfo.Length][];
            using (var reader = new BinaryIOStream(awStream, IOMode.Read, Endianness.Big, Encoding.Default, true))
            {
                for ( int i = 0; i < waveGroup.WaveInfo.Length; i++ )
                {
                    reader.Seek( waveGroup.WaveInfo[i].WaveStart, Origin.Begin );
                    waveBytes[i] = reader.ReadBytes( 0, ( int )( waveGroup.WaveInfo[i].WaveSize ) );
                }
            }

            var processes = new List<Process>();
            foreach ( var file in Directory.EnumerateFiles(replacementWavesDir, "*", SearchOption.TopDirectoryOnly) )
            {
                var indexValue = Regex.Match(Path.GetFileNameWithoutExtension(file), @"(0_)?(?<index>\d+)")
                    .Groups["index"].Value;
                var index = int.Parse(indexValue);      

                mLogger.Information( $"Mapped {file} to index {index}" );
                if ( Path.GetExtension(file).ToLower() == ".wav" )
                {
                    var rawFilePath = file + ".raw";
                    mLogger.Information( $"Encoding {file} to {rawFilePath}" );

                    var process = new Process();
                    process.StartInfo = new ProcessStartInfo( mConfiguration["MareepPath"], $@"-errand wave -input ""{file}"" -output ""{rawFilePath}"" ADPCM4" );
                    process.EnableRaisingEvents = true;
                    process.Exited += ( s, e ) =>
                    {
                        mLogger.Information( $"Injecting encoded file {rawFilePath}" );
                        waveBytes[index] = File.ReadAllBytes( rawFilePath );
                        mIO.DeleteFile( rawFilePath );
                    };
                    process.Start();
                    processes.Add( process );
                }
                else
                {
                    mLogger.Information( $"Injecting raw file {file}" );
                    waveBytes[index] = File.ReadAllBytes( file );
                }
            }

            // Wait for files to encode
            foreach ( var process in processes )
                process.WaitForExit();

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
