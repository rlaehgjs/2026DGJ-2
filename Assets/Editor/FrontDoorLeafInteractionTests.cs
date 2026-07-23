using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class FrontDoorLeafInteractionTests
{
    private const string FirstFloorMapPath = "Assets/Prefabs/Environment/Hous_1F_Update2.prefab";
    private readonly List<Object> createdObjects = new List<Object>();

    [TearDown]
    public void TearDown()
    {
        for (int index = createdObjects.Count - 1; index >= 0; index--)
        {
            Object.DestroyImmediate(createdObjects[index]);
        }

        createdObjects.Clear();
    }

    [TestCase("big_door_left")]
    [TestCase("big_door_right")]
    public void FirstFloorMap_FrontDoorLeaf_RoutesToDoorInteractable(string doorLeafName)
    {
        GameObject mapRoot = PrefabUtility.LoadPrefabContents(FirstFloorMapPath);

        try
        {
            Transform doorLeaf = FindDescendant(mapRoot.transform, doorLeafName);

            Assert.That(doorLeaf, Is.Not.Null, $"{doorLeafName} is required in the front door prefab.");

            Collider doorCollider = doorLeaf.GetComponentInChildren<Collider>(true);
            Assert.That(doorCollider, Is.Not.Null, $"{doorLeafName} requires a collider.");

            IInteractable interactable = InvokeFindInteractable(doorCollider);

            Assert.That(interactable, Is.TypeOf<DoorInteractable>(),
                $"{doorLeafName} must select DoorInteractable before the parent FrontDoorLock.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(mapRoot);
        }
    }

    [TestCase("big_door_left")]
    [TestCase("big_door_right")]
    public void FrontDoorLeaf_WithKey_UnlocksAndStartsOpening(string doorLeafName)
    {
        GameObject lockRoot = CreateGameObject("FrontDoor");
        FrontDoorLock frontDoorLock = lockRoot.AddComponent<FrontDoorLock>();

        GameObject doorLeaf = CreateGameObject(doorLeafName);
        doorLeaf.transform.SetParent(lockRoot.transform);
        BoxCollider doorCollider = doorLeaf.AddComponent<BoxCollider>();
        DoorInteractable doorInteractable = doorLeaf.AddComponent<DoorInteractable>();

        GameObject playerObject = CreateGameObject("Player");
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();

        GameObject progressObject = CreateGameObject("GameProgress");
        GameProgressManager progressManager = progressObject.AddComponent<GameProgressManager>();
        ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");

        Assert.That(frontDoorKey, Is.Not.Null);
        SetFrontDoorLockReferences(frontDoorLock, frontDoorKey, progressManager);
        SetDoorReferences(doorInteractable, frontDoorLock, doorCollider);
        Assert.That(inventory.TryAddItem(frontDoorKey, 1), Is.True);
        Assert.That(progressManager.RestoreState(GameProgressState.FindFrontDoorKey), Is.True);

        IInteractable interactable = InvokeFindInteractable(doorCollider);

        Assert.That(interactable, Is.EqualTo(doorInteractable));
        Assert.That(interactable.CanInteract(inventory), Is.True);

        interactable.Interact(inventory);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGenerator));
        Assert.That(frontDoorLock.IsUnlocked, Is.True);
        Assert.That(doorInteractable.IsOpen, Is.True);
    }

    [TestCase("big_door_left", -1f)]
    [TestCase("big_door_right", 1f)]
    public void FrontDoorWallCollider_WithKey_OpensDoorLeafBehindWall(
        string doorLeafName,
        float doorXPosition)
    {
        GameObject lockRoot = CreateGameObject("FrontDoor");
        FrontDoorLock frontDoorLock = lockRoot.AddComponent<FrontDoorLock>();

        GameObject wall = CreateGameObject("wall.001");
        wall.transform.SetParent(lockRoot.transform);
        wall.AddComponent<BoxCollider>().size = new Vector3(4f, 3f, 0.1f);

        GameObject doorLeaf = CreateGameObject(doorLeafName);
        doorLeaf.transform.SetParent(lockRoot.transform);
        doorLeaf.transform.localPosition = new Vector3(doorXPosition, 0f, 1f);
        BoxCollider doorCollider = doorLeaf.AddComponent<BoxCollider>();
        DoorInteractable doorInteractable = doorLeaf.AddComponent<DoorInteractable>();

        GameObject playerObject = CreateGameObject("Player");
        playerObject.transform.position = new Vector3(doorXPosition, 0f, -2f);
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();
        PlayerInteraction playerInteraction = playerObject.AddComponent<PlayerInteraction>();

        GameObject cameraObject = CreateGameObject("InteractionCamera");
        cameraObject.transform.SetParent(playerObject.transform);
        cameraObject.transform.localPosition = Vector3.zero;
        Camera interactionCamera = cameraObject.AddComponent<Camera>();

        GameObject progressObject = CreateGameObject("GameProgress");
        GameProgressManager progressManager = progressObject.AddComponent<GameProgressManager>();
        ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");

        Assert.That(frontDoorKey, Is.Not.Null);
        SetFrontDoorLockReferences(frontDoorLock, frontDoorKey, progressManager);
        SetDoorReferences(doorInteractable, frontDoorLock, doorCollider);
        SetPlayerInteractionReferences(playerInteraction, interactionCamera, inventory);
        Assert.That(inventory.TryAddItem(frontDoorKey, 1), Is.True);
        Assert.That(progressManager.RestoreState(GameProgressState.FindFrontDoorKey), Is.True);
        Physics.SyncTransforms();

        Ray interactionRay = interactionCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Assert.That(Physics.Raycast(interactionRay, out RaycastHit firstHit, 3f), Is.True);
        Assert.That(firstHit.collider.gameObject, Is.EqualTo(wall));

        InvokeTryInteract(playerInteraction);

        Assert.That(progressManager.CurrentState, Is.EqualTo(GameProgressState.FindGenerator));
        Assert.That(doorInteractable.IsOpen, Is.True,
            "The front wall collider must not prevent the selected door leaf from opening.");
    }

    [Test]
    public void FrontDoorLock_Unlocking_DisablesWallColliderButKeepsDoorCollider()
    {
        GameObject lockRoot = CreateGameObject("FrontDoor");
        FrontDoorLock frontDoorLock = lockRoot.AddComponent<FrontDoorLock>();

        GameObject wall = CreateGameObject("wall.001");
        wall.transform.SetParent(lockRoot.transform);
        BoxCollider wallCollider = wall.AddComponent<BoxCollider>();

        GameObject doorLeaf = CreateGameObject("big_door_left");
        doorLeaf.transform.SetParent(lockRoot.transform);
        BoxCollider doorCollider = doorLeaf.AddComponent<BoxCollider>();
        DoorInteractable doorInteractable = doorLeaf.AddComponent<DoorInteractable>();

        GameObject playerObject = CreateGameObject("Player");
        PlayerInventory inventory = playerObject.AddComponent<PlayerInventory>();

        GameObject progressObject = CreateGameObject("GameProgress");
        GameProgressManager progressManager = progressObject.AddComponent<GameProgressManager>();
        ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");

        Assert.That(frontDoorKey, Is.Not.Null);
        SetFrontDoorLockReferences(frontDoorLock, frontDoorKey, progressManager);
        SetDoorReferences(doorInteractable, frontDoorLock, doorCollider);
        Assert.That(inventory.TryAddItem(frontDoorKey, 1), Is.True);
        Assert.That(progressManager.RestoreState(GameProgressState.FindFrontDoorKey), Is.True);

        doorInteractable.Interact(inventory);

        Assert.That(wallCollider.enabled, Is.False);
        Assert.That(doorCollider.enabled, Is.True);
    }

    private GameObject CreateGameObject(string objectName)
    {
        GameObject gameObject = new GameObject(objectName);
        createdObjects.Add(gameObject);
        return gameObject;
    }

    private static Transform FindDescendant(Transform root, string objectName)
    {
        foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == objectName)
            {
                return transform;
            }
        }

        return null;
    }

    private static IInteractable InvokeFindInteractable(Collider collider)
    {
        MethodInfo findInteractable = typeof(PlayerInteraction).GetMethod(
            "FindInteractable",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(findInteractable, Is.Not.Null);
        return (IInteractable)findInteractable.Invoke(null, new object[] { collider });
    }

    private static void InvokeTryInteract(PlayerInteraction playerInteraction)
    {
        MethodInfo tryInteract = typeof(PlayerInteraction).GetMethod(
            "TryInteract",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(tryInteract, Is.Not.Null);
        tryInteract.Invoke(playerInteraction, null);
    }

    private static void SetFrontDoorLockReferences(
        FrontDoorLock frontDoorLock,
        ItemData frontDoorKey,
        GameProgressManager progressManager)
    {
        SerializedObject serializedLock = new SerializedObject(frontDoorLock);
        serializedLock.FindProperty("requiredKey").objectReferenceValue = frontDoorKey;
        serializedLock.FindProperty("gameProgressManager").objectReferenceValue = progressManager;
        serializedLock.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetDoorReferences(
        DoorInteractable doorInteractable,
        FrontDoorLock frontDoorLock,
        Collider doorCollider)
    {
        SerializedObject serializedDoor = new SerializedObject(doorInteractable);
        serializedDoor.FindProperty("frontDoorLock").objectReferenceValue = frontDoorLock;
        serializedDoor.FindProperty("doorCollider").objectReferenceValue = doorCollider;
        serializedDoor.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetPlayerInteractionReferences(
        PlayerInteraction playerInteraction,
        Camera interactionCamera,
        PlayerInventory inventory)
    {
        SerializedObject serializedPlayerInteraction = new SerializedObject(playerInteraction);
        serializedPlayerInteraction.FindProperty("interactionCamera").objectReferenceValue = interactionCamera;
        serializedPlayerInteraction.ApplyModifiedPropertiesWithoutUndo();

        FieldInfo playerInventoryField = typeof(PlayerInteraction).GetField(
            "playerInventory",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(playerInventoryField, Is.Not.Null);
        playerInventoryField.SetValue(playerInteraction, inventory);
    }
}
