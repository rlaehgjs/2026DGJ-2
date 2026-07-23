using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirstFloorInteractionAndItemSetupTests
{
    private const string FirstFloorMapPath = "Assets/Prefabs/Environment/Hous_1F_Update2.prefab";
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";

    [Test]
    public void FirstFloorMap_ContainsConfiguredDoorsAndDrawers()
    {
        GameObject mapRoot = PrefabUtility.LoadPrefabContents(FirstFloorMapPath);

        try
        {
            DoorInteractable[] doors = mapRoot.GetComponentsInChildren<DoorInteractable>(true);
            DrawerInteractable[] drawers = mapRoot.GetComponentsInChildren<DrawerInteractable>(true);

            Assert.That(doors, Has.Length.EqualTo(15), "The first floor requires 15 rotating doors.");
            Assert.That(drawers, Has.Length.EqualTo(12), "The first floor requires 12 sliding drawers.");
            AssertHingedDoor(mapRoot.transform, "Toilet/(Prb)Door/door/DoorHinge");
            AssertHingedDoor(mapRoot.transform, "Room_1/(Prb)Door (1)/door/DoorHinge");

            foreach (DoorInteractable door in doors)
            {
                AssertMovableHierarchy(door.transform, "DoorInteractable");
                Assert.That(door.GetComponentInChildren<Collider>(true), Is.Not.Null,
                    "Each rotating door requires a collider that rotates with its visual model.");
            }

            foreach (DrawerInteractable drawer in drawers)
            {
                AssertMovableHierarchy(drawer.transform, "DrawerInteractable");
                Assert.That(drawer.GetComponentInChildren<Collider>(true), Is.Not.Null,
                    "Each sliding drawer requires a collider that moves with its visual model.");
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(mapRoot);
        }
    }

    [Test]
    public void MainGameScene_ConnectsFrontDoorLockToGameProgress()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Additive);

        try
        {
            FrontDoorLock frontDoorLock = FindComponent<FrontDoorLock>(scene);
            GameProgressManager gameProgressManager = FindComponent<GameProgressManager>(scene);
            ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");

            Assert.That(frontDoorLock, Is.Not.Null, "The front door requires FrontDoorLock.");
            Assert.That(gameProgressManager, Is.Not.Null, "MainGameScene requires GameProgressManager.");

            SerializedObject serializedLock = new SerializedObject(frontDoorLock);
            Assert.That(serializedLock.FindProperty("requiredKey").objectReferenceValue, Is.EqualTo(frontDoorKey),
                "FrontDoorLock must use FrontDoorKey ItemData.");
            Assert.That(serializedLock.FindProperty("gameProgressManager").objectReferenceValue, Is.EqualTo(gameProgressManager),
                "FrontDoorLock must reference the MainGameScene GameProgressManager.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void MainGameScene_FrontDoorKeyPickup_UsesActiveJumpscareController()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Additive);

        try
        {
            ItemData frontDoorKey = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/Data/Items/FrontDoorKey.asset");
            GameObject frontDoorKeyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Interaction/FrontDoorKey.prefab");
            PickupInteractable keyPickup = FindPickupForItem(scene, frontDoorKey);
            Component controller = FindComponentByClassName(scene, "PickupJumpscareController");

            Assert.That(frontDoorKeyPrefab.GetComponent<BathroomJumpscare>(), Is.Null,
                "FrontDoorKey is a pickup and must not run a trigger-based jumpscare itself.");
            Assert.That(keyPickup, Is.Not.Null, "MainGameScene requires the FrontDoorKey pickup.");
            Assert.That(keyPickup.GetComponent<BathroomJumpscare>(), Is.Null,
                "The placed FrontDoorKey must not keep the trigger-based jumpscare component as an override.");
            Assert.That(controller, Is.Not.Null, "MainGameScene requires an active PickupJumpscareController.");
            Assert.That(controller.gameObject.activeInHierarchy, Is.True,
                "PickupJumpscareController must remain active after FrontDoorKey is collected.");

            SerializedObject serializedController = new SerializedObject(controller);
            Assert.That(serializedController.FindProperty("pickupInteractable").objectReferenceValue, Is.EqualTo(keyPickup),
                "PickupJumpscareController must listen to the placed FrontDoorKey pickup.");
            Assert.That(serializedController.FindProperty("ghostCanvasImage").objectReferenceValue, Is.Not.Null,
                "PickupJumpscareController requires the existing GhostImage object.");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [Test]
    public void MainGameScene_RefrigeratorWallRepair_UsesRestoredDoorPosition()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Additive);

        try
        {
            RefrigeratorWallRepairInteractable repair = FindComponent<RefrigeratorWallRepairInteractable>(scene);

            Assert.That(repair, Is.Not.Null, "MainGameScene requires RefrigeratorWallRepairInteractable.");

            SerializedObject serializedRepair = new SerializedObject(repair);
            SerializedProperty repairedDoor = serializedRepair.FindProperty("repairedDoor");
            SerializedProperty repairedLocalPosition = serializedRepair.FindProperty("repairedLocalPosition");

            Assert.That(repairedDoor, Is.Not.Null,
                "RefrigeratorWallRepairInteractable must expose the repairedDoor reference.");
            Assert.That(repairedLocalPosition, Is.Not.Null,
                "RefrigeratorWallRepairInteractable must expose the repairedLocalPosition value.");
            Assert.That(repairedDoor.objectReferenceValue, Is.Not.Null,
                "RefrigeratorWallRepairInteractable must reference Cube.004.");
            Assert.That(repairedDoor.objectReferenceValue.name, Is.EqualTo("Cube.004"));
            Assert.That(repairedLocalPosition.vector3Value, Is.EqualTo(new Vector3(-0.59819299f, 1.71812499f, 0.365508914f)));
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    [TestCase("Assets/Prefabs/Interaction/Nail.prefab", "Assets/Data/Items/Nails.asset", typeof(PickupInteractable))]
    [TestCase("Assets/Prefabs/Interaction/FrontDoorKey.prefab", "Assets/Data/Items/FrontDoorKey.asset", typeof(PickupInteractable))]
    [TestCase("Assets/Prefabs/Interaction/Hammer.prefab", "Assets/Data/Items/Hammer.asset", typeof(PickupInteractable))]
    [TestCase("Assets/Prefabs/Interaction/IceCube.prefab", null, typeof(HealingPickup))]
    public void CoreItemPrefab_IsModeledAndCollectible(string prefabPath, string itemDataPath, System.Type interactionType)
    {
        GameObject itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        Assert.That(itemPrefab, Is.Not.Null, $"{prefabPath} is required.");
        Assert.That(itemPrefab.isStatic, Is.False, $"{itemPrefab.name} must remain movable for pickup.");
        Assert.That(itemPrefab.GetComponent(interactionType), Is.Not.Null,
            $"{itemPrefab.name} requires {interactionType.Name}.");

        BoxCollider triggerCollider = itemPrefab.GetComponent<BoxCollider>();
        Assert.That(triggerCollider, Is.Not.Null, $"{itemPrefab.name} requires a BoxCollider.");
        Assert.That(triggerCollider.isTrigger, Is.True, $"{itemPrefab.name} collider must be a trigger.");
        Assert.That(itemPrefab.GetComponentInChildren<Renderer>(true), Is.Not.Null,
            $"{itemPrefab.name} requires a visible model.");
        Assert.That(itemPrefab.GetComponentsInChildren<Camera>(true), Is.Empty,
            $"{itemPrefab.name} must not contain a model preview camera.");
        Assert.That(itemPrefab.GetComponentsInChildren<Light>(true), Is.Empty,
            $"{itemPrefab.name} must not contain a model preview light.");
        Assert.That(itemPrefab.GetComponentsInChildren<Rigidbody>(true), Is.Empty,
            $"{itemPrefab.name} must not contain a model preview rigidbody.");

        if (itemPrefab.name == "Nail")
        {
            AssertProgressPickup<NailsPickupProgress>(itemPrefab, "NailsPickupProgress");
        }

        if (itemPrefab.name == "Hammer")
        {
            AssertProgressPickup<HammerPickupProgress>(itemPrefab, "HammerPickupProgress");
        }

        if (itemDataPath == null)
        {
            return;
        }

        PickupInteractable pickup = itemPrefab.GetComponent<PickupInteractable>();
        SerializedObject serializedPickup = new SerializedObject(pickup);
        ItemData expectedItemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath);

        Assert.That(serializedPickup.FindProperty("itemData").objectReferenceValue, Is.EqualTo(expectedItemData),
            $"{itemPrefab.name} ItemData reference is incorrect.");
    }

    private static void AssertMovableHierarchy(Transform root, string interactionName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            Assert.That(child.gameObject.isStatic, Is.False,
                $"{interactionName} hierarchy contains static object {child.name}, so its visual or collider can remain behind when opened.");
        }
    }

    private static void AssertHingedDoor(Transform mapRoot, string hingePath)
    {
        Transform hinge = mapRoot.Find(hingePath);

        Assert.That(hinge, Is.Not.Null, $"{hingePath} is required so the door rotates around its hinge.");
        Assert.That(hinge.GetComponent<DoorInteractable>(), Is.Not.Null,
            $"{hingePath} must own DoorInteractable.");
        Assert.That(hinge.Find("door_body")?.GetComponent<Collider>(), Is.Not.Null,
            $"{hingePath} door_body requires a collider that follows the hinge.");
    }

    private static void AssertProgressPickup<T>(GameObject itemPrefab, string componentName) where T : Component
    {
        T progressComponent = itemPrefab.GetComponent<T>();

        Assert.That(progressComponent, Is.Not.Null, $"{itemPrefab.name} requires {componentName}.");

        SerializedObject serializedProgress = new SerializedObject(progressComponent);
        Assert.That(serializedProgress.FindProperty("pickupInteractable").objectReferenceValue,
            Is.EqualTo(itemPrefab.GetComponent<PickupInteractable>()),
            $"{itemPrefab.name} {componentName} must reference its PickupInteractable.");
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

    private static PickupInteractable FindPickupForItem(Scene scene, ItemData itemData)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (PickupInteractable pickup in rootObject.GetComponentsInChildren<PickupInteractable>(true))
            {
                SerializedObject serializedPickup = new SerializedObject(pickup);

                if (serializedPickup.FindProperty("itemData").objectReferenceValue == itemData)
                {
                    return pickup;
                }
            }
        }

        return null;
    }

    private static Component FindComponentByClassName(Scene scene, string className)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (MonoBehaviour behaviour in rootObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && behaviour.GetType().Name == className)
                {
                    return behaviour;
                }
            }
        }

        return null;
    }
}
