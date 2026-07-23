using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BgmVolumeSlider : MonoBehaviour
{
    [SerializeField] private bool isTitleVolume;
    [SerializeField] private TMP_Text percentText;

    private Slider slider;
    private SoundManager soundManager;
    private SaveManager saveManager;

    public void Configure(SoundManager manager, SaveManager settingsSaveManager)
    {
        soundManager = manager;
        saveManager = settingsSaveManager;
        RefreshFromSavedSettings();
    }

    private void OnEnable()
    {
        RefreshFromSavedSettings();
    }

    private void RefreshFromSavedSettings()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null || percentText == null)
        {
            Debug.LogWarning("BgmVolumeSlider: Slider와 Percent Text를 모두 연결해야 합니다.", this);
            return;
        }

        if (saveManager == null)
            saveManager = FindAnyObjectByType<SaveManager>();

        GameSettingsData settings = saveManager != null ? saveManager.LoadSettings() : null;
        float savedVolume = settings == null
            ? 1f
            : isTitleVolume ? settings.TitleVolume : settings.BgmVolume;

        slider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);
        slider.onValueChanged.RemoveListener(SetVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        ApplyVolume(volume);

        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        if (isTitleVolume)
            settings.TitleVolume = volume;
        else
            settings.BgmVolume = volume;
        saveManager.SaveSettings(settings);
    }

    private void ApplyVolume(float volume)
    {
        SoundManager manager = soundManager ?? SoundManager.Instance;
        if (manager != null)
        {
            if (isTitleVolume)
                manager.SetTitleVolume(volume);
            else
                manager.SetBgmVolume(volume);
        }

        percentText.text = $"{Mathf.RoundToInt(volume * 100)}%";
    }
}
