using System.Collections.Generic;
using System.IO;
using System.Text;
using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using Serilog;

namespace MKDD.Patcher
{
    public class BAAParser
    {
        private ILogger mLogger;

        public BAAParser(ILogger logger)
        {
            mLogger = logger;
        }

        public List<FileWaveGroup> Parse(Stream stream)
        {
            var waveGroups = new List<FileWaveGroup>();
            using ( var reader = new BinaryObjectReader( stream, StreamOwnership.Retain, Endianness.Big, Encoding.ASCII ) )
            {
                while ( reader.Position + 4 < reader.Length )
                {
                    var value = reader.ReadUInt32();
                    if ( value == 0x57535953 )
                    {
                        reader.Seek( -4, SeekOrigin.Current );
                        using ( reader.WithOffsetOrigin() )
                            ReadWSYS( reader, waveGroups );
                    }
                }
            }

            return waveGroups;
        }

        private List<FileWaveGroup> ReadWsChunk( BinaryObjectReader reader )
        {
            var waveGroups = new List<FileWaveGroup>();

            reader.Skip( 4 );
            var wsysOffset = reader.ReadUInt32();
            reader.Skip( 4 );

            using ( reader.AtOffset( wsysOffset ) )
            {
                using ( reader.WithOffsetOrigin() )
                    ReadWSYS( reader, waveGroups );
            };

            return waveGroups;
        }

        private void ReadWSYS( BinaryObjectReader reader, List<FileWaveGroup> waveGroups )
        {
            var start = reader.Position;
            mLogger.Information( $"Reading WSYS at 0x{start:X8}" );

            reader.Skip( 16 );
            reader.ReadOffset(() =>
            {
                reader.Skip( 4 );
                var waveGroupCount = reader.ReadUInt32();

                for ( int i = 0; i < waveGroupCount; i++ )
                {
                    using ( reader.ReadOffset() )
                    {
                        var grp = reader.ReadObject<FileWaveGroup>();
                        waveGroups.Add( grp );
                    }
                }
            });
        }
    }
}
