using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BathroomJumpscareTriggerTests
{
    private GameObject jumpscareObject;
    private GameObject playerRoot;
    private GameObject playerVisual;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(jumpscareObject);
        Object.DestroyImmediate(playerRoot);
        Object.DestroyImmediate(playerVisual);
    }

    [Test]
    public void TriggerEnter_PlayerChildCollider_ConsumesJumpscareTrigger()
    {
        jumpscareObject = new GameObject("BathroomJumpscare");
        BathroomJumpscare jumpscare = jumpscareObject.AddComponent<BathroomJumpscare>();
        BoxCollider jumpscareCollider = jumpscareObject.AddComponent<BoxCollider>();

        playerRoot = new GameObject("Player");
        playerRoot.tag = "Player";
        playerRoot.AddComponent<Rigidbody>();

        playerVisual = new GameObject("Icecream");
        playerVisual.transform.SetParent(playerRoot.transform);
        BoxCollider visualCollider = playerVisual.AddComponent<BoxCollider>();

        InvokeTriggerEnter(jumpscare, visualCollider);

        Assert.That(jumpscareCollider.enabled, Is.False);
    }

    private static void InvokeTriggerEnter(BathroomJumpscare jumpscare, Collider other)
    {
        MethodInfo triggerEnterMethod = typeof(BathroomJumpscare).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null, "BathroomJumpscare에 Trigger 진입 처리가 필요합니다.");
        triggerEnterMethod.Invoke(jumpscare, new object[] { other });
    }
}
