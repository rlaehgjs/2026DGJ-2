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
    public void FreezerEntranceTrigger_RequiresContinuousTwoSecondStay()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        GameObject triggerObject = new GameObject("FreezerEntranceTrigger");
        createdObjects.Add(triggerObject);
        BoxCollider triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        FreezerEntranceTrigger freezerEntranceTrigger = triggerObject.AddComponent<FreezerEntranceTrigger>();
        SetFreezerEntranceTriggerReferences(freezerEntranceTrigger, triggerCollider, progressManager);
        ReinitializeLifecycle(freezerEntranceTrigger);

        Assert.That(triggerCollider.enabled, Is.False);
        InvokeTriggerEnter(freezerEntranceTrigger, playerCollider);
        AdvanceFreezerEntranceTimer(freezerEntranceTrigger, 2f);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindKitchen));

        progressManager.RestoreState(GameProgressState.EnterFreezer);

        Assert.That(triggerCollider.enabled, Is.True);
        InvokeTriggerEnter(freezerEntranceTrigger, playerCollider);
        AdvanceFreezerEntranceTimer(freezerEntranceTrigger, 1f);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.EnterFreezer));

        InvokeTriggerExit(freezerEntranceTrigger, playerCollider);
        AdvanceFreezerEntranceTimer(freezerEntranceTrigger, 1.1f);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.EnterFreezer));

        InvokeTriggerEnter(freezerEntranceTrigger, playerCollider);
        AdvanceFreezerEntranceTimer(freezerEntranceTrigger, 2f);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.Completed));
        Assert.That(triggerCollider.enabled, Is.False);
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
        ReinitializeLifecycle(wireProgress);

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
        ReinitializeLifecycle(nailsProgress);

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
    public void HammerPickupProgress_CollectsOnlyAtHammerObjective_AndAdvancesMission()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;

        GameObject hammerObject = new GameObject("Hammer");
        createdObjects.Add(hammerObject);
        PickupInteractable hammerPickup = hammerObject.AddComponent<PickupInteractable>();
        SetHammerPickupReferences(hammerPickup, saveManager);
        HammerPickupProgress hammerProgress = hammerObject.AddComponent<HammerPickupProgress>();
        SetHammerPickupProgressReferences(hammerProgress, hammerPickup, progressManager);
        ReinitializeLifecycle(hammerProgress);

        Assert.That(hammerPickup.enabled, Is.False);

        progressManager.RestoreState(GameProgressState.FindHammer);

        Assert.That(hammerPickup.enabled, Is.True);
        InvokeTriggerEnter(hammerPickup, playerCollider);

        Assert.That(inventory.HasItem("hammer", 1), Is.True);
        Assert.That(saveManager.IsItemCollected("Hammer_01"), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.RepairRefrigeratorWall));
        Assert.That(hammerObject.activeSelf, Is.False);
    }

    [Test]
    public void CoolantCapsulePickupProgress_CollectsOnlyAtCoolantCapsuleObjective_AndAdvancesMission()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out BoxCollider playerCollider);
        SaveManager saveManager = CreateSaveManager();
        SetSaveManagerReferences(saveManager, progressManager, inventory);
        saveManager.enabled = false;
        saveManager.enabled = true;

        GameObject coolantCapsuleObject = new GameObject("CoolantCapsule");
        createdObjects.Add(coolantCapsuleObject);
        PickupInteractable coolantCapsulePickup = coolantCapsuleObject.AddComponent<PickupInteractable>();
        SetCoolantCapsulePickupReferences(coolantCapsulePickup, saveManager);
        CoolantCapsulePickupProgress coolantCapsuleProgress = coolantCapsuleObject.AddComponent<CoolantCapsulePickupProgress>();
        SetCoolantCapsulePickupProgressReferences(coolantCapsuleProgress, coolantCapsulePickup, progressManager);
        ReinitializeLifecycle(coolantCapsuleProgress);

        Assert.That(coolantCapsulePickup.enabled, Is.False);

        progressManager.RestoreState(GameProgressState.FindCoolantCapsule);

        Assert.That(coolantCapsulePickup.enabled, Is.True);
        InvokeTriggerEnter(coolantCapsulePickup, playerCollider);

        Assert.That(inventory.HasItem("coolant_capsule", 1), Is.True);
        Assert.That(saveManager.IsItemCollected("CoolantCapsule_01"), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.RepairFreezer));
        Assert.That(coolantCapsuleObject.activeSelf, Is.False);
    }

    [Test]
    public void FreezerRepairInteractable_ConsumesCoolantOnlyAtRepairObjective()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject freezerObject = new GameObject("Freezer");
        createdObjects.Add(freezerObject);
        BoxCollider freezerCollider = freezerObject.AddComponent<BoxCollider>();
        FreezerRepairInteractable freezerRepair = freezerObject.AddComponent<FreezerRepairInteractable>();
        SetFreezerRepairReferences(freezerRepair, freezerCollider, progressManager);
        ReinitializeLifecycle(freezerRepair);

        Assert.That(freezerCollider.enabled, Is.False);
        Assert.That(freezerRepair.CanInteract(inventory), Is.False);

        progressManager.RestoreState(GameProgressState.RepairFreezer);

        Assert.That(freezerCollider.enabled, Is.True);
        Assert.That(freezerRepair.CanInteract(inventory), Is.False);

        ItemData coolantCapsule = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/CoolantCapsule.asset");
        Assert.That(coolantCapsule, Is.Not.Null);
        Assert.That(inventory.TryAddItem(coolantCapsule, 1), Is.True);
        Assert.That(freezerRepair.CanInteract(inventory), Is.True);

        freezerRepair.Interact(inventory);

        Assert.That(inventory.HasItem("coolant_capsule", 1), Is.False);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.EnterFreezer));
        Assert.That(freezerCollider.enabled, Is.False);
    }

    [Test]
    public void RefrigeratorWallRepairInteractable_ConsumesMaterialsOnlyAtRepairObjective()
    {
        GameProgressManager progressManager = CreateProgressManager();
        PlayerInventory inventory = CreatePlayerInventory(out _);
        GameObject wallObject = new GameObject("RefrigeratorWall");
        createdObjects.Add(wallObject);
        BoxCollider wallCollider = wallObject.AddComponent<BoxCollider>();
        RefrigeratorWallRepairInteractable wallRepair = wallObject.AddComponent<RefrigeratorWallRepairInteractable>();
        SetRefrigeratorWallRepairReferences(wallRepair, wallCollider, progressManager);
        ReinitializeLifecycle(wallRepair);

        Assert.That(wallCollider.enabled, Is.False);
        Assert.That(wallRepair.CanInteract(inventory), Is.False);

        progressManager.RestoreState(GameProgressState.RepairRefrigeratorWall);

        Assert.That(wallCollider.enabled, Is.True);
        Assert.That(wallRepair.CanInteract(inventory), Is.False);

        ItemData nails = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Nails.asset");
        ItemData hammer = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Hammer.asset");
        Assert.That(nails, Is.Not.Null);
        Assert.That(hammer, Is.Not.Null);
        Assert.That(inventory.TryAddItem(nails, 1), Is.True);
        Assert.That(inventory.TryAddItem(hammer, 1), Is.True);
        Assert.That(wallRepair.CanInteract(inventory), Is.True);

        wallRepair.Interact(inventory);

        Assert.That(inventory.HasItem("nails", 1), Is.False);
        Assert.That(inventory.HasItem("hammer", 1), Is.False);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindCoolantCapsule));
        Assert.That(wallCollider.enabled, Is.False);
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

        Assert.That(inventory.HasItem("FrontDoor_key", 1), Is.True);
        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
        Assert.That(saveManager.IsItemCollected("FrontDoorKey_01"), Is.True);
        Assert.That(keyObject.activeSelf, Is.False);
        Assert.That(frontDoorLock.CanInteract(inventory), Is.True);

        frontDoorLock.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGenerator));
        Assert.That(frontDoorLock.IsUnlocked, Is.True);
        Assert.That(inventory.GetItemAmount("FrontDoor_key"), Is.EqualTo(0),
            "The front door key must be consumed after the door is unlocked.");
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
        ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");
        Assert.That(frontDoorKey, Is.Not.Null);

        SerializedObject serializedKey = new SerializedObject(pickupInteractable);
        serializedKey.FindProperty("itemData").objectReferenceValue = frontDoorKey;
        serializedKey.FindProperty("amount").intValue = 1;
        serializedKey.FindProperty("saveId").stringValue = "FrontDoorKey_01";
        serializedKey.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedKey.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFrontDoorLockReferences(FrontDoorLock frontDoorLock, GameProgressManager progressManager)
    {
        ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");
        Assert.That(frontDoorKey, Is.Not.Null);

        SerializedObject serializedDoor = new SerializedObject(frontDoorLock);
        serializedDoor.FindProperty("requiredKey").objectReferenceValue = frontDoorKey;
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

    private static void SetHammerPickupReferences(PickupInteractable hammerPickup, SaveManager saveManager)
    {
        ItemData hammer = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Hammer.asset");
        Assert.That(hammer, Is.Not.Null);

        SerializedObject serializedPickup = new SerializedObject(hammerPickup);
        serializedPickup.FindProperty("itemData").objectReferenceValue = hammer;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.FindProperty("saveId").stringValue = "Hammer_01";
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetHammerPickupProgressReferences(
        HammerPickupProgress hammerProgress,
        PickupInteractable hammerPickup,
        GameProgressManager progressManager)
    {
        SerializedObject serializedProgress = new SerializedObject(hammerProgress);
        serializedProgress.FindProperty("pickupInteractable").objectReferenceValue = hammerPickup;
        serializedProgress.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedProgress.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetCoolantCapsulePickupReferences(PickupInteractable coolantCapsulePickup, SaveManager saveManager)
    {
        ItemData coolantCapsule = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/CoolantCapsule.asset");
        Assert.That(coolantCapsule, Is.Not.Null);

        SerializedObject serializedPickup = new SerializedObject(coolantCapsulePickup);
        serializedPickup.FindProperty("itemData").objectReferenceValue = coolantCapsule;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.FindProperty("saveId").stringValue = "CoolantCapsule_01";
        serializedPickup.FindProperty("saveManager").objectReferenceValue = saveManager;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetCoolantCapsulePickupProgressReferences(
        CoolantCapsulePickupProgress coolantCapsuleProgress,
        PickupInteractable coolantCapsulePickup,
        GameProgressManager progressManager)
    {
        SerializedObject serializedProgress = new SerializedObject(coolantCapsuleProgress);
        serializedProgress.FindProperty("pickupInteractable").objectReferenceValue = coolantCapsulePickup;
        serializedProgress.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedProgress.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFreezerRepairReferences(
        FreezerRepairInteractable freezerRepair,
        Collider freezerCollider,
        GameProgressManager progressManager)
    {
        ItemData coolantCapsule = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/CoolantCapsule.asset");
        Assert.That(coolantCapsule, Is.Not.Null);

        SerializedObject serializedRepair = new SerializedObject(freezerRepair);
        serializedRepair.FindProperty("requiredCoolantCapsule").objectReferenceValue = coolantCapsule;
        serializedRepair.FindProperty("requiredCoolantCapsuleAmount").intValue = 1;
        serializedRepair.FindProperty("interactionCollider").objectReferenceValue = freezerCollider;
        serializedRepair.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedRepair.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFreezerEntranceTriggerReferences(
        FreezerEntranceTrigger freezerEntranceTrigger,
        Collider triggerCollider,
        GameProgressManager progressManager)
    {
        SerializedObject serializedTrigger = new SerializedObject(freezerEntranceTrigger);
        serializedTrigger.FindProperty("triggerCollider").objectReferenceValue = triggerCollider;
        serializedTrigger.FindProperty("requiredStaySeconds").floatValue = 2f;
        serializedTrigger.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetRefrigeratorWallRepairReferences(
        RefrigeratorWallRepairInteractable wallRepair,
        Collider wallCollider,
        GameProgressManager progressManager)
    {
        ItemData nails = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Nails.asset");
        ItemData hammer = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Hammer.asset");
        Assert.That(nails, Is.Not.Null);
        Assert.That(hammer, Is.Not.Null);

        SerializedObject serializedRepair = new SerializedObject(wallRepair);
        serializedRepair.FindProperty("requiredNails").objectReferenceValue = nails;
        serializedRepair.FindProperty("requiredNailsAmount").intValue = 1;
        serializedRepair.FindProperty("requiredHammer").objectReferenceValue = hammer;
        serializedRepair.FindProperty("requiredHammerAmount").intValue = 1;
        serializedRepair.FindProperty("interactionCollider").objectReferenceValue = wallCollider;
        serializedRepair.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedRepair.ApplyModifiedPropertiesWithoutUndo();
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

    private static void ReinitializeLifecycle(Component component)
    {
        InvokeLifecycleMethod(component, "OnDisable");
        InvokeLifecycleMethod(component, "OnEnable");
    }

    private static void InvokeLifecycleMethod(Component component, string methodName)
    {
        MethodInfo lifecycleMethod = component.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(lifecycleMethod, Is.Not.Null);
        lifecycleMethod.Invoke(component, null);
    }

    private static void InvokeTriggerEnter(Component component, Collider playerCollider)
    {
        MethodInfo triggerEnterMethod = component.GetType().GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null);
        triggerEnterMethod.Invoke(component, new object[] { playerCollider });
    }

    private static void InvokeTriggerExit(Component component, Collider playerCollider)
    {
        MethodInfo triggerExitMethod = component.GetType().GetMethod(
            "OnTriggerExit",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerExitMethod, Is.Not.Null);
        triggerExitMethod.Invoke(component, new object[] { playerCollider });
    }

    private static void AdvanceFreezerEntranceTimer(FreezerEntranceTrigger freezerEntranceTrigger, float deltaTime)
    {
        MethodInfo advanceTimerMethod = typeof(FreezerEntranceTrigger).GetMethod(
            "AdvanceStayTimer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(advanceTimerMethod, Is.Not.Null);
        advanceTimerMethod.Invoke(freezerEntranceTrigger, new object[] { deltaTime });
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
