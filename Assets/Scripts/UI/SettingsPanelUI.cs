using UnityEngine;

public class SettingsPanelUI : MonoBehaviour
{
    public void SetKorean()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetKorean();
    }

    public void SetEnglish()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.SetEnglish();
    }
}
