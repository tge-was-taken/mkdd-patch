using System.IO;
using Newtonsoft.Json;

namespace MKDD.Patcher
{
    public class PatcherConfig
    {
        public const string FILE_PATH = "mkdd-patcher.cfg.json";

        public string OutDir { get; set; }
        public string ModsDir { get; set; }
        public string FilesDir { get; set; }
        public string CacheDir { get; set; }
        public string BinDir { get; set; }
        public string ArcPackPath { get; set; }
        public string ArcExtractPath { get; set; }

        public PatcherConfig()
        {
            ArcPackPath = "Tools/LunaboyRarcTools/ArcPack.exe";
            ArcExtractPath = "Tools/LunaboyRarcTools/ArcExtract.exe";
        }

        public static PatcherConfig Load( string path )
        {
            return JsonConvert.DeserializeObject<PatcherConfig>( File.ReadAllText( path ) );
        }

        public void Save( string path )
        {
            File.WriteAllText( path, JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }
}
