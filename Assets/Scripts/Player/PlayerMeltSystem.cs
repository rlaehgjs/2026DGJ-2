using UnityEngine;
using System;
using System.Collections.Generic;


public class PlayerMeltSystem : MonoBehaviour
{
    [SerializeField]
    private List<float> freezeStateRatioThresholds = new List<float>()
    {0.3f, 0.6f, 1.0f};
    [SerializeField]
    private float maxHp = 100f;
    [SerializeField]
    private float currentHp;
    const float PercentToDecimal = 0.01f;
    [SerializeField]
    private float minLimitPercentHpMelted = 0.1f;
    [SerializeField]
    private float minLimitHpMelted;
    private float baseMeltValue = 1.0f;
    private float scaledMeltValue; // 배율이 적용된 해동 속도

    [SerializeField]
    private FreezeState current_state = FreezeState.Frozen;
    private FreezeState previous_state;

    public event Action<float, float> OnHpChanged;
    public event Action OnHpDepleted;
    public event Action<FreezeState> OnFreezeStateChanged;

    void Start()
    {
        currentHp = maxHp;
        scaledMeltValue = baseMeltValue;
        minLimitHpMelted = maxHp * minLimitPercentHpMelted * PercentToDecimal;
    }

    void Update()
    {
        if (currentHp > minLimitHpMelted)
        {
            Melt();
        }
    }

    private void Melt()
    {
        float hpMelted = scaledMeltValue * Time.deltaTime;
        currentHp = Mathf.Clamp(currentHp - hpMelted, minLimitHpMelted, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);
        // Debug.Log("녹음! 현재 hp/최대 hp: " + currentHp + "/" + maxHp);

        CheckFreezeState();
    }

    // 일시적 피해, 회복 메서드
    public void Damage(float damage)
    {
        currentHp = Mathf.Clamp(currentHp - damage, 0, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);
        if (currentHp <= 0f)
        {
            OnHpDepleted?.Invoke();
        }

        CheckFreezeState();
    }

    private void Heal(float amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);
        
        CheckFreezeState();
    }

    public bool TryHeal(float amount)
    {
        if (amount <= 0f || currentHp >= maxHp)
        {
            return false;
        }

        Heal(amount);
        return true;
    }

    // 냉동 상태 변화 관리
    private void CheckFreezeState()
    {
        for (int i = 0; i < freezeStateRatioThresholds.Count; ++i)
        {
            if (currentHp <= maxHp * freezeStateRatioThresholds[i])
            {
                FreezeState state = (FreezeState)i;
                if (previous_state != state)
                {
                    current_state = state;
                    OnFreezeStateChanged?.Invoke(current_state);
                    previous_state = current_state;
                }
                Debug.Log("현재 상태: " + current_state);
                break;
            }
        }
    }

    public void SetMeltValueMultiplier(float multiplier)
    {
        scaledMeltValue = baseMeltValue * multiplier;
    }
}
