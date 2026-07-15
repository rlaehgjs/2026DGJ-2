using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSlider : MonoBehaviour
{
    private const string MasterVolumeKey = "MasterVolume";

    [SerializeField] private TMP_Text percentText;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        float savedVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);

        slider.SetValueWithoutNotify(savedVolume);
        SetVolume(savedVolume);

        slider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        percentText.text = $"{Mathf.RoundToInt(volume * 100)}%";

        PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        PlayerPrefs.Save();
    }
}