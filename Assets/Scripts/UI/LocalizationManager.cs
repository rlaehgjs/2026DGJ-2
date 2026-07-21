using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    [SerializeField] private TextAsset localizationCsv;

    public static LocalizationManager Instance { get; private set; }
    public int CurrentLanguage => language;
    public event Action LanguageChanged;

    private readonly Dictionary<string, string[]> texts = new();
    private int language;
    private SaveManager saveManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadCsv();
    }

    private void Start()
    {
        if (saveManager != null)
            language = saveManager.LoadSettings().Language;

        LanguageChanged?.Invoke();
    }

    public void Configure(SaveManager settingsSaveManager)
    {
        saveManager = settingsSaveManager;

        if (saveManager != null)
            language = saveManager.LoadSettings().Language;
    }

    public string Get(string key)
    {
        return texts.TryGetValue(key, out string[] value) ? value[language] : key;
    }

    public void SetLanguage(int value)
    {
        language = Mathf.Clamp(value, GameSettingsData.KoreanLanguage, GameSettingsData.EnglishLanguage);

        if (saveManager != null)
        {
            GameSettingsData settings = saveManager.LoadSettings();
            settings.Language = language;
            saveManager.SaveSettings(settings);
        }

        LanguageChanged?.Invoke();
    }

    public void SetKorean()
    {
        SetLanguage(GameSettingsData.KoreanLanguage);
    }

    public void SetEnglish()
    {
        SetLanguage(GameSettingsData.EnglishLanguage);
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
