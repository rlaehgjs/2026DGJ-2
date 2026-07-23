using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class EndingPanelFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 2f;

    private Image overlay;

    private void Awake()
    {
        overlay = GetComponent<Image>();
    }

    private void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator FadeIn()
    {
        Color color = overlay.color;
        color.a = 0f;
        overlay.color = color;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = elapsed / fadeDuration;
            overlay.color = color;
            yield return null;
        }

        color.a = 1f;
        overlay.color = color;
    }
}
