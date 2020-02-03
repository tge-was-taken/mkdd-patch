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
                ConfigurationHelper.CreateDefaultConfig(CONFIG_PATH);
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath(CONFIG_PATH))
                .Build();

            logger.Information( $"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE ({DateTime.Now.Year})\n" );

#if !DEBUG
            try
            {
#endif
                var patcher = new Patcher(logger, configuration);
                patcher.Patch(MergeOrder.FirstToLast);
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
