using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class FrontDoorLockFallbackTests
{
    [Test]
    public void FrontDoorLock_FindsSceneProgressManager_WhenReferenceIsEmpty()
    {
        GameObject managerObject = new GameObject("GameProgressManager");
        managerObject.AddComponent<GameProgressManager>();
        GameObject lockObject = new GameObject("FrontDoorLock");

        try
        {
            FrontDoorLock frontDoorLock = lockObject.AddComponent<FrontDoorLock>();
            typeof(FrontDoorLock)
                .GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(frontDoorLock, null);
            SerializedProperty progressManager = new SerializedObject(frontDoorLock)
                .FindProperty("gameProgressManager");

            Assert.That(progressManager.objectReferenceValue, Is.Not.Null);
        }
        finally
        {
            Object.DestroyImmediate(lockObject);
            Object.DestroyImmediate(managerObject);
        }
    }
}
