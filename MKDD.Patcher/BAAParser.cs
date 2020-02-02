using System.Collections.Generic;
using System.IO;
using System.Text;
using MKDD.Patcher.IO;
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

        public List<WaveGroup> Parse(Stream stream)
        {
            var waveGroups = new List<WaveGroup>();
            using ( var reader = new BinaryIOStream( stream, IOMode.Read, Endianness.Big, Encoding.ASCII, true ) )
            {
                while ( reader.Position + 4 < reader.BaseStream.Length )
                {
                    var value = reader.ReadUInt32();
                    if ( value == 0x57535953 )
                    {
                        reader.Seek( -4, Origin.Current );
                        reader.PushOffsetBase();
                        ReadWSYS( reader, waveGroups );
                        reader.PopOffsetBase();
                    }
                }

                //var startFourCC = reader.ReadUInt32();
                //var chunkFourCC = reader.ReadUInt32();
                //while ( chunkFourCC != 0x3E5F4141 )
                //{
                //    switch ( chunkFourCC )
                //    {
                //        case 0x62737420: reader.Skip( 8 ); break; // bst
                //        case 0x6273746E: reader.Skip( 8 ); break; // bstn
                //        case 0x626E6B20: reader.Skip( 8 ); break; // bnk
                //        case 0x62736674: reader.Skip( 4 ); break; // bstf
                //        case 0x62736320: reader.Skip( 8 ); break; // bsc
                //        case 0x626D7320: reader.Skip( 12 ); break; // bms
                //        case 0x62616163: reader.Skip( 8 ); break; // baac
                //        case 0x77732020: // ws
                //            waveGroups.Add( ReadWsChunk( reader ) );
                //            break;
                //        default:
                //            break;
                //    }

                //    chunkFourCC = reader.ReadUInt32();
                //}
            }

            return waveGroups;
        }

        private List<WaveGroup> ReadWsChunk( BinaryIOStream reader )
        {
            var waveGroups = new List<WaveGroup>();

            reader.Skip( 4 );
            var wsysOffset = reader.ReadUInt32();
            reader.Skip( 4 );

            using ( reader.At( wsysOffset, Origin.OffsetBase ) )
            {
                reader.PushOffsetBase();
                ReadWSYS( reader, waveGroups );
                reader.PopOffsetBase();
            }

            return waveGroups;
        }

        private void ReadWSYS( BinaryIOStream reader, List<WaveGroup> waveGroups )
        {
            var start = reader.Position;
            mLogger.Information( $"Reading WSYS at 0x{start:X8}" );

            reader.Skip( 16 );
            var winfOffset = reader.ReadUInt32();
            reader.Seek( winfOffset, Origin.OffsetBase );
            reader.Skip( 4 );
            var waveGroupCount = reader.ReadUInt32();

            for ( int i = 0; i < waveGroupCount; i++ )
            {
                var waveGroupOffset = reader.ReadUInt32();
                using ( reader.At( waveGroupOffset, Origin.OffsetBase ) )
                {
                    var grp = new WaveGroup();
                    grp.FilePosition = reader.Position;
                    grp.ArchiveName = reader.ReadString( Storage.ByValue, 112 );

                    var waveInfoCount = reader.ReadUInt32();
                    grp.WaveInfo = new WaveInfo[waveInfoCount];
                    for ( int j = 0; j < waveInfoCount; j++ )
                    {
                        var waveInfoOffset = reader.ReadUInt32();
                        using ( reader.At( waveInfoOffset, Origin.OffsetBase ) )
                        {
                            ref var info = ref grp.WaveInfo[j];
                            info.FilePosition = reader.Position;
                            info.Serialize( reader );
                        }
                    }

                    waveGroups.Add( grp );
                }
            }
        }
    }
}
