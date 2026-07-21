using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class PlayerRigidbodyConstraintTests
{
    [TestCase("Assets/Prefabs/Player/Player.prefab")]
    [TestCase("Assets/Prefabs/Player/Player(Test).prefab")]
    public void PlayerRigidbody_FreezesAllPhysicsRotation(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Assert.That(prefab, Is.Not.Null, $"{prefabPath} Prefab을 찾을 수 없습니다.");

        Rigidbody rigidbody = prefab.GetComponent<Rigidbody>();
        Assert.That(rigidbody, Is.Not.Null, $"{prefabPath}에 Rigidbody가 필요합니다.");
        Assert.That(
            rigidbody.constraints & RigidbodyConstraints.FreezeRotation,
            Is.EqualTo(RigidbodyConstraints.FreezeRotation));
    }
}
