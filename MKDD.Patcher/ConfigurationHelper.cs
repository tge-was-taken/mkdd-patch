using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MKDD.Patcher
{
    public class ConfigurationHelper
    {
        public static void CreateDefaultConfig( string configPath )
        {
            var defaultConfig = new
            {
                FilesDir = "path/to/mkdd/files/directory",
                ModsDir = "path/to/mods/directory",
                BinDir = "path/to/mods/directory/.bin",
                OutDir = "path/to/mkdd/files/directory",
                CacheDir = "path/to/mods/directory/.cache",
                ArcPackPath = "Tools/LunaboyRarcTools/ArcPack.exe",
                ArcExtractPath = "Tools/LunaboyRarcTools/ArcExtract.exe",
            };

            File.WriteAllText(configPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
        }
    }
}
