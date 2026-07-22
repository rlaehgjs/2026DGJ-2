using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text percentText;
    [SerializeField] private float minimumAmbientIntensity = 0.1f;
    [SerializeField] private float maximumAmbientIntensity = 2f;

    private Slider slider;
    private SaveManager saveManager;

    public void Configure(SaveManager settingsSaveManager)
    {
        saveManager = settingsSaveManager;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
        if (slider == null)
        {
            Debug.LogWarning("BrightnessSlider: Slider가 필요합니다.", this);
            enabled = false;
            return;
        }

        if (saveManager == null)
            saveManager = FindAnyObjectByType<SaveManager>();

        float brightness = saveManager != null ? saveManager.LoadSettings().Brightness : 0.5f;
        slider.SetValueWithoutNotify(brightness);
        ApplyBrightness(brightness);
        GetComponent<PercentageSliderUI>()?.Refresh();
        slider.onValueChanged.AddListener(SetBrightness);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(SetBrightness);
    }

    private void SetBrightness(float value)
    {
        ApplyBrightness(value);

        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        settings.Brightness = value;
        saveManager.SaveSettings(settings);
    }

    private void ApplyBrightness(float value)
    {
        RenderSettings.ambientIntensity = Mathf.Lerp(minimumAmbientIntensity, maximumAmbientIntensity, value);

        if (percentText != null)
            percentText.text = Mathf.RoundToInt(value * 100f) + "%";
    }
}
