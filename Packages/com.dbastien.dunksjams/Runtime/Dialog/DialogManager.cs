using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public DialogEmphasisSettings emphasisSettings;
    public DialogDatabase masterDatabase;
    public string currentLanguage = ""; // Empty means default (English)

    private static bool _isQuitting;

    [Header("Runtime State")]
    public DialogConversation currentConversation;
    public DialogEntry currentEntry;
    public int currentLineIndex;

    public event Action<DialogEntry, DialogLine> OnEntryStarted;
    public event Action OnConversationEnded;

    private DialogSequencer _sequencer;
    private Dictionary<string, string> _variables = new();
    private HashSet<string> _shownNodes = new();

    public bool IsSequencerPlaying => _sequencer != null && _sequencer.IsPlaying;
    public bool HasBeenShown(string nodeGUID) => _shownNodes.Contains(nodeGUID);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _sequencer = GetComponent<DialogSequencer>();
            if (_sequencer == null) _sequencer = gameObject.AddComponent<DialogSequencer>();
            LoadState();
            DontDestroyOnLoad(gameObject);

            // Initialize language from LocalizationManager if available
            UpdateLanguage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => LocalizationManager.OnLangChanged += OnLangChanged;

    private void OnDisable()
    {
        if (!_isQuitting)
            LocalizationManager.OnLangChanged -= OnLangChanged;
    }

    private void OnApplicationQuit() => _isQuitting = true;

    private void OnLangChanged() => UpdateLanguage();

    private void UpdateLanguage()
    {
        if (LocalizationManager.Instance != null) currentLanguage = LocalizationManager.Instance.CurrentLanguage;
    }

    public void StartConversation(string conversationName)
    {
        if (masterDatabase == null)
        {
            DLog.LogW("No Master Database assigned to DialogManager.");
            return;
        }

        var conv = masterDatabase.GetConversation(conversationName);
        if (conv != null)
            StartConversation(conv);
        else
            DLog.LogW($"Conversation '{conversationName}' not found in Master Database.");
    }

    public void StartConversation(DialogConversation conversation)
    {
        currentConversation = conversation;
        if (currentConversation == null) return;

        currentEntry = currentConversation.GetStartEntry();
        currentLineIndex = 0;
        ProcessCurrentLine();
    }

    public void ProcessCurrentLine()
    {
        if (currentEntry == null)
        {
            EndConversation();
            return;
        }

        // Mark as shown (SimStatus)
        if (currentLineIndex == 0)
        {
            _shownNodes.Add(currentEntry.nodeID);
            SetVariable($"SimStatus.{currentEntry.nodeID}", "WasDisplayed");
        }

        if (currentLineIndex < 0 || currentLineIndex >= currentEntry.lines.Count)
        {
            // No lines in this node? Move to links or end
            var links = GetValidLinks();
            if (links.Count == 1 && string.IsNullOrEmpty(links[0].text))
            {
                // Auto-follow single empty link
                currentEntry = links[0].destination;
                currentLineIndex = 0;
                ProcessCurrentLine();
            }
            else if (links.Count == 0)
            {
                EndConversation();
            }
            // If links.Count > 1 or has text, we wait for UI to show choices
            return;
        }

        var line = currentEntry.lines[currentLineIndex];

        // Execute Sequence
        if (!string.IsNullOrEmpty(line.sequence)) _sequencer.PlaySequence(line.sequence);

        // Node scripts usually run once per node?
        // Or per line? Pixel Crushers has them per entry.
        // Let's run node script on first line.
        if (currentLineIndex == 0 && !string.IsNullOrEmpty(currentEntry.onExecuteScript)) ExecuteSimpleScript(currentEntry.onExecuteScript);

        OnEntryStarted?.Invoke(currentEntry, line);
    }

    public void Next(int linkIndex = -1)
    {
        if (currentEntry == null) return;

        // 1. If we have more lines in the stack, go to next line
        if (currentLineIndex < currentEntry.lines.Count - 1)
        {
            currentLineIndex++;
            ProcessCurrentLine();
            return;
        }

        // 2. Otherwise, follow a link
        var validLinks = GetValidLinks();

        // Handle Random node type: pick a random output
        if (currentEntry.nodeType == DialogNodeType.Random && validLinks.Count > 0)
        {
            var randomLink = validLinks[UnityEngine.Random.Range(0, validLinks.Count)];
            currentEntry = randomLink.destination;
            currentLineIndex = 0;
            ProcessCurrentLine();
            return;
        }

        // Handle Jump node type: jump to target
        if (currentEntry.nodeType == DialogNodeType.Jump && !string.IsNullOrEmpty(currentEntry.condition))
        {
            var jumpTarget = currentEntry.condition.Trim();

            // Try to find node by ID in current conversation
            var targetEntry = currentConversation?.entries.Find(e => e.nodeID == jumpTarget);

            // If not found, try conversation name
            if (targetEntry == null && masterDatabase != null)
            {
                var targetConv = masterDatabase.GetConversation(jumpTarget);
                if (targetConv != null)
                {
                    StartConversation(targetConv);
                    return;
                }
            }

            if (targetEntry != null)
            {
                currentEntry = targetEntry;
                currentLineIndex = 0;
                ProcessCurrentLine();
                return;
            }

            DLog.LogW($"Jump target '{jumpTarget}' not found.");
            EndConversation();
            return;
        }

        // linkIndex -1 means we just clicked "Next" without picking a specific choice
        // If there's only one valid link, we take it.
        if (linkIndex == -1)
        {
            if (validLinks.Count == 1)
            {
                currentEntry = validLinks[0].destination;
                currentLineIndex = 0;
                ProcessCurrentLine();
            }
            else if (validLinks.Count == 0)
            {
                EndConversation();
            }
            else
            {
                // Multiple choices, must pick one (UI should have handled this)
                DLog.LogW("Multiple choices available, but Next() was called without index.");
            }
        }
        else if (linkIndex >= 0 && linkIndex < validLinks.Count)
        {
            currentEntry = validLinks[linkIndex].destination;
            currentLineIndex = 0;
            ProcessCurrentLine();
        }
    }

    public List<DialogLink> GetValidLinks()
    {
        if (currentEntry == null) return new List<DialogLink>();

        List<DialogLink> validLinks = new List<DialogLink>();
        foreach (var link in currentEntry.outgoingLinks)
            if (CheckCondition(link.condition))
                validLinks.Add(link);

        return validLinks;
    }

    public void ManualShowLine(DialogEntry entry, DialogLine line)
    {
        currentEntry = entry;
        currentLineIndex = 0;
        OnEntryStarted?.Invoke(entry, line);
    }

    public void EndConversation()
    {
        currentConversation = null;
        currentEntry = null;
        OnConversationEnded?.Invoke();
    }

    // --- Variable System ---

    public void SetVariable(string name, string value)
    {
        _variables[name] = value;
        PlayerPrefs.SetString("DunksDialog.Var." + name, value);
    }

    public string GetVariable(string name, string fallback = "")
    {
        if (_variables.TryGetValue(name, out var v)) return v;
        return PlayerPrefs.GetString("DunksDialog.Var." + name, fallback);
    }

    public void LoadState()
    {
        // In a real system, we'd want to loop through all DunksDialog.Var.* keys
        // but PlayerPrefs doesn't support enumeration easily.
        // For now, we rely on GetVariable checking PlayerPrefs if not in dictionary.
    }

    public void ResetState()
    {
        _variables.Clear();
        _shownNodes.Clear();
        // This would need a way to clear PlayerPrefs with specific prefix
    }

    public string ProcessText(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        string result = DialogUtility.ProcessText(input, name => GetVariable(name));
        if (emphasisSettings != null) result = emphasisSettings.ProcessEmphasis(result);
        return result;
    }

    public string GetLocalizedText(DialogLine line)
    {
        if (line == null) return string.Empty;
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            string localized = line.GetFieldValue($"Text_{currentLanguage}");
            if (!string.IsNullOrEmpty(localized)) return localized;
        }
        return line.text;
    }

    public string GetLocalizedActor(DialogLine line)
    {
        if (line == null) return string.Empty;
        if (!string.IsNullOrEmpty(currentLanguage))
        {
            string localized = line.GetFieldValue($"Actor_{currentLanguage}");
            if (!string.IsNullOrEmpty(localized)) return localized;
        }
        return line.actorName;
    }

    private bool CheckCondition(string condition) => DialogUtility.EvaluateCondition(condition, name => GetVariable(name));

    private void ExecuteSimpleScript(string script) => DialogUtility.ExecuteScript(script, (name, val) => SetVariable(name, val));
}