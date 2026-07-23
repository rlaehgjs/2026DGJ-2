using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class AntManagerTriggerTests
{
    private GameObject managerObject;
    private GameObject antGroupObject;
    private GameObject playerRoot;
    private GameObject playerVisual;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(managerObject);
        Object.DestroyImmediate(antGroupObject);
        Object.DestroyImmediate(playerRoot);
        Object.DestroyImmediate(playerVisual);
    }

    [Test]
    public void TriggerEnter_PlayerChildCollider_ActivatesAntGroup()
    {
        managerObject = new GameObject("AntTrigger");
        AntManager antManager = managerObject.AddComponent<AntManager>();

        antGroupObject = new GameObject("AntGroup");
        antGroupObject.SetActive(false);
        antManager.antGroup = antGroupObject;

        playerRoot = new GameObject("Player");
        playerRoot.tag = "Player";
        playerRoot.AddComponent<Rigidbody>();

        playerVisual = new GameObject("Icecream");
        playerVisual.transform.SetParent(playerRoot.transform);
        BoxCollider visualCollider = playerVisual.AddComponent<BoxCollider>();

        InvokeTriggerEnter(antManager, visualCollider);

        Assert.That(antGroupObject.activeSelf, Is.True);
    }

    private static void InvokeTriggerEnter(AntManager antManager, Collider other)
    {
        MethodInfo triggerEnterMethod = typeof(AntManager).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null, "AntManager에 Trigger 진입 처리가 필요합니다.");
        triggerEnterMethod.Invoke(antManager, new object[] { other });
    }
}
