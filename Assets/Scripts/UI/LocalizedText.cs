using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string key;

    private TMP_Text textComponent;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null)
            textComponent = GetComponentInChildren<TMP_Text>(true);
    }

    private void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("LocalizedText가 표시할 TextMeshPro 텍스트를 찾지 못했습니다.", this);
            enabled = false;
            return;
        }

        LocalizationManager.Instance.LanguageChanged += Apply;
        Apply();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= Apply;
    }

    private void Apply()
    {
        if (textComponent != null && LocalizationManager.Instance != null)
            textComponent.text = LocalizationManager.Instance.Get(key);
    }
}
