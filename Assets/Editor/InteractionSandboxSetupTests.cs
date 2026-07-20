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
            InspectInteractable refrigerator = FindComponent<InspectInteractable>(sandboxScene);

            Assert.That(playerInteraction, Is.Not.Null);
            Assert.That(playerInventory, Is.Not.Null);
            Assert.That(playerInteraction.GetComponent<PlayerInventory>(), Is.EqualTo(playerInventory));
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

            InspectInteractable refrigerator = FindComponent<InspectInteractable>(sandboxScene);
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
}
