using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class GameProgressManagerEditorTests
{
    [Test]
    public void GameProgressManager_UsesDedicatedCurrentStateInspector()
    {
        GameObject progressObject = new GameObject("GameProgressManager");
        GameProgressManager progressManager = progressObject.AddComponent<GameProgressManager>();

        try
        {
            Editor inspector = Editor.CreateEditor(progressManager);

            Assert.That(inspector.GetType().Name, Is.EqualTo("GameProgressManagerEditor"));

            Object.DestroyImmediate(inspector);
        }
        finally
        {
            Object.DestroyImmediate(progressObject);
        }
    }
}
