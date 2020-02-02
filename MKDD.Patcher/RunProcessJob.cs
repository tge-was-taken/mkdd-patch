using System.Collections.Generic;
using System.Diagnostics;

namespace MKDD.Patcher
{
    public struct RunProcessJob
    {
        public Process Process { get; }
        public List<string> TemporaryFiles { get; }
        public RunProcessJob( Process process, params string[] files )
        {
            Process = process;
            TemporaryFiles = new List<string>();
            foreach ( var file in files )
                TemporaryFiles.Add( file );
        }
    }
}
