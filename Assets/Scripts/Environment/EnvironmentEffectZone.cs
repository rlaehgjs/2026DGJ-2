using System.Collections.Generic;
using UnityEngine;

public class EnvironmentEffectZone : MonoBehaviour
{
    [Min(0f)][SerializeField] private float meltRateMultiplier = 1f;
    [Min(0f)][SerializeField] private float moveSpeedMultiplier = 1f;

    private readonly HashSet<Collider> playerColliders = new();
    private PlayerEffectAdapter activeAdapter;

    private void OnTriggerEnter(Collider other)
    {
        PlayerEffectAdapter adapter = other.GetComponentInParent<PlayerEffectAdapter>();

        if (adapter == null || !playerColliders.Add(other))
        {
            return;
        }

        activeAdapter = adapter;
        activeAdapter.AddEffect(this, meltRateMultiplier, moveSpeedMultiplier);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!playerColliders.Remove(other) || playerColliders.Count > 0)
        {
            return;
        }

        RemoveActiveEffect();
    }

    private void OnDisable()
    {
        RemoveActiveEffect();
        playerColliders.Clear();
    }

    private void RemoveActiveEffect()
    {
        if (activeAdapter == null)
        {
            return;
        }

        activeAdapter.RemoveEffect(this);
        activeAdapter = null;
    }
}
