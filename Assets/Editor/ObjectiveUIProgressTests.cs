using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ObjectiveUIProgressTests
{
    [Test]
    public void FindFrontDoorKey_HasLocalizedObjective()
    {
        MethodInfo getObjectiveKey = typeof(ObjectiveUI).GetMethod("GetObjectiveKey", BindingFlags.Static | BindingFlags.NonPublic);
        string key = (string)getObjectiveKey.Invoke(null, new object[] { GameProgressState.FindFrontDoorKey });
        TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Data/Configs/UI/Localization.csv");

        Assert.That(key, Is.EqualTo("objective_find_front_door_key"));
        Assert.That(csv.text, Does.Contain("objective_find_front_door_key,"));
    }
}
