using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class LocalizationManager : SingletonEagerBehaviour<LocalizationManager>
{
    public static event Action OnLangChanged;
    private Dictionary<string, string> _texts;
    private CultureInfo _culture;

    public string CurrentLanguage => _culture?.TwoLetterISOLanguageName ?? "en";

    protected override async void InitInternal() =>
        await LoadLangAsync(PlayerPrefs.GetString("language", AutoDetectLang()));

    public async Task SetLang(string langCode)
    {
        if (langCode == _culture?.TwoLetterISOLanguageName) return;

        if (await LoadLangAsync(langCode))
        {
            PlayerPrefs.SetString("language", langCode);
            OnLangChanged?.Invoke();
        }
    }

    public string Localize(string key, params object[] args) =>
        _texts.TryGetValue(key, out string value)
            ? string.Format(value, args)
            : $"[{key}]";

    private async Task<bool> LoadLangAsync(string langCode)
    {
        _culture = new CultureInfo(langCode);
        _texts = await GoogleSheets.FetchGoogleSheetAsync("<SHEET_ID>", langCode) ??
                 await LoadFallbackLang();
        return _texts != null;
    }

    private async Task<Dictionary<string, string>> LoadFallbackLang()
    {
        DLog.LogW("Loading fallback language.");
        return await GoogleSheets.FetchGoogleSheetAsync("<SHEET_ID>", GoogleSheets.FallbackLanguage);
    }

    private string AutoDetectLang() =>
        Application.systemLanguage.ToString().ToLower()[..2];
}