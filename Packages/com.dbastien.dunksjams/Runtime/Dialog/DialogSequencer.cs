using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DialogSequencer : MonoBehaviour
{
    private int _activeCommandCount;
    public bool IsPlaying => _activeCommandCount > 0;

    private Dictionary<string, Type> _commandTypeCache;

    private void Awake()
    {
        _commandTypeCache = DialogUtility.DiscoverTypesWithPrefix("SequencerCommand");
    }

    public void PlaySequence(string sequence)
    {
        if (string.IsNullOrEmpty(sequence)) return;

        string[] commands = sequence.Split(';');
        foreach (var cmd in commands) ParseAndExecute(cmd.Trim());
    }

    private void ParseAndExecute(string cmdString)
    {
        if (string.IsNullOrEmpty(cmdString)) return;

        // Simple parser for Command(arg1, arg2)
        int openParen = cmdString.IndexOf('(');
        int closeParen = cmdString.LastIndexOf(')');

        string commandName;
        string[] args = null;

        if (openParen > 0 && closeParen > openParen)
        {
            commandName = cmdString.Substring(0, openParen);
            string argsString = cmdString.Substring(openParen + 1, closeParen - openParen - 1);
            args = DialogUtility.ParseCommandArguments(argsString);
        }
        else
        {
            commandName = cmdString;
        }

        ExecuteCommand(commandName, args);
    }

    private void ExecuteCommand(string name, string[] args)
    {
        // 1. Try to find a MonoBehaviour command class
        if (_commandTypeCache != null && _commandTypeCache.TryGetValue(name, out Type cmdType))
        {
            var cmdObj = gameObject.AddComponent(cmdType) as SequencerCommand;
            if (cmdObj != null)
            {
                cmdObj.Initialize(args);
                StartCoroutine(TrackMonoCommand(cmdObj));
                return;
            }
        }

        // 2. Fallback to method-based commands (legacy/internal)
        MethodInfo mi = GetType().GetMethod("OnSeq_" + name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (mi != null)
            mi.Invoke(this, new object[] { args });
        else
            DLog.LogW($"Sequencer command '{name}' not found.");
    }

    private IEnumerator TrackMonoCommand(SequencerCommand cmd)
    {
        _activeCommandCount++;
        while (cmd != null && cmd.IsRunning()) yield return null;
        _activeCommandCount--;
    }

    // --- Built-in Commands ---

    private void OnSeq_Log(string[] args)
    {
        if (args != null && args.Length > 0)
            DLog.Log($"[Sequencer] {args[0]}");
    }

    private void OnSeq_Wait(string[] args)
    {
        if (args != null && args.Length > 0 && float.TryParse(args[0], out var duration)) StartCoroutine(TrackCommand(WaitCoroutine(duration)));
    }

    private IEnumerator TrackCommand(IEnumerator coroutine)
    {
        _activeCommandCount++;
        yield return StartCoroutine(coroutine);
        _activeCommandCount--;
    }

    private void OnSeq_Audio(string[] args)
    {
        if (args != null && args.Length > 0)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + args[0]);
            if (clip != null && AudioSystem.Instance != null)
                AudioSystem.Instance.PlayOneShot(clip);
            // If we wanted to wait for audio, we'd need its duration
            // StartCoroutine(TrackCommand(WaitCoroutine(clip.length)));
            else if (clip == null) DLog.LogW($"[Sequencer] Audio clip 'Audio/{args[0]}' not found in Resources.");
        }
    }

    private void OnSeq_Music(string[] args)
    {
        if (args != null && args.Length > 0)
        {
            var clip = Resources.Load<AudioClip>("Music/" + args[0]);
            if (clip != null && AudioSystem.Instance != null) AudioSystem.Instance.PlayMusic(clip);
        }
    }

    private IEnumerator WaitCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
    }

    // Add more here or use a dedicated command discovery system
}