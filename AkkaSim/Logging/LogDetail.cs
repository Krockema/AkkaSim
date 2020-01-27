using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace AkkaSim.Logging
{
    public class LogDetail
    {
        public string TargetName { get; set; }
        public TargetTypes TargetTypes { get; set; }
        public LogLevel LogLevel { get; set; }
        public string FilterPattern { get; set; }
    }
}
