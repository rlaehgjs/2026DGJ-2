using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PlayerPhysicsMaterialTests
{
    private const string PlayerPhysicsMaterialPath = "Assets/Art/Materials/Physics/PlayerNoFriction.physicMaterial";
    private const string IcecreamPrefabPath = "Assets/Prefabs/Player/Icecream.prefab";
    private const string PlayerTestPrefabPath = "Assets/Prefabs/Player/Player(Test).prefab";

    [Test]
    public void PlayerColliders_UseZeroFrictionPhysicsMaterial()
    {
        PhysicsMaterial physicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(PlayerPhysicsMaterialPath);

        Assert.That(physicsMaterial, Is.Not.Null, "플레이어 전용 Physics Material이 필요합니다.");
        Assert.That(physicsMaterial.dynamicFriction, Is.Zero);
        Assert.That(physicsMaterial.staticFriction, Is.Zero);
        Assert.That(physicsMaterial.bounciness, Is.Zero);
        Assert.That(physicsMaterial.frictionCombine.ToString(), Is.EqualTo("Minimum"));

        AssertPrefabColliderMaterial(IcecreamPrefabPath, physicsMaterial);
        AssertPrefabColliderMaterial(PlayerTestPrefabPath, physicsMaterial);
    }

    private static void AssertPrefabColliderMaterial(string prefabPath, PhysicsMaterial expectedMaterial)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, $"{prefabPath} Prefab을 찾을 수 없습니다.");

        BoxCollider collider = prefab.GetComponentInChildren<BoxCollider>(true);
        Assert.That(collider, Is.Not.Null, $"{prefabPath}에 Box Collider가 필요합니다.");
        Assert.That(collider.sharedMaterial, Is.EqualTo(expectedMaterial));
    }
}
