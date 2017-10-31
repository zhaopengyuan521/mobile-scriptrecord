using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomLogger
{
    public class ReadParamAppender : log4net.Appender.AppenderSkeleton
    {
        public string File { get; set; }

        public int MaxSizeRollBackups { get; set; }

        public bool AppendToFile { get; set; }

        public string MaximumFileSize { get; set; }

        public string LayoutPattern { get; set; }

        public string DatePattern { get; set; }

        public string Level { get; set; }

        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {

        }
    }
}
