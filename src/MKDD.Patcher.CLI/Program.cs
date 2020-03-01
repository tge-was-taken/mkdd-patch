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
        static void Main( string[] args )
        {
            var keepRunning = args.Length > 0 && args[0] == "--background";
            var asmName = Assembly.GetExecutingAssembly().GetName();
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"{asmName.Name}.log")
                .CreateLogger();

            PatcherConfig configuration;
            if ( !File.Exists( PatcherConfig.FILE_PATH ) )
            {
                logger.Error( $"{PatcherConfig.FILE_PATH} doesn't exist. Creating default configuration..." );
                configuration = new PatcherConfig()
                {
                    FilesDir = "path/to/mkdd/files/directory",
                    ModsDir = "path/to/mods/directory",
                    BinDir = "path/to/mods/directory/.bin",
                    OutDir = "path/to/mkdd/files/directory",
                    CacheDir = "path/to/mods/directory/.cache",
                    ArcPackPath = "Tools/LunaboyRarcTools/ArcPack.exe",
                    ArcExtractPath = "Tools/LunaboyRarcTools/ArcExtract.exe",
                };
            }
            else
            {
                configuration = PatcherConfig.Load( PatcherConfig.FILE_PATH );
            }

            logger.Information( $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE ({DateTime.Now.Year})\n" );

#if !DEBUG
            try
            {
#endif
                var modDb = new ModDb(logger, configuration, configuration.ModsDir);
                var patcher = new Patcher(logger, configuration, modDb);
                patcher.Patch( MergeOrder.TopToBottom );

                if ( keepRunning )
                {
                    while ( true )
                    {
                        patcher.Patch( MergeOrder.TopToBottom );
                        Console.WriteLine( "Press any key to patch" );
                        Console.ReadKey();
                    }
                }
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
