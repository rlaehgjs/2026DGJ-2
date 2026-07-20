# Kitchen Entry and Refrigerator Inspection Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver the temporary-object mission slice `FindKitchen -> InspectRefrigerator -> FindFrontDoorKey` in `InteractionSandbox`, including legacy-save migration and regression coverage.

**Architecture:** The progress manager remains the sole owner of mission transitions. A trigger asks it to complete kitchen arrival, while a focused `IInteractable` asks it to complete the refrigerator inspection. `SaveManager` upgrades version-1 numeric progress values before they are exposed or restored, preventing the inserted objective from reinterpreting old saves.

**Tech Stack:** Unity 6000.4.11f1, C#, Unity Test Framework/NUnit edit-mode tests, Unity YAML scene assets.

## Global Constraints

- Implement only issue #24: temporary objects in `Assets/Scenes/Sandbox/InteractionSandbox.unity`.
- Keep `PickupInteractable` automatic trigger pickup unchanged.
- Do not add ObjectiveUI, key pickup, doors, generator, repair, environment effects, MainGameScene placement, or final models.
- Preserve `GameProgressManager.TryCompleteRefrigeratorInspection()` as the public refrigerator-completion API.
- Upgrade `GameSaveData.CurrentVersion` from `1` to `2`; migrate only version-1 progress data before restoration.
- Run edit-mode tests with `C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe`.

---

## File Structure

- `Assets/Scripts/Core/Progress/GameProgressState.cs` — canonical enum order for the new first mission slice.
- `Assets/Scripts/Core/Progress/GameProgressManager.cs` — guarded mission transitions owned by the progress manager.
- `Assets/Scripts/Core/Save/GameSaveData.cs` — current save schema version and default progress state.
- `Assets/Scripts/Core/Save/SaveManager.cs` — version-1 numeric progress migration before load/restore.
- `Assets/Scripts/Interaction/KitchenArrivalTrigger.cs` — player-triggered request to complete kitchen arrival.
- `Assets/Scripts/Interaction/InspectInteractable.cs` — F-key refrigerator inspection through `IInteractable`.
- `Assets/Editor/KitchenRefrigeratorMissionTests.cs` — edit-mode tests for transition guards, migration, interactions, and restore idempotence.
- `Assets/Editor/InteractionSandboxSetupTests.cs` — edit-mode assertions for required sandbox wiring.
- `Assets/Scenes/Sandbox/InteractionSandbox.unity` — temporary player, managers, trigger, and refrigerator cube configuration.

### Task 1: Define the Progress Contract and Migrate Legacy Saves

**Files:**
- Modify: `Assets/Scripts/Core/Progress/GameProgressState.cs`
- Modify: `Assets/Scripts/Core/Progress/GameProgressManager.cs`
- Modify: `Assets/Scripts/Core/Save/GameSaveData.cs`
- Modify: `Assets/Scripts/Core/Save/SaveManager.cs`
- Create: `Assets/Editor/KitchenRefrigeratorMissionTests.cs`

**Interfaces:**
- Produces: `bool GameProgressManager.TryCompleteKitchenArrival()`; it succeeds only at `GameProgressState.FindKitchen`.
- Produces: `GameProgressState.FindKitchen`, `GameProgressState.InspectRefrigerator`, and `GameProgressState.FindFrontDoorKey` as the first three enum values.
- Produces: `SaveManager.TryLoadGame(out GameSaveData)` returning version-2 data for every version-1 save.

- [x] **Step 1: Write the failing progress and migration tests**

Create `Assets/Editor/KitchenRefrigeratorMissionTests.cs` with this content:

```csharp
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
```

- [x] **Step 2: Run the edit-mode tests and confirm the expected compile failure**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\vkccl\Desktop\Github\2026DGJ-2" -runTests -runSynchronously -testPlatform EditMode -testFilter KitchenRefrigeratorMissionTests -testResults "C:\Users\vkccl\Desktop\Github\2026DGJ-2\Logs\KitchenRefrigeratorMissionTests.xml"
```

Expected: compilation fails because `FindKitchen`, `FindFrontDoorKey`, and `TryCompleteKitchenArrival` do not exist yet.

- [x] **Step 3: Implement the state order, guarded transitions, and migration**

Replace `Assets/Scripts/Core/Progress/GameProgressState.cs` with:

```csharp
public enum GameProgressState
{
    FindKitchen,
    InspectRefrigerator,
    FindFrontDoorKey,
    FindGenerator,
    FindGeneratorWire,
    RepairGenerator,
    FindPlywood,
    FindHammer,
    RepairRefrigeratorWall,
    FindCoolantCapsule,
    RepairFreezer,
    EnterFreezer,
    Completed
}
```

Make these exact changes in `Assets/Scripts/Core/Progress/GameProgressManager.cs`:

```csharp
[SerializeField] private GameProgressState initialState = GameProgressState.FindKitchen;
```

```csharp
case GameProgressState.FindKitchen:
    SetState(GameProgressState.InspectRefrigerator, true);
    return true;
