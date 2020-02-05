using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MKDD.Patcher.GUI
{
    public class GuiConfig
    {
        public const string FILE_PATH = "mkdd-patcher-gui.cfg.json";

        public PatcherConfig Patcher { get; set; }
        public List<GuiModConfig> Mods { get; set; }

        public GuiConfig()
        {
            Patcher = new PatcherConfig();
            Mods = new List<GuiModConfig>();
        }

        public static GuiConfig Load( string path )
        {
            return JsonConvert.DeserializeObject<GuiConfig>( File.ReadAllText( path ) );
        }

        public void Save( string path )
        {
            File.WriteAllText( path, JsonConvert.SerializeObject( this, Formatting.Indented ) );
        }
    }
}
