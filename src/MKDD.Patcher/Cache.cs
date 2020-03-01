using System.Collections.Generic;

namespace MKDD.Patcher
{
    public class Cache
    {
        public const int CURRENT_VERSION = 2;

        public int Version { get; set; }
        public Dictionary<string, ContainerType> ContainerDirs { get; set; }

        public Cache()
        {
            Version = CURRENT_VERSION;
            ContainerDirs = new Dictionary<string, ContainerType>();
        }
    }
}
