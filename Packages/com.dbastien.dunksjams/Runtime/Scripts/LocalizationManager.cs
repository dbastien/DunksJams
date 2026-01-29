using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class LocalizationManager : SingletonEagerBehaviour<LocalizationManager>
{
    public static event Action OnLangChanged;
    Dictionary<string, string> _texts;
    CultureInfo _culture;

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
        _texts.TryGetValue(key, out var value) 
            ? string.Format(value, args) 
            : $"[{key}]";

    async Task<bool> LoadLangAsync(string langCode)
    {
        _culture = new CultureInfo(langCode);
        _texts = await DataUtils.FetchGoogleSheetAsync("<SHEET_ID>", langCode) ?? 
                 await LoadFallbackLang();
        return _texts != null;
    }

    async Task<Dictionary<string, string>> LoadFallbackLang()
    {
        DLog.LogW("Loading fallback language.");
        return await DataUtils.FetchGoogleSheetAsync("<SHEET_ID>", DataUtils.FallbackLanguage);
    }

    string AutoDetectLang() =>
        Application.systemLanguage.ToString().ToLower().Substring(0, 2);
}