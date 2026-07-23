using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class EnvironmentEffectZoneTests
{
    [Test]
    public void TriggerEnterAndExit_AppliesAndRemovesEffect()
    {
        Type zoneType = Type.GetType("EnvironmentEffectZone, Assembly-CSharp");
        GameObject player = new GameObject("Player");
        GameObject zoneObject = new GameObject("CoolingZone");

        try
        {
            PlayerEffectAdapter adapter = player.AddComponent<PlayerEffectAdapter>();
            Collider playerCollider = player.AddComponent<BoxCollider>();

            Assert.That(zoneType, Is.Not.Null);

            Component zone = zoneObject.AddComponent(zoneType);
            SetPrivateField(zoneType, zone, "meltRateMultiplier", 0.7f);
            SetPrivateField(zoneType, zone, "moveSpeedMultiplier", 1.2f);

            MethodInfo triggerEnter = zoneType.GetMethod("OnTriggerEnter", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo triggerExit = zoneType.GetMethod("OnTriggerExit", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(triggerEnter, Is.Not.Null);
            Assert.That(triggerExit, Is.Not.Null);

            triggerEnter.Invoke(zone, new object[] { playerCollider });

            Assert.That(adapter.CurrentMeltRateMultiplier, Is.EqualTo(0.7f).Within(0.001f));
            Assert.That(adapter.CurrentMoveSpeedMultiplier, Is.EqualTo(1.2f).Within(0.001f));

            triggerExit.Invoke(zone, new object[] { playerCollider });

            Assert.That(adapter.CurrentMeltRateMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(adapter.CurrentMoveSpeedMultiplier, Is.EqualTo(1f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(zoneObject);
        }
    }

    [Test]
    public void DisableWhilePlayerIsInside_RemovesEffect()
    {
        Type zoneType = Type.GetType("EnvironmentEffectZone, Assembly-CSharp");
        GameObject player = new GameObject("Player");
        GameObject zoneObject = new GameObject("CoolingZone");

        try
        {
            PlayerEffectAdapter adapter = player.AddComponent<PlayerEffectAdapter>();
            Collider playerCollider = player.AddComponent<BoxCollider>();
            Component zone = zoneObject.AddComponent(zoneType);

            SetPrivateField(zoneType, zone, "meltRateMultiplier", 0.7f);
            SetPrivateField(zoneType, zone, "moveSpeedMultiplier", 1.2f);

            MethodInfo triggerEnter = zoneType.GetMethod("OnTriggerEnter", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo onDisable = zoneType.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(triggerEnter, Is.Not.Null);
            Assert.That(onDisable, Is.Not.Null);

            triggerEnter.Invoke(zone, new object[] { playerCollider });

            Assert.That(adapter.CurrentMeltRateMultiplier, Is.EqualTo(0.7f).Within(0.001f));

            onDisable.Invoke(zone, null);

            Assert.That(adapter.CurrentMeltRateMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(adapter.CurrentMoveSpeedMultiplier, Is.EqualTo(1f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(zoneObject);
        }
    }

    private static void SetPrivateField(Type type, object target, string fieldName, float value)
    {
        FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        field.SetValue(target, value);
    }
}
