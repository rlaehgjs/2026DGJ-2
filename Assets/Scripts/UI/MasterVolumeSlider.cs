using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text percentText;

    private Slider slider;
    private SoundManager soundManager;
    private SaveManager saveManager;

    public void Configure(SoundManager manager, SaveManager settingsSaveManager)
    {
        soundManager = manager;
        saveManager = settingsSaveManager;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
        float savedVolume = saveManager != null ? saveManager.LoadSettings().MasterVolume : 1f;

        slider.SetValueWithoutNotify(savedVolume);
        ApplyVolume(savedVolume);
        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        ApplyVolume(volume);

        if (saveManager == null)
            return;

        GameSettingsData settings = saveManager.LoadSettings();
        settings.MasterVolume = volume;
        saveManager.SaveSettings(settings);
    }

    private void ApplyVolume(float volume)
    {
        SoundManager manager = soundManager ?? SoundManager.Instance;
        if (manager != null)
            manager.SetMasterVolume(volume);
        else
            AudioListener.volume = volume;

        if (percentText != null)
            percentText.text = $"{Mathf.RoundToInt(volume * 100)}%";
    }
}
