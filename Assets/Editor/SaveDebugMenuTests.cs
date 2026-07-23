using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class SaveDebugMenuTests
{
    private const string TestSettingsKey = "test-settings-key";

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.DeleteKey(TestSettingsKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.DeleteKey(TestSettingsKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void ClearGameSave_DeletesOnlyGameSaveKey()
    {
        PlayerPrefs.SetString(SaveManager.GameSaveKey, "game-save-data");
        PlayerPrefs.SetString(TestSettingsKey, "keep-this-setting");
        PlayerPrefs.Save();

        Type menuType = typeof(SaveDebugMenuTests).Assembly.GetType("SaveDebugMenu");
        Assert.That(menuType, Is.Not.Null);

        MethodInfo clearGameSaveMethod = menuType.GetMethod(
            "ClearGameSave",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(clearGameSaveMethod, Is.Not.Null);

        clearGameSaveMethod.Invoke(null, null);

        Assert.That(PlayerPrefs.HasKey(SaveManager.GameSaveKey), Is.False);
        Assert.That(PlayerPrefs.GetString(TestSettingsKey), Is.EqualTo("keep-this-setting"));
    }
}
