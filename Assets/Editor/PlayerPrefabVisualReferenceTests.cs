using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PlayerPrefabVisualReferenceTests
{
    private const string PlayerPrefabPath = "Assets/Prefabs/Player/Player.prefab";
    private const string PlayerModelPath = "Assets/Art/Models/Icecream/Blue_Popsicle.fbx";

    [Test]
    public void PlayerPrefab_UsesDedicatedIcecreamModel()
    {
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        MeshFilter meshFilter = playerPrefab.GetComponentInChildren<MeshFilter>(true);

        Assert.That(playerPrefab, Is.Not.Null);
        Assert.That(meshFilter, Is.Not.Null);
        Assert.That(meshFilter.sharedMesh, Is.Not.Null);
        Assert.That(AssetDatabase.GetAssetPath(meshFilter.sharedMesh), Is.EqualTo(PlayerModelPath));
    }
}
