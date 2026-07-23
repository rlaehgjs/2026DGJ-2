using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayerInteractionLoggingTests
{
    [Test]
    public void LogInteractionTarget_LogsTargetObjectAndAvailability()
    {
        GameObject frontDoor = new GameObject("FrontDoor");

        try
        {
            Collider doorCollider = frontDoor.AddComponent<BoxCollider>();
            TestInteractable interactable = frontDoor.AddComponent<TestInteractable>();

            LogAssert.Expect(
                LogType.Log,
                new Regex("^\\[Interaction\\] Target=FrontDoor; Interactable=TestInteractable; CanInteract=True$"));

            InvokeLogInteractionTarget(doorCollider, interactable, true);
        }
        finally
        {
            Object.DestroyImmediate(frontDoor);
        }
    }

    private static void InvokeLogInteractionTarget(
        Collider doorCollider,
        IInteractable interactable,
        bool canInteract)
    {
        MethodInfo logInteractionTarget = typeof(PlayerInteraction).GetMethod(
            "LogInteractionTarget",
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.That(logInteractionTarget, Is.Not.Null);
        logInteractionTarget.Invoke(null, new object[] { doorCollider, interactable, canInteract });
    }

    private sealed class TestInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInventory inventory)
        {
            return true;
        }

        public void Interact(PlayerInventory inventory)
        {
        }
    }
}
