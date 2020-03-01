using System.Collections.Generic;
using System.IO;

namespace MKDD.Patcher
{
    public class BAAPatch
    {
        public Stream BAAStream { get; }
        public Dictionary<string, Stream> AWStreams { get; }

        public BAAPatch( Stream baaStream, Dictionary<string, Stream> awStreams)
        {
            BAAStream = baaStream;
            AWStreams = awStreams;
        }
    }
}
