using System.Linq;
using MKDD.Patcher.IO;

namespace MKDD.Patcher
{
    public struct WaveInfo
    {
        public long FilePosition;
        public byte Field00;
        public byte Format;
        public byte RootKey;
        public byte Padding;
        public float SampleRate;
        public uint WaveStart;
        public uint WaveSize;
        public uint HasLoop;
        public uint LoopStart;
        public uint LoopEnd;
        public uint SampleCount;
        public ushort HistoryLast;
        public ushort HistoryPenult;
        public uint Field34;
        public uint Field38;

        public void Serialize( BinaryIOStream stream )
        {
            stream.Byte( ref Field00 );
            stream.Byte( ref Format );
            stream.Byte( ref RootKey );
            stream.Byte( ref Padding );
            stream.Single( ref SampleRate );
            stream.UInt32( ref WaveStart );
            stream.UInt32( ref WaveSize );
            stream.UInt32( ref HasLoop );
            stream.UInt32( ref LoopStart );
            stream.UInt32( ref LoopEnd );
            stream.UInt32( ref SampleCount );
            stream.UInt16( ref HistoryLast );
            stream.UInt16( ref HistoryPenult );
            stream.UInt32( ref Field34 );
            stream.UInt32( ref Field38 );
        }
    }
}
