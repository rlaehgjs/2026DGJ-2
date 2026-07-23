using UnityEditor;
using NUnit.Framework;

public class StartCutsceneAssetsTests
{
    [Test]
    public void HasAllGifFrames()
    {
        string[] frames = AssetDatabase.FindAssets(
            "t:Texture2D",
            new[] { "Assets/Resources/StartCutsceneFrames" });

        Assert.That(frames, Has.Length.EqualTo(108));
    }
}
