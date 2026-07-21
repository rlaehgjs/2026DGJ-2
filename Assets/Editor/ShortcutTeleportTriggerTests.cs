using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ShortcutTeleportTriggerTests
{
    private GameObject shortcutObject;
    private GameObject playerRoot;
    private GameObject playerVisual;
    private GameObject destinationObject;

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(shortcutObject);
        Object.DestroyImmediate(playerRoot);
        Object.DestroyImmediate(playerVisual);
        Object.DestroyImmediate(destinationObject);
    }

    [Test]
    public void TriggerEnter_PlayerChildCollider_MovesPlayerRigidBodyToDestination()
    {
        shortcutObject = new GameObject("ShortcutIn");
        ShortcutTeleport shortcutTeleport = shortcutObject.AddComponent<ShortcutTeleport>();

        destinationObject = new GameObject("ShortcutDestination");
        destinationObject.transform.position = new Vector3(10f, 2f, 5f);
        shortcutTeleport.destination = destinationObject.transform;

        playerRoot = new GameObject("Player");
        playerRoot.tag = "Player";
        Rigidbody playerBody = playerRoot.AddComponent<Rigidbody>();

        playerVisual = new GameObject("Icecream");
        playerVisual.transform.SetParent(playerRoot.transform);
        BoxCollider visualCollider = playerVisual.AddComponent<BoxCollider>();

        InvokeTriggerEnter(shortcutTeleport, visualCollider);

        Assert.That(playerBody.position, Is.EqualTo(destinationObject.transform.position));
    }

    private static void InvokeTriggerEnter(ShortcutTeleport shortcutTeleport, Collider other)
    {
        MethodInfo triggerEnterMethod = typeof(ShortcutTeleport).GetMethod(
            "OnTriggerEnter",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(triggerEnterMethod, Is.Not.Null, "ShortcutTeleport에 Trigger 진입 처리가 필요합니다.");
        triggerEnterMethod.Invoke(shortcutTeleport, new object[] { other });
    }
}
