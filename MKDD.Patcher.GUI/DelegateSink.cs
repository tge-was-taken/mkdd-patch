using Serilog.Core;
using Serilog.Events;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MKDD.Patcher.GUI
{
    public class DelegateSink : ILogEventSink
    {
        private readonly Action<LogEvent> mEmit;

        public DelegateSink(Action<LogEvent> emit)
        {
            mEmit = emit;
        }

        public void Emit( LogEvent logEvent )
        {
            mEmit( logEvent );
        }
    }
}
