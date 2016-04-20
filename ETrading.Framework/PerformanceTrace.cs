using System;
using System.Diagnostics;

namespace ETrading.Framework
{

    public static class PerformanceTrace
    {
        private static readonly PerformanceTraceInternal _trace;

#if PERFTRACE
        public static bool IsTraceEnabled = true;
#else
        public static bool IsTraceEnabled = false;
#endif
        static PerformanceTrace()
        {
            _trace = new PerformanceTraceInternal();
        }

        [Conditional("PERFTRACE")]
        public static void WriteLine(string value, params object[] args)
        {
            _trace.Write(string.Format(value, args));
        }
    }

    internal class PerformanceTraceInternal
    {
#if PERFTRACE
        private string _file;
        private ActionQueueThread _queue;
#endif

        public PerformanceTraceInternal()
        {
#if PERFTRACE
            _queue = new ActionQueueThread("PeformanceLog");

            _file = ConfigManager.Instance.Get("PeformanceLogFile", string.Empty);
            if (!string.IsNullOrEmpty(_file))
            {
                if (File.Exists(_file))
                {
                    File.Delete(_file);
                }
            }
#endif
        }

        public void Write(string value)
        {
#if PERFTRACE
            var msg = value;
            _queue.Add(() => WriteImpl(msg));
#endif
        }

        public void WriteImpl(string value)
        {
#if PERFTRACE
            if (!string.IsNullOrEmpty(_file))
            {
                var output = string.Concat(value, Environment.NewLine);
                File.AppendAllText(_file, output);
            }
            this.LogDebug(value);  
#endif
        }
    }

    public class PerformanceCheck : IDisposable
    {
#if PERFTRACE
        private Func<string> _logMessage;
        private readonly Stopwatch _stopwatch;
        
        public PerformanceCheck(Func<string> logMessage)
        {

            _logMessage = logMessage;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }
#else
        // ReSharper disable once UnusedParameter.Local
        public PerformanceCheck(Func<string> logMessage)
        {

        }
#endif

        public void Dispose()
        {
#if PERFTRACE
            _stopwatch.Stop();
            try
            {
                var message = _logMessage;
                _logMessage = null;
                if (message != null)
                {
                    var msg = string.Format("Executed in: {0}: {1}", _stopwatch.Elapsed, message());
                    PerformanceTrace.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                this.LogDebug(ex, "Failed to log performance message");
            }
#endif
        }
    }
}
