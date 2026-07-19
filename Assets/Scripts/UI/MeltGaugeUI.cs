using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeltGaugeUI : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private PlayerMeltSystem playerMeltSystem;
    [SerializeField] private Image meltFillImage;
    [SerializeField] private TMP_Text percentText;

    [Header("Game Over")]
    [SerializeField] private GameManager gameManager;

    private void OnEnable()
    {
        if (playerMeltSystem == null)
            return;

        playerMeltSystem.OnHpChanged += UpdateGauge;
        playerMeltSystem.OnHpDepleted += HandleHpDepleted;
    }

    private void OnDisable()
    {
        if (playerMeltSystem == null)
            return;

        playerMeltSystem.OnHpChanged -= UpdateGauge;
        playerMeltSystem.OnHpDepleted -= HandleHpDepleted;
    }

    private void UpdateGauge(float currentHp, float maxHp)
    {
        float ratio = maxHp <= 0f ? 0f : currentHp / maxHp;
        meltFillImage.fillAmount = ratio;
        percentText.text = $"{ratio * 100f:F1}%";
    }

    private void HandleHpDepleted()
    {
        gameManager?.TriggerGameOver();
    }
}
