using System;
using System.Collections.Generic;

public interface ICardGameIO
{
    void WriteLine(string message);
    void WriteLines(IEnumerable<string> lines);
    string ReadText(string prompt, string defaultValue = "");
    int ReadInt(string prompt, int min, int max, int defaultValue);
    int ReadChoice(string prompt, IReadOnlyList<string> options, int defaultIndex = 0);
}

public static class CardGameIO
{
    static ICardGameIO _default = new QueuedCardGameIO();
    public static ICardGameIO Default => _default;

    public static void SetDefault(ICardGameIO io)
    {
        if (io != null) _default = io;
    }
}

public sealed class QueuedCardGameIO : ICardGameIO
{
    readonly Queue<string> _inputs = new();

    public void EnqueueInput(string input)
    {
        if (!string.IsNullOrWhiteSpace(input)) _inputs.Enqueue(input);
    }

    public void EnqueueInputs(IEnumerable<string> inputs)
    {
        if (inputs == null) return;
        foreach (var input in inputs) EnqueueInput(input);
    }

    public void ClearInputs() => _inputs.Clear();

    public void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message)) DLog.Log(message);
    }

    public void WriteLines(IEnumerable<string> lines)
    {
        if (lines == null) return;
        foreach (var line in lines) WriteLine(line);
    }

    public string ReadText(string prompt, string defaultValue = "")
    {
        if (!string.IsNullOrWhiteSpace(prompt)) WriteLine(prompt);
        if (_inputs.Count == 0) return defaultValue ?? string.Empty;
        var input = _inputs.Dequeue();
        return string.IsNullOrWhiteSpace(input) ? defaultValue ?? string.Empty : input;
    }

    public int ReadInt(string prompt, int min, int max, int defaultValue)
    {
        string input = ReadText(prompt, defaultValue.ToString());
        return TryParseInt(input, min, max, defaultValue);
    }

    public int ReadChoice(string prompt, IReadOnlyList<string> options, int defaultIndex = 0)
    {
        if (options == null || options.Count == 0) return -1;
        defaultIndex = Clamp(defaultIndex, 0, options.Count - 1);

        var lines = new List<string>(options.Count + 1);
        if (!string.IsNullOrWhiteSpace(prompt)) lines.Add(prompt);
        for (int i = 0; i < options.Count; ++i) lines.Add($"{i + 1}. {options[i]}");
        WriteLines(lines);

        int choice = ReadInt("Choice", 1, options.Count, defaultIndex + 1);
        return choice - 1;
    }

    static int TryParseInt(string input, int min, int max, int defaultValue)
    {
        if (int.TryParse(input, out int value) && value >= min && value <= max) return value;
        return Clamp(defaultValue, min, max);
    }

    static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
}

public sealed class ConsoleCardGameIO : ICardGameIO
{
    public void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);
    }

    public void WriteLines(IEnumerable<string> lines)
    {
        if (lines == null) return;
        foreach (var line in lines) WriteLine(line);
    }

    public string ReadText(string prompt, string defaultValue = "")
    {
        if (!string.IsNullOrWhiteSpace(prompt)) Console.WriteLine(prompt);
        string input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? defaultValue ?? string.Empty : input;
    }

    public int ReadInt(string prompt, int min, int max, int defaultValue)
    {
        string input = ReadText(prompt, defaultValue.ToString());
        if (int.TryParse(input, out int value) && value >= min && value <= max) return value;
        return Clamp(defaultValue, min, max);
    }

    public int ReadChoice(string prompt, IReadOnlyList<string> options, int defaultIndex = 0)
    {
        if (options == null || options.Count == 0) return -1;
        defaultIndex = Clamp(defaultIndex, 0, options.Count - 1);

        var lines = new List<string>(options.Count + 1);
        if (!string.IsNullOrWhiteSpace(prompt)) lines.Add(prompt);
        for (int i = 0; i < options.Count; ++i) lines.Add($"{i + 1}. {options[i]}");
        WriteLines(lines);

        int choice = ReadInt("Choice", 1, options.Count, defaultIndex + 1);
        return choice - 1;
    }

    static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
}
