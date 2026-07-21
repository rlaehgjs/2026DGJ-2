using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SaveManagerSettingsTests
{
    private const string SettingsKey = "game-settings";

    private bool hadGameSave;
    private string previousGameSave;
    private bool hadSettings;
    private string previousSettings;

    [SetUp]
    public void SetUp()
    {
        hadGameSave = PlayerPrefs.HasKey(SaveManager.GameSaveKey);
        previousGameSave = PlayerPrefs.GetString(SaveManager.GameSaveKey);
        hadSettings = PlayerPrefs.HasKey(SettingsKey);
        previousSettings = PlayerPrefs.GetString(SettingsKey);

        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.DeleteKey(SettingsKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        RestorePlayerPrefsKey(SaveManager.GameSaveKey, hadGameSave, previousGameSave);
        RestorePlayerPrefsKey(SettingsKey, hadSettings, previousSettings);
        PlayerPrefs.Save();
    }

    [Test]
    public void SaveSettings_SavesAndLoadsAllRequestedValues()
    {
        GameObject managerObject = new GameObject("SaveManagerSettingsTests");

        try
        {
            SaveManager saveManager = managerObject.AddComponent<SaveManager>();
            object settings = CreateSettings(
                masterVolume: 0.2f,
                titleVolume: 0.3f,
                bgmVolume: 0.4f,
                sfxVolume: 0.5f,
                brightness: 0.6f,
                mouseSensitivity: 0.7f,
                language: 1);

            InvokeSaveSettings(saveManager, settings);
            object loadedSettings = InvokeLoadSettings(saveManager);

            Assert.That(PlayerPrefs.HasKey(SettingsKey), Is.True);
            Assert.That(GetFloat(loadedSettings, "MasterVolume"), Is.EqualTo(0.2f));
            Assert.That(GetFloat(loadedSettings, "TitleVolume"), Is.EqualTo(0.3f));
            Assert.That(GetFloat(loadedSettings, "BgmVolume"), Is.EqualTo(0.4f));
            Assert.That(GetFloat(loadedSettings, "SfxVolume"), Is.EqualTo(0.5f));
            Assert.That(GetFloat(loadedSettings, "Brightness"), Is.EqualTo(0.6f));
            Assert.That(GetFloat(loadedSettings, "MouseSensitivity"), Is.EqualTo(0.7f));
            Assert.That(GetInt(loadedSettings, "Language"), Is.EqualTo(1));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(managerObject);
        }
    }

    [Test]
    public void LoadSettings_WithoutStoredData_ReturnsDefaultSettings()
    {
        GameObject managerObject = new GameObject("SaveManagerSettingsTests");

        try
        {
            SaveManager saveManager = managerObject.AddComponent<SaveManager>();
            object settings = InvokeLoadSettings(saveManager);

            Assert.That(GetFloat(settings, "MasterVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(settings, "TitleVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(settings, "BgmVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(settings, "SfxVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(settings, "Brightness"), Is.EqualTo(0.5f));
            Assert.That(GetFloat(settings, "MouseSensitivity"), Is.EqualTo(0.5f));
            Assert.That(GetInt(settings, "Language"), Is.EqualTo(0));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(managerObject);
        }
    }

    [Test]
    public void SaveSettings_ClampsInvalidSliderAndLanguageValues()
    {
        GameObject managerObject = new GameObject("SaveManagerSettingsTests");

        try
        {
            SaveManager saveManager = managerObject.AddComponent<SaveManager>();
            object settings = CreateSettings(
                masterVolume: -1f,
                titleVolume: 2f,
                bgmVolume: -0.1f,
                sfxVolume: 1.1f,
                brightness: 3f,
                mouseSensitivity: -3f,
                language: 10);

            InvokeSaveSettings(saveManager, settings);
            object loadedSettings = InvokeLoadSettings(saveManager);

            Assert.That(GetFloat(loadedSettings, "MasterVolume"), Is.EqualTo(0f));
            Assert.That(GetFloat(loadedSettings, "TitleVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(loadedSettings, "BgmVolume"), Is.EqualTo(0f));
            Assert.That(GetFloat(loadedSettings, "SfxVolume"), Is.EqualTo(1f));
            Assert.That(GetFloat(loadedSettings, "Brightness"), Is.EqualTo(1f));
            Assert.That(GetFloat(loadedSettings, "MouseSensitivity"), Is.EqualTo(0f));
            Assert.That(GetInt(loadedSettings, "Language"), Is.EqualTo(1));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(managerObject);
        }
    }

    [Test]
    public void ClearGameSave_PreservesSavedSettings()
    {
        GameObject managerObject = new GameObject("SaveManagerSettingsTests");

        try
        {
            SaveManager saveManager = managerObject.AddComponent<SaveManager>();
            InvokeSaveSettings(saveManager, CreateSettings(language: 1));
            PlayerPrefs.SetString(SaveManager.GameSaveKey, "game-save-data");
            PlayerPrefs.Save();

            saveManager.ClearGameSave();

            Assert.That(PlayerPrefs.HasKey(SaveManager.GameSaveKey), Is.False);
            Assert.That(PlayerPrefs.HasKey(SettingsKey), Is.True);
            Assert.That(GetInt(InvokeLoadSettings(saveManager), "Language"), Is.EqualTo(1));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(managerObject);
        }
    }

    private static object CreateSettings(
        float masterVolume = 1f,
        float titleVolume = 1f,
        float bgmVolume = 1f,
        float sfxVolume = 1f,
        float brightness = 0.5f,
        float mouseSensitivity = 0.5f,
        int language = 0)
    {
        Type settingsType = GetSettingsType();
        object settings = Activator.CreateInstance(settingsType);

        SetField(settings, "MasterVolume", masterVolume);
        SetField(settings, "TitleVolume", titleVolume);
        SetField(settings, "BgmVolume", bgmVolume);
        SetField(settings, "SfxVolume", sfxVolume);
        SetField(settings, "Brightness", brightness);
        SetField(settings, "MouseSensitivity", mouseSensitivity);
        SetField(settings, "Language", language);
        return settings;
    }

    private static void InvokeSaveSettings(SaveManager saveManager, object settings)
    {
        MethodInfo saveSettings = typeof(SaveManager).GetMethod("SaveSettings", BindingFlags.Instance | BindingFlags.Public);
        Assert.That(saveSettings, Is.Not.Null, "SaveManager에 SaveSettings가 필요합니다.");
        saveSettings.Invoke(saveManager, new[] { settings });
    }

    private static object InvokeLoadSettings(SaveManager saveManager)
    {
        MethodInfo loadSettings = typeof(SaveManager).GetMethod("LoadSettings", BindingFlags.Instance | BindingFlags.Public);
        Assert.That(loadSettings, Is.Not.Null, "SaveManager에 LoadSettings가 필요합니다.");
        return loadSettings.Invoke(saveManager, null);
    }

    private static Type GetSettingsType()
    {
        Type settingsType = typeof(SaveManager).Assembly.GetType("GameSettingsData");
        Assert.That(settingsType, Is.Not.Null, "GameSettingsData가 필요합니다.");
        return settingsType;
    }

    private static void SetField(object settings, string fieldName, object value)
    {
        FieldInfo field = settings.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, $"{fieldName} 필드가 필요합니다.");
        field.SetValue(settings, value);
    }

    private static float GetFloat(object settings, string fieldName)
    {
        return (float)GetField(settings, fieldName).GetValue(settings);
    }

    private static int GetInt(object settings, string fieldName)
    {
        return (int)GetField(settings, fieldName).GetValue(settings);
    }

    private static FieldInfo GetField(object settings, string fieldName)
    {
        FieldInfo field = settings.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        Assert.That(field, Is.Not.Null, $"{fieldName} 필드가 필요합니다.");
        return field;
    }

    private static void RestorePlayerPrefsKey(string key, bool hadValue, string value)
    {
        if (hadValue)
        {
            PlayerPrefs.SetString(key, value);
            return;
        }

        PlayerPrefs.DeleteKey(key);
    }
}
