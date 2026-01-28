using System;
using System.Diagnostics;

public class PrecisionStopwatch : IDisposable
{
    public enum TimeUnit { Seconds, Milliseconds, Microseconds, Nanoseconds }

    readonly Stopwatch _sw;
    readonly Action<string> _onDisposeAction;
    readonly TimeUnit _timeUnit;
    bool _disposed;

    public static bool IsHighResolution => Stopwatch.IsHighResolution;
    public static long Freq => Stopwatch.Frequency;

    public PrecisionStopwatch() : this(TimeUnit.Milliseconds) { }
    public PrecisionStopwatch(TimeUnit timeUnit, bool startImmediately) : this(timeUnit, null, startImmediately) { }
    public PrecisionStopwatch(Action<string> onDisposeAction) : this(TimeUnit.Milliseconds, onDisposeAction, startImmediately: true) { }
    public PrecisionStopwatch(TimeUnit timeUnit, Action<string> onDisposeAction = null, bool startImmediately = false)
    {
        _sw = new Stopwatch();
        _timeUnit = timeUnit;
        _onDisposeAction = onDisposeAction ?? (_ => { });
        if (!Stopwatch.IsHighResolution) DLog.LogW("High precision timing is not supported on this platform.");
        if (startImmediately) Start();
    }

    public static PrecisionStopwatch StartNew(TimeUnit timeUnit = TimeUnit.Milliseconds, Action<string> onDisposeAction = null)
        => new(timeUnit, onDisposeAction, startImmediately: true);

    public void Start() => _sw.Start();
    public void Stop() => _sw.Stop();
    public void Reset() => _sw.Reset();
    public void Restart() => _sw.Restart();
    public bool IsRunning => _sw.IsRunning;

    public long ElapsedTicks => _sw.ElapsedTicks;
    public TimeSpan Elapsed => _sw.Elapsed;

    public double ElapsedSeconds => ElapsedTicks / (double)Freq;
    public double ElapsedMilliseconds => ElapsedTicks * (1000.0 / Freq);
    public double ElapsedMicroseconds => ElapsedTicks * (1_000_000.0 / Freq);
    public double ElapsedNanoseconds => ElapsedTicks * (1_000_000_000.0 / Freq);

    public double GetElapsedTime(TimeUnit unit) => unit switch
    {
        TimeUnit.Seconds => ElapsedSeconds,
        TimeUnit.Milliseconds => ElapsedMilliseconds,
        TimeUnit.Microseconds => ElapsedMicroseconds,
        TimeUnit.Nanoseconds => ElapsedNanoseconds,
        _ => ElapsedMilliseconds
    };

    public string GetFormattedElapsed()
    {
        if (ElapsedMilliseconds >= 1000) return $"{ElapsedMilliseconds / 1000:F2} s";
        if (ElapsedMilliseconds >= 1) return $"{ElapsedMilliseconds:F2} ms";
        if (ElapsedNanoseconds >= 1000) return $"{ElapsedNanoseconds / 1000:F2} µs";
        return $"{ElapsedNanoseconds:F0} ns";
    }

    public override string ToString() => GetFormattedElapsed();

    public static double Measure(Action action, TimeUnit unit = TimeUnit.Milliseconds)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        using var stopwatch = StartNew(unit);
        action();
        return stopwatch.GetElapsedTime(unit);
    }

    public static T Measure<T>(Func<T> func, out double elapsed, TimeUnit unit = TimeUnit.Milliseconds)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        using var stopwatch = StartNew(unit);
        var result = func();
        elapsed = stopwatch.GetElapsedTime(unit);
        return result;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_sw.IsRunning) Stop();
        _onDisposeAction(GetFormattedElapsed());
    }
}