using System.Collections.Generic;

namespace MKDD.Patcher
{
    public class Cache
    {
        public const int CURRENT_VERSION = 1;

        public int Version { get; set; }
        public HashSet<string> ArchiveDirs { get; set; }

        public Cache()
        {
            Version = CURRENT_VERSION;
            ArchiveDirs = new HashSet<string>();
        }
    }
}
