using System;
using NUnit.Framework;
using UnityEngine;

public class PlayerEffectAdapterTests
{
    [Test]
    public void PlayerEffectAdapter_TypeExists()
    {
        Type adapterType = Type.GetType("PlayerEffectAdapter, Assembly-CSharp");

        Assert.That(adapterType, Is.Not.Null);
    }

    [Test]
    public void AddAndRemoveEffect_RecalculatesRemainingMultipliers()
    {
        Type adapterType = Type.GetType("PlayerEffectAdapter, Assembly-CSharp");
        GameObject player = new GameObject("Player");
        GameObject marbleFloor = new GameObject("MarbleFloor");
        GameObject fanWind = new GameObject("FanWind");

        try
        {
            Component adapter = player.AddComponent(adapterType);
            var addEffect = adapterType.GetMethod("AddEffect");
            var removeEffect = adapterType.GetMethod("RemoveEffect");
            var meltMultiplier = adapterType.GetProperty("CurrentMeltRateMultiplier");
            var moveMultiplier = adapterType.GetProperty("CurrentMoveSpeedMultiplier");

            Assert.That(addEffect, Is.Not.Null);
            Assert.That(removeEffect, Is.Not.Null);
            Assert.That(meltMultiplier, Is.Not.Null);
            Assert.That(moveMultiplier, Is.Not.Null);

            addEffect.Invoke(adapter, new object[] { marbleFloor, 0.8f, 1.2f });
            addEffect.Invoke(adapter, new object[] { fanWind, 0.7f, 1.0f });

            Assert.That((float)meltMultiplier.GetValue(adapter), Is.EqualTo(0.56f).Within(0.001f));
            Assert.That((float)moveMultiplier.GetValue(adapter), Is.EqualTo(1.2f).Within(0.001f));

            removeEffect.Invoke(adapter, new object[] { marbleFloor });

            Assert.That((float)meltMultiplier.GetValue(adapter), Is.EqualTo(0.7f).Within(0.001f));
            Assert.That((float)moveMultiplier.GetValue(adapter), Is.EqualTo(1.0f).Within(0.001f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(player);
            UnityEngine.Object.DestroyImmediate(marbleFloor);
            UnityEngine.Object.DestroyImmediate(fanWind);
        }
    }
}
