using System.Diagnostics;
using System.Linq;

public static class StackTraceExtensions
{
    public static string FullInfo(this StackTrace st, int skipFrames = 0)
    {
        var frames = st.GetFrames()?.Skip(skipFrames) ?? Enumerable.Empty<StackFrame>();
        return string.Join("\n", frames.Select(f => f.ShortInfo()));
    }

    public static string ShortInfo(this StackTrace st, int skipFrames = 0) => st.ShortInfo(skipFrames);

    public static string ToString(this StackTrace st, int skipFrames = 0) => st.FullInfo(skipFrames);
}