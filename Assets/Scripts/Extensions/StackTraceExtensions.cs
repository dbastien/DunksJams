using System.Diagnostics;
using System.Linq;

public static class StackTraceExtensions
{
    public static string FullInfo(this StackTrace st, int skipFrames = 0)
    {
        var frames = st.GetFrames()?.Skip(skipFrames) ?? Enumerable.Empty<StackFrame>();
        return string.Join("\n", frames.Select(f => f.ShortInfo()));
    }
}