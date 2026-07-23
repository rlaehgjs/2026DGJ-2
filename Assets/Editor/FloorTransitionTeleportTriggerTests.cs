using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorTransitionTeleportTriggerTests
{
    private const string TriggerTypeName = "FloorTransitionTeleportTrigger";
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";

    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject createdObject in createdObjects)
        {
            UnityEngine.Object.DestroyImmediate(createdObject);
        }

        createdObjects.Clear();
    }

    [Test]
    public void TriggerEnter_PlayerChildCollider_MovesPlayerAndClearsVelocity()
    {
        GameObject triggerObject = CreateObject("FloorTransitionTrigger");
        GameObject destinationObject = CreateObject("Destination");
        destinationObject.transform.position = new Vector3(10f, 2f, 5f);
        Component trigger = CreateTeleportTrigger(triggerObject, destinationObject.transform);

        Rigidbody playerBody = CreatePlayerWithChildCollider(out BoxCollider playerChildCollider);
        playerBody.linearVelocity = new Vector3(1f, 2f, 3f);
        playerBody.angularVelocity = new Vector3(4f, 5f, 6f);

        InvokeTriggerEnter(trigger, playerChildCollider);

        Assert.That(playerBody.position, Is.EqualTo(destinationObject.transform.position));
        Assert.That(playerBody.linearVelocity, Is.EqualTo(Vector3.zero));
        Assert.That(playerBody.angularVelocity, Is.EqualTo(Vector3.zero));
    }

    [Test]
    public void TriggerEnter_ArrivalTrigger_DoesNotImmediatelyTeleportPlayerBack()
    {
        GameObject departureTriggerObject = CreateObject("DepartureTrigger");
        GameObject arrivalTriggerObject = CreateObject("ArrivalTrigger");
        GameObject departurePoint = CreateObject("DeparturePoint");
        GameObject arrivalPoint = CreateObject("ArrivalPoint");
        departurePoint.transform.position = new Vector3(-4f, 1f, 0f);
        arrivalPoint.transform.position = new Vector3(4f, 1f, 0f);

        Component departureTrigger = CreateTeleportTrigger(departureTriggerObject, arrivalPoint.transform);
        Component arrivalTrigger = CreateTeleportTrigger(arrivalTriggerObject, departurePoint.transform);
        Rigidbody playerBody = CreatePlayerWithChildCollider(out BoxCollider playerChildCollider);

        InvokeTriggerEnter(departureTrigger, playerChildCollider);
        InvokeTriggerEnter(arrivalTrigger, playerChildCollider);

        Assert.That(playerBody.position, Is.EqualTo(arrivalPoint.transform.position));
    }

    [Test]
    public void MainGameScene_ConfiguresFirstFloorTransitionTriggers()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Additive);

        try
        {
            Type triggerType = GetTriggerType();
            Transform firstFloorToSecondFloor = FindTransform(scene, "SpawnPoing_1F_to_2F");
            Transform secondFloor = FindTransform(scene, "SpawnPoint_2F");
            Transform firstFloorToBasement = FindTransform(scene, "SpawnPoing_1F_to_Base");
            Transform basementExit = FindTransform(scene, "ExitCollider");
            Transform assignedBasementTarget = GetAssignedTarget(firstFloorToBasement, triggerType);

            Assert.That(assignedBasementTarget, Is.Not.Null,
                "SpawnPoing_1F_to_Base requires a targetPoint.");
            Assert.That(assignedBasementTarget.position, Is.EqualTo(basementExit.position),
                "SpawnPoing_1F_to_Base must target the Map Basement scene instance's WaterTankExplosionController/ExitCollider.");

            AssertTriggerConfiguration(firstFloorToSecondFloor, secondFloor, triggerType);
            AssertTriggerConfiguration(secondFloor, firstFloorToSecondFloor, triggerType);
            AssertTriggerConfiguration(firstFloorToBasement, basementExit, triggerType);
            AssertTriggerConfiguration(basementExit, firstFloorToBasement, triggerType);
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private GameObject CreateObject(string objectName)
    {
        GameObject createdObject = new GameObject(objectName);
        createdObjects.Add(createdObject);
        return createdObject;
    }

    private Component CreateTeleportTrigger(GameObject triggerObject, Transform targetPoint)
    {
        Type triggerType = GetTriggerType();

        Component trigger = triggerObject.AddComponent(triggerType);
        SerializedObject serializedTrigger = new SerializedObject(trigger);
        SerializedProperty serializedTargetPoint = serializedTrigger.FindProperty("targetPoint");
        Assert.That(serializedTargetPoint, Is.Not.Null,
            "FloorTransitionTeleportTrigger requires a targetPoint Inspector field.");
        serializedTargetPoint.objectReferenceValue = targetPoint;
        serializedTrigger.ApplyModifiedPropertiesWithoutUndo();

        return trigger;
    }

    private static Type GetTriggerType()
    {
        Type triggerType = typeof(PlayerInteraction).Assembly.GetType(TriggerTypeName);
        Assert.That(triggerType, Is.Not.Null,
            "FloorTransitionTeleportTrigger script is required for floor transition colliders.");
        return triggerType;
    }

    private static Transform FindTransform(Scene scene, string transformName)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Transform transform in rootObject.GetComponentsInChildren<Transform>(true))
            {
                if (transform.name == transformName)
                {
                    return transform;
                }
            }
        }

        Assert.Fail($"MainGameScene requires {transformName}.");
        return null;
    }

    private static void AssertTriggerConfiguration(Transform source, Transform expectedTarget, Type triggerType)
    {
        Assert.That(source, Is.Not.Null, "The floor transition source object is required.");
        Assert.That(expectedTarget, Is.Not.Null, "The floor transition target object is required.");

        BoxCollider sourceCollider = source.GetComponent<BoxCollider>();
        Assert.That(sourceCollider, Is.Not.Null, $"{source.name} requires a BoxCollider.");
        Assert.That(sourceCollider.isTrigger, Is.True, $"{source.name} BoxCollider must be a trigger.");

        Component trigger = source.GetComponent(triggerType);
        Assert.That(trigger, Is.Not.Null, $"{source.name} requires FloorTransitionTeleportTrigger.");

        SerializedObject serializedTrigger = new SerializedObject(trigger);
        Assert.That(serializedTrigger.FindProperty("targetPoint").objectReferenceValue, Is.EqualTo(expectedTarget),
            $"{source.name} must target {expectedTarget.name}.");
    }

    private static Transform GetAssignedTarget(Transform source, Type triggerType)
    {
        SerializedObject serializedTrigger = new SerializedObject(source.GetComponent(triggerType));
        return serializedTrigger.FindProperty("targetPoint").objectReferenceValue as Transform;
    }

    private Rigidbody CreatePlayerWithChildCollider(out BoxCollider playerChildCollider)
    {
        GameObject playerRoot = CreateObject("Player");
        playerRoot.tag = "Player";
        Rigidbody playerBody = playerRoot.AddComponent<Rigidbody>();

        GameObject playerChild = CreateObject("PlayerChildCollider");
        playerChild.transform.SetParent(playerRoot.transform);
        playerChildCollider = playerChild.AddComponent<BoxCollider>();

        return playerBody;
    }

    private static void InvokeTriggerEnter(Component trigger, Collider other)
    {
        MethodInfo triggerEnterMethod = trigger.GetType().GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null,
            "FloorTransitionTeleportTrigger requires an OnTriggerEnter handler.");
        triggerEnterMethod.Invoke(trigger, new object[] { other });
    }
}
