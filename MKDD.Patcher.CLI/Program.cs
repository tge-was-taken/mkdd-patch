using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace MKDD.Patcher.CLI
{
    public class Program
    {
        const string CONFIG_PATH = "config.json";

        static void Main( string[] args )
        {
            var asmName = Assembly.GetExecutingAssembly().GetName();
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"{asmName.Name}.log")
                .CreateLogger();

            if ( !File.Exists( CONFIG_PATH ) )
            {
                logger.Error( $"{CONFIG_PATH} doesn't exist. Creating default configuration..." );
                var defaultConfig = new
                {
                    FilesDir = "files",
                    ModsDir = "mods",
                    OutDir = "mods/.out",
                    CacheDir = "mods/.cache",
                    ArcPackPath = string.Empty,
                    ArcExtractPath = string.Empty,
                };

                File.WriteAllText( CONFIG_PATH, JsonConvert.SerializeObject( defaultConfig, Formatting.Indented ) );
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath(CONFIG_PATH))
                .Build();


            //var baaPatchBuilder = new BAAPatchBuilder(logger, configuration);
            //baaPatchBuilder.SetBAAStream( new FileStream( @"D:\Games\GCWii\MKDD_modded\files\AudioRes\GCKart.baa", FileMode.Open, FileAccess.ReadWrite ) );
            //baaPatchBuilder.PatchAW( File.OpenRead( @"D:\Games\GCWii\MKDD_modded\files\AudioRes\Waves\Voice_0.aw" ), @"D:\Games\GCWii\MKDD_modded\files\AudioRes\_tmp\Voice_0" );
            //var patch = baaPatchBuilder.Build();

            //Directory.CreateDirectory( "out" );
            //using ( var fileStream = File.Create( "out/GCKart.baa" ) )
            //    patch.BAAStream.CopyTo( fileStream );


            //Directory.CreateDirectory( "out/Waves" );
            //foreach ( var item in patch.AWStreams )
            //{
            //    using ( var fileStream = File.Create( "out/Waves/" + item.Key ) )
            //        item.Value.CopyTo( fileStream );
            //}

            //return;

            logger.Information( $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE ({DateTime.Now.Year})\n" );

#if !DEBUG
            try
            {
#endif
                var patcher = new Patcher(logger, configuration);
                patcher.Patch();
#if !DEBUG
            }
            catch ( Exception e )
            {
                logger.Fatal( $"An unhandled exception occured: {e.Message}\nThe program will now exit." );
            }
#endif
        }
    }
}