case GameProgressState.InspectRefrigerator:
    SetState(GameProgressState.FindFrontDoorKey, true);
    return true;
case GameProgressState.FindFrontDoorKey:
    SetState(GameProgressState.FindGenerator, true);
    return true;
```

Replace `TryCompleteKitchenKey` with:

```csharp
public bool TryCompleteKitchenArrival()
{
    return TryCompleteExpectedObjective(GameProgressState.FindKitchen);
}
```

Keep `TryCompleteRefrigeratorInspection` public and unchanged in signature:

```csharp
public bool TryCompleteRefrigeratorInspection()
{
    return TryCompleteExpectedObjective(GameProgressState.InspectRefrigerator);
}
```

In `Assets/Scripts/Core/Save/GameSaveData.cs`, set the new schema defaults:

```csharp
public const int CurrentVersion = 2;

public int Version = CurrentVersion;
public GameProgressState ProgressState = GameProgressState.FindKitchen;
```

In `Assets/Scripts/Core/Save/SaveManager.cs`, replace the successful `TryLoadGame` deserialization tail with:

```csharp
currentSaveData = JsonUtility.FromJson<GameSaveData>(PlayerPrefs.GetString(GameSaveKey));

if (currentSaveData == null)
{
    return false;
}

currentSaveData.EnsureCollections();

if (MigrateSaveData(currentSaveData))
{
    SaveGame(currentSaveData);
}

saveData = currentSaveData;
return true;
```

Add these private methods before `HandleProgressChanged`:

```csharp
private static bool MigrateSaveData(GameSaveData saveData)
{
    if (saveData.Version != 1)
    {
        return false;
    }

    saveData.ProgressState = MigrateVersion1ProgressState((int)saveData.ProgressState);
    saveData.Version = GameSaveData.CurrentVersion;
    return true;
}

private static GameProgressState MigrateVersion1ProgressState(int legacyProgressValue)
{
    const int version1FindKitchenKey = 0;
    const int version1InspectRefrigerator = 1;
    const int version1Completed = 11;

    if (legacyProgressValue < version1FindKitchenKey || legacyProgressValue > version1Completed)
    {
        return GameProgressState.FindKitchen;
    }

    if (legacyProgressValue <= version1InspectRefrigerator)
    {
        return (GameProgressState)legacyProgressValue;
    }

    return (GameProgressState)(legacyProgressValue + 1);
}
```

- [x] **Step 4: Run the tests and confirm they pass**

Run the Step 2 command again.

Expected: `KitchenRefrigeratorMissionTests` completes with all seven test cases passing.

- [x] **Step 5: Commit the progress and migration contract**

```powershell
git add -- Assets/Scripts/Core/Progress/GameProgressState.cs Assets/Scripts/Core/Progress/GameProgressManager.cs Assets/Scripts/Core/Save/GameSaveData.cs Assets/Scripts/Core/Save/SaveManager.cs Assets/Editor/KitchenRefrigeratorMissionTests.cs
git commit -m "Feat: #24 미션 진행과 저장 마이그레이션"
```

### Task 2: Add Kitchen Trigger and Refrigerator Inspection Components

**Files:**
- Create: `Assets/Scripts/Interaction/KitchenArrivalTrigger.cs`
- Create: `Assets/Scripts/Interaction/InspectInteractable.cs`
- Modify: `Assets/Editor/KitchenRefrigeratorMissionTests.cs`

**Interfaces:**
- Consumes: `GameProgressManager.TryCompleteKitchenArrival()` and `GameProgressManager.TryCompleteRefrigeratorInspection()`.
- Consumes: `IInteractable.CanInteract(PlayerInventory)` and `IInteractable.Interact(PlayerInventory)`.
- Produces: `KitchenArrivalTrigger` and `InspectInteractable`, both with serialized `GameProgressManager` references.

- [x] **Step 1: Write the failing trigger and inspection tests**

Add these `using` directives to `Assets/Editor/KitchenRefrigeratorMissionTests.cs`:

```csharp
using System.Reflection;
using UnityEditor;
```

Add these tests and helpers before the existing `CreateProgressManager` method:

```csharp
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
```

- [x] **Step 2: Run the edit-mode tests and confirm the expected compile failure**

Run the Task 1 test command.

Expected: compilation fails because `KitchenArrivalTrigger` and `InspectInteractable` have not been created.

- [x] **Step 3: Implement the two focused interaction components**

Create `Assets/Scripts/Interaction/KitchenArrivalTrigger.cs`:

```csharp
using UnityEngine;

