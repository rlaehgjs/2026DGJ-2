using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class PercentageSliderUI : MonoBehaviour
{
    [SerializeField] private TMP_Text percentText;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (percentText == null)
        {
            Debug.LogWarning("PercentageSliderUI: Percent Text를 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        slider.onValueChanged.AddListener(UpdatePercentText);
        UpdatePercentText(slider.value);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(UpdatePercentText);
    }

    private void UpdatePercentText(float value)
    {
        float normalizedValue = Mathf.InverseLerp(slider.minValue, slider.maxValue, value);
        percentText.text = $"{Mathf.RoundToInt(normalizedValue * 100f)}%";
    }
}
