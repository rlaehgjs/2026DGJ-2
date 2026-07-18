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
    private float baseMeltValue = 1.0f;
    private float scaledMeltValue; // 배율이 적용된 해동 속도
    private int previous_state;

    public event Action<float, float> OnHpChanged;
    public event Action OnHpDepleted;
    public event Action<FreezeState> OnFreezeStateChanged;

    void Start()
    {
        currentHp = maxHp;
        scaledMeltValue = baseMeltValue;
    }

    void Update()
    {
        float damage = scaledMeltValue * Time.deltaTime;
        Damage(damage);
        Debug.Log("현재 hp/최대 hp: " + currentHp + "/" + maxHp);
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

    public void Heal(float amount)
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
                if (previous_state != i)
                {
                    OnFreezeStateChanged?.Invoke((FreezeState)i);
                    previous_state = i;
                }
                Debug.Log("현재 상태: " + (FreezeState)i);
                break;
            }
        }
    }

    public void SetMeltValueMultiplier(float multiplier)
    {
        scaledMeltValue = baseMeltValue * multiplier;
    }
}
