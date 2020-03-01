using System.Runtime.InteropServices;
using Amicitia.IO.Binary;

namespace MKDD.Patcher
{
    public class FileWaveInfo : IBinarySerializableWithInfo
    {
        public BinarySourceInfo BinarySourceInfo { get; set; }
        public WaveInfo WaveInfo;

        public void Read( BinaryObjectReader reader )
            => reader.Read( out WaveInfo );

        public void Write( BinaryObjectWriter writer )
            => writer.Write( ref WaveInfo );
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveInfo
    {
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
    }
}
