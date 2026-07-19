using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    private const string BrightnessKey = "Brightness";

    [SerializeField] private TMP_Text percentText;
    [SerializeField] private float minimumAmbientIntensity = 0.1f;
    [SerializeField] private float maximumAmbientIntensity = 2f;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogWarning("BrightnessSlider: Slider가 필요합니다.", this);
            enabled = false;
            return;
        }

        float brightness = PlayerPrefs.GetFloat(BrightnessKey, 0.5f);
        slider.SetValueWithoutNotify(brightness);
        ApplyBrightness(brightness);
        slider.onValueChanged.AddListener(ApplyBrightness);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(ApplyBrightness);
    }

    private void ApplyBrightness(float value)
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(
            minimumAmbientIntensity,
            maximumAmbientIntensity,
            value);

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(value * 100f) + "%";

        PlayerPrefs.SetFloat(BrightnessKey, value);
        PlayerPrefs.Save();
    }
}
