using System.Diagnostics;

public static class StackFrameExtensions
{
    public static string MethodName(this StackFrame sf) => sf.GetMethod()?.Name ?? "UnknownMethod";
    public static string ClassName(this StackFrame sf) => sf.GetMethod()?.DeclaringType?.Name ?? "UnknownClass";
    public static string FileName(this StackFrame sf) => sf.GetFileName()?.Replace("\\", "/") ?? "UnknownFile";
    
    public static int LineNumber(this StackFrame sf) => sf.GetFileLineNumber();
    public static int ColumnNumber(this StackFrame sf) => sf.GetFileColumnNumber();

    public static string ShortInfo(this StackFrame sf) => $"{sf.FileName()}:{sf.LineNumber()} ({sf.ClassName()}::{sf.MethodName()})";
    public static string FullInfo(this StackFrame sf) => $"{sf.FileName()}:{sf.LineNumber()}:{sf.ColumnNumber()} ({sf.ClassName()}::{sf.MethodName()})";

    public static string ToString(this StackFrame sf) => $"{sf.FileName()}:{sf.LineNumber()}:{sf.ColumnNumber()} ({sf.ClassName()}::{sf.MethodName()})";
}