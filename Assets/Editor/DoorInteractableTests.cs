using System;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public class DoorInteractableTests
{
    [Test]
    public void CalculateOpenLocalPosition_ChangesOnlyLocalX()
    {
        Vector3 closedLocalPosition = new Vector3(-0.377f, 0.973f, 0.372f);

        Vector3 openLocalPosition = DrawerInteractable.CalculateOpenLocalPosition(
            closedLocalPosition,
            0.12f);

        Assert.That(openLocalPosition, Is.EqualTo(new Vector3(0.12f, 0.973f, 0.372f)));
    }

    [TestCase("X", 1f, 0f, 0f)]
    [TestCase("Y", 0f, 1f, 0f)]
    [TestCase("Z", 0f, 0f, 1f)]
    public void GetLocalRotationAxis_ReturnsConfiguredAxis(
        string axisName,
        float expectedX,
        float expectedY,
        float expectedZ)
    {
        Type doorInteractableType = typeof(PlayerInteraction).Assembly.GetType("DoorInteractable");
        Assert.That(doorInteractableType, Is.Not.Null);

        Type rotationAxisType = doorInteractableType.GetNestedType(
            "RotationAxis",
            BindingFlags.NonPublic);
        Assert.That(rotationAxisType, Is.Not.Null);

        MethodInfo getLocalRotationAxisMethod = doorInteractableType.GetMethod(
            "GetLocalRotationAxis",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(getLocalRotationAxisMethod, Is.Not.Null);

        object rotationAxis = Enum.Parse(rotationAxisType, axisName);
        Vector3 actual = (Vector3)getLocalRotationAxisMethod.Invoke(
            null,
            new[] { rotationAxis });

        Assert.That(actual, Is.EqualTo(new Vector3(expectedX, expectedY, expectedZ)));
    }

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

    [Test]
    public void GetSignedOpenAngle_TowardPlayer_InvertsAwayFromPlayerAngle()
    {
        Type doorInteractableType = typeof(PlayerInteraction).Assembly.GetType("DoorInteractable");
        Assert.That(doorInteractableType, Is.Not.Null);

        MethodInfo getSignedOpenAngleMethod = doorInteractableType.GetMethod(
            "GetSignedOpenAngle",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(getSignedOpenAngleMethod, Is.Not.Null);

        object[] arguments =
        {
            Vector3.zero,
            Vector3.up,
            Vector3.right,
            new Vector3(0f, 0f, 3f),
            90f,
            true
        };

        float towardPlayerAngle = (float)getSignedOpenAngleMethod.Invoke(null, arguments);
        Assert.That(towardPlayerAngle, Is.EqualTo(-90f));
    }
}
