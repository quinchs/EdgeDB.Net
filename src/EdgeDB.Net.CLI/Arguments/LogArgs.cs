using CommandLine;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.CLI.Arguments
{
    public class LogArgs
    {
        [Option("loglevel")]
        public LogEventLevel LogLevel { get; set; } = LogEventLevel.Information;
    }
}
