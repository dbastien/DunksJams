using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogUI : MonoBehaviour
{
    [Header("UI References")] public GameObject panel;
    public TextMeshProUGUI actorText;
    public TextMeshProUGUI dialogText;
    public RectTransform responseContainer;
    public GameObject responseButtonPrefab;

    [Header("Settings")] public float typeSpeed = 0.03f;
    public bool allowSkip = true;

    private List<GameObject> _activeButtons = new();
    private Coroutine _typeRoutine;
    private bool _isTyping;
    private string _fullText;
    private Dictionary<int, float> _pauses;

    private void Start()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnEntryStarted += OnEntryStarted;
            DialogManager.Instance.OnConversationEnded += OnConversationEnded;
        }

        if (panel != null) panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnEntryStarted -= OnEntryStarted;
            DialogManager.Instance.OnConversationEnded -= OnConversationEnded;
        }
    }

    private void Update()
    {
        if (_isTyping &&
            allowSkip &&
            (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            FinishTyping();
    }

    private void OnEntryStarted(DialogEntry entry, DialogLine line)
    {
        if (panel != null) panel.SetActive(true);

        string localizedActor = DialogManager.Instance.GetLocalizedActor(line);
        string processedActor = DialogManager.Instance.ProcessText(localizedActor);
        if (actorText != null) actorText.text = processedActor;

        string localizedText = DialogManager.Instance.GetLocalizedText(line);
        string processedText = DialogManager.Instance.ProcessText(localizedText);
        _fullText = DialogUtility.ParsePauseTags(processedText, out _pauses);

        // Add to history
        if (DialogHistoryManager.Instance != null)
            DialogHistoryManager.Instance.AddToHistory(processedActor, _fullText);

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeWriterRoutine(_fullText));

        UpdateResponses();
    }

    private IEnumerator TypeWriterRoutine(string text)
    {
        _isTyping = true;
        dialogText.text = text;
        dialogText.maxVisibleCharacters = 0;

        // Force TMPro to parse text
        dialogText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogText.textInfo.characterCount;
        var counter = 0;

        while (counter <= totalVisibleCharacters)
        {
            dialogText.maxVisibleCharacters = counter;

            if (_pauses.TryGetValue(counter, out float pauseDuration)) yield return new WaitForSeconds(pauseDuration);

            yield return new WaitForSeconds(typeSpeed);
            counter++;
        }

        FinishTyping();
    }

    private void FinishTyping()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = null;
        _isTyping = false;
        dialogText.maxVisibleCharacters = 99999;
    }

    private void OnConversationEnded()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void UpdateResponses()
    {
        // Clear old buttons
        foreach (GameObject btn in _activeButtons) Destroy(btn);
        _activeButtons.Clear();

        // Only show responses if we are at the end of the stack
        if (DialogManager.Instance.currentEntry == null) return;
        if (DialogManager.Instance.currentLineIndex < DialogManager.Instance.currentEntry.lines.Count - 1) return;

        List<DialogLink> links = DialogManager.Instance.GetValidLinks();

        for (var i = 0; i < links.Count; i++)
        {
            int index = i;
            GameObject btnObj = Instantiate(responseButtonPrefab, responseContainer);
            _activeButtons.Add(btnObj);

            var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            string menuText = links[i].text;

            // Localization for menu text
            if (!string.IsNullOrEmpty(DialogManager.Instance.currentLanguage))
            {
                string locMenu = links[i].fields.GetFieldValue($"Text_{DialogManager.Instance.currentLanguage}");
                if (!string.IsNullOrEmpty(locMenu)) menuText = locMenu;
            }

            if (txt != null)
            {
                if (string.IsNullOrEmpty(menuText)) menuText = "Next...";
                txt.text = DialogManager.Instance.ProcessText(menuText);
            }

            var btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string finalMenuText = menuText;
                btn.onClick.AddListener(() =>
                {
                    // Record choice in history
                    if (DialogHistoryManager.Instance != null &&
                        !string.IsNullOrEmpty(finalMenuText) &&
                        finalMenuText != "Next...")
                        DialogHistoryManager.Instance.AddToHistory("Player", finalMenuText, true);
                    DialogManager.Instance.Next(index);
                });
            }
        }
    }
}