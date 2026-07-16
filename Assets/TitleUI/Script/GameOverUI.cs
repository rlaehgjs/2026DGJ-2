using System.Collections;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverTitle;
    [SerializeField] private GameObject buttonGroup;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup buttonCanvasGroup;
    [SerializeField] private float titleFadeInDuration = 1f;
    [SerializeField] private float titleDuration = 1f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float buttonFadeDuration = 1f;

    private void OnEnable()
    {
        StartCoroutine(ShowRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator ShowRoutine()
    {
        buttonGroup.SetActive(false);
        gameOverTitle.SetActive(true);
        titleCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < titleFadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            titleCanvasGroup.alpha = elapsed / titleFadeInDuration;
            yield return null;
        }

        titleCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(titleDuration);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            titleCanvasGroup.alpha = 1f - elapsed / fadeDuration;
            yield return null;
        }

        gameOverTitle.SetActive(false);
        buttonGroup.SetActive(true);
        buttonCanvasGroup.alpha = 0f;

        elapsed = 0f;
        while (elapsed < buttonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            buttonCanvasGroup.alpha = elapsed / buttonFadeDuration;
            yield return null;
        }

        buttonCanvasGroup.alpha = 1f;
    }
}
