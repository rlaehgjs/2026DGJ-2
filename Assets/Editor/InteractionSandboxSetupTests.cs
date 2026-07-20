using System.Reflection;
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
            RefrigeratorInspectInteractable refrigerator = FindComponent<RefrigeratorInspectInteractable>(sandboxScene);
            RefrigeratorWallRepairInteractable refrigeratorWall = FindComponent<RefrigeratorWallRepairInteractable>(sandboxScene);
            GeneratorInteractable generator = FindComponent<GeneratorInteractable>(sandboxScene);
            GameObject generatorWireObject = FindGameObject(sandboxScene, "GeneratorWire");
            PickupInteractable generatorWire = generatorWireObject == null
                ? null
                : generatorWireObject.GetComponent<PickupInteractable>();
            GeneratorWirePickupProgress generatorWireProgress = generatorWireObject == null
                ? null
                : generatorWireObject.GetComponent<GeneratorWirePickupProgress>();
            GameObject nailsObject = FindGameObject(sandboxScene, "Nails");
            PickupInteractable nailsPickup = nailsObject == null
                ? null
                : nailsObject.GetComponent<PickupInteractable>();
            NailsPickupProgress nailsProgress = nailsObject == null
                ? null
                : nailsObject.GetComponent<NailsPickupProgress>();
            GameObject hammerObject = FindGameObject(sandboxScene, "Hammer");
            PickupInteractable hammerPickup = hammerObject == null
                ? null
                : hammerObject.GetComponent<PickupInteractable>();
            HammerPickupProgress hammerProgress = hammerObject == null
                ? null
                : hammerObject.GetComponent<HammerPickupProgress>();
            GameObject frontDoorKeyObject = FindGameObject(sandboxScene, "FrontDoorKey");
            PickupInteractable frontDoorKey = frontDoorKeyObject == null
                ? null
                : frontDoorKeyObject.GetComponent<PickupInteractable>();
            FrontDoorLock frontDoorLock = FindComponent<FrontDoorLock>(sandboxScene);
            ItemData generatorWireData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/GeneratorWire.asset");
            ItemData nailsData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Nails.asset");
            ItemData hammerData = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Hammer.asset");

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(playerInteraction.GetComponent<PlayerInventory>(), Is.EqualTo(playerInventory));
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(saveManager, Is.Not.Null);
            Assert.That(kitchenTrigger, Is.Not.Null);
            Assert.That(refrigerator, Is.Not.Null);
            Assert.That(refrigeratorWall, Is.Not.Null);
            Assert.That(generator, Is.Not.Null);
            Assert.That(generatorWire, Is.Not.Null);
            Assert.That(generatorWireProgress, Is.Not.Null);
            Assert.That(nailsPickup, Is.Not.Null);
            Assert.That(nailsProgress, Is.Not.Null);
            Assert.That(hammerPickup, Is.Not.Null);
            Assert.That(hammerProgress, Is.Not.Null);
            Assert.That(frontDoorKey, Is.Not.Null);
            Assert.That(frontDoorLock, Is.Not.Null);
            Assert.That(generatorWireData, Is.Not.Null);
            Assert.That(nailsData, Is.Not.Null);
            Assert.That(hammerData, Is.Not.Null);

            BoxCollider triggerCollider = kitchenTrigger.GetComponent<BoxCollider>();
            BoxCollider refrigeratorCollider = refrigerator.GetComponent<BoxCollider>();
            BoxCollider refrigeratorWallCollider = refrigeratorWall.GetComponent<BoxCollider>();
            BoxCollider generatorCollider = generator.GetComponent<BoxCollider>();
            BoxCollider generatorWireCollider = generatorWire.GetComponent<BoxCollider>();
            BoxCollider nailsCollider = nailsPickup.GetComponent<BoxCollider>();
            BoxCollider hammerCollider = hammerPickup.GetComponent<BoxCollider>();
            BoxCollider frontDoorKeyCollider = frontDoorKey.GetComponent<BoxCollider>();
            Assert.That(triggerCollider, Is.Not.Null);
            Assert.That(triggerCollider.isTrigger, Is.True);
            Assert.That(refrigeratorCollider, Is.Not.Null);
            Assert.That(refrigeratorCollider.isTrigger, Is.False);
            Assert.That(refrigeratorWallCollider, Is.Not.Null);
            Assert.That(refrigeratorWallCollider.isTrigger, Is.False);
            Assert.That(generatorCollider, Is.Not.Null);
            Assert.That(generatorCollider.isTrigger, Is.False);
            Assert.That(generatorWireCollider, Is.Not.Null);
            Assert.That(generatorWireCollider.isTrigger, Is.True);
            Assert.That(nailsCollider, Is.Not.Null);
            Assert.That(nailsCollider.isTrigger, Is.True);
            Assert.That(hammerCollider, Is.Not.Null);
            Assert.That(hammerCollider.isTrigger, Is.True);
            Assert.That(frontDoorKeyCollider, Is.Not.Null);
            Assert.That(frontDoorKeyCollider.isTrigger, Is.True);

            Assert.That(GetObjectReference(kitchenTrigger, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(refrigerator, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(refrigeratorWall, "requiredNails"), Is.Not.Null);
            Assert.That(GetObjectReference(refrigeratorWall, "requiredHammer"), Is.Not.Null);
            Assert.That(GetObjectReference(refrigeratorWall, "interactionCollider"), Is.EqualTo(refrigeratorWallCollider));
            Assert.That(GetObjectReference(refrigeratorWall, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(generator, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(generator, "requiredWire"), Is.Not.Null);
            Assert.That(GetObjectReference(generatorWire, "saveManager"), Is.EqualTo(saveManager));
            Assert.That(GetBoolValue(generatorWire, "requiresProgressState"), Is.True);
            Assert.That(GetEnumValue(generatorWire, "requiredProgressState"), Is.EqualTo((int)GameProgressState.FindGeneratorWire));
            Assert.That(GetObjectReference(generatorWire, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(generatorWireProgress, "pickupInteractable"), Is.EqualTo(generatorWire));
            Assert.That(GetObjectReference(generatorWireProgress, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(nailsPickup, "saveManager"), Is.EqualTo(saveManager));
            Assert.That(GetBoolValue(nailsPickup, "requiresProgressState"), Is.True);
            Assert.That(GetEnumValue(nailsPickup, "requiredProgressState"), Is.EqualTo((int)GameProgressState.FindNails));
            Assert.That(GetObjectReference(nailsPickup, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(nailsProgress, "pickupInteractable"), Is.EqualTo(nailsPickup));
            Assert.That(GetObjectReference(nailsProgress, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(hammerPickup, "saveManager"), Is.EqualTo(saveManager));
            Assert.That(GetBoolValue(hammerPickup, "requiresProgressState"), Is.True);
            Assert.That(GetEnumValue(hammerPickup, "requiredProgressState"), Is.EqualTo((int)GameProgressState.FindHammer));
            Assert.That(GetObjectReference(hammerPickup, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(hammerProgress, "pickupInteractable"), Is.EqualTo(hammerPickup));
            Assert.That(GetObjectReference(hammerProgress, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(saveManager, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(saveManager, "playerInventory"), Is.EqualTo(playerInventory));
            Assert.That(GetObjectReference(frontDoorKey, "saveManager"), Is.EqualTo(saveManager));
            Assert.That(GetBoolValue(frontDoorKey, "requiresProgressState"), Is.True);
            Assert.That(GetEnumValue(frontDoorKey, "requiredProgressState"), Is.EqualTo((int)GameProgressState.FindFrontDoorKey));
            Assert.That(GetObjectReference(frontDoorKey, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(GetObjectReference(frontDoorLock, "requiredKey"), Is.Not.Null);
            Assert.That(GetObjectReference(frontDoorLock, "gameProgressManager"), Is.EqualTo(progressManager));
            Assert.That(InventoryCatalogContains(playerInventory, generatorWireData), Is.True);
            Assert.That(InventoryCatalogContains(playerInventory, nailsData), Is.True);
            Assert.That(InventoryCatalogContains(playerInventory, hammerData), Is.True);
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_RefrigeratorRaycast_AdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            Camera interactionCamera = playerInteraction.GetComponentInChildren<Camera>();

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(interactionCamera, Is.Not.Null);

            RefrigeratorInspectInteractable refrigerator = FindComponent<RefrigeratorInspectInteractable>(sandboxScene);
            Assert.That(refrigerator, Is.Not.Null);

            Vector3 cameraOffset = interactionCamera.transform.position - playerInteraction.transform.position;
            playerInteraction.transform.position = refrigerator.transform.position
                - cameraOffset
                - refrigerator.transform.forward * 2f;
            SetPlayerInteractionReferences(playerInteraction, interactionCamera, playerInventory);
            progressManager.RestoreState(GameProgressState.FindKitchen);
            Assert.That(progressManager.TryCompleteKitchenArrival(), Is.True);
            Physics.SyncTransforms();

            MethodInfo tryInteractMethod = typeof(PlayerInteraction).GetMethod(
                "TryInteract",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(tryInteractMethod, Is.Not.Null);
            tryInteractMethod.Invoke(playerInteraction, null);

            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_GeneratorRaycast_AdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GeneratorInteractable generator = FindComponent<GeneratorInteractable>(sandboxScene);
            Camera interactionCamera = playerInteraction.GetComponentInChildren<Camera>();

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(generator, Is.Not.Null);
            Assert.That(interactionCamera, Is.Not.Null);

            Vector3 cameraOffset = interactionCamera.transform.position - playerInteraction.transform.position;
            playerInteraction.transform.position = generator.transform.position
                - cameraOffset
                - generator.transform.forward * 2f;
            SetPlayerInteractionReferences(playerInteraction, interactionCamera, playerInventory);
            progressManager.RestoreState(GameProgressState.FindGenerator);
            Physics.SyncTransforms();

            MethodInfo tryInteractMethod = typeof(PlayerInteraction).GetMethod(
                "TryInteract",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(tryInteractMethod, Is.Not.Null);
            tryInteractMethod.Invoke(playerInteraction, null);

            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGeneratorWire));
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_GeneratorWirePickup_AdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GameObject generatorWireObject = FindGameObject(sandboxScene, "GeneratorWire");
            PickupInteractable generatorWire = generatorWireObject == null
                ? null
                : generatorWireObject.GetComponent<PickupInteractable>();
            GeneratorWirePickupProgress generatorWireProgress = generatorWireObject == null
                ? null
                : generatorWireObject.GetComponent<GeneratorWirePickupProgress>();
            BoxCollider playerCollider = playerInventory.GetComponentInChildren<BoxCollider>();

            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(generatorWire, Is.Not.Null);
            Assert.That(generatorWireProgress, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);

            generatorWireProgress.enabled = false;
            generatorWireProgress.enabled = true;
            Assert.That(generatorWire.enabled, Is.False);

            progressManager.RestoreState(GameProgressState.FindGeneratorWire);

            Assert.That(generatorWire.enabled, Is.True);
            InvokeTriggerEnter(generatorWire, playerCollider);

            Assert.That(playerInventory.HasItem("generator_wire", 1), Is.True);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.RepairGenerator));
            Assert.That(generatorWireObject.activeSelf, Is.False);
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_NailsPickup_AdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GameObject nailsObject = FindGameObject(sandboxScene, "Nails");
            PickupInteractable nailsPickup = nailsObject == null
                ? null
                : nailsObject.GetComponent<PickupInteractable>();
            NailsPickupProgress nailsProgress = nailsObject == null
                ? null
                : nailsObject.GetComponent<NailsPickupProgress>();
            BoxCollider playerCollider = playerInventory.GetComponentInChildren<BoxCollider>();

            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(nailsPickup, Is.Not.Null);
            Assert.That(nailsProgress, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);

            nailsProgress.enabled = false;
            nailsProgress.enabled = true;
            Assert.That(nailsPickup.enabled, Is.False);

            progressManager.RestoreState(GameProgressState.FindNails);

            Assert.That(nailsPickup.enabled, Is.True);
            InvokeTriggerEnter(nailsPickup, playerCollider);

            Assert.That(playerInventory.HasItem("nails", 1), Is.True);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindHammer));
            Assert.That(nailsObject.activeSelf, Is.False);
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_HammerPickup_AdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GameObject hammerObject = FindGameObject(sandboxScene, "Hammer");
            PickupInteractable hammerPickup = hammerObject == null
                ? null
                : hammerObject.GetComponent<PickupInteractable>();
            HammerPickupProgress hammerProgress = hammerObject == null
                ? null
                : hammerObject.GetComponent<HammerPickupProgress>();
            BoxCollider playerCollider = playerInventory.GetComponentInChildren<BoxCollider>();

            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(hammerPickup, Is.Not.Null);
            Assert.That(hammerProgress, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);

            hammerProgress.enabled = false;
            hammerProgress.enabled = true;
            Assert.That(hammerPickup.enabled, Is.False);

            progressManager.RestoreState(GameProgressState.FindHammer);

            Assert.That(hammerPickup.enabled, Is.True);
            InvokeTriggerEnter(hammerPickup, playerCollider);

            Assert.That(playerInventory.HasItem("hammer", 1), Is.True);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.RepairRefrigeratorWall));
            Assert.That(hammerObject.activeSelf, Is.False);
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_RefrigeratorWallRepairRaycast_ConsumesMaterialsAndAdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            RefrigeratorWallRepairInteractable refrigeratorWall = FindComponent<RefrigeratorWallRepairInteractable>(sandboxScene);
            Camera interactionCamera = playerInteraction.GetComponentInChildren<Camera>();
            ItemData nails = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Nails.asset");
            ItemData hammer = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/Hammer.asset");

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(refrigeratorWall, Is.Not.Null);
            Assert.That(interactionCamera, Is.Not.Null);
            Assert.That(nails, Is.Not.Null);
            Assert.That(hammer, Is.Not.Null);
            Assert.That(playerInventory.TryAddItem(nails, 1), Is.True);
            Assert.That(playerInventory.TryAddItem(hammer, 1), Is.True);

            Vector3 cameraOffset = interactionCamera.transform.position - playerInteraction.transform.position;
            playerInteraction.transform.position = refrigeratorWall.transform.position
                - cameraOffset
                - refrigeratorWall.transform.forward * 2f;
            SetPlayerInteractionReferences(playerInteraction, interactionCamera, playerInventory);
            progressManager.RestoreState(GameProgressState.RepairRefrigeratorWall);
            Physics.SyncTransforms();

            MethodInfo tryInteractMethod = typeof(PlayerInteraction).GetMethod(
                "TryInteract",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(tryInteractMethod, Is.Not.Null);
            tryInteractMethod.Invoke(playerInteraction, null);

            Assert.That(playerInventory.HasItem("nails", 1), Is.False);
            Assert.That(playerInventory.HasItem("hammer", 1), Is.False);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindCoolantCapsule));
            Assert.That(refrigeratorWall.GetComponent<BoxCollider>().enabled, Is.False);
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_GeneratorRepairRaycast_ConsumesWireAndAdvancesMission()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GeneratorInteractable generator = FindComponent<GeneratorInteractable>(sandboxScene);
            Camera interactionCamera = playerInteraction.GetComponentInChildren<Camera>();
            ItemData generatorWire = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/GeneratorWire.asset");

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(generator, Is.Not.Null);
            Assert.That(interactionCamera, Is.Not.Null);
            Assert.That(generatorWire, Is.Not.Null);
            Assert.That(playerInventory.TryAddItem(generatorWire, 1), Is.True);

            Vector3 cameraOffset = interactionCamera.transform.position - playerInteraction.transform.position;
            playerInteraction.transform.position = generator.transform.position
                - cameraOffset
                - generator.transform.forward * 2f;
            SetPlayerInteractionReferences(playerInteraction, interactionCamera, playerInventory);
            progressManager.RestoreState(GameProgressState.RepairGenerator);
            Physics.SyncTransforms();

            MethodInfo tryInteractMethod = typeof(PlayerInteraction).GetMethod(
                "TryInteract",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(tryInteractMethod, Is.Not.Null);
            tryInteractMethod.Invoke(playerInteraction, null);

            Assert.That(playerInventory.HasItem("generator_wire", 1), Is.False);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindNails));
        }
        finally
        {
            EditorSceneManager.CloseScene(sandboxScene, true);
        }
    }

    [Test]
    public void InteractionSandbox_FrontDoorInteraction_RequiresKeyAndUnlocksDoor()
    {
        Scene sandboxScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            PlayerInteraction playerInteraction = FindComponent<PlayerInteraction>(sandboxScene);
            PlayerInventory playerInventory = FindComponent<PlayerInventory>(sandboxScene);
            GameProgressManager progressManager = FindComponent<GameProgressManager>(sandboxScene);
            GameObject frontDoorKeyObject = FindGameObject(sandboxScene, "FrontDoorKey");
            PickupInteractable frontDoorKey = frontDoorKeyObject == null
                ? null
                : frontDoorKeyObject.GetComponent<PickupInteractable>();
            FrontDoorLock frontDoorLock = FindComponent<FrontDoorLock>(sandboxScene);
            Camera interactionCamera = playerInteraction.GetComponentInChildren<Camera>();
            BoxCollider playerCollider = playerInventory.GetComponentInChildren<BoxCollider>();

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(progressManager, Is.Not.Null);
            Assert.That(frontDoorKey, Is.Not.Null);
            Assert.That(frontDoorLock, Is.Not.Null);
            Assert.That(interactionCamera, Is.Not.Null);
            Assert.That(playerCollider, Is.Not.Null);

            InvokeTriggerEnter(frontDoorKey, playerCollider);

            Assert.That(playerInventory.HasItem("FrontDoor_key", 1), Is.False);
            Assert.That(frontDoorKey.gameObject.activeSelf, Is.True);

            progressManager.RestoreState(GameProgressState.FindFrontDoorKey);
            InvokeTriggerEnter(frontDoorKey, playerCollider);

            Assert.That(playerInventory.HasItem("FrontDoor_key", 1), Is.True);
            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindFrontDoorKey));
            Assert.That(frontDoorKey.gameObject.activeSelf, Is.False);

            Vector3 cameraOffset = interactionCamera.transform.position - playerInteraction.transform.position;
            playerInteraction.transform.position = frontDoorLock.transform.position
                - cameraOffset
                - frontDoorLock.transform.forward * 2f;
            SetPlayerInteractionReferences(playerInteraction, interactionCamera, playerInventory);
            Physics.SyncTransforms();

            MethodInfo tryInteractMethod = typeof(PlayerInteraction).GetMethod(
                "TryInteract",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(tryInteractMethod, Is.Not.Null);
            tryInteractMethod.Invoke(playerInteraction, null);

            Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGenerator));
            Assert.That(frontDoorLock.IsUnlocked, Is.True);
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

    private static GameObject FindGameObject(Scene scene, string objectName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Transform transform in rootObject.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == objectName)
                {
                    return transform.gameObject;
                }
            }
        }

        return null;
    }

    private static Object GetObjectReference(Object component, string propertyName)
    {
        return new SerializedObject(component).FindProperty(propertyName).objectReferenceValue;
    }

    private static bool GetBoolValue(Object component, string propertyName)
    {
        return new SerializedObject(component).FindProperty(propertyName).boolValue;
    }

    private static int GetEnumValue(Object component, string propertyName)
    {
        return new SerializedObject(component).FindProperty(propertyName).enumValueIndex;
    }

    private static bool InventoryCatalogContains(PlayerInventory inventory, ItemData itemData)
    {
        SerializedProperty itemCatalog = new SerializedObject(inventory).FindProperty("itemCatalog");

        for (int index = 0; index < itemCatalog.arraySize; index++)
        {
            if (itemCatalog.GetArrayElementAtIndex(index).objectReferenceValue == itemData)
            {
                return true;
            }
        }

        return false;
    }

    private static void SetPlayerInteractionReferences(
        PlayerInteraction playerInteraction,
        Camera interactionCamera,
        PlayerInventory playerInventory)
    {
        SerializedObject serializedInteraction = new SerializedObject(playerInteraction);
        serializedInteraction.FindProperty("interactionCamera").objectReferenceValue = interactionCamera;
        serializedInteraction.ApplyModifiedPropertiesWithoutUndo();

        FieldInfo inventoryField = typeof(PlayerInteraction).GetField(
            "playerInventory",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(inventoryField, Is.Not.Null);
        inventoryField.SetValue(playerInteraction, playerInventory);
    }

    private static void InvokeTriggerEnter(Component component, Collider playerCollider)
    {
        MethodInfo triggerEnterMethod = component.GetType().GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null);
        triggerEnterMethod.Invoke(component, new object[] { playerCollider });
    }
}
