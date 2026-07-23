using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorTransitionTeleportSetupTests
{
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";

    [Test]
    public void MainGameScene_ConfiguresAllFloorTransitionTeleportPairs()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);

        Transform firstFloorToSecondFloor = FindTransform(scene, "SpawnPoing_1F_to_2F");
        Transform secondFloor = FindTransform(scene, "SpawnPoint_2F");
        Transform firstFloorToBasement = FindTransform(scene, "SpawnPoing_1F_to_Base");
        Transform basementExit = FindTransform(scene, "ExitCollider");

        AssertTeleportConfiguration(firstFloorToSecondFloor, secondFloor);
        AssertTeleportConfiguration(secondFloor, firstFloorToSecondFloor);
        AssertTeleportConfiguration(firstFloorToBasement, basementExit);
        AssertTeleportConfiguration(basementExit, firstFloorToBasement);
    }

    private static Transform FindTransform(Scene scene, string objectName)
    {
        Transform transform = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .FirstOrDefault(candidate => candidate.name == objectName);

        Assert.That(transform, Is.Not.Null, $"MainGameScene requires {objectName}.");
        return transform;
    }

    private static void AssertTeleportConfiguration(Transform source, Transform expectedTarget)
    {
        BoxCollider collider = source.GetComponent<BoxCollider>();
        Assert.That(collider, Is.Not.Null, $"{source.name} requires a BoxCollider.");
        Assert.That(collider.isTrigger, Is.True, $"{source.name} BoxCollider must be a trigger.");

        FloorTransitionTeleportTrigger trigger = source.GetComponent<FloorTransitionTeleportTrigger>();
        Assert.That(trigger, Is.Not.Null, $"{source.name} requires FloorTransitionTeleportTrigger.");

        SerializedProperty targetPoint = new SerializedObject(trigger).FindProperty("targetPoint");
        Assert.That(targetPoint.objectReferenceValue, Is.EqualTo(expectedTarget),
            $"{source.name} must target {expectedTarget.name}.");
    }
}
