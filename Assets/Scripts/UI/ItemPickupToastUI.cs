using System.Collections;
using TMPro;
using UnityEngine;

public class ItemPickupToastUI : MonoBehaviour
{
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private float moveDistance = 166f;
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private bool playTestOnStart;

    private RectTransform rectTransform;
    private Vector2 startPosition;
    private Color textColor;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
        textColor = toastText != null ? toastText.color : Color.white;
    }

    private void Start()
    {
        if (playTestOnStart && toastText != null)
            Show(toastText.text);
    }

    public void Show(string message)
    {
        if (toastText == null)
        {
            Debug.LogWarning("ItemPickupToastUI: Toast Text를 연결해야 합니다.", this);
            return;
        }

        gameObject.SetActive(true);
        toastText.text = message;
        toastText.color = textColor;

        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveUp());
    }

    public void Hide()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        gameObject.SetActive(false);
    }

    private IEnumerator MoveUp()
    {
        rectTransform.anchoredPosition = startPosition;

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / moveDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                startPosition + Vector2.up * moveDistance,
                progress);
            toastText.color = new Color(textColor.r, textColor.g, textColor.b,
                Mathf.Lerp(textColor.a, 0f, progress));
            yield return null;
        }

        rectTransform.anchoredPosition = startPosition + Vector2.up * moveDistance;
        toastText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        moveCoroutine = null;
        gameObject.SetActive(false);
    }
}
