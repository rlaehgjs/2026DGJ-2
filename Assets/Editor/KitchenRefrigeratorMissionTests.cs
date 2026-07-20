using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class KitchenRefrigeratorMissionTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }

    [TearDown]
    public void TearDown()
    {
        for (int index = createdObjects.Count - 1; index >= 0; index--)
        {
            Object.DestroyImmediate(createdObjects[index]);
        }

        createdObjects.Clear();
        PlayerPrefs.DeleteKey(SaveManager.GameSaveKey);
        PlayerPrefs.Save();
    }

    [Test]
    public void TryCompleteKitchenArrival_AdvancesOnlyFromFindKitchen()
    {
        GameProgressManager progressManager = CreateProgressManager();

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindKitchen));
        Assert.That(progressManager.TryCompleteKitchenArrival(), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.InspectRefrigerator));
        Assert.That(progressManager.TryCompleteKitchenArrival(), Is.False);

        progressManager.RestoreState(GameProgressState.FindFrontDoorKey);

        Assert.That(progressManager.TryCompleteKitchenArrival(), Is.False);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
    }

    [TestCase(0, GameProgressState.FindKitchen)]
    [TestCase(1, GameProgressState.InspectRefrigerator)]
    [TestCase(2, GameProgressState.FindGenerator)]
    [TestCase(11, GameProgressState.Completed)]
    public void TryLoadGame_Version1Progress_MapsToVersion2(int legacyProgressValue, GameProgressState expectedState)
    {
        GameSaveData legacySave = new GameSaveData
        {
            Version = 1,
            ProgressState = (GameProgressState)legacyProgressValue
        };

        PlayerPrefs.SetString(SaveManager.GameSaveKey, JsonUtility.ToJson(legacySave));
        PlayerPrefs.Save();

        SaveManager saveManager = CreateSaveManager();

        Assert.That(saveManager.TryLoadGame(out GameSaveData migratedSave), Is.True);
        Assert.That(migratedSave.Version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(migratedSave.ProgressState, Is.EqualTo(expectedState));

        GameSaveData persistedSave = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(SaveManager.GameSaveKey));
        Assert.That(persistedSave.Version, Is.EqualTo(GameSaveData.CurrentVersion));
        Assert.That(persistedSave.ProgressState, Is.EqualTo(expectedState));
    }

    [TestCase(-1)]
    [TestCase(12)]
    public void TryLoadGame_InvalidVersion1Progress_StartsAtFindKitchen(int legacyProgressValue)
    {
        GameSaveData legacySave = new GameSaveData
        {
            Version = 1,
            ProgressState = (GameProgressState)legacyProgressValue
        };

        PlayerPrefs.SetString(SaveManager.GameSaveKey, JsonUtility.ToJson(legacySave));
        PlayerPrefs.Save();

        SaveManager saveManager = CreateSaveManager();

        Assert.That(saveManager.TryLoadGame(out GameSaveData migratedSave), Is.True);
        Assert.That(migratedSave.ProgressState, Is.EqualTo(GameProgressState.FindKitchen));
    }

    private GameProgressManager CreateProgressManager()
    {
        GameObject progressObject = new GameObject("GameProgressManager");
        createdObjects.Add(progressObject);
        return progressObject.AddComponent<GameProgressManager>();
    }

    private SaveManager CreateSaveManager()
    {
        GameObject saveObject = new GameObject("SaveManager");
        createdObjects.Add(saveObject);
        return saveObject.AddComponent<SaveManager>();
    }
}
