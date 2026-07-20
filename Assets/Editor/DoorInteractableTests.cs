using System;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class DoorInteractableTests
{
    [Test]
    public void CalculateOpenAngle_MovesDoorCenterAwayFromPlayer()
    {
        Type doorInteractableType = typeof(PlayerInteraction).Assembly.GetType("DoorInteractable");
        Assert.That(doorInteractableType, Is.Not.Null);

        MethodInfo calculateOpenAngleMethod = doorInteractableType.GetMethod(
            "CalculateOpenAngle",
            BindingFlags.Static | BindingFlags.Public);
        Assert.That(calculateOpenAngleMethod, Is.Not.Null);

        float signedAngle = (float)calculateOpenAngleMethod.Invoke(
            null,
            new object[]
            {
                Vector3.zero,
                Vector3.up,
                Vector3.right,
                new Vector3(0f, 0f, 3f),
                90f
            });

        Assert.That(signedAngle, Is.EqualTo(90f));
    }
}
