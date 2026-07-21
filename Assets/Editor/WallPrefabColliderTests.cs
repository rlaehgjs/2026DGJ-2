using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WallPrefabColliderTests
{
    private const string MainMapPrefabPath = "Assets/Prefabs/Map/Hous_1F.prefab";
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";
    private static readonly string[] StaticKitchenObjectNames =
    {
        "Gas_Stove",
        "Stove",
        "Sink",
        "Sink.001",
        "Sink.002"
    };

    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_default 1.prefab", new[] { "wall_default 1" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_default_2 1.prefab", new[] { "wall_default_2 1" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_corner 1.prefab", new[] { "wall_corner 1" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_corner_2.prefab", new[] { "wall_corner_2" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_corner_double.prefab", new[] { "wall_corner_double" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_with_one_window.prefab", new[] { "wall.002", "window_small_frame", "window_small", "glass" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_with_two_window.prefab", new[] { "wall.003", "window_right_frame", "window_left_frame", "window_left", "glass_right", "glass_left", "window_right" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_with_small_door.prefab", new[] { "wall", "small_door_frame" })]
    [TestCase("Assets/Prefabs/Environment/InteriorPrefabs/wall_with_big_door.prefab", new[] { "wall.001", "big_door_frame" })]
    public void StaticWallMeshes_HaveMeshColliders(string prefabPath, string[] staticObjectNames)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, $"{prefabPath} prefab was not found.");

        foreach (string objectName in staticObjectNames)
        {
            Transform target = FindChildByName(prefab.transform, objectName);
            Assert.That(target, Is.Not.Null, $"{prefabPath} is missing static object '{objectName}'.");
            Assert.That(target.GetComponent<MeshCollider>(), Is.Not.Null,
                $"{prefabPath} static object '{objectName}' needs a MeshCollider.");
        }
    }

    [Test]
    public void MainGameMap_StaticWallsAndKitchenMeshes_HaveColliders()
    {
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainMapPrefabPath);
        Assert.That(mapPrefab, Is.Not.Null, $"{MainMapPrefabPath} prefab was not found.");

        List<string> missingColliderObjectNames = new();

        foreach (MeshFilter meshFilter in mapPrefab.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!IsStaticWallOrKitchenObject(meshFilter.gameObject.name))
            {
                continue;
            }

            if (meshFilter.GetComponent<Collider>() == null)
            {
                missingColliderObjectNames.Add(meshFilter.gameObject.name);
            }
        }

        Assert.That(missingColliderObjectNames, Is.Empty,
            $"MainGameScene map objects need colliders: {string.Join(", ", missingColliderObjectNames)}");
    }

    [Test]
    public void MainGameMap_UserAddedFixedMeshes_HaveMeshColliders()
    {
        GameObject mapPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainMapPrefabPath);
        Assert.That(mapPrefab, Is.Not.Null, $"{MainMapPrefabPath} prefab was not found.");

        foreach (string objectName in new[] { "Cube", "bookshelf", "bookshelf (1)" })
        {
            Transform target = FindChildByName(mapPrefab.transform, objectName);
            Assert.That(target, Is.Not.Null, $"{MainMapPrefabPath} is missing '{objectName}'.");
            Assert.That(target.GetComponent<MeshCollider>(), Is.Not.Null,
                $"{MainMapPrefabPath} fixed object '{objectName}' needs a MeshCollider.");
        }
    }

    [Test]
    public void MainGameScene_HousMapHasNoAddedMeshColliderOverrides()
    {
        Scene scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Additive);

        try
        {
            List<string> overriddenObjectNames = new();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                foreach (MeshCollider collider in rootObject.GetComponentsInChildren<MeshCollider>(true))
                {
                    if (PrefabUtility.IsAddedComponentOverride(collider)
                        && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(collider.gameObject) == MainMapPrefabPath)
                    {
                        overriddenObjectNames.Add(collider.gameObject.name);
                    }
                }
            }

            Assert.That(overriddenObjectNames, Is.Empty,
                $"Move these MeshCollider overrides into {MainMapPrefabPath}: {string.Join(", ", overriddenObjectNames)}");
        }
        finally
        {
            EditorSceneManager.CloseScene(scene, true);
        }
    }

    private static bool IsStaticWallOrKitchenObject(string objectName)
    {
        return objectName.StartsWith("wall", StringComparison.OrdinalIgnoreCase)
            || Array.IndexOf(StaticKitchenObjectNames, objectName) >= 0;
    }

    private static Transform FindChildByName(Transform root, string objectName)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
            {
                return child;
            }
        }

        return null;
    }
}