public class KitchenArrivalTrigger : MonoBehaviour
{
    [SerializeField] private GameProgressManager gameProgressManager;

    private void OnTriggerEnter(Collider other)
    {
        if (gameProgressManager == null)
        {
            return;
        }

        PlayerInventory inventory = other.GetComponentInParent<PlayerInventory>();

        if (inventory == null)
        {
            return;
        }

        gameProgressManager.TryCompleteKitchenArrival();
    }
}
```

Create `Assets/Scripts/Interaction/InspectInteractable.cs`:

```csharp
using UnityEngine;

public class InspectInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameProgressManager gameProgressManager;

    public bool CanInteract(PlayerInventory inventory)
    {
        return gameProgressManager != null
            && gameProgressManager.CurrentState == GameProgressState.InspectRefrigerator;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (gameProgressManager == null)
        {
            return;
        }

        gameProgressManager.TryCompleteRefrigeratorInspection();
    }
}
```

- [x] **Step 4: Run the tests and confirm they pass**

Run the Task 1 test command.

Expected: all ten `KitchenRefrigeratorMissionTests` cases pass.

- [x] **Step 5: Commit the gameplay interaction components**

```powershell
git add -- Assets/Scripts/Interaction/KitchenArrivalTrigger.cs Assets/Scripts/Interaction/KitchenArrivalTrigger.cs.meta Assets/Scripts/Interaction/InspectInteractable.cs Assets/Scripts/Interaction/InspectInteractable.cs.meta Assets/Editor/KitchenRefrigeratorMissionTests.cs
git commit -m "Feat: #24 주방 진입과 냉장고 조사 상호작용"
```

### Task 3: Configure and Verify InteractionSandbox

**Files:**
- Modify: `Assets/Scenes/Sandbox/InteractionSandbox.unity`
- Create: `Assets/Editor/InteractionSandboxSetupTests.cs`

**Interfaces:**
- Consumes: `KitchenArrivalTrigger`, `InspectInteractable`, scene `GameProgressManager`, scene `SaveManager`, and `Player(Test)`.
- Produces: a sandbox scene in which the trigger and refrigerator reference the scene progress manager and the save manager references the scene progress manager and player inventory.

- [x] **Step 1: Write the failing sandbox-wiring test**

Create `Assets/Editor/InteractionSandboxSetupTests.cs`:

```csharp
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionSandboxSetupTests
{
    private const string ScenePath = "Assets/Scenes/Sandbox/InteractionSandbox.unity";

    [Test]
    public void InteractionSandbox_HasWiredKitchenMissionObjects()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            SaveManager saveManager = FindComponent<SaveManager>(sandboxScene);
            KitchenArrivalTrigger kitchenTrigger = FindComponent<KitchenArrivalTrigger>(sandboxScene);
            InspectInteractable refrigerator = FindComponent<InspectInteractable>(sandboxScene);

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInteraction.gameObject.name, Is.EqualTo("Player(Test)"));
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(saveManager, Is.Not.Null);
            Assert.That(kitchenTrigger, Is.Not.Null);
            Assert.That(refrigerator, Is.Not.Null);

            BoxCollider triggerCollider = kitchenTrigger.GetComponent<BoxCollider>();
            BoxCollider refrigeratorCollider = refrigerator.GetComponent<BoxCollider>();
            Assert.That(triggerCollider, Is.Not.Null);
            Assert.That(triggerCollider.isTrigger, Is.True);
            Assert.That(refrigeratorCollider, Is.Not.Null);
            Assert.That(refrigeratorCollider.isTrigger, Is.False);

            Assert.That(GetObjectReference(kitchenTrigger, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(refrigerator, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(saveManager, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(saveManager, "playerInventory"), Is.EqualTo(playerInventory));
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    private static T FindComponent<T>(Scene scene) where T : Component
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            T component = rootObject.GetComponentInChildren<T>(true);

            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static Object GetObjectReference(Object component, string propertyName)
    {
        return new SerializedObject(component).FindProperty(propertyName).objectReferenceValue;
    }
}
```

- [x] **Step 2: Run the sandbox setup test and confirm it fails**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\vkccl\Desktop\Github\2026DGJ-2" -runTests -runSynchronously -testPlatform EditMode -testFilter InteractionSandboxSetupTests -testResults "C:\Users\vkccl\Desktop\Github\2026DGJ-2\Logs\InteractionSandboxSetupTests.xml"
```

Expected: the test fails because the current sandbox lacks the scene-level mission wiring.

- [x] **Step 3: Author the scene in Unity**

Open `Assets/Scenes/Sandbox/InteractionSandbox.unity` in Unity 6000.4.11f1 and make these exact scene-only changes:

1. Add the `Assets/Prefabs/Core/GameProgressManager.prefab` as a root object named `GameProgressManager`; set its `Initial State` to `FindKitchen`.
2. Keep the existing `Player(Test)` prefab instance and verify its `PlayerInteraction` has the child `Main Camera` assigned as `Interaction Camera` with distance `3`.
3. Select the existing `SaveManager` scene instance. Assign the scene `GameProgressManager` to `Game Progress Manager` and `Player(Test)`'s `PlayerInventory` to `Player Inventory`.
4. Create a root Cube named `KitchenArrivalTrigger` at `(0, 0.5, 2)`, set its Box Collider to `Is Trigger`, and add `KitchenArrivalTrigger`. Assign the scene `GameProgressManager`.
5. Create a root Cube named `Refrigerator` at `(0, 1, 5)`, set scale to `(2, 2, 1)`, keep its Box Collider non-trigger, and add `InspectInteractable`. Assign the scene `GameProgressManager`.
6. Keep the existing automatic pickup Cube and `Heal_Test` unchanged.
7. Save the scene, let Unity generate `.meta` files for the two new scripts if needed, and verify the Scene view has no missing-script warning.

- [x] **Step 4: Run the scene wiring and full edit-mode tests**

Run:

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.4.11f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\vkccl\Desktop\Github\2026DGJ-2" -runTests -runSynchronously -testPlatform EditMode -testResults "C:\Users\vkccl\Desktop\Github\2026DGJ-2\Logs\AllEditModeTests.xml"
```

Expected: `InteractionSandboxSetupTests` and every existing edit-mode test pass.

- [ ] **Step 5: Manually verify the player flow in InteractionSandbox**

1. Clear the save through the existing Save Debug menu or `PlayerPrefs` before entering Play mode.
2. Start `InteractionSandbox`; enter `KitchenArrivalTrigger` and confirm the state becomes `InspectRefrigerator`.
3. Aim the center of `Player(Test)`'s camera at `Refrigerator` from within 3 units, press F once, and confirm the state becomes `FindFrontDoorKey`.
4. Press F again while still looking at the refrigerator and confirm the state remains `FindFrontDoorKey`.
5. Stop and restart Play mode; confirm the restored state remains `FindFrontDoorKey` and F cannot advance it.
6. Walk into the existing pickup Cube and confirm its automatic pickup behavior is unchanged.

- [x] **Step 6: Commit the sandbox setup and verification test**

```powershell
git add -- Assets/Scenes/Sandbox/InteractionSandbox.unity Assets/Editor/InteractionSandboxSetupTests.cs
git commit -m "Feat: #24 상호작용 샌드박스 구성"
```

## Plan Self-Review

- Spec coverage: Tasks 1–2 cover the required state flow, guarded refrigerator inspection, save version migration, and duplicate-completion prevention. Task 3 covers the required `InteractionSandbox` composition and manual F-key/pickup verification.
- Placeholder scan: the plan contains no incomplete requirements; each code and test change names an exact file, API, and verification command.
- Type consistency: `TryCompleteKitchenArrival`, `TryCompleteRefrigeratorInspection`, `KitchenArrivalTrigger`, `InspectInteractable`, and serialized field name `gameProgressManager` are used consistently across implementation and tests.
