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
    public void RefrigeratorInspectInteractable_OnlyCompletesRefrigeratorInspection()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject refrigeratorObject = new GameObject("Refrigerator");
        createdObjects.Add(refrigeratorObject);
        RefrigeratorInspectInteractable inspectInteractable = refrigeratorObject.AddComponent<RefrigeratorInspectInteractable>();
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
        RefrigeratorInspectInteractable inspectInteractable = refrigeratorObject.AddComponent<RefrigeratorInspectInteractable>();
        SetProgressManager(inspectInteractable, progressManager);

        Assert.That(inspectInteractable.CanInteract(inventory), Is.False);

        inspectInteractable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
    }

    [Test]
    public void GeneratorInteractable_OnlyCompletesGeneratorInspection()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject generatorObject = new GameObject("Generator");
        createdObjects.Add(generatorObject);
        GeneratorInteractable generatorInteractable = generatorObject.AddComponent<GeneratorInteractable>();
        SetProgressManager(generatorInteractable, progressManager);

        Assert.That(generatorInteractable.CanInteract(inventory), Is.False);

        progressManager.RestoreState(GameProgressState.FindGenerator);

        Assert.That(generatorInteractable.CanInteract(inventory), Is.True);
        generatorInteractable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGeneratorWire));
        Assert.That(generatorInteractable.CanInteract(inventory), Is.False);
    }

    [Test]
    public void GeneratorWirePickupProgress_CollectsOnlyAtWireObjective_AndAdvancesMission()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;

        GameObject wireObject = new GameObject("GeneratorWire");
        createdObjects.Add(wireObject);
        PickupInteractable wirePickup = wireObject.AddComponent<PickupInteractable>();
        SetGeneratorWirePickupReferences(wirePickup, saveManager);
        GeneratorWirePickupProgress wireProgress = wireObject.AddComponent<GeneratorWirePickupProgress>();
        SetGeneratorWirePickupProgressReferences(wireProgress, wirePickup, progressManager);
        wireProgress.enabled = false;
        wireProgress.enabled = true;

        Assert.That(wirePickup.enabled, Is.False);

        progressManager.RestoreState(GameProgressState.FindGeneratorWire);

        Assert.That(wirePickup.enabled, Is.True);
        InvokeTriggerEnter(wirePickup, playerCollider);

        Assert.That(inventory.HasItem("generator_wire", 1), Is.True);
        Assert.That(saveManager.IsItemCollected("GeneratorWire_01"), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.RepairGenerator));
        Assert.That(wireObject.activeSelf, Is.False);
    }

    [Test]
    public void NailsPickupProgress_CollectsOnlyAtNailsObjective_AndAdvancesMission()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;

        GameObject nailsObject = new GameObject("Nails");
        createdObjects.Add(nailsObject);
        PickupInteractable nailsPickup = nailsObject.AddComponent<PickupInteractable>();
        SetNailsPickupReferences(nailsPickup, saveManager);
        NailsPickupProgress nailsProgress = nailsObject.AddComponent<NailsPickupProgress>();
        SetNailsPickupProgressReferences(nailsProgress, nailsPickup, progressManager);
        nailsProgress.enabled = false;
        nailsProgress.enabled = true;

        Assert.That(nailsPickup.enabled, Is.False);

        progressManager.RestoreState(GameProgressState.FindNails);

        Assert.That(nailsPickup.enabled, Is.True);
        InvokeTriggerEnter(nailsPickup, playerCollider);

        Assert.That(inventory.HasItem("nails", 1), Is.True);
        Assert.That(saveManager.IsItemCollected("Nails_01"), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindHammer));
        Assert.That(nailsObject.activeSelf, Is.False);
    }

    [Test]
    public void GeneratorInteractable_ConsumesWireAndRepairsGenerator()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject generatorObject = new GameObject("Generator");
        createdObjects.Add(generatorObject);
        GeneratorInteractable generatorInteractable = generatorObject.AddComponent<GeneratorInteractable>();
        SetGeneratorReferences(generatorInteractable, progressManager);

        progressManager.RestoreState(GameProgressState.RepairGenerator);

        Assert.That(generatorInteractable.CanInteract(inventory), Is.False);

        ItemData generatorWire = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/GeneratorWire.asset");
        Assert.That(generatorWire, Is.Not.Null);
        Assert.That(inventory.TryAddItem(generatorWire, 1), Is.True);
        Assert.That(generatorInteractable.CanInteract(inventory), Is.True);

        generatorInteractable.Interact(inventory);

        Assert.That(inventory.HasItem("generator_wire", 1), Is.False);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindNails));
    }

    [Test]
    public void FrontDoor_RequiresCollectedKey_AndUnlocksWhenInteracted()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;

        GameObject keyObject = new GameObject("FrontDoorKey");
        createdObjects.Add(keyObject);
        PickupInteractable keyPickup = keyObject.AddComponent<PickupInteractable>();
        SetPickupReferences(keyPickup, saveManager);

        GameObject frontDoorObject = new GameObject("FrontDoor");
        createdObjects.Add(frontDoorObject);
        FrontDoorLock frontDoorLock = frontDoorObject.AddComponent<FrontDoorLock>();
        SetFrontDoorLockReferences(frontDoorLock, progressManager);

        Assert.That(frontDoorLock.IsLocked, Is.True);
        Assert.That(progressManager.TryCompleteKitchenArrival(), Is.True);
        Assert.That(progressManager.TryCompleteRefrigeratorInspection(), Is.True);
        Assert.That(frontDoorLock.CanInteract(inventory), Is.False);

        InvokeTriggerEnter(keyPickup, playerCollider);

        Assert.That(inventory.HasItem("kitchen_key", 1), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
        Assert.That(saveManager.IsItemCollected("FrontDoorKey_01"), Is.True);
        Assert.That(keyObject.activeSelf, Is.False);
        Assert.That(frontDoorLock.CanInteract(inventory), Is.True);

        frontDoorLock.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGenerator));
        Assert.That(frontDoorLock.IsUnlocked, Is.True);
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

    private static void SetPickupReferences(PickupInteractable pickupInteractable, SaveManager saveManager)
    {
        ItemData kitchenKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/KitchenKey.asset");
        Assert.That(kitchenKey, Is.Not.Null);

        SerializedObject serializedKey = new SerializedObject(pickupInteractable);
        serializedKey.FindProperty("itemData").objectReferenceValue = kitchenKey;
        serializedKey.FindProperty("amount").intValue = 1;
        serializedKey.FindProperty("saveId").stringValue = "FrontDoorKey_01";
        serializedKey.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedKey.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFrontDoorLockReferences(FrontDoorLock frontDoorLock, GameProgressManager progressManager)
    {
        ItemData kitchenKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/KitchenKey.asset");
        Assert.That(kitchenKey, Is.Not.Null);

        SerializedObject serializedDoor = new SerializedObject(frontDoorLock);
        serializedDoor.FindProperty("requiredKey").objectReferenceValue = kitchenKey;
        serializedDoor.FindProperty("requiredAmount").intValue = 1;
        serializedDoor.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedDoor.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetGeneratorWirePickupReferences(PickupInteractable wirePickup, SaveManager saveManager)
    {
        ItemData generatorWire = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/GeneratorWire.asset");
        Assert.That(generatorWire, Is.Not.Null);

        SerializedObject serializedPickup = new SerializedObject(wirePickup);
        serializedPickup.FindProperty("itemData").objectReferenceValue = generatorWire;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.FindProperty("saveId").stringValue = "GeneratorWire_01";
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetGeneratorWirePickupProgressReferences(
        GeneratorWirePickupProgress wireProgress,
        PickupInteractable wirePickup,
        GameProgressManager progressManager)
    {
        SerializedObject serializedProgress = new SerializedObject(wireProgress);
        serializedProgress.FindProperty("pickupInteractable").objectReferenceValue = wirePickup;
        serializedProgress.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedProgress.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetNailsPickupReferences(PickupInteractable nailsPickup, SaveManager saveManager)
    {
        ItemData nails = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Nails.asset");
        Assert.That(nails, Is.Not.Null);

        SerializedObject serializedPickup = new SerializedObject(nailsPickup);
        serializedPickup.FindProperty("itemData").objectReferenceValue = nails;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.FindProperty("saveId").stringValue = "Nails_01";
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetNailsPickupProgressReferences(
        NailsPickupProgress nailsProgress,
        PickupInteractable nailsPickup,
        GameProgressManager progressManager)
    {
        SerializedObject serializedProgress = new SerializedObject(nailsProgress);
        serializedProgress.FindProperty("pickupInteractable").objectReferenceValue = nailsPickup;
        serializedProgress.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedProgress.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetGeneratorReferences(GeneratorInteractable generatorInteractable, GameProgressManager progressManager)
    {
        ItemData generatorWire = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/GeneratorWire.asset");
        Assert.That(generatorWire, Is.Not.Null);

        SerializedObject serializedGenerator = new SerializedObject(generatorInteractable);
        serializedGenerator.FindProperty("requiredWire").objectReferenceValue = generatorWire;
        serializedGenerator.FindProperty("requiredWireAmount").intValue = 1;
        serializedGenerator.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedGenerator.ApplyModifiedPropertiesWithoutUndo();
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
