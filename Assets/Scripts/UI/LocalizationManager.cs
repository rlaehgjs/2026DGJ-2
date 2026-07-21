using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    private const string LanguageKey = "Language";

    [SerializeField] private TextAsset localizationCsv;

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
        if (localizationCsv == null)
        {
            Debug.LogError("LocalizationManager에 Localization.csv를 연결해야 합니다.", this);
            return;
        }

        foreach (string line in localizationCsv.text.Replace("\r", "").Split('\n'))
        {
            string[] cells = line.Split(',');
            if (cells.Length == 3 && cells[0] != "key")
                texts[cells[0]] = new[] { cells[1], cells[2] };
        }
    }
}
