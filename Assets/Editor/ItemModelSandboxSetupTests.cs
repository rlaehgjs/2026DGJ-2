using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemModelSandboxSetupTests
{
    private const string ScenePath = "Assets/Scenes/Sandbox/ItemModelSandbox.unity";

    [TestCase("Assets/Prefabs/Interaction/FrontDoorKey.prefab")]
    [TestCase("Assets/Prefabs/Interaction/Nail.prefab")]
    [TestCase("Assets/Prefabs/Interaction/Hammer.prefab")]
    [TestCase("Assets/Prefabs/Interaction/IceCube.prefab")]
    public void CorePickupPrefabs_UseTriggerCollider(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        BoxCollider collider = prefab == null ? null : prefab.GetComponent<BoxCollider>();

        Assert.That(prefab, Is.Not.Null, $"Pickup prefab is required: {prefabPath}");
        Assert.That(collider, Is.Not.Null, $"{prefabPath} requires a BoxCollider on its gameplay root.");
        Assert.That(collider.isTrigger, Is.True, $"{prefabPath} collider must be a trigger for pickup detection.");
    }

    [Test]
    public void ItemModelSandbox_UsesModeledCorePickupsAsGameplayRoots()
    {
        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Assert.That(sceneAsset, Is.Not.Null, "Item model sandbox scene is required.");

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);

        try
        {
            AssertPickup(scene, "FrontDoorKey", "Assets/Data/Items/FrontDoorKey.asset");
            AssertPickup(scene, "Nail", "Assets/Data/Items/Nails.asset");
            AssertNailProgress(scene);
            AssertPickup(scene, "Hammer", "Assets/Data/Items/Hammer.asset");
            AssertHealingPickup(scene, "IceCube");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static void AssertPickup(Scene scene, string objectName, string itemDataPath)
    {
        GameObject pickupObject = FindGameObject(scene, objectName);
        PickupInteractable pickup = pickupObject == null ? null : pickupObject.GetComponent<PickupInteractable>();

        Assert.That(pickupObject, Is.Not.Null, $"{objectName} pickup is required.");
        Assert.That(pickup, Is.Not.Null, $"{objectName} requires PickupInteractable.");
        Assert.That(pickupObject.GetComponent<BoxCollider>(), Is.Not.Null, $"{objectName} requires BoxCollider.");
        Assert.That(pickupObject.GetComponent<BoxCollider>().isTrigger, Is.True, $"{objectName} collider must be a trigger.");
        AssertModelRoot(pickupObject);

        SerializedObject serializedPickup = new SerializedObject(pickup);
        SerializedProperty itemData = serializedPickup.FindProperty("itemData");
        ItemData expectedItemData = AssetDatabase.LoadAssetAtPath<ItemData>(itemDataPath);
        Assert.That(itemData.objectReferenceValue, Is.EqualTo(expectedItemData), $"{objectName} ItemData reference is incorrect.");
    }

    private static void AssertHealingPickup(Scene scene, string objectName)
    {
        GameObject pickupObject = FindGameObject(scene, objectName);
        HealingPickup pickup = pickupObject == null ? null : pickupObject.GetComponent<HealingPickup>();

        Assert.That(pickupObject, Is.Not.Null, "IceCube pickup is required.");
        Assert.That(pickup, Is.Not.Null, "IceCube requires HealingPickup.");
        Assert.That(pickupObject.GetComponent<BoxCollider>(), Is.Not.Null, "IceCube requires BoxCollider.");
        Assert.That(pickupObject.GetComponent<BoxCollider>().isTrigger, Is.True, "IceCube collider must be a trigger.");
        AssertModelRoot(pickupObject);
    }

    private static void AssertNailProgress(Scene scene)
    {
        GameObject nailObject = FindGameObject(scene, "Nail");
        PickupInteractable pickup = nailObject == null ? null : nailObject.GetComponent<PickupInteractable>();
        NailsPickupProgress progress = nailObject == null ? null : nailObject.GetComponent<NailsPickupProgress>();
        GameProgressManager progressManager = FindComponent<GameProgressManager>(scene);

        Assert.That(progress, Is.Not.Null, "Nail requires NailsPickupProgress to advance the quest after collection.");
        Assert.That(progressManager, Is.Not.Null, "Nail requires a scene GameProgressManager.");

        SerializedObject serializedProgress = new SerializedObject(progress);
        Assert.That(serializedProgress.FindProperty("pickupInteractable").objectReferenceValue, Is.EqualTo(pickup),
            "NailsPickupProgress must reference the Nail PickupInteractable.");
        Assert.That(serializedProgress.FindProperty("gameProgressManager").objectReferenceValue, Is.EqualTo(progressManager),
            "NailsPickupProgress must reference the scene GameProgressManager.");
    }

    private static void AssertModelRoot(GameObject pickupObject)
    {
        Assert.That(pickupObject.transform.Find($"{pickupObject.name}Model"), Is.Null,
            $"{pickupObject.name} must use the model itself as the gameplay root, not a separate model child.");
        Assert.That(pickupObject.GetComponentInChildren<Renderer>(true), Is.Not.Null,
            $"{pickupObject.name} requires a renderer on its model hierarchy.");
        Assert.That(pickupObject.GetComponentsInChildren<Camera>(true), Is.Empty,
            $"{pickupObject.name} must not contain a camera.");
        Assert.That(pickupObject.GetComponentsInChildren<Light>(true), Is.Empty,
            $"{pickupObject.name} must not contain a light.");
        Assert.That(pickupObject.GetComponentsInChildren<Rigidbody>(true), Is.Empty,
            $"{pickupObject.name} must not contain a rigidbody.");
        Assert.That(pickupObject.GetComponentsInChildren<Collider>(true), Has.Exactly(1).InstanceOf<BoxCollider>(),
            $"{pickupObject.name} must use one BoxCollider trigger for pickup detection.");
        AssertColliderContainsVisualBounds(pickupObject);
    }

    private static void AssertColliderContainsVisualBounds(GameObject pickupObject)
    {
        BoxCollider collider = pickupObject.GetComponent<BoxCollider>();
        Renderer[] renderers = pickupObject.GetComponentsInChildren<Renderer>(true);
        Bounds visualBounds = renderers[0].bounds;

        for (int index = 1; index < renderers.Length; index++)
        {
            visualBounds.Encapsulate(renderers[index].bounds);
        }

        Assert.That(collider.bounds.Contains(visualBounds.min), Is.True,
            $"{pickupObject.name} collider must include the visual model bounds.");
        Assert.That(collider.bounds.Contains(visualBounds.max), Is.True,
            $"{pickupObject.name} collider must include the visual model bounds.");
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
}
