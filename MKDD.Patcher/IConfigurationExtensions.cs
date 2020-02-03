using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MKDD.Patcher
{
    public static class IConfigurationExtensions
    {
        public static string ToJson(this IConfiguration configuration)
        {
            var dict = new Dictionary<string, string>();
            foreach ( var item in configuration.AsEnumerable() )
                dict[item.Key] = item.Value;

            return JsonConvert.SerializeObject( dict, Formatting.Indented );
        }

        public static void ToJsonFile( this IConfiguration configuration, string path )
        {
            File.WriteAllText( path, configuration.ToJson() );
        }
    }
}
