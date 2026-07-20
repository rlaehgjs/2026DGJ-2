using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
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

    [Test]
    public void KitchenArrivalTrigger_OnlyCompletesFindKitchen()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        GameObject triggerObject = new GameObject("KitchenArrivalTrigger");
        createdObjects.Add(triggerObject);
        KitchenArrivalTrigger kitchenTrigger = triggerObject.AddComponent<KitchenArrivalTrigger>();
        SetProgressManager(kitchenTrigger, progressManager);

        InvokeTriggerEnter(kitchenTrigger, playerCollider);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.InspectRefrigerator));

        progressManager.RestoreState(GameProgressState.FindFrontDoorKey);
        InvokeTriggerEnter(kitchenTrigger, playerCollider);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
        Assert.That(inventory, Is.Not.Null);
    }

    [Test]
    public void InspectInteractable_OnlyCompletesRefrigeratorInspection()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject refrigeratorObject = new GameObject("Refrigerator");
        createdObjects.Add(refrigeratorObject);
        InspectInteractable inspectInteractable = refrigeratorObject.AddComponent<InspectInteractable>();
        SetProgressManager(inspectInteractable, progressManager);

        Assert.That(inspectInteractable.CanInteract(inventory), Is.False);

        Assert.That(progressManager.TryCompleteKitchenArrival(), Is.True);
        Assert.That(inspectInteractable.CanInteract(inventory), Is.True);

        inspectInteractable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
        Assert.That(inspectInteractable.CanInteract(inventory), Is.False);

        inspectInteractable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
    }

    [Test]
    public void RestoredFindFrontDoorKey_DoesNotAllowSecondRefrigeratorCompletion()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;
        saveManager.SaveGame(new GameSaveData
        {
            Version = GameSaveData.CurrentVersion,
            ProgressState = GameProgressState.FindFrontDoorKey
        });

        Assert.That(saveManager.LoadCurrentProgress(), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));

        GameObject refrigeratorObject = new GameObject("Refrigerator");
        createdObjects.Add(refrigeratorObject);
        InspectInteractable inspectInteractable = refrigeratorObject.AddComponent<InspectInteractable>();
        SetProgressManager(inspectInteractable, progressManager);

        Assert.That(inspectInteractable.CanInteract(inventory), Is.False);

        inspectInteractable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
    }

    private PlayerInventory CreatePlayerInventory(out BoxCollider playerCollider)
    {
        GameObject playerObject = new GameObject("Player");
        createdObjects.Add(playerObject);
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        playerCollider = playerObject.AddComponent<BoxCollider>();
        return inventory;
    }

    private static void SetProgressManager(Component component, GameProgressManager progressManager)
    {
        SerializedObject serializedComponent = new SerializedObject(component);
        serializedComponent.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedComponent.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetSaveManagerReferences(
        SaveManager saveManager,
        GameProgressManager progressManager,
        PlayerInventory inventory)
    {
        SerializedObject serializedSaveManager = new SerializedObject(saveManager);
        serializedSaveManager.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedSaveManager.FindProperty("playerInventory").objectReferenceValue = inventory;
        serializedSaveManager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void InvokeTriggerEnter(Component component, Collider playerCollider)
    {
        MethodInfo triggerEnterMethod = component.GetType().GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null);
        triggerEnterMethod.Invoke(component, new object[] { playerCollider });
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
