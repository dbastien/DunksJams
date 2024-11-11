using System.Diagnostics;
using System.Linq;

public static class StackFrameExtensions
{
    public static string MethodName(this StackFrame sf) =>
        sf.GetMethod()?.Name ?? "UnknownMethod";

    public static string ClassName(this StackFrame sf) =>
        sf.GetMethod()?.DeclaringType?.Name ?? "UnknownClass";

    public static string FileName(this StackFrame sf) =>
        sf.GetFileName()?.Replace("\\", "/") ?? "UnknownFile";
    
    public static int LineNumber(this StackFrame sf) =>
        sf.GetFileLineNumber();

    public static string ShortInfo(this StackFrame sf) =>
        $"{sf.FileName()}:{sf.LineNumber()} ({sf.ClassName()}::{sf.MethodName()})";
}