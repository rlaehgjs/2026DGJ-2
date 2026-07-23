using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ReDevelopPlayerBasementIntegrationTests
{
    private const string MainGameScenePath = "Assets/Scenes/MainGameScene.unity";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const string BasementPrefabPath = "Assets/Prefabs/Map/Map Basement.prefab";

    [Test]
    public void PlayerPrefab_HasCoreMovementComponents()
    {
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);

        Assert.That(playerPrefab, Is.Not.Null, $"Player Prefab을 찾지 못했습니다: {PlayerPrefabPath}");
        Assert.That(playerPrefab.GetComponent<PlayerMovement>(), Is.Not.Null);
        Assert.That(playerPrefab.GetComponent<PlayerLook>(), Is.Not.Null);
        Assert.That(playerPrefab.GetComponent<PlayerMeltSystem>(), Is.Not.Null);
        Assert.That(playerPrefab.GetComponent<Rigidbody>(), Is.Not.Null);
    }

    [Test]
    public void MainGameScene_HasConnectedBasementProgress()
    {
        var scene = EditorSceneManager.OpenScene(MainGameScenePath, OpenSceneMode.Single);
        GameObject basement = FindPrefabInstanceRoot(scene, BasementPrefabPath);

        Assert.That(basement, Is.Not.Null, "MainGameScene에 Map Basement Prefab 인스턴스가 없습니다.");

        GeneratorWirePickupProgress wireProgress = basement.GetComponentInChildren<GeneratorWirePickupProgress>(true);
        Assert.That(wireProgress, Is.Not.Null, "지하에 GeneratorWirePickupProgress가 없습니다.");

        SerializedObject serializedWireProgress = new(wireProgress);
        Assert.That(serializedWireProgress.FindProperty("pickupInteractable").objectReferenceValue, Is.Not.Null);
        Assert.That(serializedWireProgress.FindProperty("gameProgressManager").objectReferenceValue, Is.Not.Null);

        WaterTankExplosionController waterTank = basement.GetComponentInChildren<WaterTankExplosionController>(true);
        Assert.That(waterTank, Is.Not.Null, "지하에 WaterTankExplosionController가 없습니다.");

        SerializedObject serializedWaterTank = new(waterTank);
        Assert.That(serializedWaterTank.FindProperty("playerMeltSystem").objectReferenceValue, Is.Not.Null);
    }

    private static GameObject FindPrefabInstanceRoot(UnityEngine.SceneManagement.Scene scene, string prefabPath)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object source = PrefabUtility.GetCorrespondingObjectFromSource(root);
            if (source != null && AssetDatabase.GetAssetPath(source) == prefabPath)
            {
                return root;
            }
        }

        return null;
    }
}
