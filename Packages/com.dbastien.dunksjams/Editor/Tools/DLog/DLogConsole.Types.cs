// Assets/Editor/DLog/DLogConsole.Types.cs
using System;
using UnityEngine;

public sealed partial class DLogConsole
{
    [Serializable]
    private sealed class LogEntry
    {
        public string message;
        public string stackTrace;
        public LogType type;

        public long timeMsUtc;   // survives serialization (no double)
        public int count = 1;    // collapsed repeat count (consecutive)

        public bool expanded;

        // Pre-formatted (rich text) to avoid regex each frame
        public string richMessage;

        // Optional "best guess" source location for fast double-click open
        public string file;
        public int line;

        [NonSerialized] public string plainMessage;
        [NonSerialized] public bool messageLinkParsed;
        [NonSerialized] public LinkSpan messageLink;

        [NonSerialized] public StackLineInfo[] stackLineInfos;
        [NonSerialized] public bool stackCacheValid;
        [NonSerialized] public bool stackCacheProSkin;
        [NonSerialized] public float stackScrollHeight;

        [NonSerialized] public Vector2 stackScroll;
    }

    private struct LinkSpan
    {
        public bool hasLink;
        public string file;
        public int lineNumber;
        public int start;
        public int length;
    }

    private struct StackLineInfo
    {
        public string line;
        public string displayLine;
        public LinkSpan link;
    }
}