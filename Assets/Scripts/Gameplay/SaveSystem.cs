using System;
using System.IO;
using UnityEngine;

public class SaveSystem
{
    readonly string _saveDir;
    const string DefaultFileName = "SaveData.json";

    public event Action OnSaveSuccess, OnLoadSuccess;
    public event Action<string> OnSaveError, OnLoadError;

    public SaveSystem(string saveDir = null)
    {
        _saveDir = saveDir ?? Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(_saveDir); // Ensure the directory exists
    }

    public void Save<T>(T data, string fileName = DefaultFileName)
    {
        string filePath = Path.Combine(_saveDir, fileName);
        try
        {
            string json = JsonUtility.ToJson(data, true); // Pretty print for readability
            File.WriteAllText(filePath, json);
            OnSaveSuccess?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            OnSaveError?.Invoke(e.Message);
        }
    }

    public T Load<T>(string fileName = DefaultFileName)
    {
        string filePath = Path.Combine(_saveDir, fileName);
        try
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Save file not found: {filePath}");

            string json = File.ReadAllText(filePath);
            T data = JsonUtility.FromJson<T>(json);
            OnLoadSuccess?.Invoke();
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            OnLoadError?.Invoke(e.Message);
            return default;
        }
    }

    public void DeleteSave(string fileName = DefaultFileName)
    {
        string filePath = Path.Combine(_saveDir, fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
    }

    public bool SaveExists(string fileName = DefaultFileName) =>
        File.Exists(Path.Combine(_saveDir, fileName));
}
