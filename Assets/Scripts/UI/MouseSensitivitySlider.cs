using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MouseSensitivitySlider : MonoBehaviour
{
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private float minimumSensitivity = 0f;
    [SerializeField] private float maximumSensitivity = 1000f;

    private Slider slider;
    private SaveManager saveManager;

    public void Configure(PlayerLook look, SaveManager settingsSaveManager)
    {
        playerLook = look;
        saveManager = settingsSaveManager;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
        float sensitivity = saveManager != null ? saveManager.LoadSettings().MouseSensitivity : slider.value;

        slider.SetValueWithoutNotify(sensitivity);
        ApplySensitivity(sensitivity);
        GetComponent<PercentageSliderUI>()?.Refresh();
        slider.onValueChanged.AddListener(SetSensitivity);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(SetSensitivity);
    }

    private void SetSensitivity(float value)
    {
        ApplySensitivity(value);

        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        settings.MouseSensitivity = value;
        saveManager.SaveSettings(settings);
    }

    private void ApplySensitivity(float value)
    {
        if (playerLook == null)
            return;

        playerLook.SetMouseSensitivity(Mathf.Lerp(minimumSensitivity, maximumSensitivity, value));
    }
}
