using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MKDD.Patcher.GUI
{
    static class Program
    {
        private const string CONFIG_PATH = "config.json";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var asmName = Assembly.GetExecutingAssembly().GetName();
            var logger = new LoggerConfiguration()
                .WriteTo.File($"{asmName.Name}.log")
                .CreateLogger();

            if (!File.Exists(CONFIG_PATH))
            {
                logger.Error($"{CONFIG_PATH} doesn't exist. Creating default configuration...");
                ConfigurationHelper.CreateDefaultConfig(CONFIG_PATH);
            }

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.GetFullPath(CONFIG_PATH))
                .Build();

            logger.Information($"{asmName.Name} {asmName.Version.Major}.{asmName.Version.Minor}.{asmName.Version.Revision} by TGE ({DateTime.Now.Year})\n");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(logger, configuration));
        }
    }
}
