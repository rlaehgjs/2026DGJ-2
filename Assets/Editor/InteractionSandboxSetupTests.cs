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
