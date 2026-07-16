using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private const string LanguageKey = "Language";

    public static LocalizationManager Instance { get; private set; }
    public int CurrentLanguage => language;
    public event Action LanguageChanged;

    private readonly Dictionary<string, string[]> texts = new();
    private int language;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        language = PlayerPrefs.GetInt(LanguageKey, 0);
        LoadCsv();
    }

    public string Get(string key)
    {
        return texts.TryGetValue(key, out string[] value) ? value[language] : key;
    }

    public void SetLanguage(int value)
    {
        language = Mathf.Clamp(value, 0, 1);
        PlayerPrefs.SetInt(LanguageKey, language);
        PlayerPrefs.Save();
        LanguageChanged?.Invoke();
    }

    public void SetKorean()
    {
        SetLanguage(0);
    }

    public void SetEnglish()
    {
        SetLanguage(1);
    }

    private void LoadCsv()
    {
        TextAsset csv = Resources.Load<TextAsset>("Localization");
        foreach (string line in csv.text.Replace("\r", "").Split('\n'))
        {
            string[] cells = line.Split(',');
            if (cells.Length == 3 && cells[0] != "key")
                texts[cells[0]] = new[] { cells[1], cells[2] };
        }
    }
}
