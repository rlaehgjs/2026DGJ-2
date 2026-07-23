using System;
using UnityEngine;
using UnityEngine.UI;

public class StartCutscenePlayer : MonoBehaviour
{
    private const float FrameDuration = 0.08f;

    private Texture2D[] frames;
    private GameManager gameManager;
    private RawImage image;
    private int frameIndex;
    private float elapsed;
    private bool completed;

    private void Awake()
    {
        frames = Resources.LoadAll<Texture2D>("StartCutsceneFrames");
        Array.Sort(frames, (left, right) => string.CompareOrdinal(left.name, right.name));
        gameManager = FindAnyObjectByType<GameManager>();
        CreateDisplay();
    }

    private void Start()
    {
        if (frames.Length == 0)
        {
            Debug.LogError("Start cutscene frames are missing.", this);
            Complete();
            return;
        }

        image.texture = frames[0];
    }

    private void Update()
    {
        if (completed || frames.Length == 0)
            return;

        elapsed += Time.unscaledDeltaTime;
        if (elapsed < FrameDuration)
            return;

        elapsed -= FrameDuration;
        frameIndex++;

        if (frameIndex >= frames.Length)
        {
            Complete();
            return;
        }

        image.texture = frames[frameIndex];
    }

    private void CreateDisplay()
    {
        GameObject canvasObject = new GameObject("CutsceneCanvas", typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        GameObject imageObject = new GameObject("CutsceneImage", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(canvasObject.transform, false);
        image = imageObject.GetComponent<RawImage>();
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void Complete()
    {
        if (completed)
            return;

        completed = true;
        gameManager?.CompleteCutscene();
    }
}
