using System;
using Amicitia.IO.Binary;

namespace MKDD.Patcher
{
    public class FileWaveGroup : IBinarySerializableWithInfo
    {
        public BinarySourceInfo BinarySourceInfo { get; set; }
        public string ArchiveName { get; set; }
        public FileWaveInfo[] FileWaveInfo { get; set; }

        public void Read( BinaryObjectReader reader )
        {
            ArchiveName = reader.ReadString( StringBinaryFormat.FixedLength, 112 );
            var waveInfoCount = reader.ReadUInt32();
           
            FileWaveInfo = new FileWaveInfo[waveInfoCount];
            for ( int j = 0; j < waveInfoCount; j++ )
                FileWaveInfo[j] = reader.ReadObjectOffset<FileWaveInfo>();
        }

        public void Write( BinaryObjectWriter writer )
        {
            // Not implemented because the data is only ever used for patching
            throw new NotImplementedException();
        }
    }
}
