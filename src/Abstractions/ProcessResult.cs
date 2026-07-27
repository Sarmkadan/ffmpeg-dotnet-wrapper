using System;

namespace FFmpegDotnetWrapper.Abstraction
{
    public class ProcessResult
    {
        public int ExitCode { get; set; }
        public string StdErrTail { get; set; } = string.Empty;
        public bool TimedOut { get; set; }
        public bool WasCancelled { get; set; }
    }
}