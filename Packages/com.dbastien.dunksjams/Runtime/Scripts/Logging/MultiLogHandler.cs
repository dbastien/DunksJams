using System;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class MultiLogHandler : ILogHandler
{
    private readonly ILogHandler[] _logHandlers;

    public MultiLogHandler(params ILogHandler[] logHandlers) =>
        _logHandlers = logHandlers ?? Array.Empty<ILogHandler>();

    public void LogFormat(LogType logType, Object context, string format, params object[] args)
    {
        for (var i = 0; i < _logHandlers.Length; ++i)
            _logHandlers[i]?.LogFormat(logType, context, format, args);
    }

    public void LogException(Exception exception, Object context)
    {
        for (var i = 0; i < _logHandlers.Length; ++i)
            _logHandlers[i]?.LogException(exception, context);
    }
}