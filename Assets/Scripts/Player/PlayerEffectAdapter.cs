using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectAdapter : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerMeltSystem playerMeltSystem;

    private readonly Dictionary<Object, EffectMultipliers> activeEffects = new();

    public float CurrentMeltRateMultiplier { get; private set; } = 1f;
    public float CurrentMoveSpeedMultiplier { get; private set; } = 1f;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (playerMeltSystem == null)
        {
            playerMeltSystem = GetComponent<PlayerMeltSystem>();
        }

        ApplyCurrentMultipliers();
    }

    public void AddEffect(Object effectSource, float meltRateMultiplier, float moveSpeedMultiplier)
    {
        if (effectSource == null)
        {
            return;
        }

        activeEffects[effectSource] = new EffectMultipliers(meltRateMultiplier, moveSpeedMultiplier);
        RecalculateMultipliers();
    }

    public void RemoveEffect(Object effectSource)
    {
        if (effectSource == null || !activeEffects.Remove(effectSource))
        {
            return;
        }

        RecalculateMultipliers();
    }

    private void RecalculateMultipliers()
    {
        CurrentMeltRateMultiplier = 1f;
        CurrentMoveSpeedMultiplier = 1f;

        foreach (EffectMultipliers effect in activeEffects.Values)
        {
            CurrentMeltRateMultiplier *= effect.MeltRateMultiplier;
            CurrentMoveSpeedMultiplier *= effect.MoveSpeedMultiplier;
        }

        ApplyCurrentMultipliers();
    }

    private void ApplyCurrentMultipliers()
    {
        if (playerMovement != null)
        {
            playerMovement.SetEnvironmentSpeedMultiplier(CurrentMoveSpeedMultiplier);
        }

        if (playerMeltSystem != null)
        {
            playerMeltSystem.SetMeltValueMultiplier(CurrentMeltRateMultiplier);
        }
    }

    private readonly struct EffectMultipliers
    {
        public readonly float MeltRateMultiplier;
        public readonly float MoveSpeedMultiplier;

        public EffectMultipliers(float meltRateMultiplier, float moveSpeedMultiplier)
        {
            MeltRateMultiplier = meltRateMultiplier;
            MoveSpeedMultiplier = moveSpeedMultiplier;
        }
    }
}
