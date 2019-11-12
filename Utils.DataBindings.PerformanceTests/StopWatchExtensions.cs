using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

public static class StopWatchExtensions
{
    public static long ElapsedNanoSeconds(this Stopwatch watch)
    {
        return watch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
    }
    public static long ElapsedMicroSeconds(this Stopwatch watch)
    {
        return watch.ElapsedTicks * 1000000 / Stopwatch.Frequency;
    }
}
