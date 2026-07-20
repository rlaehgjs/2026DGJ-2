using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MouseSensitivitySlider : MonoBehaviour
{
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private float minimumSensitivity = 0f;
    [SerializeField] private float maximumSensitivity = 1000f;

    private Slider slider;

    public void Configure(PlayerLook look)
    {
        playerLook = look;
    }

    private void Awake()
    {
        slider = GetComponent<Slider>();

        if (playerLook == null)
        {
            Debug.LogWarning("MouseSensitivitySlider: Player Look을 연결해야 합니다.", this);
            enabled = false;
            return;
        }

        ApplySensitivity(slider.value);
        slider.onValueChanged.AddListener(ApplySensitivity);
    }

    private void OnDestroy()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(ApplySensitivity);
    }

    private void ApplySensitivity(float value)
    {
        playerLook.SetMouseSensitivity(
            Mathf.Lerp(minimumSensitivity, maximumSensitivity, value));
    }
}
