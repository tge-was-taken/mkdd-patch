using System;
using System.Linq;
using arookas;
using MKDD.Patcher.IO;
using NAudio.Wave;

namespace MKDD.Patcher.Audio
{
    public struct EncodedWavInfo
    {
        public AdpcmFormat Format;
        public int SampleRate;
        public int SampleCount;
        public bool HasLoop;
        public int LoopStart;
        public int LoopEnd;
        public byte[] Data;
        public AdpcmHistory[] History;
    }

    public enum AdpcmFormat
    {
        Adpcm2,
        Adpcm4,
    }

    public static class AudioHelper
    {
        public static EncodedWavInfo EncodeWavToAdpcm( string file, AdpcmFormat format )
        {
            var result = new EncodedWavInfo();
            result.Format = format;

            using ( var waveReader = new WaveFileReader( file ) )
            {
                result.SampleRate = waveReader.WaveFormat.SampleRate;
                result.SampleCount = ( int )waveReader.SampleCount;

                // Try to find the smpl chunk containing loop info
                var smplChunk = waveReader.ExtraChunks.FirstOrDefault(x => x.IdentifierAsString == "smpl");
                if ( smplChunk != null )
                {
                    using ( var reader = new BinaryIOStream( waveReader, IOMode.Read, Endianness.Big ) )
                    {
                        reader.Seek( smplChunk.StreamPosition + 0x24, Origin.Begin );
                        var sampleLoopCount = reader.ReadInt32();
                        if ( sampleLoopCount > 0 )
                        {
                            reader.Seek( 0x04 + 0x08, Origin.Current );
                            result.HasLoop = true;
                            result.LoopStart = reader.ReadInt32();
                            result.LoopEnd = reader.ReadInt32();
                        }
                    }
                }

                // Get samples
                var sampleProvider = waveReader.ToSampleProvider();
                sampleProvider = sampleProvider.ToMono();

                var samples = new float[result.SampleCount];
                sampleProvider.Read( samples, 0, samples.Length );

                // Encode
                var pcm16 = Waveform.Pcm32ToPcm16( samples );
                result.History = new AdpcmHistory[Waveform.GetAdpcmFrameCount( pcm16.Length )];

                if ( format == AdpcmFormat.Adpcm4 )
                    result.Data = Waveform.Pcm16ToAdpcm4( pcm16, result.History );
                else
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
