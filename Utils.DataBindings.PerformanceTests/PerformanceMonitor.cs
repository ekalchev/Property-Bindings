using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public sealed class PerformanceMonitor : IDisposable
{
    private bool enablePerformanceMonitor = true;
    private const string LogStartMessageTemplate = "Performance Log: {0} - Timing Started";
    private const string LogEndMessageTemplate = "Performance Log: {0} = {1}{2} {3}";
    private const string nsString = "ns";
    private const string microsecondsString = "µs";
    private const string milisecondsString = "ms";
    private const string timeWarningSuffix = "TIME WARNING !!!";
    private string description;
    private DateTime? startTime;
    private readonly long timeWarningInMiliseconds;
    private readonly bool logOnlyOnTimeWarning;
    private readonly Stopwatch stopwatch = new Stopwatch();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="description"></param>
    /// <param name="timeWarningInMiliseconds"></param>
    /// <param name="logOnlyOnTimeWarning">Write in logs only if the measured time is more than timeWarningInSeconds</param>
    public PerformanceMonitor(string description)
    {
        this.description = description;
        this.startTime = DateTime.Now;

        string message = string.Format(LogStartMessageTemplate, this.description);
        Debug.WriteLine(message);

        stopwatch.Start();

    }

    public void Dispose()
    {
        if (stopwatch.IsRunning == true)
        {
            stopwatch.Stop();

            string warning = String.Empty;

            string timeDimension;
            string time;

            if (stopwatch.ElapsedNanoSeconds() < 10000)
            {
                time = stopwatch.ElapsedNanoSeconds().ToString();
                timeDimension = nsString;

            }
            else if (stopwatch.ElapsedMicroSeconds() < 10000)
            {
                time = stopwatch.ElapsedMicroSeconds().ToString();
                timeDimension = microsecondsString;
            }
            else
            {
                time = stopwatch.ElapsedMilliseconds.ToString();
                timeDimension = milisecondsString;
            }

            string message = string.Format(LogEndMessageTemplate, description, time, timeDimension, string.Empty);
            Debug.WriteLine(message);
        }
    }
}
